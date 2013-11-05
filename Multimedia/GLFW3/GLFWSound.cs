using System;
using System.Diagnostics;
using System.IO;
using DeltaEngine.Core;
using DeltaEngine.Datatypes;
using DeltaEngine.Multimedia.GLFW.Helpers;

namespace DeltaEngine.Multimedia.GLFW
{
	/// <summary>
	/// Base for GLFW implementations for loading and playing sound effects.
	/// </summary>
	public class GLFWSound : Sound
	{
		protected GLFWSound(string contentName, GLFWSoundDevice openAL)
			: base(contentName)
		{
			this.openAL = openAL;
		}

		protected GLFWSoundDevice openAL;

		protected override void LoadData(Stream fileData)
		{
			try
			{
				LoadSound(fileData);
			}
			catch (Exception ex)
			{
				Logger.Error(ex);
				if (Debugger.IsAttached)
					throw new SoundNotFoundOrAccessible(Name, ex);
			}
		}

		protected void LoadSound(Stream fileData)
		{
			var streamReader = new BinaryReader(fileData);
			soundData = new WaveSoundData(streamReader);
			length = GetLengthInSeconds();
			bufferHandle = CreateNativeBuffer();
		}

		protected WaveSoundData soundData;
		protected float length;
		protected int bufferHandle;

		protected float GetLengthInSeconds()
		{
			float blockAlign = soundData.Channels * 2f;
			return (soundData.BufferData.Length / blockAlign) / soundData.SampleRate;
		}

		protected int CreateNativeBuffer()
		{
			int newHandle = openAL.CreateBuffer();
			if (newHandle <= 0)
				throw new UnableToCreateSoundBufferGLFWMightNotBeInitializedCorrectly();
			openAL.BufferData(newHandle, soundData.Format, soundData.BufferData, 
				soundData.BufferData.Length, soundData.SampleRate);
			return newHandle;
		}

		public class UnableToCreateSoundBufferGLFWMightNotBeInitializedCorrectly : Exception { }
		
		protected override void DisposeData()
		{
			base.DisposeData();
			if(bufferHandle != InvalidHandle)
				openAL.DeleteBuffer(bufferHandle);
			bufferHandle = InvalidHandle;
		}

		protected const int InvalidHandle = -1;

		public override float LengthInSeconds
		{
			get { return length; }
		}

		public override void PlayInstance(SoundInstance instanceToPlay)
		{
			var channelHandle = (int)instanceToPlay.Handle;
			if (channelHandle == InvalidHandle)
				return;
			openAL.SetVolume(channelHandle, instanceToPlay.Volume);
			openAL.SetPosition(channelHandle, new Vector3D(instanceToPlay.Panning, 0.0f, 0.0f));
			openAL.SetPitch(channelHandle, instanceToPlay.Pitch);
			openAL.Play(channelHandle);
		}

		public override void StopInstance(SoundInstance instanceToStop)
		{
			var channelHandle = (int)instanceToStop.Handle;
			if(channelHandle == InvalidHandle)
				return;
			openAL.Stop(channelHandle);
		}

		protected override void CreateChannel(SoundInstance instanceToAdd)
		{
			var channelHandle = openAL.CreateChannel();
			if (channelHandle <= 0)
				Logger.Error(new UnableToCreateSoundChannelGLFWMightNotBeInitializedCorrectly());
			openAL.AttachBufferToChannel(bufferHandle, channelHandle);
			instanceToAdd.Handle = channelHandle;
		}

		public class UnableToCreateSoundChannelGLFWMightNotBeInitializedCorrectly : Exception { }

		protected override void RemoveChannel(SoundInstance instanceToRemove)
		{
			var channelHandle = (int)instanceToRemove.Handle;
			if(channelHandle != InvalidHandle)
				openAL.DeleteChannel(channelHandle);
			instanceToRemove.Handle = InvalidHandle;
		}

		public override bool IsPlaying(SoundInstance instanceToCheck)
		{
			var channelHandle = (int)instanceToCheck.Handle;
			return channelHandle != InvalidHandle && openAL.IsPlaying(channelHandle);
		}
	}
}