#region "copyright"

/*
    Copyright © 2023 Christian Palm (christian@palm-family.de)
    This Source Code Form is subject to the terms of the Mozilla Public
    License, v. 2.0. If a copy of the MPL was not distributed with this
    file, You can obtain one at http://mozilla.org/MPL/2.0/.
*/

#endregion "copyright"

using Serilog;
using System.Collections.Generic;
using System.Numerics;

namespace Video2Sheet.Core.Video.Processing
{
    public struct DataPoints
    {
        public List<Vector2> ExtractionPoints { get; set; } = new List<Vector2>();

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

        public Vector2 this[int key]
        {
            get => ExtractionPoints[key];
            set => ExtractionPoints[key] = value;
        }
    }
}
