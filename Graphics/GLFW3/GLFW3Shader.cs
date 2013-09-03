using System;
using DeltaEngine.Content;
using DeltaEngine.Core;
using DeltaEngine.Datatypes;

namespace DeltaEngine.Graphics.GLFW3
{
	/// <summary>
	/// All OpenGL shaders share this basic functionality.
	/// </summary>
	public class GLFW3Shader : ShaderWithFormat
	{
		protected GLFW3Shader(string contentName, GLFW3Device device)
			: base(contentName)
		{
			this.device = device;
		}

		private readonly GLFW3Device device;

		private GLFW3Shader(ShaderCreationData creationData, GLFW3Device device)
			: base(creationData)
		{
			this.device = device;
			Create();
		}

		protected override sealed void Create()
		{
			programHandle = device.CreateShaderProgram(VertexCode, PixelCode);
			if (programHandle == GLFW3Device.InvalidHandle)
				throw new UnableToCreateOpenGLShader();
			LoadAttributeLocations();
			LoadUniformLocations();
		}

		private int programHandle;

		private class UnableToCreateOpenGLShader : Exception { }

		private void LoadAttributeLocations()
		{
			attributeLocations = new int[Format.Elements.Length];
			for (int i = 0; i < Format.Elements.Length; i++)
			{
				var identifier = AttributePrefix +
					Format.Elements[i].ElementType.ToString().Replace("2D", "").Replace("3D", "");
				attributeLocations[i] = device.GetShaderAttributeLocation(programHandle, identifier);
				if (attributeLocations[i] < 0)
					Logger.Warning("Attribute " + identifier + " not found in " + this);
			}
		}

		private const string AttributePrefix = "a";
		private int[] attributeLocations;

		private void LoadUniformLocations()
		{
			diffuseTextureUniformLocation = device.GetShaderUniformLocation(programHandle, "Texture");
			modelViewProjectionMatrixLocation = device.GetShaderUniformLocation(programHandle,
				"ModelViewProjection");
		}

		private int diffuseTextureUniformLocation;
		private int modelViewProjectionMatrixLocation;

		public override void SetModelViewProjectionMatrix(Matrix matrix)
		{
			device.SetUniformValue(modelViewProjectionMatrixLocation, matrix);
		}

		public override void SetDiffuseTexture(Image texture)
		{
			device.BindTexture((texture as GLFW3Image).Handle);
			device.SetUniformValue(diffuseTextureUniformLocation, 0);
		}

		public override void Bind()
		{
			device.UseShaderProgram(programHandle);
		}

		public override void BindVertexDeclaration()
		{
			for (int i = 0; i < Format.Elements.Length; i++)
				if (Format.Elements[i].Size == Format.Elements[i].ComponentCount)
					device.DefineVertexAttributeWithBytes(attributeLocations[i],
						Format.Elements[i].ComponentCount, Format.Stride, Format.Elements[i].Offset);
				else
					device.DefineVertexAttributeWithFloats(attributeLocations[i],
						Format.Elements[i].ComponentCount, Format.Stride, Format.Elements[i].Offset);
		}

		protected override void DisposeData()
		{
			device.DeleteShaderProgram(programHandle);
		}
	}
}