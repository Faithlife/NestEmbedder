using System;
using System.Collections.Generic;
using System.Linq;
using System.Reflection;

namespace Faithlife.NestEmbedder.EmbeddedAssemblyLoader
{
	/// <summary>
	/// Provides a utility function to load assemblies embedded in the calling assembly.
	/// </summary>
    public static class AssemblyLoader
    {
		/// <summary>
		/// Load all assemblies embedded in the calling assembly.
		/// </summary>
		/// <param name="prefix">The resource name prefix to look for when finding assemblies to load.</param>
		public static void LoadAll(string prefix = c_prefix)
		{
			_ = prefix ?? throw new ArgumentNullException(nameof(prefix));

			var containingAssembly = Assembly.GetCallingAssembly();

			// Load all embedded assemblies immediately.
			var existingAssemblies = AppDomain.CurrentDomain.GetAssemblies();
			var loadedAssemblies = new Dictionary<string, Assembly>(StringComparer.InvariantCultureIgnoreCase);
			foreach (var resourceName in GetEmbeddedAssemblyResourceNames(containingAssembly, prefix))
			{
				var (assembly, didLoadAssembly) = ReadFromLoadedAssembliesOrEmbeddedResource(containingAssembly, prefix, resourceName, existingAssemblies);
				if (didLoadAssembly)
					loadedAssemblies.Add(assembly.FullName, assembly);
			}

			// When requested, return the embedded assembly.
			AppDomain.CurrentDomain.AssemblyResolve += (_, args) => loadedAssemblies.TryGetValue(args.Name, out var assembly) ? assembly : null;
		}

		private static IEnumerable<string> GetEmbeddedAssemblyResourceNames(Assembly containingAssembly, string prefix) =>
			containingAssembly.GetManifestResourceNames()
				.Where(x => x.StartsWith(prefix, StringComparison.OrdinalIgnoreCase));

		private static (Assembly, bool) ReadFromLoadedAssembliesOrEmbeddedResource(Assembly containingAssembly, string prefix, string name, IEnumerable<Assembly> existingAssemblies)
		{
			// Check loaded assemblies first.
			var assemblyName = name.Substring(prefix.Length);
			var existingAssembly = existingAssemblies.FirstOrDefault(x => string.Equals(x.FullName, assemblyName, StringComparison.InvariantCultureIgnoreCase));
			if (existingAssembly != null)
				return (existingAssembly, false);

			// If not already loaded, then load the assembly.
			var assemblyData = ReadResourceAsBytes(containingAssembly, name);
			return (Assembly.Load(assemblyData), true);
		}

		private static byte[] ReadResourceAsBytes(Assembly containingAssembly, string name)
		{
			using (var stream = containingAssembly.GetManifestResourceStream(name) ?? throw new InvalidOperationException($"Unable to load embedded assembly {name}"))
			{
				var data = new byte[stream.Length];
				stream.Read(data, 0, data.Length);
				return data;
			}
		}

		private const string c_prefix = "faithlife.embedded-assembly.";

	}
}
