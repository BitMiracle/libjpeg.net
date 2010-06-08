/* Copyright (C) 2008-2009, Bit Miracle
 * http://www.bitmiracle.com
 * 
 * Copyright (C) 1994-1996, Thomas G. Lane.
 * This file is part of the Independent JPEG Group's software.
 * For conditions of distribution and use, see the accompanying README file.
 *
 */

using System;
using System.Collections.Generic;
using System.Text;
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
