using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using EnvDTE;
using EnvDTE80;
using Microsoft.VisualStudio;
using Microsoft.VisualStudio.Shell.Interop;
using UnityRefactorHelper.Helpers;
using UnityRefactorHelper.Model;

namespace UnityRefactorHelper.Service
{
    public static class ProjectService
    {
        private const string PersistentFileGuidName = "PersistentFileGuid";

        public static void ProcessProject(Project project)
        {
            var syncItem = Cache.Instance.ScanProjects.FirstOrDefault(x => x.ProjectName == project.Name);
            if (syncItem == null) return;

            IVsHierarchy projectHierarchy;

            if (Cache.Instance.SolutionService.GetProjectOfUniqueName(project.UniqueName, out projectHierarchy) ==
                VSConstants.S_OK)
            {
                if (projectHierarchy != null)
                {
                    var fileGuids = NavigateProjectItems(project.ProjectItems, syncItem, projectHierarchy);
                    var notFound = syncItem.Files.Where(x => !fileGuids.Contains(x.ProjectFileId)).ToList();

                    foreach (var nf in notFound)
                    {
                        syncItem.Files.Remove(nf);
                    }
                }
            }
        }
        public static void ProcessNewProject(ProjectSyncItem newItem)
        {
            var project = Projects().FirstOrDefault(x => x.Name == newItem.ProjectName);
            if (project != null)
                ProcessProject(project);
        }

        public static void DocumentSaved(Document document)
        {
            if (!Cache.Instance.Settings.IsEnabled) return;

            var item = document?.ProjectItem;

            var project = item?.ContainingProject;
            if (project != null)
            {
                var syncItem = Cache.Instance.ScanProjects.FirstOrDefault(x => x.ProjectName == project.Name);
                if (syncItem == null) return;

                IVsHierarchy projectHierarchy;

                if (Cache.Instance.SolutionService.GetProjectOfUniqueName(project.UniqueName, out projectHierarchy) ==
                    VSConstants.S_OK)
                {
                    if (projectHierarchy != null)
                    {
                        ExamineProjectItem(item, syncItem, projectHierarchy);
                    }
                }
            }
        }

        private static IEnumerable<string> NavigateProjectItems(ProjectItems projectItems, ProjectScanItem projectScan,
            IVsHierarchy projectHierarchy)
        {
            var fileGuids = new List<string>();

            if (projectItems == null)
                return fileGuids;

            foreach (ProjectItem item in projectItems)
            {
                fileGuids.AddRange(NavigateProjectItems(item.ProjectItems, projectScan, projectHierarchy));

                if (item.Kind != "{6BB5F8EE-4483-11D3-8BCF-00C04F8EC28C}") // VSConstants.GUID_ItemType_PhysicalFile
                    continue;

                var fileGuid = ExamineProjectItem(item, projectScan, projectHierarchy);
                if (!string.IsNullOrEmpty(fileGuid))
                    fileGuids.Add(fileGuid);
            }

            return fileGuids;
        }

        private static string ExamineProjectItem(ProjectItem item, ProjectScanItem projectScan,
            IVsHierarchy projectHierarchy)
        {
            var fileName = item.FileNames[1];
            if (Path.GetExtension(fileName) != ".cs") return null;

            var code = item.FileCodeModel;
            if (code == null) return null;

            var className = string.Empty;
            var namespaceName = string.Empty;

            foreach (CodeElement codeElement in code.CodeElements)
            {
                if (string.IsNullOrEmpty(className))
                    className = ExamineCodeElement(codeElement, vsCMElement.vsCMElementClass);
                if (string.IsNullOrEmpty(namespaceName))
                    namespaceName = ExamineCodeElement(codeElement, vsCMElement.vsCMElementNamespace);

                if (!string.IsNullOrEmpty(className) && !string.IsNullOrEmpty(namespaceName))
                    break;
            }

            if (string.IsNullOrEmpty(className)) return null;
            uint itemId;
            if (projectHierarchy.ParseCanonicalName(item.FileNames[0], out itemId) == VSConstants.S_OK)
            {
                var buildPropertyStorage =
                    projectHierarchy as IVsBuildPropertyStorage;

                if (buildPropertyStorage == null) return null;
                string fileGuid;
                buildPropertyStorage.GetItemAttribute(itemId, PersistentFileGuidName, out fileGuid);
                if (string.IsNullOrEmpty(fileGuid))
                {
                    fileGuid = Guid.NewGuid().ToString();
                    buildPropertyStorage.SetItemAttribute(itemId, PersistentFileGuidName, fileGuid);
                    item.ContainingProject.Save();
                }

                if (string.IsNullOrEmpty(fileGuid)) return null;

                var file = projectScan.Files.FirstOrDefault(x => x.ProjectFileId == fileGuid);

                var fileId = FileIdGenerator.Compute(namespaceName, className);

                if (file == null)
                {
                    projectScan.Files.Add(new FileScanItem { ProjectFileId = fileGuid, OldId = fileId });
                }
                else
                {
                    var newId = fileId;
                    if (file.OldId != newId)
                        file.NewId = newId;
                }

                return fileGuid;
            }

            return null;
        }

        private static string ExamineCodeElement(CodeElement codeElement, vsCMElement type)
        {
            return codeElement.Kind == type
                ? codeElement.Name
                : (from CodeElement childElement in codeElement.Children select ExamineCodeElement(childElement, type))
                    .FirstOrDefault(result => !string.IsNullOrEmpty(result));
        }

        private static IEnumerable<Project> Projects()
        {
            var projects = Cache.Instance.Dte.Solution.Projects;
            var list = new List<Project>();
            var item = projects.GetEnumerator();
            while (item.MoveNext())
            {
                var project = item.Current as Project;
                if (project == null)
                {
                    continue;
                }

                if (project.Kind == ProjectKinds.vsProjectKindSolutionFolder)
                {
                    list.AddRange(GetSolutionFolderProjects(project));
                }
                else
                {
                    list.Add(project);
                }
            }

            return list;
        }

        private static IEnumerable<Project> GetSolutionFolderProjects(Project solutionFolder)
        {
            var list = new List<Project>();
            for (var i = 1; i <= solutionFolder.ProjectItems.Count; i++)
            {
                var subProject = solutionFolder.ProjectItems.Item(i).SubProject;
                if (subProject == null)
                {
                    continue;
                }

                // If this is another solution folder, do a recursive call, otherwise add
                if (subProject.Kind == ProjectKinds.vsProjectKindSolutionFolder)
                {
                    list.AddRange(GetSolutionFolderProjects(subProject));
                }
                else
                {
                    list.Add(subProject);
                }
            }
            return list;
        }
    }
}
