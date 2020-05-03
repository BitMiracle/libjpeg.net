using System;
using System.IO;
using BitMiracle.LibJpeg.Classic;

namespace LibJpeg.Net.Benchmarks.Decode
{
    public abstract class DecodeBase
    {
        protected MemoryStream m_input;
        protected MemoryStream m_output;

        protected abstract string InputFileName { get; }
        protected virtual int OutputCapacity => 2 * 1024 * 1024;

        protected void SetupBase()
        {
            m_input = Helpers.GetJpegStream(InputFileName);
            m_output = new MemoryStream(OutputCapacity);
        }

        protected void IterationSetupBase()
        {
            m_input.Position = 0;
            m_output.Position = 0;
        }

        protected void CleanupBase()
        {
            m_input.Dispose();
            m_output.Dispose();
        }

        protected void decodeToStream()
        {
            var jpeg = new jpeg_decompress_struct(new LibJpegErrorHandler());
            startDecompression(jpeg);

            var buffer = new byte[jpeg.Output_width * jpeg.Output_components];

            var scanlines = new byte[1][];
            scanlines[0] = buffer;

            int height = jpeg.Output_height;
            for (int i = 0; i < height; i++)
            {
                if (jpeg.jpeg_read_scanlines(scanlines, 1) != 1)
                    throw new InvalidProgramException("Failed to decompress JPEG image data.");

                m_output.Write(buffer, 0, buffer.Length);
            }
        }

        private void startDecompression(jpeg_decompress_struct jpeg)
        {
            m_input.Position = 0;

            jpeg.Src = new VirtualStreamSourceManager(jpeg, m_input);
            if (jpeg.jpeg_read_header(true) != ReadResult.JPEG_HEADER_OK)
                throw new InvalidProgramException("Failed to decompress JPEG image data.");

            var colorspace = jpeg.Out_color_space;
            if (colorspace != J_COLOR_SPACE.JCS_GRAYSCALE &&
                colorspace != J_COLOR_SPACE.JCS_RGB &&
                colorspace != J_COLOR_SPACE.JCS_CMYK)
            {
                jpeg.Out_color_space = J_COLOR_SPACE.JCS_RGB;
            }

            if (!jpeg.jpeg_start_decompress())
                throw new InvalidProgramException("Failed to decompress JPEG image data.");
        }
    }
}
