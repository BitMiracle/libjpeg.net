using System.IO;

using NUnit.Framework;

using BitMiracle.LibJpeg.Classic;

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

        [Test, TestCaseSource(nameof(Files))]
        public void TestCompression(string file)
        {
            Tester.PerformCompressionTest(new string[] { }, file, "");
        }

        [Test, TestCaseSource(nameof(Files))]
        public void TestQuality(string file)
        {
            Tester.PerformCompressionTest(new string[] { "-quality", "25" }, file, "_25");
        }

        [Test, TestCaseSource(nameof(Files))]
        public void TestOptimized(string file)
        {
            Tester.PerformCompressionTest(new string[] { "-optimize" }, file, "_opt");
        }

        [Test, TestCaseSource(nameof(Files))]
        public void TestProgressive(string file)
        {
            Tester.PerformCompressionTest(new string[] { "-progressive" }, file, "_prog");
        }

        [Test, TestCaseSource(nameof(Files))]
        public void TestGrayscale(string file)
        {
            Tester.PerformCompressionTest(new string[] { "-grayscale" }, file, "_gray");
        }

        [Test]
        public void TestCompressorWithContextRows()
        {
            using (MemoryStream stream = new MemoryStream())
            {
                jpeg_compress_struct compressor = new jpeg_compress_struct(new jpeg_error_mgr())
                {
                    Image_height = 100,
                    Image_width = 100,
                    In_color_space = J_COLOR_SPACE.JCS_GRAYSCALE,
                    Input_components = 1
                };
                compressor.jpeg_set_defaults();

                compressor.Dct_method = J_DCT_METHOD.JDCT_IFAST;
                compressor.Smoothing_factor = 94;
                compressor.jpeg_set_quality(75, true);
                compressor.jpeg_simple_progression();

                compressor.Density_unit = DensityUnit.Unknown;
                compressor.X_density = (short)96;
                compressor.Y_density = (short)96;

                compressor.jpeg_stdio_dest(stream);
                compressor.jpeg_start_compress(true);

                byte[][] rowForDecompressor = new byte[1][];
                int bytesPerPixel = 1;
                while (compressor.Next_scanline < compressor.Image_height)
                {
                    byte[] row = new byte[100 * bytesPerPixel]; // wasteful, but gets you 0 bytes every time - content is immaterial.
                    rowForDecompressor[0] = row;
                    compressor.jpeg_write_scanlines(rowForDecompressor, 1);
                }
                compressor.jpeg_finish_compress();

                byte[] bytes = stream.ToArray();

                string filename = "TestCompressorWithContextRows.jpg";
                var outputPath = Tester.MapOutputPath(filename);
                File.WriteAllBytes(outputPath, bytes);
                FileAssert.AreEqual(Tester.MapExpectedPath(filename), outputPath);

                File.Delete(outputPath);
            }
        }
    }
}
