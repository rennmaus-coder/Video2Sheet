#region "copyright"

/*
    Copyright © 2023 Christian Palm (christian@palm-family.de)
    This Source Code Form is subject to the terms of the Mozilla Public
    License, v. 2.0. If a copy of the MPL was not distributed with this
    file, You can obtain one at http://mozilla.org/MPL/2.0/.
*/

#endregion "copyright"

using MemoryPack;
using VideoLibrary;

namespace Video2Sheet.Core.Video
{
    [MemoryPackable]
    public partial class VideoFile
    {
        public byte[] VideoData { get; set; }

        [MemoryPackAllowSerialize]
        public YouTubeVideo VideoInfo { get; set; }

        public VideoFile(byte[] VideoData, YouTubeVideo VideoInfo)
        { 
            this.VideoData = VideoData;
            this.VideoInfo = VideoInfo;
        }
    }
}
