﻿// Copyright (c) AlphaSierraPapa for the SharpDevelop Team
// 
// Permission is hereby granted, free of charge, to any person obtaining a copy of this
// software and associated documentation files (the "Software"), to deal in the Software
// without restriction, including without limitation the rights to use, copy, modify, merge,
// publish, distribute, sublicense, and/or sell copies of the Software, and to permit persons
// to whom the Software is furnished to do so, subject to the following conditions:
// 
// The above copyright notice and this permission notice shall be included in all copies or
// substantial portions of the Software.
// 
// THE SOFTWARE IS PROVIDED "AS IS", WITHOUT WARRANTY OF ANY KIND, EXPRESS OR IMPLIED,
// INCLUDING BUT NOT LIMITED TO THE WARRANTIES OF MERCHANTABILITY, FITNESS FOR A PARTICULAR
// PURPOSE AND NONINFRINGEMENT. IN NO EVENT SHALL THE AUTHORS OR COPYRIGHT HOLDERS BE LIABLE
// FOR ANY CLAIM, DAMAGES OR OTHER LIABILITY, WHETHER IN AN ACTION OF CONTRACT, TORT OR
// OTHERWISE, ARISING FROM, OUT OF OR IN CONNECTION WITH THE SOFTWARE OR THE USE OR OTHER
// DEALINGS IN THE SOFTWARE.

using System;
using System.Collections.Concurrent;
using System.Diagnostics;
using System.IO;
using System.Linq;
using ICSharpCode.NRefactory.TypeSystem;
using ICSharpCode.NRefactory.Utils;

namespace ICSharpCode.NRefactory.ConsistencyCheck
{
	class Program
	{
		public static readonly string[] AssemblySearchPaths = {
			@"C:\Program Files (x86)\Reference Assemblies\Microsoft\Framework\.NETFramework\v4.0",
			@"C:\Program Files (x86)\GtkSharp\2.12\lib\gtk-sharp-2.0",
			@"C:\Program Files (x86)\GtkSharp\2.12\lib\Mono.Posix",
		};
		
		public const string TempPath = @"C:\temp";
		
		public static void Main(string[] args)
		{
			Solution sln;
			using (new Timer("Loading solution... ")) {
				sln = new Solution(Path.GetFullPath("../../../NRefactory.sln"));
			}
			
			Console.WriteLine("Loaded {0} lines of code ({1:f1} MB) in {2} files in {3} projects.",
			                  sln.AllFiles.Sum(f => f.LinesOfCode),
			                  sln.AllFiles.Sum(f => f.Content.TextLength) / 1024.0 / 1024.0,
			                  sln.AllFiles.Count(),
			                  sln.Projects.Count);
			
			using (new Timer("Roundtripping tests... ")) {
				foreach (var file in sln.AllFiles) {
					RoundtripTest.RunTest(file);
				}
			}
			
			Console.Write("Press any key to continue . . . ");
			Console.ReadKey(true);
		}
		
		static ConcurrentDictionary<string, IUnresolvedAssembly> assemblyDict = new ConcurrentDictionary<string, IUnresolvedAssembly>(Platform.FileNameComparer);
		
		public static IUnresolvedAssembly LoadAssembly(string assemblyFileName)
		{
			return assemblyDict.GetOrAdd(assemblyFileName, file => new CecilLoader().LoadAssemblyFile(file));
		}
		
		sealed class Timer : IDisposable
		{
			Stopwatch w = Stopwatch.StartNew();
			
			public Timer(string title)
			{
				Console.Write(title);
			}
			
			public void Dispose()
			{
				Console.WriteLine(w.Elapsed);
			}
		}
	}
}