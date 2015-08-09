using System.Windows;
using System.Windows.Controls;
using UnityRefactorHelper.ViewModel;

namespace UnityRefactorHelper.View
{
    /// <summary>
    /// Interaction logic for UnityRefactorHelperSettingsControl.
    /// </summary>
    public partial class UnityRefactorHelperSettingsControl : UserControl
    {
        /// <summary>
        /// Initializes a new instance of the <see cref="UnityRefactorHelperSettingsControl"/> class.
        /// </summary>
        public UnityRefactorHelperSettingsControl()
        {
            InitializeComponent();
        }

        public ToolWindowViewModel ViewModel => ToolWindowViewModel.Instance;

        private void selectUnityProjectFolderBtn_Click(object sender, RoutedEventArgs e)
        {
            ViewModel.OpenUnityFolderDialog();
        }

        private void addBtn_Click(object sender, RoutedEventArgs e)
        {
            ViewModel.AddNewProjectSync();
        }

        private void saveBtn_Click(object sender, RoutedEventArgs e)
        {
            ViewModel.Save();
        }
    }
}