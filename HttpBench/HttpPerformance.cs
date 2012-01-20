using System;
using System.Collections.Concurrent;
using System.Collections.Generic;
using System.Diagnostics;
using System.IO;
using System.Linq;
using System.Net;
using System.Threading;

namespace HttpBench
{
    public class HttpPerformance
    {
        private static CredentialCache _credentialCache;
        private string _userAgent = "HttpBench/1.0";

        public IEnumerable<HttpResult> Execute(HttpSettings setting)
        {
            if (setting == null)
                throw new ArgumentNullException("setting");

            if (!string.IsNullOrWhiteSpace(setting.BasicAuthentication))
            {
                var account = setting.BasicAuthentication.Split(':');
                var credential = new NetworkCredential(account[0], account[1]);
                _credentialCache = new CredentialCache { { setting.Url, "Basic", credential } };
            }

            var results = new ConcurrentBag<HttpResult>();
            ConcurrentExecute(setting, results);

            return results;
        }

        private static int _requestTimes;

        private void ConcurrentExecute(HttpSettings setting, ConcurrentBag<HttpResult> results)
        {
            _requestTimes = setting.Times;
            var echoCount = setting.Times / 10;

            Console.WriteLine("Benchmarking {0}", setting.Url);
            if(setting.Warmup > 0)
            {
                Console.WriteLine(" warmup {0} requests", setting.Warmup);
                for (int i = 0; i < setting.Warmup; i++)
                {
                    HttpGet(setting);
                }
            }
            Console.WriteLine("");

            var threads = new List<Thread>();
            var wait = new AutoResetEvent(false);
            for (var c = 0; c < setting.Concurrent; c++)
            {
                threads.Add(new Thread(() =>
                {
                    while (_requestTimes > 0)
                    {
                        Interlocked.Decrement(ref _requestTimes);

                        if (setting.WaitMilliseconds > 0)
                            Thread.Sleep(setting.WaitMilliseconds);

                        var result = HttpGet(setting);
                        results.Add(result);

                        var requestedCount = results.Count;
                        if (echoCount >= 2 && requestedCount % echoCount == 0)
                            Console.WriteLine("Completed {0} requests", requestedCount);
                    }

                    if (results.Count >= setting.Times && _requestTimes <= 0)
                    {
                        wait.Set();
                    }
                }));
            }

            threads.AsParallel().ForAll(t => t.Start());
            wait.WaitOne();

            Console.WriteLine("");
        }

        private int ResponseBytes(HttpWebResponse response)
        {
            if (response == null)
                throw new ArgumentNullException("response");

            var responseBytes = 0;
            int bytes;
            using (var stream = response.GetResponseStream())
            {
                using (var reader = new BinaryReader(stream))
                {
                    while ((bytes = reader.ReadBytes(10240).Length) != 0)
                        responseBytes += bytes;
                }
            }

            return responseBytes;
        }

        private HttpResult HttpGet(HttpSettings setting)
        {
            if (setting == null)
                throw new ArgumentNullException("setting");

            var result = new HttpResult { ManagedThreadId = Thread.CurrentThread.ManagedThreadId, Start = DateTime.Now };
            var sw = new Stopwatch();
            sw.Start();

            var request = (HttpWebRequest)WebRequest.Create(setting.Url);
            request.Method = "GET";
            request.PreAuthenticate = true;
            request.UserAgent = _userAgent;
            if (_credentialCache != null)
            {
                request.Credentials = _credentialCache;
            }

            try
            {
                var response = (HttpWebResponse)request.GetResponse();
                result.Status = (int)response.StatusCode;
                result.TransferLength = ResponseBytes(response);
            }
            catch (WebException we)
            {
                var response = we.Response as HttpWebResponse;
                if (response != null)
                {
                    result.Status = (int)response.StatusCode;
                    result.TransferLength = ResponseBytes(response);
                }
            }

            sw.Stop();
            result.ElapsedMilliseconds = sw.ElapsedMilliseconds;
            result.End = DateTime.Now;

            return result;
        }
    }
}
