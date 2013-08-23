﻿using System;
using System.Collections;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Reflection;
using System.Runtime.InteropServices;
using DeltaEngine.Content;
using DeltaEngine.Extensions;

namespace DeltaEngine.Core
{
	/// <summary>
	/// Loads saved binary data object and reconstructs them based on the saved type information.
	/// </summary>
	internal static class BinaryDataLoader
	{
		internal static object TryCreateAndLoad(Type typeToCreate, BinaryReader reader,
			Version dataVersion)
		{
			try
			{
				return CreateAndLoadTopLevelType(typeToCreate, reader, dataVersion);
			}
			catch (TypeLoadException ex) //ncrunch: no coverage start
			{
				throw new Exception(
					"Failed to load inner type of '" + typeToCreate + "' (Version " + dataVersion + "). " +
					"Your data might be outdated '" + subTypeToCreate + "'. Try to delete your local content",
					ex);
			} //ncrunch: no coverage end
			catch (MissingMethodException ex)
			{
				throw new MissingMethodException(
					"Failed to load '" + typeToCreate + "' (Version " + dataVersion + "). " +
						"Could not create default instance of '" + subTypeToCreate + "'", ex);
			}
			catch (Exception ex)
			{
				throw new Exception(
					"Failed to load '" + typeToCreate + "' (Version " + dataVersion + ").", ex);
			}
		}

		private static object CreateAndLoadTopLevelType(Type typeToCreate, BinaryReader reader,
			Version dataVersion)
		{
			topLevelTypeToCreate = typeToCreate;
			nestingDepth = 0;
			if (ExceptionExtensions.IsDebugMode && dataVersion != null &&
				typeToCreate.Assembly.GetName().Version != dataVersion)
				Logger.Warning("Version " + dataVersion + " of data to load is different from the type " +
					"to be created: " + typeToCreate + " (" + typeToCreate.Assembly.GetName().Version + ")");
			return CreateAndLoad(typeToCreate, reader);
		}

		private static Type topLevelTypeToCreate;
		private static int nestingDepth;

		private static object CreateAndLoad(Type typeToCreate, BinaryReader reader)
		{
			subTypeToCreate = typeToCreate;
			IncreaseNestingDepth();
			object data = GetDefault(typeToCreate);
			LoadData(ref data, typeToCreate, reader);
			nestingDepth--;
			return data;
		}

		private static Type subTypeToCreate;

		private static void IncreaseNestingDepth()
		{
			//ncrunch: no coverage start
			if (nestingDepth++ > 6)
				throw new NotSupportedException("Nesting depth got too big in '" + topLevelTypeToCreate +
					"'. Investigate the stack trace and only save types that are actually data types!");
			//ncrunch: no coverage end
		}

		private static object GetDefault(Type type)
		{
			if (type.IsArray || type == typeof(Type) || type.Name == "RuntimeType" ||
				typeof(ContentData).IsAssignableFrom(type))
				return null;
			return type == typeof(string) ? "" : Activator.CreateInstance(type, true);
		}

		private static void LoadData(ref object data, Type type, BinaryReader reader)
		{
			if (typeof(ContentData).IsAssignableFrom(type))
			{
				var contentName = reader.ReadString();
				data = ContentLoader.Load(type, contentName);
				if (!contentName.StartsWith("<Generated"))
					return;
			}
			if (type.IsEnum)
			{
				LoadPrimitiveData(ref data, type, reader);
				data = Enum.ToObject(type, data);
				return;
			}
			if (LoadPrimitiveData(ref data, type, reader))
				return;
			if (type == typeof(MemoryStream))
				data = LoadMemoryStream(reader);
			else if (type == typeof(byte[]))
				data = LoadByteArray(reader);
			else if (type == typeof(char[]))
				data = LoadCharArray(reader);
			else if (data is IList || type.IsArray)
				data = LoadArray(data as IList, type, reader);
			else if (data is IDictionary || type == typeof(Dictionary<,>))
				data = LoadDictionary(data as IDictionary, reader);
			else if (type.IsClass || type.IsValueType)
				LoadClassData(data, type, reader);
		}

