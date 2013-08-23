using System;

namespace DeltaEngine.Content
{
	/// <summary>
	/// Simple factory to provide access to create content data on demand without any resolver.
	/// Once the <see cref="Resolver"/> is started it will replace this functionality.
	/// </summary>
	public class ContentDataResolver
	{
		internal ContentDataResolver() {}

		public virtual ContentData Resolve(Type contentType, string contentName)
		{
			return Activator.CreateInstance(contentType, contentName) as ContentData;
		}

		public virtual ContentData Resolve(Type contentType, ContentCreationData data)
		{
			return Activator.CreateInstance(contentType, data) as ContentData;
		}

		public virtual void MakeSureResolverIsInitializedAndContentIsReady() {}
	}
}