using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Globalization;
using System.IO;
using System.Linq;
using System.Reflection;
using Autofac;
using Autofac.Builder;
using Autofac.Core;
using Autofac.Core.Activators.Reflection;
using DeltaEngine.Content;
using DeltaEngine.Core;
using DeltaEngine.Entities;
using DeltaEngine.Extensions;
using DeltaEngine.Graphics;
using DeltaEngine.ScreenSpaces;

namespace DeltaEngine.Platforms
{
	/// <summary>
	/// Basic resolver functionality via Autofac, each configuration registers concrete types. For
	/// example GLFW uses GLFWGraphics, GLFWSound, GLFWKeyboard, etc. and makes them available.
	/// </summary>
	public abstract class Resolver : IDisposable
	{
		public void Register<T>()
		{
			Register(typeof(T));
		}

		public void Register(Type typeToRegister)
		{
			if (alreadyRegisteredTypes.Contains(typeToRegister))
				return;
			if (IsAlreadyInitialized)
				throw new UnableToRegisterMoreTypesAppAlreadyStarted();
			RegisterNonConcreteBaseTypes(typeToRegister, RegisterType(typeToRegister));
		}

		protected readonly List<Type> alreadyRegisteredTypes = new List<Type>();

		protected class UnableToRegisterMoreTypesAppAlreadyStarted : Exception {}

		public void RegisterSingleton<T>()
		{
			RegisterSingleton(typeof(T));
		}

		public void RegisterSingleton(Type typeToRegister)
		{
			if (alreadyRegisteredTypes.Contains(typeToRegister))
				return;
			if (IsAlreadyInitialized)
				throw new UnableToRegisterMoreTypesAppAlreadyStarted();
			RegisterNonConcreteBaseTypes(typeToRegister,
				RegisterType(typeToRegister).InstancePerLifetimeScope());
		}

		private
			IRegistrationBuilder<object, ConcreteReflectionActivatorData, SingleRegistrationStyle>
			RegisterType(Type t)
		{
			AddRegisteredType(t);
			if (typeof(ContentData).IsAssignableFrom(t))
				return builder.RegisterType(t).AsSelf().UsingConstructor(new ContentConstructorSelector());
			return builder.RegisterType(t).AsSelf();
		}

		private class ContentConstructorSelector : IConstructorSelector
		{
			public ConstructorParameterBinding SelectConstructorBinding(
				ConstructorParameterBinding[] constructorBindings)
			{
				foreach (var constructor in constructorBindings)
				{
					var parameters = constructor.TargetConstructor.GetParameters();
					if (parameters.Length > 0 && parameters[0].ParameterType == typeof(string))
						return constructor;
				}
				return constructorBindings.First();
			}
		}

		private void AddRegisteredType(Type type)
		{
			if (type.Namespace.StartsWith("System") || type.Namespace.StartsWith("Microsoft"))
				return;
			if (!alreadyRegisteredTypes.Contains(type))
			{
				alreadyRegisteredTypes.Add(type);
				return;
			}
			if (ExceptionExtensions.IsDebugMode && !type.IsInterface && !type.IsAbstract)
				Console.WriteLine("Warning: Type " + type + " already exists in alreadyRegisteredTypes");
		}

		private ContainerBuilder builder = new ContainerBuilder();

		protected internal void RegisterInstance(object instance)
		{
			var registration =
				builder.RegisterInstance(instance).SingleInstance().AsSelf().AsImplementedInterfaces();
			AddRegisteredType(instance.GetType());
			RegisterAllBaseTypes(instance.GetType().BaseType, registration);
		}

		private void RegisterAllBaseTypes(Type baseType,
			IRegistrationBuilder<object, SimpleActivatorData, SingleRegistrationStyle> registration)
		{
			while (baseType != null && baseType != typeof(object))
			{
				AddRegisteredType(baseType);
				registration.As(baseType);
				baseType = baseType.BaseType;
			}
		}

		private void RegisterNonConcreteBaseTypes(Type typeToRegister,
			IRegistrationBuilder<object, ConcreteReflectionActivatorData, SingleRegistrationStyle>
				registration)
		{
			foreach (var type in typeToRegister.GetInterfaces())
				AddRegisteredType(type);
			registration.AsImplementedInterfaces();
			var baseType = typeToRegister.BaseType;
			while (baseType != null && baseType != typeof(object))
			{
				if (baseType.IsAbstract)
				{
					AddRegisteredType(baseType);
					registration.As(baseType);
				}
				baseType = baseType.BaseType;
			}
		}

