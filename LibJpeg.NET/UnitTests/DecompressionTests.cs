using System;
using System.Collections.Generic;
using System.Text;
using System.IO;

using NUnit.Framework;

namespace UnitTests
{
    [TestFixture]
    public class DecompressionTests
    {
        private Tester m_tester = new Tester(@"jpeg_decompression_data\", false);

        [Test]
        public void Test3D()
        {
            m_tester.Run(new string[] { }, "3D.JPG", "3D.bmp");
        }

        [Test]
        public void TestBLU()
        {
            m_tester.Run(new string[] { }, "BLU.JPG", "BLU.bmp");
        }

        [Test]
        public void TestGLOBE1()
        {
            m_tester.Run(new string[] { }, "GLOBE1.JPG", "GLOBE1.bmp");
        }

        [Test]
        public void TestMARBLES()
        {
            m_tester.Run(new string[] { }, "MARBLES.JPG", "MARBLES.bmp");
        }

        [Test]
        public void TestPARROTS()
        {
            m_tester.Run(new string[] { }, "PARROTS.JPG", "PARROTS.bmp");
        }

        [Test]
        public void TestSPACE()
        {
            m_tester.Run(new string[] { }, "SPACE.JPG", "SPACE.bmp");
        }

        [Test]
        public void TestXING()
        {
            m_tester.Run(new string[] { }, "XING.JPG", "XING.bmp");
        }

        [Test]
        public void Testdoor()
        {
            m_tester.Run(new string[] { }, "door.jpg", "door.bmp");
        }

        [Test]
        public void Testolympusc960()
        {
            m_tester.Run(new string[] { }, "olympus-c960.jpg", "olympus-c960.bmp");
        }

        //////////////////////////////////////////////////////////////////////////
        // test fast (1-pass) dequantizer

        [Test]
        public void Test3D_fast()
        {
            m_tester.Run(new string[] { "-fast", "-colors", "256", "-bmp" }, "3D.JPG", "3D_fast.bmp");
        }

        [Test]
        public void TestBLU_fast()
        {
            m_tester.Run(new string[] { "-fast", "-colors", "256", "-bmp" }, "BLU.JPG", "BLU_fast.bmp");
        }

        [Test]
        public void TestGLOBE1_fast()
        {
            m_tester.Run(new string[] { "-fast", "-colors", "256", "-bmp" }, "GLOBE1.JPG", "GLOBE1_fast.bmp");
        }

        [Test]
        public void TestMARBLES_fast()
        {
            m_tester.Run(new string[] { "-fast", "-colors", "256", "-bmp" }, "MARBLES.JPG", "MARBLES_fast.bmp");
        }

        [Test]
        public void TestPARROTS_fast()
        {
            m_tester.Run(new string[] { "-fast", "-colors", "256", "-bmp" }, "PARROTS.JPG", "PARROTS_fast.bmp");
        }

        [Test]
        public void TestSPACE_fast()
        {
            m_tester.Run(new string[] { "-fast", "-colors", "256", "-bmp" }, "SPACE.JPG", "SPACE_fast.bmp");
        }

        [Test]
        public void TestXING_fast()
        {
            m_tester.Run(new string[] { "-fast", "-colors", "256", "-bmp" }, "XING.JPG", "XING_fast.bmp");
        }

        //////////////////////////////////////////////////////////////////////////
        // test slow (2-pass) dequantizer

        [Test]
        public void Test3D_slow()
        {
            m_tester.Run(new string[] { "-colors", "256", "-bmp" }, "3D.JPG", "3D_slow.bmp");
        }

        [Test]
        public void TestBLU_slow()
        {
            m_tester.Run(new string[] { "-colors", "256", "-bmp" }, "BLU.JPG", "BLU_slow.bmp");
        }

        [Test]
        public void TestGLOBE1_slow()
        {
            m_tester.Run(new string[] { "-colors", "256", "-bmp" }, "GLOBE1.JPG", "GLOBE1_slow.bmp");
        }

        [Test]
        public void TestMARBLES_slow()
        {
            m_tester.Run(new string[] { "-colors", "256", "-bmp" }, "MARBLES.JPG", "MARBLES_slow.bmp");
        }

        [Test]
        public void TestPARROTS_slow()
        {
            m_tester.Run(new string[] { "-colors", "256", "-bmp" }, "PARROTS.JPG", "PARROTS_slow.bmp");
        }

        [Test]
        public void TestSPACE_slow()
        {
            m_tester.Run(new string[] { "-colors", "256", "-bmp" }, "SPACE.JPG", "SPACE_slow.bmp");
        }

        [Test]
        public void TestXING_slow()
        {
            m_tester.Run(new string[] { "-colors", "256", "-bmp" }, "XING.JPG", "XING_slow.bmp");
        }
    }
}
