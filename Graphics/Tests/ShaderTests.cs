using DeltaEngine.Content;
using DeltaEngine.Graphics.Vertices;
using DeltaEngine.Platforms;
using NUnit.Framework;

namespace DeltaEngine.Graphics.Tests
{
	public class ShaderTests : TestWithMocksOrVisually
	{
		[Test, CloseAfterFirstFrame]
		public void LoadShaderAsContent()
		{
			var shaderColorUv = ContentLoader.Load<Shader>(Shader.Position2DColorUv) as ShaderWithFormat;
			var shaderColor = ContentLoader.Load<Shader>(Shader.Position2DColor) as ShaderWithFormat;
			var shaderUv = ContentLoader.Load<Shader>(Shader.Position2DUv) as ShaderWithFormat;
			Assert.AreEqual(Shader.Position2DColorUv, shaderColorUv.Name);
			Assert.AreEqual(VertexFormat.Position2DColorUv, shaderColorUv.Format);
			Assert.AreEqual(Shader.Position2DColor, shaderColor.Name);
			Assert.AreEqual(VertexFormat.Position2DColor, shaderColor.Format);
			Assert.AreEqual(Shader.Position2DUv, shaderUv.Name);
			Assert.AreEqual(VertexFormat.Position2DUv, shaderUv.Format);
		}
	}
}