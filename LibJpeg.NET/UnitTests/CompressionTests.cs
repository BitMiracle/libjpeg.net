using System;
using System.Collections.Generic;
using System.Text;
using System.IO;

using NUnit.Framework;
using cJpeg;

namespace UnitTests
{
    [TestFixture]
    public class CompressionTests
    {
        private string m_dataFolder = @"..\..\..\..\TestCase\jpeg_compression_data\";

        private string getSourcePath(string imageName)
        {
            return Path.Combine(m_dataFolder, imageName);
        }

        private void runTest(string[] args, string sourceImage, string targetImage)
        {
            // Jpeg.Program.Main is static, so lock concurent access to a test code
            // use a private field to lock upon 

            lock (m_dataFolder)
            {
                List<string> completeArgs = new List<string>(1 + args.Length + 2);

                completeArgs.Add("-c");
                for (int i = 0; i < args.Length; ++i)
                    completeArgs.Add(args[i]);

                completeArgs.Add(getSourcePath(sourceImage));
                completeArgs.Add(targetImage);

                Jpeg.Program.Main(completeArgs.ToArray());

                Assert.IsTrue(Utils.FilesAreEqual(getSourcePath(targetImage), targetImage));
            }
        }

        [Test]
        public void Test24()
        {
            runTest(new string[] { }, "test24.bmp", "test24.jpg");
        }

        [Test]
        public void Test24Quality()
        {
            runTest(new string[] { "-quality", "25" }, "test24.bmp", "test24_25.jpg");
        }

        [Test]
        public void Test24Optimize()
        {
            runTest(new string[] { "-optimize" }, "test24.bmp", "test24_opt.jpg");
        }

        [Test]
        public void Test24Progressive()
        {
            runTest(new string[] { "-progressive" }, "test24.bmp", "test24_prog.jpg");
        }

        [Test]
        public void Test24Grayscale()
        {
            runTest(new string[] { "-grayscale" }, "test24.bmp", "test24_gray.jpg");
        }

        [Test]
        public void Test8()
        {
            runTest(new string[] { }, "test8.bmp", "test8.jpg");
        }

        [Test]
        public void Test8Quality()
        {
            runTest(new string[] { "-quality", "25" }, "test8.bmp", "test8_25.jpg");
        }

        [Test]
        public void Test8Optimize()
        {
            runTest(new string[] { "-optimize" }, "test8.bmp", "test8_opt.jpg");
        }

        [Test]
        public void Test8Progressive()
        {
            runTest(new string[] { "-progressive" }, "test8.bmp", "test8_prog.jpg");
        }

        [Test]
        public void Test8Grayscale()
        {
            runTest(new string[] { "-grayscale" }, "test8.bmp", "test8_gray.jpg");
        }

        [Test]
        public void Testimg()
        {
            runTest(new string[] { }, "testimg.bmp", "testimg.jpg");
        }

        [Test]
        public void TestimgQuality()
        {
            runTest(new string[] { "-quality", "25" }, "testimg.bmp", "testimg_25.jpg");
        }

        [Test]
        public void TestimgOptimize()
        {
            runTest(new string[] { "-optimize" }, "testimg.bmp", "testimg_opt.jpg");
        }

        [Test]
        public void TestimgProgressive()
        {
            runTest(new string[] { "-progressive" }, "testimg.bmp", "testimg_prog.jpg");
        }

        [Test]
        public void TestimgGrayscale()
        {
            runTest(new string[] { "-grayscale" }, "testimg.bmp", "testimg_gray.jpg");
        }
    }
}
