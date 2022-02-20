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
        [TestCase("ExifOrientation.jpg", "6", "-rotate", "90")] // 6 = Rotate 90 CW 
        public void RotateImageWithExif(string fileName, string tag, params string[] extraArgs)
        {
            var args = new string[] { "-copy", "none" };
            var prefix = $"RotateImageWithExif_{tag}";
            Tester.TestTransform(prefix, fileName, args.ToList().Concat(extraArgs).ToArray());
        }
    }
}
