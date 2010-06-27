using System;
using System.Collections.Generic;
using System.Text;
using System.IO;

using NUnit.Framework;

namespace UnitTests
{
    [TestFixture]
    public class CompressionTests
    {
        private static string[] Files
        {
            get
            {
                return new string[]
                {
                    "test24.bmp",
                    "test8.bmp",
                    "testimg.bmp"
                };
            }
        }

        [Test, TestCaseSource("Files")]
        public void TestCompression(string file)
        {
            Tester.PerformCompressionTest(new string[] { }, file, "");
        }

        [Test, TestCaseSource("Files")]
        public void TestQuality(string file)
        {
            Tester.PerformCompressionTest(new string[] { "-quality", "25" }, file, "_25");
        }

        [Test, TestCaseSource("Files")]
        public void TestOptimized(string file)
        {
            Tester.PerformCompressionTest(new string[] { "-optimize" }, file, "_opt");
        }

        [Test, TestCaseSource("Files")]
        public void TestProgressive(string file)
        {
            Tester.PerformCompressionTest(new string[] { "-progressive" }, file, "_prog");
        }

        [Test, TestCaseSource("Files")]
        public void TestGrayscale(string file)
        {
            Tester.PerformCompressionTest(new string[] { "-grayscale" }, file, "_gray");
        }
    }
}
