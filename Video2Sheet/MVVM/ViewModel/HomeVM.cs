﻿#region "copyright"

/*
    Copyright © 2023 Christian Palm (christian@palm-family.de)
    This Source Code Form is subject to the terms of the Mozilla Public
    License, v. 2.0. If a copy of the MPL was not distributed with this
    file, You can obtain one at http://mozilla.org/MPL/2.0/.
*/

#endregion "copyright"

using NAudio.Midi;
using OpenCvSharp;
using OpenCvSharp.WpfExtensions;
using Serilog;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using System.Windows;
using System.Windows.Media.Imaging;
using Video2Sheet.Core;
using Video2Sheet.Core.Keyboard;
using Video2Sheet.Core.Video;
using Video2Sheet.Core.Video.Processing;
using Video2Sheet.Core.Video.Processing.Detection;
using Video2Sheet.Core.Video.Processing.Util;
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

        private FrameProcessor processor;

        public HomeVM()
        {
            NoteValues notes = new NoteValues();
            notes.Init(1024f);
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
                        LoadedProject.ProcessingConfig.ExtractionPoints.Generate(LoadedProject.Piano, frame.Width);
                    }
                    CurrentImage = MatDrawer.DrawPointsToMat(frame, LoadedProject.ProcessingConfig.ExtractionPoints).ToBitmapSource();
                    HomeView.UpdateSliderMaximum(LoadedProject.VideoFile.TotalFrames);
                    LoadingVisibility = Visibility.Collapsed;

                    processor = new FrameProcessor(notes, LoadedProject.ProcessingConfig, 1024f, 0, 0, LoadedProject.Piano, (int)CurrentImage.Width);
                }
                catch (Exception ex)
                {
                    Log.Logger.Error($"Error occured while loading a video from youtube: {ex.Message}\n{ex.StackTrace}");
                }
            });

            LoadFromFile = new RelayCommand(_ =>
            {
                string file = Utility.FileDialog("Videos(*.v2s;*.mp4)|*.v2s;*.mp4", "Select file").First();
                if (file == string.Empty)
                    return;

                LoadedProject = VideoImporter.LoadProjectFile(file);
                LoadedProject.VideoFile.LoadFile();
                Mat frame = LoadedProject.VideoFile.GetNextFrame();
                if (LoadedProject.ProcessingConfig.ExtractionPoints.ExtractionPoints.Count == 0)
                {
                    LoadedProject.ProcessingConfig.ExtractionPoints.Generate(LoadedProject.Piano, frame.Width);
                }
                CurrentImage = MatDrawer.DrawPointsToMat(frame, LoadedProject.ProcessingConfig.ExtractionPoints).ToBitmapSource();
                HomeView.UpdateSliderMaximum(LoadedProject.VideoFile.TotalFrames);
                processor = new FrameProcessor(notes, LoadedProject.ProcessingConfig, 1024f, 0, 0, LoadedProject.Piano, (int)CurrentImage.Width);
            });

            MoveLeft = new RelayCommand(_ =>
            {
                if (LoadedProject is null)
                    return;

                if (LoadedProject.ProcessingConfig.ExtractionPoints[0].X <= Config.MarkerStep)
                    return;

                LoadedProject?.ProcessingConfig.ExtractionPoints.Move(-Config.MarkerStep);
                UpdateFrame(FrameNr);
                // UpdateFrameWithProcessing();
            });

            MoveRight = new RelayCommand(_ =>
            {
                if (LoadedProject is null)
                    return;

                if (LoadedProject.ProcessingConfig.ExtractionPoints[LoadedProject.ProcessingConfig.ExtractionPoints.ExtractionPoints.Count - 1].X >= LoadedProject.VideoFile.GetCurrentFrame().Width - Config.MarkerStep)
                    return;

                LoadedProject?.ProcessingConfig.ExtractionPoints.Move(Config.MarkerStep);
                UpdateFrame(FrameNr);
                // UpdateFrameWithProcessing();
            });

            MoveUp = new RelayCommand(_ =>
            {
                if (LoadedProject is null)
                    return;
                if (LoadedProject.ProcessingConfig.ExtractionPoints[0].Y >= LoadedProject.VideoFile.GetCurrentFrame().Height - Config.MarkerStep)
                    return;

                LoadedProject.ProcessingConfig.ExtractionPoints.MoveUp(-Config.MarkerStep * 15);
                UpdateFrame(FrameNr);
                // UpdateFrameWithProcessing();
            });

            MoveDown = new RelayCommand(_ =>
            {
                if (LoadedProject is null)
                    return;
                if (LoadedProject.ProcessingConfig.ExtractionPoints[0].Y <= Config.MarkerStep)
                    return;

                LoadedProject.ProcessingConfig.ExtractionPoints.MoveUp(Config.MarkerStep * 15);
                UpdateFrame(FrameNr);
                // UpdateFrameWithProcessing();
            });

            ProcessVideo = new RelayCommand(async _ =>
            {
                try
                {
                    ProgressVisibility = Visibility.Visible;
                    await Task.Factory.StartNew(() =>
                    {
                        VideoProcessor processor = new VideoProcessor(LoadedProject);
                        ProcessingCallback last = null;
                        DateTime start = DateTime.Now;
                        List<double> processing = new();
                        foreach (ProcessingCallback callback in processor.ProcessVideo())
                        {
                            DateTime localStart = DateTime.Now;
                            BitmapSource source = callback.CurrentFrame.ToBitmapSource();
                            source.Freeze();
                            CurrentImage = source;
                            AnalyseProgress = ((double)callback.FrameNr / (double)LoadedProject.VideoFile.TotalFrames) * 100;
                            last = callback;
                            processing.Add((DateTime.Now - localStart).TotalMilliseconds);
                        }
                        Log.Logger.Information($"Statistics:\n\tAverage time per frame: {processing.Average()}ms\n\tMax: {processing.Max()}ms ({processing.IndexOf(processing.Max())})\n\tMin: {processing.Min()}ms ({processing.IndexOf(processing.Min())})");
                        Log.Logger.Information($"Finished video analyse, took {DateTime.Now - start}, with {last?.Failures} possible failures");
                    });
                    ProgressVisibility = Visibility.Collapsed;
                } 
                catch (Exception ex)
                {
                    Log.Logger.Error($"Error occured while processing video: {ex.Message}\n{ex.StackTrace}");
                }
            });
        }

        private void UpdateFrameWithProcessing()
        {
            Mat frame = LoadedProject?.VideoFile.GetCurrentFrame();
            ProcessingLog log = new ProcessingLog();
            MidiEventCollection events = new MidiEventCollection(0, 1024);
            ProcessingCallback callback = processor.ProcessFrame(ref frame, ref log, ref events, FrameNr);

            List<Scalar> colors = new List<Scalar>();
            foreach (Key k in processor.keys)
            {
                colors.Add(k.IsPressed ? Scalar.White : Scalar.Black);
            }

            frame = MatDrawer.DrawPointsToMat(frame, LoadedProject.ProcessingConfig.ExtractionPoints, colors);
            CurrentImage = frame.ToBitmapSource();
        }

        public void UpdateFrame(int index = 0)
        {
            if (LoadedProject is null)
                return;

            CurrentImage = MatDrawer.DrawPointsToMat(LoadedProject.VideoFile.GetFrameAtIndex(index), LoadedProject.ProcessingConfig.ExtractionPoints).ToBitmapSource();
        }
    }
}
