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
        //Texture2D bubbleStripTexture;
        Texture2D bubbleTexture;
        Texture2D plantBubbleClipTexture;
        Texture2D az5Texture;

        Texture2D reactivityMeterPromptTexture;
        Texture2D reactivityMeterDelayedTexture;
        Texture2D reactivityMeterBarTexture;

        Texture2D PowerBackgroundTexture;
        Texture2D BlankNixieTexture;
        List<Texture2D> PowerNixieTextures;

        SpriteFont defaultFont;

        Matrix scaleMatrix;

        GraphicsDeviceManager graphics;
        SpriteBatch spriteBatch;
        
        List<ControlRod> controlRods;
        Rectangle az5Rectangle;
        Plant plant;
        Bubbles bubbles;

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

            PowerNixieTextures = new List<Texture2D>();

            bubbles = new Bubbles();

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
            //bubbleStripTexture = Content.Load<Texture2D>("bubbleStrip");
            bubbleTexture = Content.Load<Texture2D>("bubbleSmall");
            plantBubbleClipTexture = Content.Load<Texture2D>("plantBubbleClip");
            reactivityMeterPromptTexture = Content.Load<Texture2D>("reactivityMeterPrompt");
            reactivityMeterDelayedTexture = Content.Load<Texture2D>("reactivityMeterDelayed");
            reactivityMeterBarTexture = Content.Load<Texture2D>("reactivityMeterBar");


            PowerBackgroundTexture = Content.Load<Texture2D>("PowerBG");
            BlankNixieTexture = Content.Load<Texture2D>("NixieOff");
            for(int i = 0; i < 10; i++)
            {
                PowerNixieTextures.Add(Content.Load<Texture2D>($"Nixie{i}"));
            }

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

            bubbles.SetFlowVelocity("pumpLeftPath", (float)gameTime.TotalGameTime.TotalSeconds);
            bubbles.Update(gameTime);

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

            foreach (Bubble bubble in bubbles.BubblesList)
            {
                spriteBatch.Draw(bubbleTexture, new Rectangle((int)(bubble.Pos.X + bubble.Offset.X), (int)(bubble.Pos.Y + bubble.Offset.Y), bubbleTexture.Width, bubbleTexture.Height), null,  bubble.BubbleColor, 0f, new Vector2(bubbleTexture.Width/2, bubbleTexture.Height/2), SpriteEffects.None, 0f);

            }
            spriteBatch.Draw(plantBubbleClipTexture, new Vector2(0, 0), Color.White); //cover bubbles

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
            
            for (int i = 0; i < 5; i++)
            {
                for (int j = 0; j < 5; j++)
                {
                    Cell cell = plant.core.cells[j, i];
                    spriteBatch.DrawString(defaultFont, $"Cell [{j},{i}]: watertemp:{Math.Round(cell.WaterTemp, 3)},promptFlux = {Math.Round(cell.PromptRate, 3)}, delayedFlux = {Math.Round(cell.DelayedRate, 3)}, moderation = {Math.Round(cell.ModerationPercent, 3)},nonreactive = {Math.Round(cell.NonReactiveAbsorbtionPercent, 3)} , reactive = {Math.Round(cell.ReactiveAbsorbtionPercent, 3)}, xenon = {Math.Round(cell.Xenon, 3)}, prexenon = {Math.Round(cell.PreXenon, 3)}, delayCrit: {Math.Round(cell.CellDelayedCriticality, 3)}, promptCrit: {Math.Round(cell.CellPromptCriticality, 3)}", new Vector2(0, 30 * (i+ 5*j)), Color.Red);
                }
            }
            spriteBatch.DrawString(defaultFont, $"Power: {Math.Round(plant.core.PowerLevel,4)} MW, prompt criticality = {Math.Round(plant.core.PromptCriticality, 4)}, delayed criticality = {Math.Round(plant.core.DelayedCriticality, 4)}", new Vector2(2400, 0), Color.Red);



            spriteBatch.Draw(reactivityMeterPromptTexture, new Rectangle(2350, 50, reactivityMeterPromptTexture.Width, reactivityMeterPromptTexture.Height), Color.White);
            spriteBatch.Draw(reactivityMeterBarTexture, new Rectangle(2356 + reactivtyToPixel(plant.core.PromptCriticality), 60, reactivityMeterBarTexture.Width, reactivityMeterBarTexture.Height), Color.White);
            
            spriteBatch.Draw(reactivityMeterDelayedTexture, new Rectangle(2350, 350, reactivityMeterDelayedTexture.Width, reactivityMeterDelayedTexture.Height), Color.White);
            spriteBatch.Draw(reactivityMeterBarTexture, new Rectangle(2356 + reactivtyToPixel(plant.core.DelayedCriticality), 360, reactivityMeterBarTexture.Width, reactivityMeterBarTexture.Height), Color.White);

            spriteBatch.Draw(PowerBackgroundTexture, new Rectangle(2350, 650, PowerBackgroundTexture.Width, PowerBackgroundTexture.Height), Color.White);
            int power = (int)Math.Min(Math.Round(plant.core.PowerLevel, 0),99999);
            for (int i = 4; i >= 0; i--)
            {
                var digit = power / (int)Math.Pow(10, i) % 10;
                if (digit == 0 && power <= Math.Pow(10, i) && i != 0)
                {
                    spriteBatch.Draw(BlankNixieTexture, new Rectangle(2360 + 111 * (4 - i), 660, BlankNixieTexture.Width, BlankNixieTexture.Height), Color.White);
                }
                else
                {
                    spriteBatch.Draw(PowerNixieTextures[digit], new Rectangle(2360 + 111 * (4 - i), 660, PowerNixieTextures[digit].Width, PowerNixieTextures[digit].Height), Color.White);
                }
            }

            //spriteBatch.Draw(bubbleStripTexture, new Rectangle(1100, 1474, 200, 45), new Rectangle((int)(gameTime.TotalGameTime.TotalSeconds * 100), 0, 200, 45), Color.Blue, 0f, new Vector2(0,0), SpriteEffects.FlipHorizontally, 0f);
            //******************************************************************

            spriteBatch.End();


            base.Draw(gameTime);
        }

        int reactivtyToPixel(double reactivty)
        {
            double RV = reactivty * 100;

            double CV;
            if(RV > 100)
            {
                CV = RV - 99;
            }
            else
            {
                CV = RV - 101;
            }

            double LV;
            if(CV < 0)
            {
                LV = -Math.Log10(-CV);
            }
            else if(CV > 0)
            {
                LV = Math.Log10(CV);
            }
            else
            {
                LV = 0;
            }

            LV += 2;

            return (int)Math.Round(LV * 200,0);
        }

    }
}
