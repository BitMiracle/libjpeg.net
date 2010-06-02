using System;
using System.Collections.Generic;
using System.IO;
using System.Text;

using NUnit.Framework;

namespace UnitTests
{
    class Tester
    {
        private const string m_testcase = @"..\..\..\TestCase\";
        private static object locked = new object();

        private bool m_testClassicImplementation = true;

        private bool m_compression;
        private string m_dataFolder;

        public Tester(string dataFolder, bool compression)
        {
            m_dataFolder = dataFolder;
            m_compression = compression;
        }

        public void Run(string[] args, string sourceImage, string targetImage)
        {
            // xJpeg.Program.Main is static, so lock concurent access to a test code
            // use a private field to lock upon 

            lock (locked)
            {
                string dataFolder = m_testcase + m_dataFolder;
                List<string> completeArgs = new List<string>(1 + args.Length + 2);

                if (!m_testClassicImplementation)
                    completeArgs.Add(m_compression ? "-c" : "-d");

                for (int i = 0; i < args.Length; ++i)
                    completeArgs.Add(args[i]);

                completeArgs.Add(Path.Combine(dataFolder, sourceImage));
                completeArgs.Add(targetImage);

                if (m_testClassicImplementation)
                {
                    if (m_compression)
                        BitMiracle.cJpeg.Program.Main(completeArgs.ToArray());
                    else
                        BitMiracle.dJpeg.Program.Main(completeArgs.ToArray());
                }
                else
                    BitMiracle.Jpeg.Program.Main(completeArgs.ToArray());

                Assert.IsTrue(Utils.FilesAreEqual(Path.Combine(dataFolder, targetImage), targetImage));
            }
        }
    }
}