		private static bool LoadPrimitiveData(ref object data, Type type, BinaryReader reader)
		{
			switch (Type.GetTypeCode(type))
			{
			case TypeCode.Boolean:
				data = reader.ReadBoolean();
				return true;
			case TypeCode.Byte:
				data = reader.ReadByte();
				return true;
			case TypeCode.Char:
				data = reader.ReadChar();
				return true;
			case TypeCode.Decimal:
				data = reader.ReadDecimal();
				return true;
			case TypeCode.Double:
				data = reader.ReadDouble();
				return true;
			case TypeCode.Single:
				data = reader.ReadSingle();
				return true;
			case TypeCode.Int16:
				data = reader.ReadInt16();
				return true;
			case TypeCode.Int32:
				data = reader.ReadInt32();
				return true;
			case TypeCode.Int64:
				data = reader.ReadInt64();
				return true;
			case TypeCode.String:
				data = reader.ReadString();
				return true;
			case TypeCode.SByte:
				data = reader.ReadSByte();
				return true;
			case TypeCode.UInt16:
				data = reader.ReadUInt16();
				return true;
			case TypeCode.UInt32:
				data = reader.ReadUInt32();
				return true;
			case TypeCode.UInt64:
				data = reader.ReadUInt64();
				return true;
			}
			return false;
		}

		private static MemoryStream LoadMemoryStream(BinaryReader reader)
		{
			int length = reader.ReadNumberMostlyBelow255();
			return new MemoryStream(reader.ReadBytes(length));
		}

		private static byte[] LoadByteArray(BinaryReader reader)
		{
			int length = reader.ReadNumberMostlyBelow255();
			return reader.ReadBytes(length);
		}

		private static char[] LoadCharArray(BinaryReader reader)
		{
			int count = reader.ReadNumberMostlyBelow255();
			return reader.ReadChars(count);
		}

		private static object LoadArray(IList list, Type arrayType, BinaryReader reader)
		{
			var count = reader.ReadNumberMostlyBelow255();
			if (list == null)
				list = Activator.CreateInstance(arrayType, new object[] { count }) as IList;
			if (count == 0)
				return list;
			var arrayElementType = (BinaryDataSaver.ArrayElementType)reader.ReadByte();
			if (arrayElementType == BinaryDataSaver.ArrayElementType.AllTypesAreNull)
				return list;
			if (arrayElementType == BinaryDataSaver.ArrayElementType.AllTypesAreDifferent)
				LoadArrayWhereAllElementsAreNotTheSameType(list, arrayType, reader, count);
			else
			{
				Type elementType = arrayElementType == BinaryDataSaver.ArrayElementType.AllTypesAreTypes
					? typeof(Type) : reader.ReadString().GetTypeFromShortNameOrFullNameIfNotFound();
				LoadArrayWhereAllElementsAreTheSameType(list, arrayType, reader, count, elementType);
			}
			return list;
		}

		internal static int ReadNumberMostlyBelow255(this BinaryReader reader)
		{
			int number = reader.ReadByte();
			if (number == 255)
				number = reader.ReadInt32();
			return number;
		}

		private static void LoadArrayWhereAllElementsAreTheSameType(IList list, Type arrayType,
			BinaryReader reader, int count, Type elementType)
		{
			if (!arrayType.IsArray)
				list.Clear();
			for (int i = 0; i < count; i++)
				if (arrayType.IsArray)
					list[i] = CreateAndLoad(elementType, reader);
				else
					list.Add(CreateAndLoad(elementType, reader));
		}

		private static void LoadArrayWhereAllElementsAreNotTheSameType(IList list, Type arrayType,
			BinaryReader reader, int count)
		{
			if (arrayType.IsArray)
				LoadArrayWithItsTypes(list, reader, count);
			else
				LoadListWithItsTypes(list, reader, count);
		}

		private static void LoadArrayWithItsTypes(IList list, BinaryReader reader, int count)
		{
			for (int i = 0; i < count; i++)
				LoadArrayElement(list, reader, i);
		}

		private static void LoadArrayElement(IList list, BinaryReader reader, int i)
		{
			Type elementType = reader.ReadString().GetTypeFromShortNameOrFullNameIfNotFound();
			bool isNotNull = reader.ReadBoolean();
			if (isNotNull)
				list[i] = CreateAndLoad(elementType, reader);
		}

