using System;
using System.Collections.Generic;
using System.Text;
using System.IO;

using NUnit.Framework;
using dJpeg;

namespace UnitTests
{
    [TestFixture]
    public class DecompressionTests
    {
        private string m_dataFolder = @"..\..\..\..\TestCase\jpeg_decompression_data\";

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

                completeArgs.Add("-d");
                for (int i = 0; i < args.Length; ++i)
                    completeArgs.Add(args[i]);

                completeArgs.Add(getSourcePath(sourceImage));
                completeArgs.Add(targetImage);

                Jpeg.Program.Main(completeArgs.ToArray());

                Assert.IsTrue(Utils.FilesAreEqual(getSourcePath(targetImage), targetImage));
            }
        }

        [Test]
        public void Test3D()
        {
            runTest(new string[] { }, "3D.JPG", "3D.bmp");
        }

        [Test]
        public void TestBLU()
        {
            runTest(new string[] { }, "BLU.JPG", "BLU.bmp");
        }

        [Test]
        public void TestGLOBE1()
        {
            runTest(new string[] { }, "GLOBE1.JPG", "GLOBE1.bmp");
        }

        [Test]
        public void TestMARBLES()
        {
            runTest(new string[] { }, "MARBLES.JPG", "MARBLES.bmp");
        }

        [Test]
        public void TestPARROTS()
        {
            runTest(new string[] { }, "PARROTS.JPG", "PARROTS.bmp");
        }

        [Test]
        public void TestSPACE()
        {
            runTest(new string[] { }, "SPACE.JPG", "SPACE.bmp");
        }

        [Test]
        public void TestXING()
        {
            runTest(new string[] { }, "XING.JPG", "XING.bmp");
        }

        [Test]
        public void Testdoor()
        {
            runTest(new string[] { }, "door.jpg", "door.bmp");
        }

        [Test]
        public void Testolympusc960()
        {
            runTest(new string[] { }, "olympus-c960.jpg", "olympus-c960.bmp");
        }

        //////////////////////////////////////////////////////////////////////////
        // test fast (1-pass) dequantizer

        [Test]
        public void Test3D_fast()
        {
            runTest(new string[] { "-fast", "-colors", "256", "-bmp" }, "3D.JPG", "3D_fast.bmp");
        }

        [Test]
        public void TestBLU_fast()
        {
            runTest(new string[] { "-fast", "-colors", "256", "-bmp" }, "BLU.JPG", "BLU_fast.bmp");
        }

        [Test]
        public void TestGLOBE1_fast()
        {
            runTest(new string[] { "-fast", "-colors", "256", "-bmp" }, "GLOBE1.JPG", "GLOBE1_fast.bmp");
        }

        [Test]
        public void TestMARBLES_fast()
        {
            runTest(new string[] { "-fast", "-colors", "256", "-bmp" }, "MARBLES.JPG", "MARBLES_fast.bmp");
        }

        [Test]
        public void TestPARROTS_fast()
        {
            runTest(new string[] { "-fast", "-colors", "256", "-bmp" }, "PARROTS.JPG", "PARROTS_fast.bmp");
        }

        [Test]
        public void TestSPACE_fast()
        {
            runTest(new string[] { "-fast", "-colors", "256", "-bmp" }, "SPACE.JPG", "SPACE_fast.bmp");
        }

        [Test]
        public void TestXING_fast()
        {
            runTest(new string[] { "-fast", "-colors", "256", "-bmp" }, "XING.JPG", "XING_fast.bmp");
        }

        //////////////////////////////////////////////////////////////////////////
        // test slow (2-pass) dequantizer

        [Test]
        public void Test3D_slow()
        {
            runTest(new string[] { "-colors", "256", "-bmp" }, "3D.JPG", "3D_slow.bmp");
        }

        [Test]
        public void TestBLU_slow()
        {
            runTest(new string[] { "-colors", "256", "-bmp" }, "BLU.JPG", "BLU_slow.bmp");
        }

        [Test]
        public void TestGLOBE1_slow()
        {
            runTest(new string[] { "-colors", "256", "-bmp" }, "GLOBE1.JPG", "GLOBE1_slow.bmp");
        }

        [Test]
        public void TestMARBLES_slow()
        {
            runTest(new string[] { "-colors", "256", "-bmp" }, "MARBLES.JPG", "MARBLES_slow.bmp");
        }

        [Test]
        public void TestPARROTS_slow()
        {
            runTest(new string[] { "-colors", "256", "-bmp" }, "PARROTS.JPG", "PARROTS_slow.bmp");
        }

        [Test]
        public void TestSPACE_slow()
        {
            runTest(new string[] { "-colors", "256", "-bmp" }, "SPACE.JPG", "SPACE_slow.bmp");
        }

        [Test]
        public void TestXING_slow()
        {
            runTest(new string[] { "-colors", "256", "-bmp" }, "XING.JPG", "XING_slow.bmp");
        }
    }
}
