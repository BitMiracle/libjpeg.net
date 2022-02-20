using System;
using System.Collections.Generic;
using System.IO;
using System.Text;
using NUnit.Framework;

namespace UnitTests
{
    class Tester
    {
        private static readonly object locked = new object();
        private readonly Action<string[]> m_action;

        public static readonly string Testcase;

        static Tester()
        {
#if NETSTANDARD
            string currentDirectoryPath = Directory.GetCurrentDirectory();
#else
            string currentDirectoryPath = TestContext.CurrentContext.TestDirectory;
#endif
            StringBuilder pathToTestcase = new StringBuilder("TestCase\\");
            var dir = new DirectoryInfo(currentDirectoryPath);
            while (dir.Parent != null)
            {
                var dirs = dir.GetDirectories("TestCase", SearchOption.TopDirectoryOnly);
                var testcase = (dirs.Length > 0) ? dirs[0] : null;
                if (testcase != null)
                {
                    Testcase = Path.Combine(currentDirectoryPath, pathToTestcase.ToString());
                    return;
                }

                dir = dir.Parent;
                pathToTestcase.Insert(0, "..\\");
            }

            Assert.Fail("Unable to find TestCase directory");

        }

        private Tester(Action<string[]> action)
        {
            m_action = action;
        }

        public static Tester CreateForCompression(bool compression)
        {
            if (compression)
                return new Tester(BitMiracle.cJpeg.Program.Main);

            return new Tester(BitMiracle.dJpeg.Program.Main);
        }

        public static Tester CreateForTransformations()
        {
            return new Tester(BitMiracle.JpegTran.Program.Main);
        }

        public static string MapOpenPath(string path)
        {
            return Path.Combine(Testcase, path);
        }

        public static string MapOutputPath(string fileName)
        {
            return Path.Combine(Path.Combine(Testcase, "Output"), fileName);
        }

        public static string MapExpectedPath(string fileName)
        {
            return Path.Combine(Path.Combine(Testcase, "Expected"), fileName);
        }

        public static void PerformCompressionTest(string[] args, string file, string suffix)
        {
            PerformTest(args, file, suffix, true);
        }

        public static void PerformDecompressionTest(string[] args, string file, string suffix)
        {
            PerformTest(args, file, suffix, false);
        }

        public static void PerformTest(string[] args, string file, string suffix, bool compression)
        {
            Tester Tester = Tester.CreateForCompression(compression);
            string inputFile = Path.Combine(Testcase, Path.GetFileName(file));

            string ext;
            if (compression)
                ext = ".jpg";
            else
                ext = ".bmp";

            string outputFile = MapOutputPath(Path.GetFileNameWithoutExtension(file) + suffix + ext);
            Tester.Run(args, inputFile, outputFile);
        }

        public static void TestTransform(string prefix, string file, params string[] args)
        {
            Tester Tester = Tester.CreateForTransformations();
            string inputFile = Path.Combine(Testcase, Path.GetFileName(file));

            string outputName = $"{prefix}_{Path.GetFileNameWithoutExtension(file)}.jpg";
            string outputFile = MapOutputPath(outputName);
            Tester.Run(args, inputFile, outputFile);
        }

        public void Run(string[] args, string sourceImage, string targetImage)
        {
            // actions can be static, so lock concurent access to a test code
            // use a private field to lock upon 

            lock (locked)
            {
                List<string> completeArgs = new List<string>(1 + args.Length + 2);

                for (int i = 0; i < args.Length; ++i)
                    completeArgs.Add(args[i]);

                completeArgs.Add(sourceImage);
                completeArgs.Add(targetImage);

                if (!File.Exists(sourceImage))
                    throw new FileNotFoundException("Source image does not exist", sourceImage);

                m_action(completeArgs.ToArray());

                string sampleFile = targetImage.Replace(@"\Output\", @"\Expected\");

                if (!File.Exists(sampleFile))
                    throw new FileNotFoundException("Expected image does not exist", sampleFile);

                FileAssert.AreEqual(sampleFile, targetImage);

                File.Delete(targetImage);
            }
        }
    }
}
