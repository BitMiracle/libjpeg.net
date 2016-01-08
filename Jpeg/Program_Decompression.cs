using System;
using System.Diagnostics;
using System.IO;

using BitMiracle.LibJpeg.Classic;
using BitMiracle.cdJpeg;

namespace BitMiracle.Jpeg
{
    partial class Program
    {
        class DecompressOptions : Options
        {
            public IMAGE_FORMATS OutputFormat = IMAGE_FORMATS.FMT_BMP;

            public bool QuantizeColors = false;
            public int DesiredNumberOfColors = 256;

            public J_DCT_METHOD DCTMethod = JpegConstants.JDCT_DEFAULT;
            public J_DITHER_MODE DitherMode = J_DITHER_MODE.JDITHER_FS;

            public bool Debug = false;
            public bool Fast = false;
            public bool Grayscale = false;
            public bool NoSmooth = false;
            public bool OnePass = false;

            public bool Scaled = false;
            public int ScaleNumerator = 1;
            public int ScaleDenominator = 1;
        }

        private static void decompress(Stream input, DecompressOptions options, Stream output)
        {
            Debug.Assert(input != null);
            Debug.Assert(options != null);
            Debug.Assert(output != null);

            /* Initialize the JPEG decompression object with default error handling. */
            jpeg_decompress_struct cinfo = new jpeg_decompress_struct(new cd_jpeg_error_mgr());

            /* Insert custom marker processor for COM and APP12.
             * APP12 is used by some digital camera makers for textual info,
             * so we provide the ability to display it as text.
             * If you like, additional APPn marker types can be selected for display,
             * but don't try to override APP0 or APP14 this way (see libjpeg.doc).
             */
            cinfo.jpeg_set_marker_processor((int)JPEG_MARKER.COM, new jpeg_decompress_struct.jpeg_marker_parser_method(printTextMarker));
            cinfo.jpeg_set_marker_processor((int)JPEG_MARKER.APP0 + 12, printTextMarker);

            /* Specify data source for decompression */
            cinfo.jpeg_stdio_src(input);

            /* Read file header, set default decompression parameters */
            cinfo.jpeg_read_header(true);

            applyOptions(cinfo, options);

            /* Initialize the output module now to let it override any crucial
             * option settings (for instance, GIF wants to force color quantization).
             */
            djpeg_dest_struct dest_mgr = null;

            switch (options.OutputFormat)
            {
                case IMAGE_FORMATS.FMT_BMP:
                    dest_mgr = new bmp_dest_struct(cinfo, false);
                    break;
                case IMAGE_FORMATS.FMT_OS2:
                    dest_mgr = new bmp_dest_struct(cinfo, true);
                    break;
                default:
                    cinfo.ERREXIT((int)ADDON_MESSAGE_CODE.JERR_UNSUPPORTED_FORMAT);
                    break;
            }

            dest_mgr.output_file = output;

            /* Start decompressor */
            cinfo.jpeg_start_decompress();

            /* Write output file header */
            dest_mgr.start_output();

            /* Process data */
            while (cinfo.Output_scanline < cinfo.Output_height)
            {
                int num_scanlines = cinfo.jpeg_read_scanlines(dest_mgr.buffer, dest_mgr.buffer_height);
                dest_mgr.put_pixel_rows(num_scanlines);
            }

            /* Finish decompression and release memory.
             * I must do it in this order because output module has allocated memory
             * of lifespan JPOOL_IMAGE; it needs to finish before releasing memory.
             */
            dest_mgr.finish_output();
            cinfo.jpeg_finish_decompress();

            /* All done. */
            if (cinfo.Err.Num_warnings != 0)
                Console.WriteLine("Corrupt-data warning count is not zero");
        }

