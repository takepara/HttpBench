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
                        if (echoCount > 0 && _requestTimes % echoCount == 0)
                            Console.WriteLine("Completed {0} requests", setting.Times - _requestTimes);

                        if (setting.WaitMilliseconds > 0)
                            Thread.Sleep(setting.WaitMilliseconds);

                        var result = HttpGet(setting);
                        results.Add(result);
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

        private int LoadBytes(HttpWebResponse response)
        {
            if (response == null)
                throw new ArgumentNullException("response");

            var loadBytes = 0;
            using (var stream = response.GetResponseStream())
            {
                using (var reader = new BinaryReader(stream))
                {
                    int bytes;
                    while ((bytes = reader.ReadBytes(10240).Length) != 0)
                        loadBytes += bytes;
                }
            }

            return loadBytes;
        }

        private HttpResult HttpGet(HttpSettings setting)
        {
            if (setting == null)
                throw new ArgumentNullException("setting");

            var result = new HttpResult { ManagedThreadId = Thread.CurrentThread.ManagedThreadId };
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
                result.TransferLength = LoadBytes(response);
            }
            catch (WebException we)
            {
                var response = we.Response as HttpWebResponse;
                if (response != null)
                {
                    result.Status = (int)response.StatusCode;
                    result.TransferLength = LoadBytes(response);
                }
            }

            sw.Stop();
            result.ElapsedMilliseconds = sw.ElapsedMilliseconds;

            return result;
        }
    }
}
