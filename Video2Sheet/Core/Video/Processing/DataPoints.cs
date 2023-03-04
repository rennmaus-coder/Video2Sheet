#region "copyright"

/*
    Copyright © 2023 Christian Palm (christian@palm-family.de)
    This Source Code Form is subject to the terms of the Mozilla Public
    License, v. 2.0. If a copy of the MPL was not distributed with this
    file, You can obtain one at http://mozilla.org/MPL/2.0/.
*/

#endregion "copyright"

using Serilog;
using System;
using System.Collections.Generic;
using System.Numerics;
using Video2Sheet.Core.Keyboard;
using Video2Sheet.MVVM;
using Video2Sheet.MVVM.ViewModel;

namespace Video2Sheet.Core.Video.Processing
{
    public class DataPoints : ObservableObject
    {
        private List<Vector2> points = new List<Vector2>();
        public List<Vector2> ExtractionPoints
        {
            get => points;
            set
            {
                points = value;
                RaisePropertyChanged();
            }
        }

        private float _offset = 1;
        public float Offset
        {
            get => _offset;
            set
            {
                _offset = value;
                Generate(MainWindowVM.Instance.HomeVM?.LoadedProject?.Piano ?? PianoConfiguration.Key88, MainWindowVM.Instance.HomeVM?.LoadedProject?.VideoFile?.GetCurrentFrame().Width ?? Config.VideoResolution);
                RaisePropertyChanged();
            }
        }

        public DataPoints()
        {

        }

        public void Move(int amount)
        {
            Log.Logger.Debug($"Moving ExtractionPoints by {amount}");
            for (int i = 0; i < ExtractionPoints.Count; i++)
            {
                Vector2 vec = ExtractionPoints[i];
                vec.X += amount;
                ExtractionPoints[i] = vec;
            }
        }

        public void MoveUp(int amount)
        {
            Log.Logger.Debug($"Moving ExtractionPoints up by {amount}");
            for (int i = 0; i < ExtractionPoints.Count; i++)
            {
                Vector2 vec = ExtractionPoints[i];
                vec.Y += amount;
                ExtractionPoints[i] = vec;
            }
        }

        public void Generate(PianoConfiguration piano, int frame_width)
        {
            List<Vector2> temp = new List<Vector2>();
            int woffset = (frame_width / piano.WhiteKeys) / 2;
            int step = frame_width / piano.WhiteKeys;

            int counter = 0;

            foreach (char key in PianoConfiguration.PianoDict[piano.Type]) // amount of white keys on a keyboard / piano
            {
                if (key.Equals('w'))
                {
                    temp.Add(new Vector2((counter * step + woffset) * Offset, 15));
                    counter++;
                }
                else
                {
                    temp.Add(new Vector2((counter * step) * Offset, 10));
                }
            }
            ExtractionPoints = temp;
        }

        public Vector2 this[int key]
        {
            get => ExtractionPoints[key];
            set => ExtractionPoints[key] = value;
        }
    }
}
