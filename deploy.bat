/*
@echo off & cls
set WinDirNet=%WinDir%\Microsoft.NET\Framework
IF EXIST "%WinDirNet%\v2.0.50727\csc.exe" set csc="%WinDirNet%\v2.0.50727\csc.exe"
IF EXIST "%WinDirNet%\v3.5\csc.exe" set csc="%WinDirNet%\v3.5\csc.exe"
IF EXIST "%WinDirNet%\v4.0.30319\csc.exe" set csc="%WinDirNet%\v4.0.30319\csc.exe"
%csc% /nologo /out:"%~0.exe" %0
"%~0.exe"
del "%~0.exe"
exit
*/

using System;
using System.Collections.Generic;
using System.IO;
 
class Deploy
{
	static void Main()
	{
		string deployDir = "./deploy/";
		
		List<string> files = new List<string>()
		{
			"./lib/Microsoft.Cci.ILGenerator.dll",
			"./lib/Microsoft.Cci.MetadataHelper.dll",
			"./lib/Microsoft.Cci.MetadataModel.dll",
			"./lib/Microsoft.Cci.MutableMetadataModel.dll",
			"./lib/Microsoft.Cci.PeReader.dll",
			"./lib/Microsoft.Cci.PeWriter.dll",
			"./lib/Microsoft.Cci.SourceModel.dll",
			
			"./src/Mirage/bin/Release/Mirage.dll",
			"./src/Mirage.Cmd/bin/Release/Mirage.Cmd.exe",
			"./src/Mirage.Compiler/bin/Release/Mirage.Compiler.exe",
			
			"./LICENSE.TXT",
			"./README.TXT",
		};
		List<string> dirs = new List<string>()
		{
			"./doc",
		};
		
		if (! Directory.Exists(deployDir))
		{
			Directory.CreateDirectory(deployDir);
		}
		foreach (var file in files)
		{
			if (File.Exists(file))
			{
				File.Copy(file, deployDir + Path.GetFileName(file), true);
			}
			else
			{
				Console.WriteLine("Could not find file {0}", file);
			}
		}
		foreach (var dir in dirs)
		{
			CopyDir(dir, deployDir + GetDirName(dir));
		}
	}
	static void CopyDir(string sourceDir, string destDir)
	{
		if (! Directory.Exists(destDir))
		{
			Directory.CreateDirectory(destDir);
		}
		foreach (var file in Directory.GetFiles(sourceDir))
		{
			File.Copy(file, destDir + "/" + Path.GetFileName(file), true);
		}
		foreach (var dir in Directory.GetDirectories(sourceDir))
		{
			CopyDir(dir, destDir + "/" + GetDirName(dir));
		}
	}
	static string GetDirName(string dir)
	{
		return dir.Substring(dir.Replace("\\","/").LastIndexOf("/")+1);
	}
}
