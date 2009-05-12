/* Copyright (C) 2008-2009, Bit Miracle
 * http://www.bitmiracle.com
 * 
 * Copyright (C) 1994-1996, Thomas G. Lane.
 * This file is part of the Independent JPEG Group's software.
 * For conditions of distribution and use, see the accompanying README file.
 *
 */

using System.Globalization;
namespace cdJpeg
{
    static class cdjpeg_utils
    {
        /// <summary>
        /// Case-insensitive matching of possibly-abbreviated keyword switches.
        /// keyword is the constant keyword (must be lower case already),
        /// minchars is length of minimum legal abbreviation.
        /// </summary>
        public static bool keymatch(string arg, string keyword, int minchars)
        {
            string lowered = arg.ToLower(CultureInfo.InvariantCulture);
            if (lowered.Length > keyword.Length)
            {
                // arg longer than keyword, no good
                return false;
            }
            else if (lowered.Length == keyword.Length)
            {
                return lowered == keyword;
            }

            return (lowered == keyword.Substring(0, minchars));
        }
    }
}
