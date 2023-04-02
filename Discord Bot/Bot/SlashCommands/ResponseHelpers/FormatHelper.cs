using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Text.RegularExpressions;
using System.Threading.Tasks;

namespace Bot.SlashCommands.ResponseHelpers
{
    public static class FormatHelper
    {
        static public readonly Regex timeFormatChecker = new("^((\\d{1,2})h|(\\d{1,2})m|(\\d{1,2})h(\\d{1,2})m)$");
        public static bool IsInTimeFormat(string input)
        {
            return timeFormatChecker.IsMatch(input);
        }
        public static float ExtractTimeInHours(string incomingValue)
        {
            var match = timeFormatChecker.Match(incomingValue);
            if (!match.Success)
            {
                return 0;
            }

            if (match.Groups[2].Success)
            {
                return float.Parse(match.Groups[2].Value);
            }

            if (match.Groups[3].Success)
            {
                return float.Parse(match.Groups[3].Value) / 60f;
            }

            return float.Parse(match.Groups[4].Value) + float.Parse(match.Groups[5].Value) / 60f;
        }

        public static List<string> ExtractUserMentions(string input)
        {
            List<string> result = new List<string>();
            var matchCollection = Regex.Matches(input, "<(?:@|@!)\\d+>");
            foreach (Match match in matchCollection)
            {
                result.Add(match.Groups[0].Value);
            }

            return result;
        }
    }
}
