using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Graphics;
using Microsoft.Xna.Framework.Input;
using System;
using System.Collections.Generic;
using VladimirIlyichLeninNuclearPowerPlant.Simulation;

namespace VladimirIlyichLeninNuclearPowerPlant
{
    /// <summary>
    /// This is the main type for your game.
    /// </summary>
    public class Game1 : Game
    {
        Texture2D plantTexture;
        Texture2D controlRodTexture;
        Texture2D controlRodTargetTexture;
        Texture2D turbineTexture;
        Texture2D pumpTexture;
        Texture2D cursorTexture;
        Texture2D az5Texture;

        SpriteFont defaultFont;

        Matrix scaleMatrix;

        GraphicsDeviceManager graphics;
        SpriteBatch spriteBatch;
        
        List<ControlRod> controlRods;
        Rectangle az5Rectangle;
        Plant plant;

        public Game1()
        {
            graphics = new GraphicsDeviceManager(this);

            //Fullscreen
            //graphics.PreferredBackBufferHeight = GraphicsAdapter.DefaultAdapter.CurrentDisplayMode.Height;
            //graphics.PreferredBackBufferWidth = GraphicsAdapter.DefaultAdapter.CurrentDisplayMode.Width;
            //graphics.ToggleFullScreen();

            //Windowed Resolution
            graphics.PreferredBackBufferHeight = 900;
            graphics.PreferredBackBufferWidth = 1600;

            Content.RootDirectory = "Content";

        }

        /// <summary>
        /// Allows the game to perform any initialization it needs to before starting to run.
        /// This is where it can query for any required services and load any non-graphic
        /// related content.  Calling base.Initialize will enumerate through any components
        /// and initialize them as well.
        /// </summary>
        protected override void Initialize()
        {
            this.IsMouseVisible = true;

            controlRods = new List<ControlRod>();

            plant = new Plant(controlRods, new StandardConstants());


            base.Initialize();
        }

        /// <summary>
        /// LoadContent will be called once per game and is the place to load
        /// all of your content.
        /// </summary>
        protected override void LoadContent()
        {
            // Create a new SpriteBatch, which can be used to draw textures.
            spriteBatch = new SpriteBatch(GraphicsDevice);

            cursorTexture = Content.Load<Texture2D>("comradeCursor");
            MouseCursor mouseCursor = MouseCursor.FromTexture2D(cursorTexture, 1, 2);
            Mouse.PlatformSetCursor(mouseCursor);
            
            plantTexture = Content.Load<Texture2D>("plant");
            controlRodTexture = Content.Load<Texture2D>("controlRod");
            controlRodTargetTexture = Content.Load<Texture2D>("controlRodTarget");
            turbineTexture = Content.Load<Texture2D>("turbine");
            pumpTexture = Content.Load<Texture2D>("pump");
            az5Texture = Content.Load<Texture2D>("AZ-5");

            az5Rectangle = new Rectangle(2000, 20, az5Texture.Width, az5Texture.Height);

            defaultFont = Content.Load<SpriteFont>("Arial");
            //Create control rod instances and add to list of control rods
            for (int i = 0; i < 5; i++)
            {
                controlRods.Add(new ControlRod(new Rectangle(1498 + (i * 43), 761, 27, 810), new Point(controlRodTexture.Width, controlRodTexture.Height)));
            }
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
            if (GamePad.GetState(PlayerIndex.One).Buttons.Back == ButtonState.Pressed || Keyboard.GetState().IsKeyDown(Keys.Escape))
                Exit();

            //Convert mouse position to game world coorinates
            Point gameMousePos = Vector2.Transform(Mouse.GetState().Position.ToVector2(), Matrix.Invert(scaleMatrix)).ToPoint();
            if (az5Rectangle.Contains(gameMousePos))
            {
                if(Mouse.GetState().LeftButton == ButtonState.Pressed)
                {
                    foreach (ControlRod rod in controlRods)
                    {
                        rod.scram();
                    }
                }
            }

            //Update all control rods
            foreach (ControlRod rod in controlRods)
            {
                rod.Update(gameMousePos, gameTime);
            }
            plant.update(gameTime);

            base.Update(gameTime);
        }

