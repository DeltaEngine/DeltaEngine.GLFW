using System;
using System.Collections.Generic;

namespace DeltaEngine.Extensions
{
	/// <summary>
	/// Allows to write out date values as structured iso date strings.
	/// </summary>
	public static class DateExtensions
	{
		public static string GetIsoDateTime(this DateTime dateTime)
		{
			return GetIsoDate(dateTime) + " " + GetIsoTime(dateTime);
		}

		public static string GetIsoDate(this DateTime date)
		{
			return date.Year + "-" + date.Month.ToString("00") + "-" + date.Day.ToString("00");
		}

		public static string GetIsoTime(this DateTime time)
		{
			return time.Hour.ToString("00") + ":" + time.Minute.ToString("00") + ":" +
				time.Second.ToString("00");
		}

		public static DateTime Parse(string dateString)
		{
			string trimmedDateString = dateString != null ? dateString.Trim() : "";
			if (trimmedDateString.Length == 0)
				return DateTime.MinValue;
			try
			{
				return trimmedDateString.Contains(" ")
					? GetDateTimeFromString(trimmedDateString) : TryParseDate(trimmedDateString);
			}
			catch
			{
				return DateTime.Now;
			}
		}

		private static DateTime TryParseDate(string trimmedDateString)
		{
			string[] dateParts = trimmedDateString.SplitAndTrim('.');
			if (dateParts.Length >= 3)
				return GetPureDate(dateParts[2], dateParts[1], dateParts[0]);
			dateParts = trimmedDateString.SplitAndTrim('-');
			if (dateParts.Length >= 3)
				return GetPureDate(dateParts[0], dateParts[1], dateParts[2]);
			dateParts = trimmedDateString.SplitAndTrim('/');
			return dateParts.Length >= 3
				? GetPureDate(dateParts[2], dateParts[0], dateParts[1]) : GetPureDate(dateParts[0]);
		}

		private static DateTime GetPureDate(string year, string month = "1", string day = "1")
		{
			if (year.Length < NumberOfYearDigits && day.Length < NumberOfYearDigits)
			{
				return GetDateTimeForFormattedCurrentTimeString(day, month, year);
			}
			return new DateTime(Convert.ToInt32(year), Convert.ToInt32(month), Convert.ToInt32(day));
		}

		private static DateTime GetDateTimeForFormattedCurrentTimeString(string year, string month,
			string day)
		{
			return new DateTime(Convert.ToInt32("20" + year), (int)Enum.Parse(typeof(Months), month),
				Convert.ToInt32(day));
		}

		private const int NumberOfYearDigits = 4;

		private static DateTime GetDateTimeFromString(string dateTimeString)
		{
			string[] dateAndTime = dateTimeString.Split(new[] { ' ', ':' });
			DateTime date = Parse(dateAndTime[0]);
			if (dateAndTime.Length == 5 && dateAndTime[4].Contains("PM"))
				return ConvertToTime(date, dateAndTime, 12);
			if (dateAndTime.Length >= 3)
				return ConvertToTime(date, dateAndTime);
			return date;
		}

		private static DateTime ConvertToTime(DateTime date, IList<string> dateAndTime, int addForPm = 0)
		{
			date = date.AddHours(Convert.ToInt32(dateAndTime[1]) + Convert.ToInt32(addForPm));
			date = date.AddMinutes(Convert.ToInt32(dateAndTime[2]));
			if (dateAndTime.Count >= 4)
				date = date.AddSeconds(Convert.ToInt32(dateAndTime[3].Replace("Z", "")));
			return date;
		}

		public static bool IsDateNewer(DateTime newerDate, DateTime olderDate)
		{
			return newerDate.CompareTo(olderDate) > 0;
		}

		private enum Months
		{
			Jan = 1,
			Feb = 2,
			Mar = 3,
			Apr = 4,
			May = 5,
			Jun = 6,
			Jul = 7,
			Aug = 8,
			Sep = 9,
			Oct = 10,
			Nov = 11,
			Dec = 12
		}
	}
}