using System;

namespace SQLite3Repository.Data
{
    public enum SQLOrderBy { ASC = 1, DESC = 2 };

    public static class Constants
    {
        //SQLite stores DateTimes as INTEGER seconds since the Unix epoch (UTC)
        public static DateTime FromUnixTime(long seconds)
        {
            return DateTime.UnixEpoch.AddSeconds(seconds);
        }
    }
}
