﻿using System;
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

namespace Task_Time_Counter_2
{
    /// <summary>
    /// Provides application-specific behavior to supplement the default Application class.
    /// </summary>
    sealed partial class App : Application
    {
        private DispatcherTimer dispatchTimer;
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
        }

        /// <summary>
        /// Initializes the main UI state.
        /// </summary>
        /// <param name="pg">The main page of the app.</param>
        public void InitializeUI(MainPage mainUI)
        {
            // Get control references.
            totalTime = mainUI.FindName("TotalTime") as TextBlock;
            taskList = mainUI.FindName("TaskList") as StackPanel;
            UIElementCollection tasks = taskList.Children;

            // Set each task's background fill.
            AssignTaskFill(0, "taskRed");
            AssignTaskFill(1, "taskGreen");
            AssignTaskFill(2, "taskBlue");
            AssignTaskFill(3, "taskYellow");
            AssignTaskFill(4, "taskPurple");
            AssignTaskFill(5, "taskCyan");
            AssignTaskFill(6, "taskOrange");
            AssignTaskFill(7, "taskPink");
            AssignTaskFill(8, "taskLightGrey");
            AssignTaskFill(9, "taskDarkGrey");

            // Set first (top) task as active.
            Task t01 = tasks.ElementAt(0) as Task;
            t01.Active = true;

            // Set up dispatch timer for updating UI.
            dispatchTimer = new DispatcherTimer();
            dispatchTimer.Tick += OnTimerTick;
            dispatchTimer.Interval = new TimeSpan(0, 0, 0, 0, 100);
            dispatchTimer.Start();
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
                    //TODO: Load state from previously suspended application
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
            //TODO: Save application state and stop any background activity
            deferral.Complete();
        }
    }
}
