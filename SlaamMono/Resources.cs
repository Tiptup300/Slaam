using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Graphics;
using SlaamMono.Library.Drawing.Text;
using SlaamMono.Library.Logging;
using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;

namespace SlaamMono
{
    public static class Resources
    {
        // General
        public static Texture2D Dot;
        public static CachedTexture2D Feedbar;

        // BG
        public static CachedTexture2D Background;
        public static CachedTexture2D BattleBG;
        public static CachedTexture2D CreditsBG;


        // Qwerty
        public static CachedTexture2D Textbox;
        public static CachedTexture2D KeyboardBG;
        public static CachedTexture2D Key;
        public static CachedTexture2D Spacebar;
        public static CachedTexture2D KeyHT;
        public static CachedTexture2D SpaceHT;

        // Char Select
        public static CachedTexture2D ProfileShell;
        public static CachedTexture2D ProfileShello;

        // Lobby
        public static CachedTexture2D LobbyUnderlay;
        public static CachedTexture2D LobbyOverlay;
        public static CachedTexture2D LobbyColorPreview;
        public static CachedTexture2D LobbyCharBar;
        public static CachedTexture2D HostingBG;
        public static CachedTexture2D CPU;
        public static CachedTexture2D LButton;
        public static CachedTexture2D LButtonHT;
        public static CachedTexture2D LobbyInfoOverlay;
        public static CachedTexture2D BoardSelect;
        public static CachedTexture2D NowLoading;
        public static CachedTexture2D BoardSelectTextUnderlay;

        // Menu
        public static CachedTexture2D Gear;
        public static CachedTexture2D MenuTop;
        public static CachedTexture2D MenuBoard;
        public static CachedTexture2D MenuBlock;
        public static CachedTexture2D MenuOverlay;

        // Menu BG's
        public static CachedTexture2D Menu0;
        public static CachedTexture2D Menu1;
        public static CachedTexture2D Menu2;
        public static CachedTexture2D Menu3;
        public static CachedTexture2D Menu4;
        public static CachedTexture2D Menu5;
        public static CachedTexture2D Menu6;
        public static CachedTexture2D Menu7;

        // Game Board
        public static CachedTexture2D TileOverlay;
        public static CachedTexture2D RespawnTileOverlay;
        public static CachedTexture2D DeadChar;
        public static CachedTexture2D ReadySetGo;
        public static CachedTexture2D TopGameBoard;
        public static CachedTexture2D Waiting;
        public static CachedTexture2D GameScreenScoreBoard;
        public static CachedTexture2D PauseScreen;


        public static Texture2D DefaultBoard;
        //public static Texture2D DefaultChar;

        // Powerups
        public static CachedTexture2D MuhLazza1;
        public static CachedTexture2D MuhLazza2;

        // Stats
        public static CachedTexture2D StatsBoard;
        public static CachedTexture2D Star;
        public static Texture2D[] StatsButtons = new Texture2D[3];

        // Fonts
        public static TextManager textmanager;
        public static SpriteFont SegoeUIx32pt;
        public static SpriteFont SegoeUIx14pt;
        public static SpriteFont SegoeUIx48ptBold;

        // Logos
        public static CachedTexture2D ZibithLogo;
        public static CachedTexture2D ZibithLogoBG;

        // Text Files
        public static List<String> BotNames;
        public static List<String> Credits;

        // Player Colors
        public static Color[] PlayerColors = new Color[] {
            Color.Red,
            Color.Blue,
            Color.Green,
            Color.Yellow,
            Color.Cyan,
            Color.Orange,
            Color.Purple,
            Color.Pink
        };

        public static CachedTexture2D MenuChoice;
        public static CachedTexture2D MenuChoiceGlow;

        // PowerUps 
        public static Texture2D[] PU_SpeedUp = new Texture2D[2];
        public static Texture2D[] PU_SpeedDown = new Texture2D[2];
        public static Texture2D[] PU_Inversion = new Texture2D[2];
        public static Texture2D[] PU_Slaam = new Texture2D[2];

        private static ILogger _logger;

        #region Constructor

        public static void Initiailze(ILogger logger)
        {
            _logger = logger;
        }

        #endregion

        #region Load All Method

