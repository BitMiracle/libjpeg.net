﻿using System;
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
        private const string m_expectedResults = @"..\..\..\TestCase\Expected\";
        private const string m_testcase = @"..\..\..\TestCase\";

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

        [Test]
        public void TestCompressionResultsSameAsForCJpeg()
        {
            using (JpegImage jpeg = new JpegImage(Path.Combine(m_testcase, "test24.bmp")))
            {
                testJpegOutput(jpeg, "test24.jpg", m_expectedResults);

                CompressionParameters parameters = new CompressionParameters();
                parameters.Quality = 25;
                testJpegOutput(jpeg, parameters, "test24_25.jpg", m_expectedResults);

                parameters = new CompressionParameters();
                parameters.SimpleProgressive = true;
                testJpegOutput(jpeg, parameters, "test24_prog.jpg", m_expectedResults);
            }
        }

        [Test, TestCaseSource("DecompressionFiles")]
        public void TestDecompressionResultsSameAsForDJpeg(string fileName)
        {
            string outputFileName = fileName.Replace(".JPG", ".bmp");
            testBitmapFromFile(m_testcase + fileName, outputFileName, m_expectedResults);
        }

        [Test]
        public void TestDecompressionFromCMYKJpeg()
        {
            using (JpegImage jpeg = new JpegImage(m_testcase + "ammerland.jpg"))
            {
                Assert.AreEqual(jpeg.BitsPerComponent, 8);
                Assert.AreEqual(jpeg.ComponentsPerSample, 4);
                Assert.AreEqual(jpeg.Colorspace, Colorspace.CMYK);
                Assert.AreEqual(jpeg.Width, 315);
                Assert.AreEqual(jpeg.Height, 349);

                testBitmapOutput(jpeg, "ammerland.bmp", m_expectedResults);
            }
        }

        [Test, TestCaseSource("BitmapFiles")]
        public void TestJpegImageFromBitmap(string fileName)
        {
            string jpegFileName = fileName.Remove(fileName.Length - 4);
            jpegFileName += ".jpg";

            using (Bitmap bmp = new Bitmap(m_testcase + fileName))
                testJpegFromBitmap(bmp, jpegFileName);

            testJpegFromFile(m_testcase + fileName, jpegFileName, m_expectedResults);
        }

        [Test]
        public void TestCreateJpegImageFromPixels()
        {
            using (JpegImage jpegImage = createImageFromPixels())
            {
                testJpegOutput(jpegImage, "JpegImageFromPixels.jpg", m_expectedResults);
                testBitmapOutput(jpegImage, "JpegImageFromPixels.png", m_expectedResults);
            }
        }

        [Test]
        public void TestGrayscaleJpegToBitmap()
        {
            using (JpegImage jpegImage = new JpegImage(m_testcase + "turkey.jpg"))
            {
                testBitmapOutput(jpegImage, "turkey.png", m_expectedResults);
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
                testJpegOutput(jpegImage, compressionParameters, output, m_expectedResults);

                using (JpegImage recompressedImage = new JpegImage(output))
                {
                    Assert.AreEqual(recompressedImage.Colorspace, jpegImage.Colorspace);
                }
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

            FileAssert.AreEqual(jpegFileName, Path.Combine(m_expectedResults, jpegFileName));
        }

        private static void testJpegFromFile(string fileName, string jpegFileName, string folderWithExpectedResults)
        {
            using (JpegImage jpeg = new JpegImage(fileName))
            {
                testJpegOutput(jpeg, jpegFileName, folderWithExpectedResults);
            }
        }

        private static void testJpegOutput(JpegImage jpeg, string jpegFileName, string folderWithExpectedResults)
        {
            testJpegOutput(jpeg, new CompressionParameters(), jpegFileName, folderWithExpectedResults);
        }

        private static void testJpegOutput(JpegImage jpeg, CompressionParameters parameters, string jpegFileName, string folderWithExpectedResults)
        {
            using (FileStream output = new FileStream(jpegFileName, FileMode.Create))
                jpeg.WriteJpeg(output, parameters);

            FileAssert.AreEqual(jpegFileName, Path.Combine(folderWithExpectedResults, jpegFileName));
        }

        private static void testBitmapFromFile(string sourceFileName, string bitmapFileName, string folderWithExpectedResults)
        {
            using (JpegImage jpeg = new JpegImage(sourceFileName))
            {
                testBitmapOutput(jpeg, bitmapFileName, folderWithExpectedResults);
            }
        }

        private static void testBitmapOutput(JpegImage jpeg, string bitmapFileName, string folderWithExpectedResults)
        {
            using (FileStream output = new FileStream(bitmapFileName, FileMode.Create))
                jpeg.WriteBitmap(output);

            FileAssert.AreEqual(bitmapFileName, Path.Combine(folderWithExpectedResults, bitmapFileName));
        }
    }
}