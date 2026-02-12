using BenchmarkDotNet.Running;

namespace Identity.BR.Benchmark
{
    internal class Program
    {
        static void Main(string[] args)
        {
            BenchmarkRunner.Run<CnpjBenchmark>();
            BenchmarkRunner.Run<CpfBenchmark>();
        }
    }
}
