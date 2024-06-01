using System.Globalization;
using System.Text;
using System.Text.RegularExpressions;

namespace AlgoLibrary
{
    public class TextAlgos
    {
        /// <summary>
        /// return the Levenshtein distance (INT) between two strings
        /// distance = 0 : perfect match
        /// operations: insertion, deletion, substitution
        /// > https://en.wikipedia.org/wiki/Levenshtein_distance
        /// </summary>
        public static int LevenshteinDistance(string s, string t)
        {
            int[,] d = new int[s.Length + 1, t.Length + 1];

            for (int i = 0; i <= s.Length; i++)
            {
                d[i, 0] = i;
            }

            for (int j = 0; j <= t.Length; j++)
            {
                d[0, j] = j;
            }

            for (int j = 1; j <= t.Length; j++)
            {
                for (int i = 1; i <= s.Length; i++)
                {
                    int cost = (s[i - 1] == t[j - 1]) ? 0 : 1;

                    d[i, j] = Math.Min(Math.Min(
                        d[i - 1, j] + 1,    // deletion
                        d[i, j - 1] + 1),   // insertion
                        d[i - 1, j - 1] + cost); // substitution
                }
            }

            return d[s.Length, t.Length];
        }

        /// <summary>
        /// Returns KV<int, string> with the best (lowest levenshtein) match from list of candidates
        /// 0 : perfect match
        /// null : no match (no candidates)
        /// </summary>
        public static KeyValuePair<int, string>? GetBestMatch(string input, List<string> candidates)
        {
            int minDistance = int.MaxValue;
            string? bestMatch = null;
            foreach (var candidate in candidates)
            {
                if (candidate == null || candidate.Length == 0) continue;
                int distance = LevenshteinDistance(input, candidate);
                if (distance < minDistance)
                {
                    minDistance = distance;
                    bestMatch = candidate;
                }
            }
            if (bestMatch == null) return null;
            return new KeyValuePair<int, string>(minDistance, bestMatch);
        }

        /// <summary>
        /// Returns "clean" unicode, lowercase string
        /// Replaces special characters with their base character
        /// Removes double spaces, ., ' - _ / and trims
        /// </summary>
        public static string CleanString(string stIn)
        {
            stIn = stIn.ToLower().Replace("-", " ").Replace("/", " ").Replace("_", " ").Replace(".", "").Replace(",", "").Replace("'", "").Replace("   ", " ").Replace("  ", " ").Trim();
            string stFormD = stIn.Normalize(NormalizationForm.FormD);
            StringBuilder sb = new StringBuilder();
            for (int ich = 0; ich < stFormD.Length; ich++)
            {
                if (stFormD[ich] == 'ø' || stFormD[ich] == 'ó') { sb.Append("o"); continue; }
                if (stFormD[ich] == 'è' || stFormD[ich] == 'é') { sb.Append("e"); continue; }
                if (stFormD[ich] == 'í') { sb.Append("i"); continue; }
                if (stFormD[ich] == 'ç') { sb.Append("c"); continue; }
                if (stFormD[ich] == 'â') { sb.Append("a"); continue; }
                UnicodeCategory uc = CharUnicodeInfo.GetUnicodeCategory(stFormD[ich]);
                if (uc != UnicodeCategory.NonSpacingMark) sb.Append(stFormD[ich]);
            }
            return (sb.ToString().Normalize(NormalizationForm.FormC).Trim());
        }

        private static Regex regexDateSpaceTime = new Regex(@"^(\d+)\/(\d+).{1,3}(\d{1,2}):(\d\d)$", RegexOptions.IgnoreCase | RegexOptions.Compiled); // matches 30/04 - 19:00

        private static Regex regexTimeToday = new Regex(@"^(\d{1,2}):(\d\d)$", RegexOptions.IgnoreCase | RegexOptions.Compiled); // matches 19:00

        private static Regex regexMonthDayTime = new Regex(@"^([A-z]+) (\d{1,2}) *\d{1,2}:\d\d$", RegexOptions.IgnoreCase | RegexOptions.Compiled); // matches april 30 19:00

        private static Regex regexDayNLTime = new Regex(@"^(ma|di|wo|do|vr|za|zo|mo|tu|we|th|fr|sa|su).{0,1}\d{2}:\d\d$", RegexOptions.IgnoreCase | RegexOptions.Compiled); // matches ma 19:00

