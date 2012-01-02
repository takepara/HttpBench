using System;
using System.Collections.Generic;
using System.Linq;

namespace HttpBench
{
    public class HttpSettingsParser
    {
        public static IEnumerable<Tuple<string, object>> GetNamedArgs(object[] args)
        {
            var argsList = args;
            if(args.Length % 2 != 0)
                argsList = args.Take(args.Length - 1).Concat(new object[] { "U", args.Last() }).ToArray();

            var leftArgs =
                argsList.Where((arg, idx) => idx % 2 == 0)
                    .Select(arg => arg.ToString().StartsWith("-") 
                        ? arg.ToString().Substring(1)
                        : arg.ToString());
            var rightArgs = argsList.Where((arg, idx) => idx % 2 != 0);

            return leftArgs.Zip(rightArgs, Tuple.Create);
        }

        public static HttpSettings Parse(params object[] args)
        {
            if (args.Length == 0)
                return null;

            dynamic instance = new HttpSettings();
            var namedArgs = GetNamedArgs(args);
            foreach (var namedArg in namedArgs)
            {
                instance[namedArg.Item1] = namedArg.Item2;
            }

            return instance;
        }
    }
}
