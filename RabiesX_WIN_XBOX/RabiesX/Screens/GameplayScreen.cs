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

        // Set sky and terrain for level.
        Model terrain;
        RabiesX.MapManager.Sky sky;

        // Set background for level.
        //Texture2D stars;

        // Set the 3D model to draw.
        RabiesX.ModelManager.MyModel shipModel;
             
        // Set the position of the camera in world space, for our view matrix.
        //static Vector3 cameraPosition = new Vector3(0.0f, 50.0f, 5000.0f);
 
        // Aspect ratio determines how to scale 3d to 2d projection.
        float aspectRatio;

        Random random = new Random();

        float pauseAlpha;

        int screenWidth;
        int screenHeight;

        // Set camera and projection views.
        Matrix view;
        Matrix proj;

        // Set the avatar position and rotation variables.
        static Vector3 avatarPosition = new Vector3(0, 0, -50);
        static Vector3 cameraPosition = avatarPosition;

        float avatarYaw;

        // Set the direction the camera points without rotation.
        Vector3 cameraReference = new Vector3(0, 0, 1);

        // Set rates in world units per 1/60th second (the default fixed-step interval).
        float rotationSpeed = 1f / 60f;
        float forwardSpeed = 50f / 60f;

        // Set field of view of the camera in radians (pi/4 is 45 degrees).
        static float viewAngle = MathHelper.PiOver4;

        // Set distance from the camera of the near and far clipping planes.
        static float nearClip = 1.0f;
        static float farClip = 2000.0f;

        #endregion

        #region Initialization


        /// <summary>
        /// Constructor.
        /// </summary>
        public GameplayScreen()
        {
            TransitionOnTime = TimeSpan.FromSeconds(1.5);
            TransitionOffTime = TimeSpan.FromSeconds(0.5);
            #if WINDOWS_PHONE
                        // Frame rate is 30 fps by default for Windows Phone.
                        ScreenManager.Game.TargetElapsedTime = TimeSpan.FromTicks(333333);

                        graphics.IsFullScreen = true;
            #endif
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
            
            // Load models and set aspect ratio.
            shipModel = new ModelManager.MyModel("Models\\p1_wedge", content);

            shipModel.Texture("Textures\\wedge_p1_diff_v1", content); 

            //aspectRatio = ScreenManager.Game.GraphicsDevice.Viewport.AspectRatio;
            
            aspectRatio = (float)ScreenManager.Game.GraphicsDevice.Viewport.Width / (float)ScreenManager.Game.GraphicsDevice.Viewport.Height;

            // Get current screen width and height.
            screenWidth = ScreenManager.Game.GraphicsDevice.Viewport.Width;
            screenHeight = ScreenManager.Game.GraphicsDevice.Viewport.Height;

            // Load background texture.
            //stars = content.Load<Texture2D>("Textures/B1_stars");

            // Load terrain and sky.
            terrain = content.Load<Model>("terrain");
            sky = content.Load<RabiesX.MapManager.Sky>("sky");
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
                // Get some input.
                UpdateInput();

                // Update camera and avatar position.
                UpdateAvatarPosition();
                UpdateCamera();

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
                //    (ScreenManager.GraphicsDevice.Viewport.Width / 2 - shipModel.getTexture.Width / 2));
                //Vector3 modeltargetPosition = new Vector3(
                //    (ScreenManager.GraphicsDevice.Viewport.Width / 2 + ScreenManager.GraphicsDevice.Viewport.Height / 2 + ScreenManager.GraphicsDevice.Viewport.MaxDepth / 2) / 3);

                //shipModel.Position = Vector3.Lerp(shipModel.Position, modeltargetPosition, 0.05f);

                //modelRotation += (float)gameTime.ElapsedGameTime.TotalMilliseconds * MathHelper.ToRadians(0.1f);

                // Add velocity to the current position.
                shipModel.Position += shipModel.Velocity;

                //shipModel.Position = cameraPosition;

                // Bleed off velocity over time.
                shipModel.Velocity *= 0.95f;

                base.Update(gameTime, otherScreenHasFocus, false);

                // TODO: this game isn't very fun! You could probably improve
                // it by inserting something more interesting in this space :-)
            }
        }

        /// <summary>
        /// Updates the position and direction of the avatar.
        /// </summary>
        void UpdateAvatarPosition()
        {
            KeyboardState keyboardState = Keyboard.GetState();
            GamePadState currentState = GamePad.GetState(PlayerIndex.One);

            if (keyboardState.IsKeyDown(Keys.A) || (currentState.DPad.Left == ButtonState.Pressed))
            {
                // Rotate left.
                avatarYaw += rotationSpeed;
            }
            if (keyboardState.IsKeyDown(Keys.D) || (currentState.DPad.Right == ButtonState.Pressed))
            {
                // Rotate right.
                avatarYaw -= rotationSpeed;
            }
            if (keyboardState.IsKeyDown(Keys.W) || (currentState.DPad.Up == ButtonState.Pressed))
            {
                Matrix forwardMovement = Matrix.CreateRotationY(avatarYaw);
                Vector3 v = new Vector3(0, 0, forwardSpeed);
                v = Vector3.Transform(v, forwardMovement);
                avatarPosition.Z += v.Z;
                avatarPosition.X += v.X;
            }
            if (keyboardState.IsKeyDown(Keys.S) || (currentState.DPad.Down == ButtonState.Pressed))
            {
                Matrix forwardMovement = Matrix.CreateRotationY(avatarYaw);
                Vector3 v = new Vector3(0, 0, -forwardSpeed);
                v = Vector3.Transform(v, forwardMovement);
                avatarPosition.Z += v.Z;
                avatarPosition.X += v.X;
            }

            // Fix camera up and down zoom with avatar;
            cameraPosition = avatarPosition; 
        }

        /// <summary>
        /// Updates the position and direction of the camera relative to the avatar.
        /// </summary>
        void UpdateCamera()
        {
            // Calculate the camera's current position.

            Matrix rotationMatrix = Matrix.CreateRotationY(avatarYaw);

            // Create a vector pointing the direction the camera is facing.
            Vector3 transformedReference = Vector3.Transform(cameraReference, rotationMatrix);

            // Calculate the position the camera is looking at.
            Vector3 cameraLookat = cameraPosition + transformedReference;

            // Set up the view matrix and projection matrix.
            view = Matrix.CreateLookAt(cameraPosition, cameraLookat, new Vector3(0.0f, 1.0f, 0.0f));

            //proj = Matrix.CreatePerspectiveFieldOfView(viewAngle, ScreenManager.Game.GraphicsDevice.Viewport.AspectRatio,
            //                                              nearClip, farClip);

            proj = Matrix.CreatePerspectiveFieldOfView(viewAngle, aspectRatio, nearClip, farClip);
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

            //// Get current screen width and height.
            //int screenWidth = ScreenManager.Game.GraphicsDevice.Viewport.Width;
            //int screenHeight = ScreenManager.Game.GraphicsDevice.Viewport.Height;

            // Prevent model from moving off the left edge of the screen.
            if (shipModel.Position.X < screenHeight * -5)
                shipModel.Position = new Vector3(screenHeight * -5, shipModel.Position.Y, shipModel.Position.Z);

            // Prevent model from moving off the right edge of the screen.
            if (shipModel.Position.X > screenHeight * 5)
                shipModel.Position = new Vector3(screenHeight * 5, shipModel.Position.Y, shipModel.Position.Z);

            //Console.WriteLine("ship position = {0}", shipModel.pX);
            //Console.WriteLine("screen position = {0}", ScreenManager.Game.GraphicsDevice.Viewport.Width);
            //Console.WriteLine("screen position = {0}", ScreenManager.Game.GraphicsDevice.Viewport.Height);

            // Prevent model from moving off the right edge of the screen.
            //int rightEdge = screenWidth - shipModel.getTexture.Width;
            //if (shipModel.Position.X > rightEdge)
            //    shipModel.Position = new Vector3(rightEdge, shipModel.Position.Y, shipModel.Position.Z);
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
            //ScreenManager.GraphicsDevice.Clear(ClearOptions.Target,
            //                                   Color.CornflowerBlue, 0, 0);
           
            ScreenManager.GraphicsDevice.Clear(Color.Black);
             
            //// Calculate the projection matrix.
            //Matrix projection = Matrix.CreatePerspectiveFieldOfView(MathHelper.PiOver4,
            //                                                        ScreenManager.GraphicsDevice.Viewport.AspectRatio,
            //                                                        1, 10000);

            //proj = Matrix.CreatePerspectiveFieldOfView(MathHelper.PiOver4,
            //                                            ScreenManager.GraphicsDevice.Viewport.AspectRatio,
            //                                            1, 10000);

            // Calculate a view matrix, moving the camera around a circle.
            //float time = (float)gameTime.TotalGameTime.TotalSeconds * 0.333f;

            //float cameraX = (float)Math.Cos(time) - shipModel.Position.X;
            //float cameraY = (float)Math.Sin(time) - shipModel.Position.Y;

            //float cameraX = shipModel.Position.X;
            //float cameraY = shipModel.Position.Y;
     
            //Vector3 cameraPosition = new Vector3(cameraX, 0, cameraY) * 64;
            //Vector3 cameraFront = new Vector3(-cameraY, 0, cameraX);

            //Matrix view = Matrix.CreateLookAt(cameraPosition,
            //                                  cameraPosition + cameraFront,
            //                                  Vector3.Up);
            
            // Draw the terrain first, then the sky. This is faster than
            // drawing the sky first, because the depth buffer can skip
            // bothering to draw sky pixels that are covered up by the
            // terrain. This trick works because the code used to draw
            // the sky forces all the sky vertices to be as far away as
            // possible, and turns depth testing on but depth writes off.

            DrawTerrain(view, proj);

            sky.Draw(view, proj);
            
            //DrawTerrain(view, projection);

            //sky.Draw(view, projection);

            // If there was any alpha blended translucent geometry in
            // the scene, that would be drawn here, after the sky.

            //// Our player and enemy are both actually just text strings.
            //SpriteBatch spriteBatch = ScreenManager.SpriteBatch;

            //spriteBatch.Begin();

            //// Background is set before other objects to be in back.
            ////spriteBatch.Draw(stars, new Rectangle(0, 0, screenWidth, screenHeight), Color.White);

            //spriteBatch.DrawString(gameFont, "// TODO", playerPosition, Color.Green);

            //spriteBatch.DrawString(gameFont, "Insert Gameplay Here",
            //                       enemyPosition, Color.DarkRed);

            //spriteBatch.End();

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

        /// <summary>
        /// Helper for drawing the terrain model.
        /// </summary>
        void DrawTerrain(Matrix view, Matrix projection)
        {
            foreach (ModelMesh mesh in terrain.Meshes)
            {
                foreach (BasicEffect effect in mesh.Effects)
                {
                    effect.View = view;
                    effect.Projection = projection;

                    effect.EnableDefaultLighting();

                    // Set the specular lighting to match the sky color.
                    effect.SpecularColor = new Vector3(0.6f, 0.4f, 0.2f);
                    effect.SpecularPower = 8;

                    // Set the fog to match the distant mountains
                    // that are drawn into the sky texture.
                    effect.FogEnabled = true;
                    effect.FogColor = new Vector3(0.15f);
                    effect.FogStart = 100;
                    effect.FogEnd = 320;
                }

                mesh.Draw();
            }
        }

        #endregion
    }
}
