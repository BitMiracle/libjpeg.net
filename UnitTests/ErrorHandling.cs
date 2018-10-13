using System;
using System.IO;

using NUnit.Framework;

namespace UnitTests
{
    [TestFixture]
    public class ErrorHandlingTests
    {
        private Tester m_testerCompress = new Tester(true);
        private Tester m_testerDecompress = new Tester(false);

        private TextWriter m_consoleOutBefore;

        [SetUp]
        public void SetUp()
        {
            m_consoleOutBefore = Console.Out;
            FileStream fs = new FileStream("ConsoleOutput.txt", FileMode.Create);
            Console.SetOut(new StreamWriter(fs));
        }

        [TearDown]
        public void TearDown()
        {
            TextWriter output = Console.Out;
            output.Flush();
            output.Close();

            Console.SetOut(m_consoleOutBefore);
        }

        [Test]
        public void TestEmptyTargetImage_Compress()
        {
            Assert.Throws<DirectoryNotFoundException>(() =>
            {
                m_testerCompress.Run(new string[] { }, Tester.MapOpenPath("testimg.bmp"), Tester.Testcase);
            });
        }

        [Test]
        public void TestEmptyTargetImage_Decompress()
        {
            Assert.Throws<DirectoryNotFoundException>(() =>
            {
                m_testerDecompress.Run(new string[] { }, Tester.MapOpenPath("3D.JPG"), Tester.Testcase);
            });
        }

        [Test]
        public void TestEmptySourceImage_Compress()
        {
            Assert.Throws<FileNotFoundException>(() =>
            {
                Tester.PerformCompressionTest(new string[] { }, "", "asd.jpg");
            });
        }

        [Test]
        public void TestEmptySourceImage_Decompress()
        {
            Assert.Throws<FileNotFoundException>(() =>
            {
                Tester.PerformDecompressionTest(new string[] { }, "", "asd.jpg");
            });
        }

        [Test]
        public void TestWrongSourceImage_Compress()
        {
            Assert.Throws<FileNotFoundException>(() =>
            {
                m_testerCompress.Run(new string[] { }, "q.bmp", "asd.jpg");
            });
        }

        [Test]
        public void TestWrongSourceImage_Decompress()
        {
            Assert.Throws<FileNotFoundException>(() =>
            {
                m_testerCompress.Run(new string[] { }, "q.bmp", "asd.jpg");
            });
        }

        //[Test]
        //public void TestWritingOfUsage_Compress()
        //{
        //    m_testerCompress.Run(new string[] { "-qwerty" }, "testimg.bmp", "testimg_gray.jpg");
        //}

        //[Test]
        //public void TestWritingOfUsage_Decompress()
        //{
        //    m_testerDecompress.Run(new string[] { "-qwerty" }, "3D.JPG", "3D.bmp");
        //}
    }
}