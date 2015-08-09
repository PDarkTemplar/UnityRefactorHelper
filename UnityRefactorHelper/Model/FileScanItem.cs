namespace UnityRefactorHelper.Model
{
    public class FileScanItem
    {
        public string ProjectFileId { get; set; }
        public int OldId { get; set; }
        public int? NewId { get; set; }
    }
}