using NodaTime.TimeZones;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace AuthService.Helpers
{
    public static class DateHelpers
    {
        /// <summary>
        /// Gets the time zone information for TZDB identifier.
        /// The tzdb identifier is a olson time representation (string).
        /// </summary>
        /// <param name="tzdbId">The TZDB identifier.</param>
        /// <returns></returns>
        public static TimeZoneInfo GetTimeZoneInfoForTzdbId(string tzdbId)
        {
            var mappings = TzdbDateTimeZoneSource.Default.WindowsMapping.MapZones;
            var map = mappings.FirstOrDefault(x =>
                x.TzdbIds.Any(z => z.Equals(tzdbId, StringComparison.OrdinalIgnoreCase)));
            return map == null ? null : TimeZoneInfo.FindSystemTimeZoneById(map.WindowsId);
        }
    }
}
