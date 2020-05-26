using System;
using System.CommandLine;
using System.CommandLine.Invocation;
using System.IO;
using System.Linq;
using System.Threading.Tasks;
using Mono.Cecil;
using Polly;

namespace Faithlife.AssemblyEmbedder
{
	internal class Program
	{
		private static Task<int> Main(string[] args)
		{
			var command = new RootCommand("Embeds .NET assemblies into another assembly, as binary resources.")
			{
				new Argument("target") { Arity = ArgumentArity.ExactlyOne, Description = "The .NET assembly to append resources onto." },
				new Argument("assemblies") { Arity = ArgumentArity.OneOrMore, Description = "The .NET assemblies to embed." },
				new Option("prefix", "The prefix for embedded resource names. Defaults to `faithlife.embedded-assembly.`."),
			};
			command.Handler = CommandHandler.Create<FileInfo, FileInfo[], string>(EmbedAssemblies);
			return command.InvokeAsync(args);
		}

		/// <summary>
		/// Embeds .NET assemblies into another assembly, as binary resources.
		/// </summary>
		/// <param name="target">The .NET assembly to append resources onto.</param>
		/// <param name="assemblies">The assemblies to append onto <paramref name="target"/>.</param>
		/// <param name="prefix">The resource name prefix for assemblies when embedding.</param>
		private static void EmbedAssemblies(FileInfo target, FileInfo[] assemblies, string prefix = c_prefix)
		{
			try
			{
				var policy = Policy
					.Handle<IOException>(ex => ex.HResult == ERROR_SHARING_VIOLATION)
					.WaitAndRetry(5, _ => TimeSpan.FromSeconds(1));

				Console.WriteLine($"Processing {target.Name}");

				var assembly = AssemblyDefinition.ReadAssembly(target.FullName, new ReaderParameters { InMemory = true });
				foreach (var file in assemblies)
				{
					Console.WriteLine($"Embedding {file.Name}");
					var fileData = File.ReadAllBytes(file.FullName);
					using var stream = new MemoryStream(fileData, writable: false);
					var embeddedAssembly = AssemblyDefinition.ReadAssembly(stream);
					var resource = new EmbeddedResource(prefix + embeddedAssembly.FullName, ManifestResourceAttributes.Public, fileData);
					assembly.MainModule.Resources.Add(resource);
				}

				policy.Execute(() => assembly.Write(target.FullName));
				Console.WriteLine($"Done processing {target.Name}");
			}
			catch (Exception e)
			{
				Console.WriteLine(e);
				throw;
			}
		}

		private const int ERROR_SHARING_VIOLATION = unchecked((int) 0x80070020);
		private const string c_prefix = "faithlife.embedded-assembly.";
	}
}
