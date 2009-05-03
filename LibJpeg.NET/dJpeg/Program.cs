/* Copyright (C) 2008-2009, Bit Miracle
 * http://www.bitmiracle.com
 * 
 * Copyright (C) 1994-1996, Thomas G. Lane.
 * This file is part of the Independent JPEG Group's software.
 * For conditions of distribution and use, see the accompanying README file.
 *
 */

/*
 * This file contains a command-line user interface for the JPEG decompressor.
 *
 * To simplify script writing, the "-outfile" switch is provided.  The syntax
 *  djpeg [options]  -outfile outputfile  inputfile
 * works regardless of which command line style is used.
 */

using System;
using System.Collections.Generic;
using System.Text;
using System.IO;

using LibJpeg.NET;
using cdJpeg;

namespace dJpeg
{
    class Program
    {
        static IMAGE_FORMATS requested_fmt;
        static string progname;    /* program name for error messages */
        static string outfilename;   /* for -outfile switch */

        static void Main(string[] args)
        {
            progname = Path.GetFileName(Environment.GetCommandLineArgs()[0]);

            /* Initialize the JPEG decompression object with default error handling. */
            cd_jpeg_error_mgr err;
            jpeg_decompress_struct cinfo = new jpeg_decompress_struct(err);

            /* Insert custom marker processor for COM and APP12.
             * APP12 is used by some digital camera makers for textual info,
             * so we provide the ability to display it as text.
             * If you like, additional APPn marker types can be selected for display,
             * but don't try to override APP0 or APP14 this way (see libjpeg.doc).
             */
            cinfo.jpeg_set_marker_processor(JPEG_MARKER.M_COM, print_text_marker);
            cinfo.jpeg_set_marker_processor(JPEG_MARKER.M_APP0 + 12, print_text_marker);

            /* Scan command line to find file names. */
            /* It is convenient to use just one switch-parsing routine, but the switch
             * values read here are ignored; we will rescan the switches after opening
             * the input file.
             * (Exception: tracing level set here controls verbosity for COM markers
             * found during jpeg_read_header...)
             */
            int file_index;
            bool parsedOK = parse_switches(cinfo, args, false, out file_index);

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

            /* Specify data source for decompression */
            cinfo.jpeg_stdio_src(input_file);

            /* Read file header, set default decompression parameters */
            cinfo.jpeg_read_header(true);

            /* Adjust default decompression parameters by re-parsing the options */
            parse_switches(cinfo, args, true, out file_index);

            /* Initialize the output module now to let it override any crucial
             * option settings (for instance, GIF wants to force color quantization).
             */
            djpeg_dest_struct dest_mgr = null;

            switch (requested_fmt)
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
            dest_mgr.output_file = output_file;

            /* Start decompressor */
            cinfo.jpeg_start_decompress();

            /* Write output file header */
            dest_mgr.start_output();

            /* Process data */
            while (cinfo.m_output_scanline < cinfo.m_output_height)
            {
                uint num_scanlines = cinfo.jpeg_read_scanlines(dest_mgr.buffer, dest_mgr.buffer_height);
                dest_mgr.put_pixel_rows(num_scanlines);
            }

            /* Finish decompression and release memory.
             * I must do it in this order because output module has allocated memory
             * of lifespan JPOOL_IMAGE; it needs to finish before releasing memory.
             */
            dest_mgr.finish_output();
            cinfo.jpeg_finish_decompress();

            /* Close files, if we opened them */
            input_file.Close();
            input_file.Dispose();

            output_file.Close();
            output_file.Dispose();

            /* All done. */
            if (cinfo.m_err.m_num_warnings != 0)
                Console.WriteLine("Corrupt-data warning count is not zero");
        }
    }
}
