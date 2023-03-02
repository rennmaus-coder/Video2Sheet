#region "copyright"

/*
    Copyright © 2023 Christian Palm (christian@palm-family.de)
    This Source Code Form is subject to the terms of the Mozilla Public
    License, v. 2.0. If a copy of the MPL was not distributed with this
    file, You can obtain one at http://mozilla.org/MPL/2.0/.
*/

#endregion "copyright"

using Melanchall.DryWetMidi.Common;
using Melanchall.DryWetMidi.Core;
using OpenCvSharp;
using OpenCvSharp.Extensions;
using System;
using System.Collections.Generic;
using System.Drawing;
using System.Numerics;

namespace Video2Sheet.Core.Video.Processing
{
    public class VideoProcessor
    {
        private VideoProject project;
        public VideoProcessor(VideoProject project) 
        { 
            this.project = project;
        }

        public IEnumerable<ProcessingCallback> ProcessVideo() 
        {
            VideoFile video = project.VideoFile;
            List<int> previous_lum = new List<int>();
            MidiFile midi = new MidiFile();
            Mat frame = new Mat();
            TrackChunk track = new TrackChunk();

            for (int frame_nr = 0; frame_nr < video.TotalFrames; frame_nr++)
            {
                video.SetFrame(frame_nr);
                frame = video.GetNextFrame();
                frame.Resize(new OpenCvSharp.Size(Config.VideoResolution, Config.VideoResolution * (frame.Width / frame.Height)));

                frame = frame.CvtColor(ColorConversionCodes.BGR2GRAY);

                if (previous_lum.Count == 0)
                {
                    for (int i = 0; i < project.ProcessingConfig.ExtractionPoints.ExtractionPoints.Count - 1; i++)
                    {
                        Vector2 vec = project.ProcessingConfig.ExtractionPoints[i];
                        int lum = frame.At<Byte>((int)vec.Y, (int)vec.X);

                        previous_lum.Add(lum);
                    }
                }
                else
                {
                    for (int i = 0; i < project.ProcessingConfig.ExtractionPoints.ExtractionPoints.Count - 1; i++)
                    {
                        Vector2 vec = project.ProcessingConfig.ExtractionPoints[i];
                        int lum = frame.At<Byte>((int)vec.Y, (int)vec.X);

                        if (lum - previous_lum[i] > project.ProcessingConfig.NoteThreshold) // Note on
                        {
                            track.Events.Add(new NoteOnEvent((SevenBitNumber)i, new SevenBitNumber(50))); // uknown velocity
                        }
                        else if (previous_lum[i] - lum > project.ProcessingConfig.NoteThreshold) // Note off
                        {
                            track.Events.Add(new NoteOffEvent((SevenBitNumber)i, new SevenBitNumber(50)));
                        }
                        previous_lum[i] = lum;
                    }
                }
                yield return new ProcessingCallback() { CurrentFrame = frame, FrameNr = frame_nr };
            }

            midi.Chunks.Add(track);
            midi.Write("C:/Users/Christian/Desktop/Test.midi");
        }
    }
}
