using System.ComponentModel.Design;
using System.Diagnostics.CodeAnalysis;
using System.Runtime.InteropServices;
using EnvDTE;
using Microsoft.VisualStudio;
using Microsoft.VisualStudio.Shell;
using Microsoft.VisualStudio.Shell.Interop;
using UnityRefactorHelper.Helpers;
using UnityRefactorHelper.Service;

namespace UnityRefactorHelper
{
    /// <summary>
    /// This is the class that implements the package exposed by this assembly.
    /// </summary>
    /// <remarks>
    /// <para>
    /// The minimum requirement for a class to be considered a valid package for Visual Studio
    /// is to implement the IVsPackage interface and register itself with the shell.
    /// This package uses the helper classes defined inside the Managed Package Framework (MPF)
    /// to do it: it derives from the Package class that provides the implementation of the
    /// IVsPackage interface and uses the registration attributes defined in the framework to
    /// register itself and its components with the shell. These attributes tell the pkgdef creation
    /// utility what data to put into .pkgdef file.
    /// </para>
    /// <para>
    /// To get loaded into VS, the package must be referred by &lt;Asset Type="Microsoft.VisualStudio.VsPackage" ...&gt; in .vsixmanifest file.
    /// </para>
    /// </remarks>
    [PackageRegistration(UseManagedResourcesOnly = true)]
    [InstalledProductRegistration("#110", "#112", "1.0", IconResourceID = 400)] // Info on this package for Help/About
    [ProvideMenuResource("Menus.ctmenu", 1)]
    [ProvideToolWindow(typeof (SettingsToolWindow))]
    [Guid(PackageGuidString)]
    [ProvideAutoLoad(VSConstants.UICONTEXT.SolutionExists_string)]
    [SuppressMessage("StyleCop.CSharp.DocumentationRules", "SA1650:ElementDocumentationMustBeSpelledCorrectly",
        Justification = "pkgdef, VS and vsixmanifest are valid VS terms")]
    public sealed class UnityRefactorHelperSettingsPackage : Package
    {
        /// <summary>
        /// UnityRefactorHelperSettingsPackage GUID string.
        /// </summary>
        public const string PackageGuidString = "704c324b-a17d-45b6-bb8d-3aacff371f0a";

        private BuildEvents _buildEvents;
        private DocumentEvents _documentEvents;
        private IVsSolutionEvents _solutionEvents;
        private IVsSolution _solution;
        private uint _solutionEventsCookie;

        #region Package Members

        /// <summary>
        /// Initialization of the package; this method is called right after the package is sited, so this is the place
        /// where you can put all the initialization code that rely on services provided by VisualStudio.
        /// </summary>
        protected override void Initialize()
        {
            UnityRefactorHelperSettingsCommand.Initialize(this);
            base.Initialize();

            Cache.Instance.OleMenuCommandService = GetService(typeof (IMenuCommandService)) as OleMenuCommandService;
            Cache.Instance.Dte = (DTE) GetService(typeof (SDTE));

            var events = Cache.Instance.Dte.Events;
            _documentEvents = events.DocumentEvents;
            _buildEvents = events.BuildEvents;

            Cache.Instance.SolutionService = GetService(typeof (IVsSolution)) as IVsSolution;

            _solutionEvents = new SolutionEvents();
            _solution = GetService(typeof (SVsSolution)) as IVsSolution;
            _solution.AdviseSolutionEvents(_solutionEvents, out _solutionEventsCookie);

            _documentEvents.DocumentSaved += DocumentEventsOnDocumentSaved;
            _buildEvents.OnBuildProjConfigDone += BuildEventsOnOnBuildProjConfigDone;
        }

        private void BuildEventsOnOnBuildProjConfigDone(string project, string projectConfig, string platform,
            string solutionConfig, bool success)
        {
            if (success)
                CommonService.UpdateUnityProject(project);
        }

        protected override void Dispose(bool disposing)
        {
            UnadviseSolutionEvents();
            Cache.Instance.Dispose();
            _buildEvents = null;
            _documentEvents = null;
            _solutionEvents = null;
            base.Dispose(disposing);
        }

        private void UnadviseSolutionEvents()
        {
            if (_solution != null)
            {
                if (_solutionEventsCookie != uint.MaxValue)
                {
                    _solution.UnadviseSolutionEvents(_solutionEventsCookie);
                    _solutionEventsCookie = uint.MaxValue;
                }

                _solution = null;
            }
        }

        private void DocumentEventsOnDocumentSaved(Document document)
        {
            ProjectService.DocumentSaved(document);
        }

        #endregion
    }
}