using System.IO;
using System.Linq;
using EnvDTE;
using Microsoft.VisualStudio;
using Microsoft.VisualStudio.Shell.Interop;
using UnityRefactorHelper.Service;
using UnityRefactorHelper.ViewModel;

namespace UnityRefactorHelper
{
    public class SolutionEvents : IVsSolutionEvents, IVsSolutionLoadEvents
    {
        public int OnAfterOpenSolution(object pUnkReserved, int fNewSolution)
        {
            ToolWindowViewModel.Instance.LoadSettings();
            return VSConstants.S_OK;
        }

        public int OnAfterCloseSolution(object pUnkReserved)
        {
            CommonService.HandleMenuButton(false);
            return VSConstants.S_OK;
        }

        public int OnAfterOpenProject(IVsHierarchy pHierarchy, int fAdded)
        {
            var project = GetProjectFromHierarchy(pHierarchy);
            if (!string.IsNullOrEmpty(project?.FileName))
            {
                ToolWindowViewModel.Instance.ProjectNames.Add(Path.GetFileNameWithoutExtension(project.FileName));
                ProjectService.ProcessProject(project);
            }
            return VSConstants.S_OK;
        }

        public int OnBeforeCloseProject(IVsHierarchy pHierarchy, int fRemoved)
        {
            var project = GetProjectFromHierarchy(pHierarchy);
            if (project != null)
            {
                var name = Path.GetFileNameWithoutExtension(project.FileName);
                var item = ToolWindowViewModel.Instance.ProjectNames.FirstOrDefault(x => x == name);
                if (item != null)
                {
                    ToolWindowViewModel.Instance.ProjectNames.Remove(item);
                }
            }
            return VSConstants.S_OK;
        }

        public int OnAfterBackgroundSolutionLoadComplete()
        {
            CommonService.HandleMenuButton(true);
            return VSConstants.S_OK;
        }

        private Project GetProjectFromHierarchy(IVsHierarchy pHierarchy)
        {
            const uint itemid = VSConstants.VSITEMID_ROOT;

            object objProj;
            pHierarchy.GetProperty(itemid, (int) __VSHPROPID.VSHPROPID_ExtObject, out objProj);
            var project = objProj as Project;
            return project;
        }

        #region Not used

        public int OnQueryCloseSolution(object pUnkReserved, ref int pfCancel)
        {
            return VSConstants.S_OK;
        }

        public int OnBeforeCloseSolution(object pUnkReserved)
        {
            return VSConstants.S_OK;
        }

        public int OnBeforeOpenSolution(string pszSolutionFilename)
        {
            return VSConstants.S_OK;
        }

        public int OnBeforeBackgroundSolutionLoadBegins()
        {
            return VSConstants.S_OK;
        }

        public int OnQueryBackgroundLoadProjectBatch(out bool pfShouldDelayLoadToNextIdle)
        {
            pfShouldDelayLoadToNextIdle = false;
            return VSConstants.S_OK;
        }

        public int OnBeforeLoadProjectBatch(bool fIsBackgroundIdleBatch)
        {
            return VSConstants.S_OK;
        }

        public int OnAfterLoadProjectBatch(bool fIsBackgroundIdleBatch)
        {
            return VSConstants.S_OK;
        }

        public int OnQueryCloseProject(IVsHierarchy pHierarchy, int fRemoving, ref int pfCancel)
        {
            return VSConstants.S_OK;
        }

        public int OnAfterLoadProject(IVsHierarchy pStubHierarchy, IVsHierarchy pRealHierarchy)
        {
            return VSConstants.S_OK;
        }

        public int OnQueryUnloadProject(IVsHierarchy pRealHierarchy, ref int pfCancel)
        {
            return VSConstants.S_OK;
        }

        public int OnBeforeUnloadProject(IVsHierarchy pRealHierarchy, IVsHierarchy pStubHierarchy)
        {
            return VSConstants.S_OK;
        }

        #endregion
    }
}