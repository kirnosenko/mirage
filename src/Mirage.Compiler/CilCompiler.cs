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
			string src = "";
			using (TextReader file = new StreamReader(fileName))
			{
				src = file.ReadToEnd();
			}

			var nameTable = new NameTable();
			using (var host = new PeReader.DefaultHost(nameTable))
			{
				// Load Mirage types

				IModule module = host.LoadUnitFrom("Mirage.dll") as IModule;
				if (module == null || module is Dummy)
				{
					return;
				}
				var machineType = module.GetAllTypes().First(x => x.Name.Value == "Machine");
				var inputType = module.GetAllTypes().First(x => x.Name.Value == "ConsoleInput");
				var outputType = module.GetAllTypes().First(x => x.Name.Value == "ConsoleOutput");

				// Create assembly

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

				// Create namespace

				var rootUnitNamespace = new RootUnitNamespace();
				assembly.UnitNamespaceRoot = rootUnitNamespace;
				rootUnitNamespace.Unit = assembly;

				// Create module class

				var moduleClass = new NamespaceTypeDefinition()
				{
					ContainingUnitNamespace = rootUnitNamespace,
					InternFactory = host.InternFactory,
					IsClass = true,
					Name = nameTable.GetNameFor("<Module>"),
				};
				assembly.AllTypes.Add(moduleClass);

				// Create program class

				var programClass = new NamespaceTypeDefinition()
				{
					ContainingUnitNamespace = rootUnitNamespace,
					InternFactory = host.InternFactory,
					IsClass = true,
					IsPublic = true,
					Methods = new List<IMethodDefinition>(1),
					Name = nameTable.GetNameFor("Program"),
				};
				programClass.BaseClasses = new List<ITypeReference>() { host.PlatformType.SystemObject };
				rootUnitNamespace.Members.Add(programClass);

				// Add types to the assembly

				assembly.AllTypes.Add(machineType);
				assembly.AllTypes.Add(inputType);
				assembly.AllTypes.Add(outputType);
				assembly.AllTypes.Add(programClass);
				
				// Create main method

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

				// Create constructors and methods

				IMethodReference machineConstructor = new Microsoft.Cci.MethodReference(
					host,
					machineType,
					CallingConvention.HasThis,
					host.PlatformType.SystemVoid,
					host.NameTable.Ctor,
					0
				);

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

				var opIncPointers = TypeHelper.GetMethod(machineType, nameTable.GetNameFor("IncPointers"));
				var opDecPointers = TypeHelper.GetMethod(machineType, nameTable.GetNameFor("DecPointers"));
				var opIncHiPointer = TypeHelper.GetMethod(machineType, nameTable.GetNameFor("IncHiPointer"));
				var opDecHiPointer = TypeHelper.GetMethod(machineType, nameTable.GetNameFor("DecHiPointer"));
				var opReflectHiPointer = TypeHelper.GetMethod(machineType, nameTable.GetNameFor("ReflectHiPointer"));
				var opLoadHiPointer = TypeHelper.GetMethod(machineType, nameTable.GetNameFor("LoadHiPointer"));
				var opDragLoPointer = TypeHelper.GetMethod(machineType, nameTable.GetNameFor("DragLoPointer"));
				var opXchPointers = TypeHelper.GetMethod(machineType, nameTable.GetNameFor("XchPointers"));

				var opClear = TypeHelper.GetMethod(machineType, nameTable.GetNameFor("Clear"));
				var opAdd = TypeHelper.GetMethod(machineType, nameTable.GetNameFor("Add"));
				var opDec = TypeHelper.GetMethod(machineType, nameTable.GetNameFor("Dec"));
				var opNot = TypeHelper.GetMethod(machineType, nameTable.GetNameFor("Not"));
				var opAnd = TypeHelper.GetMethod(machineType, nameTable.GetNameFor("And"));
				var opOr = TypeHelper.GetMethod(machineType, nameTable.GetNameFor("Or"));
				var opXor = TypeHelper.GetMethod(machineType, nameTable.GetNameFor("Xor"));

				var opInput = TypeHelper.GetMethod(machineType, nameTable.GetNameFor("Input"), inputCast.Type);
				var opOutput = TypeHelper.GetMethod(machineType, nameTable.GetNameFor("Output"), outputCast.Type);

				// Create program code

				var ilGenerator = new ILGenerator(host, mainMethod);
				ilGenerator.Emit(OperationCode.Newobj, machineConstructor);
				ilGenerator.Emit(OperationCode.Stloc_0);
				ilGenerator.Emit(OperationCode.Newobj, inputConstructor);
				ilGenerator.Emit(OperationCode.Stloc_1);
				ilGenerator.Emit(OperationCode.Newobj, outputConstructor);
				ilGenerator.Emit(OperationCode.Stloc_2);
				
				int pc = 0;
				while (pc < src.Length)
				{
					char opcode = src[pc++];

					switch (opcode)
					{
						case ']':
							ilGenerator.Emit(OperationCode.Ldloc_0);
							ilGenerator.Emit(OperationCode.Callvirt, opIncHiPointer);
							break;
						case '~':
							ilGenerator.Emit(OperationCode.Ldloc_0);
							ilGenerator.Emit(OperationCode.Callvirt, opNot);
							break;
						case '?':
							ilGenerator.Emit(OperationCode.Ldloc_0);
							ilGenerator.Emit(OperationCode.Ldloc_1);
							ilGenerator.Emit(OperationCode.Call, inputCast);
							ilGenerator.Emit(OperationCode.Callvirt, opInput);
							break;
						case '!':
							ilGenerator.Emit(OperationCode.Ldloc_0);
							ilGenerator.Emit(OperationCode.Ldloc_2);
							ilGenerator.Emit(OperationCode.Call, outputCast);
							ilGenerator.Emit(OperationCode.Callvirt, opOutput);
							break;
						default:
							break;
					}
				}

				ilGenerator.Emit(OperationCode.Ret);

				mainMethod.Body = new ILGeneratorMethodBody(
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

				using (var peStream = File.Create(exeName))
				{
					PeWriter.WritePeToStream(assembly, host, peStream);
				}
			}
		}
	}
}
