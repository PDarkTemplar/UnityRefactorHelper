using System;
using System.Collections;
using System.Collections.ObjectModel;
using System.Collections.Specialized;
using System.ComponentModel;
using System.Linq;
using System.Runtime.CompilerServices;
using System.Windows.Forms;
using UnityRefactorHelper.Annotations;
using UnityRefactorHelper.Model;
using UnityRefactorHelper.Service;
using UnityRefactorHelper.View;

namespace UnityRefactorHelper.ViewModel
{
    public class ToolWindowViewModel : INotifyPropertyChanged
    {
        public static ToolWindowViewModel Instance;
        private string _errorText;
        private bool _isEnabled;
        private string _selectedProject;
        private string _selectedProjectGuid;
        private Guid _selectedProjectGuidParsed;
        private bool _solutionLoaded;
        private string _unityProjectPath;

        static ToolWindowViewModel()
        {
            Instance = new ToolWindowViewModel();
        }

        private ToolWindowViewModel()
        {
            ProjectNames = new ObservableCollection<string>();
            SyncProjects = new ObservableCollection<ProjectSyncItem>();
            SyncProjects.CollectionChanged += SyncProjectsOnCollectionChanged;

            SyncProjectControls = new ObservableCollection<SyncProjectControl>();
        }

        public ObservableCollection<string> ProjectNames { get; set; }

        public bool SolutionLoaded
        {
            get { return _solutionLoaded; }
            set
            {
                _solutionLoaded = value;
                OnPropertyChanged();
                OnPropertyChanged(nameof(IsSaveEnabled));
            }
        }

        public string UnityProjectPath
        {
            get { return _unityProjectPath; }
            set
            {
                _unityProjectPath = value;
                OnPropertyChanged();
                OnPropertyChanged(nameof(IsSaveEnabled));
            }
        }

        public bool IsEnabled
        {
            get { return _isEnabled; }
            set
            {
                _isEnabled = value;
                OnPropertyChanged();
                OnPropertyChanged(nameof(IsSaveEnabled));
            }
        }

        public ObservableCollection<ProjectSyncItem> SyncProjects { get; set; }

        public string SelectedProject
        {
            get { return _selectedProject; }
            set
            {
                _selectedProject = value;
                OnPropertyChanged();
                OnPropertyChanged(nameof(IsAddEnabled));
            }
        }

        public string SelectedProjectGuid
        {
            get { return _selectedProjectGuid; }
            set
            {
                _selectedProjectGuidParsed = Guid.Empty;
                _selectedProjectGuid = value;
                OnPropertyChanged();
                OnPropertyChanged(nameof(IsAddEnabled));
            }
        }

        public bool IsAddEnabled => !string.IsNullOrWhiteSpace(SelectedProject) &&
                                    Guid.TryParse(SelectedProjectGuid, out _selectedProjectGuidParsed);

        public bool IsSaveEnabled
            => ((!string.IsNullOrWhiteSpace(UnityProjectPath) && SyncProjects.Any()) || !IsEnabled) && SolutionLoaded;

        public string ErrorText
        {
            get { return _errorText; }
            set
            {
                _errorText = value;
                OnPropertyChanged();
                OnPropertyChanged(nameof(ShowError));
            }
        }

        public bool ShowError => !string.IsNullOrWhiteSpace(ErrorText);
        public ObservableCollection<SyncProjectControl> SyncProjectControls { get; set; }

        public void OpenUnityFolderDialog()
        {
            var dialog = new FolderBrowserDialog();
            if (dialog.ShowDialog() == DialogResult.OK)
            {
                UnityProjectPath = dialog.SelectedPath;
            }
        }

        public void AddNewProjectSync()
        {
            if (_selectedProjectGuidParsed == Guid.Empty) return;
            if (SyncProjects.All(x => x.ProjectGuid != _selectedProjectGuidParsed))
            {
                var existed = SyncProjects.FirstOrDefault(x => x.ProjectName == SelectedProject);
                if (existed != null)
                {
                    existed.ProjectGuid = _selectedProjectGuidParsed;
                    var control = SyncProjectControls.FirstOrDefault(x => x.Item.ProjectName == existed.ProjectName);
                    if (control != null)
                        control.Item = existed;
                }
                else
                {
                    SyncProjects.Add(new ProjectSyncItem
                    {
                        ProjectGuid = _selectedProjectGuidParsed,
                        ProjectName = SelectedProject
                    });
                }
            }

            SelectedProject = null;
            SelectedProjectGuid = null;
            OnPropertyChanged(nameof(IsSaveEnabled));
        }

