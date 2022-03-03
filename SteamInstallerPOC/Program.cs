using System;
using System.Drawing.Imaging;
using System.IO;
using System.Threading;
using AutoItX3Lib;
using Serilog;
using SteamInstallerPOC.Helpers;
using SteamInstallerPOC.Configuration;
using System.Diagnostics;
using AutoIt;

namespace SteamInstallerPOC
{
    /// <summary>
    /// Install Steam Library using AutoItX dll as a Proof of Concept
    /// </summary>
    internal class SteamInstallerPOC
    {
        private const string SteamFilePath = @"C:\Program Files (x86)\Steam\steam.exe";
        private const string logFile = "SteamSetup.log";
        private static string dir = Directory.CreateDirectory("SteamSetupLog").ToString();
        private static string screenshotsDirectory = Directory.CreateDirectory(Path.Combine(dir, "SteamSetupScreenshots")).ToString();  
        private static AutoItX3 auto = new AutoItX3();
        private static Options _options;

        public static void Main(string[] args)
        {
            InitializeLogger();
            ReadCommandLineArguments(args);
            InstallSteam();
            LoginSteam();
            //ToDO
            SetUpModeSteam();
            //ToDo
            GameSetupProcess();
            SteamShutdown();
            Environment.Exit(0);
        }

        private static void InitializeLogger()
        {
            Log.Logger = new LoggerConfiguration()
                        .WriteTo.Console()
                        .WriteTo.File(Path.Combine(dir, logFile))
                        .CreateLogger();
            Log.Information("Initialize SteamInstallerPOC");
        }

        private static void ReadCommandLineArguments(in string[] args)
        {
            Log.Information("Parsing arguments...");
            _options = Options.Parse(args);
            Log.Information("Arguments parsed");
        }

        private static void InstallSteam()
        {
            Log.Information(nameof(InstallSteam));
            if (File.Exists(SteamFilePath))
            {
                Log.Information("Steam is already installed");
                return;
            }
            try
            {
                string steamInstallerFilePath = Path.Combine(Directory.GetCurrentDirectory(), "SteamSetup.exe");
                Log.Information("Installing Steam...");
                Execute(steamInstallerFilePath, "/S");
            }
            catch (Exception)
            {
                throw new ApplicationException(
                    $"Unable to install Steam");

            }
            Log.Information("Steam has been installed correctly");

        }

        private static void LoginSteam()
        {
            Log.Information(nameof(LoginSteam));
            var args = "-login " + _options.SteamUsername + " " + _options.SteamPassword + " -silent";
            Execute(SteamFilePath, args, waitForExit:false);
            Log.Information("Login succesful");
        }

        private static void SetUpModeSteam()
        {
            //SaveSteamDataAccountId
            //ProcessingSteamworksCommonRedistributablesUpdates
            //SetupOfflineMode
        }

        private static void GameSetupProcess()
        {
            Log.Information(nameof(GameSetupProcess));
            //ToDO Add GameSetup
        }

        private static void SteamShutdown()
        {
            Log.Information(nameof(SteamShutdown));
            if (auto.ProcessExists("steam.exe") == 0)
            {
                Log.Information("Steam is closed.");
                return;
            }

            for (int i = 0; i < 15; i++)
            {
                Execute(SteamFilePath, "-shutdown");
                Wait(15);
                if (auto.ProcessExists("steam.exe") != 1)
                {
                    Log.Information("Steam is closed.");
                    return;
                }
            }
            throw new ApplicationException(
                                $"Steam process cannot be closed.");
        }

        private static void Wait(double seconds) => Thread.Sleep(TimeSpan.FromSeconds(seconds));

        private static void TakeScreenshot()
        {
            var name = "SteamSetup " + DateTime.Now.ToString("u") + ".jpg";
            var image = ScreenCapture.CaptureDesktop();
            image.Save(Path.Combine(screenshotsDirectory,name), ImageFormat.Jpeg);
        }

        private static void Execute(string filePath, string arguments = null, bool waitForExit = true)
        {
            /*
             * Note: AutoIt dll is unable to execute shell process, so it's needed to utilize a Process to pass arguments
             * In the actual SteamSetup.au3 a ShellExecute is called, but this cannot be called from AutoItX3 dll
            */
            var pr = new Process
            {
                StartInfo = new ProcessStartInfo
                {
                    UseShellExecute = true,
                    WorkingDirectory = Path.GetDirectoryName(filePath),
                    FileName = Path.GetFileNameWithoutExtension(filePath),
                    Arguments = arguments ?? string.Empty
                }
            };

            if (!pr.Start())
            {
                throw new ApplicationException(
                    $"The process {filePath} with arguments {arguments} has not been started.");
            }

            if (!waitForExit) return;

            pr.WaitForExit();

            if (pr.ExitCode != 0)
            {
                Log.Fatal("An error has occurred");
                throw new ApplicationException(
                    $"The process {filePath} ended with error code {pr.ExitCode}");
            }
        }

    }
}
