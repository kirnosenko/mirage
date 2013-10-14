using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
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

				IModule module = host.LoadUnitFrom("Mirage.dll") as IModule;
				if (module == null || module is Dummy)
				{
					Console.WriteLine("Could not find");
					return;
				}
				var machineType = module.GetAllTypes().First(x => x.Name.Value == "Machine");
				var inputType = module.GetAllTypes().First(x => x.Name.Value == "ConsoleInput");
				var outputType = module.GetAllTypes().First(x => x.Name.Value == "ConsoleOutput");

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
				assembly.AllTypes.Add(machineType);
				assembly.AllTypes.Add(inputType);
				assembly.AllTypes.Add(outputType);
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

				IMethodReference inputConstructor = new Microsoft.Cci.MethodReference(
					host,
					inputType,
					CallingConvention.HasThis,
					host.PlatformType.SystemVoid,
					host.NameTable.Ctor,
					0
				);
				var inputCast = TypeHelper.GetMethod(inputType, nameTable.GetNameFor("op_Implicit"), inputType);

				IMethodReference outputConstructor = new Microsoft.Cci.MethodReference(
					host,
					outputType,
					CallingConvention.HasThis,
					host.PlatformType.SystemVoid,
					host.NameTable.Ctor,
					0
				);
				var outputCast = TypeHelper.GetMethod(outputType, nameTable.GetNameFor("op_Implicit"), outputType);

				IMethodReference machineConstructor = new Microsoft.Cci.MethodReference(
					host,
					machineType,
					CallingConvention.HasThis,
					host.PlatformType.SystemVoid,
					host.NameTable.Ctor,
					0
				);
				var opIncHiPointer = TypeHelper.GetMethod(machineType, nameTable.GetNameFor("IncHiPointer"));
				var opNot = TypeHelper.GetMethod(machineType, nameTable.GetNameFor("Not"));
				var opInput = TypeHelper.GetMethod(machineType, nameTable.GetNameFor("Input"), inputCast.Type);
				var opOutput = TypeHelper.GetMethod(machineType, nameTable.GetNameFor("Output"), outputCast.Type );

				ilGenerator.Emit(OperationCode.Newobj, inputConstructor);
				ilGenerator.Emit(OperationCode.Stloc_0);
				ilGenerator.Emit(OperationCode.Newobj, outputConstructor);
				ilGenerator.Emit(OperationCode.Stloc_1);
				ilGenerator.Emit(OperationCode.Newobj, machineConstructor);
				ilGenerator.Emit(OperationCode.Stloc_2);
				ilGenerator.Emit(OperationCode.Ldloc_2);
				ilGenerator.Emit(OperationCode.Callvirt, opIncHiPointer);
				ilGenerator.Emit(OperationCode.Ldloc_2);
				ilGenerator.Emit(OperationCode.Callvirt, opNot);
				ilGenerator.Emit(OperationCode.Ldloc_2);
				ilGenerator.Emit(OperationCode.Ldloc_0);
				ilGenerator.Emit(OperationCode.Call, inputCast);
				ilGenerator.Emit(OperationCode.Callvirt, opInput);
				ilGenerator.Emit(OperationCode.Ldloc_2);
				ilGenerator.Emit(OperationCode.Ldloc_1);
				ilGenerator.Emit(OperationCode.Call, outputCast);
				ilGenerator.Emit(OperationCode.Callvirt, opOutput);
				ilGenerator.Emit(OperationCode.Ret);

				var body = new ILGeneratorMethodBody(
					ilGenerator,
					true,
					8,
					mainMethod,
					new List<ILocalDefinition>() {
						new LocalDefinition() { Type = inputType },
						new LocalDefinition() { Type = outputType },
						new LocalDefinition() { Type = machineType },
					},
					Enumerable<ITypeDefinition>.Empty
				);
				mainMethod.Body = body;

				using (var peStream = File.Create(exeName))
				{
					PeWriter.WritePeToStream(assembly, host, peStream);
				}
			}
		}
	}
}
