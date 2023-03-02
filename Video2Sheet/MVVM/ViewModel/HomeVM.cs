#region "copyright"

/*
    Copyright © 2023 Christian Palm (christian@palm-family.de)
    This Source Code Form is subject to the terms of the Mozilla Public
    License, v. 2.0. If a copy of the MPL was not distributed with this
    file, You can obtain one at http://mozilla.org/MPL/2.0/.
*/

#endregion "copyright"

using OpenCvSharp;
using OpenCvSharp.WpfExtensions;
using Serilog;
using System;
using System.Linq;
using System.Threading;
using System.Threading.Tasks;
using System.Windows;
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
        public RelayCommand MoveUp { get; set; }
        public RelayCommand MoveDown { get; set; }
        public RelayCommand ProcessVideo { get; set; }

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

        private Visibility _progVis = Visibility.Collapsed;
        public Visibility ProgressVisibility
        {
            get => _progVis;
            set
            {
                _progVis = value;
                RaisePropertyChanged();
            }
        }

        private double _analyseProgress = 0;
        public double AnalyseProgress
        {
            get => _analyseProgress;
            set
            {
                _analyseProgress = value;
                RaisePropertyChanged();
            }
        }

        private int _framenr = 0;
        public int FrameNr
        {
            get => _framenr;
            set
            {
                _framenr = value;
                RaisePropertyChanged();
            }
        }

        private VideoProject _project;
        public VideoProject LoadedProject
        {
            get => _project;
            set
            {
                _project = value;
                RaisePropertyChanged();
            }
        }

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

                if (LoadedProject.ProcessingConfig.ExtractionPoints[0].X <= Config.MarkerStep)
                    return;

                LoadedProject?.ProcessingConfig.ExtractionPoints.Move(-Config.MarkerStep);
                UpdateFrame(FrameNr);
            });

            MoveRight = new RelayCommand(_ =>
            {
                if (LoadedProject is null)
                    return;

                if (LoadedProject.ProcessingConfig.ExtractionPoints[LoadedProject.ProcessingConfig.ExtractionPoints.ExtractionPoints.Count - 1].X >= LoadedProject.VideoFile.GetCurrentFrame().Width - Config.MarkerStep)
                    return;

                LoadedProject?.ProcessingConfig.ExtractionPoints.Move(Config.MarkerStep);
                UpdateFrame(FrameNr);
            });

            MoveUp = new RelayCommand(_ =>
            {
                if (LoadedProject is null)
                    return;
                if (LoadedProject.ProcessingConfig.ExtractionPoints[0].Y >= LoadedProject.VideoFile.GetCurrentFrame().Height - Config.MarkerStep)
                    return;

                LoadedProject.ProcessingConfig.ExtractionPoints.MoveUp(-Config.MarkerStep * 15);
                UpdateFrame(FrameNr);
            });

            MoveDown = new RelayCommand(_ =>
            {
                if (LoadedProject is null)
                    return;
                if (LoadedProject.ProcessingConfig.ExtractionPoints[0].Y <= Config.MarkerStep)
                    return;

                LoadedProject.ProcessingConfig.ExtractionPoints.MoveUp(Config.MarkerStep * 15);
                UpdateFrame(FrameNr);
            });

            ProcessVideo = new RelayCommand(async _ =>
            {
                try
                {
                    ProgressVisibility = Visibility.Visible;
                    await Task.Factory.StartNew(() =>
                    {
                        VideoProcessor processor = new VideoProcessor(LoadedProject);
                        foreach (ProcessingCallback callback in processor.ProcessVideo())
                        {
                            BitmapSource source = callback.CurrentFrame.ToBitmapSource();
                            source.Freeze();
                            CurrentImage = source;
                            AnalyseProgress = callback.FrameNr / LoadedProject.VideoFile.TotalFrames;
                        }
                    });
                    ProgressVisibility = Visibility.Collapsed;
                } 
                catch (Exception ex)
                {
                    Log.Logger.Error($"Error occured while processing video: {ex.Message}\n{ex.StackTrace}");
                }
            });
        }

        public void UpdateFrame(int index = 0)
        {
            if (LoadedProject is null)
                return;

            CurrentImage = MatDrawer.DrawPointsToMat(LoadedProject.VideoFile.GetFrameAtIndex(index), LoadedProject.ProcessingConfig.ExtractionPoints).ToBitmapSource();
        }
    }
}
