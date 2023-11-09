#region "copyright"

/*
    Copyright © 2023 Christian Palm (christian@palm-family.de)
    This Source Code Form is subject to the terms of the Mozilla Public
    License, v. 2.0. If a copy of the MPL was not distributed with this
    file, You can obtain one at http://mozilla.org/MPL/2.0/.
*/

#endregion "copyright"

namespace Video2Sheet.MVVM.ViewModel
{
    public class MainWindowVM : ObservableObject
    {
        public static MainWindowVM Instance { get; set; }

        public HomeVM HomeVM { get; set; }
        public OptionsVM OptionsVM { get; set; }
        public SheetVideoVM SheetVideoVM { get; set; }

        public MainWindowVM()
        {
            Instance = this;
            HomeVM = new HomeVM();
            SheetVideoVM = new SheetVideoVM();
            OptionsVM = new OptionsVM();
        }
    }
}
