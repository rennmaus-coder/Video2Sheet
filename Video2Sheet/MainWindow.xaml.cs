#region "copyright"

/*
    Copyright © 2023 Christian Palm (christian@palm-family.de)
    This Source Code Form is subject to the terms of the Mozilla Public
    License, v. 2.0. If a copy of the MPL was not distributed with this
    file, You can obtain one at http://mozilla.org/MPL/2.0/.
*/

#endregion "copyright"

using Serilog;
using System.IO;
using System.Windows;
using Video2Sheet.Core;
using Video2Sheet.MVVM.ViewModel;
using Wpf.Ui.Controls;

namespace Video2Sheet
{
    /// <summary>
    /// Interaction logic for MainWindow.xaml
    /// </summary>
    public partial class MainWindow : UiWindow
    {
        public static DependencyObject DepObject;
        public MainWindow()
        {
            Directory.CreateDirectory(AppConstants.DATA_DIR);

            if (File.Exists(Path.Combine(AppConstants.DATA_DIR, "latest.log")))
                File.Delete(Path.Combine(AppConstants.DATA_DIR, "latest.log"));

            Log.Logger = new LoggerConfiguration()
                .MinimumLevel.Is(Config.LogLevel)
                .WriteTo.Console()
                .WriteTo.File(Path.Combine(AppConstants.DATA_DIR, "latest.log"), outputTemplate: "{Timestamp:HH:mm:ss} [{Level:u3}] {Message:lj}{NewLine}{Exception}")
                .CreateLogger();

            new MainWindowVM();
            InitializeComponent();
            DepObject = this;
        }
    }
}
