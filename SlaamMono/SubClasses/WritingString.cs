﻿using Microsoft.Xna.Framework;

namespace SlaamMono.SubClasses
{
    public struct WritingString
    {
        public string Str;
        public Vector2 Pos;

        public WritingString(string str, Vector2 pos)
        {
            Str = str;
            Pos = pos;
            Pos.Y += 3;
        }
    }

}
