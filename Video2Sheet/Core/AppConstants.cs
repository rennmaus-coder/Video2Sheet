﻿#region "copyright"

/*
    Copyright © 2023 Christian Palm (christian@palm-family.de)
    This Source Code Form is subject to the terms of the Mozilla Public
    License, v. 2.0. If a copy of the MPL was not distributed with this
    file, You can obtain one at http://mozilla.org/MPL/2.0/.
*/

#endregion "copyright"

using System;
using System.IO;

namespace Video2Sheet.Core
{
    public static class AppConstants
    {
        public const string APP_NAME = "Video2Sheet";
        public static string DATA_DIR = Path.Combine(Environment.GetFolderPath(Environment.SpecialFolder.LocalApplicationData), APP_NAME);
    }
}
