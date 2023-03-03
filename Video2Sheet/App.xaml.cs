#region "copyright"

/*
    Copyright © 2023 Christian Palm (christian@palm-family.de)
    This Source Code Form is subject to the terms of the Mozilla Public
    License, v. 2.0. If a copy of the MPL was not distributed with this
    file, You can obtain one at http://mozilla.org/MPL/2.0/.
*/

#endregion "copyright"

using Newtonsoft.Json;
using System.IO;
using System.Windows;
using Video2Sheet.Core;
using Video2Sheet.MVVM.ViewModel;

namespace Video2Sheet
{
    /// <summary>
    /// Interaction logic for App.xaml
    /// </summary>
    public partial class App : Application
    {
        private void Application_Exit(object sender, ExitEventArgs e)
        {
            Config.Save();
            string json = JsonConvert.SerializeObject(MainWindowVM.Instance.HomeVM.LoadedProject, Formatting.Indented);

            if (json == "{}")
                return;
            File.WriteAllText(Path.Combine(MainWindowVM.Instance.HomeVM.LoadedProject.GetFolder(), MainWindowVM.Instance.HomeVM.LoadedProject.GetFileName()), json);
        }
    }
}
