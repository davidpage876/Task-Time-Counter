using System;
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
using Windows.Storage;

// The User Control item template is documented at https://go.microsoft.com/fwlink/?LinkId=234236

namespace Task_Time_Counter_2
{
    public sealed partial class SettingsFlyout : UserControl
    {
        private App app;
        private CheckBox showDecimalTimesToggle;
        private CheckBox recordOnTaskMovedToTopToggle;

        public SettingsFlyout()
        {
            this.InitializeComponent();

            app = (App)Application.Current;

            // Get control references.
            showDecimalTimesToggle = FindName("ShowDecimalTimesToggle") as CheckBox;
            recordOnTaskMovedToTopToggle = FindName("RecordOnTaskMovedToTopToggle") as CheckBox;
        }

        /// <summary>
        /// Update setting controls based on state.
        /// </summary>
        public void UpdateUI()
        {
            showDecimalTimesToggle.IsChecked = app.ShowDecimalTimes;
            recordOnTaskMovedToTopToggle.IsChecked = app.RecordOnTaskMovedToTop;
        }

        private App AppRef
        {
            get 
            {
                if (app == null)
                {
                    app = (App)Application.Current;
                }
                return app;
            }
        }

        private void OnShowDecimalTimesChecked(object sender, RoutedEventArgs e)
        {
            AppRef.ShowDecimalTimes = true;
        }

        private void OnShowDecimalTimesUnchecked(object sender, RoutedEventArgs e)
        {
            AppRef.ShowDecimalTimes = false;
        }

        private void OnRecordOnTaskMovedToTopChecked(object sender, RoutedEventArgs e)
        {
            AppRef.RecordOnTaskMovedToTop = true;
        }

        private void OnRecordOnTaskMovedToTopUnchecked(object sender, RoutedEventArgs e)
        {
            AppRef.RecordOnTaskMovedToTop = false;
        }
    }
}
