using System;
using System.Diagnostics;
using System.Threading.Tasks;
using System.Linq;
using System.Threading;

namespace EfficientApiCalls
{
    public static class BenchmarkHelper
    {
        public static async Task BenchAsync(Func<object, CancellationToken, Task> operation, int numberOfExecutions, string tag, object content, CancellationToken cancellationToken)
        {
            long[] timings = new long[numberOfExecutions];

            var sw = new Stopwatch();
            for (int i = 0; i < numberOfExecutions; i++)
            {
                sw.Restart();

                await operation(content, cancellationToken);
                sw.Stop();
                timings[i] = sw.ElapsedMilliseconds;
                //Console.WriteLine($"{tag} : {sw.ElapsedMilliseconds} ms");
            }

            var average = timings.Average();

            Console.WriteLine($"{tag} : Average {average} ms");
        }

        public static async Task BenchAsync<T>(Func<CancellationToken, Task<T>> operation, int numberOfExecutions, string tag, CancellationToken cancellationToken)
        {
            long[] timings = new long[numberOfExecutions];

            var sw = new Stopwatch();
            for (int i = 0; i < numberOfExecutions; i++)
            {
                sw.Restart();

                var result = await operation(cancellationToken);
                sw.Stop();
                timings[i] = sw.ElapsedMilliseconds;
                //Console.WriteLine($"{tag} : {sw.ElapsedMilliseconds} ms");
            }

            var average = timings.Average();

            Console.WriteLine($"{tag} : Average {average} ms");
        }

        public static async Task BenchAsync<T>(Func<Task<T>> operation, int numberOfExecutions, string tag)
        {
            long[] timings = new long[numberOfExecutions];

            var sw = new Stopwatch();
            for (int i = 0; i < numberOfExecutions; i++)
            {
                sw.Restart();
                await operation();
                sw.Stop();
                timings[i] = sw.ElapsedMilliseconds;
                //Console.WriteLine($"{tag} : {sw.ElapsedMilliseconds} ms");
            }

            var average = timings.Average();

            Console.WriteLine($"{tag} : Average {average} ms");

        }
    }
}