		private static void LoadListWithItsTypes(IList list, BinaryReader reader, int count)
		{
			list.Clear();
			for (int i = 0; i < count; i++)
				LoadListElement(list, reader);
		}

		private static void LoadListElement(IList list, BinaryReader reader)
		{
			Type elementType = reader.ReadString().GetTypeFromShortNameOrFullNameIfNotFound();
			bool isNotNull = reader.ReadBoolean();
			if (isNotNull)
				list.Add(CreateAndLoad(elementType, reader));
		}

		private static IDictionary LoadDictionary(IDictionary data, BinaryReader reader)
		{
			var count = reader.ReadNumberMostlyBelow255();
			data.Clear();
			if (count == 0)
				return data;
			var valuesType = (BinaryDataSaver.ArrayElementType)reader.ReadByte();
			if (valuesType == BinaryDataSaver.ArrayElementType.AllTypesAreDifferent)
				LoadDictionaryWhereAllValuesAreNotTheSameType(data, reader, count);
			else
				LoadDictionaryWhereAllValuesAreTheSameType(data, reader, count);
			return data;
		}

		private static void LoadDictionaryWhereAllValuesAreNotTheSameType(IDictionary data,
			BinaryReader reader, int count)
		{
			Type everyKeyType = reader.ReadString().GetTypeFromShortNameOrFullNameIfNotFound();
			for (int i = 0; i < count; i++)
			{
				var thisValueType = reader.ReadString().GetTypeFromShortNameOrFullNameIfNotFound();
				data.Add(CreateAndLoad(everyKeyType, reader), CreateAndLoad(thisValueType, reader));
			}
		}

		private static void LoadDictionaryWhereAllValuesAreTheSameType(IDictionary data,
			BinaryReader reader, int count)
		{
			Type everyKeyType = reader.ReadString().GetTypeFromShortNameOrFullNameIfNotFound();
			Type everyValueType = reader.ReadString().GetTypeFromShortNameOrFullNameIfNotFound();
			for (int i = 0; i < count; i++)
				data.Add(CreateAndLoad(everyKeyType, reader), CreateAndLoad(everyValueType, reader));
		}

		private static void LoadClassData(object data, Type type, BinaryReader reader)
		{
			foreach (FieldInfo field in GetBackingFields(type))
			{
				Type fieldType = field.FieldType;
				if (fieldType.DoNotNeedToSaveType() || fieldType == type)
					continue;
				if (field.FieldType.IsClass)
				{
					bool isNull = !reader.ReadBoolean();
					if (isNull)
						continue;
					if (fieldType.NeedToSaveTypeName())
						fieldType = reader.ReadString().GetTypeFromShortNameOrFullNameIfNotFound();
				}
				field.SetValue(data, CreateAndLoad(fieldType, reader));
			}
		}

		internal static IEnumerable<FieldInfo> GetBackingFields(this Type type)
		{
			FieldInfo[] fields =
				type.GetFields(BindingFlags.Instance | BindingFlags.Public | BindingFlags.NonPublic);
			if (!type.IsValueType && type.BaseType != null && type.BaseType != typeof(object))
				return CombineFields(GetBackingFields(type.BaseType), fields);
			if (!type.IsExplicitLayout)
				return fields;
			var offsets = new List<int>();
			var nonDuplicateFields = new List<FieldInfo>();
			foreach (FieldInfo field in fields)
			{
				object[] offsetAttributes = field.GetCustomAttributes(typeof(FieldOffsetAttribute), false);
				int offsetValue = (offsetAttributes[0] as FieldOffsetAttribute).Value;
				if (offsets.Contains(offsetValue))
					continue;
				nonDuplicateFields.Add(field);
				offsets.Add(offsetValue);
			}
			return nonDuplicateFields;
		}

		private static IEnumerable<FieldInfo> CombineFields(IEnumerable<FieldInfo> baseBackingFields,
			IEnumerable<FieldInfo> fields)
		{
			var combinedFields = new List<FieldInfo>(baseBackingFields);
			foreach (var field in fields)
				if (combinedFields.All(existingField => existingField.Name != field.Name))
					combinedFields.Add(field);
			return combinedFields;
		}
	}
}