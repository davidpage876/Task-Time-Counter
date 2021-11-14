using System;
using System.Diagnostics;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Runtime.InteropServices.WindowsRuntime;
using Windows.ApplicationModel;
using Windows.ApplicationModel.Activation;
using Windows.Foundation;
using Windows.Foundation.Collections;
using Windows.UI.Xaml;
using Windows.UI.Xaml.Controls;
using Windows.UI.Xaml.Controls.Primitives;
using Windows.UI.Xaml.Data;
using Windows.UI.Xaml.Input;
using Windows.UI.Xaml.Media;
using Windows.UI.Xaml.Navigation;
using Windows.UI.ViewManagement;
using Windows.Storage;

namespace Task_Time_Counter_2
{
    /// <summary>
    /// Provides application-specific behavior to supplement the default Application class.
    /// </summary>
    sealed partial class App : Application
    {
        private ApplicationDataContainer localSettings;
        private DispatcherTimer dispatchTimer;
        private DispatcherTimer autosaveTimer;
        private TextBlock totalTime;

        StackPanel taskList;

        /// <summary>
        /// Formats the given time in long format: "(numeric hours) HH:MM:SS.S".
        /// </summary>
        public static string FormatTimeLong(TimeSpan elapsed)
        {
            return string.Format("({0}) {1:00}:{2:00}:{3:00}.{4:0}",
                Math.Round(elapsed.TotalHours, 1),
                elapsed.Hours, elapsed.Minutes, elapsed.Seconds, elapsed.Milliseconds / 100);
        }

        /// <summary>
        /// Formats the given time in short format: "HH:MM:SS.S".
        /// </summary>
        public static string FormatTimeShort(TimeSpan elapsed)
        {
            return string.Format("{0:00}:{1:00}:{2:00}.{3:0}",
                elapsed.Hours, elapsed.Minutes, elapsed.Seconds, elapsed.Milliseconds / 100);
        }

        /// <summary>
        /// Initializes the singleton application object.  This is the first line of authored code
        /// executed, and as such is the logical equivalent of main() or WinMain().
        /// </summary>
        public App()
        {
            this.InitializeComponent();
            this.Suspending += OnSuspending;

            // Set up preferred window size.
            ApplicationView.PreferredLaunchViewSize = new Size(320, 670);
            ApplicationView.PreferredLaunchWindowingMode = ApplicationViewWindowingMode.PreferredLaunchViewSize;

            // Get local app data store.
            localSettings = ApplicationData.Current.LocalSettings;
        }

        /// <summary>
        /// Initializes the main UI state.
        /// </summary>
        /// <param name="pg">The main page of the app.</param>
        public void InitializeUI(MainPage mainUI)
        {
            // Set up preferred minimum window size.
            var appView = ApplicationView.GetForCurrentView();
            appView.SetPreferredMinSize(new Size(290, 200));

            // Set window title bar color.
            var titleBar = appView.TitleBar;
            var windowBarColor = (Windows.UI.Color)Resources["windowBarColor"];
            titleBar.BackgroundColor = windowBarColor;
            titleBar.ButtonBackgroundColor = windowBarColor;
            titleBar.InactiveBackgroundColor = windowBarColor;
            titleBar.ButtonInactiveBackgroundColor = windowBarColor;

            // Get control references.
            totalTime = mainUI.FindName("TotalTime") as TextBlock;
            taskList = mainUI.FindName("TaskList") as StackPanel;
            UIElementCollection tasks = taskList.Children;

            // Set each task's background fill.
            ResetTaskColors();

            // Set first (top) task as active.
            Task t01 = tasks.ElementAt(0) as Task;
            t01.HasContent = true;
            t01.Active = true;

            // Set up dispatch timer for updating UI.
            dispatchTimer = new DispatcherTimer();
            dispatchTimer.Tick += OnTimerTick;
            dispatchTimer.Interval = new TimeSpan(0, 0, 0, 0, 100);
            dispatchTimer.Start();

            // Attempt to load state from a previous session.
            LoadState();

            // Set up dispatch timer for periodic auto-saving to file.
            autosaveTimer = new DispatcherTimer();
            autosaveTimer.Tick += OnAutosaveTick;
            autosaveTimer.Interval = new TimeSpan(0, 0, 30); // Update every 30 seconds.
            autosaveTimer.Start();
        }

        /// <summary>
        /// Sets a task to be the active (top-most) task.
        /// Get returns a reference to the currently active (top-most) task.
        /// </summary>
        public Task ActiveTask
        {
            set
            {
                Task task = value;

                // Make the current top-most task inactive.
                Task currentTask = ActiveTask;
                bool wasRecording = currentTask.IsRecording;
                currentTask.Active = false;

                // Reorder this task to be at the top.
                taskList.Children.Remove(task);
                taskList.Children.Insert(0, task);

                // Start recording time if we were previously recording.
                task.IsRecording = wasRecording;
                task.Active = true;
            }
            get
            {
                return taskList.Children.First() as Task;
            }
        }

