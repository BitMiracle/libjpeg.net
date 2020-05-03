using System;
using System.Globalization;
using BitMiracle.LibJpeg.Classic;

namespace LibJpeg.Net.Benchmarks
{
    class LibJpegErrorHandler : jpeg_error_mgr
    {
        public override void error_exit()
        {
            string buffer = format_message();
            string message = string.Format(CultureInfo.CurrentCulture, "Failed to process JPEG image: {0}", buffer);
            throw new InvalidProgramException(message);
        }

        public override void output_message()
        {
            string buffer = format_message();
            Console.WriteLine(buffer);
        }
    }
}
