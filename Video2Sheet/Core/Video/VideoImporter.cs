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
using System.Text;
using System.Threading.Tasks;
using System.Windows;
using VideoLibrary;

namespace Video2Sheet.Core.Video
{
    public static class VideoImporter
    {
        public static async Task<VideoProject> LoadYoutubeVideo(string url)
        {
            try
            {
                YouTube yt = YouTube.Default;
                IEnumerable<YouTubeVideo> ytvideos = yt.GetAllVideos(url);
                YouTubeVideo ytvideo = ytvideos.Where(x => x.Resolution == Config.VideoResolution).FirstOrDefault(ytvideos.First());
                Log.Logger.Information($"Found video {ytvideo.FullName} for URL {url}");

                Log.Logger.Information("Starting download from " + ytvideo.Uri);
                DateTime start = DateTime.Now;

                VideoFile video = new VideoFile(await ytvideo.GetBytesAsync(), ytvideo.Title);

                Log.Logger.Information("Video download took: " + (DateTime.Now - start).TotalSeconds + "s");

                VideoProject project = new VideoProject(video);

                Directory.CreateDirectory(project.GetFolder());
                string json = JsonConvert.SerializeObject(project);

                File.WriteAllText(Path.Combine(project.GetFolder(), project.GetFileName()), json);
                await File.WriteAllBytesAsync(Path.Combine(project.GetFolder(), project.GetFileName().Replace("v2s", "mp4")), project.VideoFile.VideoData);

                return project;
            }
            catch (Exception ex)
            {
                Log.Logger.Error($"Error while processing URL: {ex.Message}\n{ex.StackTrace}");
            }
            return null;
        }

        public static async Task<VideoProject> LoadProjectFile(string filepath)
        {
            VideoProject project = null;
            try
            {
                switch (Path.GetExtension(filepath))
                {
                    case ".v2s":
                        {
                            project = JsonConvert.DeserializeObject<VideoProject>(filepath);
                            project.VideoFile.VideoData = await File.ReadAllBytesAsync(filepath.Replace("v2s", "mp4"));
                            break;
                        }
                    case ".mp4":
                        {
                            VideoFile file = new VideoFile(await File.ReadAllBytesAsync(filepath), Path.GetFileNameWithoutExtension(filepath));
                            project = new VideoProject(file);
                            break;
                        }
                }
                return project;
            }
            catch (Exception ex)
            {
                Log.Logger.Error($"Error while processing URL: {ex.Message}\n{ex.StackTrace}");
            }
            return project;
        }
    }
}
