#region "copyright"

/*
    Copyright © 2023 Christian Palm (christian@palm-family.de)
    This Source Code Form is subject to the terms of the Mozilla Public
    License, v. 2.0. If a copy of the MPL was not distributed with this
    file, You can obtain one at http://mozilla.org/MPL/2.0/.
*/

#endregion "copyright"

using Newtonsoft.Json;
using OpenCvSharp;
using Serilog;
using System;
using System.IO;

namespace Video2Sheet.Core.Video
{
    public class VideoFile
    {
        public string Title { get; set; }

        [JsonIgnore]
        public int TotalFrames
        {
            get => capture?.FrameCount ?? 0;
        }

        [JsonIgnore]
        public double FPS
        {
            get => capture?.Fps ?? 0;
        }

        private VideoCapture capture;
        private Mat currentFrame;

        public VideoFile(string title)
        {
            this.Title = title;
        }

        public Mat GetNextFrame()
        {
            Mat image = new Mat();
            try
            {
                if (!capture.Read(image))
                {
                    Log.Logger.Information("No Frame found");
                }
                currentFrame?.Dispose();
                currentFrame = image;
            }
            catch (Exception ex)
            {
                Log.Logger.Error($"Something went wrong grabbing the next Frame: {ex.Message}\n{ex.StackTrace}");
            }
            return currentFrame;
        }

        public Mat GetCurrentFrame()
        {
            return currentFrame;
        }

        public Mat GetFrameAtIndex(int index)
        {
            int old = capture.PosFrames;
            capture.PosFrames = index;

            Mat res = new Mat();
            capture.Read(res);

            capture.PosFrames = old;

            currentFrame = res;
            return res;
        }

        public void SetFrame(int index)
        {
            capture.PosFrames = index;
        }

        public string GetFilePath()
        {
            return Path.Combine(AppConstants.DATA_DIR, Utility.ReplaceInvalidChars(Title), Utility.ReplaceInvalidChars(Title) + ".mp4");
        }

        public void LoadFile()
        {
            if (capture != null)
            {
                Log.Logger.Debug("File already loaded, returning...");
                return;
            }
            string file = GetFilePath();
            capture = new VideoCapture(file);
        }
    }
}
