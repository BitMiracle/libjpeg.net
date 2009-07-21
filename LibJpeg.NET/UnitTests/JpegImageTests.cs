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
        private const string m_dataFolder = @"..\..\..\..\TestCase\Data\";
        private static List<string> m_testFiles = new List<string>();

        static JpegImageTests()
        {
            DirectoryInfo testcaseDataDir = new DirectoryInfo(m_dataFolder);
            foreach (FileInfo fi in testcaseDataDir.GetFiles())
                m_testFiles.Add(fi.Name);

            DirectoryInfo testcaseJpgDir = new DirectoryInfo(m_dataFolder + @"jpg\");
            foreach (FileInfo fi in testcaseJpgDir.GetFiles())
                m_testFiles.Add(@"jpg\" + fi.Name);
        }

        [Test]
        public void TestJpegImageFromBitmap()
        {
            foreach (string fileName in m_testFiles)
            {
                using (Bitmap bmp = new Bitmap(m_dataFolder + fileName))
                {
                    JpegImage jpeg = new JpegImage(bmp);
                    Assert.AreEqual(jpeg.Width, bmp.Width);
                    Assert.AreEqual(jpeg.Height, bmp.Height);
                    Assert.AreEqual(jpeg.ComponentsPerSample, 3);//Number of components in Bitmap
                    for (int y = 0; y < jpeg.Height; ++y)
                    {
                        SampleRow row = jpeg.GetRow(y);
                        Assert.IsNotNull(row);
                        Assert.AreEqual(row.Length, jpeg.Width);

                        for (int x = 0; x < row.Length; ++x)
                        {
                            Sample sample = row[x];
                            Assert.IsNotNull(sample);
                            Assert.AreEqual(sample.BitsPerComponent, jpeg.BitsPerComponent);
                            Assert.AreEqual(sample.ComponentCount, jpeg.ComponentsPerSample);

                            Color bitmapPixel = bmp.GetPixel(x, y);
                            Assert.AreEqual(sample.GetComponent(0), bitmapPixel.R);
                            Assert.AreEqual(sample.GetComponent(1), bitmapPixel.G);
                            Assert.AreEqual(sample.GetComponent(2), bitmapPixel.B);
                        }
                    }
                }
            }
        }

        [Test]
        public void TestJpegImageFromStream()
        {
            for (int i = 0; i < m_testFiles.Count; ++i)
            {
                string jpegFile = m_testFiles[i];
                using (FileStream jpegData = new FileStream(m_dataFolder + jpegFile, FileMode.Open))
                {
                    if (jpegFile.Contains("\\"))
                        jpegFile = jpegFile.Replace('\\', ' ');

                    JpegImage image = new JpegImage(jpegData);
                    using (FileStream output = new FileStream("Compressed" + jpegFile + ".jpg", FileMode.Create))
                        image.WriteCompressed(output);

                    using (FileStream output = new FileStream("Decompressed" + jpegFile + ".png", FileMode.Create))
                        image.WriteDecompressed(output);
                }
            }
        }

        [Test]
        public void TestCreateJpegImageFromPixels()
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

            using (FileStream output = new FileStream("JpegImageFromPixels.jpg", FileMode.Create))
                jpegImage.WriteCompressed(output);

            using (FileStream output = new FileStream("JpegImageFromPixels.png", FileMode.Create))
                jpegImage.WriteDecompressed(output);
        }
    }
}