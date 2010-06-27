using System;
using System.Collections.Generic;
using System.Text;
using System.IO;

using NUnit.Framework;

using BitMiracle.LibJpeg.Classic;

namespace UnitTests
{
    [TestFixture]
    public class DecompressionTests
    {
        private static string[] ShortList
        {
            get
            {
                return new string[]
                {
                    "3D.JPG",
                    "BLU.JPG",
                    "GLOBE1.JPG",
                    "MARBLES.JPG",
                    "PARROTS.JPG",
                    "SPACE.JPG",
                    "XING.JPG",
                };
            }
        }

        private static string[] FullList
        {
            get
            {
                return new string[]
                {
                    "3D.JPG",
                    "BLU.JPG",
                    "GLOBE1.JPG",
                    "MARBLES.JPG",
                    "PARROTS.JPG",
                    "SPACE.JPG",
                    "XING.JPG",
                    "door.jpg",
                    "olympus-c960.jpg"
                };
            }
        }

        private Tester m_tester = new Tester(@"jpeg_decompression_data\", false);

        [Test, TestCaseSource("FullList")]
        public void TestDeCompression(string file)
        {
            Tester.PerformDeCompressionTest(new string[] { }, file, "");
        }

        [Test, TestCaseSource("ShortList")]
        public void TestFastDequantizer(string file)
        {
            // test fast (1-pass) dequantizer
            Tester.PerformDeCompressionTest(new string[] { "-fast", "-colors", "256", "-bmp" }, file, "_fast");
        }

        [Test, TestCaseSource("ShortList")]
        public void TestSlowDequantizer(string file)
        {
            // test slow (2-pass) dequantizer
            Tester.PerformDeCompressionTest(new string[] { "-colors", "256", "-bmp" }, file, "_slow");
        }

        //[Test]
        //public void Test3D()
        //{
        //    m_tester.Run(new string[] { }, "3D.JPG", "3D.bmp");
        //}

        //[Test]
        //public void TestBLU()
        //{
        //    m_tester.Run(new string[] { }, "BLU.JPG", "BLU.bmp");
        //}

        //[Test]
        //public void TestGLOBE1()
        //{
        //    m_tester.Run(new string[] { }, "GLOBE1.JPG", "GLOBE1.bmp");
        //}

        //[Test]
        //public void TestMARBLES()
        //{
        //    m_tester.Run(new string[] { }, "MARBLES.JPG", "MARBLES.bmp");
        //}

        //[Test]
        //public void TestPARROTS()
        //{
        //    m_tester.Run(new string[] { }, "PARROTS.JPG", "PARROTS.bmp");
        //}

        //[Test]
        //public void TestSPACE()
        //{
        //    m_tester.Run(new string[] { }, "SPACE.JPG", "SPACE.bmp");
        //}

        //[Test]
        //public void TestXING()
        //{
        //    m_tester.Run(new string[] { }, "XING.JPG", "XING.bmp");
        //}

        //[Test]
        //public void Testdoor()
        //{
        //    m_tester.Run(new string[] { }, "door.jpg", "door.bmp");
        //}

        //[Test]
        //public void Testolympusc960()
        //{
        //    m_tester.Run(new string[] { }, "olympus-c960.jpg", "olympus-c960.bmp");
        //}

        ////////////////////////////////////////////////////////////////////////////
        //// test fast (1-pass) dequantizer

        //[Test]
        //public void Test3D_fast()
        //{
        //    m_tester.Run(new string[] { "-fast", "-colors", "256", "-bmp" }, "3D.JPG", "3D_fast.bmp");
        //}

        //[Test]
        //public void TestBLU_fast()
        //{
        //    m_tester.Run(new string[] { "-fast", "-colors", "256", "-bmp" }, "BLU.JPG", "BLU_fast.bmp");
        //}

        //[Test]
        //public void TestGLOBE1_fast()
        //{
        //    m_tester.Run(new string[] { "-fast", "-colors", "256", "-bmp" }, "GLOBE1.JPG", "GLOBE1_fast.bmp");
        //}

        //[Test]
        //public void TestMARBLES_fast()
        //{
        //    m_tester.Run(new string[] { "-fast", "-colors", "256", "-bmp" }, "MARBLES.JPG", "MARBLES_fast.bmp");
        //}

        //[Test]
        //public void TestPARROTS_fast()
        //{
        //    m_tester.Run(new string[] { "-fast", "-colors", "256", "-bmp" }, "PARROTS.JPG", "PARROTS_fast.bmp");
        //}

        //[Test]
        //public void TestSPACE_fast()
        //{
        //    m_tester.Run(new string[] { "-fast", "-colors", "256", "-bmp" }, "SPACE.JPG", "SPACE_fast.bmp");
        //}

        //[Test]
        //public void TestXING_fast()
        //{
        //    m_tester.Run(new string[] { "-fast", "-colors", "256", "-bmp" }, "XING.JPG", "XING_fast.bmp");
        //}

        ////////////////////////////////////////////////////////////////////////////
        //// test slow (2-pass) dequantizer

        //[Test]
        //public void Test3D_slow()
        //{
        //    m_tester.Run(new string[] { "-colors", "256", "-bmp" }, "3D.JPG", "3D_slow.bmp");
        //}

        //[Test]
        //public void TestBLU_slow()
        //{
        //    m_tester.Run(new string[] { "-colors", "256", "-bmp" }, "BLU.JPG", "BLU_slow.bmp");
        //}

        //[Test]
        //public void TestGLOBE1_slow()
        //{
        //    m_tester.Run(new string[] { "-colors", "256", "-bmp" }, "GLOBE1.JPG", "GLOBE1_slow.bmp");
        //}

        //[Test]
        //public void TestMARBLES_slow()
        //{
        //    m_tester.Run(new string[] { "-colors", "256", "-bmp" }, "MARBLES.JPG", "MARBLES_slow.bmp");
        //}

        //[Test]
        //public void TestPARROTS_slow()
        //{
        //    m_tester.Run(new string[] { "-colors", "256", "-bmp" }, "PARROTS.JPG", "PARROTS_slow.bmp");
        //}

        //[Test]
        //public void TestSPACE_slow()
        //{
        //    m_tester.Run(new string[] { "-colors", "256", "-bmp" }, "SPACE.JPG", "SPACE_slow.bmp");
        //}

        //[Test]
        //public void TestXING_slow()
        //{
        //    m_tester.Run(new string[] { "-colors", "256", "-bmp" }, "XING.JPG", "XING_slow.bmp");
        //}

        [Test]
        public void TestMarkerList()
        {
            jpeg_decompress_struct cinfo = new jpeg_decompress_struct();
            using (FileStream input = new FileStream(@"..\..\..\TestCase\PARROTS.JPG", FileMode.Open))
            {
                /* Specify data source for decompression */
                cinfo.jpeg_stdio_src(input);

                const int markerDataLengthLimit = 1000;
                cinfo.jpeg_save_markers((int)JPEG_MARKER.COM, markerDataLengthLimit);
                cinfo.jpeg_save_markers((int)JPEG_MARKER.APP0, markerDataLengthLimit);

                /* Read file header, set default decompression parameters */
                cinfo.jpeg_read_header(true);

                Assert.AreEqual(cinfo.Marker_list.Count, 3);

                int[] expectedMarkerType = { (int)JPEG_MARKER.APP0, (int)JPEG_MARKER.APP0, (int)JPEG_MARKER.COM };
                int[] expectedMarkerOriginalLength = { 14, 3072, 10 };
                for (int i = 0; i < cinfo.Marker_list.Count; ++i)
                {
                    jpeg_marker_struct marker = cinfo.Marker_list[i];
                    Assert.IsNotNull(marker);
                    Assert.AreEqual(marker.Marker, expectedMarkerType[i]);
                    Assert.AreEqual(marker.OriginalLength, expectedMarkerOriginalLength[i]);
                    Assert.LessOrEqual(marker.Data.Length, markerDataLengthLimit);
                }
            }
        }
    }
}