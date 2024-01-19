﻿using System.IO;
using System.Diagnostics;
using EmulatorLauncher.Common;
using EmulatorLauncher.Common.FileFormats;

namespace EmulatorLauncher
{
    class SSFGenerator : Generator
    {
        public override System.Diagnostics.ProcessStartInfo Generate(string system, string emulator, string core, string rom, string playersControllers, ScreenResolution resolution)
        {
            string path = AppConfig.GetFullPath("ssf");

            string exe = Path.Combine(path, "SSF.exe");
            if (!File.Exists(exe))
                return null;

            if (!SystemConfig.isOptSet("saturn_custom_setting") || !SystemConfig.getOptBoolean("saturn_custom_setting"))
                SetupConfiguration(path);

            return new ProcessStartInfo()
            {
                FileName = exe,
                WorkingDirectory = path,
                Arguments = "\"" + rom + "\"",
            };
        }

        private void SetupConfiguration(string path)
        {
            bool fullscreen = !IsEmulationStationWindowed() || SystemConfig.getOptBoolean("forcefullscreen");

            // Delete interfering files
            string gameSettingsPath = Path.Combine(path, "Setting", "Saturn");
            if (Directory.Exists(gameSettingsPath))
                Directory.Delete(gameSettingsPath, true);
            
            string settingFile = Path.Combine(path, "Setting.ini");
            if (File.Exists(settingFile))
                File.Delete(settingFile);

            // SSF.ini file
            string iniFile = Path.Combine(path, "SSF.ini");

            using (var ini = IniFile.FromFile(iniFile, IniOptions.KeepEmptyValues | IniOptions.KeepEmptyLines))
            {
                if (SystemConfig.isOptSet("use_external_bios") && SystemConfig.getOptBoolean("use_external_bios"))
                {
                    string saturnBios = Path.Combine(AppConfig.GetFullPath("bios"), "saturn_bios.bin");
                    if (File.Exists(saturnBios))
                        ini.WriteValue("Peripheral", "SaturnBIOS", "\"" + saturnBios + "\"");
                }
                else
                    ini.WriteValue("Peripheral", "SaturnBIOS", "\"" + "" + "\"");

                if (fullscreen)
                    ini.WriteValue("Other", "ScreenMode", "\"" + "1" + "\"");
                else
                    ini.WriteValue("Other", "ScreenMode", "\"" + "0" + "\"");

                if (SystemConfig.isOptSet("saturn_region") && !string.IsNullOrEmpty(SystemConfig["saturn_region"]))
                {
                    ini.WriteValue("Peripheral", "Areacode", "\"" + SystemConfig["saturn_region"] + "\"");
                    ini.WriteValue("Peripheral", "AutoAreacode", "\"" + "0" + "\"");
                }
                else
                    ini.WriteValue("Peripheral", "AutoAreacode", "\"" + "1" + "\"");

                if (SystemConfig.isOptSet("smooth") && !SystemConfig.getOptBoolean("smooth"))
                    ini.WriteValue("Screen", "BilinearFiltering", "\"" + "0" + "\"");
                else
                    ini.WriteValue("Screen", "BilinearFiltering", "\"" + "1" + "\"");

                if (SystemConfig.isOptSet("vsync") && !SystemConfig.getOptBoolean("vsync"))
                {
                    ini.WriteValue("Screen", "VSynchWaitFullscreen", "\"" + "0" + "\"");
                    ini.WriteValue("Screen", "VSynchWaitWindow", "\"" + "0" + "\"");
                }
                else
                {
                    ini.WriteValue("Screen", "VSynchWaitFullscreen", "\"" + "1" + "\"");
                    ini.WriteValue("Screen", "VSynchWaitWindow", "\"" + "1" + "\"");
                }

                if (SystemConfig.isOptSet("ssf_scanlines") && SystemConfig.getOptBoolean("ssf_scanlines"))
                {
                    ini.WriteValue("Screen", "DisableFullscreenScanline", "\"" + "0" + "\"");
                    ini.WriteValue("Screen", "Scanline", "\"" + "1" + "\"");
                    ini.WriteValue("Screen", "ScanlineRatio", "\"" + "70" + "\"");
                }
                else
                {
                    ini.WriteValue("Screen", "DisableFullscreenScanline", "\"" + "1" + "\"");
                    ini.WriteValue("Screen", "Scanline", "\"" + "0" + "\"");
                }

                BindBoolIniFeature(ini, "Screen", "WideScreen", "saturn_widescreen", "\"" + "1" + "\"", "\"" + "0" + "\"");
            }
        }
    }
}
