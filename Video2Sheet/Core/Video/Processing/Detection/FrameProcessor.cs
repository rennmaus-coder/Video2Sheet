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
    /// <summary>
    /// Processes frames of a video
    /// Create one instance for each video
    /// </summary>
    public class FrameProcessor
    {
        private bool hasMidiStarted = false;
        private long absoluteMidiTime = 0;
        private ProcessingConfig config;
        private float ticksPerFrame;
        private int keysOffset;
        private INoteDetection detect;

        public Key[] keys;

        public FrameProcessor(NoteValues noteValues, ProcessingConfig config, float ticks, float movement, float ticksPerFrame, PianoConfiguration piano)
        {
            this.config = config;

            keysOffset = 109 - (piano.WhiteKeys + piano.BlackKeys);

            keys = new Key[config.ExtractionPoints.ExtractionPoints.Count];

            for (int i = 0; i < keys.Length; i++)
            {
                keys[i] = new Key();
            }

            detect = new BasicDetector(ticksPerFrame, movement, keysOffset, noteValues);
            // detect = new WidthDetector(24, 12, piano, 20, 10, 0, movement, ticksPerFrame, keysOffset, noteValues);
            this.ticksPerFrame = ticksPerFrame;
        }

        public ProcessingCallback ProcessFrame(ref Mat frame, ref ProcessingLog log, ref MidiEventCollection eventCollection, int frame_nr)
        {
            if (frame_nr == 0)
            {
                for (int i = 0; i < config.ExtractionPoints.ExtractionPoints.Count - 1; i++)
                {
                    Vector2 vec = config.ExtractionPoints[i];
                    int lum = frame.At<Byte>((int)vec.Y, (int)vec.X);

                    keys[i].PreviousLum = lum;
                }
            }
            else
            {

                // absoluteMidiTime = ProcessingUtil.NormalizeDeltaTime(absoluteMidiTime, NoteValues);

                /* for (int i = 0; i < config.ExtractionPoints.ExtractionPoints.Count - 1; i++)
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
                        Log.Logger.Debug($"Detected NoteOn Event at index {i} at frame {frame_nr}");

                        keys[i].IsPressed = true;
                        keys[i].TurnedOnFrame = frame_nr;
                        keys[i].Offset = ProcessingUtil.GetEndOfNote(frame, (int)vec.X, (int)vec.Y, config.NoteThreshold) - (int)vec.Y;
                        // keys[i].StartTime = ProcessingUtil.NormalizeDeltaTime(absoluteMidiTime, NoteValues);
                        keys[i].StartTime = absoluteMidiTime;

                        hasMidiStarted = true;
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

                        int dt = (int)(ticksPerFrame * (frame_nr - keys[i].TurnedOnFrame + ((float)movement / (float)keys[i].Offset)));
                        // dt = ProcessingUtil.NormalizeDeltaTime(dt, NoteValues);

                        if (dt < 0)
                        {
                            PossibleFailures++;
                            Log.Logger.Warning($"DeltaTime was {dt} at frame {frame_nr}, key: {i + keysOffset}");
                            continue;
                        }

                        NoteOnEvent on = new NoteOnEvent(keys[i].StartTime, 1, i + keysOffset, 50, dt);
                        NoteEvent e = new NoteEvent(keys[i].StartTime + dt, 1, MidiCommandCode.NoteOff, i + keysOffset, 0);

                        eventCollection.AddEvent(on, 1);
                        eventCollection.AddEvent(e, 1);

                        log.Events.Add(new KeyEvent(i, frame_nr, keys[i].Offset, dt, absoluteMidiTime));

                        keys[i].IsPressed = false;
                    }
                    keys[i].PreviousLum = lum;
                } */

                if (detect.UpdateKeys(frame, config, ref keys, ref eventCollection, ref log, frame_nr, absoluteMidiTime) && !hasMidiStarted)
                    hasMidiStarted = true;

                if (hasMidiStarted)
                    absoluteMidiTime += (long)ticksPerFrame;
            }

            return new ProcessingCallback() { FrameNr = frame_nr, Failures = detect.PossibleFailures };
        }

        public void FinalizeNotes(ref MidiEventCollection eventCollection)
        {
            for (int i = 0; i < keys.Length - 1; i++)
            {
                if (keys[i].IsPressed)
                {
                    detect.PossibleFailures++;

                    eventCollection.AddEvent(new NoteEvent(absoluteMidiTime, 1, MidiCommandCode.NoteOff, i + keysOffset, 0), 1);
                    keys[i].IsPressed = false;
                }
            }
        }
    }
}
