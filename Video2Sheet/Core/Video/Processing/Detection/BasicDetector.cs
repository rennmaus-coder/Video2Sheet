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
using Serilog;
using System;
using System.Numerics;
using Video2Sheet.Core.Keyboard;
using Video2Sheet.Core.Video.Processing.Util;

namespace Video2Sheet.Core.Video.Processing.Detection
{
    public class BasicDetector : INoteDetection
    {
        public int PossibleFailures { get; set; }
        private float ticksPerFrame;
        private float movement;
        private int keysOffset;
        private NoteValues notes;

        public BasicDetector(float ticksPerFrame, float movement, int keysOffset, NoteValues notes)
        {
            this.ticksPerFrame = ticksPerFrame;
            this.movement = movement;
            this.keysOffset = keysOffset;
            this.notes = notes; 
        }


        public bool UpdateKeys(Mat frame, ProcessingConfig config, ref Key[] keys, ref MidiEventCollection eventCollection, ref ProcessingLog log, int frame_nr, long midiTime)
        {
            bool hadUpdate = false;
            for (int i = 0; i < config.ExtractionPoints.ExtractionPoints.Count - 1; i++)
            {
                Vector2 vec = config.ExtractionPoints[i];
                int lum = frame.At<Byte>((int)vec.Y, (int)vec.X);

                if (lum - keys[i].PreviousLum > config.NoteThreshold) // Note on
                {
                    if (keys[i].IsPressed)
                    {
                        Log.Logger.Warning($"Detected NoteOn Event at index {i} at frame {frame_nr} without detected NoteOff Event");
                        PossibleFailures++;
                        keys[i].PreviousLum = lum;
                        continue;
                    }
                    if (IsInCenter(vec, frame, config))
                    {
                        Log.Logger.Debug($"Detected NoteOn Event at index {i} at frame {frame_nr}");

                        keys[i].TurnedOnFrame = frame_nr;
                        keys[i].Offset = ProcessingUtil.GetEndOfNote(frame, (int)vec.X, (int)vec.Y, config.NoteThreshold) - (int)vec.Y;
                        keys[i].StartTime = midiTime;
                        keys[i].IsPressed = true;

                        hadUpdate = true;
                    }
                }
                else if (keys[i].PreviousLum - lum > config.NoteThreshold) // Note off
                {
                    if (!keys[i].IsPressed)
                    {
                        Log.Logger.Warning($"Detected NoteOff Event at index {i} at frame {frame_nr} without detected NoteOn Event");
                        PossibleFailures++;
                        keys[i].PreviousLum = lum;
                        continue;
                    }
                    Log.Logger.Debug($"Detected NoteOff Event at index {i} at frame {frame_nr}");

                    float dt = ticksPerFrame * ((frame_nr - keys[i].TurnedOnFrame) + (keys[i].Offset / movement));
                    // dt = ProcessingUtil.NormalizeDeltaTime((int)dt, notes);

                    if (dt < 0)
                    {
                        PossibleFailures++;
                        Log.Logger.Warning($"DeltaTime was {dt} at frame {frame_nr}, key: {i + keysOffset}");
                        continue;
                    }

                    NoteOnEvent on = new NoteOnEvent(keys[i].StartTime, 1, i + keysOffset, 50, (int)dt);
                    NoteEvent e = new NoteEvent((long)(keys[i].StartTime + dt), 1, MidiCommandCode.NoteOff, i + keysOffset, 0);

                    eventCollection.AddEvent(on, 1);
                    eventCollection.AddEvent(e, 1);

                    log.Events.Add(new KeyEvent(i, frame_nr, keys[i].Offset, (long)dt, midiTime));

                    keys[i].IsPressed = false;

                    hadUpdate = true;
                }
                keys[i].PreviousLum = lum;
            }
            return hadUpdate;
        }

        private bool IsInCenter(Vector2 vec, Mat frame, ProcessingConfig config)
        {
            int toLeft = 0;
            int toRight = 0;
            int inversetolerance = 6;
            int lum = frame.At<byte>((int)vec.Y, (int)vec.X);
            for (int i = 0; i < 100; i++)
            {
                if (lum - frame.At<byte>((int)vec.Y, (int)vec.X + i) > config.NoteThreshold) // Note off
                {
                    toRight = i;
                    break;
                }
            }
            for (int i = 0; i < 100; i++)
            {
                if (lum - frame.At<byte>((int)vec.Y, (int)vec.X - i) > config.NoteThreshold) // Note off
                {
                    toLeft = i;
                    break;
                }
            }

            if (toLeft < inversetolerance || toRight < inversetolerance)
            {
                return false;
            }
            return true;
        }
    }
}
