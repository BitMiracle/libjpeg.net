using System;
using BenchmarkDotNet.Configs;
using BenchmarkDotNet.Diagnosers;
using BenchmarkDotNet.Diagnostics.Windows;
using BenchmarkDotNet.Environments;
using BenchmarkDotNet.Jobs;

namespace LibJpeg.Net.Benchmarks
{
    class NuGetConfigSourceAttribute : Attribute, IConfigSource
    {
        public NuGetConfigSourceAttribute(bool profileMemory = false)
        {
            // NOTE: The version of NuGet package referenced in this project should be <= 
            // than every version in this array.
            const string BaselineVersion = "1.4.309";
            string[] docoticVersions =
            {
                BaselineVersion,
                "1.5.313",
            };
            Runtime[] runtimes =
            {
                ClrRuntime.Net472,
                CoreRuntime.Core31
            };

            ManualConfig config = ManualConfig.CreateEmpty();
            foreach (Runtime runtime in runtimes)
            {
                foreach (string version in docoticVersions)
                {
                    bool baseline = (version == BaselineVersion && runtime == ClrRuntime.Net472);
                    config.AddJob(createJob("BitMiracle.LibJpeg.NET", version, runtime, baseline));
                }
            }

            if (profileMemory)
            {
                config.AddDiagnoser(MemoryDiagnoser.Default);
                config.AddDiagnoser(new NativeMemoryProfiler());
            }

            Config = config;
        }

        public IConfig Config { get; }

        private static Job createJob(string packageName, string packageVersion, Runtime runtime, bool baseline)
        {
            var baseJob = Job.MediumRun;
            return baseJob
                .WithNuGet(packageName, packageVersion)
                .WithId(packageVersion)
                .WithRuntime(runtime)
                .WithBaseline(baseline);
        }
    }
}
