﻿using Microsoft.Xna.Framework.Graphics;
using SimpleInjector;
using SlaamMono.Composition.x_;
using SlaamMono.Gameplay;
using SlaamMono.Library;
using SlaamMono.Library.Configurations;
using SlaamMono.Library.Logging;
using SlaamMono.Library.Rendering;
using SlaamMono.Library.ResourceManagement;
using SlaamMono.Library.Screens;
using SlaamMono.MatchCreation;
using SlaamMono.Menus;
using SlaamMono.Metrics;
using SlaamMono.PlayerProfiles;
using SlaamMono.ResourceManagement;
using SlaamMono.ResourceManagement.Loading;
using SlaamMono.x_;
using ZzziveGameEngine;

namespace SlaamMono.Composition
{
    public class Composer
    {
        private Container _container;

        public Container BuildContainer(x_Di resolver)
        {
            _container = new Container();

            _container.RegisterInstance(resolver);
            register();
            registerComponents();
            registerScreens();
            registerResources();
            registerGameplay();

            return _container;
        }

        private void register()
        {
            _container.RegisterInstance(new ProfileFileVersion(new byte[] { 000, 000, 000, 002, }));
            _container.RegisterInstance(new GameConfig(true));
            _container.Register<SlaamGameApp>(Lifestyle.Singleton);
            _container.Register<SlaamGame>(Lifestyle.Singleton);
            _container.Register<IGraphicsState, GraphicsState>(Lifestyle.Singleton);
            _container.RegisterInstance(new GraphicsConfig(GameGlobals.DRAWING_GAME_WIDTH, GameGlobals.DRAWING_GAME_HEIGHT, false, false));
            _container.Register<IGraphicsConfigurer, GraphicsConfigurer>(Lifestyle.Singleton);
            _container.Register<ILoggingDevice, TextFileLoggingDevice>(Lifestyle.Singleton);
            _container.Register<ILogger, Logger>(Lifestyle.Singleton);
        }

        public void registerComponents()
        {
            _container.Register<RenderService>(Lifestyle.Singleton);
            _container.Register<IRenderService, RenderService>(Lifestyle.Singleton);
            _container.Register<FpsRenderer>(Lifestyle.Singleton);
        }

        private void registerScreens()
        {
            _container.RegisterInstance(new Mut<ScreenTransitionState>(new ScreenTransitionState(null, null, false)));
            _container.Register<IMainMenuScreen, MainMenuScreenPerformer>();
            _container.Register<CreditsScreenPerformer>();
            _container.Register<HighScoreScreenPerformer>();
            _container.Register<ProfileEditScreenPerformer>();
            _container.Register<CharacterSelectionScreenPerformer>();
            _container.Register<IScreenManager, ScreenManager>(Lifestyle.Singleton);
            _container.Register<ILogoScreen, LogoScreenPerformer>();
            _container.Register<GameScreenPerformer>();

            // Register all IRequests
            _container.RegisterSingleton(typeof(IResolver<>), typeof(SlaamGameApp).Assembly);
            _container.RegisterSingleton(typeof(IResolver<,>), typeof(SlaamGameApp).Assembly);
        }

        private void registerResources()
        {
            _container.RegisterInstance(new Mut<ResourcesState>());
            _container.RegisterInstance(new ResourcesListsToLoad(new string[] { "BotNames", "Credits", "Textures", "Fonts", "BoardList" }));
            _container.Register<IResources, Resources>(Lifestyle.Singleton);
            _container.Register<IFileLoader<Texture2D>, Texture2DLoader>(Lifestyle.Singleton);
            _container.Register<IFileLoader<string[]>, CommentedTextLineLoader>(Lifestyle.Singleton);
            _container.Register<IFileLoader<SpriteFont>, FontLoader>(Lifestyle.Singleton);
            _container.Register<IFileLoader<CachedTexture>, CachedTextureLoader>(Lifestyle.Singleton);
            _container.Register<IResourceLoader, ResourceLoader>(Lifestyle.Singleton);
            _container.Register<IResolver<TextureRequest, CachedTexture>, CachedTextureRequestHandler>(Lifestyle.Singleton);
        }

        private void registerGameplay()
        {
            _container.Register<PlayerColorResolver>(Lifestyle.Singleton);
        }
    }
}
