using System;
using System.Globalization;
using DeltaEngine.Extensions;
using NUnit.Framework;

namespace DeltaEngine.Tests.Extensions
{
	public class DateExtensionsTests
	{
		[Test]
		public void GetIsoDateTime()
		{
			var testTime = new DateTime(2013, 11, 17, 13, 6, 1);
			Assert.AreEqual("2013-11-17 13:06:01", testTime.GetIsoDateTime());
			AssertDateTime(testTime, DateTime.Parse(testTime.GetIsoDateTime()));
		}

		private static void AssertDateTime(DateTime expectedDateTime, DateTime actualDateTime)
		{
			Assert.AreEqual(expectedDateTime.GetIsoDateTime(), actualDateTime.GetIsoDateTime());
		}

		[Test]
		public void EmptyStringJustReturnsTheSmallestDate()
		{
			Assert.AreEqual(DateTime.MinValue, DateExtensions.Parse(""));
		}

		[Test]
		public void IncorrectDateStringWillReturnCurrentDateTime()
		{
			AssertDateTime(DateTime.Now, DateExtensions.Parse("2013[08]17"));
			AssertDateTime(DateTime.Now, DateExtensions.Parse("2013[08]17 00"));
		}

		[Test]
		public void ParsePureIsoAndGermanAndEnglishDate()
		{
			const int Day = 21;
			const int Month = 8;
			const int Year = 2013;
			var expectedDate = new DateTime(Year, Month, Day);
			Assert.AreEqual(expectedDate, DateExtensions.Parse(Year + "-" + Month + "-" + Day));
			Assert.AreEqual(expectedDate, DateExtensions.Parse(Day + "." + Month + "." + Year));
			Assert.AreEqual(expectedDate, DateExtensions.Parse(Month + "/" + Day + "/" + Year));
		}

		[Test]
		public void GetDateTimeFromString()
		{
			var isoDateTime = DateExtensions.Parse("2013-08-22 22:37:46Z");
			var germanDateTime = DateExtensions.Parse("22.8.2013 22:37:46");
			var englishDateTime = DateExtensions.Parse("08/22/2013 10:37:46 PM");
			Assert.AreEqual(isoDateTime, germanDateTime);
			Assert.AreEqual(isoDateTime, englishDateTime);
			var currentTime = DateTime.Now;
			Assert.That(currentTime,
				Is.EqualTo(DateExtensions.Parse(currentTime.ToString())).Within
					(999).Milliseconds);
		}

		[Test]
		public void CheckIsDateNewer()
		{
			Assert.IsTrue(DateExtensions.IsDateNewer(DateTime.Now, DateTime.Today));
			Assert.IsTrue(DateExtensions.IsDateNewer(DateTime.Today, DateTime.MinValue));
			Assert.IsFalse(DateExtensions.IsDateNewer(DateTime.MinValue, DateTime.MaxValue));
		}
	}
}