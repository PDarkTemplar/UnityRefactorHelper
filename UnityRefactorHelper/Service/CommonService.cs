using System;
using System.Collections.Generic;
using System.ComponentModel.Design;
using System.IO;
using Newtonsoft.Json;
using UnityRefactorHelper.Helpers;
using UnityRefactorHelper.Model;
using UnityRefactorHelper.ViewModel;

namespace UnityRefactorHelper.Service
{
    public static class CommonService
    {
        private const string SettingsFileName = "UnityRefactorSettings.json";
        private const string SettingsCacheFileName = "UnityRefactorSettingsCache.bin";

        public static void HandleMenuButton(bool enabled)
        {
            var mc =
                Cache.Instance.OleMenuCommandService.FindCommand(new CommandID(Constants.CommandSet, Constants.CommandId));
            if (mc != null)
                mc.Enabled = enabled;
            ToolWindowViewModel.Instance.SolutionLoaded = enabled;
        }

        public static void SaveSettings()
        {
            var dte = Cache.Instance.Dte;

            var path = Path.Combine(Path.GetDirectoryName(dte.Solution.FullName), SettingsFileName);
            File.WriteAllText(path, JsonConvert.SerializeObject(Cache.Instance.Settings));
        }

        public static void LoadSettings()
        {
            var dte = Cache.Instance.Dte;

            var path = Path.Combine(Path.GetDirectoryName(dte.Solution.FullName), SettingsFileName);
            if (File.Exists(path))
            {
                var content = File.ReadAllText(path);
                Cache.Instance.Settings = JsonConvert.DeserializeObject<Settings>(content);
            }
        }

        public static void SaveSyncProjectItemsToCache()
        {
            var dte = Cache.Instance.Dte;
            var scanProjects = Cache.Instance.ScanProjects;
            var path = Path.Combine(Path.GetDirectoryName(dte.Solution.FullName), SettingsCacheFileName);
            File.WriteAllText(path, JsonConvert.SerializeObject(scanProjects));
        }

        public static void LoadSyncProjectsCache()
        {
            var dte = Cache.Instance.Dte;
            var path = Path.Combine(Path.GetDirectoryName(dte.Solution.FullName), SettingsCacheFileName);
            if (File.Exists(path))
            {
                var content = File.ReadAllText(path);
                Cache.Instance.ScanProjects = JsonConvert.DeserializeObject<List<ProjectScanItem>>(content);
            }
        }

        public static void UpdateUnityProject(string vsProject)
        {
            if(!Cache.Instance.Settings.IsEnabled) return;
            var projectName = Path.GetFileNameWithoutExtension(vsProject);
            var replacer = new UnityProjectFileIdReplacer(projectName);
            replacer.Replace();
        }
    }
}