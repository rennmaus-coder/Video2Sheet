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
using System.Collections.Generic;
using Video2Sheet.Core.Keyboard;
using Video2Sheet.Core.Video.Processing.Util;

namespace Video2Sheet.Core.Video.Processing.Detection
{
    public class WidthDetector : INoteDetection
    {
        public int PossibleFailures { get; set; }

        public static int WhiteWidth;
        public static int BlackWidth;
        private PianoConfiguration piano;

        private float movement;
        private float ticksPerFrame;
        private int keysOffset;
        private NoteValues notes;

        public int DetectionY;

        private int FrameWidth;

        private int[] PreviousLum;

        bool[] isPressed;
        bool[] changes;

        public WidthDetector(int whiteWidth, int blackWidth, PianoConfiguration piano, int detectionY, int frameWidth, float movement, float ticksPerFrame, int keysOffset, NoteValues notes)
        {
            WhiteWidth = whiteWidth;
            BlackWidth = blackWidth;
            this.piano = piano;
            DetectionY = detectionY;
            FrameWidth = frameWidth;

            isPressed = new bool[frameWidth];
            this.movement = movement;
            this.ticksPerFrame = ticksPerFrame;
            this.keysOffset = keysOffset;
            this.notes = notes;

            PreviousLum = new int[frameWidth];
            changes = new bool[frameWidth];
        }

        public bool UpdateKeys(Mat frame, ProcessingConfig config, ref Key[] keys, ref MidiEventCollection eventCollection, ref ProcessingLog log, int frame_nr, long midiTime)
        {
            bool hadUpdate = false;
            if (frame_nr == 0)
            {
                for (int i = 0; i < frame.Width - 1; i++)
                {
                    PreviousLum[i] = frame.At<byte>(DetectionY, i);
                }
            }
            else
            {
                for (int i = 0; i < frame.Width - 1; i++)
                {
                    int lum = frame.At<byte>(DetectionY, i);

                    if (lum - PreviousLum[i] > config.NoteThreshold) // Note on
                    {
                        isPressed[i] = true;
                        hadUpdate = true;
                    }
                    else if (PreviousLum[i] - lum > config.NoteThreshold) // Note off
                    {
                        changes[i] = true;
                        hadUpdate = true;
                    }
                    PreviousLum[i] = lum;
                }

                for (int i = 0; i < isPressed.Length - 1; i++)  // Process note on Events
                {
                    int width = 0;
                    while (isPressed[i])
                    {
                        width++;
                        i++;
                    }

                    if (width > 0)
                    {
                        int x = i - width / 2;
                        List<int> indecies = GetKeysByTransform(x, width);
                        indecies.ForEach(x => x += keysOffset);
                        foreach (int k in indecies)
                        {
                            keys[k].TurnedOnFrame = frame_nr;
                            keys[k].Offset = ProcessingUtil.GetEndOfNote(frame, x, DetectionY, config.NoteThreshold) - DetectionY;
                            keys[k].StartTime = midiTime;
                            keys[k].IsPressed = true;
                        }
                    }
                }

                for (int i = 0; i < changes.Length - 1; i++) // Process note off Events
                {
                    int width = 0;
                    while (changes[i])
                    {
                        width++;
                        i++;
                    }

                    if (width > 0)
                    {
                        int x = i - width / 2;
                        List<int> indecies = GetKeysByTransform(x, width);
                        foreach (int z in indecies)
                        {
                            int k = z + keysOffset;
                            int dt = (int)(ticksPerFrame * ((frame_nr - keys[k].TurnedOnFrame) + (keys[k].Offset / movement)));
                            // dt = ProcessingUtil.NormalizeDeltaTime(dt, notes);

                            if (dt < 0)
                            {
                                PossibleFailures++;
                                Log.Logger.Warning($"DeltaTime was {dt} at frame {frame_nr}, key: {k}");
                                continue;
                            }

                            NoteOnEvent on = new NoteOnEvent((long)(keys[k].StartTime + (keys[k].Offset / movement)), 1, k, 50, dt);
                            NoteEvent e = new NoteEvent(keys[k].StartTime + dt, 1, MidiCommandCode.NoteOff, k, 0);

                            eventCollection.AddEvent(on, 1);
                            eventCollection.AddEvent(e, 1);

                            log.Events.Add(new KeyEvent(k, frame_nr, keys[k].Offset, dt, midiTime));

                            keys[k].IsPressed = false;

                        }
                    }
                }
            }
            /*else
            {
                for (int i = 0; i < frame.Width - 1; i++)
                {
                    int lum = frame.At<byte>(DetectionY, i);

                    if (lum - PreviousLum[i] > config.NoteThreshold) // Note on
                    {
                        isPressed[i] = true;
                    }
                    else if (lum - PreviousLum[i] < config.NoteThreshold)
                    {
                        isPressed[i] = false;
                        List<int> indecies = GetKeysByTransform(i, WhiteWidth);

                        for (int z = 0; i < indecies.Count() - 1; z++)
                        {
                            keys[indecies[z]].IsPressed = false;

                            int dt = (int)(ticksPerFrame * ((frame_nr - keys[i].TurnedOnFrame) + (movement / keys[i].Offset)));
                            dt = ProcessingUtil.NormalizeDeltaTime(dt, notes);

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

                            log.Events.Add(new KeyEvent(i, frame_nr, keys[i].Offset, dt, midiTime));

                            hadUpdate = true;
                        }
                    }

                    for (int _ = 0; i < isPressed.Count() - 1; _++)
                    {
                        int width = 0;
                        while (isPressed[_])
                        {
                            _++;
                            width++;
                        }

                        _ -= width;

                        if (width != 0)
                        {
                            List<int> indecies = GetKeysByTransform(_ + width / 2, width);

                            for (int z = 0; i < indecies.Count() - 1; z++)
                            {
                                keys[indecies[z]].IsPressed = true;
                                keys[indecies[z]].TurnedOnFrame = frame_nr;
                                keys[indecies[z]].StartTime = midiTime;

                                hadUpdate = true;
                            }
                        }
                    }
                }
            }*/

            return hadUpdate;
        }

        public static List<int> GetKeysByTransform(int x, int width)
        {
            List<int> keys = new List<int>();

            if (width >= WhiteWidth - 2 && width <= WhiteWidth + 2) // within tolerance of 2px
            {
                int index = 0;
                while (x > WhiteWidth)
                {
                    x -= WhiteWidth;
                    index++;
                }
                keys.Add(index);
            }
            else if (width >= BlackWidth - 2 && width <= BlackWidth + 2)
            {
                int index = 0;
                while (x > WhiteWidth)
                {
                    x -= WhiteWidth;
                    index++;
                }
                keys.Add(index);
            }

            return keys;
        }
    }
}
