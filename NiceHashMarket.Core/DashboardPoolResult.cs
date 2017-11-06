using System;
using Newtonsoft.Json.Linq;
using NiceHashMarket.Core.Helpers;
using NiceHashMarket.Logger;
using NiceHashMarket.Model.Enums;
using RestSharp;

namespace NiceHashMarket.Core
{
    public class DashboardPoolResult
    {
        private string _host;
        private readonly RestClient _client;
        private MetricPrefixEnum _metricPrefix;


        public string Host
        {
            get => _host;
            set => _host = value;
        }

        public DateTime QueryDateTime { get; set; }

        public int Difficulty { get; set; }

        public int GlobalHashRate { get; set; }
        public int PoolHashRate { get; set; }

        public double RoundProgress { get; set; } = -1;

        public DashboardPoolResult()
        {
            
        }

        public DashboardPoolResult(string host, DateTime queryDateTime, MetricPrefixEnum metricPrefix, string apiKey, string apiId)
        {
            QueryDateTime = queryDateTime;

            _host = host;
            _client = new RestClient(_host);
            _metricPrefix = metricPrefix;

            var request = new RestRequest($"index.php?page=api&action=getdashboarddata&api_key={apiKey}&id={apiId}");

            ParseRestResponse(_client.Execute(request));
        }

        private void ParseRestResponse(IRestResponse response)
        {
            JObject jobject;

            try
            {
                jobject = JObject.Parse(response.Content);
            }
            catch (Exception ex)
            {
                MarketLogger.Error($"DashboardPoolResult {ex.Message} {response.Content}");
                return;
            }

            var data = jobject["getdashboarddata"]["data"];

            var networkData = data["network"]; // block, difficulty, esttimerperblock, hashrate, nextdifficulty

            var poolData = data["pool"]; // shares, workers, hashrate, esttimerperblock

            var poolSharesData = poolData["shares"]; // estimated, valid, invalid, progress

            Difficulty = (int) networkData["difficulty"].Value<double>();
            PoolHashRate = (int)_metricPrefix.MetricConvert(MetricPrefixEnum.Giga, poolData["hashrate"].Value<double>());
            GlobalHashRate = (int)_metricPrefix.MetricConvert(MetricPrefixEnum.Giga, networkData["hashrate"].Value<double>());
            RoundProgress = poolSharesData["progress"].Value<double>();
        }
    }
}
