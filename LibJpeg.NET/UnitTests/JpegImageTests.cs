using System;
using System.Collections.Generic;
using System.Drawing;
using System.IO;
using System.Text;

using NUnit.Framework;

using BitMiracle.LibJpeg;

namespace UnitTests
{
    [TestFixture]
    public class JpegImageTests
    {
        private const string m_expectedResults = @"..\..\ExpectedResults\";
        private const string m_testcase = @"..\..\..\..\TestCase\";
        private const string m_dataFolder = m_testcase + @"Data\";

        [Test]
        public void TestCompressionResultSameAsCJpeg()
        {
            string pathToTestFiles = m_testcase + @"jpeg_compression_data\";
            JpegImage jpeg = new JpegImage(pathToTestFiles + "test24.bmp");
            testJpegOutput(jpeg, "test24.jpg", pathToTestFiles);

            CompressionParameters parameters = new CompressionParameters();
            parameters.Quality = 25;
            testJpegOutput(jpeg, parameters, "test24_25.jpg", pathToTestFiles);

            parameters = new CompressionParameters();
            parameters.SimpleProgressive = true;
            testJpegOutput(jpeg, parameters, "test24_prog.jpg", pathToTestFiles);
        }

        [Test]
        public void TestDecompressionFromCMYKJpeg()
        {
            JpegImage jpeg = new JpegImage(m_dataFolder + "ammerland.jpg");
            Assert.AreEqual(jpeg.BitsPerComponent, 8);
            Assert.AreEqual(jpeg.ComponentsPerSample, 4);
            Assert.AreEqual(jpeg.Colorspace, Colorspace.CMYK);
            Assert.AreEqual(jpeg.Width, 315);
            Assert.AreEqual(jpeg.Height, 349);

            using (FileStream output = new FileStream("ammerland.bmp", FileMode.Create))
                jpeg.WriteBitmap(output);
        }

        [Test]
        public void TestJpegImageFromBitmap()
        {
            string[] bitmaps = new string[4] { "duck.bmp", "particle.bmp", "pink.png", "rainbow.bmp" };
            foreach (string fileName in bitmaps)
            {
                string jpegFileName = fileName.Remove(fileName.Length - 4);
                jpegFileName += ".jpg";

                using (Bitmap bmp = new Bitmap(m_dataFolder + fileName))
                    testJpegFromBitmap(bmp, jpegFileName);

                testJpegFromFile(m_dataFolder + fileName, jpegFileName, m_expectedResults);
            }
        }

        [Test]
        [Ignore]
        public void TestJpegImageFromStream()
        {
            List<string> jpegs = new List<string>();
            jpegs.Add("BLU.JPG");
            jpegs.Add("PARROTS.JPG");
            jpegs.Add("3D.JPG");

            foreach (string fileName in jpegs)
            {
                string outputFileName = fileName.Replace(".JPG", ".bmp");
                using (FileStream output = new FileStream(outputFileName, FileMode.Create))
                {
                    JpegImage jpeg = new JpegImage(m_dataFolder + @"jpg/" + fileName);
                    jpeg.WriteBitmap(output);
                }

                Assert.IsTrue(Utils.FilesAreEqual(outputFileName, Path.Combine(m_expectedResults, outputFileName)));
            }
        }

        [Test]
        public void TestCreateJpegImageFromPixels()
        {
            JpegImage jpegImage = createImageFromPixels();

            testJpegOutput(jpegImage, "JpegImageFromPixels.jpg", m_expectedResults);

            const string outputBitmap = "JpegImageFromPixels.png";
            using (FileStream output = new FileStream(outputBitmap, FileMode.Create))
                jpegImage.WriteBitmap(output);

            Assert.IsTrue(Utils.FilesAreEqual(outputBitmap, Path.Combine(m_expectedResults, outputBitmap)));
        }

        [Test]
        public void TestCreateFromPixelsAndRecompress()
        {
            JpegImage jpegImage = createImageFromPixels();

            CompressionParameters compressionParameters = new CompressionParameters();
            compressionParameters.Quality = 20;
            const string output = "JpegImageFromPixels_20.jpg";
            testJpegOutput(jpegImage, compressionParameters, output, m_expectedResults);

            JpegImage recompressedImage = new JpegImage(output);
            Assert.AreEqual(recompressedImage.Colorspace, jpegImage.Colorspace);
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


        private static void testJpegFromBitmap(Bitmap bmp, string jpegFileName)
        {
            JpegImage jpeg = new JpegImage(bmp);
            Assert.AreEqual(jpeg.Width, bmp.Width);
            Assert.AreEqual(jpeg.Height, bmp.Height);
            Assert.AreEqual(jpeg.ComponentsPerSample, 3);//Number of components in Bitmap

            using (FileStream output = new FileStream(jpegFileName, FileMode.Create))
                jpeg.WriteJpeg(output);

            Assert.IsTrue(Utils.FilesAreEqual(jpegFileName, Path.Combine(m_expectedResults, jpegFileName)));
        }

        private static void testJpegFromFile(string fileName, string jpegFileName, string folderWithExpectedResults)
        {
            JpegImage jpeg = new JpegImage(fileName);
            testJpegOutput(jpeg, jpegFileName, folderWithExpectedResults);
        }

        private static void testJpegOutput(JpegImage jpeg, string jpegFileName, string folderWithExpectedResults)
        {
            testJpegOutput(jpeg, new CompressionParameters(), jpegFileName, folderWithExpectedResults);
        }

        private static void testJpegOutput(JpegImage jpeg, CompressionParameters parameters, string jpegFileName, string folderWithExpectedResults)
        {
            using (FileStream output = new FileStream(jpegFileName, FileMode.Create))
                jpeg.WriteJpeg(output, parameters);

            Assert.IsTrue(Utils.FilesAreEqual(jpegFileName, Path.Combine(folderWithExpectedResults, jpegFileName)));
        }
    }
}