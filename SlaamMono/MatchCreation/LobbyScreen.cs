using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Graphics;
using SlaamMono.Gameplay;
using SlaamMono.Graphing;
using SlaamMono.Library;
using SlaamMono.Library.Input;
using SlaamMono.Library.Logging;
using SlaamMono.Library.Rendering;
using SlaamMono.Library.Rendering.Text;
using SlaamMono.Library.Screens;
using SlaamMono.Menus;
using SlaamMono.PlayerProfiles;
using SlaamMono.Resources;
using SlaamMono.x_;
using System.Collections.Generic;
using System.IO;

namespace SlaamMono.MatchCreation
{
    public class LobbyScreen : IScreen
    {
        public static Texture2D DefaultBoard;
#if !ZUNE
        private const int MAX_PLAYERS = 8;
#else
        private const int MAX_PLAYERS = 4;
#endif
        public List<CharacterShell> SetupChars;
        private Texture2D CurrentBoardTexture;
        private int PlayerAmt;
        private string[] Dialogs = new string[2];
        private string CurrentBoardLocation;
#if !ZUNE
        private bool ButtonOnLeft = true;
#endif
        private bool ViewingSettings = false;
        public Graph MainMenu = new Graph(new Rectangle(10, 10, GameGlobals.DRAWING_GAME_WIDTH - 20, 624), 2, new Color(0, 0, 0, 150));
        private IntRange MenuChoice;

        private readonly ILogger _logger;
        private readonly IScreenManager _screenDirector;
        private readonly PlayerColorResolver _playerColorResolver;

        public LobbyScreen(List<CharacterShell> chars, ILogger logger, IScreenManager screenDirector, PlayerColorResolver playerColorResolver)
        {
            SetupChars = chars;
            _logger = logger;
            _screenDirector = screenDirector;
            _playerColorResolver = playerColorResolver;
            PlayerAmt = SetupChars.Count;
            MainMenu.Items.Columns.Add("SETTING");
            MainMenu.Items.Columns.Add("SETTING");
            MainMenu.Items.Add(true,
                new GraphItemSetting(0, "GameType", "Classic", "Spree", "Timed Spree"),
                new GraphItemSetting(1, "Lives (Classic Only)", "3", "5", "10", "20", "40", "50", "100"),
                new GraphItemSetting(2, "Speed", "0.50x", "0.75x", "1.00x", "1.25x", "1.50x"),
                new GraphItemSetting(3, "Time of Match (Timed Spree Only)", "1 Minute", "2 Minutes", "3 Minutes", "5 Minutes", "10 Minutes", "20 Minutes", "30 Minutes", "45 Minutes", "1 Hour"),
                new GraphItemSetting(4, "Respawn Time", "Instant", "2 Seconds", "4 Seconds", "6 Seconds", "8 Seconds", "10 Seconds"),
                new GraphItemSetting(5, "Kills To Win (Spree Only)", "5", "10", "15", "20", "25", "30", "40", "50", "100"),
                new GraphItem("", "Choose Board..."),
                new GraphItem("", "Save"),
                new GraphItem("", "Cancel")
            );
            MenuChoice = new IntRange(0, 0, MainMenu.Items.Count - 1);
            CurrentMatchSettings.ReadValues(this);
            MainMenu.SetHighlight(MenuChoice.Value);

            if (CurrentMatchSettings.BoardLocation != null && CurrentMatchSettings.BoardLocation.Trim() != "" && File.Exists(CurrentMatchSettings.BoardLocation))
            {
                LoadBoard(CurrentMatchSettings.BoardLocation);
            }
            else
            {
                BoardThumbnailViewer viewer = new BoardThumbnailViewer(this);
                viewer.Open();
                while (!viewer.FoundBoard)
                {
                    viewer.Update();
                }
                LoadBoard(viewer.ValidBoard);
                viewer.Close();
            }

            SetupZune();
        }

