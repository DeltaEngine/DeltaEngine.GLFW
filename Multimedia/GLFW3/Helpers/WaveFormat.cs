namespace DeltaEngine.Multimedia.GLFW.Helpers
{
	/// <summary>
	/// All wave formats we're currently able to parse which is read from the fmt chunk.
	/// </summary>
	internal enum WaveFormat
	{
		Pcm = 1,
		MsAdpcm = 2,
		IeeeFloat = 3,
		WaveFormatExtensible = 0xFFFE,
	}
}
