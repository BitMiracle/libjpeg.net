/* Copyright (C) 2008-2009, Bit Miracle
 * http://www.bitmiracle.com
 * 
 * Copyright (C) 1994-1996, Thomas G. Lane.
 * This file is part of the Independent JPEG Group's software.
 * For conditions of distribution and use, see the accompanying README file.
 *
 */

/*
 * This file contains routines to read input images in Microsoft "BMP"
 * format (MS Windows 3.x, OS/2 1.x, and OS/2 2.x flavors).
 * Currently, only 8-bit and 24-bit images are supported, not 1-bit or
 * 4-bit (feeding such low-depth images into JPEG would be silly anyway).
 * Also, we don't support RLE-compressed files.
 *
 * Original code was contributed by James Arthur Boucher.
 */

using System;
using System.Collections.Generic;
using System.Text;
using System.IO;

using BitMiracle.LibJpeg.Classic;
using BitMiracle.cdJpeg;

namespace BitMiracle.cJpeg
{
    /// <summary>
    /// 
    /// </summary>
    class bmp_source_struct : cjpeg_source_struct
    {
        private enum PixelRowsMethod
        {
            preload,
            use8bit,
            use24bit
        }

        private jpeg_compress_struct cinfo;
        private PixelRowsMethod m_pixelRowsMethod;

        // BMP colormap (converted to my format)
        private byte[][] colormap;

        // Needed to reverse row order
        private jvirt_sarray_control whole_image;

        // Current source row number
        private int source_row;

        // Physical width of scanlines in file
        private int row_width;

        // remembers 8- or 24-bit format
        private int bits_per_pixel;

        public bmp_source_struct(jpeg_compress_struct cinfo)
        {
            this.cinfo = cinfo;
        }

