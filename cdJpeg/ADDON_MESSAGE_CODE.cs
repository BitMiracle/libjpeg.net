/* Copyright (C) 2008-2011, Bit Miracle
 * http://www.bitmiracle.com
 * 
 * Copyright (C) 1994-1996, Thomas G. Lane.
 * This file is part of the Independent JPEG Group's software.
 * For conditions of distribution and use, see the accompanying README file.
 *
 */

/*
 * This file defines the error and message codes for the cjpeg/djpeg
 * applications.  These strings are not needed as part of the JPEG library
 * proper. Edit this file to add new codes, or to translate the message strings to
 * some other language.
 */

namespace BitMiracle.cdJpeg
{
    public enum ADDON_MESSAGE_CODE
    {
        // Must be first entry!
        JMSG_FIRSTADDONCODE = 1000,

        JERR_BMP_BADCMAP,
        JERR_BMP_BADDEPTH,
        JERR_BMP_BADHEADER,
        JERR_BMP_BADPLANES,
        JERR_BMP_COLORSPACE,
        JERR_BMP_COMPRESSED,
        JERR_BMP_NOT,
        JTRC_BMP,
        JTRC_BMP_MAPPED,
        JTRC_BMP_OS2,
        JTRC_BMP_OS2_MAPPED,

        JERR_GIF_BUG,
        JERR_GIF_CODESIZE,
        JERR_GIF_COLORSPACE,
        JERR_GIF_IMAGENOTFOUND,
        JERR_GIF_NOT,
        JTRC_GIF,
        JTRC_GIF_BADVERSION,
        JTRC_GIF_EXTENSION,
        JTRC_GIF_NONSQUARE,
        JWRN_GIF_BADDATA,
        JWRN_GIF_CHAR,
        JWRN_GIF_ENDCODE,
        JWRN_GIF_NOMOREDATA,

        JERR_BAD_CMAP_FILE,
        JERR_TOO_MANY_COLORS,
        JERR_UNGETC_FAILED,
        JERR_UNKNOWN_FORMAT,
        JERR_UNSUPPORTED_FORMAT,

        JMSG_LASTADDONCODE
    }
}