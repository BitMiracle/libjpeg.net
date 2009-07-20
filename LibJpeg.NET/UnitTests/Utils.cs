using System;
using System.Collections.Generic;
using System.Text;
using System.IO;

using NUnit.Framework;

namespace UnitTests
{
    class Utils
    {
        internal static bool FilesAreEqual(string left, string right)
        {
            byte[] leftBytes = File.ReadAllBytes(left);
            byte[] rightBytes = File.ReadAllBytes(right);

            if (leftBytes == null || rightBytes == null)
                return false;

            if (leftBytes.Length != rightBytes.Length)
                return false;

            for (int i = 0; i < leftBytes.Length; i++)
            {
                if (leftBytes[i] != rightBytes[i])
                    return false;
            }

            return true;
        }

        internal static void TestCompression(string[] args, string sourceImage, string targetImage, bool testClassicImpl, string dataFolder)
        {
            testCompressionOrDecompression(true, args, sourceImage, targetImage, testClassicImpl, dataFolder);
        }

        internal static void TestDecompression(string[] args, string sourceImage, string targetImage, bool testClassicImpl, string dataFolder)
        {
            testCompressionOrDecompression(false, args, sourceImage, targetImage, testClassicImpl, dataFolder);
        }

        private static void testCompressionOrDecompression(bool compression, string[] args, string sourceImage, string targetImage, bool testClassicImpl, string dataFolder)
        {
            // xJpeg.Program.Main is static, so lock concurent access to a test code
            // use a private field to lock upon 

            object locked = new object();
            lock (locked)
            {
                List<string> completeArgs = new List<string>(1 + args.Length + 2);

                if (!testClassicImpl)
                    completeArgs.Add(compression ? "-c" : "-d");

                for (int i = 0; i < args.Length; ++i)
                    completeArgs.Add(args[i]);

                completeArgs.Add(Path.Combine(dataFolder, sourceImage));
                completeArgs.Add(targetImage);

                if (testClassicImpl)
                {
                    if (compression)
                        cJpeg.Program.Main(completeArgs.ToArray());
                    else
                        dJpeg.Program.Main(completeArgs.ToArray());
                }
                else
                    Jpeg.Program.Main(completeArgs.ToArray());

                Assert.IsTrue(Utils.FilesAreEqual(Path.Combine(dataFolder, targetImage), targetImage));
            }
        }
    }
}
