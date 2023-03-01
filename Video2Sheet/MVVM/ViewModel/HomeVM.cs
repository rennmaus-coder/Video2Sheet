#region "copyright"

/*
    Copyright © 2023 Christian Palm (christian@palm-family.de)
    This Source Code Form is subject to the terms of the Mozilla Public
    License, v. 2.0. If a copy of the MPL was not distributed with this
    file, You can obtain one at http://mozilla.org/MPL/2.0/.
*/

#endregion "copyright"

using Newtonsoft.Json;
using Serilog;
using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Net.Http;
using System.Runtime.Serialization.Formatters.Binary;
using System.Text;
using System.Threading.Tasks;
using System.Windows;
using System.Windows.Input;
using System.Windows.Media.Imaging;
using Video2Sheet.Core;
using Video2Sheet.Core.Video;
using VideoLibrary;

namespace Video2Sheet.MVVM.ViewModel
{
    public class HomeVM : ObservableObject
    {
        public RelayCommand SearchVideo { get; set; }
        public RelayCommand LoadFromFile { get; set; }

        private string _videoURL;
        public string VideoURL
        {
            get => _videoURL;
            set
            {
                RaisePropertyChanged();
                _videoURL = value;
            }
        }

        private Visibility _loadervis = Visibility.Collapsed;
        public Visibility LoadingVisibility
        {
            get => _loadervis;
            set
            {
                _loadervis = value;
                RaisePropertyChanged();
            }
        }

        public VideoProject LoadedProject { get; set; }

        private BitmapSource _currentImage;
        public BitmapSource CurrentImage
        {
            get => _currentImage;
            set
            {
                CurrentImage = value;
                RaisePropertyChanged();
            }
        }

        public HomeVM()
        {
            SearchVideo = new RelayCommand(async _ =>
            {
                LoadingVisibility = Visibility.Visible;
                LoadedProject = await VideoImporter.LoadYoutubeVideo(VideoURL);
                LoadingVisibility = Visibility.Collapsed;
            });

            LoadFromFile = new RelayCommand(async _ =>
            {
                string file = Utility.FileDialog("Videos(*.v2s;*.mp4)|*.v2s;*.mp4", "Select file").First();
                LoadedProject = await VideoImporter.LoadProjectFile(file);
            });
        }

        public void KeyPress(KeyEventArgs e)
        {
            if (e.Key == Key.Left)
            {
                LoadedProject.ProcessingConfig.ExtractionPoints.Move(-5);
            }
            else if (e.Key == Key.Right)
            {
                LoadedProject.ProcessingConfig.ExtractionPoints.Move(5);
            }
        }
    }
}
