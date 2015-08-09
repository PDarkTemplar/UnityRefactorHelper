using System;
using System.ComponentModel;
using System.Runtime.CompilerServices;
using System.Windows;
using System.Windows.Controls;
using UnityRefactorHelper.Annotations;
using UnityRefactorHelper.Model;

namespace UnityRefactorHelper.View
{
    /// <summary>
    /// Interaction logic for SyncProjectControl.xaml
    /// </summary>
    public partial class SyncProjectControl : UserControl, INotifyPropertyChanged
    {
        private ProjectSyncItem _item;

        public SyncProjectControl(ProjectSyncItem item)
        {
            Item = item;
            InitializeComponent();
        }

        public ProjectSyncItem Item
        {
            get { return _item; }
            set
            {
                _item = value;
                OnPropertyChanged(nameof(LabelText));
            }
        }

        public string LabelText => $"{Item.ProjectName} : {Item.ProjectGuid}";
        public event PropertyChangedEventHandler PropertyChanged;
        public event EventHandler OnSyncItemDelete;
        public event EventHandler OnSyncItemClick;

        private void removeBtn_Click(object sender, RoutedEventArgs e)
        {
            var messageBoxResult = MessageBox.Show("Are you sure?", "Delete Confirmation", MessageBoxButton.YesNo);
            if (messageBoxResult != MessageBoxResult.Yes) return;
            OnSyncItemDelete?.Invoke(this, EventArgs.Empty);
        }

        private void syncBtn_Click(object sender, RoutedEventArgs e)
        {
            OnSyncItemClick?.Invoke(Item, EventArgs.Empty);
        }

        [NotifyPropertyChangedInvocator]
        protected virtual void OnPropertyChanged([CallerMemberName] string propertyName = null)
        {
            PropertyChanged?.Invoke(this, new PropertyChangedEventArgs(propertyName));
        }
    }
}