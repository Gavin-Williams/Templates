using CoreAppTemplate;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Windows.UI.Core;

namespace Framework
{
    public class Input
    {
        private WorkManager workManager;

        public Input(CoreApp coreApp, WorkManager workManager)
        {
            Debug.LogMessage("Input.Ctor");
            this.workManager = workManager;
            coreApp.CoreWindow.KeyDown += CoreWindow_KeyDown;
        }

        private void CoreWindow_KeyDown(CoreWindow sender, KeyEventArgs args)
        {
            if (args.VirtualKey == Windows.System.VirtualKey.Escape)
                workManager.Exit();
        }
    }
}
