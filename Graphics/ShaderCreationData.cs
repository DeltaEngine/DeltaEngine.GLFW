using DeltaEngine.Content;
using DeltaEngine.Graphics.Vertices;

namespace DeltaEngine.Graphics
{
	/// <summary>
	/// Creates a shader directly from vertex and fragment shader code for OpenGL frameworks plus
	/// Hlsl code for DirectX frameworks. If you only provide shader code for a specific framework, 
	/// it breaks multiplatform compatibility. Use this only for testing and use content normally. 
	/// </summary>
	public class ShaderCreationData : ContentCreationData
	{
		private ShaderCreationData() {}

		public ShaderCreationData(string vertexCode, string fragmentCode, string hlslCode,
			VertexFormat format)
		{
			VertexCode = vertexCode;
			FragmentCode = fragmentCode;
			HlslCode = hlslCode;
			Format = format;
		}

		public string VertexCode { get; private set; }
		public string FragmentCode { get; private set; }
		public string HlslCode { get; private set; }
		public VertexFormat Format { get; private set; }
	}
}