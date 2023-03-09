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
using Melanchall.DryWetMidi.Interaction;
using OpenCvSharp;
using Serilog;
using System;
using System.Collections.Generic;
using System.Numerics;
using Video2Sheet.Core.Keyboard;

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
            int[] previous_lum = new int[project.ProcessingConfig.ExtractionPoints.ExtractionPoints.Count];
            Key[] keys = new Key[project.ProcessingConfig.ExtractionPoints.ExtractionPoints.Count];
            for (int i = 0; i < keys.Length; i++)
            {
                keys[i] = new Key();
            }

            int possible_failures = 0;

            Mat frame = new Mat();
            MidiFile midi = new MidiFile();
            midi.TimeDivision = new TicksPerQuarterNoteTimeDivision(256);
            TrackChunk track = new TrackChunk();

            int movement = CalculateMovement();

            if (project.ProcessingConfig.BPM > 0)
            {
                track.Events.Add(new SetTempoEvent((long)(project.ProcessingConfig.BPM / 60 * 1000 * 1000)));
            }

            video.SetFrame(0);

            for (int frame_nr = 0; frame_nr < video.TotalFrames; frame_nr++)
            {
                frame = video.GetNextFrame();
                frame.Resize(new Size(Config.VideoResolution, Config.VideoResolution * (frame.Width / frame.Height)));

                frame = frame.CvtColor(ColorConversionCodes.BGR2GRAY);

                if (previous_lum.Length == 0)
                {
                    for (int i = 0; i < project.ProcessingConfig.ExtractionPoints.ExtractionPoints.Count - 1; i++)
                    {
                        Vector2 vec = project.ProcessingConfig.ExtractionPoints[i];
                        int lum = frame.At<Byte>((int)vec.Y, (int)vec.X);

                        previous_lum.SetValue(lum, i);
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
                            if (keys[i].IsPressed)
                            {
                                Log.Logger.Warning($"Detected NoteOn Event at index {i} at frame {frame_nr} without detected NoteOff Event");
                                possible_failures++;
                                previous_lum[i] = lum;
                                continue;
                            }
                            if (PianoConfiguration.PianoDict[project.Piano.Type][i] == 'b')
                            {
                                if (keys[i - 1].IsPressed)
                                {
                                    previous_lum[i] = lum;
                                    continue;
                                }
                            }
                            Log.Logger.Debug($"Detected NoteOn Event at index {i} at frame {frame_nr}");
                            track.Events.Add(new NoteOnEvent((SevenBitNumber)i, new SevenBitNumber(50))); // uknown velocity
                            keys[i].IsPressed = true;
                            keys[i].TurnedOnFrame = frame_nr;
                            keys[i].Offset = GetEndOfNote(frame, (int)vec.X, (int)vec.Y) - (int)vec.Y;
                        }
                        else if (previous_lum[i] - lum > project.ProcessingConfig.NoteThreshold) // Note off
                        {
                            if (!keys[i].IsPressed)
                            {
                                Log.Logger.Warning($"Detected NoteOff Event at index {i} at frame {frame_nr} without detected NoteOn Event");
                                possible_failures++;
                                previous_lum[i] = lum;
                                continue;
                            }
                            Log.Logger.Debug($"Detected NoteOff Event at index {i} at frame {frame_nr}");
                            MidiEvent note = new NoteOffEvent((SevenBitNumber)i, new SevenBitNumber(0)) { DeltaTime = (long)(256.0f * (project.ProcessingConfig.BPM / 60.0f / project.VideoFile.FPS) * ((frame_nr - keys[i].TurnedOnFrame) * keys[i].Offset)) };
                            track.Events.Add(note);
                            keys[i].IsPressed = false;
                        }
                        previous_lum[i] = lum;
                    }
                }

                List<Scalar> colors = new List<Scalar>();
                foreach (Key k in keys)
                {
                    colors.Add(k.IsPressed ? Scalar.White : Scalar.Black);
                }
                frame = MatDrawer.DrawPointsToMat(frame, project.ProcessingConfig.ExtractionPoints, colors);
                yield return new ProcessingCallback() { CurrentFrame = frame, FrameNr = frame_nr, Failures = possible_failures };
            }

            for (int i = 0; i < keys.Length; i++)
            {
                if (keys[i].IsPressed)
                {
                    possible_failures++;
                    track.Events.Add(new NoteOffEvent((SevenBitNumber)i, new SevenBitNumber(0)) { DeltaTime = (long)((256.0f * (project.ProcessingConfig.BPM / 60.0f)) / project.VideoFile.FPS) * (project.VideoFile.TotalFrames - keys[i].TurnedOnFrame) });
                    keys[i].IsPressed = false;
                }
            }
            midi.Chunks.Add(track);
            midi.Write("C:/Users/Christian/Desktop/Test.midi", true);
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

        private int GetEndOfNote(Mat frame, int x, int y)
        {
            for (int resy = y; y < frame.Height - 1; resy++)
            {
                if (frame.At<Byte>(resy, x) - 17 < project.ProcessingConfig.NoteThreshold)
                {
                    return resy;
                }
            }
            return y;
        }
    }
}
