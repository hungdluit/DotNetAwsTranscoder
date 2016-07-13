using System;
using System.IO;

namespace AwsTranscoder.Helpers
{
    public static class CommonHelpers
    {
        /// <summary>
        /// Force convert to int even if it might not be.  If not return default value for type.
        /// </summary>
        public static int ToInt(this object o)
        {
            if (o == null)
                return 0;

            int i;
            int.TryParse(o.ToString(), out i);
            return i;
        }

        /// <summary>
        /// Get windows date from unix based milliseconds
        /// </summary>
        /// <param name="unixMilli">milliseconds</param>
        /// <returns></returns>
        public static DateTime DateFromUnixMilli(this long unixMilli)
        {
            return new DateTime(1970, 1, 1).AddMilliseconds(unixMilli);
        }

        /// <summary>
        /// Get the filename without the extension
        /// </summary>
        /// <param name="fileName">The full filename</param>
        /// <returns>Filename without extension.</returns>
        public static string WithoutExtension(this string fileName)
        {
            return Path.GetFileNameWithoutExtension(fileName)?.TrimEnd('.');
        }
    }
}