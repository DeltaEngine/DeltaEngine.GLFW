using System.IO;

namespace DeltaEngine.Multimedia.GLFW.Helpers
{
	/// <summary>
	/// Audio data converter capable of converting IEEE floating point audio to pcm 16 bit.
	/// </summary>
	internal struct IeeeFloatConverter
	{
		public IeeeFloatConverter(int bitsPerSample)
		{
			is64BitFloat = bitsPerSample == 64;
		}

		private readonly bool is64BitFloat;

		public byte[] ConvertToPcm(byte[] sourceData)
		{
			byte[] result;
			using (var resultStream = new MemoryStream())
			{
				var writer = new BinaryWriter(resultStream);
				var reader = new BinaryReader(new MemoryStream(sourceData));
				ReadAllSamples(reader, writer, sourceData.Length);
				result = resultStream.ToArray();
			}

			return result;
		}

		private void ReadAllSamples(BinaryReader reader, BinaryWriter writer, int sourceLength)
		{
			int length = sourceLength / (is64BitFloat ? 8 : 4);
			for (int index = 0; index < length; index++)
			{
				double value = is64BitFloat ? reader.ReadDouble() : reader.ReadSingle();
				writer.Write((short)(value * 32767));
			}
		}
	}
}
