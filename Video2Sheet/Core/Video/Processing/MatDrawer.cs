#region "copyright"

/*
    Copyright © 2023 Christian Palm (christian@palm-family.de)
    This Source Code Form is subject to the terms of the Mozilla Public
    License, v. 2.0. If a copy of the MPL was not distributed with this
    file, You can obtain one at http://mozilla.org/MPL/2.0/.
*/

#endregion "copyright"

using OpenCvSharp;

namespace Video2Sheet.Core.Video.Processing
{
    public static class MatDrawer
    {
        public static Mat DrawPointsToMat(Mat frame, DataPoints points)
        {
            Mat res = new Mat();
            frame.CopyTo(res);
            foreach (var point in points.ExtractionPoints)
            {
                res.DrawMarker(new Point(point.X, point.Y), Scalar.White, MarkerTypes.Square, res.Width / 150 - 5, 2);
            }
            return res;
        }
    }
}
