using System;

namespace GaleForceCore.Helpers
{
    public static class TimeHelpers
    {
        /// <summary>
        /// Return a DateTime for a Unix time stamp.
        /// </summary>
        /// <param name="unixTimeStamp">The unix time stamp.</param>
        /// <returns>DateTime.</returns>
        public static DateTime UnixTimeStampToDateTime(double unixTimeStamp)
        {
            // Unix timestamp is seconds past epoch
            System.DateTime dtDateTime = new DateTime(1970, 1, 1, 0, 0, 0, 0, System.DateTimeKind.Utc);
            dtDateTime = dtDateTime.AddSeconds(unixTimeStamp).ToLocalTime();
            return dtDateTime;
        }

        /// <summary>
        /// Returns the Max date between two dates. If both null, which shouldn't happen, returns MinValue.
        /// </summary>
        /// <param name="d1">The first date.</param>
        /// <param name="d2">The second date.</param>
        /// <returns>DateTime.</returns>
        public static DateTime Max(DateTime d1, DateTime d2)
        {
            if(d1 == null && d2 == null)
            {
                return DateTime.MinValue;
            }
            else if(d1 == null)
            {
                return d2;
            }
            else if(d2 == null)
            {
                return d1;
            }
            else
            {
                return d1.CompareTo(d2) < 0 ? d2 : d1;
            }
        }

        /// <summary>
        /// Returns the Min date between two dates. If both null, which shouldn't happen, returns MaxValue.
        /// </summary>
        /// <param name="d1">The first date.</param>
        /// <param name="d2">The second date.</param>
        /// <returns>DateTime.</returns>
        public static DateTime Min(DateTime d1, DateTime d2)
        {
            if(d1 == null && d2 == null)
            {
                return DateTime.MaxValue;
            }
            else if(d1 == null)
            {
                return d2;
            }
            else if(d2 == null)
            {
                return d1;
            }
            else
            {
                return d1.CompareTo(d2) < 0 ? d1 : d2;
            }
        }
    }
}
