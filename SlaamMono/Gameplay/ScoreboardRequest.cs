﻿using Microsoft.Xna.Framework;
using SlaamMono.Gameplay.Actors;
using ZzziveGameEngine;

namespace SlaamMono.Gameplay
{
    public class ScoreboardRequest : IRequest
    {
        public Vector2 Position { get; private set; }
        public CharacterActor Character { get; private set; }
        public GameType GameType { get; private set; }

        public ScoreboardRequest(Vector2 position, CharacterActor character, GameType gameType)
        {
            Position = position;
            Character = character;
            GameType = gameType;
        }

    }
}
