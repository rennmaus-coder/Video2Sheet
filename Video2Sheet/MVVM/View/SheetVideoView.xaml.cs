﻿#region "copyright"

/*
    Copyright © 2023 Christian Palm (christian@palm-family.de)
    This Source Code Form is subject to the terms of the Mozilla Public
    License, v. 2.0. If a copy of the MPL was not distributed with this
    file, You can obtain one at http://mozilla.org/MPL/2.0/.
*/

#endregion "copyright"

using System.Windows.Controls;
using Video2Sheet.MVVM.ViewModel;

namespace Video2Sheet.MVVM.View
{
    /// <summary>
    /// Interaktionslogik für HomeView.xaml
    /// </summary>
    public partial class SearchVideoView : UserControl
    {
        private static SearchVideoView inst;
        public SearchVideoView()
        {
            InitializeComponent();
            inst = this;
        }

        private void Slider_MouseLeftButtonUp(object sender, System.Windows.Input.MouseButtonEventArgs e)
        {
            MainWindowVM.Instance.SheetVideoVM.UpdateFrame((int)frameNr.Value);
            MainWindowVM.Instance.SheetVideoVM.FrameNr = (int)frameNr.Value;
        }

        public static void UpdateSliderMaximum(int max)
        {
            inst.frameNr.Maximum = max;
        }
    }
}
