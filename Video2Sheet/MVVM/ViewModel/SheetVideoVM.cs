#region "copyright"

/*
    Copyright © 2023 Christian Palm (christian@palm-family.de)
    This Source Code Form is subject to the terms of the Mozilla Public
    License, v. 2.0. If a copy of the MPL was not distributed with this
    file, You can obtain one at http://mozilla.org/MPL/2.0/.
*/

#endregion "copyright"

using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows.Media.Imaging;
using System.Windows;
using Video2Sheet.Core.Video;
using OpenCvSharp;
using Serilog;
using System.Diagnostics;
using Video2Sheet.Core.Video.Processing.Detection;
using Video2Sheet.Core.Video.Processing;
using Video2Sheet.Core;
using Video2Sheet.MVVM.View;
using OpenCvSharp.WpfExtensions;
using System.IO;
using System.Windows.Controls;
using System.Xml.Linq;
using System.Windows.Threading;
using System.Collections;

namespace Video2Sheet.MVVM.ViewModel
{
    public class SheetVideoVM : ObservableObject
    {
        public RelayCommand SearchVideo { get; set; }
        public RelayCommand LoadFromFile { get; set; }
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

        private int startFrame = 0;
        public int StartFrame
        {
            get => startFrame;
            set
            {
                startFrame = value;
                RaisePropertyChanged();
            }
        }

        private int endFrame = 0;
        public int EndFrame
        {
            get => endFrame;
            set
            {
                endFrame = value;
                RaisePropertyChanged();
            }
        }

        private int high = 100;
        public int High
        {
            get => high;
            set
            {
                high = value;
                RaisePropertyChanged();
            }
        }

        private int low = 10;
        public int Low
        {
            get => low;
            set
            {
                low = value;
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

        private VideoFile video;
        public VideoFile Video
        {
            get => video;
            set
            {
                video = value;
                RaisePropertyChanged();
            }
        }

        public SheetVideoVM()
        {
            SearchVideo = new RelayCommand(async _ =>
            {
                try
                {
                    LoadingVisibility = Visibility.Visible;
                    Video = (await VideoImporter.LoadYoutubeVideo(VideoURL)).VideoFile;
                    Video.LoadFile();
                    Mat frame = Video.GetNextFrame();
                    UpdateFrame();

                    SearchVideoView.UpdateSliderMaximum(Video.TotalFrames);
                    LoadingVisibility = Visibility.Collapsed;
                }
                catch (Exception ex)
                {
                    Log.Logger.Error($"Error occured while loading a video from youtube: {ex.Message}\n{ex.StackTrace}");
                }
            });

            LoadFromFile = new RelayCommand(_ =>
            {
                string file = Utility.FileDialog("Video(*.mp4)|*.mp4", "Select file").First();
                if (file == string.Empty)
                    return;

                Video = new VideoFile(Path.GetFileNameWithoutExtension(file));
                if (!File.Exists(Video.GetFilePath()))
                {
                    Directory.CreateDirectory(Path.GetDirectoryName(Video.GetFilePath()));
                    File.Copy(file, Video.GetFilePath());
                }
                Video.LoadFile();
                Mat frame = Video.GetNextFrame();
                UpdateFrame();

                SearchVideoView.UpdateSliderMaximum(Video.TotalFrames);
            });

            ProcessVideo = new RelayCommand(async _ =>
            {
                ProgressVisibility = Visibility.Visible;

                EndFrame = EndFrame != 0 ? EndFrame : Video.TotalFrames;
                string path = Path.Combine(Environment.GetFolderPath(Environment.SpecialFolder.Desktop), "Sheet");
                Directory.CreateDirectory(path);
                await Compute(High, Low, StartFrame, EndFrame, path);

                ProgressVisibility = Visibility.Collapsed;
            });
        }

        public void UpdateFrame(int index = 0)
        {
            if (Video is null)
                return;

            Video.SetFrame(index);
            FrameNr = index;
            Video.GetNextFrame();
            UpdateFrame();
        }

        private void UpdateFrame()
        {
            Mat frame = Video.GetCurrentFrame();
            frame = MatDrawer.DrawLine(frame, High);
            frame = MatDrawer.DrawLine(frame, Low);
            CurrentImage = frame.ToBitmapSource();
        }

        private void UpdateFrameMat(Mat frame)
        {
            frame = MatDrawer.DrawLine(frame, High);
            frame = MatDrawer.DrawLine(frame, Low);
            CurrentImage = frame.ToBitmapSource();
        }

        private async Task Compute(int high, int low, int start, int end, string folder)
        {
            await Task.Run(() =>
            {
                Mat lastMat = new Mat();

                for (int i = start; i < end; i += 24)
                {
                    if (i > end)
                    {
                        break;
                    }
                    Mat frame = Video.GetFrameAtIndex(i);
                    frame = frame.CvtColor(ColorConversionCodes.BGR2GRAY);
                    frame = frame[new OpenCvSharp.Rect(0, low, frame.Width - 1, high - low)];
                    if (!AreEqual(lastMat, frame, 1_500_000))
                    {
                        FinalizeImage(frame).SaveImage($"C:/Users/Christian/Desktop/Cache/{i}.jpg");
                    }
                    lastMat = frame;
                    Application.Current.Dispatcher.Invoke(() =>
                    {
                        AnalyseProgress = ((double)i / (double)EndFrame) * 100;
                        UpdateFrameMat(frame);
                    });
                }
            });

        }

        private bool AreEqual(Mat one, Mat two, int threshold)
        {
            if (one.Width != two.Width || one.Height != two.Height)
            {
                return false;
            }
            Mat res = new Mat();
            Cv2.Subtract(one, two, res);
            double r = res.Sum().Val0;
            return r < threshold;
        }

        private Mat FinalizeImage(Mat image)
        {
            float ar = (float)image.Height / (float)image.Width;
            Mat result = image.Resize(new OpenCvSharp.Size(1080, 1080 * ar));
            result = image.GaussianBlur(OpenCvSharp.Size.Zero, 3);
            Cv2.AddWeighted(image, 1.5, result, -0.5, 0, result);
            return result;
        }
    }
}
