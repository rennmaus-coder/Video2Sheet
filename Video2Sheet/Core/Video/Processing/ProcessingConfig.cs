#region "copyright"

/*
    Copyright © 2023 Christian Palm (christian@palm-family.de)
    This Source Code Form is subject to the terms of the Mozilla Public
    License, v. 2.0. If a copy of the MPL was not distributed with this
    file, You can obtain one at http://mozilla.org/MPL/2.0/.
*/

#endregion "copyright"

using System.Numerics;

namespace Video2Sheet.Core.Video.Processing
{
    public class ProcessingConfig
    {
        public DataPoints ExtractionPoints { get; set; }

        public int NoteThreshold { get; set; } = 80; // Change in luminance to detect a note on / off event

        public ProcessingConfig()
        {
            ExtractionPoints = new DataPoints();
        }

        public void GenerateExtractionPoints(int resolution)
        {
            ExtractionPoints.ExtractionPoints.Clear();
            int offset = (resolution / 52) / 2;
            int step = resolution / 52;

            for (int i = 0; i < 52; i++) // amount of white keys on a keyboard / piano
            {
                ExtractionPoints.ExtractionPoints.Add(new Vector2(i * step + offset, 15));
            }
        }
    }
}
