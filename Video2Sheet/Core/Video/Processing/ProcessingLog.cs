#region "copyright"

/*
    Copyright © 2023 Christian Palm (christian@palm-family.de)
    This Source Code Form is subject to the terms of the Mozilla Public
    License, v. 2.0. If a copy of the MPL was not distributed with this
    file, You can obtain one at http://mozilla.org/MPL/2.0/.
*/

#endregion "copyright"

using System.Collections.Generic;

namespace Video2Sheet.Core.Video.Processing
{
    public class ProcessingLog
    {
        public List<KeyEvent> Events { get; set; } = new List<KeyEvent>();
    }

    public class KeyEvent
    {
        public int Key;
        public int FrameNr;
        public int Offset;
        public long DeltaTime;
        public long AbsoluteTime;

        public KeyEvent(int key, int frameNr, int offset, long deltaTime, long absoluteTime)
        {
            Key = key;
            FrameNr = frameNr;
            Offset = offset;
            DeltaTime = deltaTime;
            AbsoluteTime = absoluteTime;
        }
    }
}
