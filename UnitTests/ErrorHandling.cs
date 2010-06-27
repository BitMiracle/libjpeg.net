using System;
using System.Collections.Generic;
using System.IO;
using System.Text;

using NUnit.Framework;

namespace UnitTests
{
    [TestFixture]
    public class ErrorHandlingTests
    {
        private Tester m_testerCompress = new Tester(true);
        private Tester m_testerDecompress = new Tester(false);

        private TextWriter m_consoleOutBefore;

        [TestFixtureSetUp]
        public void SetUp()
        {
            m_consoleOutBefore = Console.Out;
            FileStream fs = new FileStream("ConsoleOutput.txt", FileMode.Create);
            Console.SetOut(new StreamWriter(fs));
        }

        [TestFixtureTearDown]
        public void TearDown()
        {
            TextWriter output = Console.Out;
            output.Flush();
            output.Close();

            Console.SetOut(m_consoleOutBefore);
        }

        [Test]
        [ExpectedException(typeof(DirectoryNotFoundException))]
        public void TestEmptyTargetImage_Compress()
        {
            m_testerCompress.Run(new string[] { }, Path.Combine(Tester.Testcase, "testimg.bmp"), Tester.Testcase);
        }

        [Test]
        [ExpectedException(typeof(DirectoryNotFoundException))]
        public void TestEmptyTargetImage_Decompress()
        {
            m_testerDecompress.Run(new string[] { }, Path.Combine(Tester.Testcase, "3D.JPG"), Tester.Testcase);
        }

        [Test]
        [ExpectedException(typeof(FileNotFoundException))]
        public void TestEmptySourceImage_Compress()
        {
            Tester.PerformCompressionTest(new string[] { }, "", "asd.jpg");
        }

        [Test]
        [ExpectedException(typeof(FileNotFoundException))]
        public void TestEmptySourceImage_Decompress()
        {
            Tester.PerformDecompressionTest(new string[] { }, "", "asd.jpg");
        }

        [Test]
        [ExpectedException(typeof(FileNotFoundException))]
        public void TestWrongSourceImage_Compress()
        {
            m_testerCompress.Run(new string[] { }, "q.bmp", "asd.jpg");
        }

        [Test]
        [ExpectedException(typeof(FileNotFoundException))]
        public void TestWrongSourceImage_Decompress()
        {
            m_testerCompress.Run(new string[] { }, "q.bmp", "asd.jpg");
        }

        [Test]
        public void TestWritingOfUsage_Compress()
        {
            m_testerCompress.Run(new string[] { "-qwerty" }, "testimg.bmp", "testimg_gray.jpg");
        }

        [Test]
        public void TestWritingOfUsage_Decompress()
        {
            m_testerDecompress.Run(new string[] { "-qwerty" }, "3D.JPG", "3D.bmp");
        }
    }
}