#region "copyright"

/*
    Copyright © 2023 Christian Palm (christian@palm-family.de)
    This Source Code Form is subject to the terms of the Mozilla Public
    License, v. 2.0. If a copy of the MPL was not distributed with this
    file, You can obtain one at http://mozilla.org/MPL/2.0/.
*/

#endregion "copyright"

using System.IO;
using Video2Sheet.Core.Keyboard;
using Video2Sheet.Core.Video.Processing;

namespace Video2Sheet.Core.Video
{
    public class VideoProject
    {
        public VideoFile VideoFile { get; set; }
        public ProcessingConfig ProcessingConfig { get; set; }

        public PianoConfiguration Piano { get; set; } = PianoConfiguration.Key88;

        public VideoProject(VideoFile VideoFile, ProcessingConfig pconfig)
        { 
            this.VideoFile = VideoFile;
            ProcessingConfig = pconfig;
        }

        public string GetFileName()
        {
            return Utility.ReplaceInvalidChars(VideoFile.Title) + ".v2s";
        }

        public string GetFolder()
        {
            return Path.Combine(AppConstants.DATA_DIR, GetFileName().Replace(".v2s", ""));
        }
    }
}
