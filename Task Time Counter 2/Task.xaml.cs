using System;
using System.Diagnostics;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Runtime.InteropServices.WindowsRuntime;
using Windows.Foundation;
using Windows.Foundation.Collections;
using Windows.UI.Xaml;
using Windows.UI.Xaml.Controls;
using Windows.UI.Xaml.Controls.Primitives;
using Windows.UI.Xaml.Data;
using Windows.UI.Xaml.Input;
using Windows.UI.Xaml.Media;
using Windows.UI.Xaml.Navigation;
using Windows.UI.Xaml.Shapes;

// The User Control item template is documented at https://go.microsoft.com/fwlink/?LinkId=234236

namespace Task_Time_Counter_2
{
    public sealed partial class Task : UserControl
    {
        private const string PLACEHOLDER_TASK_NAME = "Unnamed";
        private DispatcherTimer dispatchTimer;
        private Stopwatch stopwatch;
        private TimeSpan timeOffset = TimeSpan.Zero;
        private bool hasContent = false;
        private bool showAddContent = false;
        private bool isActive = false;
        private bool isRecording = false;
        private bool isEditingName = false;
        private bool isEditingTime = false;
        private FocusState nameFocusState = FocusState.Pointer;
        private FocusState timeFocusState = FocusState.Pointer;
        private string taskName = "";
        private Brush normalFill;
        private Brush focusFill;

        // References.
        private App app;
        private Border panel;
        private Grid blank;
        private Grid taskContent;
        private Button toTopBtn;
        private Button playPauseBtn;
        private Button timeBtn;
        private TextBox timeEdit;
        private Button nameBtn;
        private TextBox nameEdit;
        private Flyout moreMenu;

        public Task()
        {
            this.InitializeComponent();

            // Get app reference.
            app = Application.Current as App;

            // Get control references.
            panel = FindName("Panel") as Border;
            blank = FindName("Blank") as Grid;
            taskContent = FindName("TaskContent") as Grid;
            toTopBtn = FindName("ToTopBtn") as Button;
            playPauseBtn = FindName("PlayPauseBtn") as Button;
            timeBtn = FindName("TimeBtn") as Button;
            timeEdit = FindName("TimeEdit") as TextBox;
            nameBtn = FindName("NameBtn") as Button;
            nameEdit = FindName("NameEdit") as TextBox;
            moreMenu = FindName("MoreMenu") as Flyout;

            // Set up stopwatch for recording time.
            stopwatch = new Stopwatch();

            // Set up dispatch timer for updating UI.
            dispatchTimer = new DispatcherTimer();
            dispatchTimer.Tick += OnTimerTick;
            dispatchTimer.Interval = new TimeSpan(0, 0, 0, 0, 100); // Update every 100 milliseconds.

            // Update UI based on default state.
            UpdateUI();
        }

        /// <summary>
        /// Assigns fill style to use for this task.
        /// 
        /// Focus is used when the task is active, normal otherwise.
        /// </summary>
        public void AssignFillStyle(Brush normal, Brush focus)
        {
            normalFill = normal;
            focusFill = focus;
            UpdateUI();
        }

        /// <summary>
        /// Does the task have content, or is it blank.
        /// </summary>
        public bool HasContent
        {
            set
            {
                hasContent = value;
                UpdateUI();
            }
            get { return hasContent; }
        }

        /// <summary>
        /// Should we display the "Add task" button when the task is blank.
        /// </summary>
        public bool ShowAddContent
        {
            set
            {
                showAddContent = value;
                UpdateBlankUI();
            }
            get { return showAddContent; }
        }

        /// <summary>
        /// Is this task the active timer.
        /// 
        /// If the task is not active we stop recording to the timer.
        /// 
        /// Note: Use App.ActiveTask to change the active timer intelligently
        /// bringing this task to the top and disabling the previously active task.
        /// </summary>
        public bool Active
        {
            set
            {
                isActive = value;

                // Stop recording if not active.
                if (!isActive)
                {
                    IsRecording = false;
                }
                else
                {
                    UpdateUI();
                }
            }
            get { return isActive; }
        }

        /// <summary>
        /// Are we recording to the timer.
        /// </summary>
        public bool IsRecording
        {
            set 
            { 
                isRecording = value;
                if (isRecording)
                {
                    stopwatch.Start();
                    dispatchTimer.Start();
                }
                else
                {
                    stopwatch.Stop();
                    dispatchTimer.Stop();
                }
                UpdateUI();
            }
            get { return isRecording; }
        }

        /// <summary>
        /// Sets the recorded time on the timer.
        /// Get returns the current recorded time on the timer.
        /// </summary>
        public TimeSpan Time
        {
            set
            {
                if (IsRecording)
                {
                    stopwatch.Restart();
                } 
                else
                {
                    stopwatch.Reset();
                }
                timeOffset = value;
                UpdateTimerUI();
            }
            get
            {
                return timeOffset + stopwatch.Elapsed;
            }
        }

