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

// The Blank Page item template is documented at https://go.microsoft.com/fwlink/?LinkId=402352&clcid=0x409

namespace Task_Time_Counter_2
{
    /// <summary>
    /// An empty page that can be used on its own or navigated to within a Frame.
    /// </summary>
    public sealed partial class MainPage : Page
    {
        private App app;

        public MainPage()
        {
            this.InitializeComponent();

            // Initialize UI.
            app = (App)Application.Current;
            app.InitializeUI(this);
        }

        private void OnClearTimesTapped(object sender, TappedRoutedEventArgs e)
        {
            app.ClearTaskTimes();
        }

        private void OnClearTimesKeyDown(object sender, KeyRoutedEventArgs e)
        {
            if (App.IsAcceptKey(e))
            {
                app.ClearTaskTimes();
            }
        }

        private void OnClearAllTapped(object sender, TappedRoutedEventArgs e)
        {
            app.ClearTaskTimes();
            app.ClearTaskNames();
            app.ClearTaskListLayout();
        }

        private void OnClearAllKeyDown(object sender, KeyRoutedEventArgs e)
        {
            if (App.IsAcceptKey(e))
            {
                app.ClearTaskTimes();
                app.ClearTaskNames();
                app.ClearTaskListLayout();
            }
        }
    }
}
