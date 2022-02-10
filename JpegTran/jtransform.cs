using System;

using BitMiracle.LibJpeg.Classic;

namespace BitMiracle.JpegTran
{
    /// <summary>
    /// Image transformation routines and other utility code. These are NOT part of the core 
    /// JPEG library.
    /// </summary>
    [System.Diagnostics.CodeAnalysis.SuppressMessage("Style", "IDE1006:Naming Styles")]
    class jtransform
    {
        /* Parse a crop specification (written in X11 geometry style).
         * The routine returns true if the spec string is valid, false if not.
         *
         * The crop spec string should have the format
         *	<width>[{fr}]x<height>[{fr}]{+-}<xoffset>{+-}<yoffset>
         * where width, height, xoffset, and yoffset are unsigned integers.
         * Each of the elements can be omitted to indicate a default value.
         * (A weakness of this style is that it is not possible to omit xoffset
         * while specifying yoffset, since they look alike.)
         *
         * This code is loosely based on XParseGeometry from the X11 distribution.
         */
        public static bool parse_crop_spec(jpeg_transform_info info, string spec)
        {
            throw new NotImplementedException();
        }

        /* Request any required workspace.
         *
         * This routine figures out the size that the output image will be
         * (which implies that all the transform parameters must be set before
         * it is called).
         *
         * We allocate the workspace virtual arrays from the source decompression
         * object, so that all the arrays (both the original data and the workspace)
         * will be taken into account while making memory management decisions.
         * Hence, this routine must be called after jpeg_read_header (which reads
         * the image dimensions) and before jpeg_read_coefficients (which realizes
         * the source's virtual arrays).
         *
         * This function returns false right away if -perfect is given
         * and transformation is not perfect. Otherwise returns true.
         */
        public static bool request_workspace(jpeg_decompress_struct srcinfo, jpeg_transform_info info)
        {
            throw new NotImplementedException();
        }

        /* Adjust output image parameters as needed.
         *
         * This must be called after jpeg_copy_critical_parameters()
         * and before jpeg_write_coefficients().
         *
         * The return value is the set of virtual coefficient arrays to be written
         * (either the ones allocated by jtransform_request_workspace, or the
         * original source data arrays). The caller will need to pass this value
         * to jpeg_write_coefficients().
         */
        public static jvirt_array<JBLOCK>[] adjust_parameters(
            jpeg_decompress_struct srcinfo, jpeg_compress_struct dstinfo,
            jvirt_array<JBLOCK>[] src_coef_arrays, jpeg_transform_info info)
        {
            throw new NotImplementedException();
        }

        /* Copy markers saved in the given source object to the destination object.
         * This should be called just after jpeg_start_compress() or
         * jpeg_write_coefficients().
         * Note that those routines will have written the SOI, and also the
         * JFIF APP0 or Adobe APP14 markers if selected.
         */
        public static void jcopy_markers_execute(jpeg_decompress_struct srcinfo, jpeg_compress_struct dstinfo)
        {
            throw new NotImplementedException();
        }

        /* Execute the actual transformation, if any.
         *
         * This must be called *after* jpeg_write_coefficients, because it depends
         * on jpeg_write_coefficients to have computed subsidiary values such as
         * the per-component width and height fields in the destination object.
         *
         * Note that some transformations will modify the source data arrays!
         */
        public static void execute_transformation(
            jpeg_decompress_struct srcinfo, jpeg_compress_struct dstinfo,
            jvirt_array<JBLOCK>[] src_coef_arrays, jpeg_transform_info info)
        {
            throw new NotImplementedException();
        }
    }
}