        /// <summary>
        /// Returns the name of the task.
        /// </summary>
        public string TaskName
        {
            set
            {
                taskName = value;
                UpdateNameUI();
            }
            get { return taskName; }
        }

        /// <summary>
        /// Resets the recorded task time to zero.
        /// </summary>
        public void ResetTime()
        {
            timeOffset = TimeSpan.Zero;
            stopwatch.Reset();
            IsRecording = false;
        }

        /// <summary>
        /// Resets the task name to blank.
        /// </summary>
        public void ResetName()
        {
            taskName = "";
            UpdateNameUI();
        }

        /// <summary>
        /// Update all task controls based on current state.
        /// </summary>
        public void UpdateUI()
        {
            // Update panel background.
            panel.Background = isActive ? focusFill : normalFill;

            // Update content state.
            if (hasContent)
            {
                // Show content.
                taskContent.Visibility = Visibility.Visible;
                UpdateBlankUI();

                // Update button state.
                const string PLAY_ICON = "";
                const string PAUSE_ICON = "";
                if (isActive)
                {
                    toTopBtn.Visibility = Visibility.Collapsed;
                    playPauseBtn.Visibility = Visibility.Visible;
                    if (isRecording)
                    {
                        playPauseBtn.Content = PAUSE_ICON;
                    }
                    else
                    {
                        playPauseBtn.Content = PLAY_ICON;
                    }
                }
                else
                {
                    toTopBtn.Visibility = Visibility.Visible;
                    playPauseBtn.Visibility = Visibility.Collapsed;
                }

                // Update displayed task name.
                UpdateNameUI();

                // Update displayed timer value.
                UpdateTimerUI();
            }
            else
            {
                // Show blank state.
                taskContent.Visibility = Visibility.Collapsed;
                UpdateBlankUI();
            }

            // Update "Add task" button in task list.
            app.UpdateTaskListAddButton();
        }

        /// <summary>
        /// Update timer time display based on current state.
        /// </summary>
        public void UpdateTimerUI()
        {
            timeBtn.Content = App.FormatTime(Time, app.ShowDecimalTimes);
        }

        /// <summary>
        /// Update task name display based on current state.
        /// </summary>
        public void UpdateNameUI()
        {
            if (taskName == "")
            {
                nameBtn.Content = PLACEHOLDER_TASK_NAME;
            }
            else
            {
                nameBtn.Content = taskName;
            }
        }

        /// <summary>
        /// Update display of task when it has no content based on current state.
        /// </summary>
        public void UpdateBlankUI()
        {
            if (!hasContent && showAddContent)
            {
                blank.Visibility = Visibility.Visible;
            }
            else
            {
                blank.Visibility = Visibility.Collapsed;
            }
        }

        /// <summary>
        /// Opens the name text editor, allowing the user to change the task name.
        /// </summary>
        public void StartEditingName()
        {
            isEditingName = true;

            // Get focus state on open.
            nameFocusState = nameBtn.FocusState;
            if (nameFocusState == FocusState.Unfocused)
            {
                nameFocusState = FocusState.Programmatic;
            }

            // Set the edit field text to match the current text.
            nameEdit.Text = taskName;

            // Open the edit field and focus on it.
            nameBtn.Visibility = Visibility.Collapsed;
            nameEdit.Visibility = Visibility.Visible;
            nameEdit.Focus(FocusState.Programmatic);
            nameEdit.SelectAll();
        }

        /// <summary>
        /// Closes the name text editor.
        /// </summary>
        /// <param name="apply">
        /// If true, changes made in the text editor are saved. 
        /// If false, changes are discarded and the original name is unchanged.
        /// </param>
        public void StopEditingName(bool apply)
        {
            isEditingName = false;

            // Apply change.
            if (apply)
            {
                taskName = nameEdit.Text;
                UpdateNameUI();
            }

            // Close the edit field.
            nameBtn.Visibility = Visibility.Visible;
            nameEdit.Visibility = Visibility.Collapsed;

            // Focus on the name button using the original focus state we entered with.
            nameBtn.Focus(nameFocusState);
        }


        /// <summary>
        /// Opens the time text editor, allowing the user to change the task time.
        /// </summary>
        public void StartEditingTime()
        {
            isEditingTime = true;

            // Get focus state on open.
            timeFocusState = timeBtn.FocusState;
            if (timeFocusState == FocusState.Unfocused)
            {
                timeFocusState = FocusState.Programmatic;
            }

            // Set the edit field time to match the current text.
            timeEdit.Text = App.FormatTime(Time);

            // Open the edit field and focus on it.
            timeBtn.Visibility = Visibility.Collapsed;
            timeEdit.Visibility = Visibility.Visible;
            timeEdit.Focus(FocusState.Programmatic);
            timeEdit.SelectAll();
        }