        /// <summary>
        /// Parse optional switches.
        /// Returns argv[] index of first file-name argument (== argc if none).
        /// Any file names with indexes <= last_file_arg_seen are ignored;
        /// they have presumably been processed in a previous iteration.
        /// (Pass 0 for last_file_arg_seen on the first or only iteration.)
        /// for_real is false on the first (dummy) pass; we may skip any expensive
        /// processing.
        /// </summary>
        private static DecompressOptions parseSwitchesForDecompression(string[] argv)
        {
            Debug.Assert(argv != null);
            if (argv.Length <= 1)
                return null;

            DecompressOptions result = new DecompressOptions();

            int lastFileArgSeen = -1;

            /* Scan command line options, adjust parameters */
            string arg;
            for (int argn = 1; argn < argv.Length; argn++)
            {
                arg = argv[argn];
                if (arg[0] != '-')
                {
                    /* Not a switch, must be a file name argument */
                    lastFileArgSeen = argn;
                    break;
                }

                arg = arg.Substring(1);

                if (cdjpeg_utils.keymatch(arg, "bmp", 1))
                {
                    result.OutputFormat = IMAGE_FORMATS.FMT_BMP;
                }
                else if (cdjpeg_utils.keymatch(arg, "colors", 1) ||
                         cdjpeg_utils.keymatch(arg, "colours", 1) ||
                         cdjpeg_utils.keymatch(arg, "quantize", 1) ||
                         cdjpeg_utils.keymatch(arg, "quantise", 1))
                {
                    /* Do color quantization. */

                    if (++argn >= argv.Length) /* advance to next argument */
                        return null;

                    try
                    {
                        result.QuantizeColors = true;
                        result.DesiredNumberOfColors = int.Parse(argv[argn]);
                    }
                    catch (Exception e)
                    {
                        Console.WriteLine(e.Message);
                        return null;
                    }
                }
                else if (cdjpeg_utils.keymatch(arg, "dct", 2))
                {
                    /* Select IDCT algorithm. */
                    if (++argn >= argv.Length) /* advance to next argument */
                        return null;

                    if (cdjpeg_utils.keymatch(argv[argn], "int", 1))
                        result.DCTMethod = J_DCT_METHOD.JDCT_ISLOW;
                    else if (cdjpeg_utils.keymatch(argv[argn], "fast", 2))
                        result.DCTMethod = J_DCT_METHOD.JDCT_IFAST;
                    else if (cdjpeg_utils.keymatch(argv[argn], "float", 2))
                        result.DCTMethod = J_DCT_METHOD.JDCT_FLOAT;
                    else
                        return null;
                }
                else if (cdjpeg_utils.keymatch(arg, "dither", 2))
                {
                    /* Select dithering algorithm. */
                    if (++argn >= argv.Length) /* advance to next argument */
                        return null;

                    if (cdjpeg_utils.keymatch(argv[argn], "fs", 2))
                        result.DitherMode = J_DITHER_MODE.JDITHER_FS;
                    else if (cdjpeg_utils.keymatch(argv[argn], "none", 2))
                        result.DitherMode = J_DITHER_MODE.JDITHER_NONE;
                    else if (cdjpeg_utils.keymatch(argv[argn], "ordered", 2))
                        result.DitherMode = J_DITHER_MODE.JDITHER_ORDERED;
                    else
                        return null;
                }
                else if (cdjpeg_utils.keymatch(arg, "debug", 1) || cdjpeg_utils.keymatch(arg, "verbose", 1))
                {
                    /* Enable debug printouts. */
                    result.Debug = true;

                    /* On first -d, print version identification */
                    if (!m_printedVersion)
                    {
                        Console.Write(string.Format("Bit Miracle's DJPEG, version {0}\n{1}\n", jpeg_common_struct.Version, jpeg_common_struct.Copyright));
                        m_printedVersion = true;
                    }
                }
                else if (cdjpeg_utils.keymatch(arg, "fast", 1))
                {
                    result.Fast = true;
                }
                else if (cdjpeg_utils.keymatch(arg, "grayscale", 2) || cdjpeg_utils.keymatch(arg, "greyscale", 2))
                {
                    /* Force monochrome output. */
                    result.Grayscale = true;
                }
                else if (cdjpeg_utils.keymatch(arg, "nosmooth", 3))
                {
                    /* Suppress fancy upsampling */
                    result.NoSmooth = true;
                }
                else if (cdjpeg_utils.keymatch(arg, "onepass", 3))
                {
                    /* Use fast one-pass quantization. */
                    result.OnePass = true;
                }
                else if (cdjpeg_utils.keymatch(arg, "os2", 3))
                {
                    /* BMP output format (OS/2 flavor). */
                    result.OutputFormat = IMAGE_FORMATS.FMT_OS2;
                }
                else if (cdjpeg_utils.keymatch(arg, "outfile", 4))
                {
                    /* Set output file name. */
                    if (++argn >= argv.Length) /* advance to next argument */
                        return null;

                    result.OutputFileName = argv[argn];   /* save it away for later use */
                }
                else if (cdjpeg_utils.keymatch(arg, "scale", 1))
                {
                    /* Scale the output image by a fraction M/N. */
                    if (++argn >= argv.Length) /* advance to next argument */
                        return null;

                    int slashPos = argv[argn].IndexOf('/');
                    if (slashPos == -1)
                        return null;

                    try
                    {
                        string num = argv[argn].Substring(0, slashPos);
                        string denom = argv[argn].Substring(slashPos + 1);
                        result.Scaled = true;
                        result.ScaleNumerator = int.Parse(num);
                        result.ScaleDenominator = int.Parse(denom);
                    }
                    catch (Exception e)
                    {
                        Console.WriteLine(e.Message);
                        return null;
                    }
                }
                else /* bogus switch */
                    return null;
            }

            /* Must have either -outfile switch or explicit output file name */
            if (result.OutputFileName.Length == 0)
            {
                // file_index should point to input file 
                if (lastFileArgSeen != argv.Length - 2)
                {
                    Console.WriteLine(string.Format("{0}: must name one input and one output file.", m_programName));
                    return null;
                }

                // output file comes right after input one
                result.InputFileName = argv[lastFileArgSeen];
                result.OutputFileName = argv[lastFileArgSeen + 1];
            }
            else
            {
                // file_index should point to input file
                if (lastFileArgSeen != argv.Length - 1)
                {
                    Console.WriteLine(string.Format("{0}: must name one input and one output file.", m_programName));
                    return null;
                }

                result.InputFileName = argv[lastFileArgSeen];
            }

            return result;
        }