		public BaseType Resolve<BaseType>() where BaseType : class
		{
			MakeSureContainerIsInitialized();
			if (typeof(BaseType) == typeof(EntitiesRunner))
				return EntitiesRunner.Current as BaseType;
			if (typeof(BaseType) == typeof(GlobalTime))
				return GlobalTime.Current as BaseType;
			if (typeof(BaseType) == typeof(ScreenSpace))
				return ScreenSpace.Current as BaseType;
			if (typeof(BaseType) == typeof(Randomizer))
				return Randomizer.Current as BaseType;
			return (BaseType)container.Resolve(typeof(BaseType));
		}

		private void MakeSureContainerIsInitialized()
		{
			if (IsAlreadyInitialized)
				return; //ncrunch: no coverage
			RegisterAllTypesFromAllAssemblies<ContentData, UpdateBehavior, DrawBehavior>();
			container = builder.Build();
		}

		protected bool IsAlreadyInitialized
		{
			get { return container != null; }
		}

		private IContainer container;

		private void RegisterAllTypesFromAllAssemblies<InstanceType, UpdateType, DrawType>()
		{
			var assemblies = TryLoadAllUnloadedAssemblies(AppDomain.CurrentDomain.GetAssemblies());
			foreach (Assembly assembly in assemblies)
				if (!AssemblyExtensions.IsPlatformAssembly(assembly.GetName().Name))
				{
					Type[] assemblyTypes = TryToGetAssemblyTypes(assembly);
					if (assemblyTypes == null)
						continue;
					RegisterAllTypesInAssembly<InstanceType>(assemblyTypes, false);
					RegisterAllTypesInAssembly<UpdateType>(assemblyTypes, true);
					RegisterAllTypesInAssembly<DrawType>(assemblyTypes, true);
					RegisterAllTypesInAssembly(assemblyTypes);
				}
		}

		private static IEnumerable<Assembly> TryLoadAllUnloadedAssemblies(Assembly[] loadedAssemblies)
		{
			var assemblies = new List<Assembly>(loadedAssemblies);
			var dllFiles = Directory.GetFiles(Directory.GetCurrentDirectory(), "*.dll");
			foreach (var filePath in dllFiles)
				try
				{
					string name = Path.GetFileNameWithoutExtension(filePath);
					if (new AssemblyName(name).IsAllowed() && !AssemblyExtensions.IsPlatformAssembly(name) &&
						!name.EndsWith(".Mocks") && assemblies.All(a => a.GetName().Name != name))
						assemblies.Add(Assembly.LoadFrom(filePath));
				}
				catch (Exception ex)
				{
					Logger.Warning("Failed to load assembly " + filePath + ": " + ex.Message);
				}
			foreach (var assembly in loadedAssemblies)
				if (assembly.IsAllowed() && !AssemblyExtensions.IsPlatformAssembly(assembly.GetName().Name))
					TryLoadDependentAssemblies(assembly, assemblies);
			return assemblies;
		}

		private static void TryLoadDependentAssemblies(Assembly assembly, List<Assembly> assemblies)
		{
			try
			{
				LoadDependentAssemblies(assembly, assemblies);
			}
			catch (FileNotFoundException exception)
			{
				throw new DependentAssemblieNotFound(assembly, exception);
			}
		}

		private static void LoadDependentAssemblies(Assembly assembly, List<Assembly> assemblies)
		{
			foreach (var dependency in assembly.GetReferencedAssemblies())
				if (dependency.IsAllowed() && !dependency.Name.EndsWith(".Mocks") &&
					assemblies.All(loaded => dependency.Name != loaded.GetName().Name))
					assemblies.Add(Assembly.Load(dependency));
		}

		private class DependentAssemblieNotFound : Exception
		{
			public DependentAssemblieNotFound(Assembly assembly, FileNotFoundException exception)
				: base("Dependency \"" + exception.FileName + "\" for assembly \"" + assembly.FullName +
				"\" not found") { }
		}

		private static Type[] TryToGetAssemblyTypes(Assembly assembly)
		{
			try
			{
				return assembly.GetTypes();
			}
			catch (Exception ex)
			{
				string errorText = ex.ToString();
				var loaderError = ex as ReflectionTypeLoadException;
				if (loaderError != null)
					foreach (var error in loaderError.LoaderExceptions)
						errorText += "\n\n" + error;
				Logger.Warning("Failed to load types from " + assembly.GetName().Name + ": " + errorText);
				return null;
			}
		}

