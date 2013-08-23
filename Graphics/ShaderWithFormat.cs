using System;
using System.IO;
using DeltaEngine.Content;
using DeltaEngine.Core;
using DeltaEngine.Graphics.Vertices;

namespace DeltaEngine.Graphics
{
	/// <summary>
	/// Adds graphics specific features to a shader object like VertexFormat and the shader code.
	/// </summary>
	public abstract class ShaderWithFormat : Shader
	{
		protected ShaderWithFormat(string contentName)
			: base(contentName) {}

		protected ShaderWithFormat(ShaderCreationData creationData)
			: base("<GeneratedShader>")
		{
			Initialize(creationData);
		}

		protected void Initialize(ShaderCreationData data)
		{
			if (data == null)
				throw new UnableToCreateShaderNoCreationDataWasGiven();
			if (data.Format == null || data.Format.Elements.Length == 0)
				throw new UnableToCreateShaderWithoutValidVertexFormat();
			if (string.IsNullOrEmpty(data.VertexCode) || string.IsNullOrEmpty(data.FragmentCode))
				throw new UnableToCreateShaderWithoutValidVertexAndPixelCode();
			Format = data.Format;
			VertexCode = data.VertexCode;
			PixelCode = data.FragmentCode;
		}

		private class UnableToCreateShaderNoCreationDataWasGiven : NullReferenceException { }
		private class UnableToCreateShaderWithoutValidVertexFormat : Exception { }
		private class UnableToCreateShaderWithoutValidVertexAndPixelCode : Exception { }

		public VertexFormat Format { get; private set; }
		protected string VertexCode { get; private set; }
		protected string PixelCode { get; private set; }

		protected override void LoadData(Stream fileData)
		{
			if (fileData.Length == 0)
				throw new EmptyShaderFileGiven();
			var data = new BinaryReader(fileData).Create();
			Initialize(data as ShaderCreationData);
			Create();
		}

		public class EmptyShaderFileGiven : Exception {}

		protected abstract void Create();

		protected override bool AllowCreationIfContentNotFound
		{
			get
			{
				return Name == Shader.Position2DUv || Name == Shader.Position3DUv ||
					Name == Shader.Position2DColor || Name == Shader.Position3DColor ||
					Name == Shader.Position2DColorUv || Name == Shader.Position3DColorUv;
			}
		}

		protected override void CreateDefault()
		{
			switch (Name)
			{
			case Shader.Position2DUv:
				Initialize(new ShaderCreationData(UvVertexCode, UvFragmentCode, UvHlslCode,
					VertexFormat.Position2DUv));
				break;
			case Shader.Position2DColor:
				Initialize(new ShaderCreationData(ColorVertexCode, ColorFragmentCode, ColorHlslCode,
					VertexFormat.Position2DColor));
				break;
			case Shader.Position2DColorUv:
				Initialize(new ShaderCreationData(ColorUvVertexCode, ColorUvFragmentCode, ColorUvHlslCode,
					VertexFormat.Position2DColorUv));
				break;
			case Shader.Position3DUv:
				Initialize(new ShaderCreationData(UvVertexCode, UvFragmentCode, UvHlslCode,
					VertexFormat.Position3DUv));
				break;
			case Shader.Position3DColor:
				Initialize(new ShaderCreationData(ColorVertexCode, ColorFragmentCode, ColorHlslCode,
					VertexFormat.Position3DColor));
				break;
			case Shader.Position3DColorUv:
				Initialize(new ShaderCreationData(ColorUvVertexCode, ColorUvFragmentCode, ColorUvHlslCode,
					VertexFormat.Position3DColorUv));
				break;
			}
			Create();
		}

		internal const string UvVertexCode =
@"uniform mat4 ModelViewProjection;
attribute vec4 aPosition;
attribute vec2 aTextureUV;
varying vec2 vTexcoord;
void main()
{
	vTexcoord = aTextureUV;
	gl_Position = ModelViewProjection * aPosition;
}";

