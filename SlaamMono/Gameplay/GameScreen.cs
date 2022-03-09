using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Graphics;
using SlaamMono.Composition.x_;
using SlaamMono.Gameplay.Actors;
using SlaamMono.Gameplay.Boards;
using SlaamMono.Gameplay.Powerups;
using SlaamMono.Gameplay.Statistics;
using SlaamMono.Library;
using SlaamMono.Library.Input;
using SlaamMono.Library.Logging;
using SlaamMono.Library.Rendering;
using SlaamMono.Library.ResourceManagement;
using SlaamMono.Library.Screens;
using SlaamMono.MatchCreation;
using SlaamMono.PlayerProfiles;
using SlaamMono.SubClasses;
using SlaamMono.Survival;
using SlaamMono.x_;
using System;
using System.Linq;
using ZBlade;
using ZzziveGameEngine;
using ZzziveGameEngine.StateManagement;

namespace SlaamMono.Gameplay
{
    public class GameScreen : IStatePerformer
    {
        public static GameScreen Instance;

        private GameScreenState _state = new GameScreenState();
        private SurvivalGameScreenState _survivalState = new SurvivalGameScreenState();

        private readonly IResources _resources;
        private readonly IGraphicsState _graphics;
        private readonly IResolver<ScoreboardRequest, Scoreboard> _gameScreenScoreBoardResolver;
        private readonly ILogger _logger;

        public GameScreen(
            IResources resources,
            IGraphicsState graphicsState,
            IResolver<ScoreboardRequest, Scoreboard> gameScreenScoreBoardResolver,
            ILogger logger)
        {
            _resources = resources;
            _graphics = graphicsState;
            _gameScreenScoreBoardResolver = gameScreenScoreBoardResolver;
            _logger = logger;
        }

        public void Initialize(GameScreenRequestState gameScreenRequest)
        {
            _state.SetupCharacters = gameScreenRequest.SetupCharacters;
            _state.CurrentMatchSettings = gameScreenRequest.MatchSettings;
        }

        public void InitializeState()
        {
            _state.PowerupTime = new Timer(new TimeSpan(0, 0, 0, 15));
            _state.ReadySetGoThrottle = new Timer(new TimeSpan(0, 0, 0, 0, 325));
            _state.Tiles = new Tile[GameGlobals.BOARD_WIDTH, GameGlobals.BOARD_HEIGHT];
            _state.CurrentGameStatus = GameStatus.Waiting;
            _state.GameType = _state.CurrentMatchSettings.GameType;
            SetupTheBoard(_state.CurrentMatchSettings.BoardLocation);
            _state.CurrentGameStatus = GameStatus.MovingBoard;
            _resources.GetTexture("ReadySetGo").Load();
            _resources.GetTexture("BattleBG").Load();

            _state.Boardpos = new Vector2(calcFinalBoardPosition().X, -_state.Tileset.Height);

            _state.Timer = new GameScreenTimer(
                x_Di.Get<IResources>(),
                new Vector2(1024, 0),
                _state.GameType);

            for (int x = 0; x < GameGlobals.BOARD_WIDTH; x++)
            {
                for (int y = 0; y < GameGlobals.BOARD_HEIGHT; y++)
                {
                    _state.Tiles[x, y] = new Tile(
                        _state.Boardpos,
                        new Vector2(x, y),
                        _state.Tileset,
                        x_Di.Get<IResources>(),
                        x_Di.Get<IRenderGraph>());
                }
            }
            _state.ScoreKeeper = new MatchScoreCollection(this, _state.GameType, _state);
            _state.ReadySetGoThrottle.Update(FrameRateDirector.MovementFactorTimeSpan);
            if (_state.GameType == GameType.Classic)
            {
                _state.StepsRemaining = _state.SetupCharacters.Count - 1;
            }
            else if (_state.GameType == GameType.TimedSpree)
            {
                _state.StepsRemaining = 7;
            }
            else if (_state.GameType == GameType.Spree)
            {
                _state.StepsRemaining = 100;
                _state.KillsToWin = _state.CurrentMatchSettings.KillsToWin;
                _state.SpreeStepSize = 10;
                _state.SpreeCurrentStep = 0;
            }

            setupPauseMenu();
        }

