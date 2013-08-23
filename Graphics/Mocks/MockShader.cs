using System;
using DeltaEngine.Content;
using DeltaEngine.Datatypes;

namespace DeltaEngine.Graphics.Mocks
{
	public class MockShader : ShaderWithFormat
	{
		public MockShader(string contentName, Device device)
			: base(contentName)
		{
			if (device == null)
				throw new NeedDeviceForShaderCreation();
		}

		public class NeedDeviceForShaderCreation : Exception { }

		private MockShader(ShaderCreationData customShader, Device device)
			: base(customShader)
		{
			if (device == null)
				throw new NeedDeviceForShaderCreation();
		}

		protected override void DisposeData() { }
		public override void SetModelViewProjectionMatrix(Matrix matrix) { }
		public override void SetDiffuseTexture(Image texture) { }
		public override void Bind() { }
		protected override void Create() { }
	}
}