using System.Globalization;

namespace BitMiracle.cdJpeg
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