        public static void SetupZune()
        {
            SlaamGame.mainBlade.Status = ZBlade.BladeStatus.In;
            SlaamGame.mainBlade.UserCanCloseMenu = false;

            ZBlade.InfoBlade.BladeHiddenSetup = ZBlade.InfoBlade.BladeInSetup;
            ZBlade.InfoBlade.BladeInSetup = new ZBlade.BladeSetup("Back", "Start", "Game Settings");
        }

        public static void ResetZune()
        {
            SlaamGame.mainBlade.Status = ZBlade.BladeStatus.Hidden;
            ZBlade.InfoBlade.BladeInSetup = ZBlade.InfoBlade.BladeHiddenSetup;
        }

        public static Texture2D LoadQuickBoard()
        {
            if (DefaultBoard == null)
            {
                BoardThumbnailViewer viewer = new BoardThumbnailViewer(null);
                viewer.Open();
                while (!viewer.FoundBoard)
                {
                    viewer.Update();
                }
                DefaultBoard = SlaamGame.Content.Load<Texture2D>("content\\Boards\\" + GameGlobals.TEXTURE_FILE_PATH + viewer.ValidBoard);
                viewer.Close();
            }

            return DefaultBoard;
        }

        public void Open()
        {
            BackgroundManager.ChangeBG(BackgroundManager.BackgroundType.Menu);
            if (SetupChars.Count == 1)
            {
                AddComputer();
                PlayerAmt++;
            }
            FeedManager.InitializeFeeds(DialogStrings.LobbyScreenFeed);
        }

