using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Graphics;
using Microsoft.Xna.Framework.Input;
using System.Collections.Generic;

namespace VladimirIlyichLeninNuclearPowerPlant
{
    /// <summary>
    /// This is the main type for your game.
    /// </summary>
    public class Game1 : Game
    {
        Texture2D plantTexture;
        Texture2D controlRodTexture;
        Texture2D turbineTexture;
        Texture2D cursorTexture;

        Matrix scaleMatrix;

        GraphicsDeviceManager graphics;
        SpriteBatch spriteBatch;

        MouseCursor mouseCursor;

        List<ControlRod> controlRods;

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
            turbineTexture = Content.Load<Texture2D>("turbine");

            //Create control rod instances and add to list of control rods
            for (int i = 0; i < 5; i++)
            {
                controlRods.Add(new ControlRod(new Rectangle(1497 + (i * 43), 509, 27, 810), new Point(controlRodTexture.Width, controlRodTexture.Height)));
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

            //Update all control rods
            foreach (ControlRod rod in controlRods)
            {
                rod.Update(gameMousePos);
            }

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
                spriteBatch.Draw(controlRodTexture, rod.rodRectangle, Color.White);
            }
            
            //**********************   TEMP CODE   *****************************
            //spriteBatch.Draw(controlRodTexture, new Rectangle(1497, 509, controlRodTexture.Width, controlRodTexture.Height), Color.White);
            //for (int i = 0; i < 5; i++)
            //{
            //    spriteBatch.Draw(controlRodTexture, new Rectangle(1497 + i * 43, 509 + i * 80, controlRodTexture.Width, controlRodTexture.Height), Color.White);
            //}
            spriteBatch.Draw(turbineTexture, new Rectangle(421, 1481, turbineTexture.Width, turbineTexture.Height), null, Color.White, -(float)gameTime.TotalGameTime.TotalSeconds * 10, new Vector2(turbineTexture.Width/2, turbineTexture.Height/2), SpriteEffects.None, 0f);
            //******************************************************************

            spriteBatch.End();


            base.Draw(gameTime);
        }
    }
}
