using System;
using System.Diagnostics;
using System.Linq;
using System.Management;
using System.Text;
using System.Threading;
using System.Threading.Tasks;

namespace ProcessWatcher
{
    internal class Program
    {
        static void Main(string[] args)
        {
            Console.Title = "Process watcher";
            Console.ForegroundColor = ConsoleColor.Cyan;
            Console.WriteLine("Watching process created by Gaton");

            Console.WriteLine();

            Task.Factory.StartNew(() => StartProcessWatcher());

            Thread.Sleep(-1);
        }
        private static void StartProcessWatcher()
        {
            try
            {
                ManagementEventWatcher startWatch = new ManagementEventWatcher(new WqlEventQuery("SELECT * FROM Win32_ProcessStartTrace"));
                startWatch.EventArrived += new EventArrivedEventHandler(startWatch_EventArrived);
                startWatch.Start();
            }
            catch (Exception ex)
            {
                Console.WriteLine($"Failed to start process watcher: {ex}");
            }
        }

        private static void startWatch_EventArrived(object sender, EventArrivedEventArgs e)
        {
            StringBuilder sb = new StringBuilder();

            try
            {
                foreach (var info in e.NewEvent.Properties)
                {
                    sb.Append("\"" + info.Name + "\" ");

                    if (info.Value is byte[])
                    {
                        sb.AppendLine("\"" + Convert.ToBase64String((byte[])info.Value) + "\"");
                    }
                    else
                    { 
                        sb.AppendLine("\"" + info.Value + "\"");
                    }
                }
            }
            catch (Exception ex)
            {
                sb.AppendLine(ex.ToString());
            }

            Process ProcessStarted = Process.GetProcessById(int.Parse(e.NewEvent.Properties["ProcessID"].Value.ToString()));

            ProcessStarted.Exited += ProcessExited;

            sb.AppendLine();
            sb.AppendLine("Argumentos: " + Program.GetCommandLine(ProcessStarted));
            sb.AppendLine();

            Console.ForegroundColor = ConsoleColor.Blue;
            Console.WriteLine("Processo iniciado:");

            Console.ForegroundColor = ConsoleColor.White;
            Console.WriteLine(sb.ToString());
        }
        public static string GetCommandLine(Process process) => new ManagementObjectSearcher("SELECT CommandLine FROM Win32_Process WHERE ProcessId = " + process.Id).Get().Cast<ManagementBaseObject>().SingleOrDefault()?["CommandLine"]?.ToString();
        internal static void ProcessExited(object sender, System.EventArgs e)
        {
            Console.WriteLine("Proces exited " + e.ToString());
        }
    }
}
