using System;
using System.Globalization;
using System.IO;
using System.Reflection;
using System.Runtime.InteropServices;
using System.Security;
using System.Security.Permissions;
using System.Security.Policy;
using System.Text;
using System.Threading;
using Newtonsoft.Json;

namespace CsSandboxer
{
	static class Program
	{
		private static readonly JsonSerializerSettings settings = new JsonSerializerSettings
		{
			TypeNameHandling = TypeNameHandling.All
		};

		static void Main(string[] args)
		{
			Console.OutputEncoding = Encoding.UTF8;
			SetErrorMode(ErrorModes.SEM_NOGPFAULTERRORBOX); // WinOnly StackOverflow handling fix
			Thread.CurrentThread.CurrentCulture = CultureInfo.InvariantCulture;
			Console.InputEncoding = Encoding.UTF8;
			var assemblyPath = args[0];
			var id = args[1];
			Assembly assembly = null;
			Sandboxer sandboxer = null;

			try
			{
				assembly = Assembly.LoadFrom(assemblyPath);
				var domain = CreateDomain(id, assemblyPath);
				sandboxer = CreateSandboxer(domain);
			}
			catch (Exception ex)
			{
				HandleException(ex);
			}

			if (assembly == null || sandboxer == null)
				Environment.Exit(1);

			GC.Collect();

			Console.Out.WriteLine("Ready");
			var runCommand = Console.In.ReadLineAsync();
			if (!runCommand.Wait(1000) || runCommand.Result != "Run")
			{
				Console.Error.WriteLine($"Can't receive Run command: {runCommand.Result}");
				Environment.Exit(2);
			}

			try
			{
				sandboxer.ExecuteUntrustedCode(assembly.EntryPoint);
			}
			catch (Exception ex)
			{
				HandleException(ex);
			}
		}

		private static void HandleException(Exception ex)
		{
			Console.Error.WriteLine();
			Console.Error.Write(JsonConvert.SerializeObject(ex, settings));
			Console.Error.Close();
			Environment.Exit(3);
		}

		private static void HandleException(object sender, UnhandledExceptionEventArgs e)
		{
			HandleException(e.ExceptionObject as Exception);
		}

		private static AppDomain CreateDomain(string id, string assemblyPath)
		{
			var permSet = new PermissionSet(PermissionState.None);
			permSet.AddPermission(new FileIOPermission(FileIOPermissionAccess.Read, assemblyPath));
			permSet.AddPermission(new FileIOPermission(FileIOPermissionAccess.PathDiscovery, assemblyPath));
			permSet.AddPermission(new FileIOPermission(FileIOPermissionAccess.PathDiscovery, Environment.CurrentDirectory));
			permSet.AddPermission(new EnvironmentPermission(EnvironmentPermissionAccess.Read, "InsideSandbox"));
			
			/*
			 * Permissions for NUnit: see https://github.com/nunit/nunit/blob/master/src/NUnitFramework/tests/Assertions/LowTrustFixture.cs#L166
			 * and https://github.com/nunit/nunit/issues/2792 for details
			 */
			permSet.AddPermission(new ReflectionPermission(
				ReflectionPermissionFlag.MemberAccess));            // Required to instantiate classes that contain test code and to get cross-appdomain communication to work.
			permSet.AddPermission(new SecurityPermission(
				SecurityPermissionFlag.Execution |                  // Required to execute test code
				SecurityPermissionFlag.SerializationFormatter       // Required to support cross-appdomain test result formatting by NUnit TestContext
			));
			/* In .NET <= 3.5 add following EnvironmentPermission:
			permSet.AddPermission(new EnvironmentPermission(PermissionState.Unrestricted)); // Required for NUnit.Framework.Assert.GetStackTrace()
			*/ 

			var evidence = new Evidence();
			evidence.AddHostEvidence(new Zone(SecurityZone.Untrusted));
			var fullyTrustAssemblies = typeof(Sandboxer).Assembly.Evidence.GetHostEvidence<StrongName>();

			var applicationBase = Path.GetDirectoryName(assemblyPath);
			var adSetup = new AppDomainSetup
			{
				ApplicationBase = applicationBase,
			};

			/* Copy CsSandboxer.exe to destination folder, because it's needed them to set domain.UnhandledException handler below. */
			var sandboxAssembly = typeof(Sandboxer).Assembly.Location;
			File.Copy(sandboxAssembly, Path.Combine(applicationBase, Path.GetFileName(sandboxAssembly)), overwrite: true);

			var domain = AppDomain.CreateDomain(id, evidence, adSetup, permSet, fullyTrustAssemblies);
			domain.UnhandledException += HandleException;
			return domain;
		}

		private static Sandboxer CreateSandboxer(AppDomain domain)
		{
			var handle = Activator.CreateInstanceFrom(
				domain,
				typeof(Sandboxer).Assembly.ManifestModule.FullyQualifiedName,
				typeof(Sandboxer).FullName
			);
			var sandboxer = (Sandboxer)handle.Unwrap();
			return sandboxer;
		}

		[DllImport("kernel32.dll")]
		static extern ErrorModes SetErrorMode(ErrorModes uMode);

		[Flags]
		private enum ErrorModes : uint
		{
			SYSTEM_DEFAULT = 0x0,
			SEM_FAILCRITICALERRORS = 0x0001,
			SEM_NOALIGNMENTFAULTEXCEPT = 0x0004,
			SEM_NOGPFAULTERRORBOX = 0x0002,
			SEM_NOOPENFILEERRORBOX = 0x8000
		}
	}
}