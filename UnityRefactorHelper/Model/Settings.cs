using System.Collections.ObjectModel;

namespace UnityRefactorHelper.Model
{
    public class Settings
    {
        public Settings()
        {
            ProjectSyncItems = new ObservableCollection<ProjectSyncItem>();
        }

        public bool IsEnabled { get; set; }
        public string UnityProjectPath { get; set; }
        public ObservableCollection<ProjectSyncItem> ProjectSyncItems { get; set; }
    }
}