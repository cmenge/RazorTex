using System;
using System.Configuration;
using System.Diagnostics;
using System.IO;
using System.Text.RegularExpressions;
using System.Threading;

namespace Sample
{
    internal class PdfRenderer
    {
        private static readonly string _pdfLatexWorkingDirectory = ConfigurationManager.AppSettings["PdfLatexWorkingDirectory"];
        private static readonly string _pdfLatexBinPath = ConfigurationManager.AppSettings["PdfLatexBinPath"];
        private static readonly string _pdfLatexDestinationPath = ConfigurationManager.AppSettings["PdfLatexDestinationPath"];
        private static bool _runsInIIS = Boolean.Parse(ConfigurationManager.AppSettings["PdfLatexInIIS"]);

        /// <summary>
        /// Gets the number of pages written to the PDF. This value is set automatically when calling RenderPdf.
        /// If no output was written or an error occurred, this will be 0.
        /// </summary>
        public int PageCount { get; private set; }

        public bool HasErrors { get; private set; }

        private bool hasReadFinished = false;
        private object syncRoot = new object();

        public PdfRenderResult RenderPdf(Guid temporaryId, string latexString)
        {
            PageCount = 0;
            HasErrors = false;

            string errorMessage = string.Empty;

            Process workerProcess = null;
            try
            {
                string resultFileName = Path.Combine(_pdfLatexDestinationPath, temporaryId.ToString("N") + ".pdf");
                string latexFileName = Path.Combine(_pdfLatexWorkingDirectory, temporaryId.ToString("N") + ".tex");
                // -enable-pipes -enable-write18
                //-interaction=batchmode 
                string commandLine = String.Format(" -output-directory \"{0}\" \"{1}\"", _pdfLatexDestinationPath, latexFileName);//-quiet 
                commandLine = commandLine.Replace('\\', '/');

                //_log.Debug(() => String.Format("pdflatex, running: {0} {1} in {2}", fileName, commandLine, _sTempPath));

                ProcessStartInfo processStartInfo = new ProcessStartInfo();
                processStartInfo.WorkingDirectory = _pdfLatexWorkingDirectory;
                processStartInfo.CreateNoWindow = true;
                processStartInfo.FileName = _pdfLatexBinPath;
                processStartInfo.Arguments = commandLine;

                // Redirect streams is helpful to monitor output and generate error messages. While this works fine in Cassini,
                // it does not work in IIS and leads to annoying errors (namely hanging process that do not except, log or respond).
                // UseShellExecute and RedirectStandardOutput are always linked together.
                bool redirectIoStreams = true;

                workerProcess = new Process();
                if (redirectIoStreams)
                {
                    workerProcess.OutputDataReceived += new DataReceivedEventHandler(pdfLaTeXProcess_OutputDataReceived);
                    processStartInfo.RedirectStandardOutput = true;
                    //processStartInfo.RedirectStandardError = true;
                    processStartInfo.UseShellExecute = false;
                }
                else
                {
                    // http://social.msdn.microsoft.com/Forums/en-SG/csharpgeneral/thread/5772b66b-57f7-44e0-98d2-b6d9c5c0869a
                    //processStartInfo.Verb = "runas";
                    processStartInfo.UseShellExecute = true;
                    processStartInfo.RedirectStandardOutput = false;
                    processStartInfo.RedirectStandardError = false;
                }

                workerProcess.StartInfo = processStartInfo;
                workerProcess.Start();

                if (redirectIoStreams)
                {
                    workerProcess.BeginOutputReadLine();
                    // workerProcess.BeginErrorReadLine();
                }

                using (StreamWriter sw = File.CreateText(latexFileName))
                {
                    if (sw != null)
                        sw.Write(latexString);
                }

                // FIXME
                while (hasReadFinished == false)
                    Thread.Sleep(5);

                workerProcess.WaitForExit();
                
                return new PdfRenderResult { OutputFileName = resultFileName, PageCount = PageCount, HasError = HasErrors };
            }
            catch (Exception ex)
            {
                throw;
            }
            finally
            {
                workerProcess.Dispose();
            }
        }

        void pdfLaTeXProcess_OutputDataReceived(object sender, DataReceivedEventArgs e)
        {
            // http://stackoverflow.com/questions/63303/c-how-do-i-know-when-the-last-outputdatareceived-has-arrived
            if (e.Data == null)
                hasReadFinished = true;

            if (!string.IsNullOrWhiteSpace(e.Data))
            {
                if (e.Data.StartsWith("!"))
                {
                    // _log.Error(() => e.Data);
                    HasErrors = true;
                }
                else if (e.Data.StartsWith("Output written on"))
                {
                    Regex r = new Regex(@"(\d) page");
                    var match = r.Match(e.Data);
                    if (!string.IsNullOrWhiteSpace(match.Groups[1].Value))
                    {
                        int pageCount;
                        Int32.TryParse(match.Groups[1].Value, out pageCount);
                        PageCount = pageCount;
                    }
                }
            }
        }
    }
}
