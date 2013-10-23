using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text;
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
				var opSal = TypeHelper.GetMethod(machineType, nameTable.GetNameFor("Sal"));
				var opSar = TypeHelper.GetMethod(machineType, nameTable.GetNameFor("Sar"));

				var opLoadData = TypeHelper.GetMethod(machineType, nameTable.GetNameFor("LoadData"), host.PlatformType.SystemString);
				var opInput = TypeHelper.GetMethod(machineType, nameTable.GetNameFor("Input"), inputCast.Type);
				var opOutput = TypeHelper.GetMethod(machineType, nameTable.GetNameFor("Output"), outputCast.Type);

				var opJz = TypeHelper.GetMethod(machineType, nameTable.GetNameFor("Jz"));

				// Create program code

				var labels = new Stack<ILGeneratorLabel>(100);

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
						case '>':
							ilGenerator.Emit(OperationCode.Ldloc_0);
							ilGenerator.Emit(OperationCode.Callvirt, opIncPointers);
							break;
						case '<':
							ilGenerator.Emit(OperationCode.Ldloc_0);
							ilGenerator.Emit(OperationCode.Callvirt, opDecPointers);
							break;
						case ']':
							ilGenerator.Emit(OperationCode.Ldloc_0);
							ilGenerator.Emit(OperationCode.Callvirt, opIncHiPointer);
							break;
						case '[':
							ilGenerator.Emit(OperationCode.Ldloc_0);
							ilGenerator.Emit(OperationCode.Callvirt, opDecHiPointer);
							break;
						case '#':
							ilGenerator.Emit(OperationCode.Ldloc_0);
							ilGenerator.Emit(OperationCode.Callvirt, opReflectHiPointer);
							break;
						case '$':
							ilGenerator.Emit(OperationCode.Ldloc_0);
							ilGenerator.Emit(OperationCode.Callvirt, opLoadHiPointer);
							break;
						case '=':
							ilGenerator.Emit(OperationCode.Ldloc_0);
							ilGenerator.Emit(OperationCode.Callvirt, opDragLoPointer);
							break;
						case '%':
							ilGenerator.Emit(OperationCode.Ldloc_0);
							ilGenerator.Emit(OperationCode.Callvirt, opXchPointers);
							break;
						case '_':
							ilGenerator.Emit(OperationCode.Ldloc_0);
							ilGenerator.Emit(OperationCode.Callvirt, opClear);
							break;
						case '+':
							ilGenerator.Emit(OperationCode.Ldloc_0);
							ilGenerator.Emit(OperationCode.Callvirt, opAdd);
							break;
						case '-':
							ilGenerator.Emit(OperationCode.Ldloc_0);
							ilGenerator.Emit(OperationCode.Callvirt, opDec);
							break;
						case '~':
							ilGenerator.Emit(OperationCode.Ldloc_0);
							ilGenerator.Emit(OperationCode.Callvirt, opNot);
							break;
						case '&':
							ilGenerator.Emit(OperationCode.Ldloc_0);
							ilGenerator.Emit(OperationCode.Callvirt, opAnd);
							break;
						case '|':
							ilGenerator.Emit(OperationCode.Ldloc_0);
							ilGenerator.Emit(OperationCode.Callvirt, opOr);
							break;
						case '^':
							ilGenerator.Emit(OperationCode.Ldloc_0);
							ilGenerator.Emit(OperationCode.Callvirt, opXor);
							break;
						case '*':
							ilGenerator.Emit(OperationCode.Ldloc_0);
							ilGenerator.Emit(OperationCode.Callvirt, opSal);
							break;
						case '/':
							ilGenerator.Emit(OperationCode.Ldloc_0);
							ilGenerator.Emit(OperationCode.Callvirt, opSar);
							break;
						case '(':
							int dataStart = pc;
							int dataEnd = dataStart;
							while (src[pc++] != ')')
							{
								dataEnd = pc;
							}
							ilGenerator.Emit(OperationCode.Ldloc_0);
							ilGenerator.Emit(OperationCode.Ldstr, src.Substring(dataStart, dataEnd - dataStart));
							ilGenerator.Emit(OperationCode.Callvirt, opLoadData);
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
						case '{':
							var cycleStart = new ILGeneratorLabel();
							var cycleEnd = new ILGeneratorLabel();
							labels.Push(cycleStart);
							labels.Push(cycleEnd);
							ilGenerator.Emit(OperationCode.Br, cycleEnd);
							ilGenerator.MarkLabel(cycleStart);
							break;
						case '}':
							ilGenerator.MarkLabel(labels.Pop());
							ilGenerator.Emit(OperationCode.Ldloc_0);
							ilGenerator.Emit(OperationCode.Callvirt, opJz);
							ilGenerator.Emit(OperationCode.Ldc_I4_0);
							ilGenerator.Emit(OperationCode.Ceq);
							ilGenerator.Emit(OperationCode.Stloc_3);
							ilGenerator.Emit(OperationCode.Ldloc_3);
							ilGenerator.Emit(OperationCode.Brtrue, labels.Pop());
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
						new LocalDefinition() { Type = machineType },
						new LocalDefinition() { Type = inputType },
						new LocalDefinition() { Type = outputType },
						new LocalDefinition() { Type = host.PlatformType.SystemInt32 },
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
