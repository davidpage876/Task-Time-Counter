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

// The User Control item template is documented at https://go.microsoft.com/fwlink/?LinkId=234236

namespace Task_Time_Counter_2
{
    public sealed partial class Task : UserControl
    {
        private bool isActive = false;
        private bool isRecording = false;
        Button toTopBtn;
        Button playPauseBtn;

        public Task()
        {
            this.InitializeComponent();

            // Get control references.
            toTopBtn = FindName("ToTopBtn") as Button;
            playPauseBtn = FindName("PlayPauseBtn") as Button;

            // Set default state.
            Active = false;
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
                    isRecording = false;
                }

                UpdateUI();
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
                UpdateUI();
            }
            get { return isRecording; }
        }

        private void UpdateUI()
        {
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
        }

        private void textBlock_SelectionChanged(object sender, RoutedEventArgs e)
        {

        }

        private void TimeEdit_TextChanged(object sender, TextChangedEventArgs e)
        {

        }

        private void TimeBtn_Click(object sender, RoutedEventArgs e)
        {

        }
        
        private void onPlayPauseTapped(object sender, TappedRoutedEventArgs e)
        {
            IsRecording = !IsRecording;
        }
    }
}