        private Vector2 calcFinalBoardPosition()
        {
            int width = (int)(_graphics.Get().PreferredBackBufferWidth / 2f);
            int height = (int)(_graphics.Get().PreferredBackBufferHeight / 2f);
            int boardWidth = GameGlobals.BOARD_WIDTH * GameGlobals.TILE_SIZE;
            int boardHeight = GameGlobals.BOARD_HEIGHT * GameGlobals.TILE_SIZE;

            return new Vector2(width - boardWidth / 2f, height - boardHeight / 2f);
        }

        private void setupPauseMenu()
        {
            SlaamGame.mainBlade.UserCanNavigateMenu = true;
            SlaamGame.mainBlade.UserCanCloseMenu = false;
            MenuTextItem resume = new MenuTextItem("Resume Game");
            resume.Activated += delegate
            {
                _state.IsPaused = false;
                SlaamGame.mainBlade.Status = BladeStatus.Hidden;
            };
            MenuTextItem quit = new MenuTextItem("Quit Game");

            quit.Activated += delegate
            {
                SlaamGame.mainBlade.Status = BladeStatus.Hidden;
                EndGame();
            };

            _state.main.Nodes.Add(resume);
            _state.main.Nodes.Add(quit);

            SlaamGame.mainBlade.TopMenu = _state.main;
        }

        private void SetupTheBoard(string BoardLoc)
        {
            if (_state.GameType == GameType.Survival)
            {
                survival_SetupTheBoard(BoardLoc);
            }
            else
            {
                _state.Tileset = SlaamGame.Content.Load<Texture2D>("content\\Boards\\" + GameGlobals.TEXTURE_FILE_PATH + BoardLoc);

                for (int x = 0; x < _state.SetupCharacters.Count; x++)
                {
                    if (_state.SetupCharacters[x].PlayerType == PlayerType.Player)
                    {
                        _state.Characters.Add(
                            new CharacterActor(
                                SlaamGame.Content.Load<Texture2D>("content\\skins\\" + _state.SetupCharacters[x].SkinLocation),
                                _state.SetupCharacters[x].CharacterProfileIndex,
                                new Vector2(-100, -100),
                                InputComponent.Players[(int)_state.SetupCharacters[x].PlayerIndex],
                                _state.SetupCharacters[x].PlayerColor,
                                x,
                                x_Di.Get<IResources>()));
                    }
                    else
                    {
                        ProfileManager.AllProfiles[_state.SetupCharacters[x].CharacterProfileIndex].Skin = _state.SetupCharacters[x].SkinLocation;
                        _state.Characters.Add(
                            new BotActor(
                                SlaamGame.Content.Load<Texture2D>("content\\skins\\" + _state.SetupCharacters[x].SkinLocation),
                                _state.SetupCharacters[x].CharacterProfileIndex,
                                new Vector2(-100, -100),
                                this,
                                _state.SetupCharacters[x].PlayerColor,
                                _state.Characters.Count,
                                x_Di.Get<IResources>()));
                    }

                    _state.Scoreboards.Add(
                        _gameScreenScoreBoardResolver.Resolve(
                            new ScoreboardRequest(
                                Vector2.Zero,
                                _state.Characters[_state.Characters.Count - 1],
                                _state.GameType)));
                }
            }
        }
        private void survival_SetupTheBoard(string BoardLoc)
        {
            _state.GameType = GameType.Survival;
            MatchSettings.CurrentMatchSettings.GameType = GameType.Survival;
            MatchSettings.CurrentMatchSettings.SpeedMultiplyer = 1f;
            MatchSettings.CurrentMatchSettings.RespawnTime = new TimeSpan(0, 0, 8);
            MatchSettings.CurrentMatchSettings.LivesAmt = 1;
            _state.Tileset = LobbyScreen.LoadQuickBoard();

            _state.Characters.Add(new CharacterActor(SlaamGame.Content.Load<Texture2D>("content\\skins\\" + _state.SetupCharacters[0].SkinLocation) /*Texture2D.FromFile(Game1.Graphics.GraphicsDevice, SetupChars[0].SkinLocation)*/, _state.SetupCharacters[0].CharacterProfileIndex, new Vector2(-100, -100), InputComponent.Players[0], Color.White, 0, x_Di.Get<IResources>()));
            _state.Scoreboards.Add(
                _gameScreenScoreBoardResolver.Resolve(
                    new ScoreboardRequest(
                        new Vector2(-250, 10),
                        _state.Characters[0],
                        _state.GameType)));
        }

