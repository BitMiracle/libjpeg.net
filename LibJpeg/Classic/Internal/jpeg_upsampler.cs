﻿namespace BitMiracle.LibJpeg.Classic.Internal
{
    /// <summary>
    /// Upsampling (note that upsampler must also call color converter)
    /// </summary>
    abstract class jpeg_upsampler
    {
        protected bool m_need_context_rows; /* true if need rows above & below */

        public abstract void start_pass();
        public abstract void upsample(ComponentBuffer[] input_buf, ref int in_row_group_ctr, int in_row_groups_avail, byte[][] output_buf, ref int out_row_ctr, int out_rows_avail);

        public bool NeedContextRows()
        {
            return m_need_context_rows;
        }
    }
}
