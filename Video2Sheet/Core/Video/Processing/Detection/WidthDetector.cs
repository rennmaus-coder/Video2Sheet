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
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows.Controls;
using Video2Sheet.Core.Keyboard;
using Video2Sheet.Core.Video.Processing.Util;

namespace Video2Sheet.Core.Video.Processing.Detection
{
    public class WidthDetector : INoteDetection
    {
        public int PossibleFailures { get; set; }

        private int WhiteWidth;
        private int BlackWidth;
        private PianoConfiguration piano;

        private float movement;
        private float ticksPerFrame;
        private int keysOffset;
        private NoteValues notes;

        public int DetectionY;
        public int ExtractionY;

        private int FrameWidth;

        private int[] PreviousLum;

        bool[] isPressed;

        public WidthDetector(int whiteWidth, int blackWidth, PianoConfiguration piano, int detectionY, int extractionY, int frameWidth, float movement, float ticksPerFrame, int keysOffset, NoteValues notes)
        {
            WhiteWidth = whiteWidth;
            BlackWidth = blackWidth;
            this.piano = piano;
            DetectionY = detectionY;
            ExtractionY = extractionY;
            FrameWidth = frameWidth;

            isPressed = new bool[frameWidth];
            this.movement = movement;
            this.ticksPerFrame = ticksPerFrame;
            this.keysOffset = keysOffset;
            this.notes = notes;
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

        public List<int> GetKeysByTransform(int x, int width)
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

            return keys;
        }
    }
}
