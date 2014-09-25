﻿using System;
using System.Collections.Generic;
using System.Text;

namespace BitMiracle.LibJpeg.Classic
{
    /// <summary>
    /// The unit of density.
    /// </summary>
#if EXPOSE_LIBJPEG
    public
#endif
    enum DensityUnit
    {
        /// <summary>
        /// Unknown density
        /// </summary>
        Unknown = 0,
        /// <summary>
        /// Dots/inch
        /// </summary>
        DotsInch = 1,
        /// <summary>
        /// Dots/cm
        /// </summary>
        DotsCm = 2
    }
}