        /// <summary>
        /// Closes the time text editor.
        /// </summary>
        /// <param name="apply">
        /// If true, changes made in the text editor are saved, if they are valid.
        /// If false, or if the input given is invalid
        /// changes are discarded and the original time is unchanged.
        /// </param>
        public void StopEditingTime(bool apply)
        {
            isEditingTime = false;

            // Attempt to apply change.
            if (apply) 
            {
                try
                {
                    // Restart the timer with the given value as the starting time offset.
                    Time = TimeSpan.Parse(timeEdit.Text);
                }
                catch (Exception e)
                {
                    // Do not apply change if the input was invalid.
                    Debug.WriteLine(e.Message);
                }
            }

            // Close the edit field.
            timeBtn.Visibility = Visibility.Visible;
            timeEdit.Visibility = Visibility.Collapsed;

            // Focus on the time button using the original focus state we entered with.
            timeBtn.Focus(timeFocusState);
        }

        private void OnTimerTick(object sender, object e)
        {
            UpdateTimerUI();
        }

        private void onPlayPauseTapped(object sender, TappedRoutedEventArgs e)
        {
            IsRecording = !IsRecording;
        }

        private void onPlayPauseKeyDown(object sender, KeyRoutedEventArgs e)
        {
            if (App.IsAcceptKey(e))
            {
                IsRecording = !IsRecording;
            }
        }

        private void OnNameTapped(object sender, TappedRoutedEventArgs e)
        {
            // Prevent propogation of event to name input field.
            e.Handled = true;

            // Enable editing of task name.
            StartEditingName();
        }

        private void OnNameBtnKeyDown(object sender, KeyRoutedEventArgs e)
        {
            if (App.IsAcceptKey(e))
            {
                // Prevent propogation of event to name input field.
                e.Handled = true;

                // Enable editing of task name.
                StartEditingName();
            }
        }

        private void OnNameFocusLost(object sender, RoutedEventArgs e)
        {
            // Apply editing of task name on focus lost.
            if (isEditingName)
            {
                StopEditingName(true);
            }
        }

        private void OnNameKeyDown(object sender, KeyRoutedEventArgs e)
        {
            switch (e.Key)
            {
                // Cancel task name edit on Escape.
                case Windows.System.VirtualKey.Escape:
                    StopEditingName(false);
                    break;

                // Apply task name edit on Enter.
                case Windows.System.VirtualKey.Enter:
                    StopEditingName(true);
                    break;
            }
        }

        private void OnNameContextMenuOpening(object sender, ContextMenuEventArgs e)
        {
            // Prevent default context menu from opening.
            e.Handled = true;

            // Apply editing of task name on context menu open (right click).
            StopEditingName(true);
        }

        private void OnTimeTapped(object sender, TappedRoutedEventArgs e)
        {
            // Prevent propogation of event to time input field.
            e.Handled = true;

            // Enable editing of task time.
            StartEditingTime();
        }

        private void OnTimeBtnKeyDown(object sender, KeyRoutedEventArgs e)
        {
            if (App.IsAcceptKey(e))
            {
                // Prevent propogation of event to time input field.
                e.Handled = true;

                // Enable editing of task time.
                StartEditingTime();
            }
        }

        private void OnTimeLostFocus(object sender, RoutedEventArgs e)
        {
            // Apply editing of task time on focus lost.
            if (isEditingTime)
            {
                StopEditingTime(true);
            }
        }

        private void OnTimeKeyDown(object sender, KeyRoutedEventArgs e)
        {
            switch (e.Key)
            {
                // Cancel task time edit on Escape.
                case Windows.System.VirtualKey.Escape:
                    StopEditingTime(false);
                    break;

                // Apply task time edit on Enter.
                case Windows.System.VirtualKey.Enter:
                    StopEditingTime(true);
                    break;
            }
        }

        private void OnTimeContextMenuOpening(object sender, ContextMenuEventArgs e)
        {
            // Prevent default context menu from opening.
            e.Handled = true;

            // Apply editing of task time on context menu open (right click).
            StopEditingTime(true);
        }

        private void ToTopBtnPressed(object sender, TappedRoutedEventArgs e)
        {
            app.ActiveTask = this;
        }
        private void ToTopBtnKeyDown(object sender, KeyRoutedEventArgs e)
        {
            if (App.IsAcceptKey(e))
            {
                app.ActiveTask = this;
            }
        }

        private void AddBtnPressed(object sender, TappedRoutedEventArgs e)
        {
            HasContent = true;
            
            // Attempt to focus on the added task content.
            Focus(FocusState.Programmatic);

            // Encourage user to set the task name.
            StartEditingName();
        }

        private void AddBtnKeyDown(object sender, KeyRoutedEventArgs e)
        {
            if (App.IsAcceptKey(e))
            {
                HasContent = true;

                // Attempt to focus on the added task content.
                Focus(FocusState.Programmatic);
            }
        }

        private void OnDeleteBtnTapped(object sender, TappedRoutedEventArgs e)
        {
            moreMenu.Hide();
            app.RemoveTask(this);
        }

        private void OnDeleteBtnKeyDown(object sender, KeyRoutedEventArgs e)
        {
            if (App.IsAcceptKey(e))
            {
                moreMenu.Hide();
                app.RemoveTask(this);
            }
        }
    }
}
