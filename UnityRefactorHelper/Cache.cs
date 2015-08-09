using System;
using System.Collections.Generic;
using EnvDTE;
using Microsoft.VisualStudio.Shell;
using Microsoft.VisualStudio.Shell.Interop;
using UnityRefactorHelper.Model;

namespace UnityRefactorHelper
{
    public class Cache : IDisposable
    {
        static Cache()
        {
            Instance = new Cache();
        }

        private Cache()
        {
            ScanProjects = new List<ProjectScanItem>();
        }

        public OleMenuCommandService OleMenuCommandService { get; set; }
        public DTE Dte { get; set; }
        public IVsSolution SolutionService { get; set; }

        public Settings Settings { get; set; }
        public List<ProjectScanItem> ScanProjects { get; set; }

        public static Cache Instance { get; private set; }

        public void Dispose()
        {
            SolutionService = null;
            Dte = null;
            OleMenuCommandService = null;
        }
    }
}