        /// <summary>
        /// Resets all task times to zero and stops the active timer.
        /// </summary>
        public void ClearTaskTimes()
        {
            foreach (Task task in taskList.Children)
            {
                task.ResetTime();
            }
        }

        /// <summary>
        /// Resets all task names to blank.
        /// </summary>
        public void ClearTaskNames()
        {
            foreach (Task task in taskList.Children)
            {
                task.ResetName();
            }
        }

        /// <summary>
        /// Resets task colors to their starting defaults.
        /// </summary>
        public void ResetTaskColors()
        {
            foreach (Task task in taskList.Children)
            {
                task.AssignFillStyle(
                    Resources["taskBrush"] as Brush, 
                    Resources["taskFocusBrush"] as Brush);
            }
        }

        private string MakeTaskName(int i)
        {
            return string.Format("Task{0}", i);
        } 

        /// <summary>
        /// Saves app state for recall between sessions.
        /// </summary>
        public void SaveState()
        {
            // Save task list data.
            int i = 0;
            foreach (Task task in taskList.Children)
            {
                // Save task data.
                var taskData = new ApplicationDataCompositeValue();
                taskData["TaskName"] = task.TaskName;
                taskData["TaskTime"] = task.Time;

                localSettings.Values[MakeTaskName(i)] = taskData;
                i++;
            }
        }

        /// <summary>
        /// Restores previously saved app state.
        /// 
        /// If no app state was saved, has no effect.
        /// </summary>
        public void LoadState()
        {
            // Load task list data.
            int i = 0;
            foreach (Task task in taskList.Children)
            {
                var taskData = (ApplicationDataCompositeValue)
                    localSettings.Values[MakeTaskName(i)]; 

                if (taskData == null)
                {
                    // Data not found.
                    break;
                }
                else
                {
                    // Load task data.
                    task.TaskName = (string)taskData["TaskName"];
                    task.Time = (TimeSpan)taskData["TaskTime"];
                }
                i++;
            }
        }

        private void AssignTaskFill(int index, string styleName)
        {
            var task = taskList.Children.ElementAt(index) as Task;
            task.AssignFillStyle(
                Resources[styleName] as Brush, 
                Resources[styleName + "Focus"] as Brush);
        }

        private void OnTimerTick(object sender, object e)
        {
            // Update total time.
            TimeSpan total = TimeSpan.Zero;
            foreach (Task task in taskList.Children)
            {
                total += task.Time;
            }
            totalTime.Text = FormatTimeLong(total);
        }

        private void OnAutosaveTick(object sender, object e)
        {
            SaveState();
        }

        /// <summary>
        /// Invoked when the application is launched normally by the end user.  Other entry points
        /// will be used such as when the application is launched to open a specific file.
        /// </summary>
        /// <param name="e">Details about the launch request and process.</param>
        protected override void OnLaunched(LaunchActivatedEventArgs e)
        {
            Frame rootFrame = Window.Current.Content as Frame;

            // Do not repeat app initialization when the Window already has content,
            // just ensure that the window is active
            if (rootFrame == null)
            {
                // Create a Frame to act as the navigation context and navigate to the first page
                rootFrame = new Frame();

                rootFrame.NavigationFailed += OnNavigationFailed;

                if (e.PreviousExecutionState == ApplicationExecutionState.Terminated)
                {
                    LoadState();
                }

                // Place the frame in the current Window
                Window.Current.Content = rootFrame;
            }

            if (e.PrelaunchActivated == false)
            {
                if (rootFrame.Content == null)
                {
                    // When the navigation stack isn't restored navigate to the first page,
                    // configuring the new page by passing required information as a navigation
                    // parameter
                    rootFrame.Navigate(typeof(MainPage), e.Arguments);
                }
                // Ensure the current window is active
                Window.Current.Activate();
            }
        }

        /// <summary>
        /// Invoked when Navigation to a certain page fails
        /// </summary>
        /// <param name="sender">The Frame which failed navigation</param>
        /// <param name="e">Details about the navigation failure</param>
        void OnNavigationFailed(object sender, NavigationFailedEventArgs e)
        {
            throw new Exception("Failed to load Page " + e.SourcePageType.FullName);
        }

        /// <summary>
        /// Invoked when application execution is being suspended.  Application state is saved
        /// without knowing whether the application will be terminated or resumed with the contents
        /// of memory still intact.
        /// </summary>
        /// <param name="sender">The source of the suspend request.</param>
        /// <param name="e">Details about the suspend request.</param>
        private void OnSuspending(object sender, SuspendingEventArgs e)
        {
            var deferral = e.SuspendingOperation.GetDeferral();

            // Save before exiting.
            SaveState();

            deferral.Complete();
        }
    }
}
