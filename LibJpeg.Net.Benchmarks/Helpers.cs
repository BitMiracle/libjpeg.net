using System;
using System.IO;
using System.Linq;
using System.Text;

namespace LibJpeg.Net.Benchmarks
{
    static class Helpers
    {
        public static MemoryStream GetJpegStream(string fileName)
        {
            string pathToTestCase = findTestCase();
            return new MemoryStream(File.ReadAllBytes(Path.Combine(pathToTestCase, fileName)));
        }

        private static string findTestCase()
        {
            string currentDirectoryPath = Directory.GetCurrentDirectory();
            var pathToTestcase = new StringBuilder("TestCase\\");
            var dir = new DirectoryInfo(currentDirectoryPath);
            while (dir.Parent != null)
            {
                var testcase = dir.EnumerateDirectories("TestCase", SearchOption.TopDirectoryOnly).FirstOrDefault();
                if (testcase != null)
                {
                    return Path.Combine(currentDirectoryPath, pathToTestcase.ToString());
                }

                dir = dir.Parent;
                pathToTestcase.Insert(0, "..\\");
            }

            throw new InvalidProgramException("Unable to find TestCase directory");
        }
    }
}
