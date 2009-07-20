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
        private Tester m_tester = new Tester(@"jpeg_compression_data\", true);

        [Test]
        public void Test24()
        {
            m_tester.Run(new string[] { }, "test24.bmp", "test24.jpg");
        }

        [Test]
        public void Test24Quality()
        {
            m_tester.Run(new string[] { "-quality", "25" }, "test24.bmp", "test24_25.jpg");
        }

        [Test]
        public void Test24Optimize()
        {
            m_tester.Run(new string[] { "-optimize" }, "test24.bmp", "test24_opt.jpg");
        }

        [Test]
        public void Test24Progressive()
        {
            m_tester.Run(new string[] { "-progressive" }, "test24.bmp", "test24_prog.jpg");
        }

        [Test]
        public void Test24Grayscale()
        {
            m_tester.Run(new string[] { "-grayscale" }, "test24.bmp", "test24_gray.jpg");
        }

        [Test]
        public void Test8()
        {
            m_tester.Run(new string[] { }, "test8.bmp", "test8.jpg");
        }

        [Test]
        public void Test8Quality()
        {
            m_tester.Run(new string[] { "-quality", "25" }, "test8.bmp", "test8_25.jpg");
        }

        [Test]
        public void Test8Optimize()
        {
            m_tester.Run(new string[] { "-optimize" }, "test8.bmp", "test8_opt.jpg");
        }

        [Test]
        public void Test8Progressive()
        {
            m_tester.Run(new string[] { "-progressive" }, "test8.bmp", "test8_prog.jpg");
        }

        [Test]
        public void Test8Grayscale()
        {
            m_tester.Run(new string[] { "-grayscale" }, "test8.bmp", "test8_gray.jpg");
        }

        [Test]
        public void Testimg()
        {
            m_tester.Run(new string[] { }, "testimg.bmp", "testimg.jpg");
        }

        [Test]
        public void TestimgQuality()
        {
            m_tester.Run(new string[] { "-quality", "25" }, "testimg.bmp", "testimg_25.jpg");
        }

        [Test]
        public void TestimgOptimize()
        {
            m_tester.Run(new string[] { "-optimize" }, "testimg.bmp", "testimg_opt.jpg");
        }

        [Test]
        public void TestimgProgressive()
        {
            m_tester.Run(new string[] { "-progressive" }, "testimg.bmp", "testimg_prog.jpg");
        }

        [Test]
        public void TestimgGrayscale()
        {
            m_tester.Run(new string[] { "-grayscale" }, "testimg.bmp", "testimg_gray.jpg");
        }
    }
}