        public virtual IState Perform()
        {
            if (_state.GameType == GameType.Survival)
            {
                survival_Perform();
            }
            if (_state.IsPaused)
            {

                SlaamGame.mainBlade.Status = BladeStatus.Out;
                return _state;
            }

            _state.Timer.Update(_state);
            updateScoreBoards();

            if (_state.CurrentGameStatus == GameStatus.MovingBoard)
            {
                updateMovingBoardState();
            }
            else if (_state.CurrentGameStatus == GameStatus.Respawning)
            {
                updateRespawningGameState();
            }
            else if (_state.CurrentGameStatus == GameStatus.Waiting)
            {
                updateWaitingGameState();
            }
            else if (_state.CurrentGameStatus == GameStatus.Playing)
            {
                updatePlayingGameState();
            }
            else if (_state.CurrentGameStatus == GameStatus.Over)
            {
                updateOverGameState();
            }
            return _state;
        }

        public void survival_Perform()
        {
            if (_state.CurrentGameStatus == GameStatus.Playing)
            {
                _survivalState.TimeToAddBot.Update(FrameRateDirector.MovementFactorTimeSpan);
                if (_survivalState.TimeToAddBot.Active)
                {
                    for (int x = 0; x < _survivalState.BotsToAdd + 1; x++)
                    {
                        survival_AddNewBot(_state);
                        _survivalState.BotsAdded++;

                        if (_state.Rand.Next(0, _survivalState.BotsAdded - 1) == _survivalState.BotsAdded)
                        {
                            _survivalState.BotsToAdd++;
                        }
                    }
                }

                for (int x = 0; x < _state.Characters.Count; x++)
                {
                    if (_state.Characters[x] != null && _state.Characters[x].Lives == 0)
                    {
                        _state.Characters[x] = null;
                        _state.NullChars++;
                    }
                }
            }

            bool temp = _state.CurrentGameStatus == GameStatus.Waiting;
            if (_state.CurrentGameStatus == GameStatus.Playing && temp)
            {
                survival_AddNewBot(_state);
            }
        }
        private void survival_AddNewBot(GameScreenState gameScreenState)
        {
            _state.Characters.Add(
                new BotActor(
                    SlaamGame.Content.Load<Texture2D>("content\\skins\\" + SkinLoadingFunctions.ReturnRandSkin(_logger)),
                    ProfileManager.GetBotProfile(),
                    new Vector2(-200, -200),
                    this,
                    Color.Black,
                    _state.Characters.Count,
                    x_Di.Get<IResources>()));

            ProfileManager.ResetAllBots();
            RespawnCharacter(gameScreenState, _state.Characters.Count - 1);
        }

