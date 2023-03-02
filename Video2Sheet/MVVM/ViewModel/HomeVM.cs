#region "copyright"

/*
    Copyright © 2023 Christian Palm (christian@palm-family.de)
    This Source Code Form is subject to the terms of the Mozilla Public
    License, v. 2.0. If a copy of the MPL was not distributed with this
    file, You can obtain one at http://mozilla.org/MPL/2.0/.
*/

#endregion "copyright"

using OpenCvSharp;
using OpenCvSharp.Extensions;
using OpenCvSharp.WpfExtensions;
using Serilog;
using System;
using System.Linq;
using System.Windows;
using System.Windows.Input;
using System.Windows.Media.Imaging;
using Video2Sheet.Core;
using Video2Sheet.Core.Video;
using Video2Sheet.Core.Video.Processing;
using Video2Sheet.MVVM.View;

namespace Video2Sheet.MVVM.ViewModel
{
    public class HomeVM : ObservableObject
    {
        public RelayCommand SearchVideo { get; set; }
        public RelayCommand LoadFromFile { get; set; }
        public RelayCommand MoveLeft { get; set; }
        public RelayCommand MoveRight { get; set; }

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

        private double _framenr = 0;
        public double FrameNr
        {
            get => _framenr;
            set
            {
                _framenr = value;
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
                _currentImage = value;
                RaisePropertyChanged();
            }
        }

        public HomeVM()
        {
            SearchVideo = new RelayCommand(async _ =>
            {
                try
                {
                    LoadingVisibility = Visibility.Visible;
                    LoadedProject = await VideoImporter.LoadYoutubeVideo(VideoURL);
                    LoadedProject.VideoFile.LoadFile();
                    Mat frame = LoadedProject.VideoFile.GetNextFrame();
                    if (LoadedProject.ProcessingConfig.ExtractionPoints.ExtractionPoints.Count == 0)
                    {
                        LoadedProject.ProcessingConfig.GenerateExtractionPoints(frame.Width);
                    }
                    CurrentImage = MatDrawer.DrawPointsToMat(frame, LoadedProject.ProcessingConfig.ExtractionPoints).ToBitmapSource();
                    HomeView.UpdateSliderMaximum(LoadedProject.VideoFile.TotalFrames);
                    LoadingVisibility = Visibility.Collapsed;
                }
                catch (Exception ex)
                {
                    Log.Logger.Error($"Error occured while loading a video from youtube: {ex.Message}\n{ex.StackTrace}");
                }
            });

            LoadFromFile = new RelayCommand(_ =>
            {
                string file = Utility.FileDialog("Videos(*.v2s;*.mp4)|*.v2s;*.mp4", "Select file").First();
                LoadedProject = VideoImporter.LoadProjectFile(file);
                LoadedProject.VideoFile.LoadFile();
                Mat frame = LoadedProject.VideoFile.GetNextFrame();
                if (LoadedProject.ProcessingConfig.ExtractionPoints.ExtractionPoints.Count == 0)
                {
                    LoadedProject.ProcessingConfig.GenerateExtractionPoints(frame.Width);
                }
                CurrentImage = MatDrawer.DrawPointsToMat(frame, LoadedProject.ProcessingConfig.ExtractionPoints).ToBitmapSource();
                HomeView.UpdateSliderMaximum(LoadedProject.VideoFile.TotalFrames);
            });

            MoveLeft = new RelayCommand(_ =>
            {
                if (LoadedProject is null)
                    return;

                if (LoadedProject.ProcessingConfig.ExtractionPoints[0].X <= 5)
                    return;

                LoadedProject?.ProcessingConfig.ExtractionPoints.Move(-5);
                UpdateFrame(FrameNr);
            });

            MoveRight = new RelayCommand(_ =>
            {
                if (LoadedProject is null)
                    return;

                if (LoadedProject.ProcessingConfig.ExtractionPoints[LoadedProject.ProcessingConfig.ExtractionPoints.ExtractionPoints.Count - 1].X >= LoadedProject.VideoFile.GetCurrentFrame().Width - 5)
                    return;

                LoadedProject?.ProcessingConfig.ExtractionPoints.Move(5);
                UpdateFrame(FrameNr);
            });
        }

        public void UpdateFrame(double index = 0)
        {
            if (LoadedProject is null)
                return;

            CurrentImage = MatDrawer.DrawPointsToMat(LoadedProject.VideoFile.GetFrameAtIndex(index), LoadedProject.ProcessingConfig.ExtractionPoints).ToBitmapSource();
        }
    }
}
