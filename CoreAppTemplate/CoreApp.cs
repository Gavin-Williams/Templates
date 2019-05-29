using Framework;
using System;
using System.Reflection;
using System.Threading.Tasks;
using Windows.ApplicationModel;
using Windows.ApplicationModel.Activation;
using Windows.ApplicationModel.Core;
using Windows.Foundation;
using Windows.Graphics.Display;
using Windows.System.Threading;
using Windows.UI.Core;
using Windows.UI.Input;
using Windows.UI.ViewManagement;

namespace CoreAppTemplate
{
    internal class ApplicationSource : IFrameworkViewSource
    {
        public virtual IFrameworkView CreateView()
        {
            return new CoreApp();
        }
    }

    /// <summary>
    /// Provide core functionality for the Windows Runtime application (UWP)
    /// </summary>
    public class CoreApp : IFrameworkView
    {
        //public delegate void SetWindowEventHandler(object sender, SetWindowEventArgs e);
        public delegate void LoadEventHandler(object sender, EventArgs e);
        public delegate void WindowSizeChangedEventHandler(object sender, WindowSizeChangedEventArgs e);
        public delegate void SuspendingEventHandler(object sender, EventArgs e);

        public event LoadEventHandler OnLoad;
        public event WindowSizeChangedEventHandler OnWindowSizeChanged;
        public event SuspendingEventHandler OnSuspending;
        public CoreWindow CoreWindow { get; private set; }

        // Framework
        private WorkManager workManager;
        private Input input;

        [MTAThread] // Any thread that you spin off from the ASTA must be in an MTA. MTAThread is suitable for DX apps.
        public static int Main()
        {
            Debug.LogMessage("App.Main");
            var appSource = new ApplicationSource();
            CoreApplication.Run(appSource);
            return 0;
        }

        // ==================================================
        // IFrameworkView implementations
        // ==================================================

        public void Initialize(CoreApplicationView appView)
        {
            // Use Initialize to allocate your main classes and connect up the basic event handlers.
            Debug.LogMessage("App.Initialize");

            // allocate objects
            workManager = new WorkManager(this);


            // register event handlers for app lifecycle
            appView.Activated += new TypedEventHandler<CoreApplicationView, IActivatedEventArgs>(Activated);

            CoreApplication.Suspending += new EventHandler<SuspendingEventArgs>(SuspendingAsync);
            CoreApplication.Resuming += new EventHandler<object>(Resuming);
            CoreApplication.Exiting += ExitingEventHandler;
        }

        /// <summary>
        /// Called when the CoreWindow is created or re-created.
        /// </summary>
        /// <param name="coreWindow"></param>
        public void SetWindow(CoreWindow coreWindow)
        {
            // Use SetWindow to create your main window and connect any window specific events.
            Debug.LogMessage("App.SetWindow");
            this.CoreWindow = coreWindow;

            // window visibility and size-change events
            this.CoreWindow.SizeChanged += new TypedEventHandler<CoreWindow, WindowSizeChangedEventArgs>(WindowSizeChanged);
            this.CoreWindow.VisibilityChanged += new TypedEventHandler<CoreWindow, VisibilityChangedEventArgs>(VisibilityChanged);
            this.CoreWindow.Closed += new TypedEventHandler<CoreWindow, CoreWindowEventArgs>(WindowClosed);

            // display events
            DisplayInformation currentDisplayInformation = DisplayInformation.GetForCurrentView();
            currentDisplayInformation.DpiChanged += new TypedEventHandler<DisplayInformation, object>(DpiChanged);
            currentDisplayInformation.OrientationChanged += new TypedEventHandler<DisplayInformation, object>(OrientationChanged);
            DisplayInformation.DisplayContentsInvalidated += new TypedEventHandler<DisplayInformation, object>(DisplayContentsInvalidated);

            // disable all pointer visual feedback for better performance when touching.
            var pointerVisualizationSettings = PointerVisualizationSettings.GetForCurrentView();
            pointerVisualizationSettings.IsContactFeedbackEnabled = false;
            pointerVisualizationSettings.IsBarrelButtonFeedbackEnabled = false;

            // ======================================================
            // Set initial window size & fullscreen 
            // ======================================================
            ApplicationView.PreferredLaunchViewSize = new Size(1600, 900);
            ApplicationView.PreferredLaunchWindowingMode = ApplicationViewWindowingMode.PreferredLaunchViewSize;
            ApplicationView.GetForCurrentView().SetPreferredMinSize(new Size(100, 100));

            // ======================================================
            // create any window dependent classes
            // ======================================================
            workManager.AttachWindow(CoreWindow);
            input = new Input(this, workManager);
        }