        /// <summary>
        /// Loads all nessessary resources that are constant.
        /// </summary>
        public static void LoadAll()
        {
            _logger.Log("Resources Loading...");

            Dot = new Texture2D(SlaamGame.Graphics.GraphicsDevice, 1, 1);

            //Dot = new Texture2D(Game1.Graphics.GraphicsDevice, 1, 1, 1, TextureUsage.None, SurfaceFormat.Color);
            Dot.SetData<Color>(new Color[] { Color.White });
            _logger.Log(" - Dot Image Created.");


            BattleBG = new CachedTexture2D("BattleScreen//battlebg");
            ReadySetGo = new CachedTexture2D("BattleScreen//readysetgo");
            RespawnTileOverlay = new CachedTexture2D("BattleScreen//respawnOverlay");
            TileOverlay = new CachedTexture2D("BattleScreen//tileOverlay");

            MenuTop = new CachedTexture2D("MenuScreen//menutop");
            ProfileShell = new CachedTexture2D("MenuScreen//CharacterSelectBox");
            StatsBoard = new CachedTexture2D("MenuScreen/StatsScreen");
            StatsButtons[0] = LoadImage("MenuScreen/StatsButton1");
            StatsButtons[1] = LoadImage("MenuScreen/StatsButton2");
            StatsButtons[2] = LoadImage("MenuScreen/StatsButton3");

            LobbyCharBar = new CachedTexture2D("LobbyScreen/PlayerBar");
            LobbyUnderlay = new CachedTexture2D("LobbyScreen/LobbyBG");
            LobbyOverlay = new CachedTexture2D("LobbyScreen/LobbyOverlay");
            LobbyColorPreview = new CachedTexture2D("LobbyScreen/PlayerColorPreview");

            BoardSelect = new CachedTexture2D("Misc//boardSelect");
            ZibithLogoBG = new CachedTexture2D("Misc//LogoBG");
            ZibithLogo = new CachedTexture2D("Misc//Logo");
            NowLoading = new CachedTexture2D("Misc//BoardLoading");
            Background = new CachedTexture2D("Misc//background");
            SlaamGame.mainBlade.CurrentGameInfo.GameIcon = LoadImage("Misc//ZBladeIcon");

            SegoeUIx32pt = LoadFont("SegoeUI-32pt");
            SegoeUIx14pt = LoadFont("SegoeUI-14pt");
            SegoeUIx48ptBold = LoadFont("SegoeUI-48pt");
            BotNames = LoadStringList("BotNames");
            Credits = LoadStringList("Credits");

            LoadPowerup(PU_SpeedUp, "SpeedUp");
            LoadPowerup(PU_SpeedDown, "SpeedDown");
            LoadPowerup(PU_Inversion, "Inversion");
            LoadPowerup(PU_Slaam, "Slaam");

            _logger.Log("All Resources Finished Loading;");
            textmanager = new TextManager(SlaamGame.Instance);
            SlaamGame.Instance.Components.Add(textmanager);
        }

        #endregion

        #region Easy Load Methods

        /// <summary>
        /// Easy method of loading in an image.
        /// </summary>
        /// <param name="baseName">Name of Resource</param>
        /// <returns></returns>
        public static Texture2D LoadImage(string baseName)
        {
            String loc;

#if ZUNE
            loc = System.IO.Path.Combine("content\\textures\\MOBILE\\", baseName);
#else
            loc = System.IO.Path.Combine($"content\\textures{GameGlobals.TEXTURE_FILE_PATH}", baseName);
#endif

            Texture2D output;

            try
            {
                output = SlaamGame.Content.Load<Texture2D>(loc);
                _logger.Log(" - " + baseName + " Texture Loaded.");
            }
            catch (Exception ex)
            {
                _logger.Log($"Texture at {loc} failed to load. Error: {ex.Message}");
                output = new Texture2D(SlaamGame.Graphics.GraphicsDevice, 1, 1);
            }
            return output;
        }

        private static List<String> LoadStringList(string baseName)
        {
            var temp = File.ReadAllLines(Directory.GetCurrentDirectory() + "\\content\\" + baseName + ".txt").ToList();

            for (int x = 0; x < temp.Count; x++)
            {
                if (temp[x].Trim().Substring(0, 2) == "//")
                    temp.RemoveAt(x--);

            }
            return temp;
        }

        private static void LoadPowerup(Texture2D[] Texs, string powerupname)
        {
            Texs[0] = LoadImage("powerups\\" + powerupname);
            Texs[1] = LoadImage("powerups\\" + powerupname + "0");
        }

        /// <summary>
        /// Easy method of loading in an font.
        /// </summary>
        /// <param name="baseName">Name of Resource</param>
        /// <returns></returns>
        private static SpriteFont LoadFont(string baseName)
        {
            SpriteFont temp = SlaamGame.Content.Load<SpriteFont>(string.Format("content\\{0}", baseName));
            _logger.Log(" - " + baseName + " Font Loaded.");
            return temp;
        }

        #endregion

        #region Draw String Method

        /// <summary>
        /// Draws in a string using Nuclex.Fonts library.
        /// </summary>
        /// <param name="str">String to draw</param>
        /// <param name="pos">Position to draw</param>
        /// <param name="fnt">Font to draw in</param>
        /// <param name="alnt">Alignment to draw</param>
        /// <param name="col">Color of font</param>
        /// <param name="Shadow">Draw shadow?</param>
        public static void DrawString(/*SpriteBatch batch,*/ String str, Vector2 pos, SpriteFont fnt, FontAlignment alnt, Color col, bool Shadow)
        {
            TextAlignment alignment = TextAlignment.Default;

            if (alnt == FontAlignment.Middle)
            {
                alignment = TextAlignment.VerticallyCentered;
            }
            else if (alnt == FontAlignment.Top)
            {
                alignment = TextAlignment.Top;
            }
            else if (alnt == FontAlignment.Right)
            {
                alignment = TextAlignment.Right;
            }
            else if (alnt == FontAlignment.Center)
            {
                alignment = TextAlignment.Centered;
            }
            else if (alnt == FontAlignment.CompletelyCentered)
            {
                alignment = TextAlignment.Centered;
            }

            if (Shadow)
            {
                textmanager.DrawString(fnt, new Vector2(pos.X + 1, pos.Y + 2), str, alignment, new Color(0, 0, 0, 127));
                textmanager.DrawString(fnt, new Vector2(pos.X + 2, pos.Y + 1), str, alignment, new Color(0, 0, 0, 127));
            }
            textmanager.DrawString(fnt, pos, str, alignment, col);
        }



        #endregion
    }

    #region FontAlignment Enumeration

    public enum FontAlignment
    {
        Left,
        Center,
        Right,
        Top,
        Middle,
        Bottom,
        CompletelyCentered,
    }

    #endregion
}
