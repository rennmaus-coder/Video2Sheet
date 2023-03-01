#region "copyright"

/*
    Copyright © 2023 Christian Palm (christian@palm-family.de)
    This Source Code Form is subject to the terms of the Mozilla Public
    License, v. 2.0. If a copy of the MPL was not distributed with this
    file, You can obtain one at http://mozilla.org/MPL/2.0/.
*/

#endregion "copyright"

using Microsoft.Win32;
using System.Collections.Generic;
using System.IO;

namespace Video2Sheet.Core
{
    public static class Utility
    {
        public static string ReplaceInvalidChars(string filename)
        {
            return string.Join("_", filename.Split(Path.GetInvalidFileNameChars()));
        }

        public static List<string> FileDialog(string filter, string title)
        {
            OpenFileDialog dialog = new OpenFileDialog();
            dialog.Filter = filter;
            dialog.Title = title;
            dialog.Multiselect = false;
            if ((bool)dialog.ShowDialog())
            {
                return dialog.FileNames.ToList();
            }
            return null;
        }

        public static List<string> ToList(this string[] str)
        {
            List<string> list = new List<string>();
            list.AddRange(str);
            return list;
        }
    }
}
