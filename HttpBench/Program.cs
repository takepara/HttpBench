using System;
using System.Collections.Generic;
using System.Dynamic;
using System.Linq;
using System.Net;
using System.Text;

namespace HttpBench
{
    internal class Program
    {
        private static void Main(string[] args)
        {
            // 勝手証明書を許容する
            ServicePointManager.ServerCertificateValidationCallback = (sender, certificate, chain, sslerrors) => true;

            var setting = HttpSettingsParser.Parse(args);
            if (setting == null || !setting.IsValid)
            {
                PrintHelp();
                return;
            }

            var performance = new HttpPerformance();
            var results = performance.Execute(setting);
            PrintResults(setting, results);

#if DEBUG
            Console.ReadLine();
#endif
        }

        static void PrintHelp()
        {
            Console.WriteLine("Usage: hb [options] [http://]hostname[:port]/path");
            Console.WriteLine("Options are:");
            Console.WriteLine(HttpSettings.GetHelp());
        }

        static void PrintResults(HttpSettings setting, IEnumerable<HttpResult> results)
        {
            if (setting == null)
                throw new ArgumentNullException("setting");

            if (results == null)
                throw new ArgumentNullException("results");

            var totalElapsed = results.Sum(r => r.ElapsedMilliseconds);
            var totalTransferred = results.Sum(r => r.TransferLength);
            var totalTime = (results.Max(r => r.End) - results.Min(r => r.Start)).TotalMilliseconds;
            Console.WriteLine("Concurrent level:\t{0:N0}", setting.Concurrent);
            Console.WriteLine("Time taken for tests:\t{0:F3} seconds", totalTime / 1000);

            Console.WriteLine("Complete requests:\t{0:N0}", results.Count(r => r.Status == 200));
            Console.WriteLine("Failed requests:\t{0:N0}", results.Count(r => r.Status != 200));
            Console.WriteLine("Total transferred:\t{0:N0} bytes", results.Sum(r => r.TransferLength));
            var requestPerSec = 1000.0 / ((double)totalTime / results.Count());
            var timePerSec = (double)totalTime / results.Count();
            Console.WriteLine("Request per second:\t{0:F3} (#/sec)", requestPerSec);
            Console.WriteLine("Time per request:\t{0:F3} [ms]", timePerSec);
            Console.WriteLine("Transfer rate:  \t{0:F2} [Kbytes/sec] received", ((double)totalTransferred / 1024) / ((double)totalTime / 1000.0));

            Console.WriteLine("");
            var elapseds = (from r in results
                            group r by r.ElapsedMilliseconds
                            into timeGroup
                            select new
                                       {
                                           ElapsedMilliseconds = timeGroup.Key,
                                           Percent = (double) timeGroup.Count()/setting.Times
                                       } as dynamic
                           ).OrderBy(e => e.ElapsedMilliseconds);

            Console.WriteLine(" fastest:\t{0} ms", elapseds.Min(e => (long)e.ElapsedMilliseconds));
            Console.WriteLine(" average:\t{0} ms", (long)elapseds.Average(e => (long)e.ElapsedMilliseconds));
            Console.WriteLine(" longest:\t{0} ms", elapseds.Max(e => (long)e.ElapsedMilliseconds));
            Console.WriteLine("");
            Console.WriteLine("Percentage of the requests (ms)");
            Console.WriteLine("");
            var percents = new List<dynamic>();
            percents.AddRange(elapseds);
            if (elapseds.Count() > 10)
            {
                var totalPercents = 0.0;
                var sectionPoint = 0.5;
                percents.Clear();
                foreach (var elapsed in elapseds)
                {
                    totalPercents += elapsed.Percent;
                    if (totalPercents >= sectionPoint || elapsed == elapseds.Last())
                    {
                        dynamic p = new ExpandoObject();
                        p.ElapsedMilliseconds = elapsed.ElapsedMilliseconds;
                        p.Percent = totalPercents;

                        percents.Add(p);
                        sectionPoint += 0.05;
                    }
                }
            }


            foreach (var elapsed in percents.OrderBy(e => e.ElapsedMilliseconds))
            {
                Console.WriteLine("  {0:0#.#0} %:\t{1} ms", elapsed.Percent * 100, elapsed.ElapsedMilliseconds);
            }
            
#if DEBUG
            var managedThreads = results.Select(r => r.ManagedThreadId)
                .Distinct()
                .OrderBy(t => t)
                .Select(t => t.ToString());
            var ids = string.Join(", ", managedThreads);
            //if (ids.Length > 80)
            //    ids = ids.Substring(0, 80) + "...";

            Console.WriteLine("");
            Console.WriteLine("Managed Threads:\t{0}\n {1}", managedThreads.Count(), ids);
#endif
        }
    }
}