        private static Regex regexYYYYMMDD = new Regex(@"^(\d{4}).(\d{1,2}).(\d{1,2})", RegexOptions.Compiled); // matches 2024-04-30T19:00:00Z

        /// <summary>
        /// Returns a DateOnly object from a string
        /// Best attempt to fetch nearby (future) date from string in NL or EN
        /// Default: returns todays DateOnly
        /// </summary>
        /// <param name="date_str"></param>
        /// <returns></returns>
        public static DateOnly GetDateFromStr_DefaultToday(string date_str)
        {
            string datestr = date_str.ToLower();

            Match dateSpaceTimeMatch = regexDateSpaceTime.Match(datestr);
            if (dateSpaceTimeMatch.Success)
            {
                return new DateOnly(DateTime.Now.Year, int.Parse(dateSpaceTimeMatch.Groups[2].Value), int.Parse(dateSpaceTimeMatch.Groups[1].Value));
            }

            Match timeTodayMatch = regexTimeToday.Match(datestr); // matches xx:yy (todays time)
            if (timeTodayMatch.Success)
            {
                if (DateTime.Now.Hour >= int.Parse(timeTodayMatch.Groups[1].Value))
                {
                    return new DateOnly(DateTime.Now.Year, DateTime.Now.Month, DateTime.Now.Day).AddDays(1);
                }
                return new DateOnly(DateTime.Now.Year, DateTime.Now.Month, DateTime.Now.Day);
            }

            Match yyyymmddMatch = regexYYYYMMDD.Match(datestr);
            if (yyyymmddMatch.Success)
            {
                return new DateOnly(int.Parse(yyyymmddMatch.Groups[1].Value), int.Parse(yyyymmddMatch.Groups[2].Value), int.Parse(yyyymmddMatch.Groups[3].Value));
            }

            Match dayNLTimeMatch = regexDayNLTime.Match(datestr);
            DayOfWeek currentDayOfWeek = DateTime.Today.DayOfWeek; // sunday = 0, monday = 1, etc.
            if (dayNLTimeMatch.Success)
            {
                int daysToAdd = 0;
                switch (dayNLTimeMatch.Groups[1].Value.ToLower())
                {
                    case "ma":
                    case "mo": // monday : DayOfWeek = 1
                        daysToAdd = (1 - (int)currentDayOfWeek + 7) % 7;
                        break;
                    case "di":
                    case "tu": // tuesday : DayOfWeek = 2
                        daysToAdd = (2 - (int)currentDayOfWeek + 7) % 7;
                        break;
                    case "wo":
                    case "we": // wednesday : DayOfWeek = 3
                        daysToAdd = (3 - (int)currentDayOfWeek + 7) % 7;
                        break;
                    case "do":
                    case "th": // thursday : DayOfWeek = 4
                        daysToAdd = (4 - (int)currentDayOfWeek + 7) % 7;
                        break;
                    case "vr":
                    case "fr": // friday : DayOfWeek = 5
                        daysToAdd = (5 - (int)currentDayOfWeek + 7) % 7;
                        break;
                    case "za":
                    case "sa": // saturday : DayOfWeek = 6
                        daysToAdd = (6 - (int)currentDayOfWeek + 7) % 7;
                        break;
                    case "zo":
                    case "su": // sunday : DayOfWeek = 0
                        daysToAdd = (7 - (int)currentDayOfWeek + 7) % 7;
                        break;
                }
                return new DateOnly(DateTime.Now.Year, DateTime.Now.Month, DateTime.Now.Day).AddDays(daysToAdd);
            }

            if (datestr.StartsWith("vandaag") || datestr.StartsWith("today"))
            {
                return new DateOnly(DateTime.Now.Year, DateTime.Now.Month, DateTime.Now.Day);
            }

            if (datestr.StartsWith("morgen") || datestr.StartsWith("tomorrow"))
            {
                return new DateOnly(DateTime.Now.Year, DateTime.Now.Month, DateTime.Now.Day).AddDays(1);
            }

            Match monthDayTimeMatch = regexMonthDayTime.Match(datestr);
            if (monthDayTimeMatch.Success)
            {
                try
                {
                    DateTime datetime = DateTime.ParseExact($"{monthDayTimeMatch.Groups[1]} {monthDayTimeMatch.Groups[2]}", "MMMM d", System.Globalization.CultureInfo.InvariantCulture);
                    return new DateOnly(DateTime.Now.Year, datetime.Month, datetime.Day);
                }
                catch (FormatException)
                {
                    Console.WriteLine($"DEBUG: Date format unknown: {datestr} ; returned todays date");
                    return new DateOnly(DateTime.Now.Year, DateTime.Now.Month, DateTime.Now.Day);
                }
            }

            DateOnly[] weekDates = new DateOnly[7];
            DateOnly today = new DateOnly(DateTime.Now.Year, DateTime.Now.Month, DateTime.Now.Day);

            for (int i = 0; i < 7; i++)
            {
                DateOnly weekday = today.AddDays(i);
                weekDates[i] = weekday;
            }

            string[] months_nl = { "jan", "feb", "maa", "apr", "mei", "jun", "jul", "aug", "sep", "okt", "nov", "dec" };

            int date_month = today.Month;
            int date_day = today.Day;

            string[] datestr_split = datestr.ToLower().Trim('.').Trim().Split(" ");
            if (datestr_split.Length == 2)
            {
                if (months_nl.Contains(datestr_split[1]))
                {
                    date_month = Array.IndexOf(months_nl, datestr_split[1].ToLower()) + 1;
                }
                int.TryParse(datestr_split[0], out date_day);
            }

            switch (datestr)
            {
                case "today":
                case "vandaag":
                case "niet begonnen":
                    return today;
                case "morgen":
                case "tomorrow":
                    return today.AddDays(1);
                default:
                    break;
            }

            char[] splitChars = { '/', '.' };
            datestr_split = datestr.Trim().Trim('.').Split(splitChars);
            if (datestr_split.Length >= 2)
            {
                string dateStr = datestr_split[0].Trim(',');
                string monthStr = datestr_split[1].Trim(',');

                if (datestr.Length > 2) dateStr = datestr.Substring(0, 2);
                if (monthStr.Length > 2) monthStr = datestr.Substring(0, 2);

                int.TryParse(dateStr, out date_day);
                int.TryParse(monthStr, out date_month);
            }

            try
            {
                return new DateOnly(today.Year, date_month, date_day);
            }
            catch (System.ArgumentOutOfRangeException)
            {
                Console.WriteLine($"DEBUG: Date format unknown: {datestr} ; returned todays date");
                return new DateOnly(today.Year, today.Month, today.Day);
            }
        }

