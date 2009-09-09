/* Copyright (C) 2008-2009, Bit Miracle
 * http://www.bitmiracle.com
 * 
 * Copyright (C) 1994-1996, Thomas G. Lane.
 * This file is part of the Independent JPEG Group's software.
 * For conditions of distribution and use, see the accompanying README file.
 *
 */

using System;
using System.Collections.Generic;
using System.Text;

using BitMiracle.LibJpeg.Classic;
using BitMiracle.cdJpeg;

/*
 * This file contains routines to write output images in Microsoft "BMP"
 * format (MS Windows 3.x and OS/2 1.x flavors).
 * Either 8-bit colormapped or 24-bit full-color format can be written.
 * No compression is supported.
 *
 * These routines may need modification for non-Unix environments or
 * specialized applications.  As they stand, they assume output to
 * an ordinary stdio stream.
 *
 * This code contributed by James Arthur Boucher.
 */

/*
* To support 12-bit JPEG data, we'd have to scale output down to 8 bits.
* This is not yet implemented.
*/

/*
* Since BMP stores scanlines bottom-to-top, we have to invert the image
* from JPEG's top-to-bottom order.  To do this, we save the outgoing data
* in a virtual array during put_pixel_row calls, then actually emit the
* BMP file during finish_output.  The virtual array contains one byte per
* pixel if the output is grayscale or colormapped, three if it is full color.
*/

namespace BitMiracle.Jpeg
{
    class bmp_dest_struct : djpeg_dest_struct
    {
        private jpeg_decompress_struct cinfo;
        private bool m_putGrayRows;
        private bool is_os2;        /* saves the OS2 format request flag */

        private jvirt_sarray_control whole_image;  /* needed to reverse row order */
        private int data_width;  /* bytes per row */
        private int row_width;       /* physical width of one row in the BMP file */
        private int pad_bytes;      /* number of padding bytes needed per row */
        private int cur_output_row;  /* next row# to write to virtual array */

        public bmp_dest_struct(jpeg_decompress_struct cinfo, bool is_os2)
        {
            this.cinfo = cinfo;
            this.is_os2 = is_os2;

            if (cinfo.Out_color_space == J_COLOR_SPACE.JCS_GRAYSCALE)
            {
                m_putGrayRows = true;
            }
            else if (cinfo.Out_color_space == J_COLOR_SPACE.JCS_RGB)
            {
                if (cinfo.Quantize_colors)
                    m_putGrayRows = true;
                else
                    m_putGrayRows = false;
            }
            else
            {
                cinfo.ERREXIT((int)ADDON_MESSAGE_CODE.JERR_BMP_COLORSPACE);
            }

            /* Calculate output image dimensions so we can allocate space */
            cinfo.jpeg_calc_output_dimensions();

            /* Determine width of rows in the BMP file (padded to 4-byte boundary). */
            row_width = cinfo.Output_width * cinfo.Output_components;
            data_width = row_width;
            while ((row_width & 3) != 0)
                row_width++;

            pad_bytes = row_width - data_width;

            /* Allocate space for inversion array, prepare for write pass */
            whole_image = new jvirt_sarray_control(cinfo, row_width, cinfo.Output_height);
            cur_output_row = 0;
            if (cinfo.Progress != null)
            {
                cdjpeg_progress_mgr progress = (cdjpeg_progress_mgr)cinfo.Progress;
                progress.total_extra_passes++; /* count file input as separate pass */
            }

            /* Create decompressor output buffer. */
            buffer = jpeg_common_struct.AllocJpegSamples(row_width, 1);
            buffer_height = 1;
        }

        /// <summary>
        /// Startup: normally writes the file header.
        /// In this module we may as well postpone everything until finish_output.
        /// </summary>
        public override void start_output()
        {
            /* no work here */
        }

        /// <summary>
        /// Write some pixel data.
        /// In this module rows_supplied will always be 1.
        /// </summary>
        public override void put_pixel_rows(int rows_supplied)
        {
            if (m_putGrayRows)
                put_gray_rows(rows_supplied);
            else
                put_24bit_rows(rows_supplied);
        }

