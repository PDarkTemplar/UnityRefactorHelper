using System.Runtime.InteropServices;
using Microsoft.VisualStudio.Shell;
using UnityRefactorHelper.View;

namespace UnityRefactorHelper
{
    [Guid("5825C503-1846-4B31-95ED-716E24E5E9E4")]
    public sealed class SettingsToolWindow : ToolWindowPane
    {
        public SettingsToolWindow() :
            base(null)
        {
            Caption = "Unity Refactor Helper";

            Content = new UnityRefactorHelperSettingsControl();
        }
    }
}