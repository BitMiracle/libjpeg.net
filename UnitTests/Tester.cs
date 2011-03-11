using System;
using System.Collections.Generic;
using System.IO;
using System.Text;

using NUnit.Framework;

namespace UnitTests
{
    class Tester
    {
        private static object locked = new object();
        private bool m_compression;

        public static string Testcase
        {
            get
            {
                return @"..\..\..\TestCase\";
            }
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
            Tester tester = new Tester(compression);
            string inputFile = Path.Combine(Testcase, Path.GetFileName(file));

            string ext;
            if (compression)
                ext = ".jpg";
            else
                ext = ".bmp";

            string outputFile = Testcase + @"Output\" + Path.GetFileNameWithoutExtension(file) + suffix + ext;
            tester.Run(args, inputFile, outputFile);
        }

        public Tester(bool compression)
        {
            m_compression = compression;
        }

        public void Run(string[] args, string sourceImage, string targetImage)
        {
            // xJpeg.Program.Main is static, so lock concurent access to a test code
            // use a private field to lock upon 

            lock (locked)
            {
                List<string> completeArgs = new List<string>(1 + args.Length + 2);

                for (int i = 0; i < args.Length; ++i)
                    completeArgs.Add(args[i]);

                completeArgs.Add(sourceImage);
                completeArgs.Add(targetImage);

                if (m_compression)
                    BitMiracle.cJpeg.Program.Main(completeArgs.ToArray());
                else
                    BitMiracle.dJpeg.Program.Main(completeArgs.ToArray());

                string sampleFile = targetImage.Replace(@"\Output\", @"\Expected\");
                FileAssert.AreEqual(sampleFile, targetImage);
            }
        }
    }
}