        public static class TeamNames
        {
            private static Regex regexYouthFB = new Regex(@"^(jong|jeugd|youth)|(?:(?:(?:U|O)+(19|21|23))|(B|reserves))$", RegexOptions.IgnoreCase | RegexOptions.Compiled);

            private static Regex regexWomenFB = new Regex(@"(women|\(D\)|\(W\)|(\[W\]))$", RegexOptions.IgnoreCase | RegexOptions.Compiled);
            /// <summary>
            /// Normalise female (football) team name & lowercase
            /// output: fc xx [w]
            /// </summary>
            /// <param name="teamName">eg FC XX Women (W) [W]</param>
            /// <returns></returns>
            public static string NormalizeWomenFB(string teamName)
            {
                return regexWomenFB.Replace(teamName, "[w]").Trim().ToLower();
            }

            /// <summary>
            /// Normalise youth (football) team name & lowercase
            /// input: FC XX U19,U21,U23, youth XX, XX reserves, etc.
            /// output : fc uxx or fc II
            /// </summary>
            /// <param name="youthClass">out int eg: 19, 21, 23</param>
            /// <param name="teamName">eg fc XX U19</param>
            public static string NormalizeYouthFB(string teamName, out int? youthClass)
            {
                youthClass = null;
                Match reYouthMatch = regexYouthFB.Match(teamName);

                if (reYouthMatch.Groups[2].Success)
                {
                    youthClass = int.Parse(reYouthMatch.Groups[2].Value);
                    return regexYouthFB.Replace(teamName, "").Trim().ToLower() + " u" + youthClass.ToString();
                }
                else if (reYouthMatch.Groups[1].Success || reYouthMatch.Groups[3].Success)
                {
                    return regexYouthFB.Replace(teamName, "").Trim().ToLower() + " ii";
                 }
                if (youthClass != null)
                {
                    return regexYouthFB.Replace(teamName, "").Trim().ToLower() + " u" + youthClass.ToString();
                }
                return teamName.Trim().ToLower();
            }
        }
    }
}
