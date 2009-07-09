using System;
using System.Collections.Generic;
using System.Drawing;
using System.IO;
using System.Text;

using NUnit.Framework;

using LibJpeg;

namespace UnitTests
{
    [TestFixture]
    public class NewAPITests
    {
        private const string m_dataFolder = @"..\..\..\..\TestCase\Data\";
        private static List<string> m_testFiles = new List<string>();

        static NewAPITests()
        {
            DirectoryInfo testcaseDataDir = new DirectoryInfo(m_dataFolder);
            foreach (FileInfo fi in testcaseDataDir.GetFiles())
                m_testFiles.Add(fi.Name);

            DirectoryInfo testcaseJpgDir = new DirectoryInfo(m_dataFolder + @"jpg\");
            foreach (FileInfo fi in testcaseDataDir.GetFiles())
                m_testFiles.Add(fi.Name);
        }

        [Test]
        public void TestCompressionFromDotNetBitmap()
        {
            for (int i = 0; i < m_testFiles.Count; ++i)
            {
                string fileName = m_testFiles[i];
                Bitmap bmp = new Bitmap(m_dataFolder + fileName);
                DotNetBitmapSource source = new DotNetBitmapSource(bmp);

                if (fileName.Contains("\\"))
                    fileName = fileName.Replace('\\', ' ');
                using (FileStream output = new FileStream("Compressed" + fileName + ".jpg", FileMode.Create))
                {
                    Jpeg jpeg = new Jpeg();
                    jpeg.Compress(source, output);
                }
            }
        }

        [Test]
        public void TestJpegImageFromBitmap()
        {
            foreach (string fileName in m_testFiles)
            {
                Bitmap bmp = new Bitmap(m_dataFolder + fileName);
                JpegImage jpeg = new JpegImage(bmp);
                Assert.AreEqual(jpeg.Width, bmp.Width);
                Assert.AreEqual(jpeg.Height, bmp.Height);
                Assert.AreEqual(jpeg.ComponentsPerSample, 3);//Number of components in Bitmap
                for (int y = 0; y < jpeg.Height; ++y)
                {
                    RowOfSamples row = jpeg.GetRow(y);
                    Assert.IsNotNull(row);
                    Assert.AreEqual(row.SampleCount, jpeg.Width);

                    for (int x = 0; x < row.SampleCount; ++x)
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

        [Test]
        public void TestJpegImageDecompression()
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
    }
}