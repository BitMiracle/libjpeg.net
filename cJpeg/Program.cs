/*
 * This file contains a command-line user interface for the JPEG compressor.
 *
 * To simplify script writing, the "-outfile" switch is provided.  The syntax
 *  cjpeg [options]  -outfile outputfile  inputfile
 * works regardless of which command line style is used.
 */

using System;
using System.IO;

using BitMiracle.LibJpeg.Classic;
using BitMiracle.cdJpeg;

namespace BitMiracle.cJpeg
{
    public class Program
    {
        static bool printed_version = false;
        static string progname;    /* program name for error messages */
        static string outfilename;   /* for -outfile switch */

        public static void Main(string[] args)
        {
            progname = Path.GetFileName(Environment.GetCommandLineArgs()[0]);

            cd_jpeg_error_mgr err = new cd_jpeg_error_mgr();
            jpeg_compress_struct cinfo = new jpeg_compress_struct(err);

            /* Initialize JPEG parameters.
             * Much of this may be overridden later.
             * In particular, we don't yet know the input file's color space,
             * but we need to provide some value for jpeg_set_defaults() to work.
             */

            cinfo.In_color_space = J_COLOR_SPACE.JCS_RGB; /* arbitrary guess */
            cinfo.jpeg_set_defaults();

            /* Scan command line to find file names.
             * It is convenient to use just one switch-parsing routine, but the switch
             * values read here are ignored; we will rescan the switches after opening
             * the input file.
             */
            int file_index;
            if (!parse_switches(cinfo, args, false, out file_index))
            {
                usage();
                return;
            }

            /* Must have either -outfile switch or explicit output file name */
            if (outfilename == null)
            {
                // file_index should point to input file 
                if (file_index != args.Length - 2)
                {
                    Console.WriteLine(string.Format("{0}: must name one input and one output file.", progname));
                    usage();
                    return;
                }

                // output file comes right after input one
                outfilename = args[file_index + 1];
            }
            else
            {
                // file_index should point to input file
                if (file_index != args.Length - 1)
                {
                    Console.WriteLine(string.Format("{0}: must name one input and one output file.", progname));
                    usage();
                    return;
                }
            }

            /* Open the input file. */
            FileStream input_file = null;
            if (file_index < args.Length)
            {
                try
                {
                    input_file = new FileStream(args[file_index], FileMode.Open);
                }
                catch (Exception e)
                {
                    Console.WriteLine(string.Format("{0}: can't open {1}", progname, args[file_index]));
                    Console.WriteLine(e.Message);
                    return;
                }
            }
            else
            {
                Console.WriteLine(string.Format("{0}: sorry, can't read file from console"));
                return;
            }

            /* Open the output file. */
            FileStream output_file = null;
            if (outfilename != null)
            {
                try
                {
                    output_file = new FileStream(outfilename, FileMode.Create);
                }
                catch (Exception e)
                {
                    Console.WriteLine(string.Format("{0}: can't open {1}", progname, args[file_index]));
                    Console.WriteLine(e.Message);
                    return;
                }
            }
            else
            {
                Console.WriteLine(string.Format("{0}: sorry, can't write file to console"));
                return;
            }

            /* Figure out the input file format, and set up to read it. */
            cjpeg_source_struct src_mgr = new bmp_source_struct(cinfo);
            src_mgr.input_file = input_file;

            /* Read the input file header to obtain file size & colorspace. */
            src_mgr.start_input();

            /* Now that we know input colorspace, fix colorspace-dependent defaults */
            cinfo.jpeg_default_colorspace();

            /* Adjust default compression parameters by re-parsing the options */
            parse_switches(cinfo, args, true, out file_index);

            /* Specify data destination for compression */
            cinfo.jpeg_stdio_dest(output_file);

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

            /* Close files, if we opened them */
            input_file.Close();
            input_file.Dispose();

            output_file.Close();
            output_file.Dispose();

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
        static bool parse_switches(jpeg_compress_struct cinfo, string[] argv, bool for_real, out int fileIndex)
        {
            /* Set up default JPEG parameters. */
            bool force_baseline = false; /* by default, allow 16-bit quantizers */
            bool simple_progressive = false;

            string qualityarg = null;	/* saves -quality parm if any */
            string qtablefile = null;    /* saves -qtables filename if any */
            string qslotsarg = null; /* saves -qslots parm if any */
            string samplearg = null; /* saves -sample parm if any */

            outfilename = null;
            fileIndex = -1;
            cinfo.Err.Trace_level = 0;

            /* Scan command line options, adjust parameters */
            int argn = 0;
            for (; argn < argv.Length; argn++)
            {
                string arg = argv[argn];
                if (string.IsNullOrEmpty(arg) || arg[0] != '-')
                {
                    /* Not a switch, must be a file name argument */
                    fileIndex = argn;
                    break;
                }

                arg = arg.Substring(1);

                if (cdjpeg_utils.keymatch(arg, "baseline", 2))
                {
                    /* Force baseline-compatible output (8-bit quantizer values). */
                    force_baseline = true;
                }
                else if (cdjpeg_utils.keymatch(arg, "block", 2))
                {
                    /* Set DCT block size. */
                    argn++; /* advance to next argument */
                    if (argn >= argv.Length)
                        return false;

                    int val;
                    if (!int.TryParse(argv[argn], out val))
                        return false;

                    if (val < 1 || val > 16)
                        return false;

                    cinfo.block_size = val;
                }
                else if (cdjpeg_utils.keymatch(arg, "dct", 2))
                {
                    /* Select DCT algorithm. */
                    argn++; /* advance to next argument */
                    if (argn >= argv.Length)
                        return false;

                    if (cdjpeg_utils.keymatch(argv[argn], "int", 1))
                        cinfo.Dct_method = J_DCT_METHOD.JDCT_ISLOW;
                    else if (cdjpeg_utils.keymatch(argv[argn], "fast", 2))
                        cinfo.Dct_method = J_DCT_METHOD.JDCT_IFAST;
                    else if (cdjpeg_utils.keymatch(argv[argn], "float", 2))
                        cinfo.Dct_method = J_DCT_METHOD.JDCT_FLOAT;
                    else
                        return false;
                }
                else if (cdjpeg_utils.keymatch(arg, "debug", 1) || cdjpeg_utils.keymatch(arg, "verbose", 1))
                {
                    /* Enable debug printouts. */
                    /* On first -d, print version identification */
                    if (!printed_version)
                    {
                        Console.Write(string.Format("Bit Miracle's CJPEG, version {0}\n{1}\n", jpeg_common_struct.Version, jpeg_common_struct.Copyright));
                        printed_version = true;
                    }
                    cinfo.Err.Trace_level++;
                }
                else if (cdjpeg_utils.keymatch(arg, "grayscale", 2) || cdjpeg_utils.keymatch(arg, "greyscale", 2))
                {
                    /* Force a monochrome JPEG file to be generated. */
                    cinfo.jpeg_set_colorspace(J_COLOR_SPACE.JCS_GRAYSCALE);
                }
                else if (cdjpeg_utils.keymatch(arg, "rgb", 3) || cdjpeg_utils.keymatch(arg, "rgb1", 4))
                {
                    /* Force an RGB JPEG file to be generated. */
                    /* Note: Entropy table assignment in Jpeg_color_space depends
                     * on color_transform.
                     */
                    cinfo.color_transform = (arg == "rgb") ? J_COLOR_TRANSFORM.JCT_SUBTRACT_GREEN : J_COLOR_TRANSFORM.JCT_NONE;
                    cinfo.Jpeg_color_space = J_COLOR_SPACE.JCS_RGB;
                }
                else if (cdjpeg_utils.keymatch(arg, "bgycc", 5))
                {
                    /* Force a big gamut YCC JPEG file to be generated. */
                    cinfo.Jpeg_color_space = J_COLOR_SPACE.JCS_BG_YCC;
                }
                else if (cdjpeg_utils.keymatch(arg, "optimize", 1) || cdjpeg_utils.keymatch(arg, "optimise", 1))
                {
                    /* Enable entropy parm optimization. */
                    cinfo.Optimize_coding = true;
                }
                else if (cdjpeg_utils.keymatch(arg, "nosmooth", 3))
                {
                    /* Suppress fancy downsampling. */
                    cinfo.do_fancy_downsampling = false;
                }
                else if (cdjpeg_utils.keymatch(arg, "outfile", 4))
                {
                    /* Set output file name. */
                    argn++;/* advance to next argument */
                    if (argn >= argv.Length)
                        return false;

                    outfilename = argv[argn];   /* save it away for later use */
                }
                else if (cdjpeg_utils.keymatch(arg, "progressive", 1))
                {
                    /* Select simple progressive mode. */
                    simple_progressive = true;
                    /* We must postpone execution until num_components is known. */
                }
                else if (cdjpeg_utils.keymatch(arg, "quality", 1))
                {
                    /* Quality ratings (quantization table scaling factors). */
                    argn++;/* advance to next argument */
                    if (argn >= argv.Length)
                        return false;

                    qualityarg = argv[argn];
                }
                else if (cdjpeg_utils.keymatch(arg, "qslots", 2))
                {
                    /* Quantization table slot numbers. */
                    argn++; /* advance to next argument */
                    if (argn >= argv.Length)
                        return false;

                    qslotsarg = argv[argn];
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
                        return false;

                    qtablefile = argv[argn];
                    /* We postpone actually reading the file in case -quality comes later. */
                }
                else if (cdjpeg_utils.keymatch(arg, "restart", 1))
                {
                    /* Restart interval in MCU rows (or in MCUs with 'b'). */
                    argn++; /* advance to next argument */

                    if (argn >= argv.Length)
                        return false;

                    bool inBlocks = false;
                    if (argv[argn].EndsWith("b") || argv[argn].EndsWith("B"))
                        inBlocks = true;

                    string parsee = argv[argn];
                    if (inBlocks)
                        parsee = parsee.Remove(parsee.Length - 1);

                    try
                    {
                        int val = int.Parse(parsee);
                        if (val < 0 || val > 65535)
                            return false;

                        if (inBlocks)
                        {
                            cinfo.Restart_interval = val;
                            cinfo.Restart_in_rows = 0; /* else prior '-restart n' overrides me */
                        }
                        else
                        {
                            cinfo.Restart_in_rows = val;
                            /* restart_interval will be computed during startup */
                        }
                    }
                    catch (Exception e)
                    {
                        Console.WriteLine(e.Message);
                        return false;
                    }
                }
                else if (cdjpeg_utils.keymatch(arg, "sample", 2))
                {
                    /* Set sampling factors. */
                    argn++; /* advance to next argument */
                    if (argn >= argv.Length)
                        return false;

                    samplearg = argv[argn];
                    /* Must delay setting sample factors until after we have processed any
                     * colorspace-determining switches, since jpeg_set_colorspace sets
                     * default sampling factors.
                     */
                }
                else if (cdjpeg_utils.keymatch(arg, "scale", 4))
                {
                    /* Scale the image by a fraction M/N. */
                    argn++; /* advance to next argument */
                    if (argn >= argv.Length)
                        return false;

                    string[] parts = argv[argn].Split(',');
                    if (parts.Length != 2)
                        return false;

                    if (!int.TryParse(parts[0], out cinfo.scale_num))
                        return false;

                    if (!int.TryParse(parts[1], out cinfo.scale_denom))
                        return false;
                }
                else if (cdjpeg_utils.keymatch(arg, "smooth", 2))
                {
                    /* Set input smoothing factor. */

                    argn++; /* advance to next argument */
                    if (argn >= argv.Length)
                        return false;

                    try
                    {
                        int val = int.Parse(argv[argn]);
                        if (val < 0 || val > 100)
                            return false;

                        cinfo.Smoothing_factor = val;
                    }
                    catch (Exception e)
                    {
                        Console.WriteLine(e.Message);
                        return false;
                    }
                }
                else
                {
                    /* bogus switch */
                    return false;
                }
            }

            /* Post-switch-scanning cleanup */

            if (for_real)
            {
                /* Set quantization tables for selected quality. */
                /* Some or all may be overridden if -qtables is present. */
                if (qualityarg != null)
                {
                    if (!set_quality_ratings(cinfo, qualityarg, force_baseline))
                        return false;
                }

                if (qtablefile != null) /* process -qtables if it was present */
                {
                    if (!read_quant_tables(cinfo, qtablefile, force_baseline))
                        return false;
                }

                if (qslotsarg != null)  /* process -qslots if it was present */
                {
                    if (!set_quant_slots(cinfo, qslotsarg))
                        return false;
                }

                if (samplearg != null)  /* process -sample if it was present */
                {
                    if (!set_sample_factors(cinfo, samplearg))
                        return false;
                }

                if (simple_progressive) /* process -progressive; -scans can override */
                    cinfo.jpeg_simple_progression();
            }

            return true;
        }

        /// <summary>
        /// complain about bad command line
        /// </summary>
        static void usage()
        {
            Console.WriteLine(string.Format("usage: {0} [switches] inputfile outputfile", progname));

            Console.WriteLine("Switches (names may be abbreviated):");
            Console.WriteLine("  -quality N[,...]     Compression quality (0..100; 5-95 is useful range)");
            Console.WriteLine("  -grayscale     Create monochrome JPEG file");
            Console.WriteLine("  -rgb           Create RGB JPEG file");
            Console.WriteLine("  -optimize      Optimize Huffman table (smaller file, but slow compression)");
            Console.WriteLine("  -progressive   Create progressive JPEG file");
            Console.WriteLine("  -scale M/N     Scale image by fraction M/N, eg, 1/2");
            Console.WriteLine("Switches for advanced users:");
            Console.WriteLine("  -arithmetic    Use arithmetic coding");
            Console.WriteLine("  -block N       DCT block size (1..16; default is 8)");
            Console.WriteLine("  -rgb1          Create RGB JPEG file with reversible color transform");
            Console.WriteLine("  -bgycc         Create big gamut YCC JPEG file");
            Console.WriteLine(string.Format("  -dct int       Use integer DCT method {0}", (JpegConstants.JDCT_DEFAULT == J_DCT_METHOD.JDCT_ISLOW ? " (default)" : "")));
            Console.WriteLine(string.Format("  -dct fast      Use fast integer DCT (less accurate) {0}", (JpegConstants.JDCT_DEFAULT == J_DCT_METHOD.JDCT_IFAST ? " (default)" : "")));
            Console.WriteLine(string.Format("  -dct float     Use floating-point DCT method {0}", (JpegConstants.JDCT_DEFAULT == J_DCT_METHOD.JDCT_FLOAT ? " (default)" : "")));
            Console.WriteLine("  -nosmooth      Don't use high-quality downsampling");
            Console.WriteLine("  -restart N     Set restart interval in rows, or in blocks with B");
            Console.WriteLine("  -smooth N      Smooth dithered input (N=1..100 is strength)");
            Console.WriteLine("  -outfile name  Specify name for output file");
            Console.WriteLine("  -verbose  or  -debug   Emit debug output");
            Console.WriteLine("Switches for wizards:");
            Console.WriteLine("  -baseline      Force baseline quantization tables");
            Console.WriteLine("  -qtables file  Use quantization tables given in file");
            Console.WriteLine("  -qslots N[,...]    Set component quantization tables");
            Console.WriteLine("  -sample HxV[,...]  Set component sampling factors");
        }

        static bool read_quant_tables(jpeg_compress_struct cinfo, string filename, bool force_baseline)
        {
            // not implemented yet
            return false;
        }

        static bool set_quality_ratings(jpeg_compress_struct cinfo, string arg, bool force_baseline)
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
