using System;
using System.Windows;
using System.IO;

namespace PestControlAnimator
{
    public partial class App : Application
    {
        public static string startupPath = "";

        private void Application_Startup(object sender, StartupEventArgs e)
        {
            string[] arguments = Environment.GetCommandLineArgs();

            if (arguments.GetLength(0) > 1)
            {
                if (File.Exists(arguments[1]))
                {
                    startupPath = arguments[1];
                }
            }
        }
    }
}
