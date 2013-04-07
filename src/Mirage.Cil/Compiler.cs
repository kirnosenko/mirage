using System;
using System.Collections.Generic;
using System.Reflection;
using System.Reflection.Emit;

namespace Mirage.Cil
{
	public class Compiler
	{
		public void Compile(string filename, string src)
		{
			string assemblyName = filename;

			AssemblyBuilder assembly = AppDomain.CurrentDomain.DefineDynamicAssembly(
				new AssemblyName(assemblyName),
				AssemblyBuilderAccess.Save
			);
			ModuleBuilder module = assembly.DefineDynamicModule(assemblyName, filename);
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
