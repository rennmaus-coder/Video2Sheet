#region "copyright"

/*
    Copyright © 2023 Christian Palm (christian@palm-family.de)
    This Source Code Form is subject to the terms of the Mozilla Public
    License, v. 2.0. If a copy of the MPL was not distributed with this
    file, You can obtain one at http://mozilla.org/MPL/2.0/.
*/

#endregion "copyright"

using NAudio.Midi;
using Newtonsoft.Json;
using OpenCvSharp;
using Serilog;
using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Numerics;
using Video2Sheet.Core.Keyboard;
using Video2Sheet.Core.Video.Processing.Detection;
using Video2Sheet.Core.Video.Processing.Util;

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
            float ticks = 1024f;

            NoteValues notes = new NoteValues();
            notes.Init(ticks); // Initialize note values (setting individual tick lengths)

            VideoFile video = project.VideoFile;

            Mat frame;
            MidiEventCollection eventCollection = new MidiEventCollection(0, (int)ticks);

            if (project.ProcessingConfig.BPM > 0) // Write Tempo to Midi if set
            {
                eventCollection.AddEvent(new TempoEvent((int)(project.ProcessingConfig.BPM / 60 * 1000 * 1000), 0), 1);
            }

            ProcessingLog log = new ProcessingLog();

            video.SetFrame(0);

            int movement = CalculateMovement();
            float ticksPerFrame = (float)(ticks * project.ProcessingConfig.BPM / (60.0f * project.VideoFile.FPS)); // ticks per frame

            video.SetFrame(0);

            FrameProcessor processor = new FrameProcessor(notes, project.ProcessingConfig, ticks, movement, ticksPerFrame, project.Piano, video.GetCurrentFrame().Width);


            for (int frame_nr = 0; frame_nr < video.TotalFrames; frame_nr++)
            {
                frame = video.GetNextFrame();

                ProcessingCallback callback = processor.ProcessFrame(ref frame, ref log, ref eventCollection, frame_nr);

                List<Scalar> colors = new List<Scalar>();
                foreach (Key k in processor.keys)
                {
                    colors.Add(k.IsPressed ? Scalar.White : Scalar.Black);
                }

                frame = MatDrawer.DrawPointsToMat(frame, project.ProcessingConfig.ExtractionPoints, colors);
                callback.CurrentFrame = frame;

                yield return callback;
            }

            processor.FinalizeNotes(ref eventCollection);
            eventCollection.PrepareForExport();
            MidiFile.Export("C:/Users/Christian/Desktop/Test.midi", eventCollection);

            File.WriteAllText(Path.Combine(AppConstants.DATA_DIR, "Processinglog.json"), JsonConvert.SerializeObject(log, Formatting.Indented));
        }

        private int CalculateMovement()
        {
            int endY1 = 0;
            int endX = 0;

            Mat frame = project.VideoFile.GetNextFrame();

            int lum = frame.At<Byte>(0, 0);

            for (int i = 0; i < project.VideoFile.TotalFrames; i++)
            {
                for (int x = 0; x < frame.Width - 1; x++)
                {
                    if (frame.At<Byte>(5, x) - lum > project.ProcessingConfig.NoteThreshold)
                    {
                        for (int y = 5; y < frame.Height - 1; y++)
                        {
                            if (frame.At<Byte>(5, x) - lum < project.ProcessingConfig.NoteThreshold)
                            {
                                endY1 = y;
                                endX = x;
                                break;
                            }
                        }
                    }
                    if (endY1 > 0)
                        break;
                }
                if (endY1 > 0)
                    break;
            }

            frame = project.VideoFile.GetNextFrame();
            int endY2 = 0;
                
            for (int y = 5; y < frame.Height - 1; y++)
            {
                if (frame.At<Byte>(y, endX) - lum < project.ProcessingConfig.NoteThreshold)
                {
                    endY2 = y;
                    break;
                }
            }

            return endY2 - endY1;
        }
    }
}
