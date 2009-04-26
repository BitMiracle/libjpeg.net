/* Copyright (C) 2008-2009, Bit Miracle
 * http://www.bitmiracle.com
 * 
 * Copyright (C) 1994-1996, Thomas G. Lane.
 * This file is part of the Independent JPEG Group's software.
 * For conditions of distribution and use, see the accompanying README file.
 *
 */

namespace LibJpeg.NET
{
    class cd_jpeg_error_mgr : jpeg_error_mgr
    {
        protected override string GetMessageText(int code)
        {
            switch ((ADDON_MESSAGE_CODE) code)
            {
            default:
                return base.GetMessageText(code);

            case JERR_BMP_BADCMAP:
                return "Unsupported BMP colormap format";
            case JERR_BMP_BADDEPTH:
                return "Only 8- and 24-bit BMP files are supported";
            case JERR_BMP_BADHEADER:
                return "Invalid BMP file: bad header length";
            case JERR_BMP_BADPLANES:
                return "Invalid BMP file: biPlanes not equal to 1";
            case JERR_BMP_COLORSPACE:
                return "BMP output must be grayscale or RGB";
            case JERR_BMP_COMPRESSED:
                return "Sorry, compressed BMPs not yet supported";
            case JERR_BMP_NOT:
                return "Not a BMP file - does not start with BM";
            case JTRC_BMP:
                return "%ux%u 24-bit BMP image";
            case JTRC_BMP_MAPPED:
                return "%ux%u 8-bit colormapped BMP image";
            case JTRC_BMP_OS2:
                return "%ux%u 24-bit OS2 BMP image";
            case JTRC_BMP_OS2_MAPPED:
                return "%ux%u 8-bit colormapped OS2 BMP image";
                //
            case JERR_GIF_BUG:
                return "GIF output got confused";
            case JERR_GIF_CODESIZE:
                return "Bogus GIF codesize %d";
            case JERR_GIF_COLORSPACE:
                return "GIF output must be grayscale or RGB";
            case JERR_GIF_IMAGENOTFOUND:
                return "Too few images in GIF file";
            case JERR_GIF_NOT:
                return "Not a GIF file";
            case JTRC_GIF:
                return "%ux%ux%d GIF image";
            case JTRC_GIF_BADVERSION:
                return "Warning: unexpected GIF version number '%c%c%c'";
            case JTRC_GIF_EXTENSION:
                return "Ignoring GIF extension block of type 0x%02x";
            case JTRC_GIF_NONSQUARE:
                return "Caution: nonsquare pixels in input";
            case JWRN_GIF_BADDATA:
                return "Corrupt data in GIF file";
            case JWRN_GIF_CHAR:
                return "Bogus char 0x%02x in GIF file, ignoring";
            case JWRN_GIF_ENDCODE:
                return "Premature end of GIF image";
            case JWRN_GIF_NOMOREDATA:
                return "Ran out of GIF bits";
                //
            case JERR_BAD_CMAP_FILE:
                return "Color map file is invalid or of unsupported format";
            case JERR_TOO_MANY_COLORS:
                return "Output file format cannot handle %d colormap entries";
            case JERR_UNGETC_FAILED:
                return "ungetc failed";
            case JERR_UNKNOWN_FORMAT:
                return "Unrecognized input file format";
            case JERR_UNSUPPORTED_FORMAT:
                return "Unsupported output file format";
            }
        }
    }
}