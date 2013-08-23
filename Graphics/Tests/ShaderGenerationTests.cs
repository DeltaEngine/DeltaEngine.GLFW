﻿using System.IO;
using DeltaEngine.Content;
using DeltaEngine.Core;
using DeltaEngine.Graphics.Vertices;
using NUnit.Framework;

namespace DeltaEngine.Graphics.Tests
{
	public class ShaderGenerationTests
	{
		[Test, Ignore]
		public void Create3DShaderContentFiles()
		{
			Create(ShaderWithFormat.UvVertexCode, ShaderWithFormat.UvFragmentCode,
				ShaderWithFormat.UvHlslCode, VertexFormat.Position2DUv, Shader.Position2DUv);
			Create(ShaderWithFormat.ColorVertexCode, ShaderWithFormat.ColorFragmentCode,
				ShaderWithFormat.ColorHlslCode, VertexFormat.Position2DColor, Shader.Position2DColor);
			Create(ShaderWithFormat.ColorUvVertexCode, ShaderWithFormat.ColorUvFragmentCode,
				ShaderWithFormat.ColorUvHlslCode, VertexFormat.Position2DColorUv, Shader.Position2DColorUv);
			Create(ShaderWithFormat.UvVertexCode, ShaderWithFormat.UvFragmentCode,
				ShaderWithFormat.UvHlslCode, VertexFormat.Position3DUv, Shader.Position3DUv);
			Create(ShaderWithFormat.ColorVertexCode, ShaderWithFormat.ColorFragmentCode,
				ShaderWithFormat.ColorHlslCode, VertexFormat.Position3DColor, Shader.Position3DColor);
			Create(ShaderWithFormat.ColorUvVertexCode, ShaderWithFormat.ColorUvFragmentCode,
				ShaderWithFormat.ColorUvHlslCode, VertexFormat.Position3DColorUv, Shader.Position3DColorUv);
		}

		private static void Create(string vertexCode, string fragmentCode, string hlslCode,
			VertexFormat format, string name)
		{
			var data = new ShaderCreationData(vertexCode, fragmentCode, hlslCode, format);
			using (var file = File.Create(Path.Combine("Content", name + ".deltashader")))
				BinaryDataExtensions.Save(data, new BinaryWriter(file));
		}
	}
}