using BenchmarkDotNet.Attributes;

namespace LibJpeg.Net.Benchmarks.Decode
{
    [NuGetConfigSource]
    public class DecodeRgb : DecodeBase
    {
        protected override string InputFileName => "MARBLES.JPG";

        [GlobalSetup]
        public void Setup()
        {
            SetupBase();
        }

        [IterationSetup]
        public void IterationSetup()
        {
            IterationSetupBase();
        }

        [GlobalCleanup]
        public void Cleanup()
        {
            CleanupBase();
        }

        [Benchmark]
        public void DecodeRgbToStream()
        {
            decodeToStream();
        }
    }
}
