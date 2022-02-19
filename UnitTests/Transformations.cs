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
    }
}