		private void RegisterAllTypesInAssembly<T>(Type[] assemblyTypes, bool registerAsSingleton)
		{
			foreach (Type type in assemblyTypes)
				if (typeof(T).IsAssignableFrom(type) && IsTypeResolveable(type))
					if (registerAsSingleton)
						RegisterSingleton(type);
					else
						Register(type);
		}

		/// <summary>
		/// Allows to ignore most types. IsAbstract will also check if the class is static
		/// </summary>
		private static bool IsTypeResolveable(Type type)
		{
			if (type.IsEnum || type.IsAbstract || type.IsInterface || type.IsValueType ||
				typeof(Exception).IsAssignableFrom(type) || type == typeof(Action) ||
				type == typeof(Action<>) || typeof(MulticastDelegate).IsAssignableFrom(type))
				return false;
			if (IsGeneratedType(type) || IsGenericType(type) || type.Name.StartsWith("Mock") ||
				type.Name == "Program")
				return false;
			return !IgnoreForResolverAttribute.IsTypeIgnored(type);
		}

		private static bool IsGeneratedType(Type type)
		{
			return type.FullName.StartsWith("<") || type.Name.StartsWith("<>");
		}

		private static bool IsGenericType(Type type)
		{
			return type.FullName.Contains("`1");
		}

		private void RegisterAllTypesInAssembly(Type[] assemblyTypes)
		{
			foreach (Type type in assemblyTypes)
				if (IsTypeResolveable(type) && !alreadyRegisteredTypes.Contains(type))
				{
					builder.RegisterType(type).AsSelf().InstancePerLifetimeScope();
					AddRegisteredType(type);
				}
		}

		internal object Resolve(Type baseType, object customParameter = null)
		{
			MakeSureContainerIsInitialized();
			return ResolveAndShowErrorBoxIfNoDebuggerIsAttached(baseType, customParameter);
		}

		private object ResolveAndShowErrorBoxIfNoDebuggerIsAttached(Type baseType, object parameter)
		{
			try
			{
				return TryResolve(baseType, parameter);
			}
			catch (Exception ex)
			{
				Logger.Error(ex);
				if (Debugger.IsAttached || baseType == typeof(Window) ||
					StackTraceExtensions.StartedFromNCrunch)
					throw;
				ShowInitializationErrorBox(baseType, ex.InnerException ?? ex);
				return null;
			}
		}

		private object TryResolve(Type baseType, object parameter)
		{
			if (parameter == null)
				return container.Resolve(baseType);
			MakeSureContentManagerIsReady();
			if (parameter is ContentCreationData &&
				(baseType == typeof(Image) || baseType == typeof(Shader) ||
					baseType.Assembly.GetName().Name == "DeltaEngine.Graphics"))
				return Activator.CreateInstance(FindConcreteType(baseType),
					BindingFlags.NonPublic | BindingFlags.Instance, default(Binder),
					new[] { parameter, Resolve<Device>() }, default(CultureInfo));
			return container.Resolve(baseType,
				new Parameter[] { new TypedParameter(parameter.GetType(), parameter) });
		}

		/// <summary>
		/// When resolving loading the first ContentData instance we need to make sure all Content is
		/// available and can be loaded. Otherwise we have to wait to avoid content crashes.
		/// </summary>
		internal abstract void MakeSureContentManagerIsReady();

		private Type FindConcreteType(Type baseType)
		{
			foreach (var type in alreadyRegisteredTypes)
				if (!type.IsAbstract && baseType.IsAssignableFrom(type))
					return type;
			return null;
		}

		private void ShowInitializationErrorBox(Type baseType, Exception ex)
		{
			var exceptionText = StackTraceExtensions.FormatExceptionIntoClickableMultilineText(ex);
			var window = Resolve<Window>();
			window.CopyTextToClipboard(exceptionText);
			if (
				window.ShowMessageBox("Fatal Initialization Error",
					"Unable to resolve class " + baseType + "\n" +
						(ExceptionExtensions.IsDebugMode ? exceptionText : ex.GetType().Name + " " + ex.Message) +
						"\n\nMessage was logged and copied to the clipboard. Click Ignore to try to continue.",
					new[] { "Ignore", "Abort" }) == "Ignore")
				return;
			Dispose();
			if (!StackTraceExtensions.StartedFromNCrunch)
				Environment.Exit((int)AppRunner.ExitCode.InitializationError);
		}

		public virtual void Dispose()
		{
			if (IsAlreadyInitialized)
				DisposeContainerOnlyOnce();
		}

		private void DisposeContainerOnlyOnce()
		{
			var remContainerToDispose = container;
			container = null;
			remContainerToDispose.Dispose();
			builder = null;
		}
	}
}