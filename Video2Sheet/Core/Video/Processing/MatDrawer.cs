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

namespace Video2Sheet.Core.Video.Processing
{
    public static class MatDrawer
    {
        public static Mat DrawLine(Mat frame, int height)
        {
            Mat res = new Mat();
            frame.CopyTo(res);
            res.Line(0, height, res.Width, height, Scalar.Black);
            return res;
        }
        public static Mat DrawPointsToMat(Mat frame, DataPoints points)
        {
            Mat res = new Mat();
            frame.CopyTo(res);
            int size = Math.Clamp(res.Width / 150 - 5, 5, 200);
            foreach (var point in points.ExtractionPoints)
            {
                res.DrawMarker(new Point(point.X, point.Y), Scalar.White, MarkerTypes.Square, size, 2);
            }
            return res;
        }

        public static Mat DrawPointsToMat(Mat frame, DataPoints points, List<Scalar> colors)
        {
            Mat res = new Mat();
            frame.CopyTo(res);
            int size = Math.Clamp(res.Width / 150 - 5, 5, 200);
            foreach (var point in points.ExtractionPoints)
            {
                res.DrawMarker(new Point(point.X, point.Y), colors[points.ExtractionPoints.IndexOf(point)], MarkerTypes.Square, size, 2);
            }
            return res;
        }
    }
}
