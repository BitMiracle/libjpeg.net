using BitMiracle.LibJpeg.Classic;

using JDIMENSION = System.Int32;

namespace BitMiracle.JpegTran
{
    /// <summary>
    /// Transform parameters.
    /// </summary>
    /// <remarks>
    /// NB: application must not change any elements of this struct after calling
    /// jtransform_request_workspace.
    /// </remarks>
    [System.Diagnostics.CodeAnalysis.SuppressMessage("Style", "IDE1006:Naming Styles")]
#pragma warning disable 0649
    class jpeg_transform_info
    {
        /* Options: set by caller */

        /// <summary>
        /// image transform operator
        /// </summary>
        public JXFORM_CODE transform;

        /// <summary>
        /// if TRUE, fail if partial MCUs are requested
        /// </summary>
        public bool perfect;

        /// <summary>
        /// if TRUE, trim partial MCUs as needed
        /// </summary>
        public bool trim;

        /// <summary>
        /// if TRUE, convert color image to grayscale
        /// </summary>
        public bool force_grayscale;

        /// <summary>
        /// if TRUE, crop or wipe source image, or drop
        /// </summary>
        public bool crop;

        /* Crop parameters: application need not set these unless crop is TRUE.
         * These can be filled in by jtransform_parse_crop_spec().
         */

        /// <summary>
        /// Width of selected region
        /// </summary>
        public JDIMENSION crop_width;

        /// <summary>
        /// (force disables adjustment)
        /// </summary>
        public JCROP_CODE crop_width_set;

        /// <summary>
        /// Height of selected region
        /// </summary>
        public JDIMENSION crop_height;

        /// <summary>
        /// (force disables adjustment)
        /// </summary>
        public JCROP_CODE crop_height_set;

        /// <summary>
        /// X offset of selected region
        /// </summary>
        public JDIMENSION crop_xoffset;

        /// <summary>
        /// (negative measures from right edge)
        /// </summary>
        public JCROP_CODE crop_xoffset_set;

        /// <summary>
        /// Y offset of selected region
        /// </summary>
        public JDIMENSION crop_yoffset;

        /// <summary>
        /// (negative measures from bottom edge)
        /// </summary>
        public JCROP_CODE crop_yoffset_set;

        /* Drop parameters: set by caller for drop request */
        
        /// <summary>
        /// 
        /// </summary>
        public jpeg_decompress_struct drop_ptr;

        // public jvirt_barray_ptr[] drop_coef_arrays;

        /* Internal workspace: caller should not touch these */

        /// <summary>
        /// # of components in workspace
        /// </summary>
        public int num_components;

        /// <summary>
        /// workspace for transformations
        /// </summary>
        public jvirt_array<JBLOCK>[] workspace_coef_arrays;

        /// <summary>
        /// cropped destination dimensions
        /// </summary>
        public JDIMENSION output_width;

        /// <summary>
        /// 
        /// </summary>
        public JDIMENSION output_height;

        /// <summary>
        /// destination crop offsets measured in iMCUs
        /// </summary>
        public JDIMENSION x_crop_offset;

        /// <summary>
        /// 
        /// </summary>
        public JDIMENSION y_crop_offset;

        /// <summary>
        /// drop/wipe dimensions measured in iMCUs
        /// </summary>
        public JDIMENSION drop_width;

        /// <summary>
        /// 
        /// </summary>
        public JDIMENSION drop_height;

        /// <summary>
        /// destination iMCU size
        /// </summary>
        public int iMCU_sample_width;

        /// <summary>
        /// 
        /// </summary>
        public int iMCU_sample_height;
    }
#pragma warning restore 0649
}
