using System;
using System.Collections.Generic;
using System.Threading;
using System.Threading.Tasks;
using Newtonsoft.Json.Linq;
using NiceHashMarket.YiiMiningPool.Interfaces;
using RestSharp;

namespace NiceHashMarket.YiiMiningPool
{
    public class YiiPool : IYiiPool
    {
        private Timer _timer;

        private Dictionary<YiiCommandEnum, Action<JObject>> _executedCommands;

        public event EventHandler<JObject> StatusReceived;
        public virtual string ApiUrl { get; set; }

        public YiiPool()
        {
            _executedCommands = new Dictionary<YiiCommandEnum, Action<JObject>>();
            _timer = new Timer(ApiTimerHandler, null, 0, 0);
        }


        public void LoopCommand(int period, YiiCommandEnum command, IDictionary<string, string> commandParams = null)
        {
            _timer.Change(0, period);
            AddOrUpdateExecutedCommands(command);
        }

        private void AddOrUpdateExecutedCommands(YiiCommandEnum command)
        {
            if (_executedCommands.ContainsKey(command))
            {
                //TODO update dictionary
            }
            else
            {
                _executedCommands.Add(command, null);
            }
        }

        private void ApiTimerHandler(object state)
        {
            foreach (var command in _executedCommands)
            {
                Task.Factory.StartNew(() => ApiRequest(command.Key))
                    .ContinueWith(t =>
                    {
                        var content = t.Result.Content;

                        try
                        {
                            if (content.Length > 10)
                                OnStatusReceived(JObject.Parse(content));
                        }
                        catch(Exception e)
                        {
                            
                        }
                    });
            }
        }

        private IRestResponse ApiRequest(YiiCommandEnum commandKey)
        {
            var _client = new RestClient(ApiUrl);

            var request = new RestRequest($"{commandKey}");

            return _client.Execute(request);
        }

        protected virtual void OnStatusReceived(JObject e)
        {
            StatusReceived?.Invoke(this, e);
        }
    }
}
