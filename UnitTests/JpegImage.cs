using System.Drawing;
using System.IO;

using NUnit.Framework;

using BitMiracle.LibJpeg;

namespace UnitTests
{
    [TestFixture]
    public class JpegImageTests
    {
        private static string[] DecompressionFiles
        {
            get
            {
                return new string[]
                {
                    "BLU.JPG",
                    "PARROTS.JPG",
                    "3D.JPG",
                    "MARBLES.JPG",
                };
            }
        }

        private static string[] BitmapFiles
        {
            get
            {
                return new string[]
                {
                    "duck.bmp",
                    "particle.bmp",
                    "pink.png",
                    "rainbow.bmp"
                };
            }
        }

#if !NETSTANDARD
        [Test]
        public void TestCompressionResultsSameAsForCJpeg()
        {
            using (JpegImage jpeg = new JpegImage(Tester.MapOpenPath("test24.bmp")))
            {
                testJpegOutput(jpeg, "test24.jpg");

                CompressionParameters parameters = new CompressionParameters();
                parameters.Quality = 25;
                testJpegOutput(jpeg, parameters, "test24_25.jpg");

                parameters = new CompressionParameters();
                parameters.SimpleProgressive = true;
                testJpegOutput(jpeg, parameters, "test24_prog.jpg");
            }
        }

        [Test, TestCaseSource("DecompressionFiles")]
        public void TestDecompressionResultsSameAsForDJpeg(string fileName)
        {
            string outputFileName = fileName.Replace(".JPG", ".bmp");
            testBitmapFromFile(Tester.MapOpenPath(fileName), outputFileName);
        }

        [Test]
        public void TestDecompressionFromCMYKJpeg()
        {
            using (JpegImage jpeg = new JpegImage(Tester.MapOpenPath("ammerland.jpg")))
            {
                Assert.AreEqual(jpeg.BitsPerComponent, 8);
                Assert.AreEqual(jpeg.ComponentsPerSample, 4);
                Assert.AreEqual(jpeg.Colorspace, Colorspace.CMYK);
                Assert.AreEqual(jpeg.Width, 315);
                Assert.AreEqual(jpeg.Height, 349);

                testBitmapOutput(jpeg, "ammerland.bmp");
            }
        }

        [Test, TestCaseSource("BitmapFiles")]
        public void TestJpegImageFromBitmap(string fileName)
        {
            string jpegFileName = fileName.Remove(fileName.Length - 4);
            jpegFileName += ".jpg";

            using (Bitmap bmp = new Bitmap(Tester.MapOpenPath(fileName)))
                testJpegFromBitmap(bmp, jpegFileName);

            testJpegFromFile(Tester.MapOpenPath(fileName), jpegFileName);
        }

        [Test]
        public void TestGrayscaleJpegToBitmap()
        {
            using (JpegImage jpegImage = new JpegImage(Tester.MapOpenPath("turkey.jpg")))
            {
                testBitmapOutput(jpegImage, "turkey.png");
            }
        }

        [Test]
        public void TestCreateFromPixelsAndRecompress()
        {
            using (JpegImage jpegImage = createImageFromPixels())
            {
                CompressionParameters compressionParameters = new CompressionParameters();
                compressionParameters.Quality = 20;
                const string output = "JpegImageFromPixels_20.jpg";
                testJpegOutput(jpegImage, compressionParameters, output);

                using (JpegImage recompressedImage = new JpegImage(output))
                {
                    Assert.AreEqual(recompressedImage.Colorspace, jpegImage.Colorspace);
                }
            }
        }
#endif

        [Test]
        public void TestCreateJpegImageFromPixels()
        {
            using (JpegImage jpegImage = createImageFromPixels())
            {
                testJpegOutput(jpegImage, "JpegImageFromPixels.jpg");
                testBitmapOutput(jpegImage, "JpegImageFromPixels.png");
            }
        }

        private static JpegImage createImageFromPixels()
        {
            byte[] rowData = new byte[96];
            for (int i = 0; i < rowData.Length; ++i)
            {
                if (i < 5)
                    rowData[i] = 0xE4;
                else if (i < 15)
                    rowData[i] = 0xAB;
                else if (i < 35)
                    rowData[i] = 0x00;
                else if (i < 55)
                    rowData[i] = 0x65;
                else
                    rowData[i] = 0xF0;
            }

            const int width = 24;
            const int height = 25;
            const byte bitsPerComponent = 8;
            const byte componentsPerSample = 4;
            const Colorspace colorspace = Colorspace.CMYK;

            SampleRow row = new SampleRow(rowData, width, bitsPerComponent, componentsPerSample);
            SampleRow[] rows = new SampleRow[height];
            for (int i = 0; i < rows.Length; ++i)
                rows[i] = row;

            JpegImage jpegImage = new JpegImage(rows, colorspace);
            Assert.AreEqual(jpegImage.Width, width);
            Assert.AreEqual(jpegImage.Height, rows.Length);
            Assert.AreEqual(jpegImage.BitsPerComponent, bitsPerComponent);
            Assert.AreEqual(jpegImage.ComponentsPerSample, componentsPerSample);
            Assert.AreEqual(jpegImage.Colorspace, colorspace);
            return jpegImage;
        }

#if !NETSTANDARD
        private static void testJpegFromBitmap(Bitmap bmp, string jpegFileName)
        {
            using (JpegImage jpeg = new JpegImage(bmp))
            {
                Assert.AreEqual(jpeg.Width, bmp.Width);
                Assert.AreEqual(jpeg.Height, bmp.Height);
                Assert.AreEqual(jpeg.ComponentsPerSample, 3);//Number of components in Bitmap

                using (FileStream output = new FileStream(jpegFileName, FileMode.Create))
                    jpeg.WriteJpeg(output);
            }

            FileAssert.AreEqual(jpegFileName, Tester.MapExpectedPath(jpegFileName));
        }

        private static void testJpegFromFile(string fileName, string jpegFileName)
        {
            using (JpegImage jpeg = new JpegImage(fileName))
            {
                testJpegOutput(jpeg, jpegFileName);
            }
        }

        private static void testBitmapFromFile(string sourceFileName, string bitmapFileName)
        {
            using (JpegImage jpeg = new JpegImage(sourceFileName))
            {
                testBitmapOutput(jpeg, bitmapFileName);
            }
        }
#endif

        private static void testJpegOutput(JpegImage jpeg, string jpegFileName)
        {
            testJpegOutput(jpeg, new CompressionParameters(), jpegFileName);
        }

        private static void testJpegOutput(JpegImage jpeg, CompressionParameters parameters, string jpegFileName)
        {
            using (FileStream output = new FileStream(jpegFileName, FileMode.Create))
                jpeg.WriteJpeg(output, parameters);

            FileAssert.AreEqual(jpegFileName, Tester.MapExpectedPath(jpegFileName));
        }

        private static void testBitmapOutput(JpegImage jpeg, string bitmapFileName)
        {
            using (FileStream output = new FileStream(bitmapFileName, FileMode.Create))
                jpeg.WriteBitmap(output);

            FileAssert.AreEqual(bitmapFileName, Tester.MapExpectedPath(bitmapFileName));
        }
    }
}