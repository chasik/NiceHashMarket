using System;
using System.IO;
using System.Threading;
using System.Threading.Tasks;
using System.Windows.Threading;
using Renci.SshNet;

namespace NiceHashMarket.Core.Helpers
{
    public static class SshCommandExtensions
    {
        public static async Task ExecuteAsync(this SshCommand sshCommand, IProgress<ScriptOutputLine> progress, CancellationToken cancellationToken)
        {
            var asyncResult = sshCommand.BeginExecute();
            var stdoutStreamReader = new StreamReader(sshCommand.OutputStream);
            var stderrStreamReader = new StreamReader(sshCommand.ExtendedOutputStream);

            while (!asyncResult.IsCompleted)
            {
                await CheckOutputAndReportProgress(sshCommand, stdoutStreamReader, stderrStreamReader, progress, cancellationToken);

                await Dispatcher.Yield(DispatcherPriority.ApplicationIdle);
            }

            sshCommand.EndExecute(asyncResult);

            await CheckOutputAndReportProgress(sshCommand, stdoutStreamReader, stderrStreamReader, progress, cancellationToken);
        }

        private static async Task CheckOutputAndReportProgress(SshCommand sshCommand, TextReader stdoutStreamReader,
            TextReader stderrStreamReader, IProgress<ScriptOutputLine> progress, CancellationToken cancellationToken)
        {
            if (cancellationToken.IsCancellationRequested)
            {
                sshCommand.CancelAsync();
            }
            cancellationToken.ThrowIfCancellationRequested();

            await CheckStdoutAndReportProgressAsync(stdoutStreamReader, progress);
            await CheckStderrAndReportProgressAsync(stderrStreamReader, progress);
        }

        private static async Task CheckStdoutAndReportProgressAsync(TextReader stdoutStreamReader,
            IProgress<ScriptOutputLine> stdoutProgress)
        {
            var stdoutLine = await stdoutStreamReader.ReadToEndAsync();

            if (!string.IsNullOrEmpty(stdoutLine))
            {
                stdoutProgress.Report(new ScriptOutputLine(line: stdoutLine, isErrorLine: false));
            }
        }

        private static async Task CheckStderrAndReportProgressAsync(TextReader stderrStreamReader, IProgress<ScriptOutputLine> stderrProgress)
        {
            var stderrLine = await stderrStreamReader.ReadToEndAsync();

            if (!string.IsNullOrEmpty(stderrLine))
            {
                stderrProgress.Report(new ScriptOutputLine(line: stderrLine,  isErrorLine: true));
            }
        }
    }

    public class ScriptOutputLine
    {
        public ScriptOutputLine(string line, bool isErrorLine)
        {
            Line = line;
            IsErrorLine = isErrorLine;
        }

        public string Line { get; }

        public bool IsErrorLine { get; }
    }
}
