using System;
using System.Collections.Generic;
using System.Linq;
using System.Reflection;
using System.Reflection.Emit;
using Mono.Reflection;

namespace Mirage.Cil
{
	public class Compiler
	{
		public void Compile(string assemblyName, string src)
		{
			string filename = assemblyName + ".exe";

			AssemblyBuilder assembly = AppDomain.CurrentDomain.DefineDynamicAssembly(
				new AssemblyName(assemblyName),
				AssemblyBuilderAccess.Save
			);
			ModuleBuilder module = assembly.DefineDynamicModule(assemblyName, filename);
			
			Type[] typesToCopy = Assembly.GetAssembly(typeof(Machine)).GetTypes();
			foreach (var t in typesToCopy)
			{		
				TypeBuilder tb = module.DefineType(t.Name, t.Attributes);
				/*
				foreach (var c in t.GetConstructors())
				{
					tb.DefineConstructor(
						c.Attributes,
						c.CallingConvention, 
						c.GetParameters().Select(x => x.ParameterType).ToArray()
					);
				}
				*/
				foreach (var f in t.GetFields(BindingFlags.Public | BindingFlags.NonPublic | BindingFlags.Instance))
				{
					tb.DefineField(f.Name, f.FieldType, f.Attributes);
				}

				foreach (var m in t.GetMethods(BindingFlags.Public|BindingFlags.NonPublic|BindingFlags.Instance))
				{
					if (m.Module.Assembly == Assembly.GetAssembly(typeof(Machine)))
					{
						List<Instruction> list = MethodBodyReader.GetInstructions(m);
						
						MethodBuilder mb = tb.DefineMethod(
							m.Name,
							m.Attributes,
							m.CallingConvention,
							m.ReturnType,
							m.GetParameters().Select(x => x.ParameterType).ToArray()
						);
						
						ILGenerator gen = mb.GetILGenerator();
						
						foreach (var i in list)
						{
							if (i.Operand == null)
							{
								gen.Emit(i.OpCode);
							}
							else
							{
								if (i.Operand is byte)
								{
									gen.Emit(i.OpCode, (byte)i.Operand);
								}
								else if (i.Operand is ConstructorInfo)
								{
									gen.Emit(i.OpCode, (ConstructorInfo)i.Operand);
								}
								else if (i.Operand is double)
								{
									gen.Emit(i.OpCode, (double)i.Operand);
								}
								else if (i.Operand is FieldInfo)
								{
									gen.Emit(i.OpCode, (FieldInfo)i.Operand);
								}
								else if (i.Operand is float)
								{
									gen.Emit(i.OpCode, (float)i.Operand);
								}
								else if (i.Operand is int)
								{
									gen.Emit(i.OpCode, (int)i.Operand);
								}
								else if (i.Operand is Label)
								{
									gen.Emit(i.OpCode, (Label)i.Operand);
								}
								else if (i.Operand is Label[])
								{
									gen.Emit(i.OpCode, (Label[])i.Operand);
								}
								else if (i.Operand is LocalBuilder)
								{
									gen.Emit(i.OpCode, (LocalBuilder)i.Operand);
								}
								else if (i.Operand is long)
								{
									gen.Emit(i.OpCode, (long)i.Operand);
								}
								else if (i.Operand is MethodInfo)
								{
									gen.Emit(i.OpCode, (MethodInfo)i.Operand);
								}
								else if (i.Operand is sbyte)
								{
									gen.Emit(i.OpCode, (sbyte)i.Operand);
								}
								else if (i.Operand is short)
								{
									gen.Emit(i.OpCode, (short)i.Operand);
								}
								else if (i.Operand is SignatureHelper)
								{
									gen.Emit(i.OpCode, (SignatureHelper)i.Operand);
								}
								else if (i.Operand is string)
								{
									gen.Emit(i.OpCode, (string)i.Operand);
								}
								else if (i.Operand is Type)
								{
									gen.Emit(i.OpCode, (Type)i.Operand);
								}
							}
						}
						//mb.CreateMethodBody(body, body.Length);
						gen.Emit(OpCodes.Ret);
					}
				}
				tb.CreateType();
			}
			
			TypeBuilder type = module.DefineType("Program", TypeAttributes.Class | TypeAttributes.Public);
			FieldBuilder memory = type.DefineField("memory", typeof(byte[]), FieldAttributes.Private | FieldAttributes.Static);
			FieldBuilder pointer = type.DefineField("pointer", typeof(int), FieldAttributes.Private | FieldAttributes.Static);
			MethodBuilder method = type.DefineMethod(
				"Main",
				MethodAttributes.HideBySig | MethodAttributes.Public | MethodAttributes.Static,
				typeof(void),
				new Type[] { typeof(string[]) }
			);
			ILGenerator ilGen = method.GetILGenerator();

			/*
			ilGen.Emit(OpCodes.Ldstr, "Hello, World!");
			ilGen.Emit(OpCodes.Call, typeof(Console).GetMethod("WriteLine", new Type[] { typeof(string) }));
			ilGen.Emit(OpCodes.Ldc_I4_1);
			ilGen.Emit(OpCodes.Call, typeof(Console).GetMethod("ReadKey", new Type[] { typeof(bool) }));
			*/
			
			ilGen.Emit(OpCodes.Ldc_I4, 64 * 1024);
			ilGen.Emit(OpCodes.Newarr, typeof(byte));
			ilGen.Emit(OpCodes.Stsfld, memory);

			Stack<Label> labels = new Stack<Label>(20);

			foreach (var t in src)
			{
				switch (t)
				{
					case '>':
						{
							ilGen.Emit(OpCodes.Ldsfld, pointer);
							ilGen.Emit(OpCodes.Ldc_I4_1);
							ilGen.Emit(OpCodes.Add);
							ilGen.Emit(OpCodes.Stsfld, pointer);
							break;
						}
					case '<':
						{
							ilGen.Emit(OpCodes.Ldsfld, pointer);
							ilGen.Emit(OpCodes.Ldc_I4_1);
							ilGen.Emit(OpCodes.Sub);
							ilGen.Emit(OpCodes.Stsfld, pointer);
							break;
						}
					case '+':
						{
							ilGen.Emit(OpCodes.Ldsfld, memory);
							ilGen.Emit(OpCodes.Ldsfld, pointer);
							ilGen.Emit(OpCodes.Ldelema, typeof(byte));
							ilGen.Emit(OpCodes.Dup);
							ilGen.Emit(OpCodes.Ldobj, typeof(byte));
							ilGen.Emit(OpCodes.Ldc_I4_1);
							ilGen.Emit(OpCodes.Add);
							ilGen.Emit(OpCodes.Conv_U1);
							ilGen.Emit(OpCodes.Stobj, typeof(byte));
							break;
						}
					case '-':
						{
							ilGen.Emit(OpCodes.Ldsfld, memory);
							ilGen.Emit(OpCodes.Ldsfld, pointer);
							ilGen.Emit(OpCodes.Ldelema, typeof(byte));
							ilGen.Emit(OpCodes.Dup);
							ilGen.Emit(OpCodes.Ldobj, typeof(byte));
							ilGen.Emit(OpCodes.Ldc_I4_1);
							ilGen.Emit(OpCodes.Sub);
							ilGen.Emit(OpCodes.Conv_U1);
							ilGen.Emit(OpCodes.Stobj, typeof(byte));
							break;
						}
					case '[':
						{
							var Lbl = ilGen.DefineLabel();
							ilGen.MarkLabel(Lbl);
							labels.Push(Lbl);
							break;
						}
					case ']':
						{
							ilGen.Emit(OpCodes.Ldsfld, memory);
							ilGen.Emit(OpCodes.Ldsfld, pointer);
							ilGen.Emit(OpCodes.Ldelem_U1);
							ilGen.Emit(OpCodes.Ldc_I4_0);
							ilGen.Emit(OpCodes.Ceq);
							ilGen.Emit(OpCodes.Brtrue, 5);
							ilGen.Emit(OpCodes.Br, labels.Pop());
							break;
						}
					case '.':
						{
							ilGen.Emit(OpCodes.Ldsfld, memory);
							ilGen.Emit(OpCodes.Ldsfld, pointer);
							ilGen.Emit(OpCodes.Ldelem_U1);
							ilGen.EmitCall(OpCodes.Call, typeof(Console).GetMethod("Write", new[] { typeof(char) }), new[] { typeof(char) });
							ilGen.Emit(OpCodes.Nop);
							break;
						}
					case ',':
						{
							ilGen.Emit(OpCodes.Ldsfld, memory);
							ilGen.Emit(OpCodes.Ldsfld, pointer);
							ilGen.EmitCall(OpCodes.Call, typeof(Console).GetMethod("ReadLine"), new[] { typeof(string) });
							ilGen.Emit(OpCodes.Call, typeof(Convert).GetMethod("ToByte", new[] { typeof(string) }));
							ilGen.Emit(OpCodes.Stelem_I1);
							break;
						}
				}
			}
			ilGen.Emit(OpCodes.Ret);

			type.CreateType();
			assembly.SetEntryPoint(method, PEFileKinds.ConsoleApplication);
			assembly.Save(filename);
		}
		public bool CheckSyntax(string src)
		{
			int opened = 0;
			for (int i = 0; i < src.Length; i++)
			{
				if (src[i] == '[')
				{
					opened++;
				}
				else if (src[i] == ']')
				{
					opened--;
				}
			}
			if (opened != 0)
			{
				throw new ApplicationException("Error!");
			}

			return true;
		}
	}
}
