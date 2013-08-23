using System;
using System.Collections.Generic;
using System.Linq;

namespace DeltaEngine.Extensions
{
	/// <summary>
	/// Provides the number of elements in an enum.
	/// </summary>
	public static class EnumExtensions
	{
		public static Array GetEnumValues(this Enum anyEnum)
		{
			Type enumType = anyEnum.GetType();
			return Enum.GetValues(enumType);
		}

		public static IEnumerable<EnumType> GetEnumValues<EnumType>()
		{
			return from object value in Enum.GetValues(typeof(EnumType)) select (EnumType)value;
		}

		public static int GetCount(this Enum anyEnum)
		{
			return GetEnumValues(anyEnum).Length;
		}

		public static T Parse<T>(this string text)
		{
			return (T)Enum.Parse(typeof(T), text);
		}
	}
}