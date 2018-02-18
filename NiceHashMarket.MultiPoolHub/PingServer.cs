using System;
using System.Collections;
using System.Diagnostics;
using System.Net.Sockets;
using System.Text;
using System.Threading;

namespace NiceHashMarket.MultiPoolHub
{
    public class PingServer
    {
        private readonly StratumConnection _stratumConnection;
        private TcpClient _tcpClient;
        private Timer _timer;
        private bool? _lastAuthResult;
        private string _page = string.Empty;

        public int CommandGlobalId;
        public Hashtable PendingAcks = new Hashtable();
        private IAsyncResult _oldReadAsyncResult;
        private IAsyncResult _oldConnectAsyncResult;


        public event Action<PingResult> PingResultChanged;

        public event EventHandler<StratumEventArgs> GotSetDifficulty;
        public event EventHandler<StratumEventArgs> GotNotify;
        public event EventHandler<StratumEventArgs> GotResponse;

        public PingServer(StratumConnection stratumConnection, int interval = 10000)
        {
            CommandGlobalId = 1;

            _stratumConnection = stratumConnection;

            //_timer = new Timer(_ => { ConnectToServer(); }, null, 0, interval);
            ConnectToServer();
        }

        private void ConnectToServer()
        {
           try
            {
                _tcpClient = new TcpClient(AddressFamily.InterNetwork);

                _oldConnectAsyncResult = _tcpClient.BeginConnect(_stratumConnection.Host, _stratumConnection.Port, ConnectCallback, _tcpClient);
            }
            catch (Exception ex)
            {
                Console.WriteLine("Socket error:" + ex.Message);
            }
        }

        private void SendSubscribe()
        {
            var command = new StratumCommand
            {
                Id = CommandGlobalId++,
                Method = "mining.subscribe",
                Parameters = new ArrayList()
            };


            var request = Helpers.JsonSerialize(command) + "\n";

            var bytesSent = Encoding.ASCII.GetBytes(request);

            try
            {
                _tcpClient.GetStream().Write(bytesSent, 0, bytesSent.Length);

                if (command.Id != null)
                    PendingAcks.Add(command.Id, command.Method);
            }
            catch (Exception ex)
            {
                Console.WriteLine("Socket error:" + ex.Message);
                ConnectToServer();
            }
        }

        private void SendAuthorize()
        {
            var command = new StratumCommand
            {
                Id = CommandGlobalId++,
                Method = "mining.authorize",
                Parameters = new ArrayList {_stratumConnection.UserName, _stratumConnection.Password}
            };

            var request = Helpers.JsonSerialize(command) + "\n";

            var bytesSent = Encoding.ASCII.GetBytes(request);

            try
            {
                _tcpClient.GetStream().Write(bytesSent, 0, bytesSent.Length);

                if (command.Id != null)
                    PendingAcks.Add(command.Id, command.Method);
            }
            catch (Exception ex)
            {
                Console.WriteLine("Socket error:" + ex.Message);
                ConnectToServer();
            }
        }

        private void ConnectCallback(IAsyncResult result)
        {
            if (_oldConnectAsyncResult != null && result != _oldConnectAsyncResult) return;

            // We are connected successfully
            try
            {
                SendSubscribe();
                SendAuthorize();

                var networkStream = _tcpClient.GetStream();
                var buffer = new byte[_tcpClient.ReceiveBufferSize];

                // Now we are connected start async read operation.
                _oldReadAsyncResult = networkStream.BeginRead(buffer, 0, buffer.Length, ReadCallback, buffer);
            }
            catch (Exception ex)
            {
                Console.WriteLine($"Socket error {_stratumConnection.Algo}:" + ex.Message);
            }
        }

        // Callback for Read operation
        private void ReadCallback(IAsyncResult result)
        {
            if (_oldReadAsyncResult != null && result != _oldReadAsyncResult) return;

            NetworkStream networkStream;
            int bytesread;

            var buffer = result.AsyncState as byte[];

            try
            {
                networkStream = _tcpClient.GetStream();
                bytesread = networkStream.EndRead(result);
            }
            catch (Exception ex)
            {
                Console.WriteLine("Socket error:" + ex.Message);
                return;
            }

            if (bytesread == 0)
            {
                //Debug.WriteLine(_stratumConnection.Algo + "    " + DateTime.Now + " Disconnected. Reconnecting...");

                _tcpClient.Close();
                _tcpClient = null;

                PendingAcks.Clear();

                Thread.Sleep(1000);

                ConnectToServer();

                return;
            }

            // Get the data
            var data = Encoding.ASCII.GetString(buffer, 0, bytesread);
            Debug.WriteLine($"|{_stratumConnection.Algo}| " + data);

            _page = _page + data;

            var foundClose = _page.IndexOf('}');

            while (foundClose > 0)
            {
                var currentString = _page.Substring(0, foundClose + 1);

                // We can get either a command or response from the server. Try to deserialise both
                var currentCommand = Helpers.JsonDeserialize<StratumCommand>(currentString);
                var currentResponse = Helpers.JsonDeserialize<StratumResponse>(currentString);

                var e = new StratumEventArgs{Algo = _stratumConnection.Algo};

                if (currentCommand.Method != null)             // We got a command
                {
                    e.MiningEventArg = currentCommand;

                    switch (currentCommand.Method)
                    {
                        case "mining.notify":
                            GotNotify?.Invoke(this, e);
                            break;
                        case "mining.set_difficulty":
                            GotSetDifficulty?.Invoke(this, e);
                            break;
                    }
                }
                else if (currentResponse.Error != null || currentResponse.Result != null)       // We got a response
                {
                    e.MiningEventArg = currentResponse;

                    // Find the command that this is the response to and remove it from the list of commands that we're waiting on a response to
                    var command = currentResponse.Id == null ? null : (string)PendingAcks[currentResponse.Id];

                    if (currentResponse.Id != null && PendingAcks.ContainsKey(currentResponse.Id))
                        PendingAcks.Remove(currentResponse.Id);

                    if (command == null)
                        Console.WriteLine("Unexpected Response");
                    else
                    {
                        GotResponse?.Invoke(command, e);

                        if (command.Contains("mining.authorize"))
                        {
                            var authSuccess = bool.Parse((e.MiningEventArg as StratumResponse)?.Result.ToString());

                            if (_lastAuthResult != authSuccess)
                                OnPingResultChanged(new PingResult(authSuccess, _stratumConnection.ToString()));

                            _lastAuthResult = authSuccess;
                        }
                    }
                }

                _page = _page.Remove(0, foundClose + 2);
                foundClose = _page.IndexOf('}');
            }

            // Then start reading from the network again.
            _oldReadAsyncResult = networkStream.BeginRead(buffer, 0, buffer.Length, ReadCallback, buffer);
        }

        protected virtual void OnPingResultChanged(PingResult obj)
        {
            PingResultChanged?.Invoke(obj);
        }
    }
}