        private void updateOverGameState()
        {
            _state.IsTiming = false;
            _state.ReadySetGoThrottle.Update(FrameRateDirector.MovementFactorTimeSpan);
            if (_state.ReadySetGoThrottle.Active)
            {
                EndGame();
            }
        }
        private void updatePlayingGameState()
        {
            for (int x = 0; x < _state.Characters.Count; x++)
            {
                if (_state.Characters[x] != null)
                {
                    int X1 = (int)((_state.Characters[x].Position.X - _state.Boardpos.X) % GameGlobals.TILE_SIZE);
                    int Y1 = (int)((_state.Characters[x].Position.Y - _state.Boardpos.Y) % GameGlobals.TILE_SIZE);
                    int X = (int)((_state.Characters[x].Position.X - _state.Boardpos.X - X1) / GameGlobals.TILE_SIZE);
                    int Y = (int)((_state.Characters[x].Position.Y - _state.Boardpos.Y - Y1) / GameGlobals.TILE_SIZE);
                    _state.Characters[x].Update(new Vector2(X, Y), new Vector2(X1, Y1), _state);
                    if (_state.Characters[x].CurrentState == CharacterActor.CharacterState.Respawning)
                    {
                        RespawnCharacter(_state, x);
                    }
                }
            }
            for (int x = 0; x < GameGlobals.BOARD_WIDTH; x++)
            {
                for (int y = 0; y < GameGlobals.BOARD_HEIGHT; y++)
                {
                    _state.Tiles[x, y].Update(_state);
                }
            }
            _state.PowerupTime.Update(FrameRateDirector.MovementFactorTimeSpan);
            if (_state.PowerupTime.Active)
            {
                bool found = true;
                int newx = _state.Rand.Next(0, GameGlobals.BOARD_WIDTH);
                int newy = _state.Rand.Next(0, GameGlobals.BOARD_HEIGHT);
                int ct = 0;

                while (_state.Tiles[newx, newy].CurrentTileCondition != TileCondition.Normal)
                {
                    newx = _state.Rand.Next(0, GameGlobals.BOARD_WIDTH);
                    newy = _state.Rand.Next(0, GameGlobals.BOARD_HEIGHT);
                    ct++;
                    if (ct > 100)
                    {
                        found = false;
                        break;
                    }
                }
                if (found)
                {
                    _state.Tiles[newx, newy].MarkWithPowerup(PowerupManager.Instance.GetRandomPowerup());
                }
            }
        }
        private void updateWaitingGameState()
        {
            _state.ReadySetGoThrottle.Update(FrameRateDirector.MovementFactorTimeSpan);
            if (_state.ReadySetGoThrottle.Active)
            {
                _state.ReadySetGoPart++;
                if (_state.ReadySetGoPart > 2)
                {
                    _state.CurrentGameStatus = GameStatus.Playing;
                    _state.ReadySetGoPart = 2;
                    _state.ReadySetGoThrottle.Reset();
                    _state.IsTiming = true;
                }
            }
        }
        private void updateRespawningGameState()
        {
            _state.ReadySetGoThrottle.Update(FrameRateDirector.MovementFactorTimeSpan);
            if (_state.ReadySetGoThrottle.Active)
            {
                _state.Scoreboards[_state.ReadySetGoPart].Moving = true;
                RespawnCharacter(_state, _state.ReadySetGoPart++);
                if (_state.ReadySetGoPart == _state.Characters.Count)
                {
                    _state.CurrentGameStatus = GameStatus.Waiting;
                    _state.ReadySetGoThrottle.Threshold = new TimeSpan(0, 0, 0, 1, 300);
                    _state.ReadySetGoThrottle.Reset();
                    _state.ReadySetGoThrottle.Update(FrameRateDirector.MovementFactorTimeSpan);
                    _state.ReadySetGoPart = 0;
                    _state.Timer.Moving = true;
                }
            }
        }
        private void updateMovingBoardState()
        {
            _state.Boardpos = new Vector2(_state.Boardpos.X, _state.Boardpos.Y + FrameRateDirector.MovementFactor * (10f / 100f));

            if (_state.Boardpos.Y >= calcFinalBoardPosition().Y)
            {
                _state.Boardpos = calcFinalBoardPosition();
                _state.CurrentGameStatus = GameStatus.Respawning;
            }
            for (int x = 0; x < GameGlobals.BOARD_WIDTH; x++)
            {
                for (int y = 0; y < GameGlobals.BOARD_HEIGHT; y++)
                {
                    _state.Tiles[x, y].ResetTileLocation(_state.Boardpos, new Vector2(x, y));
                }
            }
        }
        private void updateScoreBoards()
        {
            for (int x = 0; x < _state.Scoreboards.Count; x++)
            {
                _state.Scoreboards[x].Update();
            }
        }

