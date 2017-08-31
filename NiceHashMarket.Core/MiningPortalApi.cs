using System;
using System.Collections.Concurrent;
using System.Globalization;
using System.Linq;
using System.Threading;
using System.Threading.Tasks;
using HtmlAgilityPack;
using NiceHashMarket.Core.Interfaces;
using NiceHashMarket.Logger;
using NiceHashMarket.Model;
using NiceHashMarket.Model.Enums;

namespace NiceHashMarket.Core
{
    public class MiningPortalApi : IPoolApi
    {
        private readonly string _host;
        private Timer _timer;
        private readonly string _restRequest;
        private int _currentTimeZoneHoursOffset;
        private string _apiId;
        private string _apiKey;
        private MetricPrefixEnum _metricPrefix;

        public event EventHandler<BlockInfo> RowOfBlockParsed;
        public event EventHandler<BlockInfo> NewBlockFounded;
        public event EventHandler<BlockInfo> BlockUpdated;
        public event EventHandler<DashboardPoolResult> DifficultyChanged;
        public event EventHandler<DashboardPoolResult> GlobalHashRateChanged;
        public event EventHandler<DashboardPoolResult> PoolHashRateChanged;
        public event EventHandler<DashboardPoolResult> RoundProgressChanged;


        public DateTime LastQueryDateTime { get; set; }
        public int LastDifficulty { get; set; }

        public int LastGlobalHashRate { get; set; }
        public int LastPoolHashRate { get; set; }

        public double LastRoundProgress { get; set; }

        public ConcurrentQueue<BlockInfo> Blocks { get; set; }

        public MiningPortalApi(string host, int period, int currentTimeZoneHoursOffset, MetricPrefixEnum metricPrefix, string apiKey, string apiId)
        {
            _apiKey = apiKey;
            _apiId = apiId;

            _metricPrefix = metricPrefix;
            _host = host;
            _currentTimeZoneHoursOffset = currentTimeZoneHoursOffset;

            Blocks = new ConcurrentQueue<BlockInfo>();

            _restRequest = $"{_host}/index.php?page=statistics&action=blocks";

            _timer = new Timer(TimerHandler, null, 0, period);
        }

        private void TimerHandler(object state)
        {
            Task.Factory.StartNew(() => { GetStatisticOfBlocks(DateTime.Now); });

            if (_apiKey != null && _apiId != null)
            {
                Task.Factory.StartNew(() => GetDashBoard(DateTime.Now, _metricPrefix))
                    .ContinueWith(t =>
                    {
                        if (t.Result.QueryDateTime < LastQueryDateTime) return;

                        LastQueryDateTime = t.Result.QueryDateTime;

                        if (LastDifficulty != t.Result.Difficulty && t.Result.Difficulty != 1)
                        {
                            OnDifficultyChanged(t.Result);
                            LastDifficulty = t.Result.Difficulty;
                        }

                        if (LastPoolHashRate != t.Result.PoolHashRate)
                        {
                            OnPoolHashRateChanged(t.Result);
                            LastPoolHashRate = t.Result.PoolHashRate;
                        }

                        if (LastGlobalHashRate != t.Result.GlobalHashRate)
                        {
                            OnGlobalHashRateChanged(t.Result);
                            LastGlobalHashRate = t.Result.GlobalHashRate;
                        }

                        if (LastRoundProgress != t.Result.RoundProgress)
                        {
                            OnRoundProgressChanged(t.Result);
                            LastRoundProgress = t.Result.RoundProgress;
                        }
                    });
            }
        }

        private DashboardPoolResult GetDashBoard(DateTime queryDateTime, MetricPrefixEnum metricPrefix)
        {
            var dashBoardResponse = new DashboardPoolResult(_host, queryDateTime, metricPrefix, _apiKey, _apiId);

            return dashBoardResponse;
        }

        private void GetStatisticOfBlocks(DateTime queryTime)
        {
            var web = new HtmlWeb();

            HtmlDocument document = null;
            try
            {
                document = web.Load(_restRequest);
            }
            catch (Exception ex)
            {
                MarketLogger.Error(ex, $"{ex.Message}");
            }

            if (document == null)
                return;

            var rowsOfBlock = document.DocumentNode.Descendants("table")
                .Where(y => y.Attributes.Contains("class") 
                        && y.Attributes["class"].Value.Contains("table")
                        && y.Attributes["class"].Value.Contains("table-striped")
                        && y.Attributes["class"].Value.Contains("table-bordered")
                        && y.Attributes["class"].Value.Contains("table-hover")

                        && y.Descendants("tr").Count() > 10
                )
                .SelectMany(y => y.Descendants("tr"))
                .Where(y => !y.Descendants("td").Any(yy => yy.Attributes.Contains("colspan")))
                .ToList();

            rowsOfBlock.ForEach(r =>
            {
                var id = r.Descendants("td").FirstOrDefault()?.InnerText;

                if (id == null) return;

                var percent = r.Descendants("td").LastOrDefault()?.Descendants("font").Single().InnerText;
                var dateTime = r.Descendants("td").Skip(3).FirstOrDefault()?.InnerText;

                var block = new BlockInfo(id, percent, DateTime.ParseExact(dateTime, "MM/dd/yyyy HH:mm:ss", CultureInfo.InvariantCulture).AddHours(_currentTimeZoneHoursOffset));

                OnRowOfBlockParsed(block);

                var existBlock = Blocks.FirstOrDefault(b => b.Id == block.Id);

                if (existBlock == null)
                {
                    Blocks.Enqueue(block);

                    OnNewBlockFounded(block);
                }
                else if (Math.Abs(existBlock.Percent - block.Percent) > 0.001 && existBlock.Percent < block.Percent)
                {
                    existBlock.Percent = block.Percent;
                    OnBlockUpdated(existBlock);
                }
                
            });
        }

        protected virtual void OnRowOfBlockParsed(BlockInfo block)
        {
            RowOfBlockParsed?.Invoke(this, block);
        }

        protected virtual void OnNewBlockFounded(BlockInfo e)
        {
            NewBlockFounded?.Invoke(this, e);
        }

        protected virtual void OnBlockUpdated(BlockInfo e)
        {
            BlockUpdated?.Invoke(this, e);
        }

        protected virtual void OnDifficultyChanged(DashboardPoolResult dashboardResult)
        {
            DifficultyChanged?.Invoke(this, dashboardResult);
        }

        protected virtual void OnPoolHashRateChanged(DashboardPoolResult dashboardResult)
        {
            PoolHashRateChanged?.Invoke(this, dashboardResult);
        }

        protected virtual void OnGlobalHashRateChanged(DashboardPoolResult dashboardResult)
        {
            GlobalHashRateChanged?.Invoke(this, dashboardResult);
        }

        protected virtual void OnRoundProgressChanged(DashboardPoolResult dashboardResult)
        {
            RoundProgressChanged?.Invoke(this, dashboardResult);
        }
    }
}
