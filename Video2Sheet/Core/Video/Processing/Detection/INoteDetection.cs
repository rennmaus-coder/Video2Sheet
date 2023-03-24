#region "copyright"

/*
    Copyright © 2023 Christian Palm (christian@palm-family.de)
    This Source Code Form is subject to the terms of the Mozilla Public
    License, v. 2.0. If a copy of the MPL was not distributed with this
    file, You can obtain one at http://mozilla.org/MPL/2.0/.
*/

#endregion "copyright"

using NAudio.Midi;
using OpenCvSharp;
using System.Collections.Generic;
using Video2Sheet.Core.Keyboard;

namespace Video2Sheet.Core.Video.Processing.Detection
{
    public interface INoteDetection
    {
        public int PossibleFailures { get; set; }
        public bool UpdateKeys(Mat frame, ProcessingConfig config, ref Key[] keys, ref MidiEventCollection eventCollection, ref ProcessingLog log, int frame_nr, long midiTime);
    }
}
