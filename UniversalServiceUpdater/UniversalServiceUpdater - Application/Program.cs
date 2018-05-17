using Ionic.Zip;
using System;
using System.Diagnostics;
using System.IO;
using System.Linq;
using System.Net;
using System.Threading;
using System.Windows.Forms;
using UniversalServiceUpdater;

namespace UniversalServiceUpdater___Application
{
    static class Program
    {

        #region Vars

        //Vars that have to be changed
        private static string ApplicationSearchName;
        private static string ApplicationDirectory;
        private static string DownloadURI;
        private static Process updateProgram;

        #endregion
        /// <summary>
        /// The main entry point for the application.
        /// </summary>
        [STAThread]
        static void Main(string[] args)
        {
            ILogger.SetLoggingEvents();

            //Set local param
            ApplicationSearchName = args[0];
            ApplicationDirectory = args[1];
            DownloadURI = args[2];

            ILogger.AddToLog("info", "Application Name : " + ApplicationSearchName);
            ILogger.AddToLog("info", "Application Directory : " + ApplicationDirectory);
            ILogger.AddToLog("info", "DownloadURI : " + DownloadURI);

            //Get process
            updateProgram = Process.GetProcessesByName(ApplicationSearchName).FirstOrDefault();

            ILogger.AddToLog("info", "Got process! Verifying");
            if (updateProgram == null) { ILogger.AddToLog("error", "Process is null!"); }

            if (File.Exists(Application.StartupPath + "\\" + ApplicationSearchName + ".zip")) File.Delete(Application.StartupPath + "\\" + ApplicationSearchName + ".zip");

            DownloadUpdate();
            UpdateLoop();
        }

        /// <summary>
        /// Download the update in a temp directory
        /// </summary>
        private static void DownloadUpdate()
        {
            ILogger.AddToLog("info", "Downloading update");

            using (WebClient webClient = new WebClient())
            {
                //Gets the download directory
                Uri URL = new Uri(DownloadURI);

                try
                {
                    webClient.DownloadFile(URL, Application.StartupPath + "\\" + ApplicationSearchName + ".zip");
                }
                catch { }
            }

            ILogger.AddToLog("info", "Update done!");
        }

        /// <summary>
        /// Tries to update the program
        /// </summary>
        private static void UpdateLoop()
        {
            Thread CheckApplicationThread = new Thread(new ThreadStart(() =>
            {
                ILogger.AddToLog("info", "Running loop");
                while (true)
                {
                    Console.WriteLine("In Loop");
                    //Check if the program has exit.
                    if (updateProgram.HasExited)
                    {
                        //Verifying that all instances of this application has exited.
                        updateProgram = Process.GetProcessesByName(ApplicationSearchName).FirstOrDefault();

                        if (updateProgram == null)
                        {
                            ILogger.AddToLog("info", "Application exit!");
                            //Extracts the update
                            ExtractUpdate();
                        }
                    }

                    Thread.Sleep(5000);
                }
            }));

            CheckApplicationThread.Start();
        }

        /// <summary>
        /// Extracts the update and then closes
        /// </summary>
        private static void ExtractUpdate()
        {
            while (true)
            {
                try
                {
                    ILogger.AddToLog("info", "Deleting the directory and remaking it");
                    //Deletes all files in a directory
                    DirectoryInfo di = new DirectoryInfo(ApplicationDirectory);

                    foreach (FileInfo file in di.GetFiles())
                    {
                        file.Delete();
                    }
                    foreach (DirectoryInfo dir in di.GetDirectories())
                    {
                        dir.Delete(true);
                    }

                    break;
                }
                catch { }
            }

            ILogger.AddToLog("info", "Extracting update!");
            //Extracts all files into the application directory
            using (ZipFile zip = ZipFile.Read(Application.StartupPath + "\\" + ApplicationSearchName + ".zip"))
            {
                zip.ExtractAll(ApplicationDirectory);
            }

            ILogger.AddToLog("info", "Extract update!");

            Environment.Exit(1);
        }
    }
}
