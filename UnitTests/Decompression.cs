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

        [Test, TestCaseSource("FullList")]
        public void TestDeCompression(string file)
        {
            Tester.PerformDecompressionTest(new string[] { }, file, "");
        }

        [Test, TestCaseSource("ShortList")]
        public void TestFastDequantizer(string file)
        {
            // test fast (1-pass) dequantizer
            Tester.PerformDecompressionTest(new string[] { "-fast", "-colors", "256", "-bmp" }, file, "_fast");
        }

        [Test, TestCaseSource("ShortList")]
        public void TestSlowDequantizer(string file)
        {
            // test slow (2-pass) dequantizer
            Tester.PerformDecompressionTest(new string[] { "-colors", "256", "-bmp" }, file, "_slow");
        }

        [TestCase("logo1.jpg")]
        [TestCase("logo2.jpg")]
        public void TestCmykToRgb(string file)
        {
            // forcing rgb output
            Tester.PerformDecompressionTest(new string[] { "-rgb", "-colors", "256" }, file, "_cmyk2rgb256");
        }

        [Test]
        public void TestMarkerList()
        {
            jpeg_decompress_struct cinfo = new jpeg_decompress_struct();
            using (FileStream input = new FileStream(Path.Combine(Tester.Testcase, "PARROTS.JPG"), FileMode.Open))
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