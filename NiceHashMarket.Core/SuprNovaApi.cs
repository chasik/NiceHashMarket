using System;
using System.Collections.Concurrent;
using System.Collections.Generic;
using System.Linq;
using System.Net;
using System.Runtime.CompilerServices;
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

        public event EventHandler<BlockInfo> OnRowOfBlockParsed;

        public ConcurrentDictionary<DateTime, List<BlockInfo>> Blocks { get; set; }


        public SuprNovaApi(string host, int period)
        {
            Blocks = new ConcurrentDictionary<DateTime, List<BlockInfo>>();

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

                var block = new BlockInfo(id, percent);

                OnOnRowOfBlockParsed(block);

            });


        }

        protected virtual void OnOnRowOfBlockParsed(BlockInfo block)
        {
            OnRowOfBlockParsed?.Invoke(this, block);
        }
    }
}
