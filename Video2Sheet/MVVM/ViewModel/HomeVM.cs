#region "copyright"

/*
    Copyright © 2023 Christian Palm (christian@palm-family.de)
    This Source Code Form is subject to the terms of the Mozilla Public
    License, v. 2.0. If a copy of the MPL was not distributed with this
    file, You can obtain one at http://mozilla.org/MPL/2.0/.
*/

#endregion "copyright"

using Serilog;
using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows;
using Video2Sheet.Core;
using VideoLibrary;

namespace Video2Sheet.MVVM.ViewModel
{
    public class HomeVM : ObservableObject
    {
        public RelayCommand SearchVideo { get; set; }

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

        public HomeVM()
        {
            SearchVideo = new RelayCommand(async _ =>
            {
                try
                {
                    YouTube yt = YouTube.Default;
                    YouTubeVideo video = yt.GetVideo(VideoURL);
                    Log.Logger.Information($"Found video {video.FullName} for URL {VideoURL}");

                    LoadingVisibility = Visibility.Visible;
                    await File.WriteAllBytesAsync(Path.Combine(AppConstants.DATA_DIR, "TempVid.v2s"), await video.GetBytesAsync()); // Download yt video and write it to file
                }
                catch (Exception ex)
                {
                    Log.Logger.Error($"Error while processing URL: {ex.Message}\n{ex.StackTrace}");
                }
                finally
                {
                    LoadingVisibility = Visibility.Collapsed;
                }
            });
        }
    }
}
