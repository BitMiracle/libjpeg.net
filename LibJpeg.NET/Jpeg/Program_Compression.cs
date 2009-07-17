using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.IO;
using System.Text;

using LibJpeg.Classic;
using cdJpeg;

namespace Jpeg
{
    partial class Program
    {
        private static void compress(Stream input, CompressOptions options, Stream output)
        {
            Debug.Assert(input != null);
            Debug.Assert(options != null);
            Debug.Assert(output != null);

            jpeg_compress_struct cinfo = new jpeg_compress_struct(new cd_jpeg_error_mgr());

            /* Initialize JPEG parameters.
             * Much of this may be overridden later.
             * In particular, we don't yet know the input file's color space,
             * but we need to provide some value for jpeg_set_defaults() to work.
             */
            cinfo.In_color_space = J_COLOR_SPACE.JCS_RGB; /* arbitrary guess */
            cinfo.jpeg_set_defaults();

            /* Figure out the input file format, and set up to read it. */
            cjpeg_source_struct src_mgr = new bmp_source_struct(cinfo);
            src_mgr.input_file = input;

            /* Read the input file header to obtain file size & colorspace. */
            src_mgr.start_input();

            /* Now that we know input colorspace, fix colorspace-dependent defaults */
            cinfo.jpeg_default_colorspace();

            /* Adjust default compression parameters */
            if (!applyOptions(cinfo, options))
                return;

            /* Specify data destination for compression */
            cinfo.jpeg_stdio_dest(output);

            /* Start compressor */
            cinfo.jpeg_start_compress(true);

            /* Process data */
            while (cinfo.Next_scanline < cinfo.Image_height)
            {
                int num_scanlines = src_mgr.get_pixel_rows();
                cinfo.jpeg_write_scanlines(src_mgr.buffer, num_scanlines);
            }

            /* Finish compression and release memory */
            src_mgr.finish_input();
            cinfo.jpeg_finish_compress();

            /* All done. */
            if (cinfo.Err.Num_warnings != 0)
                Console.WriteLine("Corrupt-data warning count is not zero");
        }