		internal const string UvFragmentCode =
@"precision mediump float;
uniform sampler2D Texture;
varying vec2 vTexcoord;
void main()
{
	gl_FragColor = texture2D(Texture, vTexcoord);
}";

		internal const string ColorVertexCode =
@"uniform mat4 ModelViewProjection;
attribute vec4 aPosition;
attribute vec4 aColor;
varying vec4 diffuseColor;
void main()
{
	gl_Position = ModelViewProjection * aPosition;
	diffuseColor = aColor;
}";

		internal const string ColorFragmentCode =
@"precision mediump float;
varying vec4 diffuseColor;
void main()
{
	gl_FragColor = diffuseColor;
}";

		internal const string ColorUvVertexCode =
@"uniform mat4 ModelViewProjection;
attribute vec4 aPosition;
attribute vec4 aColor;
attribute vec2 aTextureUV;
varying vec4 diffuseColor;
varying vec2 diffuseTexCoord;
void main()
{
	gl_Position = ModelViewProjection * aPosition;
	diffuseColor = aColor;
	diffuseTexCoord = aTextureUV;
}";

		internal const string ColorUvFragmentCode =
@"precision mediump float;
uniform sampler2D Texture;
varying vec4 diffuseColor;
varying vec2 diffuseTexCoord;
void main()
{
	gl_FragColor = texture2D(Texture, diffuseTexCoord) * diffuseColor;
}";

		internal const string UvHlslCode = @"
struct VertexInputType
{
	float4 position : SV_POSITION;
	float2 texCoord : TEXCOORD;
};

struct PixelInputType
{
	float4 position : SV_POSITION;
	float2 texCoord : TEXCOORD;
};

float4x4 WorldViewProjection;

PixelInputType VsMain(VertexInputType input)
{
	PixelInputType output;
	output.position = mul(input.position, WorldViewProjection);
	output.texCoord = input.texCoord;
	return output;
}

Texture2D DiffuseTexture;

SamplerState TextureSamplerState
{
	Filter = MIN_MAG_MIP_LINEAR;
	AddressU = Wrap;
	AddressV = Wrap;
};

float4 PsMain(PixelInputType input) : SV_TARGET
{
	return DiffuseTexture.Sample(TextureSamplerState, input.texCoord);
}";

		internal const string ColorHlslCode = @"
struct VertexInputType
{
	float4 position : SV_POSITION;
	float4 color : COLOR;
};

struct PixelInputType
{
	float4 position : SV_POSITION;
	float4 color : COLOR;
};

float4x4 WorldViewProjection;

PixelInputType VsMain(VertexInputType input)
{
	PixelInputType output;
	output.position = mul(input.position, WorldViewProjection);
	output.color = input.color;
	return output;
}

float4 PsMain(PixelInputType input) : SV_TARGET
{
	return input.color;
}";

		internal const string ColorUvHlslCode = @"
struct VertexInputType
{
	float4 position : SV_POSITION;
	float4 color : COLOR;
	float2 texCoord : TEXCOORD;
};

struct PixelInputType
{
	float4 position : SV_POSITION;
	float4 color : COLOR;
	float2 texCoord : TEXCOORD;
};

float4x4 WorldViewProjection;

PixelInputType VsMain(VertexInputType input)
{
	PixelInputType output;
	output.position = mul(input.position, WorldViewProjection);
	output.color = input.color;
	output.texCoord = input.texCoord;
	return output;
}

Texture2D DiffuseTexture;

SamplerState TextureSamplerState
{
	Filter = MIN_MAG_MIP_LINEAR;
	AddressU = Wrap;
	AddressV = Wrap;
};

float4 PsMain(PixelInputType input) : SV_TARGET
{
	return DiffuseTexture.Sample(TextureSamplerState, input.texCoord) * input.color;
}";
	}
}