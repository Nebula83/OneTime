using System;
using System.Collections.Generic;
using System.Globalization;
using System.Linq;
using System.Text;
using System.Text.RegularExpressions;

namespace Nebula.OneTime.TimesheetModels
{
    internal class AltenTimeSheet
    {
        public string Employee { get; set; }
        public List<int> Days { get; set; }
        public DateTime StartDate { get; set; }
        public DateTime EndDate { get; set; }
        public List<double> WorkingHours { get; set; }
        public string Project { get; set; }

        public AltenTimeSheet(string pdfString)
        {
            ParseString(pdfString);
        }

        private void ParseString(string pdfString)
        {
            bool outOfPeriod = pdfString.Contains("Out of period");

            var lines = pdfString.Replace("\r", "").Split('\n');

            Employee = Regex.Matches(lines[2], @"^ *Werknemer *(.*)")[0].Groups[1].ToString();

            var dates = Regex.Matches(lines[3], @"\d{2}/\d{2}/\d{4}").Cast<Match>().Select(m => m.Value).ToList();
            StartDate = DateTime.ParseExact(dates[0], "dd/MM/yyyy", null);
            EndDate = DateTime.ParseExact(dates[1], "dd/MM/yyyy", null);

            var indexDays = outOfPeriod ? 7 : 5;
            Days = Regex.Matches(lines[indexDays], @"(\d{1,2})").Cast<Match>().Select(m => int.Parse(m.Value)).ToList();

            var indexHours = outOfPeriod ? 10 : 8;
            WorkingHours = new List<double>(Days.Count);
            var hours = Regex.Matches(lines[indexHours], @"(--|\d,\d{2})").Cast<Match>().Select(m =>
            {
                double.TryParse(m.Value,
                    NumberStyles.Any, CultureInfo.CreateSpecificCulture("nl-NL"), out var result);
                return result;
            }).ToList();

            var currentDay = StartDate;
            using (var hour = hours.GetEnumerator())
            {
                foreach (var day in Days)
                {
                    if (currentDay.Day != day) { throw new Exception("Day match error"); }
                    if (currentDay.DayOfWeek != DayOfWeek.Saturday && currentDay.DayOfWeek != DayOfWeek.Sunday)
                    {
                        hour.MoveNext();
                        WorkingHours.Add(hour.Current);
                    }
                    else
                    {
                        WorkingHours.Add(0);
                    }
                    currentDay = currentDay.AddDays(1);
                }
            }

            Project = lines[7].Trim();
        }

        public override string ToString()
        {
            var sb = new StringBuilder();
            sb.AppendLine($"{Employee} {Project}");
            sb.AppendLine($"{StartDate.ToShortDateString()} - {EndDate.ToShortDateString()}");
            foreach (var day in Days)
            {
                sb.Append($"{day}".PadRight(4));
            }
            sb.AppendLine();
            foreach (var hour in WorkingHours)
            {
                sb.Append($"{hour}".PadRight(4));
            }
            sb.AppendLine();

            return sb.ToString();
        }
    }
}
