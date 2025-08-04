using System.IO;
using System.Windows.Forms;

namespace Cheviren
{
    internal static class Program
    {
        /// <summary>
        ///  The main entry point for the application.
        /// </summary>
        [STAThread]
        static void Main()
        {
            TryAddToStartup();
            // To customize application configuration such as set high DPI settings or default font,
            // see https://aka.ms/applicationconfiguration.
            ApplicationConfiguration.Initialize();
            Application.Run(new Form1());
        }

        static void TryAddToStartup()
        {
            try
            {
                string startupFolder = Environment.GetFolderPath(Environment.SpecialFolder.Startup);
                string exePath = Application.ExecutablePath;
                string shortcutPath = Path.Combine(startupFolder, "Cheviren.lnk");

                if (!File.Exists(shortcutPath))
                {
                    Type shellType = Type.GetTypeFromProgID("WScript.Shell");
                    dynamic shell = Activator.CreateInstance(shellType);
                    dynamic shortcut = shell.CreateShortcut(shortcutPath);
                    shortcut.TargetPath = exePath;
                    shortcut.WorkingDirectory = Path.GetDirectoryName(exePath);
                    shortcut.IconLocation = exePath; // Uygulamanın kendi ikonu
                    shortcut.Save();
                }
            }
            catch
            {
                // Hata oluşursa sessizce geç
            }
        }
    }
}