        public void RenderState(SpriteBatch batch)
        {
            if (_state.IsPaused)
            {
                return;
            }
            for (int x = 0; x < GameGlobals.BOARD_WIDTH; x++)
            {
                for (int y = 0; y < GameGlobals.BOARD_HEIGHT; y++)
                {
                    _state.Tiles[x, y].Draw(batch);
                }
            }

            float PlayersDrawn = 0, CurrY = 0;

            int CurrPlayer = -1;

            while (PlayersDrawn < _state.Characters.Count - _state.NullChars)
            {
                CurrY = 1280;
                CurrPlayer = -1;
                for (int x = 0; x < _state.Characters.Count; x++)
                {
                    if (_state.Characters[x] != null && !_state.Characters[x].Drawn && _state.Characters[x].Position.Y <= CurrY)
                    {
                        CurrY = _state.Characters[x].Position.Y;
                        CurrPlayer = x;
                    }
                }
                _state.Characters[CurrPlayer].Drawn = true;
                _state.Characters[CurrPlayer].Draw(batch);
                PlayersDrawn++;
            }

            resetCharactersDrawnStatus();

            for (int x = 0; x < _state.Characters.Count; x++)
            {
                if (_state.Characters[x] != null)
                {
                    _state.Characters[x].Drawn = false;
                }
            }
            if (_state.CurrentGameStatus == GameStatus.Waiting || _state.CurrentGameStatus == GameStatus.Over)
            {
                batch.Draw(_resources.GetTexture("ReadySetGo").Texture, new Vector2((float)_state.Rand.NextDouble() * (1 + _state.ReadySetGoPart) + GameGlobals.DRAWING_GAME_WIDTH / 2 - _resources.GetTexture("ReadySetGo").Width / 2, (float)_state.Rand.NextDouble() * (1 + _state.ReadySetGoPart) + GameGlobals.DRAWING_GAME_HEIGHT / 2 - _resources.GetTexture("ReadySetGo").Width / 8), new Rectangle(0, _state.ReadySetGoPart * (_resources.GetTexture("ReadySetGo").Height / 4), _resources.GetTexture("ReadySetGo").Width, _resources.GetTexture("ReadySetGo").Height / 4), Color.White);
            }
        }
        private void resetCharactersDrawnStatus()
        {
            _state.Characters
                .Where(character => character != null)
                .ToList()
                .ForEach(character => character.Drawn = false);
        }

        public void Close()
        {
            _resources.GetTexture("ReadySetGo").Unload();
            _resources.GetTexture("BattleBG").Unload();
        }




        private static void markBoardOutline()
        {
            for (int x = 0; x < GameGlobals.BOARD_WIDTH; x++)
            {
                // TODO FIX LOGIC!
                /*tiles[x, 0 + Boardsize].MarkTile(Color.Black, ShortenTime, true, -2);
                tiles[x, 15 - Boardsize].MarkTile(Color.Black, ShortenTime, true, -2);
                tiles[0 + Boardsize, x].MarkTile(Color.Black, ShortenTime, true, -2);
                tiles[15 - Boardsize, x].MarkTile(Color.Black, ShortenTime, true, -2);*/
            }
        }

        private void EndGame()
        {
            if (_state.GameType == GameType.Survival)
            {
                survival_EndGame();
            }
            else
            {
                _state.ScoreKeeper.CalcTotals(_state);
                for (int x = 0; x < _state.Characters.Count; x++)
                {
                    _state.Characters[x].SaveProfileData();
                }
                ProfileManager.SaveProfiles();
                new StatsScreenRequestState(_state.ScoreKeeper, _state.GameType);
            }
        }
        private void survival_EndGame()
        {
            if (ProfileManager.AllProfiles[_state.Characters[0].ProfileIndex].BestGame < _state.Timer.CurrentGameTime)
            {
                ProfileManager.AllProfiles[_state.Characters[0].ProfileIndex].BestGame = _state.Timer.CurrentGameTime;
            }
            ProfileManager.SaveProfiles();
            new StatsScreenRequestState(_state.ScoreKeeper, _state.GameType);
        }

