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
    }
}