﻿using System;
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

// The User Control item template is documented at https://go.microsoft.com/fwlink/?LinkId=234236

namespace Task_Time_Counter_2
{
    public sealed partial class Task : UserControl
    {
        private DispatcherTimer dispatchTimer;
        private Stopwatch stopwatch;
        private bool isActive = false;
        private bool isRecording = false;
        private bool isEditingName = false;
        private bool isEditingTime = false;
        private string taskName = "Unnamed task";

        // References.
        private Button toTopBtn;
        private Button playPauseBtn;
        private Button timeBtn;
        private TextBox timeEdit;
        private Button nameBtn;
        private TextBox nameEdit;

        public Task()
        {
            this.InitializeComponent();

            // Get control references.
            toTopBtn = FindName("ToTopBtn") as Button;
            playPauseBtn = FindName("PlayPauseBtn") as Button;
            timeBtn = FindName("TimeBtn") as Button;
            timeEdit = FindName("TimeEdit") as TextBox;
            nameBtn = FindName("NameBtn") as Button;
            nameEdit = FindName("NameEdit") as TextBox;

            // Set up stopwatch for recording time.
            stopwatch = new Stopwatch();

            // Set up dispatch timer for updating UI.
            dispatchTimer = new DispatcherTimer();
            dispatchTimer.Tick += OnTimerTick;
            dispatchTimer.Interval = new TimeSpan(0, 0, 0, 0, 100);

            // Update UI based on default state.
            UpdateUI();
        }

        /// <summary>
        /// Is this task the active timer.
        /// 
        /// If the task is not active we stop recording to the timer.
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
        /// Returns the current recorded time on the timer in seconds.
        /// </summary>
        public TimeSpan Time
        {
            get 
            {
                return stopwatch.Elapsed;
            }
        }

        private void UpdateUI()
        {
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

        private void UpdateNameUI()
        {
            nameBtn.Content = taskName;
        }

        private void UpdateTimerUI()
        {
            timeBtn.Content = FormatTimeLong(Time);
        }

        private void OnTimerTick(object sender, object e)
        {
            UpdateTimerUI();
        }

        private string FormatTimeLong(TimeSpan elapsed)
        {
            return string.Format("({0}) {1:00}:{2:00}:{3:00}.{4:0}",
                Math.Round(elapsed.TotalHours, 1),
                elapsed.Hours, elapsed.Minutes, elapsed.Seconds, elapsed.Milliseconds / 100);
        }

        private string FormatTimeShort(TimeSpan elapsed)
        {
            return string.Format("{0:00}:{1:00}:{2:00}.{3:0}",
                elapsed.Hours, elapsed.Minutes, elapsed.Seconds, elapsed.Milliseconds / 100);
        }

        private void OpenEditName()
        {
            isEditingName = true;

            // Set the edit field text to match the current text.
            nameEdit.Text = taskName;

            // Open the edit field and focus on it.
            nameBtn.Visibility = Visibility.Collapsed;
            nameEdit.Visibility = Visibility.Visible;
            nameEdit.Focus(FocusState.Programmatic);
            nameEdit.SelectAll();
        }

        private void CloseEditName(bool apply)
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

            // Focus on the name button, without showing the keyboard focus visual.
            nameBtn.Focus(FocusState.Pointer);
        }

        private void OpenEditTime()
        {
            isEditingTime = true;

            // Set the edit field time to match the current text.
            timeEdit.Text = FormatTimeShort(Time);

            // Open the edit field and focus on it.
            timeBtn.Visibility = Visibility.Collapsed;
            timeEdit.Visibility = Visibility.Visible;
            timeEdit.Focus(FocusState.Programmatic);
            timeEdit.SelectAll();
        }

        private void CloseEditTime(bool apply)
        {
            isEditingTime = false;

            // Attempt to apply change.
            if (apply) 
            {
                try
                {
                    // Get hour, minute and seconds from input.
                    string input = timeEdit.Text;
                    string[] parts = input.Split(':');
                    if (parts.Length != 3)
                    {
                        throw new Exception(string.Format("Invalid format: \"{0}\"", input));
                    }

                    int hour = int.Parse(parts[0]); // May throw FormatException.
                    if (hour > 60 || hour < 0)
                    {
                        throw new Exception(string.Format("Hours outside of range (0-60): \"{0}\"", input));
                    }

                    int minute = int.Parse(parts[1]); // May throw FormatException.
                    if (minute > 60 || minute < 0)
                    {
                        throw new Exception(string.Format("Minutes outside of range (0-60): \"{0}\"", input));
                    }

                    float seconds = float.Parse(parts[2]); // May throw FormatException.
                    if (seconds > 60f + float.Epsilon || seconds < float.Epsilon)
                    {
                        throw new Exception(string.Format("Seconds outside of range (0-60): \"{0}\"", input));
                    }

                    // Update timer with new time.
                    // Todo.

                    UpdateTimerUI();
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

            // Focus on the time button, without showing the keyboard focus visual.
            timeBtn.Focus(FocusState.Pointer);
        }

        private void textBlock_SelectionChanged(object sender, RoutedEventArgs e)
        {

        }
        
        private void onPlayPauseTapped(object sender, TappedRoutedEventArgs e)
        {
            IsRecording = !IsRecording;
        }

        private void OnNameTapped(object sender, TappedRoutedEventArgs e)
        {
            // Enable editing of task name.
            OpenEditName();
        }

        private void OnNameFocusLost(object sender, RoutedEventArgs e)
        {
            // Apply editing of task name on focus lost.
            if (isEditingName)
            {
                CloseEditName(true);
            }
        }

        private void OnNameKeyDown(object sender, KeyRoutedEventArgs e)
        {
            switch (e.Key)
            {
                // Cancel task name edit on Escape.
                case Windows.System.VirtualKey.Escape:
                    CloseEditName(false);
                    break;

                // Apply task name edit on Enter.
                case Windows.System.VirtualKey.Enter:
                    CloseEditName(true);
                    break;
            }
        }

        private void OnNameContextMenuOpening(object sender, ContextMenuEventArgs e)
        {
            // Prevent default context menu from opening.
            e.Handled = true;

            // Apply editing of task name on context menu open (right click).
            CloseEditName(true);
        }

        private void OnTimeTapped(object sender, TappedRoutedEventArgs e)
        {
            // Enable editing of task time.
            OpenEditTime();
        }

        private void OnTimeLostFocus(object sender, RoutedEventArgs e)
        {
            // Apply editing of task time on focus lost.
            if (isEditingTime)
            {
                CloseEditTime(true);
            }
        }

        private void OnTimeKeyDown(object sender, KeyRoutedEventArgs e)
        {
            switch (e.Key)
            {
                // Cancel task time edit on Escape.
                case Windows.System.VirtualKey.Escape:
                    CloseEditTime(false);
                    break;

                // Apply task time edit on Enter.
                case Windows.System.VirtualKey.Enter:
                    CloseEditTime(true);
                    break;
            }
        }

        private void OnTimeContextMenuOpening(object sender, ContextMenuEventArgs e)
        {
            // Prevent default context menu from opening.
            e.Handled = true;

            // Apply editing of task time on context menu open (right click).
            CloseEditTime(true);
        }
    }
}
