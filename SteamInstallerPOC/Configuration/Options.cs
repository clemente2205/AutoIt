using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using NDesk.Options;

namespace SteamInstallerPOC.Configuration
{
    internal class Options
    {
        /// <summary>
        /// Setup SteamUsername
        /// </summary>
        public string SteamUsername { get; set; }

        /// <summary>
        /// Setup SteamPassword
        /// </summary>
        public string SteamPassword { get; set; }

        /// <summary>
        /// Setup LeaveSteamOnlineAfterSetup
        /// </summary>
        public string LeaveSteamOnlineAfterSetup { get; set; }

        /// <summary>
        /// Setup CloudSync
        /// </summary>
        public string CloudSync { get; set; }

        internal OptionSet OptionSet { get; set; }

        public static Options Parse(string[] optionArgs)
        {
            if (optionArgs == null || optionArgs.Length == 0)
            {
                var rawOptions = new Options()
                {
                    SteamUsername = "abstractavogadro",
                    SteamPassword = "5ffHRpPzStbH",
                    LeaveSteamOnlineAfterSetup = "false",
                    CloudSync = "false"
                };
                return rawOptions;
            }

            var options = new Options();
            var optionSet = new OptionSet
            {
                { "steam-username=|e", "Steam username", v => options.SteamUsername = v },
                { "steam-password=|p", "Steam password", v => options.SteamPassword = v },
                { "leave-Steam-Online-After-Setup=|p", "Steam online", v => options.LeaveSteamOnlineAfterSetup = v },
                { "cloud-sync=|p", "Cloud sync", v => options.CloudSync = v }
            };
            optionSet.Parse(optionArgs);
            options.OptionSet = optionSet;
            return options;
        }
    }
}
