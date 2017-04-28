using System;
using System.Runtime.InteropServices;
using System.Windows.Forms;
using System.Diagnostics;
using System.IO;
using System.Media;

namespace ConsoleApplication1
{
    class Program
    {
        static float startTime;
        static float time;

        static string version = "1.23";

        static int startSeconds;

        //Config variables
        static int minsToSongReboot;
        static int minsToSystemReboot;
        static string foobarLocation;
        static string obsLocation;
        static string bipLocation;

        [DllImport("user32.dll")]
        private static extern IntPtr FindWindow(string lpClassName, string lpWindowName);

        [DllImport("user32.dll")]
        private static extern bool SetForegroundWindow(IntPtr hWnd);

        [STAThread]
        static void Main(string[] args)
        {
            //CONFIG
            string currentPath = Path.GetDirectoryName(Application.ExecutablePath);

            if (File.Exists(currentPath + "\\radioConfig.txt") == false)
            {
                CreateConfig();
            }

            string configLocation = currentPath + "\\radioConfig.txt";

            //Se chequea si la config es de la misma versión.
            string[] lines = File.ReadAllLines(currentPath + "\\radioConfig.txt");
            if (lines[1] != "v" + version)
            {
                File.Delete(configLocation);
                CreateConfig();
            }
            //VARS FROM CONFIG
            for (int lineIndex = 0; lineIndex < lines.Length; lineIndex++)
            {
                if (lines[lineIndex].Contains("="))
                {
                    string[] words = lines[lineIndex].Split('=');
                    if ("minsToSongReboot" == words[0])
                    {
                        minsToSongReboot = Int16.Parse(words[1]);
                    }
                    if ("minsToSystemReboot" == words[0])
                    {
                        minsToSystemReboot = Int16.Parse(words[1]);
                    }
                    if ("foobarLocation" == words[0])
                    {
                        foobarLocation = words[1];
                    }
                    if ("obsLocation" == words[0])
                    {
                        obsLocation = words[1];
                    }
                    if ("bipLocation" == words[0])
                    {
                        bipLocation = words[1];
                    }
                }
            }

            //CODE STARTS
            Console.WriteLine("OnlyDallas Radio - v" + version);
            Console.WriteLine("-----------------------");
            Console.WriteLine("Las canciones son reiniciadas cada " + minsToSongReboot + " minutos");
            Console.WriteLine("El sistema es reiniciado cada " + minsToSystemReboot + " minutos");
            Console.WriteLine("Se inicia a las " + DateTime.Now.ToString("MM:dd:hh:mm:ss:fff"));

            //Se ajustan los timers.
            startTime = Int64.Parse(DateTime.Now.ToString("MMddhhmm"));
            startSeconds = DateTime.Now.Second * 1000 + DateTime.Now.Millisecond;

            //Se inicia foobar2000 y se lo pone como background.
            Process.Start(@foobarLocation);
            Console.WriteLine("Foobar2000 esta activo");
            IntPtr zero = FindWindow(null, "   [foobar2000 v1.3.10]");

            while (true)
            {
                time = Int64.Parse(DateTime.Now.ToString("MMddhhmm"));

                //Bip del cambio de hora
                if (DateTime.Now.Minute == 59 && DateTime.Now.Second == 55)
                {
                    (new SoundPlayer(@bipLocation)).Play();
                }

                //Reinicio de canciones
                if ((time - startTime) % minsToSongReboot == 0 && startSeconds == (DateTime.Now.Second * 1000 + DateTime.Now.Millisecond))
                {
                    Console.WriteLine("Cancion reiniciada a las " + DateTime.Now.ToString("MM:dd:hh:mm:ss:fff") + " y la diferencia es " + (time - startTime));
                    SetForegroundWindow(zero);
                    SendKeys.SendWait("{ENTER}");

                }

                //Reinicio del sistema
                if ((time - startTime) % minsToSystemReboot == 0 && startSeconds == (DateTime.Now.Second * 1000 + DateTime.Now.Millisecond))
                {
                    Process.Start("ShutDown", "-r"); //restart
                }
            }
        }

        static void CreateConfig()
        {
            string currentPath = Path.GetDirectoryName(Application.ExecutablePath);
            //Create a file to write to.
            using (StreamWriter sw = File.CreateText(@currentPath + "\\radioConfig.txt"))
            {
                sw.WriteLine("OnlyDallas Radio - Config");
                sw.WriteLine("v" + version);
                sw.WriteLine("-------------------------");
                sw.WriteLine("minsToSongReboot=30");
                sw.WriteLine("minsToSystemReboot=30");
                sw.WriteLine("foobarLocation=" + currentPath);
                sw.WriteLine("obsLocation=" + currentPath);
                sw.WriteLine("bipLocation=" + currentPath);
            }
            Console.WriteLine("Un radioConfig.txt fue creado en " + currentPath);
        }
    }
}
