#region File Description
//-----------------------------------------------------------------------------
// GameplayScreen.cs
//
// Microsoft XNA Community Game Platform
// Copyright (C) Microsoft Corporation. All rights reserved.
//-----------------------------------------------------------------------------
#endregion

#region Using Statements
using System;
using System.Threading;
using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Audio;
using Microsoft.Xna.Framework.Content;
using Microsoft.Xna.Framework.Graphics;
using Microsoft.Xna.Framework.Input;
#endregion

namespace RabiesX
{
    /// <summary>
    /// This screen implements the actual game logic. It is just a
    /// placeholder to get the idea across: you'll probably want to
    /// put some more interesting gameplay in here!
    /// </summary>
    /// 
    class GameplayScreen : GameScreen
    {
        #region Fields
        
        ContentManager content;
        SpriteFont gameFont;

        Vector2 playerPosition = new Vector2(100, 100);
        Vector2 enemyPosition = new Vector2(100, 100);

        // Set the 3D model to draw.
        RabiesX.ModelManager.MyModel shipModel;
        
        // Set the position of the camera in world space, for our view matrix.
        Vector3 cameraPosition = new Vector3(0.0f, 50.0f, 5000.0f);
 
        // The aspect ratio determines how to scale 3d to 2d projection
        float aspectRatio;

        Random random = new Random();

        float pauseAlpha;

        #endregion

        #region Initialization


        /// <summary>
        /// Constructor.
        /// </summary>
        public GameplayScreen()
        {
            TransitionOnTime = TimeSpan.FromSeconds(1.5);
            TransitionOffTime = TimeSpan.FromSeconds(0.5);
        }


        /// <summary>
        /// Load graphics content for the game.
        /// </summary>
        public override void LoadContent()
        {
            if (content == null)
                content = new ContentManager(ScreenManager.Game.Services, "Content");

            gameFont = content.Load<SpriteFont>("gamefont");

            // A real game would probably have more content than this sample, so
            // it would take longer to load. We simulate that by delaying for a
            // while, giving you a chance to admire the beautiful loading screen.
            Thread.Sleep(1000);

            // Once the load has finished, we use ResetElapsedTime to tell the game's
            // timing mechanism that we have just finished a very long frame, and that
            // it should not try to catch up.
            ScreenManager.Game.ResetElapsedTime();

            // Load models and set aspect ratio
            shipModel = new ModelManager.MyModel("Models\\p1_wedge", content);

            aspectRatio = ScreenManager.Game.GraphicsDevice.Viewport.AspectRatio;
        }


        /// <summary>
        /// Unload graphics content used by the game.
        /// </summary>
        public override void UnloadContent()
        {
            content.Unload();
        }


        #endregion

        #region Update and Draw


        /// <summary>
        /// Updates the state of the game. This method checks the GameScreen.IsActive
        /// property, so the game will stop updating when the pause menu is active,
        /// or if you tab away to a different application.
        /// </summary>
        public override void Update(GameTime gameTime, bool otherScreenHasFocus,
                                                       bool coveredByOtherScreen)
        {
            base.Update(gameTime, otherScreenHasFocus, false);

            // Gradually fade in or out depending on whether we are covered by the pause screen.
            if (coveredByOtherScreen)
                pauseAlpha = Math.Min(pauseAlpha + 1f / 32, 1);
            else
                pauseAlpha = Math.Max(pauseAlpha - 1f / 32, 0);

            if (IsActive)
            {
                // Apply some random jitter to make the enemy move around.
                const float randomization = 10;

                enemyPosition.X += (float)(random.NextDouble() - 0.5) * randomization;
                enemyPosition.Y += (float)(random.NextDouble() - 0.5) * randomization;

                // Apply a stabilizing force to stop the enemy moving off the screen.
                Vector2 targetPosition = new Vector2(
                    ScreenManager.GraphicsDevice.Viewport.Width / 2 - gameFont.MeasureString("Insert Gameplay Here").X / 2, 
                    200);

                enemyPosition = Vector2.Lerp(enemyPosition, targetPosition, 0.05f);

                // Apply a stabilizing force to stop the test model moving off the screen.
                //Vector3 modeltargetPosition = new Vector3(
                //    (ScreenManager.GraphicsDevice.Viewport.Width / 2 + ScreenManager.GraphicsDevice.Viewport.Height / 2 + ScreenManager.GraphicsDevice.Viewport.MaxDepth / 2) / 3);

                //modeltargetPosition = Vector3.Lerp(modelPosition, modeltargetPosition, 0.05f);

                //modelRotation += (float)gameTime.ElapsedGameTime.TotalMilliseconds * MathHelper.ToRadians(0.1f);

                // Get some input.
                UpdateInput();

                // Add velocity to the current position.
                shipModel.Position += shipModel.Velocity;

                // Bleed off velocity over time.
                shipModel.Velocity *= 0.95f;

                base.Update(gameTime, otherScreenHasFocus, false);
                // TODO: this game isn't very fun! You could probably improve
                // it by inserting something more interesting in this space :-)
            }
        }

