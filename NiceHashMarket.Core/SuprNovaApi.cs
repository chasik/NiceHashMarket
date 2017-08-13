using System;
using System.Collections.Concurrent;
using System.Globalization;
using System.Linq;
using System.Threading;
using System.Threading.Tasks;
using HtmlAgilityPack;
using NiceHashMarket.Model;

namespace NiceHashMarket.Core
{
    public class SuprNovaApi
    {
        private readonly string _host;
        private Timer _timer;
        private readonly string _restRequest;
        private int _currentTimeZoneHoursOffset;

        public event EventHandler<BlockInfo> RowOfBlockParsed;
        public event EventHandler<BlockInfo> NewBlockFounded;
        public event EventHandler<BlockInfo> BlockUpdated;

        public ConcurrentQueue<BlockInfo> Blocks { get; set; }


        public SuprNovaApi(string host, int period, int currentTimeZoneHoursOffset)
        {
            _currentTimeZoneHoursOffset = currentTimeZoneHoursOffset;

            Blocks = new ConcurrentQueue<BlockInfo>();

            _restRequest = $"{host}/index.php?page=statistics&action=blocks";

            _host = host;
            _timer = new Timer(TimerHandler, null, 0, period);
        }

        private void TimerHandler(object state)
        {
            Task.Factory.StartNew(() => { GetStatisticOfBlocks(DateTime.Now); });
        }

        public void GetStatisticOfBlocks(DateTime queryTime)
        {
            var web = new HtmlWeb();
            var document = web.Load(_restRequest);

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
    }
}
