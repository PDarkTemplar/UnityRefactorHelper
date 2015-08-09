using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text.RegularExpressions;
using UnityRefactorHelper.Model;
using UnityRefactorHelper.Service;

namespace UnityRefactorHelper.Helpers
{
    public class UnityProjectFileIdReplacer
    {
        private readonly string _vsProject;

        public UnityProjectFileIdReplacer(string vsProject)
        {
            _vsProject = vsProject;
        }

        public void Replace()
        {
            var scanProject = Cache.Instance.ScanProjects.FirstOrDefault(x => x.ProjectName == _vsProject);
            if (scanProject == null) return;

            ProcessFiles(scanProject);
        }

        private void ProcessFiles(ProjectScanItem projectScanItem)
        {
            var path = Cache.Instance.Settings.UnityProjectPath;
            var scenes = GetFilePathes(path, "*.unity");
            var prefabs = GetFilePathes(path, "*.prefab");
            var replaceIn = new List<string>();
            replaceIn.AddRange(scenes);
            replaceIn.AddRange(prefabs);

            ProcessReplace(replaceIn, projectScanItem);
            ProcessProjectScanItem(projectScanItem);
        }

        private IEnumerable<string> GetFilePathes(string parentPath, string mask)
        {
            var filenames = Directory.GetFiles(parentPath, mask, SearchOption.AllDirectories);
            return filenames;
        }

        private void ProcessReplace(IEnumerable<string> replacePathes, ProjectScanItem projectScanItem)
        {
            foreach (var replacePath in replacePathes)
            {
                var lines = File.ReadAllLines(replacePath);
                var newLines = new List<string>();
                foreach (var line in lines)
                {
                    var isFound = false;
                    foreach (var file in projectScanItem.Files.Where(x => x.NewId.HasValue))
                    {
                        var r = Regex.Match(line,
                            $"  m_Script: {{fileID: {file.OldId}, guid: {projectScanItem.ProjectGuid.ToString("n").ToLower()}, type: 3}}",
                            RegexOptions.IgnoreCase);
                        if (r.Success)
                        {
                            newLines.Add(
                                r.Result(
                                    $"  m_Script: {{fileID: {file.NewId}, guid: {projectScanItem.ProjectGuid.ToString("n").ToLower()}, type: 3}}"));
                            isFound = true;
                            break;
                        }
                    }
                    if (!isFound)
                        newLines.Add(line);
                }
                File.WriteAllLines(replacePath, newLines.ToArray());
            }
        }

        private void ProcessProjectScanItem(ProjectScanItem projectScanItem)
        {
            var renamed = projectScanItem.Files.Where(x => x.NewId.HasValue);
            foreach (var r in renamed)
            {
                r.OldId = r.NewId.Value;
                r.NewId = null;
            }

            CommonService.SaveSyncProjectItemsToCache();
        }
    }
}