        protected void UpdateInput()
        {
            // Get the keyboard state.
            KeyboardState currentState = Keyboard.GetState(PlayerIndex.One);
            if (currentState.IsKeyDown(Keys.Space) || currentState.IsKeyDown(Keys.Up) || currentState.IsKeyDown(Keys.Down) ||
                currentState.IsKeyDown(Keys.Left) || currentState.IsKeyDown(Keys.Right))
            {
                Vector2 movement = Vector2.Zero;

                if (currentState.IsKeyDown(Keys.Left))
                    movement.X--;

                if (currentState.IsKeyDown(Keys.Right))
                    movement.X++;

                if (currentState.IsKeyDown(Keys.Up))
                    movement.Y--;

                if (currentState.IsKeyDown(Keys.Down))
                    movement.Y++;

                float thrusters = 0.0f;

                if (currentState.IsKeyDown(Keys.Space))
                    thrusters += 1.1f;

                // Rotate the model using the left thumbstick, and scale it down.
                shipModel.rX -= movement.X * 0.10f;

                // Create some velocity if the right trigger is down.
                Vector3 modelVelocityAdd = Vector3.Zero;

                // Find out what direction we should be thrusting, using rotation.
                modelVelocityAdd.X = -(float)Math.Sin(shipModel.rX);
                modelVelocityAdd.Z = -(float)Math.Cos(shipModel.rX);

                // Now scale our direction by when the SpaceBar is down.
                modelVelocityAdd *= thrusters;

                // Finally, add this vector to our velocity.
                shipModel.Velocity += modelVelocityAdd;
                

                // In case you get lost, press LeftShift to warp back to the center.
                if (currentState.IsKeyDown(Keys.LeftShift) == true)
                {
                    shipModel.Position = Vector3.Zero;
                    shipModel.Velocity = Vector3.Zero;
                    shipModel.Rotation = Vector3.Zero;
                }
            }
        }

        /// <summary>
        /// Lets the game respond to player input. Unlike the Update method,
        /// this will only be called when the gameplay screen is active.
        /// </summary>
        public override void HandleInput(InputState input)
        {
            if (input == null)
                throw new ArgumentNullException("input");

            // Look up inputs for the active player profile.
            int playerIndex = (int)ControllingPlayer.Value;

            KeyboardState keyboardState = input.CurrentKeyboardStates[playerIndex];
            GamePadState gamePadState = input.CurrentGamePadStates[playerIndex];

            // The game pauses either if the user presses the pause button, or if
            // they unplug the active gamepad. This requires us to keep track of
            // whether a gamepad was ever plugged in, because we don't want to pause
            // on PC if they are playing with a keyboard and have no gamepad at all!
            bool gamePadDisconnected = !gamePadState.IsConnected &&
                                       input.GamePadWasConnected[playerIndex];

            if (input.IsPauseGame(ControllingPlayer) || gamePadDisconnected)
            {
                ScreenManager.AddScreen(new PauseMenuScreen(), ControllingPlayer);
            }
            else
            {
                // Otherwise move the player position.
                Vector2 movement = Vector2.Zero;
                
                if (keyboardState.IsKeyDown(Keys.A))
                    movement.X--;

                if (keyboardState.IsKeyDown(Keys.D))
                    movement.X++;

                if (keyboardState.IsKeyDown(Keys.W))
                    movement.Y--;

                if (keyboardState.IsKeyDown(Keys.S))
                    movement.Y++;
                
                Vector2 thumbstick = gamePadState.ThumbSticks.Left;

                movement.X += thumbstick.X;
                movement.Y -= thumbstick.Y;

                if (movement.Length() > 1)
                    movement.Normalize();

                playerPosition += movement * 2;
            }
        }

   
        /// <summary>
        /// Draws the gameplay screen.
        /// </summary>
        public override void Draw(GameTime gameTime)
        {
            // This game has a blue background. Why? Because!
            ScreenManager.GraphicsDevice.Clear(ClearOptions.Target,
                                               Color.CornflowerBlue, 0, 0);

            // Our player and enemy are both actually just text strings.
            SpriteBatch spriteBatch = ScreenManager.SpriteBatch;

            spriteBatch.Begin();

            spriteBatch.DrawString(gameFont, "// TODO", playerPosition, Color.Green);

            spriteBatch.DrawString(gameFont, "Insert Gameplay Here",
                                   enemyPosition, Color.DarkRed);

            spriteBatch.End();

            // Copy any parent transforms.
            Matrix[] transforms = new Matrix[shipModel.ModelHeld.Bones.Count];
            shipModel.ModelHeld.CopyAbsoluteBoneTransformsTo(transforms);

            // Draw the model. A model can have multiple meshes, so loop.
            foreach (ModelMesh mesh in shipModel.ModelHeld.Meshes)
            {
                // This is where the mesh orientation is set, as well as our camera and projection.
                foreach (BasicEffect effect in mesh.Effects)
                {
                    effect.EnableDefaultLighting();
                    effect.World = transforms[mesh.ParentBone.Index] * Matrix.CreateRotationY(shipModel.rX) * Matrix.CreateTranslation(shipModel.Position);
                    effect.View = Matrix.CreateLookAt(cameraPosition, Vector3.Zero, Vector3.Up);
                    effect.Projection = Matrix.CreatePerspectiveFieldOfView(MathHelper.ToRadians(45.0f), aspectRatio, 1.0f, 10000.0f);
                }
                // Draw the mesh using the effects set above.
                mesh.Draw();
            }
            base.Draw(gameTime);

            // If the game is transitioning on or off, fade it out to black.
            if (TransitionPosition > 0 || pauseAlpha > 0)
            {
                float alpha = MathHelper.Lerp(1f - TransitionAlpha, 1f, pauseAlpha / 2);

                ScreenManager.FadeBackBufferToBlack(alpha);
            }
        }


        #endregion
    }
}