        /// <summary>
        /// Finish up at the end of the file.
        /// Here is where we really output the BMP file.
        /// </summary>
        public override void finish_output()
        {
            /* Write the header and colormap */
            if (is_os2)
                write_os2_header();
            else
                write_bmp_header();

            cdjpeg_progress_mgr progress = (cdjpeg_progress_mgr)cinfo.Progress;
            /* Write the file body from our virtual array */
            for (int row = cinfo.Output_height; row > 0; row--)
            {
                if (progress != null)
                {
                    progress.Pass_counter = cinfo.Output_height - row;
                    progress.Pass_limit = cinfo.Output_height;
                    progress.progress_monitor();
                }

                byte[][] image_ptr = whole_image.access_virt_sarray(row - 1, 1);
                int imageIndex = 0;
                for (int col = row_width; col > 0; col--)
                {
                    output_file.WriteByte(image_ptr[0][imageIndex]);
                    imageIndex++;
                }
            }

            if (progress != null)
                progress.completed_extra_passes++;

            /* Make sure we wrote the output file OK */
            output_file.Flush();
        }

        /// <summary>
        /// Write some pixel data.
        /// In this module rows_supplied will always be 1.
        /// 
        /// This version is for writing 24-bit pixels
        /// </summary>
        private void put_24bit_rows(int rows_supplied)
        {
            /* Access next row in virtual array */
            byte[][] image_ptr = whole_image.access_virt_sarray(cur_output_row, 1);
            cur_output_row++;

            /* Transfer data.  Note destination values must be in BGR order
             * (even though Microsoft's own documents say the opposite).
             */
            int bufferIndex = 0;
            int imageIndex = 0;
            for (int col = cinfo.Output_width; col > 0; col--)
            {
                image_ptr[0][imageIndex + 2] = buffer[0][bufferIndex];   /* can omit GETJSAMPLE() safely */
                bufferIndex++;
                image_ptr[0][imageIndex + 1] = buffer[0][bufferIndex];
                bufferIndex++;
                image_ptr[0][imageIndex] = buffer[0][bufferIndex];
                bufferIndex++;
                imageIndex += 3;
            }

            /* Zero out the pad bytes. */
            int pad = pad_bytes;
            while (--pad >= 0)
            {
                image_ptr[0][imageIndex] = 0;
                imageIndex++;
            }
        }

        /// <summary>
        /// Write some pixel data.
        /// In this module rows_supplied will always be 1.
        /// 
        /// This version is for grayscale OR quantized color output
        /// </summary>
        private void put_gray_rows(int rows_supplied)
        {
            /* Access next row in virtual array */
            byte[][] image_ptr = whole_image.access_virt_sarray(cur_output_row, 1);
            cur_output_row++;

            /* Transfer data. */
            int index = 0;
            for (int col = cinfo.Output_width; col > 0; col--)
            {
                image_ptr[0][index] = buffer[0][index];/* can omit GETJSAMPLE() safely */
                index++;
            }

            /* Zero out the pad bytes. */
            int pad = pad_bytes;
            while (--pad >= 0)
            {
                image_ptr[0][index] = 0;
                index++;
            }
        }

