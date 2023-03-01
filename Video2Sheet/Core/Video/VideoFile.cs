﻿#region "copyright"

/*
    Copyright © 2023 Christian Palm (christian@palm-family.de)
    This Source Code Form is subject to the terms of the Mozilla Public
    License, v. 2.0. If a copy of the MPL was not distributed with this
    file, You can obtain one at http://mozilla.org/MPL/2.0/.
*/

#endregion "copyright"

using Newtonsoft.Json;

namespace Video2Sheet.Core.Video
{
    public class VideoFile
    {
        [JsonIgnore]
        public byte[] VideoData { get; set; }

        public string Title { get; set; }

        public VideoFile(byte[] VideoData, string title)
        { 
            this.VideoData = VideoData;
            this.Title = title;
        }
    }
}