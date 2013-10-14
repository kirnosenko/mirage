using System;
using System.Collections.Generic;
using System.IO;
using Microsoft.Cci;
using Microsoft.Cci.MutableCodeModel;

namespace Mirage.Compiler
{
	public class CilCompiler
	{
		public void Compile(string fileName)
		{
			string appName = Path.GetFileNameWithoutExtension(fileName);
			string exeName = appName + ".exe";

			var nameTable = new NameTable();
			using (var host = new PeReader.DefaultHost(nameTable))
			{
				var coreAssembly = host.LoadAssembly(host.CoreAssemblySymbolicIdentity);

				var assembly = new Assembly()
				{
					Name = nameTable.GetNameFor(appName),
					ModuleName = nameTable.GetNameFor(exeName),
					PlatformType = host.PlatformType,
					Kind = ModuleKind.ConsoleApplication,
					RequiresStartupStub = host.PointerSize == 4,
					TargetRuntimeVersion = coreAssembly.TargetRuntimeVersion,
				};
				assembly.AssemblyReferences.Add(coreAssembly);

				var rootUnitNamespace = new RootUnitNamespace();
				assembly.UnitNamespaceRoot = rootUnitNamespace;
				rootUnitNamespace.Unit = assembly;

				var moduleClass = new NamespaceTypeDefinition()
				{
					ContainingUnitNamespace = rootUnitNamespace,
					InternFactory = host.InternFactory,
					IsClass = true,
					Name = nameTable.GetNameFor("<Module>"),
				};
				assembly.AllTypes.Add(moduleClass);

				var programClass = new NamespaceTypeDefinition()
				{
					ContainingUnitNamespace = rootUnitNamespace,
					InternFactory = host.InternFactory,
					IsClass = true,
					IsPublic = true,
					Methods = new List<IMethodDefinition>(1),
					Name = nameTable.GetNameFor("Program"),
				};
				rootUnitNamespace.Members.Add(programClass);
				assembly.AllTypes.Add(programClass);
				programClass.BaseClasses = new List<ITypeReference>() { host.PlatformType.SystemObject };

				var mainMethod = new MethodDefinition()
				{
					ContainingTypeDefinition = programClass,
					InternFactory = host.InternFactory,
					IsCil = true,
					IsStatic = true,
					Name = nameTable.GetNameFor("Main"),
					Type = host.PlatformType.SystemVoid,
					Visibility = TypeMemberVisibility.Public,
				};
				assembly.EntryPoint = mainMethod;
				programClass.Methods.Add(mainMethod);

				var ilGenerator = new ILGenerator(host, mainMethod);

				var systemConsole = UnitHelper.FindType(nameTable, coreAssembly, "System.Console");
				var writeLine = TypeHelper.GetMethod(systemConsole, nameTable.GetNameFor("WriteLine"), host.PlatformType.SystemString);

				ilGenerator.Emit(OperationCode.Ldstr, "hello");
				ilGenerator.Emit(OperationCode.Call, writeLine);
				ilGenerator.Emit(OperationCode.Ret);

				var body = new ILGeneratorMethodBody(ilGenerator, true, 1, mainMethod, Enumerable<ILocalDefinition>.Empty, Enumerable<ITypeDefinition>.Empty);
				mainMethod.Body = body;

				using (var peStream = File.Create(exeName))
				{
					PeWriter.WritePeToStream(assembly, host, peStream);
				}
			}
		}
	}
}
