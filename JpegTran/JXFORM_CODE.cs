namespace BitMiracle.JpegTran
{
    /// <summary>
    /// Supported types of image transformations.
    /// </summary>
    enum JXFORM_CODE
    {
		/// <summary>
		/// no transformation
		/// </summary>
		JXFORM_NONE,

		/// <summary>
		/// horizontal flip
		/// </summary>
		JXFORM_FLIP_H,

		/// <summary>
		/// vertical flip
		/// </summary>
		JXFORM_FLIP_V,

		/// <summary>
		/// transpose across UL-to-LR axis
		/// </summary>
		JXFORM_TRANSPOSE,

		/// <summary>
		/// transpose across UR-to-LL axis
		/// </summary>
		JXFORM_TRANSVERSE,

		/// <summary>
		/// 90-degree clockwise rotation
		/// </summary>
		JXFORM_ROT_90,

		/// <summary>
		/// 180-degree rotation
		/// </summary>
		JXFORM_ROT_180,

		/// <summary>
		/// 270-degree clockwise (or 90 ccw)
		/// </summary>
		JXFORM_ROT_270,

		/// <summary>
		/// wipe
		/// </summary>
		JXFORM_WIPE,

		/// <summary>
		/// drop
		/// </summary>
		JXFORM_DROP
	}
}