        /// <summary>
        /// Parse optional switches.
        /// Returns true if switches were parsed successfully; false otherwise.
        /// fileIndex receives index of first file-name argument (== -1 if none).
        /// for_real is false on the first (dummy) pass; we may skip any expensive
        /// processing.
        /// </summary>
        private static CompressOptions parseSwitchesForCompression(string[] argv)
        {
            Debug.Assert(argv != null);
            if (argv.Length <= 1)
            {
                usageForCompression();
                return null;
            }

            CompressOptions options = new CompressOptions();

            /* Scan command line options, adjust parameters */
            int lastFileArgSeen = -1;
            for (int argn = 1; argn < argv.Length; argn++)
            {
                string arg = argv[argn];
                if (arg[0] != '-')
                {
                    /* Not a switch, must be a file name argument */
                    lastFileArgSeen = argn;
                    break;
                }

                arg = arg.Substring(1);

                if (cdjpeg_utils.keymatch(arg, "baseline", 1))
                {
                    /* Force baseline-compatible output (8-bit quantizer values). */
                    options.ForceBaseline = true;
                }
                else if (cdjpeg_utils.keymatch(arg, "dct", 2))
                {
                    /* Select DCT algorithm. */
                    argn++; /* advance to next argument */
                    if (argn >= argv.Length)
                    {
                        usageForCompression();
                        return null;
                    }

                    if (cdjpeg_utils.keymatch(argv[argn], "int", 1))
                        options.DCTMethod = J_DCT_METHOD.JDCT_ISLOW;
                    else if (cdjpeg_utils.keymatch(argv[argn], "fast", 2))
                        options.DCTMethod = J_DCT_METHOD.JDCT_IFAST;
                    else if (cdjpeg_utils.keymatch(argv[argn], "float", 2))
                        options.DCTMethod = J_DCT_METHOD.JDCT_FLOAT;
                    else
                    {
                        usageForCompression();
                        return null;
                    }
                }
                else if (cdjpeg_utils.keymatch(arg, "debug", 1) || cdjpeg_utils.keymatch(arg, "verbose", 1))
                {
                    /* Enable debug printouts. */
                    options.Debug = true;

                    /* On first -d, print version identification */
                    if (!m_printedVersion)
                    {
                        Console.Write(string.Format("Bit Miracle's CJPEG, version {0}\n{1}\n", jpeg_common_struct.Version, jpeg_common_struct.Copyright));
                        m_printedVersion = true;
                    }
                }
                else if (cdjpeg_utils.keymatch(arg, "grayscale", 2) || cdjpeg_utils.keymatch(arg, "greyscale", 2))
                {
                    /* Force a monochrome JPEG file to be generated. */
                    options.Grayscale = true;
                }
                else if (cdjpeg_utils.keymatch(arg, "optimize", 1) || cdjpeg_utils.keymatch(arg, "optimise", 1))
                {
                    /* Enable entropy parm optimization. */
                    options.Optimize = true;
                }
                else if (cdjpeg_utils.keymatch(arg, "outfile", 4))
                {
                    /* Set output file name. */
                    argn++;/* advance to next argument */
                    if (argn >= argv.Length)
                    {
                        usageForCompression();
                        return null;
                    }

                    options.OutputFileName = argv[argn];
                }
                else if (cdjpeg_utils.keymatch(arg, "progressive", 1))
                {
                    /* Select simple progressive mode. */
                    options.Progressive = true;
                    /* We must postpone execution until num_components is known. */
                }
                else if (cdjpeg_utils.keymatch(arg, "quality", 1))
                {
                    /* Quality factor (quantization table scaling factor). */
                    argn++;/* advance to next argument */
                    if (argn >= argv.Length)
                    {
                        usageForCompression();
                        return null;
                    }

                    try
                    {
                        options.Quality = int.Parse(argv[argn]);
                    }
                    catch (Exception e)
                    {
                        Console.WriteLine(e.Message);
                        usageForCompression();
                        return null;
                    }
                }
                else if (cdjpeg_utils.keymatch(arg, "qslots", 2))
                {
                    /* Quantization table slot numbers. */
                    argn++; /* advance to next argument */
                    if (argn >= argv.Length)
                    {
                        usageForCompression();
                        return null;
                    }

                    options.Qslots = argv[argn];
                    /* Must delay setting qslots until after we have processed any
                     * colorspace-determining switches, since jpeg_set_colorspace sets
                     * default quant table numbers.
                     */
                }
                else if (cdjpeg_utils.keymatch(arg, "qtables", 2))
                {
                    /* Quantization tables fetched from file. */
                    argn++; /* advance to next argument */
                    if (argn >= argv.Length)
                    {
                        usageForCompression();
                        return null;
                    }

                    options.Qtables = argv[argn];
                    /* We postpone actually reading the file in case -quality comes later. */
                }
                else if (cdjpeg_utils.keymatch(arg, "restart", 1))
                {
                    /* Restart interval in MCU rows (or in MCUs with 'b'). */
                    argn++; /* advance to next argument */

                    if (argn >= argv.Length)
                    {
                        usageForCompression();
                        return null;
                    }

                    bool inBlocks = false;
                    if (argv[argn].EndsWith("b") || argv[argn].EndsWith("B"))
                        inBlocks = true;

                    string parsee = argv[argn];
                    if (inBlocks)
                        parsee = parsee.Remove(parsee.Length - 1);

                    int val;
                    try
                    {
                        val = int.Parse(parsee);
                    }
                    catch (Exception e)
                    {
                        Console.WriteLine(e.Message);
                        usageForCompression();
                        return null;
                    }

                    if (val < 0 || val > 65535)
                    {
                        usageForCompression();
                        return null;
                    }

                    if (inBlocks)
                    {
                        options.RestartInterval = val;
                        options.RestartInRows = 0; /* else prior '-restart n' overrides me */
                    }
                    else
                    {
                        options.RestartInRows = val;
                        /* restart_interval will be computed during startup */
                    }
                }
                else if (cdjpeg_utils.keymatch(arg, "sample", 2))
                {
                    /* Set sampling factors. */
                    argn++; /* advance to next argument */
                    if (argn >= argv.Length)
                    {
                        usageForCompression();
                        return null;
                    }

                    options.Sample = argv[argn];
                    /* Must delay setting sample factors until after we have processed any
                     * colorspace-determining switches, since jpeg_set_colorspace sets
                     * default sampling factors.
                     */
                }
                else if (cdjpeg_utils.keymatch(arg, "smooth", 2))
                {
                    /* Set input smoothing factor. */

                    argn++; /* advance to next argument */
                    if (argn >= argv.Length)
                    {
                        usageForCompression();
                        return null;
                    }

                    int val;
                    try
                    {
                        val = int.Parse(argv[argn]);
                    }
                    catch (Exception e)
                    {
                        Console.WriteLine(e.Message);
                        usageForCompression();
                        return null;
                    }

                    if (val < 0 || val > 100)
                    {
                        usageForCompression();
                        return null;
                    }

                    options.SmoothingFactor = val;
                }
                else
                {
                    usageForCompression(); /* bogus switch */
                    return null;
                }
            }

            /* Must have either -outfile switch or explicit output file name */
            if (options.OutputFileName.Length == 0)
            {
                // file_index should point to input file 
                if (lastFileArgSeen != argv.Length - 2)
                {
                    Console.WriteLine(string.Format("{0}: must name one input and one output file.", m_programName));
                    usageForCompression();
                    return null;
                }

                // output file comes right after input one
                options.InputFileName = argv[lastFileArgSeen];
                options.OutputFileName = argv[lastFileArgSeen + 1];
            }
            else
            {
                // file_index should point to input file
                if (lastFileArgSeen != argv.Length - 1)
                {
                    Console.WriteLine(string.Format("{0}: must name one input and one output file.", m_programName));
                    usageForCompression();
                    return null;
                }

                options.InputFileName = argv[lastFileArgSeen];
            }

            return options;
        }

