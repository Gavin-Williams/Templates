using CoreAppTemplate;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Windows.Foundation;
using Windows.System.Threading;
using Windows.UI.Core;

namespace Framework
{
    public class WorkManager
    {
        public WorkManagerStates State { get; private set; }
        private CoreApp coreApp;
        private CoreWindow window;

        public WorkManager(CoreApp coreApp)
        {
            Debug.LogMessage("WorkManager.Ctor");
            this.coreApp = coreApp;
        }

        public void Start()
        {
            Debug.LogMessage("WorkManager.Start");

            // make sure events are processed before entering main loop, because the window may not be visible yet.
            CoreWindow.GetForCurrentThread().Dispatcher.ProcessEvents(CoreProcessEventsOption.ProcessAllIfPresent);

            IAsyncAction mainLoopWorker;
            WorkItemHandler workHandler = new WorkItemHandler((IAsyncAction action) =>
            {
                Debug.LogMessage("WorkManager *** thread started *** ");
                while ((action.Status == AsyncStatus.Started) && State != WorkManagerStates.Exiting)
                {
                    if (State == WorkManagerStates.Running)
                    {

                    
                    }
                }

                // signal application quit or else ProcessUntilQuit will wait for windows close button press
                coreApp.Exit();
                Debug.LogMessage("WorkManager *** thread terminated ***");
            });
            mainLoopWorker = ThreadPool.RunAsync(workHandler, WorkItemPriority.High, WorkItemOptions.TimeSliced);

            // ProcessUntilQuit will block the UI thread and process events as they appear until the App terminates.
            CoreWindow.GetForCurrentThread().Dispatcher.ProcessEvents(CoreProcessEventsOption.ProcessUntilQuit);
        }

        public void AttachWindow(CoreWindow window)
        {
            Debug.LogMessage("WorkManager.AttachWindow");
            this.window = window;
        }

        public void Exit()
        {
            Debug.LogMessage("WorkManager.Exit");
            State = WorkManagerStates.Exiting;
        }
    }
}
