using System.Collections.Generic;

namespace UnityRefactorHelper.Model
{
    public class ProjectScanItem : ProjectSyncItem
    {
        public ProjectScanItem()
        {
            Files = new List<FileScanItem>();
        }

        public List<FileScanItem> Files { get; }
    }
}