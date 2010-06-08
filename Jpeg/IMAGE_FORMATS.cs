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

namespace BitMiracle.Jpeg
{
    /// <summary>
    /// This list defines the known output image formats
    /// (not all of which need be supported by a given version).
    /// </summary>
    enum IMAGE_FORMATS
    {
        FMT_BMP, /* BMP format (Windows flavor) */
        FMT_OS2, /* BMP format (OS/2 flavor) */
    }
}