        /// <summary>
        /// Read the file header; detects image size and component count.
        /// </summary>
        public override void start_input()
        {
            byte[] bmpfileheader = new byte[14];
            /* Read and verify the bitmap file header */
            if (!ReadOK(input_file, bmpfileheader, 0, 14))
                cinfo.ERREXIT(J_MESSAGE_CODE.JERR_INPUT_EOF);

            if (GET_2B(bmpfileheader, 0) != 0x4D42) /* 'BM' */
                cinfo.ERREXIT((int)ADDON_MESSAGE_CODE.JERR_BMP_NOT);
            
            int bfOffBits = GET_4B(bmpfileheader, 10);
            /* We ignore the remaining fileheader fields */

            /* The infoheader might be 12 bytes (OS/2 1.x), 40 bytes (Windows),
             * or 64 bytes (OS/2 2.x).  Check the first 4 bytes to find out which.
             */
            byte[] bmpinfoheader = new byte[64];
            if (!ReadOK(input_file, bmpinfoheader, 0, 4))
                cinfo.ERREXIT(J_MESSAGE_CODE.JERR_INPUT_EOF);

            int headerSize = GET_4B(bmpinfoheader, 0);
            if (headerSize < 12 || headerSize> 64)
                cinfo.ERREXIT((int)ADDON_MESSAGE_CODE.JERR_BMP_BADHEADER);

            if (!ReadOK(input_file, bmpinfoheader, 4, headerSize - 4))
                cinfo.ERREXIT(J_MESSAGE_CODE.JERR_INPUT_EOF);

            int biWidth = 0;      /* initialize to avoid compiler warning */
            int biHeight = 0;
            int biPlanes;
            int biCompression;
            int biXPelsPerMeter;
            int biYPelsPerMeter;
            int biClrUsed = 0;
            int mapentrysize = 0;       /* 0 indicates no colormap */
            switch (headerSize)
            {
            case 12:
                /* Decode OS/2 1.x header (Microsoft calls this a BITMAPCOREHEADER) */
                biWidth = GET_2B(bmpinfoheader, 4);
                biHeight = GET_2B(bmpinfoheader, 6);
                biPlanes = GET_2B(bmpinfoheader, 8);
                bits_per_pixel = GET_2B(bmpinfoheader, 10);

                switch (bits_per_pixel)
                {
                case 8:
                    /* colormapped image */
                    mapentrysize = 3;       /* OS/2 uses RGBTRIPLE colormap */
                    cinfo.TRACEMS(1, (int)ADDON_MESSAGE_CODE.JTRC_BMP_OS2_MAPPED, biWidth, biHeight);
                    break;
                case 24:
                    /* RGB image */
                    cinfo.TRACEMS(1, (int)ADDON_MESSAGE_CODE.JTRC_BMP_OS2, biWidth, biHeight);
                    break;
                default:
                    cinfo.ERREXIT((int)ADDON_MESSAGE_CODE.JERR_BMP_BADDEPTH);
                    break;
                }
                if (biPlanes != 1)
                    cinfo.ERREXIT((int)ADDON_MESSAGE_CODE.JERR_BMP_BADPLANES);
                break;
            case 40:
            case 64:
                /* Decode Windows 3.x header (Microsoft calls this a BITMAPINFOHEADER) */
                /* or OS/2 2.x header, which has additional fields that we ignore */
                biWidth = GET_4B(bmpinfoheader, 4);
                biHeight = GET_4B(bmpinfoheader, 8);
                biPlanes = GET_2B(bmpinfoheader, 12);
                bits_per_pixel = GET_2B(bmpinfoheader, 14);
                biCompression = GET_4B(bmpinfoheader, 16);
                biXPelsPerMeter = GET_4B(bmpinfoheader, 24);
                biYPelsPerMeter = GET_4B(bmpinfoheader, 28);
                biClrUsed = GET_4B(bmpinfoheader, 32);
                /* biSizeImage, biClrImportant fields are ignored */

                switch (bits_per_pixel)
                {
                case 8:
                    /* colormapped image */
                    mapentrysize = 4;       /* Windows uses RGBQUAD colormap */
                    cinfo.TRACEMS(1, (int)ADDON_MESSAGE_CODE.JTRC_BMP_MAPPED, biWidth, biHeight);
                    break;
                case 24:
                    /* RGB image */
                    cinfo.TRACEMS(1, (int)ADDON_MESSAGE_CODE.JTRC_BMP, biWidth, biHeight);
                    break;
                default:
                    cinfo.ERREXIT((int)ADDON_MESSAGE_CODE.JERR_BMP_BADDEPTH);
                    break;
                }
                if (biPlanes != 1)
                    cinfo.ERREXIT((int)ADDON_MESSAGE_CODE.JERR_BMP_BADPLANES);
                if (biCompression != 0)
                    cinfo.ERREXIT((int)ADDON_MESSAGE_CODE.JERR_BMP_COMPRESSED);

                if (biXPelsPerMeter > 0 && biYPelsPerMeter > 0)
                {
                    /* Set JFIF density parameters from the BMP data */
                    cinfo.X_density = (short)(biXPelsPerMeter / 100); /* 100 cm per meter */
                    cinfo.Y_density = (short)(biYPelsPerMeter / 100);
                    cinfo.Density_unit = DensityUnit.DotsCm;
                }
                break;
            default:
                cinfo.ERREXIT((int)ADDON_MESSAGE_CODE.JERR_BMP_BADHEADER);
                break;
            }

            /* Compute distance to bitmap data --- will adjust for colormap below */
            int bPad = bfOffBits - (headerSize + 14);

            /* Read the colormap, if any */
            if (mapentrysize > 0)
            {
                if (biClrUsed <= 0)
                    biClrUsed = 256;        /* assume it's 256 */
                else if (biClrUsed > 256)
                    cinfo.ERREXIT((int)ADDON_MESSAGE_CODE.JERR_BMP_BADCMAP);
                /* Allocate space to store the colormap */
                colormap = jpeg_common_struct.AllocJpegSamples(biClrUsed, 3);
                /* and read it from the file */
                read_colormap(biClrUsed, mapentrysize);
                /* account for size of colormap */
                bPad -= biClrUsed * mapentrysize;
            }

            /* Skip any remaining pad bytes */
            if (bPad < 0)           /* incorrect bfOffBits value? */
                cinfo.ERREXIT((int)ADDON_MESSAGE_CODE.JERR_BMP_BADHEADER);

            while (--bPad >= 0)
            {
                read_byte();
            }

            /* Compute row width in file, including padding to 4-byte boundary */
            if (bits_per_pixel == 24)
                row_width = biWidth * 3;
            else
                row_width = biWidth;

            while ((row_width & 3) != 0)
                row_width++;

            /* Allocate space for inversion array, prepare for preload pass */
            whole_image = new jvirt_sarray_control(cinfo, row_width, biHeight);
            m_pixelRowsMethod = PixelRowsMethod.preload;
            if (cinfo.Progress != null)
            {
                cdjpeg_progress_mgr progress = cinfo.Progress as cdjpeg_progress_mgr;
                progress.total_extra_passes++; /* count file input as separate pass */
            }

            /* Allocate one-row buffer for returned data */
            buffer = jpeg_common_struct.AllocJpegSamples(biWidth * 3, 1);
            buffer_height = 1;

            cinfo.In_color_space = J_COLOR_SPACE.JCS_RGB;
            cinfo.Input_components = 3;
            cinfo.Data_precision = 8;
            cinfo.Image_width = biWidth;
            cinfo.Image_height = biHeight;
        }

        public override int get_pixel_rows()
        {
            if (m_pixelRowsMethod == PixelRowsMethod.preload)
                return preload_image();
            else if (m_pixelRowsMethod == PixelRowsMethod.use8bit)
                return get_8bit_row();

            return get_24bit_row();
        }

        // Finish up at the end of the file.
        public override void finish_input()
        {
            // no work
        }

