using BitMiracle.LibJpeg.Classic;

namespace BitMiracle.cdJpeg
{
    class cd_jpeg_error_mgr : jpeg_error_mgr
    {
        protected override string GetMessageText(int code)
        {
            switch ((ADDON_MESSAGE_CODE) code)
            {
                default:
                    return base.GetMessageText(code);

                case ADDON_MESSAGE_CODE.JERR_BMP_BADCMAP:
                    return "Unsupported BMP colormap format";
                case ADDON_MESSAGE_CODE.JERR_BMP_BADDEPTH:
                    return "Only 8- and 24-bit BMP files are supported";
                case ADDON_MESSAGE_CODE.JERR_BMP_BADHEADER:
                    return "Invalid BMP file: bad header length";
                case ADDON_MESSAGE_CODE.JERR_BMP_BADPLANES:
                    return "Invalid BMP file: biPlanes not equal to 1";
                case ADDON_MESSAGE_CODE.JERR_BMP_COLORSPACE:
                    return "BMP output must be grayscale or RGB";
                case ADDON_MESSAGE_CODE.JERR_BMP_COMPRESSED:
                    return "Sorry, compressed BMPs not yet supported";
                case ADDON_MESSAGE_CODE.JERR_BMP_NOT:
                    return "Not a BMP file - does not start with BM";
                case ADDON_MESSAGE_CODE.JTRC_BMP:
                    return "{0}x{1} 24-bit BMP image";
                case ADDON_MESSAGE_CODE.JTRC_BMP_MAPPED:
                    return "{0}x{1} 8-bit colormapped BMP image";
                case ADDON_MESSAGE_CODE.JTRC_BMP_OS2:
                    return "{0}x{1} 24-bit OS2 BMP image";
                case ADDON_MESSAGE_CODE.JTRC_BMP_OS2_MAPPED:
                    return "{0}x{1} 8-bit colormapped OS2 BMP image";
                    //
                case ADDON_MESSAGE_CODE.JERR_GIF_BUG:
                    return "GIF output got confused";
                case ADDON_MESSAGE_CODE.JERR_GIF_CODESIZE:
                    return "Bogus GIF codesize {0}";
                case ADDON_MESSAGE_CODE.JERR_GIF_COLORSPACE:
                    return "GIF output must be grayscale or RGB";
                case ADDON_MESSAGE_CODE.JERR_GIF_IMAGENOTFOUND:
                    return "Too few images in GIF file";
                case ADDON_MESSAGE_CODE.JERR_GIF_NOT:
                    return "Not a GIF file";
                case ADDON_MESSAGE_CODE.JTRC_GIF:
                    return "{0}x{1}x{2} GIF image";
                case ADDON_MESSAGE_CODE.JTRC_GIF_BADVERSION:
                    return "Warning: unexpected GIF version number '{0}{1}{2}'";
                case ADDON_MESSAGE_CODE.JTRC_GIF_EXTENSION:
                    return "Ignoring GIF extension block of type 0x{0:X2}";
                case ADDON_MESSAGE_CODE.JTRC_GIF_NONSQUARE:
                    return "Caution: nonsquare pixels in input";
                case ADDON_MESSAGE_CODE.JWRN_GIF_BADDATA:
                    return "Corrupt data in GIF file";
                case ADDON_MESSAGE_CODE.JWRN_GIF_CHAR:
                    return "Bogus char 0x{0:X2} in GIF file, ignoring";
                case ADDON_MESSAGE_CODE.JWRN_GIF_ENDCODE:
                    return "Premature end of GIF image";
                case ADDON_MESSAGE_CODE.JWRN_GIF_NOMOREDATA:
                    return "Ran out of GIF bits";
                    //
                case ADDON_MESSAGE_CODE.JERR_BAD_CMAP_FILE:
                    return "Color map file is invalid or of unsupported format";
                case ADDON_MESSAGE_CODE.JERR_TOO_MANY_COLORS:
                    return "Output file format cannot handle %d colormap entries";
                case ADDON_MESSAGE_CODE.JERR_UNGETC_FAILED:
                    return "ungetc failed";
                case ADDON_MESSAGE_CODE.JERR_UNKNOWN_FORMAT:
                    return "Unrecognized input file format";
                case ADDON_MESSAGE_CODE.JERR_UNSUPPORTED_FORMAT:
                    return "Unsupported output file format";
            }
        }
    }
}