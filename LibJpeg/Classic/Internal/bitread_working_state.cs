namespace BitMiracle.LibJpeg.Classic.Internal
{
    /// <summary>
    /// Bitreading working state within an MCU
    /// </summary>
    class bitread_working_state
    {
        public int get_buffer;    /* current bit-extraction buffer */
        public int bits_left;      /* # of unused bits in it */
    }
}
