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
using System.Collections.Generic;

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
            try
            {
                ReadCommandLineArguments(args);
                InstallSteam();
                LoginSteam();
                //ToDO
                SetUpModeSteam();
                //ToDo
                GameSetupProcess();
                SteamShutdown();
            }
            catch (Exception ex)
            {
                Log.Fatal("Unable to install Steam");
                Environment.Exit(1);
            }
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
            Wait(10);
            Log.Information("Login succesful");
        }

        private static void SetUpModeSteam()
        {
            SaveSteamDataAccountID();
            SetupOfflineMode();

            void SaveSteamDataAccountID()
            {
                Log.Debug(nameof(SaveSteamDataAccountID));
                var userdataDir = "C:/Program Files (x86)/Steam/userdata";
                if (!Directory.Exists(userdataDir))
                {
                    Log.Warning("Directory does not exist, unable to save data accounts");
                    return;
                }
                List<string> fileList = (List<string>) Directory.EnumerateFiles(userdataDir);
                if (fileList[0] == null)
                {
                    Log.Warning("No files were found, unable to save data accounts");
                    return;
                }
                var steamDataAccountId = fileList[1];

                //TODO

                //disable cloud auto sync

                //var enableSync = bool.Parse(_options.CloudSync) ? 1 : 0;

                //var content = '"UserRoamingConfigStore"' & @CRLF & '{' & @CRLF & @TAB & '"Software"' & @CRLF & @TAB & '{' & @CRLF & @TAB & @TAB & '"Valve"' & @CRLF & @TAB & @TAB & '{' & @CRLF & @TAB & @TAB & @TAB & '"Steam"' & @CRLF & @TAB & @TAB & @TAB & '{' & @CRLF & @TAB & @TAB & @TAB & @TAB & '"CloudEnabled"' & @TAB & @TAB & '"' & $enableSync & '"' & @CRLF & @TAB & @TAB & @TAB & '}' & @CRLF & @TAB & @TAB & '}' & @CRLF & @TAB & '}' & @CRLF & '}';

                //var dirPath = $userdataDir & '\' & $steamDataAccountId & '\7\remote'

                //var filePath = $dirPath & '\sharedconfig.vdf'


                //   OverWrite($dirPath, $filePath, $content)

                // disable news

                //$content = '"UserLocalConfigStore"' & @CRLF & '{' & @CRLF & @TAB & '"News"' & @CRLF & @TAB & '{' & @CRLF & @TAB & @TAB & '"NotifyAvailableGames"' & @TAB & @TAB & '"0"' & @CRLF & @TAB & '}' & @CRLF & '}'
                //$dirPath = $userdataDir & '\' & $steamDataAccountId & '\config'
                //$filePath = $dirPath & '\localconfig.vdf'


                //   OverWrite($dirPath, $filePath, $content)

                //   ; save < steamDataAccountId >

                //$content = '{"SteamDataAccountId":"' & $steamDataAccountId & '"}'
                //$dirPath = @ScriptDir
                //$filePath = $dirPath & '\SteamUserProfile.json'


                //   OverWrite($dirPath, $filePath, $content)
            }

            void SetupOfflineMode()
            {
                if (bool.Parse(_options.LeaveSteamOnlineAfterSetup))
                {
                    return;
                }
                Log.Debug(nameof(SetupOfflineMode));
                auto.AutoItSetOption("MouseCoordMode", 0); //Use coords relative to the current active window (https://www.autoitscript.com/autoit3/docs/functions/AutoItSetOption.htm)
	            if(auto.ProcessExists("steam.exe") == 0)
                {
                    Execute(SteamFilePath, waitForExit:false);
                }
                auto.WinWait("Steam");
                auto.WinActivate("Steam");
                Wait(1);

                auto.MouseClick("LEFT",27,18,1,0);
                Wait(1);
                TakeScreenshot();

                auto.MouseClick("LEFT", 41, 35, 1, 0);
                Wait(1);
                TakeScreenshot();

                auto.WinWait("Steam - Go Offline");
                auto.WinActivate("Steam - Go Offline");
                Wait(1);

                auto.MouseClick("LEFT", 178, 152, 1, 0);
                Wait(5);
                TakeScreenshot();
                
                Wait(10); //Sometimes the steam doesn't enough time to change the state

                auto.WinWait("Steam");

                auto.AutoItSetOption("MouseCoordMode", 1); 
            }

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
             * Note: AutoItX3 dll is unable to execute shell process by itself, so it's needed to utilize a Process to pass arguments
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