        /// <summary>
        /// Construct stage and build early scene resources, or load a previously saved app state.
        /// </summary>
        /// <param name="entryPoint"></param>
        public void Load(String entryPoint)
        {
            // Use Load to handle any remaining setup, and to initiate the async creation of objects and loading 
            // of resources. If you need to create any temporary files or data, such as procedurally generated 
            // assets, do it here too.
            Debug.LogMessage("App.Load");

            OnLoad?.Invoke(this, new EventArgs()); // Does this make sense if SceneManager is constructed in the method.
        }

        public void Run()
        {
            Debug.LogMessage("App.Run");
            workManager.Start();
        }

        /// <summary>
        /// Terminate events don't cause this to be called. It will be called if the IFrameworkView class is torn down while the app is in the foreground.
        /// </summary>
        public void Uninitialize()
        {
            Debug.LogMessage("App.Uninitialize");
        }

        // ==================================================
        // Event handlers for app lifecycle management
        // ==================================================
        private void Activated(CoreApplicationView appView, IActivatedEventArgs args)
        {
            Debug.LogMessage("App.Activated");
            // Run() won't start until the CoreWindow is activated
            CoreWindow.GetForCurrentThread().Activate();
        }

        // ==================================================
        // Window event handlers
        // ==================================================

        private void VisibilityChanged(CoreWindow sender, VisibilityChangedEventArgs args)
        {
            Debug.LogMessage("App.VisibilityChanged");
        }

        private void WindowSizeChanged(CoreWindow sender, WindowSizeChangedEventArgs e)
        {
            Debug.LogMessage("App.WindowSizeChanged");
            double oldW = CoreWindow.Bounds.Width;
            double oldH = CoreWindow.Bounds.Height;

            // pass new window size to any objects that depend upon it here...

            OnWindowSizeChanged?.Invoke(sender, e);
        }

        private void WindowClosed(CoreWindow sender, CoreWindowEventArgs args)
        {
            Debug.LogMessage("App.WindowClosed");
        }

        /// <summary>
        /// This can be called to programmatically exit the application,
        /// As an alternative to the app being closed with the close button.
        /// </summary>
        public void Exit()
        {
            Debug.LogMessage("App.Exit");

            // close any objects here...

            CoreApplication.Exit();
        }

        /// <summary>
        /// This will be called when the application exits.
        /// </summary>
        /// <param name="sender"></param>
        /// <param name="e"></param>
        private void ExitingEventHandler(object sender, object e)
        {
            Debug.LogMessage("App.ExitingEventHandler");
        }

        /// <summary>
        /// note: Suspending event will not fire during debugging. But sometimes it does.
        /// </summary>
        /// <param name="sender"></param>
        /// <param name="args"></param>
        private async void SuspendingAsync(object sender, SuspendingEventArgs args)
        {
            Debug.LogMessage("App.SuspendingAsync");
            // save app state asynchronously after requesting a deferral. Holding a deferral indicates that the application is busy
            // performing suspending operations. Be aware that a deferral may not be held indefinitely. After about five seconds,
            // the app will be forced to exit.
            SuspendingDeferral deferral = args.SuspendingOperation.GetDeferral();

            await Task.Run(() =>
            {
                // stop any running processes...

                OnSuspending?.Invoke(this, null);
            });

            deferral.Complete();
            CoreApplication.Exit();
#if DEBUG
            System.Diagnostics.Debug.WriteLine("App.SuspendingAsync() complete.");
#endif
        }

        private void Resuming(object sender, object args)
        {
            Debug.LogMessage("App.Resuming");
            // https://docs.microsoft.com/en-us/windows/uwp/launch-resume/resume-an-app
            // This is relevant to desktop apps, because it provides a mechanism for handling 
            // change of focus from this app to another and back again.

            // Restore any data or state that was unloaded on suspend. By default, data and 
            // state are persisted when resuming from suspend. Note that this event does not 
            // occur if the app was previously terminated.

            OnLoad?.Invoke(this, new EventArgs());
        }

        // ==================================================
        // Display properties event handlers
        // ==================================================

        private void DpiChanged(DisplayInformation sender, Object args)
        {
            Debug.LogMessage("App.DpiChanged");
        }

        private void OrientationChanged(DisplayInformation sender, Object args)
        {
            Debug.LogMessage("App.OrientationChanged");
        }

        private void DisplayContentsInvalidated(DisplayInformation sender, Object args)
        {
            Debug.LogMessage("App.DisplayContentsInvalidated");
        }

        public CoreWindow GetCoreWindow()
        {
            Debug.LogMessage("App.GetCoreWindow");
            return CoreWindow.GetForCurrentThread();
        }

        public Assembly GetAssembly()
        {
            return typeof(CoreApp).GetTypeInfo().Assembly;
        }

        public void SetTitleBarText(string text)
        {
            Debug.LogMessage("App.SetTitleBarText");
            ApplicationView appView = ApplicationView.GetForCurrentView();
            appView.Title = text;
        }

      
    }
}
