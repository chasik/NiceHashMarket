using System;
using System.IO;
using System.Net;
using Newtonsoft.Json;
using NiceHashMarket.Model.Enums;
// For generating HTTP requests and getting responses.
// For JSON parsing of remote APIs.

namespace NiceHashBotLib
{
    public class HandlerClass
    {
        public static WhattomineResult HandleOrder(CoinsWhatToMineEnum coin, int outterDifficulty)
        {
            // Retreive JSON data from API server. Replace URL with your own API request URL.
            var jsonData = GetHTTPResponseInJSON($"http://www.whattomine.com/coins/{(byte)coin}.json");
            if (jsonData == null) return new WhattomineResult();

            // Serialize returned JSON data.
            WhattomineResponse response;
            try
            {
                response = JsonConvert.DeserializeObject<WhattomineResponse>(jsonData);
            }
            catch
            {
                return new WhattomineResult();
            }

            return new WhattomineResult(response, outterDifficulty);
        }

        /// <summary>
        /// Data structure used for serializing JSON response from CoinWarz. 
        /// It allows us to parse JSON with one line of code and easily access every data contained in JSON message.
        /// </summary>
#pragma warning disable 0649
        public class WhattomineResponse
        {
            public string name;
            public string tag;
            public string algorithm;
            public string block_time;
            public double block_reward;
            public double block_reward24;
            public int last_block;
            public double difficulty;
            public double difficulty24;
            public string nethash;
            public double exchange_rate;
            public double exchange_rate24;
            public double exchange_rate_vol;
            public string exchange_rate_curr;
            public string market_cap;
            public string estimated_rewards;
            public string pool_fee;
            public string btc_revenue;
            public string revenue;
            public string cost;
            public string profit;
            public string status;
            public bool lagging;
            public int timestamp;
        }
#pragma warning restore 0649

        // Following methods do not need to be altered.
        #region PRIVATE_METHODS

        /// <summary>
        /// Get HTTP JSON response for provided URL.
        /// </summary>
        /// <param name="URL">URL.</param>
        /// <returns>JSON data returned by webserver or null if error occured.</returns>
        private static string GetHTTPResponseInJSON(string URL)
        {
            try
            {
                var WReq = (HttpWebRequest)WebRequest.Create(URL);
                WReq.Timeout = 60000;
                var WResp = WReq.GetResponse();
                var DataStream = WResp.GetResponseStream();
                DataStream.ReadTimeout = 60000;
                var SReader = new StreamReader(DataStream);
                var ResponseData = SReader.ReadToEnd();

                if (ResponseData[0] != '{')
                    throw new Exception("Not JSON data.");

                return ResponseData;
            }
            catch (Exception ex)
            {
                return null;
            }
        }

        #endregion
    }

    public class WhattomineResult
    {
        public double MaxPrice { get; set; }
        public double MaxPrice24 { get; set; }

        public double Difficulty { get; set; }
        public double Difficulty24 { get; set; }

        public double Price { get; set; }
        public string BlockTime { get; set; }

        public WhattomineResult()
        {
        }

        public WhattomineResult(HandlerClass.WhattomineResponse response, int outterDifficulty)
        {
            Difficulty = outterDifficulty == 0 ? response.difficulty : outterDifficulty;
            Difficulty24 = response.difficulty24;

            Price = response.exchange_rate;
            BlockTime = response.block_time;

            MaxPrice = Math.Floor(CalcPriceByDifficulty(Difficulty, response.exchange_rate, response.block_reward) * 10000) / 10000;
            MaxPrice24 = Math.Floor(CalcPriceByDifficulty(Difficulty24, response.exchange_rate24, response.block_reward24) * 10000) / 10000;
        }

        private static double CalcPriceByDifficulty(double difficulty, double exchangeRate, double blockReward)
        {
            // Calculate mining profitability in BTC per 1 TH of hashpower.
            var HT = difficulty * (Math.Pow(2.0, 32) / 1000000000000.0);
            var CPD = blockReward * 24.0 * 3600.0 / HT;

            return CPD * exchangeRate;
        }
    }
}