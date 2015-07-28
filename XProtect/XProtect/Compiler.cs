using System;
using System.Collections.Generic;
using System.Text;
using System.CodeDom.Compiler;
using System.IO;
using Microsoft.CSharp;
using System.Windows.Forms;

    class Compiler
    {
        public static void Compile(string source, string Output, string EFResource, string Icon = null)
        {
            CompilerParameters CParams = new CompilerParameters();
            CParams.GenerateExecutable = true;
            CParams.OutputAssembly = Output;
            string options = "/optimize+  /t:winexe";
            if (Icon != null)
                options += " /win32icon:\"" + Icon + "\"";
            CParams.CompilerOptions = options;
            CParams.TreatWarningsAsErrors = false;
            CParams.ReferencedAssemblies.Add("System.dll");
            CParams.ReferencedAssemblies.Add("System.Windows.Forms.dll");
            CParams.ReferencedAssemblies.Add("System.Drawing.dll");
            CParams.ReferencedAssemblies.Add("System.Data.dll");
            CParams.ReferencedAssemblies.Add("Microsoft.VisualBasic.dll");
            CParams.EmbeddedResources.Add(EFResource);
            Dictionary<string, string> ProviderOptions = new Dictionary<string, string>();
            ProviderOptions.Add("CompilerVersion", "v2.0");
            CompilerResults Results = new CSharpCodeProvider(ProviderOptions).CompileAssemblyFromSource(CParams, source);
            if (Results.Errors.Count > 0)
            {
                MessageBox.Show(string.Format("The compiler has encountered {0} errors",
                    Results.Errors.Count), "Errors while compiling", MessageBoxButtons.OK,
                    MessageBoxIcon.Error);

                foreach (CompilerError Err in Results.Errors)
                {
                    MessageBox.Show(string.Format("{0}\nLine: {1} - Column: {2}\nFile: {3}", Err.ErrorText,
                        Err.Line, Err.Column, Err.FileName), "Error",
                        MessageBoxButtons.OK, MessageBoxIcon.Error);
                }

            }
        }
    }
