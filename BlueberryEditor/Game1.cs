using BlueberryCore;
using BlueberryCore.DataTools;
using BlueberryCore.TextureAtlases;
using Microsoft.Xna.Framework;
using System;
using System.Collections.Generic;

namespace BlueberryEditor
{
    /// <summary>
    /// This is the main type for your game.
    /// </summary>
    public class Game1 : Core
    {
        //TextureAtlas atlas;
        public Game1() : base() { }

        /// <summary>
        /// Allows the game to perform any initialization it needs to before starting to run.
        /// This is where it can query for any required services and load any non-graphic
        /// related content.  Calling base.Initialize will enumerate through any components
        /// and initialize them as well.
        /// </summary>
        protected override void Initialize()
        {
            // TODO: Add your initialization logic here
            
            base.Initialize();

            //var type = Type.GetType("BlueberryCore.Sprite, BlueberryCore, Version=1.0.0.0, Culture=neutral");
            //var inst = Activator.CreateInstance(type, "test", "/missileLauncer/Image");
            //new BinTexturePacker(graphicsDevice).Pack("C:/Users/lotziko/Dropbox/TomorrowMY/GM2/Tomorrow2/sprites", "C:/Users/lotziko/Dropbox/Workspace/Workspace/BlueberryEditor/Content", "test", true, FileType.PNG, 1024, 1024, 0);
            new BinTexturePacker(graphicsDevice).Pack("C:/Users/lotziko/Dropbox/x1", "C:/Users/lotziko/Dropbox/Workspace/Workspace/BlueberryEditor/Content", "UI", true, FileType.BlueberryAtlas);
            //new BinTexturePacker(graphicsDevice).Pack("C:/Users/lotziko/Dropbox/NewTomorrowGraphics", "C:/Users/lotziko/Dropbox/Workspace/Workspace/BlueberryEditor/Content", "test", true, FileType.BlueberryAtlas, 512, 512);
            Scene = new TestScene();

            Window.AllowUserResizing = true;
            
        }

        /// <summary>
        /// LoadContent will be called once per game and is the place to load
        /// all of your content.
        /// </summary>
        protected override void LoadContent()
        {
            //atlas = Content.Load<TextureAtlas>("test");
            // TODO: use this.Content to load your game content here
        }

        /// <summary>
        /// UnloadContent will be called once per game and is the place to unload
        /// game-specific content.
        /// </summary>
        protected override void UnloadContent()
        {
            // TODO: Unload any non ContentManager content here
        }

        /// <summary>
        /// Allows the game to run logic such as updating the world,
        /// checking for collisions, gathering input, and playing audio.
        /// </summary>
        /// <param name="gameTime">Provides a snapshot of timing values.</param>
        protected override void Update(GameTime gameTime)
        {
            // TODO: Add your update logic here

            base.Update(gameTime);
        }

        /// <summary>
        /// This is called when the game should draw itself.
        /// </summary>
        /// <param name="gameTime">Provides a snapshot of timing values.</param>
        protected override void Draw(GameTime gameTime)
        {
            // TODO: Add your drawing code here
            base.Draw(gameTime);
            //Graphics.instance.Draw(atlas.GetTexture(), Vector2.Zero);
        }
    }
}