        public void Update()
        {
            BackgroundManager.SetRotation(1f);
            if (ViewingSettings)
            {
                if (InputComponent.Players[0].PressedDown)
                {
                    MenuChoice.Add(1);
                    MainMenu.SetHighlight(MenuChoice.Value);
                }
                if (InputComponent.Players[0].PressedUp)
                {
                    MenuChoice.Sub(1);
                    MainMenu.SetHighlight(MenuChoice.Value);
                }

                if (MainMenu.Items[MenuChoice.Value].GetType() == typeof(GraphItemSetting))
                {
                    if (InputComponent.Players[0].PressedLeft)
                    {
                        MainMenu.Items[MenuChoice.Value].ToSetting().ChangeValue(false);
                        MainMenu.CalculateBlocks();
                    }
                    else if (InputComponent.Players[0].PressedRight)
                    {
                        MainMenu.Items[MenuChoice.Value].ToSetting().ChangeValue(true);
                        MainMenu.CalculateBlocks();
                    }
                }
                else
                {
                    if (InputComponent.Players[0].PressedAction)
                    {
                        if (MainMenu.Items[MenuChoice.Value].Details[1] == "Save")
                        {
                            CurrentMatchSettings.SaveValues(this, CurrentBoardLocation);
                            ViewingSettings = false;
                            SetupZune();
                        }
                        else if (MainMenu.Items[MenuChoice.Value].Details[1] == "Cancel")
                        {
                            CurrentMatchSettings.ReadValues(this);
                            ViewingSettings = false;
                            SetupZune();
                        }
                        else
                        {
                            _screenDirector.ChangeTo(new BoardThumbnailViewer(this));
                        }
                        BackgroundManager.ChangeBG(BackgroundManager.BackgroundType.Menu);
                    }
                }
            }
            else
            {
                if (InputComponent.Players[0].PressedAction2)
                {
                    _screenDirector.ChangeTo(
                        new ClassicCharSelectScreen(
                            DiImplementer.Instance.Get<ILogger>(),
                            DiImplementer.Instance.Get<MainMenuScreen>(),
                            DiImplementer.Instance.Get<IScreenManager>()));
                    ProfileManager.ResetAllBots();
                    ResetZune();
                }

                if (InputComponent.Players[0].PressedUp && SetupChars.Count < MAX_PLAYERS)
                {
                    AddComputer();
                }
                if (InputComponent.Players[0].PressedDown && SetupChars.Count > PlayerAmt)
                {
                    ProfileManager.ResetBot(SetupChars[SetupChars.Count - 1].CharProfile);
                    SetupChars.RemoveAt(SetupChars.Count - 1);
                }

                if (InputComponent.Players[0].PressedStart)
                {
                    CurrentMatchSettings.SaveValues(this, CurrentBoardLocation);
                    GameScreen.Instance = new GameScreen(
                        SetupChars,
                        DiImplementer.Instance.Get<ILogger>(),
                        DiImplementer.Instance.Get<IScreenManager>());
                    _screenDirector.ChangeTo(GameScreen.Instance);
                    ProfileManager.ResetAllBots();
                    ResetZune();
                }

                if (InputComponent.Players[0].PressedAction)
                {
                    ViewingSettings = true;
                    ResetZune();

                    BackgroundManager.ChangeBG(BackgroundManager.BackgroundType.Normal);
                }

            }
        }
#if !ZUNE
        public void Draw(SpriteBatch batch)
        {
            if (ViewingSettings)
            {
                MainMenu.Draw(batch);
            }
            else
            {
                if (CurrentBoardTexture != null)
                    batch.Draw(CurrentBoardTexture, new Rectangle((int)(640 - Resources.LobbyUnderlay.Width / 2) + 17, (int)(512 - Resources.LobbyUnderlay.Height / 2) + 21, 400, 400), Color.White);
                batch.Draw(Resources.LobbyInfoOverlay.Texture, new Vector2((int)(640 - Resources.LobbyUnderlay.Width / 2) + 17, (int)(512 - Resources.LobbyUnderlay.Height / 2) + 21 + 400 - Resources.LobbyInfoOverlay.Height), Color.White);
               
                Resources.DrawString(DialogStrings.GetDescMsg(), new Vector2((int)(640 - Resources.LobbyUnderlay.Width / 2) + 17 + 9, (int)(512 - Resources.LobbyUnderlay.Height / 2) + 21 + 400 - Resources.LobbyInfoOverlay.Height + 22), Resources.SegoeUIx14pt, TextAlignment.Default, Color.White, true);

                batch.Draw(Resources.LobbyUnderlay.Texture, new Vector2(640 - Resources.LobbyUnderlay.Width / 2, 512 - Resources.LobbyUnderlay.Height / 2), Color.White);

                float YOffset = 330;

                for (int x = 0; x < SetupChars.Count; x++)
                {
                    batch.Draw(Resources.LobbyCharBar.Texture, new Vector2(689, YOffset + 30 * x), Color.White);
                    batch.Draw(Resources.LobbyColorPreview.Texture, new Vector2(689, YOffset + 30 * x), SetupChars[x].PlayerColor);
                    if (SetupChars[x].Type == PlayerType.Player)
                        Resources.DrawString(DialogStrings.Player + (x + 1) + ": " + ProfileManager.AllProfiles[SetupChars[x].CharProfile].Name, new Vector2(725, YOffset + 18 + 30 * x), Resources.SegoeUIx14pt, TextAlignment.Default, Color.Black, false);
                    else
                        Resources.DrawString(DialogStrings.Player + (x + 1) + ": BOT [ " + ProfileManager.AllProfiles[SetupChars[x].CharProfile].Name + " ]", new Vector2(725, YOffset + 18 + 30 * x), Resources.SegoeUIx14pt, TextAlignment.Default, Color.Red, false);
                }

                batch.Draw(Resources.LobbyOverlay.Texture, new Vector2(640 - Resources.LobbyUnderlay.Width / 2, 512 - Resources.LobbyUnderlay.Height / 2), Color.White);
                batch.Draw(Resources.CPU.Texture, new Vector2(640 - Resources.LobbyUnderlay.Width / 2, 512 - Resources.LobbyUnderlay.Height / 2), Color.White);

                batch.Draw(Resources.LButton.Texture, new Vector2(700, 673), Color.White);
                batch.Draw(Resources.LButton.Texture, new Vector2(845, 673), Color.White);
                Resources.DrawString("Start", new Vector2(700 + 145 / 2, 673 + 18), Resources.SegoeUIx14pt, TextAlignment.Centered, Color.White, true);
                Resources.DrawString("Settings", new Vector2(845 + 145 / 2, 673 + 18), Resources.SegoeUIx14pt, TextAlignment.Centered, Color.White, true);
                batch.Draw(Resources.LButtonHT.Texture, new Vector2((ButtonOnLeft ? 700 : 845), 673), Color.White);
                //Resources.DrawString(Dialogs[0], new Vector2(700, 336), Resources.SegoeUIx14pt, TextAlignment.Default, Color.Black,false);
                //Resources.DrawString(Dialogs[1], new Vector2(700, 698), Resources.SegoeUIx14pt, TextAlignment.Default, Color.Black,false);
            }
        }
#else
        public void Draw(SpriteBatch batch)
        {
            if (ViewingSettings)
            {
                MainMenu.Draw(batch);
            }
            else
            {
                batch.Draw(ResourceManager.Instance.GetTexture("LobbyUnderlay").Texture, Vector2.Zero, Color.White);
                float YOffset = 75;

                for (int x = 0; x < SetupChars.Count; x++)
                {
                    batch.Draw(ResourceManager.Instance.GetTexture("LobbyCharBar").Texture, new Vector2(0, YOffset + 30 * x), Color.White);
                    batch.Draw(ResourceManager.Instance.GetTexture("LobbyColorPreview").Texture, new Vector2(0, YOffset + 30 * x), SetupChars[x].PlayerColor);
                    if (SetupChars[x].Type == PlayerType.Player)
                        RenderGraphManager.Instance.RenderText(DialogStrings.Player + (x + 1) + ": " + ProfileManager.AllProfiles[SetupChars[x].CharProfile].Name, new Vector2(36, YOffset + 18 + 30 * x), ResourceManager.Instance.GetFont("SegoeUIx14pt"), Color.Black, TextAlignment.Default, false);
                    else
                        RenderGraphManager.Instance.RenderText(DialogStrings.Player + (x + 1) + ": *" + ProfileManager.AllProfiles[SetupChars[x].CharProfile].Name + "*", new Vector2(36, YOffset + 18 + 30 * x), ResourceManager.Instance.GetFont("SegoeUIx14pt"), Color.Red, TextAlignment.Default, false);
                }
                batch.Draw(ResourceManager.Instance.GetTexture("LobbyOverlay").Texture, Vector2.Zero, Color.White);
            }
        }
#endif
        public void Close()
        {
            CurrentBoardTexture = null;
            ResourceManager.Instance.GetTexture("LobbyUnderlay").Dispose();
            ResourceManager.Instance.GetTexture("LobbyCharBar").Dispose();
            ResourceManager.Instance.GetTexture("LobbyColorPreview").Dispose();
            ResourceManager.Instance.GetTexture("LobbyOverlay").Dispose();
        }

