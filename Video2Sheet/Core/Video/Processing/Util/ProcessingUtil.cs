#region "copyright"

/*
    Copyright © 2023 Christian Palm (christian@palm-family.de)
    This Source Code Form is subject to the terms of the Mozilla Public
    License, v. 2.0. If a copy of the MPL was not distributed with this
    file, You can obtain one at http://mozilla.org/MPL/2.0/.
*/

#endregion "copyright"

using OpenCvSharp;
using System;
using System.Collections.Generic;
using System.Linq;

namespace Video2Sheet.Core.Video.Processing.Util
{
    public static class ProcessingUtil
    {
        public static int GetEndOfNote(Mat frame, int x, int y, int threshold)
        {
            for (int resy = y; y < frame.Height - 1; resy++)
            {
                if (frame.At<Byte>(resy, x) - 17 < threshold)
                {
                    return resy;
                }
            }
            return y;
        }

        public static int NormalizeDeltaTime(long dt, NoteValues notes)
        {
            if (dt == 0)
                return 0;

            float norm = dt;
            if (dt > notes.whole)
                norm = norm % notes.whole;

            double multiplicator = Math.Floor(dt / notes.whole);

            Dictionary<float, float> table = new Dictionary<float, float>();
            table[notes.whole] = notes.whole - norm;
            table[notes.half] = notes.half - norm;
            table[notes.quater] = notes.quater - norm;
            table[notes._8] = notes._8 - norm;
            table[notes._16] = notes._16 - norm;
            table[notes._32] = notes._32 - norm;
            table[notes._64] = notes._64 - norm;
            table[notes._128] = notes._128 - norm;
            table[notes._256] = notes._256 - norm;
            table[notes._512] = notes._512 - norm;
            table[notes._1024] = notes._1024 - norm;
            table[notes._halfc] = notes._halfc - norm;
            table[notes._quaterc] = notes._quaterc - norm;
            table[notes._8c] = notes._8c - norm;
            table[notes._16c] = notes._16c - norm;
            table[notes._32c] = notes._32c - norm;
            table[notes._64c] = notes._64c - norm;
            table[notes._128c] = notes._128c - norm;
            table[notes._256c] = notes._256c - norm;
            table[notes._512c] = notes._512c - norm;

            foreach (var key in table.Keys)
            {
                table[key] = Math.Abs(table[key]);
            }

            float res = (float)table.FirstOrDefault(x => x.Value == table.Values.Min()).Key;

            return (int)(Math.Round(norm / notes._1024) * notes._1024 + notes.whole * multiplicator);
        }
    }
}
