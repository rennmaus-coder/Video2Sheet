#region "copyright"

/*
    Copyright © 2023 Christian Palm (christian@palm-family.de)
    This Source Code Form is subject to the terms of the Mozilla Public
    License, v. 2.0. If a copy of the MPL was not distributed with this
    file, You can obtain one at http://mozilla.org/MPL/2.0/.
*/

#endregion "copyright"

using System.Collections.Generic;

namespace Video2Sheet.Core.Keyboard
{
    public struct PianoConfiguration
    {
        public int WhiteKeys;
        public int BlackKeys;
        public PianoType Type;

        public readonly static PianoConfiguration Key32 = new() { WhiteKeys = 19, BlackKeys = 13, Type = PianoType.Key32 };
        public readonly static PianoConfiguration Key36 = new() { WhiteKeys = 21, BlackKeys = 15, Type = PianoType.Key36 };
        public readonly static PianoConfiguration Key49 = new() { WhiteKeys = 29, BlackKeys = 20, Type = PianoType.Key49 };
        public readonly static PianoConfiguration Key54 = new() { WhiteKeys = 32, BlackKeys = 22, Type = PianoType.Key54 };
        public readonly static PianoConfiguration Key61 = new() { WhiteKeys = 36, BlackKeys = 25, Type = PianoType.Key61 };
        public readonly static PianoConfiguration Key76 = new() { WhiteKeys = 45, BlackKeys = 31, Type = PianoType.Key76 };
        public readonly static PianoConfiguration Key88 = new() { WhiteKeys = 52, BlackKeys = 36, Type = PianoType.Key88 };

        public readonly static Dictionary<PianoType, string> PianoDict = new Dictionary<PianoType, string>()
        {
            { PianoType.Key32, "wbwbwbwwbwbwwbwbwbwwbwbwwbwbwbww" },
            { PianoType.Key36, "wbwbwbwwbwbwwbwbwbwwbwbwwbwbwbwwbwbw" },
            { PianoType.Key49, "wbwbwwbwbwbwwbwbwwbwbwbwwbwbwwbwbwbwwbwbwwbwbwbww" },
            { PianoType.Key54, "wbwbwwbwbwbwwbwbwwbwbwbwwbwbwwbwbwbwwbwbwwbwbwbwwbwbww" },
            { PianoType.Key61, "wbwbwwbwbwbwwbwbwwbwbwbwwbwbwwbwbwbwwbwbwwbwbwbwwbwbwwbwbwbww" },
            { PianoType.Key76, "wwbwbwbwwbwbwwbwbwbwwbwbwwbwbwbwwbwbwwbwbwbwwbwbwwbwbwbwwbwbwwbwbwbwwbwbwwbw" },
            { PianoType.Key88, "wbwwbwbwwbwbwbwwbwbwwbwbwbwwbwbwwbwbwbwwbwbwwbwbwbwwbwbwwbwbwbwwbwbwwbwbwbwwbwbwwbwbwbww" }
        };

        public static PianoConfiguration GetByType(PianoType type)
        {
            switch (type)
            {
                case PianoType.Key32:
                    return Key32;
                case PianoType.Key36:
                    return Key36;
                case PianoType.Key49:
                    return Key49;
                case PianoType.Key54:
                    return Key54;
                case PianoType.Key61:
                    return Key61;
                case PianoType.Key76:
                    return Key76;
                case PianoType.Key88:
                    return Key88;
                default:
                    break;
            }
            return new PianoConfiguration();
        }
    }

    public enum PianoType
    {
        Key32,
        Key36,
        Key49,
        Key54,
        Key61,
        Key76,
        Key88
}
}