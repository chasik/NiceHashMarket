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
        private bool? _lastPingResult;
        private string _page = string.Empty;

        public int ID;
        public event Action<PingResult> PingResultChanged;

        public PingServer(StratumConnection stratumConnection, int interval = 1000)
        {
            _stratumConnection = stratumConnection;

            _timer = new Timer(_ => { ConnectToServer(); }, null, 0, interval);
        }

        private void ConnectToServer()
        {
            ID = 1;
            try
            {
                _tcpClient = new TcpClient(AddressFamily.InterNetwork);

                _tcpClient.BeginConnect(_stratumConnection.Host, _stratumConnection.Port, ConnectCallback, _tcpClient);
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
                Id = ID++,
                Method = "mining.subscribe",
                Parameters = new ArrayList()
            };


            var request = Helpers.JsonSerialize(command) + "\n";

            var bytesSent = Encoding.ASCII.GetBytes(request);

            try
            {
                _tcpClient.GetStream().Write(bytesSent, 0, bytesSent.Length);
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
                Id = ID++,
                Method = "mining.authorize",
                Parameters = new ArrayList {_stratumConnection.UserName, _stratumConnection.Password}
            };

            var request = Helpers.JsonSerialize(command) + "\n";

            var bytesSent = Encoding.ASCII.GetBytes(request);

            try
            {
                _tcpClient.GetStream().Write(bytesSent, 0, bytesSent.Length);
            }
            catch (Exception ex)
            {
                Console.WriteLine("Socket error:" + ex.Message);
                ConnectToServer();
            }
        }

        private void ConnectCallback(IAsyncResult result)
        {
            if (_lastPingResult != _tcpClient.Connected)
                OnPingResultChanged(new PingResult(_tcpClient.Connected, _stratumConnection.ToString()));

            _lastPingResult = _tcpClient.Connected;

            // We are connected successfully
            try
            {
                var networkStream = _tcpClient.GetStream();
                var buffer = new byte[_tcpClient.ReceiveBufferSize];

                // Now we are connected start async read operation.
                networkStream.BeginRead(buffer, 0, buffer.Length, ReadCallback, buffer);

                SendSubscribe();
                SendAuthorize();
            }
            catch (Exception ex)
            {
                Console.WriteLine($"Socket error {_stratumConnection.Algo}:" + ex.Message);
            }
        }

        // Callback for Read operation
        private void ReadCallback(IAsyncResult result)
        {
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
                Console.WriteLine(DateTime.Now + " Disconnected. Reconnecting...");

                _tcpClient.Close();
                _tcpClient = null;

                ConnectToServer();
                return;
            }

            // Get the data
            var data = Encoding.ASCII.GetString(buffer, 0, bytesread);
            Debug.WriteLine(data);

            _page = _page + data;

            var foundClose = _page.IndexOf('}');

            while (foundClose > 0)
            {
                var currentString = _page.Substring(0, foundClose + 1);

                // We can get either a command or response from the server. Try to deserialise both
                var command = Helpers.JsonDeserialize<StratumCommand>(currentString);
                var response = Helpers.JsonDeserialize<StratumResponse>(currentString);

                var e = new StratumEventArgs();

                if (command.Method != null)             // We got a command
                {
                    Console.WriteLine(DateTime.Now + " Got Command: " + currentString);
                    Debug.WriteLine(DateTime.Now + " Got Command: " + currentString);
                    e.MiningEventArg = command;

                    switch (command.Method)
                    {
                        case "mining.notify":
                            //if (GotNotify != null)
                            //    GotNotify(this, e);
                            break;
                        case "mining.set_difficulty":
                            //if (GotSetDifficulty != null)
                            //    GotSetDifficulty(this, e);
                            break;
                    }
                }
                else if (response.Error != null || response.Result != null)       // We got a response
                {
                    Console.WriteLine(DateTime.Now + " Got Response: " + currentString);
                    Debug.WriteLine(DateTime.Now + " Got Response: " + currentString);
                    e.MiningEventArg = response;

                    // Find the command that this is the response to and remove it from the list of commands that we're waiting on a response to
                    //var Cmd = (string)PendingACKs[response.Id];

                    //if (Cmd == null)
                    //    Console.WriteLine("Unexpected Response");
                    //else if (GotResponse != null)
                    //    GotResponse(Cmd, e);
                }

                _page = _page.Remove(0, foundClose + 2);
                foundClose = _page.IndexOf('}');
            }

            // Then start reading from the network again.
            networkStream.BeginRead(buffer, 0, buffer.Length, ReadCallback, buffer);
        }

        protected virtual void OnPingResultChanged(PingResult obj)
        {
            PingResultChanged?.Invoke(obj);
        }
    }
}
