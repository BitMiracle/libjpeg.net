using System;
using System.Collections.Generic;
using System.Text;

using LibJpeg.NET;

namespace cJpeg
{
    class Program
    {
        static void Main(string[] args)
        {
            //progname = argv[0];

            jpeg_compress_struct cinfo;
            cd_jpeg_error_mgr err;
            cinfo.SetErrorManager(err);

            /* Initialize JPEG parameters.
             * Much of this may be overridden later.
             * In particular, we don't yet know the input file's color space,
             * but we need to provide some value for jpeg_set_defaults() to work.
             */

            cinfo.m_in_color_space = J_COLOR_SPACE.JCS_RGB; /* arbitrary guess */
            cinfo.jpeg_set_defaults();

            /* Scan command line to find file names.
             * It is convenient to use just one switch-parsing routine, but the switch
             * values read here are ignored; we will rescan the switches after opening
             * the input file.
             */

            int file_index = parse_switches(cinfo, argc, argv, 0, false);

            /* Must have either -outfile switch or explicit output file name */
            if (outfilename == NULL)
            {
                if (file_index != argc - 2)
                {
                    fprintf(stderr, "%s: must name one input and one output file\n", progname);
                    usage();
                }
                outfilename = argv[file_index + 1];
            }
            else
            {
                if (file_index != argc - 1)
                {
                    fprintf(stderr, "%s: must name one input and one output file\n", progname);
                    usage();
                }
            }

            FILE* input_file;

            /* Open the input file. */
            if (file_index < argc)
            {
                if ((input_file = fopen(argv[file_index], "rb")) == NULL)
                {
                    fprintf(stderr, "%s: can't open %s\n", progname, argv[file_index]);
                    return EXIT_FAILURE;
                }
            }
            else
            {
                /* default input file is stdin */
                input_file = read_stdin();
            }

            FILE* output_file;

            /* Open the output file. */
            if (outfilename != NULL)
            {
                if ((output_file = fopen(outfilename, "wb")) == NULL)
                {
                    fprintf(stderr, "%s: can't open %s\n", progname, outfilename);
                    return EXIT_FAILURE;
                }
            }
            else
            {
                /* default output file is stdout */
                output_file = write_stdout();
            }

            /* Figure out the input file format, and set up to read it. */
            cjpeg_source_struct* src_mgr = new bmp_source_struct(&cinfo);
            src_mgr->input_file = input_file;

            /* Read the input file header to obtain file size & colorspace. */
            src_mgr->start_input();

            /* Now that we know input colorspace, fix colorspace-dependent defaults */
            cinfo.jpeg_default_colorspace();

            /* Adjust default compression parameters by re-parsing the options */
            file_index = parse_switches(&cinfo, argc, argv, 0, true);

            /* Specify data destination for compression */
            cinfo.jpeg_stdio_dest(output_file);

            /* Start compressor */
            cinfo.jpeg_start_compress(true);

            /* Process data */
            while (cinfo.m_next_scanline < cinfo.m_image_height)
            {
                JDIMENSION num_scanlines = src_mgr->get_pixel_rows();
                cinfo.jpeg_write_scanlines(src_mgr->buffer, num_scanlines);
            }

            /* Finish compression and release memory */
            src_mgr->finish_input();
            cinfo.jpeg_finish_compress();

            /* Close files, if we opened them */
            if (input_file != stdin)
                fclose(input_file);

            if (output_file != stdout)
                fclose(output_file);

            /* All done. */
            if (cinfo.m_err->m_num_warnings != 0)
                return EXIT_WARNING;

            return EXIT_SUCCESS;
        }
    }
}