        /// <summary>
        /// Write a Windows-style BMP file header, including colormap if needed
        /// </summary>
        private void write_bmp_header()
        {
            int bits_per_pixel;
            int cmap_entries;

            /* Compute colormap size and total file size */
            if (cinfo.Out_color_space == J_COLOR_SPACE.JCS_RGB)
            {
                if (cinfo.Quantize_colors)
                {
                    /* Colormapped RGB */
                    bits_per_pixel = 8;
                    cmap_entries = 256;
                }
                else
                {
                    /* Unquantized, full color RGB */
                    bits_per_pixel = 24;
                    cmap_entries = 0;
                }
            }
            else
            {
                /* Grayscale output.  We need to fake a 256-entry colormap. */
                bits_per_pixel = 8;
                cmap_entries = 256;
            }

            /* File size */
            int headersize = 14 + 40 + cmap_entries * 4; /* Header and colormap */
            int bfSize = headersize + row_width * cinfo.Output_height;

            /* Set unused fields of header to 0 */
            byte[] bmpfileheader = new byte[14];
            byte[] bmpinfoheader = new byte[40];

            /* Fill the file header */
            bmpfileheader[0] = 0x42;    /* first 2 bytes are ASCII 'B', 'M' */
            bmpfileheader[1] = 0x4D;
            PUT_4B(bmpfileheader, 2, bfSize); /* bfSize */
            /* we leave bfReserved1 & bfReserved2 = 0 */
            PUT_4B(bmpfileheader, 10, headersize); /* bfOffBits */

            /* Fill the info header (Microsoft calls this a BITMAPINFOHEADER) */
            PUT_2B(bmpinfoheader, 0, 40);   /* biSize */
            PUT_4B(bmpinfoheader, 4, cinfo.Output_width); /* biWidth */
            PUT_4B(bmpinfoheader, 8, cinfo.Output_height); /* biHeight */
            PUT_2B(bmpinfoheader, 12, 1);   /* biPlanes - must be 1 */
            PUT_2B(bmpinfoheader, 14, bits_per_pixel); /* biBitCount */
            /* we leave biCompression = 0, for none */
            /* we leave biSizeImage = 0; this is correct for uncompressed data */

            if (cinfo.Density_unit == DensityUnit.DotsCm)
            {
                /* if have density in dots/cm, then */
                PUT_4B(bmpinfoheader, 24, (int)(cinfo.X_density * 100)); /* XPels/M */
                PUT_4B(bmpinfoheader, 28, (int)(cinfo.Y_density * 100)); /* XPels/M */
            }
            PUT_2B(bmpinfoheader, 32, cmap_entries); /* biClrUsed */
            /* we leave biClrImportant = 0 */

            try
            {
                output_file.Write(bmpfileheader, 0, 14);
            }
            catch (Exception e)
            {
                cinfo.TRACEMS(0, J_MESSAGE_CODE.JERR_FILE_WRITE, e.Message);
                cinfo.ERREXIT(J_MESSAGE_CODE.JERR_FILE_WRITE);
            }

            try
            {
                output_file.Write(bmpinfoheader, 0, 40);
            }
            catch (Exception e)
            {
                cinfo.TRACEMS(0, J_MESSAGE_CODE.JERR_FILE_WRITE, e.Message);
                cinfo.ERREXIT(J_MESSAGE_CODE.JERR_FILE_WRITE);
            }

            if (cmap_entries > 0)
                write_colormap(cmap_entries, 4);
        }

        /// <summary>
        /// Write an OS2-style BMP file header, including colormap if needed
        /// </summary>
        private void write_os2_header()
        {
            int bits_per_pixel;
            int cmap_entries;

            /* Compute colormap size and total file size */
            if (cinfo.Out_color_space == J_COLOR_SPACE.JCS_RGB)
            {
                if (cinfo.Quantize_colors)
                {
                    /* Colormapped RGB */
                    bits_per_pixel = 8;
                    cmap_entries = 256;
                }
                else
                {
                    /* Unquantized, full color RGB */
                    bits_per_pixel = 24;
                    cmap_entries = 0;
                }
            }
            else
            {
                /* Grayscale output.  We need to fake a 256-entry colormap. */
                bits_per_pixel = 8;
                cmap_entries = 256;
            }

            /* File size */
            int headersize = 14 + 12 + cmap_entries * 3; /* Header and colormap */
            int bfSize = headersize + row_width * cinfo.Output_height;

            /* Set unused fields of header to 0 */
            byte[] bmpfileheader = new byte[14];
            byte[] bmpcoreheader = new byte[12];

            /* Fill the file header */
            bmpfileheader[0] = 0x42;    /* first 2 bytes are ASCII 'B', 'M' */
            bmpfileheader[1] = 0x4D;
            PUT_4B(bmpfileheader, 2, bfSize); /* bfSize */
            /* we leave bfReserved1 & bfReserved2 = 0 */
            PUT_4B(bmpfileheader, 10, headersize); /* bfOffBits */

            /* Fill the info header (Microsoft calls this a BITMAPCOREHEADER) */
            PUT_2B(bmpcoreheader, 0, 12);   /* bcSize */
            PUT_2B(bmpcoreheader, 4, cinfo.Output_width); /* bcWidth */
            PUT_2B(bmpcoreheader, 6, cinfo.Output_height); /* bcHeight */
            PUT_2B(bmpcoreheader, 8, 1);    /* bcPlanes - must be 1 */
            PUT_2B(bmpcoreheader, 10, bits_per_pixel); /* bcBitCount */

            try
            {
                output_file.Write(bmpfileheader, 0, 14);
            }
            catch (Exception e)
            {
                cinfo.TRACEMS(0, J_MESSAGE_CODE.JERR_FILE_WRITE, e.Message);
                cinfo.ERREXIT(J_MESSAGE_CODE.JERR_FILE_WRITE);
            }

            try
            {
                output_file.Write(bmpcoreheader, 0, 12);
            }
            catch (Exception e)
            {
                cinfo.TRACEMS(0, J_MESSAGE_CODE.JERR_FILE_WRITE, e.Message);
                cinfo.ERREXIT(J_MESSAGE_CODE.JERR_FILE_WRITE);
            }

            if (cmap_entries > 0)
                write_colormap(cmap_entries, 3);
        }

