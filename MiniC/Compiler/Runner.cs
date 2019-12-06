using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace MiniC.Compiler
{
    class Runner
    {
        static string FileName = "main.s";
        static string OutputExecutable = "out.exe";
        public static void AssembleAndLink(string asm)
        {
            System.IO.File.WriteAllText(FileName, asm);
            ProcessStartInfo p = new ProcessStartInfo();
            p.FileName = "gcc.exe";
            p.Arguments = $"-m32 -o {OutputExecutable} {FileName}";
            p.WindowStyle = ProcessWindowStyle.Hidden;
            p.CreateNoWindow = true;
            int exitCode;
            // Run the external process & wait for it to finish
            using (Process proc = Process.Start(p))
            {
                proc.WaitForExit();
                // Retrieve the app's exit code
                exitCode = proc.ExitCode;
            }
        }
        public static void Run()
        {
            ProcessStartInfo p = new ProcessStartInfo();
            p.FileName = "cmd.exe";
            p.Arguments = $"/C {OutputExecutable}";
            p.UseShellExecute = true;
            p.WorkingDirectory = System.Environment.CurrentDirectory;
            p.WindowStyle = ProcessWindowStyle.Normal;
            Process.Start(p);
            //int exitCode;
            //using (Process proc = Process.Start(p))
            //{
            //    proc.WaitForExit();
            //    exitCode = proc.ExitCode;
            //}
        }
    }
}
