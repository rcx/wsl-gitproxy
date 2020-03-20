using System;
using System.Collections;
using System.Collections.Generic;
using System.Diagnostics;
using System.IO;
using System.Linq;
using System.Text;
using System.Threading;
using System.Threading.Tasks;

namespace GitProxy
{
    class Program
    {
        private static readonly string wslpath = "/bin/wslpath";

        public static string Quote(IList args)
        {
            StringBuilder sb = new StringBuilder();
            foreach (string arg in args)
            {
                int backslashes = 0;

                // Add a space to separate this argument from the others
                if (sb.Length > 0)
                {
                    sb.Append(" ");
                }

                sb.Append('"');

                foreach (char c in arg)
                {
                    if (c == '\\')
                    {
                        // Don't know if we need to double yet.
                        backslashes++;
                    }
                    else if (c == '"')
                    {
                        // Double backslashes.
                        sb.Append(new String('\\', backslashes * 2));
                        backslashes = 0;
                        sb.Append("\\\"");
                    }
                    else {
                        // Normal char
                        if (backslashes > 0)
                        {
                            sb.Append(new String('\\', backslashes));
                            backslashes = 0;
                        }
                        sb.Append(c);
                    }
                }

                // Add remaining backslashes, if any.
                if (backslashes > 0)
                {
                    sb.Append(new String('\\', backslashes));
                }

                sb.Append(new String('\\', backslashes));
                sb.Append('"');
            }
            return sb.ToString();
        }

        static string ConvertPath(string arg)
        {
            arg = "'" + arg.Replace("'", "'\\''") + "'";
            var processStartInfo = new ProcessStartInfo
            {
                FileName = @"c:\windows\system32\cmd.exe",
                Arguments = "/c c:\\windows\\system32\\wsl.exe " + wslpath + " " + arg,
                UseShellExecute = false,
                RedirectStandardOutput = true,
            };
            var p = new FixedProcess();
            p.StartInfo = processStartInfo;
            p.EnableRaisingEvents = true;
            p.Start();
            string output = p.StandardOutput.ReadToEnd();
            p.WaitForExit();
            return output.Replace("\n", "");
        }

        static void ConvertPaths(string[] args)
        {
            for (int i = 0; i < args.Length; i++)
            {
                if (args[i].Length >= 3)
                {
                    if (args[i][1] == ':' && args[i][2] == '\\')
                    {
                        args[i] = ConvertPath(args[i]);
                    }
                }
            }
        }

        static void Main(string[] args)
        {
            //File.AppendAllText(@"c:\\users\\user\\debug.txt", "RAW ARGS: " + string.Join(" @ ", args) + Environment.NewLine);

            ConvertPaths(args);
            var argsList = new List<string>(args);
            string joinedArgs = Quote(argsList);

            //File.AppendAllText(@"c:\\users\\user\\debug.txt", "JOINED ARGS: " + joinedArgs + Environment.NewLine);

            string programName = Path.GetFileNameWithoutExtension(System.Diagnostics.Process.GetCurrentProcess().MainModule.FileName);

            string curDir = Environment.CurrentDirectory;
            curDir = "'" + curDir.Replace("'", "'\\''") + "'";

            bool fix = true;
            string tempStdout = null, tempStderr = null;
            if (fix)
            {
                programName = "git";
                tempStdout = Path.GetTempPath() + Guid.NewGuid().ToString() + ".tmp";
                tempStderr = Path.GetTempPath() + Guid.NewGuid().ToString() + ".tmp";
                joinedArgs += " >" + tempStdout + " 2>" + tempStderr;
            }

            var processStartInfo = new ProcessStartInfo
            {
                FileName = @"c:\windows\system32\cmd.exe",
                Arguments = "/c c:\\windows\\system32\\wsl.exe cd \"`" + wslpath + " " + curDir + "`\"; " + programName + " " + joinedArgs,
                UseShellExecute = false,
                RedirectStandardOutput = false,
            };

            var process = new Process();
            process.StartInfo = processStartInfo;
            if (fix)
            {
                process.Start();
                process.WaitForExit();
                byte[] outputBytes = File.ReadAllBytes(tempStdout);
                string output = Encoding.UTF8.GetString(outputBytes);
                byte[] stderrBytes = File.ReadAllBytes(tempStderr);
                string stderr = Encoding.UTF8.GetString(stderrBytes);
                File.Delete(tempStdout);
                File.Delete(tempStderr);
                output = output.Replace("/mnt/c/", "c:/");
                stderr = stderr.Replace("/mnt/c/", "c:/");
                Console.Write(output);
                Console.Error.Write(stderr);
                //File.AppendAllText(@"c:\\users\\user\\debug2.txt", "OUTPUT: " + output);
            }

            process.WaitForExit();
            Environment.Exit(process.ExitCode);
        }
    }
}
