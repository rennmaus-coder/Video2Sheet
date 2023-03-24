#region "copyright"

/*
    Copyright © 2023 Christian Palm (christian@palm-family.de)
    This Source Code Form is subject to the terms of the Mozilla Public
    License, v. 2.0. If a copy of the MPL was not distributed with this
    file, You can obtain one at http://mozilla.org/MPL/2.0/.
*/

#endregion "copyright"

namespace Video2Sheet.Core.Video.Processing.Util
{
    public struct NoteValues
    {
        public float whole;
        public float half;
        public float quater;
        public float _8;
        public float _16;
        public float _32;
        public float _64;
        public float _128;
        public float _256;
        public float _512;
        public float _1024;

        public float _halfc; // c stands for dot next to note
        public float _quaterc;
        public float _8c;
        public float _16c;
        public float _32c;
        public float _64c;
        public float _128c;
        public float _256c;
        public float _512c;

        public void Init(float ticks)
        {
            whole = ticks * 4;
            half = ticks * 2;
            quater = ticks;
            _8 = ticks / 2f;
            _16 = ticks / 4f;
            _32 = ticks / 8f;
            _64 = ticks / 10f;
            _128 = ticks / 10f;
            _256 = ticks / 12f;
            _512 = ticks / 14f;
            _1024 = ticks / 16f;

            _halfc = half + quater;
            _quaterc = quater + _8;
            _8c = _8 + _16;
            _16c = _16 + _32;
            _32c = _32 + _64;
            _64c = _64 + _128;
            _128c = _128 + _256;
            _256c = _256 + _512;
            _512c = _512 + _1024;
        }
    }
}