        /// <summary>
        /// Loads the new Board Texture and loads its name/creator.
        /// </summary>
        public void LoadBoard(string brdloc)
        {
            if (CurrentBoardTexture != null && CurrentBoardLocation == brdloc)
            {

            }
            else
            {
                try
                {
                    CurrentBoardTexture = SlaamGame.Content.Load<Texture2D>("content\\Boards\\" + GameGlobals.TEXTURE_FILE_PATH + brdloc);
                    CurrentBoardLocation = brdloc;
                }
                catch (FileNotFoundException)
                {

                }
                Dialogs[0] = DialogStrings.CurrentBoard + CurrentBoardLocation.Substring(CurrentBoardLocation.IndexOf('_') + 1).Replace(".png", "").Replace("boards\\", "");
                if (CurrentBoardLocation.IndexOf('_') >= 0)
                    Dialogs[1] = DialogStrings.CreatedBy + CurrentBoardLocation.Substring(0, CurrentBoardLocation.IndexOf('_')).Replace(".png", "").Replace("boards\\", "");
                else
                    Dialogs[1] = "";

                DefaultBoard = CurrentBoardTexture;
            }
        }

        /// <summary>
        /// Adds a new computer player.
        /// </summary>
        private void AddComputer()
        {
            SetupChars.Add(new CharacterShell(ClassicCharSelectScreen.ReturnRandSkin(_logger), ProfileManager.GetBotProfile(), (ExtendedPlayerIndex)SetupChars.Count, PlayerType.Computer, _playerColorResolver.GetColorByIndex(SetupChars.Count)));
        }
    }
}