        private static bool applyOptions(jpeg_compress_struct compressor, CompressOptions options)
        {
            compressor.jpeg_set_quality(options.Quality, options.ForceBaseline);
            compressor.Dct_method = options.DCTMethod;

            if (options.Debug)
                compressor.Err.Trace_level = 1;

            if (options.Grayscale)
                compressor.jpeg_set_colorspace(J_COLOR_SPACE.JCS_GRAYSCALE);

            if (options.Optimize)
                compressor.Optimize_coding = true;

            compressor.Restart_interval = options.RestartInterval;
            compressor.Restart_in_rows = options.RestartInRows;

            compressor.Smoothing_factor = options.SmoothingFactor;

            int q_scale_factor = 100;
            if (options.Quality != 75)
                q_scale_factor = jpeg_compress_struct.jpeg_quality_scaling(options.Quality);

            /* Set quantization tables for selected quality. */
            /* Some or all may be overridden if -qtables is present. */
            if (options.Qtables != "") /* process -qtables if it was present */
            {
                if (!read_quant_tables(compressor, options.Qtables, q_scale_factor, options.ForceBaseline))
                {
                    usageForCompression();
                    return false;
                }
            }

            if (options.Qslots != "")  /* process -qslots if it was present */
            {
                if (!set_quant_slots(compressor, options.Qslots))
                {
                    usageForCompression();
                    return false;
                }
            }

            if (options.Sample != "")  /* process -sample if it was present */
            {
                if (!set_sample_factors(compressor, options.Sample))
                {
                    usageForCompression();
                    return false;
                }
            }

            if (options.Progressive) /* process -progressive; -scans can override */
                compressor.jpeg_simple_progression();

            return true;
        }

        static bool read_quant_tables(jpeg_compress_struct cinfo, string filename, int scale_factor, bool force_baseline)
        {
            // not implemented yet
            return false;
        }

        static bool set_quant_slots(jpeg_compress_struct cinfo, string arg)
        {
            // not implemented yet
            return false;
        }

        static bool set_sample_factors(jpeg_compress_struct cinfo, string arg)
        {
            // not implemented yet
            return false;
        }
    }
}
