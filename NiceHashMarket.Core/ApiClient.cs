using System;
using System.Collections.Generic;
using System.Linq;
using System.Text.RegularExpressions;
using Newtonsoft.Json.Linq;
using NiceHashMarket.Model;
using NiceHashMarket.Model.Enums;
using NiceHashMarket.Model.Interfaces;
using RestSharp;

namespace NiceHashMarket.Core
{
    public class ApiClient
    {
        private readonly RestClient _client;

        public ApiClient()
        {
            //_client = new RestClient("https://api.nicehash.com");
            _client = new RestClient("https://www.nicehash.com");
        }

        private IRestResponse RequestPubJson(string queryShortString)
        {
            var unixTimestamp = (int)DateTime.UtcNow.Subtract(new DateTime(1970, 1, 1)).TotalMilliseconds;

            var request = new RestRequest($"livePubJSON?{queryShortString}&callback=jQuery111307128799129489092_{unixTimestamp}&_={unixTimestamp + 111}");
            return _client.Execute(request);
        }

        private List<Order> GetOrderFromJtoken(JToken jtoken, ServerEnum server)
        {
            return jtoken.Select(order => new Order
                (
                    id: order[0].Value<int>(),
                    price: order[3].Value<decimal>(),
                    amount: order[4].Value<decimal>(),
                    speed: order[6].Value<decimal>(),
                    workers: order[5].Value<int>(),
                    type: order[1].Value<int>(),
                    active: order[2].Value<int>(),
                    server: server
                )).ToList();
        }

        public IEnumerable<Order> GetOrders(IAlgo algo)
        {
            var regex = new Regex(@"^.*\((?<json>(?:.*))\)");
            var match = regex.Match(RequestPubJson($"l=0&a={algo.Id}").Content);

            if (!match.Success || string.IsNullOrEmpty(match.Groups["json"].Value))
                return null;

            var jobject = JObject.Parse(match.Groups["json"].Value);

            var euServer = jobject["eu"];
            var euOrders = euServer["orders"];

            var result = GetOrderFromJtoken(euOrders, ServerEnum.Europe);

            var usServer = jobject["usa"];
            var usOrders = usServer["orders"];

            result.AddRange(GetOrderFromJtoken(usOrders, ServerEnum.Usa));

            return result;
        }

    }
}