        static void applyOptions(jpeg_decompress_struct decompressor, DecompressOptions options)
        {
            Debug.Assert(decompressor != null);
            Debug.Assert(options != null);

            if (options.QuantizeColors)
            {
                decompressor.Quantize_colors = true;
                decompressor.Desired_number_of_colors = options.DesiredNumberOfColors;
            }

            decompressor.Dct_method = options.DCTMethod;
            decompressor.Dither_mode = options.DitherMode;

            if (options.Debug)
                decompressor.Err.Trace_level = 1;

            if (options.Fast)
            {
                /* Select recommended processing options for quick-and-dirty output. */
                decompressor.Two_pass_quantize = false;
                decompressor.Dither_mode = J_DITHER_MODE.JDITHER_ORDERED;
                if (!decompressor.Quantize_colors) /* don't override an earlier -colors */
                    decompressor.Desired_number_of_colors = 216;
                decompressor.Dct_method = JpegConstants.JDCT_FASTEST;
                decompressor.Do_fancy_upsampling = false;
            }

            if (options.Grayscale)
                decompressor.Out_color_space = J_COLOR_SPACE.JCS_GRAYSCALE;

            if (options.NoSmooth)
                decompressor.Do_fancy_upsampling = false;

            if (options.OnePass)
                decompressor.Two_pass_quantize = false;

            if (options.Scaled)
            {
                decompressor.Scale_num = options.ScaleNumerator;
                decompressor.Scale_denom = options.ScaleDenominator;
            }
        }

        /// <summary>
        /// Marker processor for COM and interesting APPn markers.
        /// This replaces the library's built-in processor, which just skips the marker.
        /// We want to print out the marker as text, to the extent possible.
        /// Note this code relies on a non-suspending data source.
        /// </summary>
        static bool printTextMarker(jpeg_decompress_struct cinfo)
        {
            bool traceit = (cinfo.Err.Trace_level >= 1);

            int length = jpeg_getc(cinfo) << 8;
            length += jpeg_getc(cinfo);
            length -= 2;            /* discount the length word itself */

            if (traceit)
            {
                if (cinfo.Unread_marker == (int)JPEG_MARKER.COM)
                {
                    Console.WriteLine("Comment, length {0}:", length);
                }
                else
                {
                    /* assume it is an APPn otherwise */
                    Console.WriteLine("APP{0}, length {1}:", cinfo.Unread_marker - JPEG_MARKER.APP0, length);
                }
            }

            int lastch = 0;
            while (--length >= 0)
            {
                int ch = jpeg_getc(cinfo);
                if (traceit)
                {
                    /* Emit the character in a readable form.
                     * Nonprintables are converted to \nnn form,
                     * while \ is converted to \\.
                     * Newlines in CR, CR/LF, or LF form will be printed as one newline.
                     */
                    if (ch == '\r')
                    {
                        Console.WriteLine();
                    }
                    else if (ch == '\n')
                    {
                        if (lastch != '\r')
                            Console.WriteLine();
                    }
                    else if (ch == '\\')
                    {
                        Console.Write("\\\\");
                    }
                    else if (!Char.IsControl((char)ch))
                    {
                        Console.Write(ch);
                    }
                    else
                    {
                        Console.Write(encodeOctalString(ch));
                    }

                    lastch = ch;
                }
            }

            if (traceit)
                Console.WriteLine();

            return true;
        }

        /// <summary>
        /// Read next byte
        /// </summary>
        static int jpeg_getc(jpeg_decompress_struct decompressor)
        {
            int v;
            if (!decompressor.Src.GetByte(out v))
                decompressor.ERREXIT(J_MESSAGE_CODE.JERR_CANT_SUSPEND);

            return v;
        }

        private static string encodeOctalString(int value)
        {
            //return octal encoding \ddd of the character value. 
            return string.Format(
                @"\{0}{1}{2}",
                ((value >> 6) & 7),
                ((value >> 3) & 7),
                (value & 7)
            );
        }
    }
}
