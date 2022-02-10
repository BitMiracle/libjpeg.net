namespace BitMiracle.JpegTran
{
    /// <summary>
    /// Support for copying optional markers from source to destination file.
    /// </summary>
    enum JCOPY_OPTION
    {
        /// <summary>
        /// copy no optional markers
        /// </summary>
        JCOPYOPT_NONE,

        /// <summary>
        /// copy only comment (COM) markers
        /// </summary>
        JCOPYOPT_COMMENTS,

        /// <summary>
        /// copy all optional markers
        /// </summary>
        JCOPYOPT_ALL
    }
}
