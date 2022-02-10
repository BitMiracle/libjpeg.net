namespace BitMiracle.JpegTran
{
    /// <summary>
    /// Codes for crop parameters
    /// </summary>
    enum JCROP_CODE
    {
        /// <summary>
        /// Unspecified
        /// </summary>
        JCROP_UNSET,

        /// <summary>
        /// Positive for xoffset, yoffset, width, or height.
        /// </summary>
        JCROP_POS,

        /// <summary>
        /// Negative for xoffset, yoffset, width, or height.
        /// </summary>
        JCROP_NEG,

        /// <summary>
        /// Force for width or height.
        /// </summary>
        JCROP_FORCE,

        /// <summary>
        /// Reflect for width or height.
        /// </summary>
        JCROP_REFLECT
    }
}
