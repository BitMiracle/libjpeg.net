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

        [Test]
        public void TestCompressionFromDotNetBitmap()
        {
            string[] testFiles = new string[] { "particle.bmp", "2pilots.jpg", "ammerland.jpg", "ca-map.gif", "rainbow.bmp" };
            foreach (string fileName in testFiles)
            {
                Bitmap bmp = new Bitmap(m_dataFolder + fileName);
                DotNetBitmapSource source = new DotNetBitmapSource(bmp);

                using (FileStream output = new FileStream("Compressed" + fileName + ".jpg", FileMode.Create))
                {
                    Jpeg jpeg = new Jpeg();
                    jpeg.Compress(source, output);
                }
            }
        }

        [Test]
        public void TestJpegImage()
        {
            Bitmap bmp = new Bitmap(m_dataFolder + "particle.bmp");
            JpegImage jpeg = new JpegImage(bmp);
            Assert.AreEqual(jpeg.Width, bmp.Width);
            Assert.AreEqual(jpeg.Height, bmp.Height);
            for (int i = 0; i < jpeg.Height; ++i)
            {
                RowOfSamples row = jpeg.GetRow(i);
                Assert.IsNotNull(row);
                Assert.AreEqual(row.SampleCount, jpeg.Width);
            }
        }
    }
}