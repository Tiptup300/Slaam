﻿using SlaamMono.PlayerProfiles;
using System.Collections.Generic;
using ZzziveGameEngine.StateManagement;

namespace SlaamMono.Gameplay
{
    public class GameScreenRequestState : IState
    {
        public List<CharacterShell> SetupCharacters { get; private set; }
        public MatchSettings MatchSettings { get; private set; }

        public GameScreenRequestState(List<CharacterShell> chars, MatchSettings matchSettings)
        {
            SetupCharacters = chars;
            MatchSettings = matchSettings;
        }
    }
}