        /// <summary>
        /// This is called when the game should draw itself.
        /// </summary>
        /// <param name="gameTime">Provides a snapshot of timing values.</param>
        protected override void Draw(GameTime gameTime)
        {

            GraphicsDevice.Clear(Color.CornflowerBlue);

            //Calculate scaling between output window resolution and the virtual game resolution
            float scaleX = (float)graphics.PreferredBackBufferWidth / plantTexture.Width;
            float scaleY = (float)graphics.PreferredBackBufferHeight / plantTexture.Height;
            scaleMatrix = Matrix.CreateScale(scaleX, scaleY, 1.0f);


            spriteBatch.Begin(transformMatrix: scaleMatrix); //scale the following spritebatch to the output window resolution
            spriteBatch.Draw(plantTexture, new Vector2(0, 0), Color.White); //draw background

            //Draw all controll rods
            foreach (ControlRod rod in controlRods)
            {
                spriteBatch.Draw(controlRodTexture, rod.rectangle, Color.White);
                if (rod.insertedPercentage != rod.targetPercentage)
                { 
                    spriteBatch.Draw(controlRodTargetTexture, rod.targetRectangle, Color.White);
                }
            }

            spriteBatch.Draw(az5Texture, new Rectangle(2000, 20, az5Texture.Width, az5Texture.Height), Color.White);
            //**********************   TEMP CODE   *****************************
            //spriteBatch.Draw(controlRodTexture, new Rectangle(1497, 509, controlRodTexture.Width, controlRodTexture.Height), Color.White);
            //for (int i = 0; i < 5; i++)
            //{
            //    spriteBatch.Draw(controlRodTexture, new Rectangle(1497 + i * 43, 509 + i * 80, controlRodTexture.Width, controlRodTexture.Height), Color.White);
            //}
            spriteBatch.Draw(turbineTexture, new Rectangle(421, 1481, turbineTexture.Width, turbineTexture.Height), null, Color.White, -(float)gameTime.TotalGameTime.TotalSeconds * 7, new Vector2(turbineTexture.Width / 2, turbineTexture.Height / 2), SpriteEffects.None, 0f);
            spriteBatch.Draw(pumpTexture, new Rectangle(1011 + 67, 1390 + 67, pumpTexture.Width, pumpTexture.Height), null, Color.White, -(float)gameTime.TotalGameTime.TotalSeconds * 5, new Vector2(pumpTexture.Width / 2, pumpTexture.Height / 2), SpriteEffects.None, 0f);
            spriteBatch.Draw(pumpTexture, new Rectangle(2047 + 67, 1393 + 67, pumpTexture.Width, pumpTexture.Height), null, Color.White, -(float)gameTime.TotalGameTime.TotalSeconds * -5, new Vector2(pumpTexture.Width / 2, pumpTexture.Height / 2), SpriteEffects.FlipHorizontally, 0f);
            //******************************************************************
            for (int i = 0; i < 5; i++)
            {
                for (int j = 0; j < 5; j++)
                {
                    Cell cell = plant.core.cells[j, i];
                    spriteBatch.DrawString(defaultFont, $"Cell [{j},{i}]: temp:{Math.Round(cell.Temp, 3)},promptFlux = {Math.Round(cell.PromptRate, 3)}, delayedFlux = {Math.Round(cell.DelayedRate, 3)}, moderation = {Math.Round(cell.ModerationPercent, 3)},nonreactive = {Math.Round(cell.NonReactiveAbsorbtionPercent, 3)} , reactive = {Math.Round(cell.ReactiveAbsorbtionPercent, 3)}, xenon = {Math.Round(cell.Xenon, 3)}, prexenon = {Math.Round(cell.PreXenon, 3)}", new Vector2(0, 30 * (i+ 5*j)), Color.Red);
                }
            }
            spriteBatch.DrawString(defaultFont, $"Power: {plant.core.PowerLevel} MW", new Vector2(2400, 0), Color.Red);
            spriteBatch.End();


            base.Draw(gameTime);
        }
    }
}
