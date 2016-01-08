using System.IO;

namespace BitMiracle.cJpeg
{
    abstract class cjpeg_source_struct
    {
        public abstract void start_input();
        public abstract int get_pixel_rows();
        public abstract void finish_input();

        public Stream input_file;
        public byte[][] buffer;
        public uint buffer_height;
    }
}
