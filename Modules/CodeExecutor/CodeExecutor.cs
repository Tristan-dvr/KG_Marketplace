using Mono.CSharp;

namespace Marketplace.Modules.CodeExecutor;

[UsedImplicitly, Market_Autoload(Market_Autoload.Type.Client, Market_Autoload.Priority.Last, "OnInit")]
public static class CodeExecutor
{
    private static Evaluator codeExecutor;
    private static readonly StringBuilder builder = new();
    private static readonly StringWriter sw = new StringWriter(builder);

    private static void OnInit()
    {
        CompilerSettings settings = new()
        {
            Version = LanguageVersion.Experimental,
            GenerateDebugInfo = false,
            StdLib = true,
            Target = Target.Library,
            WarningLevel = 0,
            EnhancedWarnings = false
        };
        CompilerContext context = new(settings, new StreamReportPrinter(sw));
        codeExecutor = new(context);
        foreach (Assembly asm in AppDomain.CurrentDomain.GetAssemblies())
            if (asm.GetName().Name is not "mscorlib" and not "System.Core" and not "System" and not "System.Xml")
                codeExecutor.ReferenceAssembly(asm);
        AppDomain.CurrentDomain.AssemblyLoad += (_, args) => { codeExecutor.ReferenceAssembly(args.LoadedAssembly); };
        codeExecutor.Run("using System;");
        codeExecutor.Run("using System.Collections.Generic;");
        codeExecutor.Run("using System.Globalization;");
        codeExecutor.Run("using System.IO;");
        codeExecutor.Run("using System.Linq;");
        codeExecutor.Run("using System.Text;");
        codeExecutor.Run("using System.Reflection;");
        codeExecutor.Run("using UnityEngine;");
        codeExecutor.Run("using UnityEngine.UI;");
        codeExecutor.Run("using HarmonyLib;");
        object _ = null;
        codeExecutor.Compile(@"Marketplace.Utils.print(""Code Executor Init"")")?.Invoke(ref _);
    }
    
    private static void ExecuteCode(string code)
    {
        var method = codeExecutor.Compile(code);
        object toRef = null;
        method?.Invoke(ref toRef);
        string result = sw.ToString();
        if (string.IsNullOrEmpty(result)) return;
        Utils.print($"Code execution result:\n{result}", ConsoleColor.Red);
        builder.Clear();
    }

    public static void ExecuteScript(string name)
    {
        name = name.ToLower();
        if (Scripts_DataTypes.SyncedScripts.Value.TryGetValue(name, out string code))
            ExecuteCode(code);
    }
}