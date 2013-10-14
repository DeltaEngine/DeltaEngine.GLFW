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
			programHandle = device.CreateShaderProgram(OpenGLVertexCode, OpenGLFragmentCode);
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
			lightmapTextureUniformLocation = device.GetShaderUniformLocation(programHandle, "Lightmap");
			modelViewProjectionMatrixLocation = device.GetShaderUniformLocation(programHandle,
				"ModelViewProjection");
			jointMatricesLocation = device.GetShaderUniformLocation(programHandle, "JointTransforms");
			lightPositionUniformLocation = device.GetShaderUniformLocation(programHandle, "viewPosition");
			viewPositionUniformLocation = device.GetShaderUniformLocation(programHandle, "lightPosition");
		}

		private int diffuseTextureUniformLocation;
		private int lightmapTextureUniformLocation;
		private int modelViewProjectionMatrixLocation;
		private int jointMatricesLocation;
		private int lightPositionUniformLocation;
		private int viewPositionUniformLocation;

		public override void SetModelViewProjectionMatrix(Matrix matrix)
		{
			device.SetUniformValue(modelViewProjectionMatrixLocation, matrix);
		}

		public override void SetJointMatrices(Matrix[] jointMatrices)
		{
			device.SetUniformValues(jointMatricesLocation, jointMatrices);
		}

		public override void SetDiffuseTexture(Image texture)
		{
			device.BindTexture((texture as GLFW3Image).Handle);
			device.SetUniformValue(diffuseTextureUniformLocation, 0);
		}

		public override void SetLightmapTexture(Image texture)
		{
			device.BindTexture((texture as GLFW3Image).Handle, 1);
			device.SetUniformValue(lightmapTextureUniformLocation, 1);
		}

		public override void SetLightPosition(Vector3D vector)
		{
			device.SetUniformValue(lightPositionUniformLocation, vector);
		}

		public override void SetViewPosition(Vector3D vector)
		{
			device.SetUniformValue(viewPositionUniformLocation, vector);
		}

		public override void Bind()
		{
			device.Shader = this;
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