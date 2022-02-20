using System.Linq;

using NUnit.Framework;

namespace UnitTests
{
    [TestFixture]
    public class Transformations
    {
        [TestCase("door.jpg")]
        public void Rotate90(string fileName)
        {
            Tester.TestTransform("Rotate90", fileName, "-copy", "none", "-rotate", "90");
        }

        [TestCase("ExifOrientation.jpg", "1")] // 1 = Horizontal (normal) 
        [TestCase("ExifOrientation.jpg", "2", "-flip", "horizontal")] // 2 = Mirror horizontal 
        [TestCase("ExifOrientation.jpg", "3", "-rotate", "180")] // 3 = Rotate 180
        [TestCase("ExifOrientation.jpg", "4", "-flip", "vertical")] // 4 = Mirror vertical
        [TestCase("ExifOrientation.jpg", "6", "-rotate", "90")] // 6 = Rotate 90 CW 
        [TestCase("ExifOrientation.jpg", "8", "-rotate", "270")] // 8 = Rotate 270 CW 
        [TestCase("ExifOrientation2.jpg", "1")]
        [TestCase("ExifOrientation2.jpg", "2", "-flip", "horizontal")]
        [TestCase("ExifOrientation2.jpg", "3", "-rotate", "180")]
        [TestCase("ExifOrientation2.jpg", "4", "-flip", "vertical")]
        [TestCase("ExifOrientation2.jpg", "6", "-rotate", "90")]
        [TestCase("ExifOrientation2.jpg", "8", "-rotate", "270")]
        [TestCase("ExifOrientation3.jpg", "1")]
        [TestCase("ExifOrientation3.jpg", "2", "-flip", "horizontal")]
        [TestCase("ExifOrientation3.jpg", "3", "-rotate", "180")]
        [TestCase("ExifOrientation3.jpg", "4", "-flip", "vertical")]
        [TestCase("ExifOrientation3.jpg", "6", "-rotate", "90")]
        [TestCase("ExifOrientation3.jpg", "8", "-rotate", "270")]
        public void RotateImageWithExif(string fileName, string tag, params string[] extraArgs)
        {
            var args = new string[] { "-copy", "none" };
            var prefix = $"RotateImageWithExif_{tag}";
            Tester.TestTransform(prefix, fileName, args.ToList().Concat(extraArgs).ToArray());
        }
    }
}
