/*
 * This file contains a command-line user interface for JPEG transcoding.
 * 
 * It is very similar to cJpeg, and partly to dJpeg, but provides
 * lossless transcoding between different JPEG file formats.  It also
 * provides some lossless and sort-of-lossless transformations of JPEG data.
 */

using System;
using System.IO;

using BitMiracle.cdJpeg;
using BitMiracle.LibJpeg.Classic;

namespace BitMiracle.JpegTran
{
    [System.Diagnostics.CodeAnalysis.SuppressMessage("Style", "IDE1006:Naming Styles")]
    public class Program
    {
        static string progName;

        static bool printed_version;

        /* image transformation options */
        static jpeg_transform_info transformoption = new jpeg_transform_info();

        static string outfilename; /* for -outfile switch */
        static string dropfilename;	/* for -drop switch */
        static string scaleoption; /* -scale switch */

        static JCOPY_OPTION copyoption;	/* -copy switch */

        static void Main(string[] args)
        {
            progName = Path.GetFileName(Environment.GetCommandLineArgs()[0]);

            // Initialize the JPEG decompression object with default error handling.
            var srcinfo = new jpeg_decompress_struct();

            // Initialize the JPEG compression object with default error handling.
            var dstinfo = new jpeg_compress_struct();

            /* Scan command line to find file names.
             * It is convenient to use just one switch-parsing routine, but the switch
             * values read here are mostly ignored; we will rescan the switches after
             * opening the input file.  Also note that most of the switches affect the
             * destination JPEG object, so we parse into that and then copy over what
             * needs to affect the source too.
             */
            if (!parse_switches(dstinfo, args, false, out int file_index))
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
                    Console.WriteLine($"{progName}: must name one input and one output file.");
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
                    Console.WriteLine($"{progName}: must name one input and one output file.");
                    usage();
                    return;
                }
            }

            /* Open the input file. */
            if (file_index >= args.Length)
            {
                Console.WriteLine($"{progName}: sorry, can't read file from console");
                return;
            }