        /// <summary>
        /// Write the colormap.
        /// Windows uses BGR0 map entries; OS/2 uses BGR entries.
        /// </summary>
        private void write_colormap(int map_colors, int map_entry_size)
        {
            byte[][] colormap = cinfo.Colormap;
            int num_colors = cinfo.Actual_number_of_colors;

            int i = 0;
            if (colormap != null)
            {
                if (cinfo.Out_color_components == 3)
                {
                    /* Normal case with RGB colormap */
                    for (i = 0; i < num_colors; i++)
                    {
                        output_file.WriteByte(colormap[2][i]);
                        output_file.WriteByte(colormap[1][i]);
                        output_file.WriteByte(colormap[0][i]);
                        if (map_entry_size == 4)
                            output_file.WriteByte(0);
                    }
                }
                else
                {
                    /* Grayscale colormap (only happens with grayscale quantization) */
                    for (i = 0; i < num_colors; i++)
                    {
                        output_file.WriteByte(colormap[0][i]);
                        output_file.WriteByte(colormap[0][i]);
                        output_file.WriteByte(colormap[0][i]);
                        if (map_entry_size == 4)
                            output_file.WriteByte(0);
                    }
                }
            }
            else
            {
                /* If no colormap, must be grayscale data.  Generate a linear "map". */
                for (i = 0; i < 256; i++)
                {
                    output_file.WriteByte((byte)i);
                    output_file.WriteByte((byte)i);
                    output_file.WriteByte((byte)i);
                    if (map_entry_size == 4)
                        output_file.WriteByte(0);
                }
            }

            /* Pad colormap with zeros to ensure specified number of colormap entries */
            if (i > map_colors)
                cinfo.ERREXIT((int)ADDON_MESSAGE_CODE.JERR_TOO_MANY_COLORS, i);

            for (; i < map_colors; i++)
            {
                output_file.WriteByte(0);
                output_file.WriteByte(0);
                output_file.WriteByte(0);
                if (map_entry_size == 4)
                    output_file.WriteByte(0);
            }
        }

        private static void PUT_2B(byte[] array, int offset, int value)
        {
            array[offset] = (byte)((value) & 0xFF);
            array[offset + 1] = (byte)(((value) >> 8) & 0xFF);
        }

        private static void PUT_4B(byte[] array, int offset, int value)
        {
            array[offset] = (byte)((value) & 0xFF);
            array[offset + 1] = (byte)(((value) >> 8) & 0xFF);
            array[offset + 2] = (byte)(((value) >> 16) & 0xFF);
            array[offset + 3] = (byte)(((value) >> 24) & 0xFF);
        }
    }
}