        /// <summary>
        /// Read one row of pixels. 
        /// The image has been read into the whole_image array, but is otherwise
        /// unprocessed.  We must read it out in top-to-bottom row order, and if
        /// it is an 8-bit image, we must expand colormapped pixels to 24bit format.
        /// This version is for reading 8-bit colormap indexes.
        /// </summary>
        private int get_8bit_row()
        {
            /* Fetch next row from virtual array */
            source_row--;

            byte[][] image_ptr = whole_image.access_virt_sarray(source_row, 1);

            /* Expand the colormap indexes to real data */
            int imageIndex = 0;
            int outIndex = 0;
            for (int col = cinfo.Image_width; col > 0; col--)
            {
                int t = image_ptr[0][imageIndex];
                imageIndex++;

                buffer[0][outIndex] = colormap[0][t]; /* can omit GETbyte() safely */
                outIndex++;
                buffer[0][outIndex] = colormap[1][t];
                outIndex++;
                buffer[0][outIndex] = colormap[2][t];
                outIndex++;
            }

            return 1;
        }

        /// <summary>
        /// Read one row of pixels. 
        /// The image has been read into the whole_image array, but is otherwise
        /// unprocessed.  We must read it out in top-to-bottom row order, and if
        /// it is an 8-bit image, we must expand colormapped pixels to 24bit format.
        /// This version is for reading 24-bit pixels.
        /// </summary>
        private int get_24bit_row()
        {
            /* Fetch next row from virtual array */
            source_row--;
            byte[][] image_ptr = whole_image.access_virt_sarray(source_row, 1);

            /* Transfer data.  Note source values are in BGR order
             * (even though Microsoft's own documents say the opposite).
             */
            int imageIndex = 0;
            int outIndex = 0;

            for (int col = cinfo.Image_width; col > 0; col--)
            {
                buffer[0][outIndex + 2] = image_ptr[0][imageIndex];   /* can omit GETbyte() safely */
                imageIndex++;
                buffer[0][outIndex + 1] = image_ptr[0][imageIndex];
                imageIndex++;
                buffer[0][outIndex] = image_ptr[0][imageIndex];
                imageIndex++;
                outIndex += 3;
            }

            return 1;
        }

        /// <summary>
        /// This method loads the image into whole_image during the first call on
        /// get_pixel_rows. 
        /// </summary>
        private int preload_image()
        {
            cdjpeg_progress_mgr progress = cinfo.Progress as cdjpeg_progress_mgr;

            /* Read the data into a virtual array in input-file row order. */
            for (int row = 0; row < cinfo.Image_height; row++)
            {
                if (progress != null)
                {
                    progress.Pass_counter = row;
                    progress.Pass_limit = cinfo.Image_height;
                    progress.Updated();
                }

                byte[][] image_ptr = whole_image.access_virt_sarray(row, 1);
                int imageIndex = 0;
                for (int col = row_width; col > 0; col--)
                {
                    /* inline copy of read_byte() for speed */
                    int c = input_file.ReadByte();
                    if (c == -1)
                        cinfo.ERREXIT(J_MESSAGE_CODE.JERR_INPUT_EOF);

                    image_ptr[0][imageIndex] = (byte)c;
                    imageIndex++;
                }
            }

            if (progress != null)
                progress.completed_extra_passes++;

            /* Set up to read from the virtual array in top-to-bottom order */
            switch (bits_per_pixel)
            {
                case 8:
                    m_pixelRowsMethod = PixelRowsMethod.use8bit;
                    break;
                case 24:
                    m_pixelRowsMethod = PixelRowsMethod.use24bit;
                    break;
                default:
                    cinfo.ERREXIT((int)ADDON_MESSAGE_CODE.JERR_BMP_BADDEPTH);
                    break;
            }

            source_row = cinfo.Image_height;

            /* And read the first row */
            return get_pixel_rows();
        }

        // Read next byte from BMP file
        private int read_byte()
        {
            int c = input_file.ReadByte();
            if (c == -1)
                cinfo.ERREXIT(J_MESSAGE_CODE.JERR_INPUT_EOF);

            return c;
        }

        // Read the colormap from a BMP file
        private void read_colormap(int cmaplen, int mapentrysize)
        {
            switch (mapentrysize)
            {
                case 3:
                    /* BGR format (occurs in OS/2 files) */
                    for (int i = 0; i < cmaplen; i++)
                    {
                        colormap[2][i] = (byte)read_byte();
                        colormap[1][i] = (byte)read_byte();
                        colormap[0][i] = (byte)read_byte();
                    }
                    break;
                case 4:
                    /* BGR0 format (occurs in MS Windows files) */
                    for (int i = 0; i < cmaplen; i++)
                    {
                        colormap[2][i] = (byte)read_byte();
                        colormap[1][i] = (byte)read_byte();
                        colormap[0][i] = (byte)read_byte();
                        read_byte();
                    }
                    break;
                default:
                    cinfo.ERREXIT((int)ADDON_MESSAGE_CODE.JERR_BMP_BADCMAP);
                    break;
            }
        }
        
        private static bool ReadOK(Stream file, byte[] buffer, int offset, int len)
        {
            int read = file.Read(buffer, offset, len);
            return (read == len);
        }

        private static int GET_2B(byte[] array, int offset)
        {
            return (int)array[offset] + ((int)array[offset + 1] << 8);
        }

        private static int GET_4B(byte[] array, int offset)
        {
            return (int)array[offset] + ((int)array[offset + 1] << 8) + ((int)array[offset + 2] << 16) + ((int)array[offset + 3] << 24);
        }
    }
}
