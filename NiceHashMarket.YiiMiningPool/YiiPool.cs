using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
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

        private List<YiiCommand> _executedCommands;

        public event EventHandler<JEnumerable<JToken>> StatusReceived;

        public DateTime LastQuery { get; set; }

        public Dictionary<string, IYiiAlgo> PoolsAlgos { get; set; }

        public virtual string ApiUrl { get; set; }

        public Dictionary<string, double> PrecisionAlgosCorrections { get; set; }

        public YiiPoolEnum PoolType { get; set; }

        public YiiPool()
        {
            _executedCommands = new List<YiiCommand>();
            _timer = new Timer(ApiTimerHandler, null, 0, 0);

            if (PrecisionAlgosCorrections == null)
                PrecisionAlgosCorrections = new Dictionary<string, double>();

            PoolsAlgos = new Dictionary<string, IYiiAlgo>();
        }


        public void LoopCommand(int period, YiiCommandEnum command, IDictionary<string, string> commandParams = null)
        {
            _timer.Change(0, period);
            AddOrUpdateExecutedCommands(command, commandParams);
        }

        private void AddOrUpdateExecutedCommands(YiiCommandEnum command, IDictionary<string, string> commandParams = null)
        {
            if (_executedCommands.Any(c => c.Command == command))
            {
                //TODO update dictionary
            }
            else
            {
                _executedCommands.Add(new YiiCommand {Command = command, CommandParams = commandParams, Action = null });
            }
        }

        private void ApiTimerHandler(object state)
        {
            foreach (var command in _executedCommands)
            {
                Task.Factory.StartNew(() => ApiRequest(command.Command, command.CommandParams))
                    .ContinueWith(t =>
                    {
                        var content = t.Result.Content;

                        try
                        {
                            if (content.Length < 10) return;

                            LastQuery = DateTime.Now;

                            ParseYiiStatusCommand(GetJTokensFromResponse(content), JtokenParser);
                        }
                        catch (Exception e)
                        {
                            
                        }
                    });
            }
        }

        public virtual YiiAlgo JtokenParser(JToken jtoken)
        {
            return jtoken.First.ToObject<YiiAlgo>();
        }

        public virtual JEnumerable<JToken> GetJTokensFromResponse(string content)
        {
            return JObject.Parse(content).Children();
        }

        private void ParseYiiStatusCommand(JEnumerable<JToken> algos, Func<JToken, YiiAlgo> parser)
        {
            foreach (var algo in algos)
            {
                var yiiAlgo = parser.Invoke(algo);

                var correction = PrecisionAlgosCorrections.SingleOrDefault(c =>
                    string.Equals(c.Key, yiiAlgo.Name, StringComparison.InvariantCultureIgnoreCase));

                if (!correction.Equals(default(KeyValuePair<string, double>)))
                {
                    yiiAlgo.EstimateCurrent *= correction.Value;
                    yiiAlgo.EstimateLast24H *= correction.Value;
                    yiiAlgo.ActualLast24H *= correction.Value;
                }

                if (!PoolsAlgos.ContainsKey(yiiAlgo.Name))
                {
                    PoolsAlgos.Add(yiiAlgo.Name, yiiAlgo);
                }

                PoolsAlgos[yiiAlgo.Name] = yiiAlgo;
            }

            OnStatusReceived(algos);
        }

        private IRestResponse ApiRequest(YiiCommandEnum commandKey, IDictionary<string, string> commandParams)
        {
            var _client = new RestClient(ApiUrl);
            var resource = new StringBuilder($"{commandKey}");
            if (commandParams != null && commandParams.Any())
            {
                resource.Append("?");
                foreach (var parameter in commandParams)
                {
                    resource.Append($"{parameter.Key}={parameter.Value}&");
                }
            }

            var request = new RestRequest(resource.ToString().TrimEnd('&').ToLowerInvariant());

            return _client.Execute(request);
        }

        protected virtual void OnStatusReceived(JEnumerable<JToken> e)
        {
            StatusReceived?.Invoke(this, e);
        }

        public override string ToString()
        {
            return PoolType.ToString();
        }
    }
}
