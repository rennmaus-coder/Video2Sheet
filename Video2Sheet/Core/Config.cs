#region "copyright"

/*
    Copyright © 2023 Christian Palm (christian@palm-family.de)
    This Source Code Form is subject to the terms of the Mozilla Public
    License, v. 2.0. If a copy of the MPL was not distributed with this
    file, You can obtain one at http://mozilla.org/MPL/2.0/.
*/

#endregion "copyright"

using Newtonsoft.Json;
using Serilog.Events;
using System;
using System.Collections.Generic;
using System.IO;

namespace Video2Sheet.Core
{
    public static class Config
    {
        public static LogEventLevel LogLevel { get; set; } = LogEventLevel.Information;
        public static int VideoResolution { get; set; } = 720;
        public static int MarkerStep { get; set; } = 5;

        static Config()
        {
            if (File.Exists(Path.Combine(AppConstants.DATA_DIR, "Config.json"))) 
            {
                Dictionary<string, object> config = JsonConvert.DeserializeObject<Dictionary<string, object>>(File.ReadAllText(Path.Combine(AppConstants.DATA_DIR, "Config.json")));

                foreach (string key in config.Keys)
                {
                    if (key.Equals(nameof(LogLevel)))
                    {
                        LogLevel = (LogEventLevel)Enum.GetValues(typeof(LogEventLevel)).GetValue(int.Parse(config[key].ToString()));
                    }
                    else if (key.Equals(nameof(VideoResolution)))
                    {
                        VideoResolution = int.Parse(config[key].ToString());
                    }
                    else if (key.Equals(nameof(MarkerStep)))
                    {
                        MarkerStep = int.Parse(config[key].ToString());
                    }
                }
               
            }
        }

        public static void Save()
        {
            Dictionary<string, object> config = new Dictionary<string, object>()
            {
                { nameof(LogLevel), LogLevel },
                { nameof(VideoResolution), VideoResolution },
                { nameof(MarkerStep), MarkerStep }
            };

            File.WriteAllText(Path.Combine(AppConstants.DATA_DIR, "Config.json"), JsonConvert.SerializeObject(config));
        }
    }
}
