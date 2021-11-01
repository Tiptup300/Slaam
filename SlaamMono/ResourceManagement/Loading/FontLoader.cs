﻿using Microsoft.Xna.Framework.Graphics;
using SlaamMono.Library.ResourceManagement;

namespace SlaamMono.ResourceManagement.Loading
{
    public class FontLoader : IFileLoader<SpriteFont>
    {
        public object Load(string filePath)
        {
            SpriteFont output;

            output = SlaamGame.Content.Load<SpriteFont>(filePath);

            return output;
        }
    }
}