        public void Save()
        {
            var settings = new Settings();
            if (IsEnabled)
            {
                settings.IsEnabled = true;
                settings.UnityProjectPath = UnityProjectPath;
                settings.ProjectSyncItems = SyncProjects;
            }

            var oldSettings = Cache.Instance.Settings;
            try
            {
                Cache.Instance.Settings = settings;
                CommonService.SaveSettings();
                ErrorText = null;
            }
            catch (Exception)
            {
                Cache.Instance.Settings = oldSettings;
                ErrorText = "Can't save settings.";
            }

            SynchronizeProjects();
            CommonService.SaveSyncProjectItemsToCache();
        }

        private void SynchronizeProjects()
        {
            var existedItemsWithDifferentGuid =
                Cache.Instance.ScanProjects.Where(
                    x => SyncProjects.Any(sp => sp.ProjectName == x.ProjectName && sp.ProjectGuid != x.ProjectGuid));
            var newItems =
                SyncProjects.Where(x => Cache.Instance.ScanProjects.All(sp => sp.ProjectName != x.ProjectName));
            var deleteItems =
                Cache.Instance.ScanProjects.Where(x => SyncProjects.All(sp => sp.ProjectName != x.ProjectName)).ToList();

            foreach (var deleteItem in deleteItems)
            {
                Cache.Instance.ScanProjects.Remove(deleteItem);
            }

            foreach (var existedItem in existedItemsWithDifferentGuid)
            {
                var syncItem = SyncProjects.FirstOrDefault(x => x.ProjectName == existedItem.ProjectName);
                if (syncItem != null)
                {
                    existedItem.ProjectGuid = syncItem.ProjectGuid;
                }
            }

            foreach (var newItem in newItems)
            {
                Cache.Instance.ScanProjects.Add(new ProjectScanItem
                {
                    ProjectName = newItem.ProjectName,
                    ProjectGuid = newItem.ProjectGuid
                });
                ProjectService.ProcessNewProject(newItem);
            }
        }

        private void SyncProjectsOnCollectionChanged(object sender,
            NotifyCollectionChangedEventArgs notifyCollectionChangedEventArgs)
        {
            if (notifyCollectionChangedEventArgs.Action == NotifyCollectionChangedAction.Add)
            {
                var items = notifyCollectionChangedEventArgs.NewItems as IEnumerable;
                foreach (ProjectSyncItem item in items)
                {
                    var control = new SyncProjectControl(item);
                    control.OnSyncItemDelete += SyncProjectOnItemDelete;
                    control.OnSyncItemClick += SyncProjectOnSyncItemClick;
                    SyncProjectControls.Add(control);
                }
            }
        }

        private void SyncProjectOnSyncItemClick(object sender, EventArgs eventArgs)
        {
            var item = sender as ProjectSyncItem;
            if (item != null)
            {
                SelectedProject = item.ProjectName;
                SelectedProjectGuid = item.ProjectGuid.ToString();
            }
        }

        private void SyncProjectOnItemDelete(object sender, EventArgs eventArgs)
        {
            var item = sender as SyncProjectControl;
            if (item != null)
            {
                SyncProjectControls.Remove(item);
                SyncProjects.Remove(item.Item);
                OnPropertyChanged(nameof(IsSaveEnabled));
            }
        }

        public void LoadSettings()
        {
            try
            {
                CommonService.LoadSettings();
            }
            catch
            {
                return;
            }

            var settings = Cache.Instance.Settings;
            if (settings == null) return;
            MapSettings(settings);
            try
            {
                CommonService.LoadSyncProjectsCache();
            }
            catch (Exception ex)
            {
            }
        }

        private void MapSettings(Settings settings)
        {
            IsEnabled = settings.IsEnabled;
            UnityProjectPath = settings.UnityProjectPath;
            foreach (var item in settings.ProjectSyncItems)
            {
                SyncProjects.Add(item);
            }
            Cache.Instance.ScanProjects.AddRange(SyncProjects.Select(x => new ProjectScanItem
            {
                ProjectGuid = x.ProjectGuid,
                ProjectName = x.ProjectName
            }));

            OnPropertyChanged(nameof(IsSaveEnabled));
        }

        #region INotifyPropertyChanged

        public event PropertyChangedEventHandler PropertyChanged;

        [NotifyPropertyChangedInvocator]
        protected virtual void OnPropertyChanged([CallerMemberName] string propertyName = null)
        {
            PropertyChanged?.Invoke(this, new PropertyChangedEventArgs(propertyName));
        }

        #endregion
    }
}