        public static void ShortenBoard(GameScreenState gameScreenState)
        {
            TimeSpan ShortenTime = new TimeSpan(0, 0, 0, 2);
            if (gameScreenState.BoardSize < 6)
            {
                markBoardOutline();
                gameScreenState.BoardSize++;
            }
            gameScreenState.StepsRemaining--;
            if (gameScreenState.StepsRemaining == 0)
            {
                gameScreenState.CurrentGameStatus = GameStatus.Over;
                gameScreenState.ReadySetGoPart = 3;
                gameScreenState.ReadySetGoThrottle.Update(FrameRateDirector.MovementFactorTimeSpan);
            }
        }
        public static void RespawnCharacter(GameScreenState gameScreenState, int characterIndex)
        {
            int newx = gameScreenState.Rand.Next(0, GameGlobals.BOARD_WIDTH);
            int newy = gameScreenState.Rand.Next(0, GameGlobals.BOARD_HEIGHT);

            while (gameScreenState.Tiles[newx, newy].Dead || gameScreenState.Tiles[newx, newy].CurrentTileCondition == TileCondition.RespawnPoint)
            {
                newx = gameScreenState.Rand.Next(0, GameGlobals.BOARD_WIDTH);
                newy = gameScreenState.Rand.Next(0, GameGlobals.BOARD_HEIGHT);
            }
            Vector2 newCharPos = InterpretCoordinates(gameScreenState, new Vector2(newx, newy), false);
            gameScreenState.Characters[characterIndex].Respawn(new Vector2(newCharPos.X + GameGlobals.TILE_SIZE / 2f, newCharPos.Y + GameGlobals.TILE_SIZE / 2f), new Vector2(newx, newy), gameScreenState.Tiles);
        }
        public static Vector2 InterpretCoordinates(GameScreenState gameScreenState, Vector2 position, bool flip)
        {
            if (!flip)
            {
                return new Vector2(gameScreenState.Boardpos.X + position.X * GameGlobals.TILE_SIZE, gameScreenState.Boardpos.Y + position.Y * GameGlobals.TILE_SIZE);
            }
            else
            {

                int X1 = (int)((position.X - gameScreenState.Boardpos.X) % GameGlobals.TILE_SIZE);
                int Y1 = (int)((position.Y - gameScreenState.Boardpos.Y) % GameGlobals.TILE_SIZE);
                int X = (int)((position.X - gameScreenState.Boardpos.X - X1) / GameGlobals.TILE_SIZE);
                int Y = (int)((position.Y - gameScreenState.Boardpos.Y - Y1) / GameGlobals.TILE_SIZE);

                if (position.X < gameScreenState.Boardpos.X)
                    X = -1;
                if (position.Y < gameScreenState.Boardpos.Y)
                    Y = -1;

                return new Vector2(X, Y);
            }
        }

        // to remove
        public static void ReportKilling(int Killer, int Killee, GameScreenState gameScreenState)
        {
            if (gameScreenState.GameType == GameType.Survival)
            {
                survival_ReportKilling(Killer, Killee, gameScreenState);
            }
            else
            {
                if (gameScreenState.Characters[Killee].Lives == 0 && gameScreenState.GameType == GameType.Classic)
                {
                    ShortenBoard(gameScreenState);
                }

                if (Killer != -2 && Killer < gameScreenState.Characters.Count)
                {
                    gameScreenState.Characters[Killer].Kills++;
                }
                gameScreenState.ScoreKeeper.ReportKilling(Killer, Killee, gameScreenState);

                if (gameScreenState.GameType == GameType.Spree && Killer != -2)
                {
                    if (gameScreenState.Characters[Killer].Kills > gameScreenState.SpreeHighestKillCount)
                    {
                        gameScreenState.SpreeCurrentStep += gameScreenState.Characters[Killer].Kills - gameScreenState.SpreeHighestKillCount;
                        gameScreenState.SpreeHighestKillCount = gameScreenState.Characters[Killer].Kills;

                        if (gameScreenState.SpreeCurrentStep >= gameScreenState.SpreeStepSize)
                        {
                            gameScreenState.SpreeCurrentStep -= gameScreenState.SpreeStepSize;
                            if (gameScreenState.Characters[Killer].Kills < gameScreenState.KillsToWin && gameScreenState.StepsRemaining == 1)
                            {
                                // WHY IS THIS HAPPENING!?!??!?!
                            }
                            else
                            {
                                ShortenBoard(gameScreenState);
                                int TimesShortened = 100 - gameScreenState.StepsRemaining;
                            }
                        }

                        if (gameScreenState.Characters[Killer].Kills == gameScreenState.KillsToWin)
                        {
                            gameScreenState.StepsRemaining = 1;
                            ShortenBoard(gameScreenState);
                        }
                    }
                }
            }
        }

        public static void survival_ReportKilling(int Killer, int Killee, GameScreenState gameScreenState)
        {
            if (Killer == 0)
            {
                gameScreenState.Characters[Killer].Kills++;
            }

            if (Killee == 0)
            {
                gameScreenState.CurrentGameStatus = GameStatus.Over;
                gameScreenState.ReadySetGoPart = 3;
                gameScreenState.ReadySetGoThrottle.Update(FrameRateDirector.MovementFactorTimeSpan);
            }
        }
    }
}
