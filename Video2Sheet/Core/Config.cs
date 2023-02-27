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
        public static LogEventLevel LogLevel { get; set; }

        static Config()
        {
            if (File.Exists(Path.Combine(AppConstants.DATA_DIR, "Config.json"))) 
            {
                Dictionary<string, object> config = JsonConvert.DeserializeObject<Dictionary<string, object>>(File.ReadAllText(Path.Combine(AppConstants.DATA_DIR, "Config.json")));
                LogLevel = (LogEventLevel)Enum.GetValues(typeof(LogEventLevel)).GetValue(int.Parse(config[nameof(LogLevel)].ToString()));
            }
        }

        public static void Save()
        {
            Dictionary<string, object> config = new Dictionary<string, object>()
            {
                { nameof(LogLevel), LogLevel }
            };

            File.WriteAllText(Path.Combine(AppConstants.DATA_DIR, "Config.json"), JsonConvert.SerializeObject(config));
        }
    }
}
