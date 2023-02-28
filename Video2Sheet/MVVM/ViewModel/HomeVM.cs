#region "copyright"

/*
    Copyright © 2023 Christian Palm (christian@palm-family.de)
    This Source Code Form is subject to the terms of the Mozilla Public
    License, v. 2.0. If a copy of the MPL was not distributed with this
    file, You can obtain one at http://mozilla.org/MPL/2.0/.
*/

#endregion "copyright"

using MemoryPack;
using Serilog;
using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Net.Http;
using System.Text;
using System.Threading.Tasks;
using System.Windows;
using Video2Sheet.Core;
using Video2Sheet.Core.Video;
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
                await LoadVideo(VideoURL);
            });
        }

        public async Task<VideoFile> LoadVideo(string url)
        {
            try
            {
                YouTube yt = YouTube.Default;
                YouTubeVideo ytvideo = yt.GetVideo(url);
                Log.Logger.Information($"Found video {ytvideo.FullName} for URL {url}");

                LoadingVisibility = Visibility.Visible;
                string path = Utility.ReplaceInvalidChars(Path.Combine(AppConstants.DATA_DIR, $"{ytvideo.Title}.v2s"));

                Log.Logger.Information("Starting download from " + ytvideo.Uri);
                VideoFile video = new VideoFile(await ytvideo.GetBytesAsync(), ytvideo);
                File.WriteAllBytes(path, MemoryPackSerializer.Serialize(video));
                File.Delete(path);
                return video;
            }
            catch (Exception ex)
            {
                Log.Logger.Error($"Error while processing URL: {ex.Message}\n{ex.StackTrace}");
            }
            finally
            {
                LoadingVisibility = Visibility.Collapsed;
            }
            return null;
        }
    }
}