            try
            {
                using (var input_file = new FileStream(args[file_index], FileMode.Open))
                {
                    /* Open the drop file. */
                    if (dropfilename != null)
                    {
                        // TODO: 
                        throw new NotImplementedException();
                    }

                    /* Specify data source for decompression */
                    srcinfo.jpeg_stdio_src(input_file);

                    /* Enable saving of extra markers that we want to copy */
                    {
                        // TODO: 
                        _ = copyoption;
                    }

                    /* Read file header */
                    srcinfo.jpeg_read_header(true);

                    /* Adjust default decompression parameters */
                    if (scaleoption != null)
                    {
                        // TODO:
                        _ = scaleoption;
                    }

                    if (dropfilename != null)
                    {
                        // TODO: 
                        throw new NotImplementedException();
                    }

                    /* Any space needed by a transform option must be requested before
                     * jpeg_read_coefficients so that memory allocation will be done right.
                     */

                    /* Fail right away if -perfect is given and transformation is not perfect.
                     */
                    if (!jtransform.request_workspace(srcinfo, transformoption))
                    {
                        Console.WriteLine($"{progName}: transformation is not perfect");
                        return;
                    }

                    /* Read source file as DCT coefficients */
                    var src_coef_arrays = srcinfo.jpeg_read_coefficients();

                    if (dropfilename != null)
                    {
                        // TODO: 
                        throw new NotImplementedException();
                    }

                    /* Initialize destination compression parameters from source values */
                    srcinfo.jpeg_copy_critical_parameters(dstinfo);

                    /* Adjust destination parameters if required by transform options;
                     * also find out which set of coefficient arrays will hold the output.
                     */
                    var dst_coef_arrays = jtransform.adjust_parameters(
                        srcinfo, dstinfo, src_coef_arrays, transformoption);

                    /* Open the output file. */
                    if (outfilename == null)
                    {
                        Console.WriteLine($"{progName}: sorry, can't write file to console");
                        return;
                    }

                    try
                    {
                        using (var output_file = new FileStream(outfilename, FileMode.Create))
                        {
                            /* Adjust default compression parameters by re-parsing the options */
                            parse_switches(dstinfo, args, true, out _);

                            /* Specify data destination for compression */
                            dstinfo.jpeg_stdio_dest(output_file);

                            /* Start compressor (note no image data is actually written here) */
                            dstinfo.jpeg_write_coefficients(dst_coef_arrays);

                            /* Copy to the output file any extra markers that we want to preserve */
                            jtransform.jcopy_markers_execute(srcinfo, dstinfo);

                            jtransform.execute_transformation(srcinfo, dstinfo, src_coef_arrays, transformoption);

                            /* Finish compression and release memory */
                            dstinfo.jpeg_finish_compress();
                            dstinfo.jpeg_destroy();

                            if (dropfilename != null)
                            {
                                // TODO: 
                                throw new NotImplementedException();
                            }

                            srcinfo.jpeg_finish_decompress();
                            srcinfo.jpeg_destroy();
                        }
                    }
                    catch (Exception e)
                    {
                        Console.WriteLine($"{progName}: can't open {outfilename} for writing");
                        Console.WriteLine(e.Message);
                        return;
                    }
                }
            }
            catch (Exception e)
            {
                Console.WriteLine($"{progName}: can't open {args[file_index]} for reading");
                Console.WriteLine(e.Message);
                return;
            }
        }

        /// <summary>
        /// complain about bad command line
        /// </summary>
        private static void usage()
        {
            Console.WriteLine($"usage: {progName} [switches] inputfile outputfile");

            Console.WriteLine("Switches (names may be abbreviated):");
            Console.WriteLine("  -copy none     Copy no extra markers from source file");
            Console.WriteLine("  -copy comments Copy only comment markers (default)");
            Console.WriteLine("  -copy all      Copy all extra markers");
            Console.WriteLine("  -optimize      Optimize Huffman table (smaller file, but slow compression)");
            Console.WriteLine("  -progressive   Create progressive JPEG file");
            Console.WriteLine("Switches for modifying the image:");
            Console.WriteLine("  -crop WxH+X+Y  Crop to a rectangular subarea");
            Console.WriteLine("  -drop +X+Y filename          Drop another image");
            Console.WriteLine("  -flip [horizontal|vertical]  Mirror image (left-right or top-bottom)");
            Console.WriteLine("  -grayscale     Reduce to grayscale (omit color data)");
            Console.WriteLine("  -perfect       Fail if there is non-transformable edge blocks");
            Console.WriteLine("  -rotate [90|180|270]         Rotate image (degrees clockwise)");
            Console.WriteLine("  -scale M/N     Scale output image by fraction M/N, eg, 1/8");
            Console.WriteLine("  -transpose     Transpose image");
            Console.WriteLine("  -transverse    Transverse transpose image");
            Console.WriteLine("  -trim          Drop non-transformable edge blocks");
            Console.WriteLine("                 with -drop: Requantize drop file to source file");
            Console.WriteLine("  -wipe WxH+X+Y  Wipe (gray out) a rectangular subarea");
            Console.WriteLine("Switches for advanced users:");
            Console.WriteLine("  -arithmetic    Use arithmetic coding");
            Console.WriteLine("  -restart N     Set restart interval in rows, or in blocks with B");
            Console.WriteLine("  -outfile name  Specify name for output file");
            Console.WriteLine("  -verbose  or  -debug   Emit debug output");
            Console.WriteLine("Switches for wizards:");
            Console.WriteLine("  -scans file    Create multi-scan JPEG per script file");
        }

        /// <summary>
        /// Parse optional switches.
        /// Returns true if switches were parsed successfully; false otherwise.
        /// fileIndex receives index of first file-name argument (== -1 if none).
        /// for_real is false on the first (dummy) pass; we may skip any expensive
        /// processing.
        /// </summary>
        private static bool parse_switches(
            jpeg_compress_struct cinfo, string[] argv, bool for_real, out int fileIndex)
        {
            /* Set up default JPEG parameters. */
            bool simple_progressive = false;
            string scansarg = null;	/* saves -scans parm if any */
            outfilename = null;
            scaleoption = null;
            copyoption = JCOPY_OPTION.JCOPYOPT_COMMENTS;
            transformoption.transform = JXFORM_CODE.JXFORM_NONE;
            transformoption.perfect = for_real;
            transformoption.trim = for_real;
            transformoption.force_grayscale = for_real;
            transformoption.crop = for_real;
            cinfo.Err.Trace_level = 0;
            fileIndex = -1;

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

                if (cdjpeg_utils.keymatch(arg, "arithmetic", 1))
                {
                    Console.WriteLine("Switches (names may be abbreviated):");
                    return false;
                }
                else if (cdjpeg_utils.keymatch(arg, "copy", 2))
                {
                    /* Select which extra markers to copy. */
                    argn++; /* advance to next argument */
                    if (argn >= argv.Length)
                    {
                        usage();
                        return false;
                    }

                    if (cdjpeg_utils.keymatch(argv[argn], "none", 1))
                        copyoption = JCOPY_OPTION.JCOPYOPT_NONE;
                    else if (cdjpeg_utils.keymatch(argv[argn], "comments", 1))
                        copyoption = JCOPY_OPTION.JCOPYOPT_COMMENTS;
                    else if (cdjpeg_utils.keymatch(argv[argn], "all", 1))
                        copyoption = JCOPY_OPTION.JCOPYOPT_ALL;
                    else
                        usage();
                }
                else if (cdjpeg_utils.keymatch(arg, "crop", 2))
                {
                    /* Perform lossless cropping. */
                    argn++; /* advance to next argument */
                    if (argn >= argv.Length)
                    {
                        usage();
                        return false;
                    }

                    if (transformoption.crop /* reject multiple crop/drop/wipe requests */ ||
                        !jtransform.parse_crop_spec(transformoption, argv[argn]))
                    {
                        Console.WriteLine($"{progName} bogus -crop argument {argv[argn]}");
                        return false;
                    }
                }
                else if (cdjpeg_utils.keymatch(arg, "drop", 2))
                {
                    argn++; /* advance to next argument */
                    if (argn >= argv.Length)
                    {
                        usage();
                        return false;
                    }

                    if (transformoption.crop /* reject multiple crop/drop/wipe requests */ ||
                        !jtransform.parse_crop_spec(transformoption, argv[argn]) ||
                        transformoption.crop_width_set != JCROP_CODE.JCROP_UNSET ||
                        transformoption.crop_height_set != JCROP_CODE.JCROP_UNSET)
                    {
                        Console.WriteLine($"{progName} bogus -drop argument {argv[argn]}");
                        return false;
                    }

                    argn++; /* advance to next argument */
                    if (argn >= argv.Length)
                    {
                        usage();
                        return false;
                    }

                    dropfilename = argv[argn];
                    select_transform(JXFORM_CODE.JXFORM_DROP);
                }
                else if (cdjpeg_utils.keymatch(arg, "debug", 1) ||
                    cdjpeg_utils.keymatch(arg, "verbose", 1))
                {
                    /* Enable debug printouts. */
                    /* On first -d, print version identification */
                    if (!printed_version)
                    {
                        Console.WriteLine($"Bit Miracle's JpegTran, version {jpeg_common_struct.Version}\n{jpeg_common_struct.Copyright}");
                        printed_version = true;
                    }
                    cinfo.Err.Trace_level++;
                }
                else if (cdjpeg_utils.keymatch(arg, "flip", 1))
                {
                    /* Mirror left-right or top-bottom. */
                    argn++; /* advance to next argument */
                    if (argn >= argv.Length)
                    {
                        usage();
                        return false;
                    }

                    if (cdjpeg_utils.keymatch(argv[argn], "horizontal", 1))
                        select_transform(JXFORM_CODE.JXFORM_FLIP_H);
                    else if (cdjpeg_utils.keymatch(argv[argn], "vertical", 1))
                        select_transform(JXFORM_CODE.JXFORM_FLIP_V);
                    else
                        usage();
                }
                else if (cdjpeg_utils.keymatch(arg, "grayscale", 1) ||
                    cdjpeg_utils.keymatch(arg, "greyscale", 1))
                {
                    /* Force to grayscale. */
                    transformoption.force_grayscale = true;
                }
                else if (cdjpeg_utils.keymatch(arg, "optimize", 1) ||
                    cdjpeg_utils.keymatch(arg, "optimise", 1))
                {
                    /* Enable entropy parm optimization. */
                    cinfo.Optimize_coding = true;
                }
                else if (cdjpeg_utils.keymatch(arg, "outfile", 4))
                {
                    /* Set output file name. */
                    argn++; /* advance to next argument */
                    if (argn >= argv.Length)
                    {
                        usage();
                        return false;
                    }

                    outfilename = argv[argn];   /* save it away for later use */
                }
                else if (cdjpeg_utils.keymatch(arg, "perfect", 2))
                {
                    /* Fail if there is any partial edge MCUs that the transform can't
                     * handle. */
                    transformoption.perfect = true;
                }
                else if (cdjpeg_utils.keymatch(arg, "progressive", 2))
                {
                    /* Select simple progressive mode. */
                    simple_progressive = true;
                    /* We must postpone execution until num_components is known. */
                }
                else if (cdjpeg_utils.keymatch(arg, "restart", 1))
                {
                    /* Restart interval in MCU rows (or in MCUs with 'b'). */
                    argn++; /* advance to next argument */
                    if (argn >= argv.Length)
                    {
                        usage();
                        return false;
                    }

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
                else if (cdjpeg_utils.keymatch(arg, "rotate", 2))
                {
                    /* Rotate 90, 180, or 270 degrees (measured clockwise). */
                    argn++; /* advance to next argument */
                    if (argn >= argv.Length)
                    {
                        usage();
                        return false;
                    }

                    if (cdjpeg_utils.keymatch(argv[argn], "90", 2))
                        select_transform(JXFORM_CODE.JXFORM_ROT_90);
                    else if (cdjpeg_utils.keymatch(argv[argn], "180", 3))
                        select_transform(JXFORM_CODE.JXFORM_ROT_180);
                    else if (cdjpeg_utils.keymatch(argv[argn], "270", 3))
                        select_transform(JXFORM_CODE.JXFORM_ROT_270);
                    else
                        usage();
                }
                else if (cdjpeg_utils.keymatch(arg, "scale", 4))
                {
                    /* Scale the output image by a fraction M/N. */
                    argn++; /* advance to next argument */
                    if (argn >= argv.Length)
                    {
                        usage();
                        return false;
                    }

                    scaleoption = argv[argn];
                    /* We must postpone processing until decompression startup. */
                }
                else if (cdjpeg_utils.keymatch(arg, "scans", 1))
                {
                    /* Set scan script. */
                    argn++; /* advance to next argument */
                    if (argn >= argv.Length)
                    {
                        usage();
                        return false;
                    }

                    scansarg = argv[argn];
                    /* We must postpone reading the file in case -progressive appears. */
                }
                else if (cdjpeg_utils.keymatch(arg, "transpose", 1))
                {
                    /* Transpose (across UL-to-LR axis). */
                    select_transform(JXFORM_CODE.JXFORM_TRANSPOSE);
                }
                else if (cdjpeg_utils.keymatch(arg, "transverse", 6))
                {
                    /* Transverse transpose (across UR-to-LL axis). */
                    select_transform(JXFORM_CODE.JXFORM_TRANSVERSE);
                }
                else if (cdjpeg_utils.keymatch(arg, "trim", 3))
                {
                    /* Trim off any partial edge MCUs that the transform can't handle. */
                    transformoption.trim = true;
                }
                else if (cdjpeg_utils.keymatch(arg, "wipe", 1))
                {
                    argn++; /* advance to next argument */
                    if (argn >= argv.Length)
                    {
                        usage();
                        return false;
                    }

                    if (transformoption.crop /* reject multiple crop/drop/wipe requests */ ||
                        !jtransform.parse_crop_spec(transformoption, argv[argn]))
                    {
                        Console.WriteLine($"{progName}: bogus -wipe argument {argv[argn]}");
                        return false;
                    }
                    select_transform(JXFORM_CODE.JXFORM_WIPE);
                }
                else
                {
                    usage();            /* bogus switch */
                }
            }

            /* Post-switch-scanning cleanup */

            if (for_real)
            {
                if (simple_progressive) /* process -progressive; -scans can override */
                    cinfo.jpeg_simple_progression();

                if (scansarg != null)
                {
                    /* process -scans if it was present */
                    throw new NotImplementedException();
                }
            }

            return true;
        }

        /* Silly little routine to detect multiple transform options,
         * which we can't handle.
         */
        private static void select_transform(JXFORM_CODE transform)
        {
            if (transformoption.transform == JXFORM_CODE.JXFORM_NONE ||
                transformoption.transform == transform)
            {
                transformoption.transform = transform;
            }
            else
            {
                Console.WriteLine($"{progName}: can only do one image transformation at a time");
                usage();
            }
        }
    }
}
