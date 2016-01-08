using System.IO;

namespace BitMiracle.dJpeg
{
    /// <summary>
    /// Object interface for djpeg's output file encoding modules
    /// </summary>
    abstract class djpeg_dest_struct
    {
        /* Target file spec; filled in by djpeg.c after object is created. */
        public Stream output_file;

        /* Output pixel-row buffer.  Created by module init or start_output.
         * Width is cinfo.output_width * cinfo.output_components;
         * height is buffer_height.
         */
        public byte[][] buffer;
        public int buffer_height;

        /* start_output is called after jpeg_start_decompress finishes.
         * The color map will be ready at this time, if one is needed.
         */
        public abstract void start_output();

        /* Emit the specified number of pixel rows from the buffer. */
        public abstract void put_pixel_rows(int rows_supplied);

        /* Finish up at the end of the image. */
        public abstract void finish_output();
    }
}
