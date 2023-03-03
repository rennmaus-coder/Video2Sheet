#region "copyright"

/*
    Copyright © 2023 Christian Palm (christian@palm-family.de)
    This Source Code Form is subject to the terms of the Mozilla Public
    License, v. 2.0. If a copy of the MPL was not distributed with this
    file, You can obtain one at http://mozilla.org/MPL/2.0/.
*/

#endregion "copyright"

namespace Video2Sheet.Core.Keyboard
{
    public struct PianoConfiguration
    {
        public int WhiteKeys;
        public int BlackKeys;
        public PianoType Type;

        public static PianoConfiguration Key32 = new() { WhiteKeys = 19, BlackKeys = 13, Type = PianoType.Key32 };
        public static PianoConfiguration Key36 = new() { WhiteKeys = 21, BlackKeys = 15, Type = PianoType.Key36 };
        public static PianoConfiguration Key49 = new() { WhiteKeys = 29, BlackKeys = 20, Type = PianoType.Key49 };
        public static PianoConfiguration Key54 = new() { WhiteKeys = 32, BlackKeys = 22, Type = PianoType.Key54 };
        public static PianoConfiguration Key61 = new() { WhiteKeys = 36, BlackKeys = 25, Type = PianoType.Key61 };
        public static PianoConfiguration Key76 = new() { WhiteKeys = 45, BlackKeys = 31, Type = PianoType.Key76 };
        public static PianoConfiguration Key88 = new() { WhiteKeys = 52, BlackKeys = 36, Type = PianoType.Key88 };
        public static PianoConfiguration Key91 = new() { WhiteKeys = 54, BlackKeys = 38, Type = PianoType.Key91 };
        public static PianoConfiguration Key97 = new() { WhiteKeys = 57, BlackKeys = 40, Type = PianoType.Key97 };
        public static PianoConfiguration Key108 = new() { WhiteKeys = 63, BlackKeys = 45, Type = PianoType.Key108 };

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
                case PianoType.Key91:
                    return Key91;
                case PianoType.Key97:
                    return Key97;
                case PianoType.Key108:
                    return Key108;
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
        Key88,
        Key91,
        Key97,
        Key108
}
}