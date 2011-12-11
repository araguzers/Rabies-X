#region Using Statements
using System;
using System.Text;
using System.Threading;
using System.Collections;
using Microsoft.Xna.Framework;
using System.Collections.Generic;
using Microsoft.Xna.Framework.Audio;
using Microsoft.Xna.Framework.Content;
using Microsoft.Xna.Framework.Graphics;
using Microsoft.Xna.Framework.Input;
using SkinnedModel;
#endregion


namespace RabiesX
{
    /// <summary>
    /// This is a game component that implements IUpdateable.
    /// </summary>
    class Level2GameScreen : GameplayScreen
    {
        public Level2GameScreen()
            : base()
        {

        }

        /// <summary>
        /// Allows the game component to perform any initialization it needs to before starting
        /// to run.  This is where it can query for any required services and load content.
        /// </summary>

        public override void LoadContent()
        {
            if (content == null)
                content = new ContentManager(ScreenManager.Game.Services, "Content");

            TOTAL_RABID_DOGS = 40;
            aspectRatio = (float)ScreenManager.Game.GraphicsDevice.Viewport.Width / (float)ScreenManager.Game.GraphicsDevice.Viewport.Height;

            // Initialize flicker for when health bar on danger.
            flicker = true;

            // Get current screen width and height.
            screenWidth = ScreenManager.Game.GraphicsDevice.Viewport.Width;
            screenHeight = ScreenManager.Game.GraphicsDevice.Viewport.Height;

            // Setup frame buffer.
            GameStateManagementGame.graphics.SynchronizeWithVerticalRetrace = false;
            GameStateManagementGame.graphics.PreferredBackBufferWidth = screenWidth;
            GameStateManagementGame.graphics.PreferredBackBufferHeight = screenHeight;
            GameStateManagementGame.graphics.PreferMultiSampling = true;
            GameStateManagementGame.graphics.ApplyChanges();

            cry = content.Load<SoundEffect>("Audio\\Waves\\araguzbattlecry");
            cryInstance = cry.CreateInstance();

            win = content.Load<SoundEffect>("Audio\\Waves\\winningyell");
            winInstance = win.CreateInstance();

            bark = content.Load<SoundEffect>("Audio\\Waves\\barkingdog");
            barkInstance = bark.CreateInstance();

            donotfear = content.Load<SoundEffect>("Audio\\Waves\\donotfear");
            donotfearInstance = donotfear.CreateInstance();

            motherearth = content.Load<SoundEffect>("Audio\\Waves\\motherearth");
            motherearthInstance = motherearth.CreateInstance();

            callmegerry = content.Load<SoundEffect>("Audio\\Waves\\callmegerry");
            callmegerryInstance = callmegerry.CreateInstance();

            jacksonwincry = content.Load<SoundEffect>("Audio\\Waves\\jacksonwincry");
            jacksonwincryInstance = jacksonwincry.CreateInstance();

            jacksonlosecry = content.Load<SoundEffect>("Audio\\Waves\\jacksonlosecry");
            jacksonlosecryInstance = jacksonlosecry.CreateInstance();

            taunt = content.Load<SoundEffect>("Audio\\Waves\\jacksontaunt");
            tauntInstance = taunt.CreateInstance();

            becareful = content.Load<SoundEffect>("Audio\\Waves\\becareful");
            becarefulInstance = becareful.CreateInstance();

            dogsinpark = content.Load<SoundEffect>("Audio\\Waves\\dogsinpark");
            dogsinparkInstance = dogsinpark.CreateInstance();

            onfire = content.Load<SoundEffect>("Audio\\Waves\\onfire");
            onfireInstance = onfire.CreateInstance();

            totherescue = content.Load<SoundEffect>("Audio\\Waves\\totherescue");
            totherescueInstance = totherescue.CreateInstance();

            telepath = content.Load<SoundEffect>("Audio\\Waves\\telepath");
            telepathInstance = telepath.CreateInstance();

            collectsample = content.Load<SoundEffect>("Audio\\Waves\\collectsample");
            collectsampleInstance = collectsample.CreateInstance();

            virussamples = content.Load<SoundEffect>("Audio\\Waves\\virussamples");
            virussamplesInstance = virussamples.CreateInstance();

            plasmaray = content.Load<SoundEffect>("Audio\\Waves\\plasmaray");
            plasmarayInstance = plasmaray.CreateInstance();
            plasmarayInstance.IsLooped = true;

            gameMusic = content.Load<SoundEffect>("Audio\\Waves\\gamemusic");
            gameMusicInstance = gameMusic.CreateInstance();
            gameMusicInstance.IsLooped = true;
            //gameMusicInstance.Play();

            numberOfCollectedSamples = 0;

            initialNumberOfDogs = TOTAL_RABID_DOGS;

            // Position the in-game text.
            fontPos = new Vector2(1.0f, 1.0f);

            // Setup the initial input states.
            curMouseState = Mouse.GetState();
            curKeyboardState = Keyboard.GetState();

            // Load the game font.
            gameFont = content.Load<SpriteFont>("Fonts\\gamefont");

            // Initialize the sprite batch for the in-game font.
            spriteBatch = new SpriteBatch(ScreenManager.GraphicsDevice);

            // Load the in-game font.
            spriteFont = content.Load<SpriteFont>("Fonts\\ingamefont");

            // Initialize the sprite batch for the health bar.
            mBatch = new SpriteBatch(ScreenManager.GraphicsDevice);

            // Load the health bar image.
            mHealthBar = content.Load<Texture2D>("healthbar");

            // Load the bullet image and setup bullet effects.
            quadEffect = content.Load<Effect>("Effects\\effects");
            quad = new Quad(ScreenManager.GraphicsDevice, Vector3.Zero, Vector3.Up, screenWidth / 2, screenHeight / 2);
            bullets = content.Load<Texture2D>("Textures\\bulletfire");

            // Load the effects and setup vertices for triangle indicator.
            vertices = new List<VertexPositionColor[]>();
            effect = content.Load<Effect>("Effects\\effects");
            indicatorPos = new Vector3(5, 45, 0);
            SetUpVertices(indicatorPos, indicatorScale);

            // Intialize lists for enemies.
            rabidDogHealths = new List<int>();
            rabidDogRadii = new List<float>();
            rabidDogModels = new List<MyModel>();
            rabidDogEntities = new List<Entity>();
            rabidDogHealthDecrs = new List<float>();
            rabidDogBounds = new List<BoundingSphere>();
            bottleModels = new List<MyModel>();
            bottleEntities = new List<Entity>();
            bottleBounds = new List<BoundingSphere>();
            bottleRadii = new List<float>();
            modelEnemyTransforms = new List<Matrix[]>();
            bulletList = new List<Bullet>();
            dogs = new List<AnimationPlayer>();
            dogClips = new List<AnimationClip>();
            rabidDogPreviousPositions = new List<Vector3>();
            for (int i = 0; i < TOTAL_RABID_DOGS; i++)
            {
                rabidDogHealths.Add(100);
                rabidDogHealthDecrs.Add(0.0f);
                modelEnemyTransforms.Add(null);
                rabidDogPreviousPositions.Add(new Vector3());
            }

            healed = new bool[initialNumberOfDogs];
            collected = new bool[initialNumberOfDogs];
            for (int index = 0; index < initialNumberOfDogs; index++)
            {
                healed[index] = false;
                collected[index] = false;
            }

            // Load models and set aspect ratio.
            //playerModel = new MyModel("Models\\ball", content);
            //playerModel.Texture("Textures\\wedge_p1_diff_v1", content);
            LoadAraguz();
            for (int i = 0; i < TOTAL_RABID_DOGS; i++)
            {
                LoadDog();
                //rabidDogModels.Add(new MyModel("Models\\troubled_canine", content));
                //rabidDogModels[i].Texture("Textures\\DogEyes", content);
                //rabidDogModels[i].Texture("Textures\\DogPupil", content);
                //rabidDogModels[i].Texture("Textures\\DogSkin", content);
                bottleModels.Add(new MyModel("Models\\sample_bottle", content));
                bottleModels[i].Texture("Textures\\Chemical1", content);
                bottleModels[i].Texture("Textures\\Chemical2", content);
                bottleModels[i].Texture("Textures\\RedLiquid", content);
                bottleModels[i].Texture("Textures\\TopOfFlask", content);
                bottleModels[i].Texture("Textures\\gray", content);
                //rabidDogModels.Add(new MyModel("Models\\ball", content));
                //rabidDogModels[i].Texture("Textures\\wedge_p1_diff_v1", content);
            }

            sound = content.Load<SoundEffect>("Audio\\Waves\\wind");
            soundInstance = sound.CreateInstance();
            soundInstance.IsLooped.Equals(true);
            soundInstance.Play();

            // Load terrain and sky.
            grassy_level = content.Load<Model>("terrain");
            //content.Load<Texture2D>("Textures\\parkgrass");
            sky = content.Load<Sky>("sky");

            levelModel = new MyModel("Models\\zanzibardesert", content);
            levelModel.Texture("Textures\\desertmud", content);

            // Determine the radius of the player model.           
            BoundingSphere bounds = new BoundingSphere();
            foreach (ModelMesh mesh in playerModel.ModelHeld.Meshes)
                bounds = BoundingSphere.CreateMerged(bounds, mesh.BoundingSphere);
            playerRadius = bounds.Radius;

            playerBounds = bounds;

            BoundingSphere lbounds = new BoundingSphere();
            foreach (ModelMesh mesh in levelModel.ModelHeld.Meshes)
                lbounds = BoundingSphere.CreateMerged(lbounds, mesh.BoundingSphere);
            levelRadius = lbounds.Radius;

            levelBounds = lbounds;

            // Determine the radii of the enemy models.  
            for (int i = 0; i < TOTAL_RABID_DOGS; i++)
            {
                BoundingSphere rbounds = new BoundingSphere();
                foreach (ModelMesh mesh in rabidDogModels[i].ModelHeld.Meshes)
                    rbounds = BoundingSphere.CreateMerged(rbounds, mesh.BoundingSphere);
                rabidDogRadii.Add(rbounds.Radius);

                rabidDogBounds.Add(rbounds);

                BoundingSphere bottlebounds = new BoundingSphere();
                foreach (ModelMesh mesh in bottleModels[i].ModelHeld.Meshes)
                    bottlebounds = BoundingSphere.CreateMerged(bottlebounds, mesh.BoundingSphere);
                bottleRadii.Add(bottlebounds.Radius);

                bottleBounds.Add(bottlebounds);
            }

            // Determine the radius of the height map.           
            BoundingSphere tbounds = new BoundingSphere();
            foreach (ModelMesh mesh in grassy_level.Meshes)
                tbounds = BoundingSphere.CreateMerged(tbounds, mesh.BoundingSphere);
            terrainRadius = tbounds.Radius;

            terrainBounds = tbounds;

            // Setup the camera.
            camera = new ThirdPersonCamera();
            camera.Perspective(CAMERA_FOVX, (float)screenWidth / (float)screenHeight,
                CAMERA_ZNEAR, CAMERA_ZFAR);
            camera.LookAt(new Vector3(0.0f, playerRadius * 3.0f, playerRadius * 7.0f),
                Vector3.Zero, Vector3.Up);

            // Setup the player entity.
            playerEntity = new Entity();
            playerEntity.ConstrainToWorldYAxis = true;
            playerEntity.Position = new Vector3(0.0f, 1.0f + playerRadius, 0.0f);

            levelEntity = new Entity();
            levelEntity.ConstrainToWorldYAxis = true;
            levelEntity.Position = new Vector3(0.0f, 1.0f + levelRadius, 0.0f);

            // Setup the enemy and bottle entities.
            for (int i = 0; i < TOTAL_RABID_DOGS; i++)
            {
                rabidDogEntities.Add(new Entity());
                rabidDogEntities[i].ConstrainToWorldYAxis = true;
                rabidDogEntities[i].Position = new Vector3((random.Next(1000) * 1.0f + 200.0f) + playerEntity.Position.X, 20.0f + rabidDogRadii[i], random.Next(1000));

                bottleEntities.Add(new Entity());
                bottleEntities[i].ConstrainToWorldYAxis = true;
                bottleEntities[i].Position = rabidDogEntities[i].Position;
            }

            // Setup the terrain entity.
            terrainEntity = new Entity();
            terrainEntity.ConstrainToWorldYAxis = true;
            terrainEntity.Position = new Vector3(0.0f, 1.0f + terrainRadius, 0.0f);

            // A real game would probably have more content than this sample, so
            // it would take longer to load. We simulate that by delaying for a
            // while, giving you a chance to admire the beautiful loading screen.
            Thread.Sleep(3000);

            // Once the load has finished, we use ResetElapsedTime to tell the game's
            // timing mechanism that we have just finished a very long frame, and that
            // it should not try to catch up.
            ScreenManager.Game.ResetElapsedTime();
        }

        public override void UnloadContent()
        {
            base.UnloadContent();
        }

        /// <summary>
        /// Allows the game component to update itself.
        /// </summary>
        /// <param name="gameTime">Provides a snapshot of timing values.</param>
        public override void Update(GameTime gameTime, bool otherScreenHasFocus, bool coveredByOtherScreen)
        {
            animationPlayer.Update(gameTime.ElapsedGameTime, true, playerEntity.WorldMatrix);
            for (int i = 0; i < initialNumberOfDogs; i++)
            {
                if (!healed[i])
                    dogs[i].Update(gameTime.ElapsedGameTime, true, rabidDogEntities[i].WorldMatrix);
            }
            base.Update(gameTime, otherScreenHasFocus, false);
            if (IsActive)
            {
                if (dogsinparkInstance.State == SoundState.Playing)
                    dogsinparkInstance.Stop();
                if (telepathInstance.State == SoundState.Playing)
                    telepathInstance.Stop();
                if (virussamplesInstance.State == SoundState.Playing)
                    virussamplesInstance.Stop();
                if (totherescueInstance.State == SoundState.Playing)
                    totherescueInstance.Stop();
            }
            //if (IsActive)
            //{
            //    if (timeLeft == 582)
            //        telepathInstance.Play();
            //    if (timeLeft == 572)
            //        virussamplesInstance.Play();
            //    if (timeLeft == 552)
            //        totherescueInstance.Play();
            //    if ((timeLeft % 12) == 0 && timeLeft >= 0)
            //        if (!barkInstance.IsDisposed && (dogsinparkInstance.State != SoundState.Playing) && (onfireInstance.State != SoundState.Playing) && (virussamplesInstance.State != SoundState.Playing))
            //            barkInstance.Play();
            //    if (timeLeft == 500)
            //        cryInstance.Play();
            //    if (timeLeft == 400)
            //        donotfearInstance.Play();
            //    if (timeLeft == 300)
            //        motherearthInstance.Play();
            //    if (timeLeft == 200)
            //        callmegerryInstance.Play();
            //    if (timeLeft <= 0)
            //        if (!barkInstance.IsDisposed)
            //            barkInstance.Stop();
            //}

            //// Gradually fade in or out depending on whether we are covered by the pause screen.
            //if (coveredByOtherScreen)
            //    pauseAlpha = Math.Min(pauseAlpha + 1f / 32, 1);
            //else
            //    pauseAlpha = Math.Max(pauseAlpha - 1f / 32, 0);

            //if (IsActive)
            //{
            //    float moveSpeed = gameTime.ElapsedGameTime.Milliseconds / 500.0f * bulletSpeed;

            //    ProcessKeyboard();
            //    UpdatePlayer(gameTime);
            //    UpdateEnemies(gameTime);
            //    UpdateFrameRate(gameTime);
            //    UpdateBulletPositions(moveSpeed);
            //    //UpdateBottles(gameTime);

            //    // Rotate triangle level indicator.
            //    angle += 0.005f;

            //    // Apply some random jitter to make the enemy move around.
            //    const float randomization = 10;

            //    enemyPosition.X += (float)(random.NextDouble() - 0.5) * randomization;
            //    enemyPosition.Y += (float)(random.NextDouble() - 0.5) * randomization;

            //    // Apply a stabilizing force to stop the enemy moving off the screen.
            //    Vector2 targetPosition = new Vector2(
            //        ScreenManager.GraphicsDevice.Viewport.Width / 2 - gameFont.MeasureString("").X / 2,
            //        200);

            //    enemyPosition = Vector2.Lerp(enemyPosition, targetPosition, 0.05f);

            //    base.Update(gameTime, otherScreenHasFocus, false);

            //    // TODO: this game isn't very fun! You could probably improve
            //    // it by inserting something more interesting in this space :-)
            //}
        }

        public override void HandleInput(InputState input)
        {
            base.HandleInput(input);
        }

        public override void Draw(GameTime gameTime)
        {
            ScreenManager.GraphicsDevice.Clear(Color.Black);

            ScreenManager.GraphicsDevice.BlendState = BlendState.Opaque;
            ScreenManager.GraphicsDevice.DepthStencilState = DepthStencilState.Default;
            ScreenManager.GraphicsDevice.SamplerStates[0] = SamplerState.LinearWrap;

            // Draw the terrain first, then the sky. This is faster than
            // drawing the sky first, because the depth buffer can skip
            // bothering to draw sky pixels that are covered up by the
            // terrain. This trick works because the code used to draw
            // the sky forces all the sky vertices to be as far away as
            // possible, and turns depth testing on but depth writes off.

            //DrawPlayer();

            this.DrawEnemies();

            this.DrawAraguz();

            this.DrawLevel();

            this.DrawBullets();

            this.DrawBottles();

            this.DrawIndicator();

            this.sky.Draw(camera.ViewMatrix, camera.ProjectionMatrix);

            this.DrawTerrain(camera.ViewMatrix, camera.ProjectionMatrix);

            this.DrawText();

            // If there was any alpha blended translucent geometry in
            // the scene, that would be drawn here, after the sky.

            mBatch.Begin(0, BlendState.AlphaBlend);

            //Draw the negative space for the health bar.
            mBatch.Draw(mHealthBar, new Rectangle(ScreenManager.Game.GraphicsDevice.Viewport.Width - mHealthBar.Width - 30, 30, mHealthBar.Width, 25), new Rectangle(0, 45, mHealthBar.Width, 25), Color.Gray);

            //for (int index = 0; index < initialNumberOfDogs; index++)
            //{
            //    if (!healed[index])
            //            mBatch.Draw(mHealthBar, new Rectangle(ScreenManager.Game.GraphicsDevice.Viewport.Width - mHealthBar.Width - 30, 30, mHealthBar.Width, 25), new Rectangle(0, 45, mHealthBar.Width, 25), Color.Gray);
            //}

            // Draw the current health for the health bar.
            if (mCurrentHealth > 50)
            {
                mBatch.Draw(mHealthBar, new Rectangle(ScreenManager.Game.GraphicsDevice.Viewport.Width - mHealthBar.Width - 30, 30, (int)(mHealthBar.Width * ((double)mCurrentHealth / 100)), 25), new Rectangle(0, 45, mHealthBar.Width, 25), Color.DarkRed);
            }

            else if ((mCurrentHealth <= 50) && (mCurrentHealth > 25))
            {
                mBatch.Draw(mHealthBar, new Rectangle(ScreenManager.Game.GraphicsDevice.Viewport.Width - mHealthBar.Width - 30, 30, (int)(mHealthBar.Width * ((double)mCurrentHealth / 100)), 25), new Rectangle(0, 45, mHealthBar.Width, 25), Color.Red);
            }
            else
            {
                if (flicker == true)
                {
                    mBatch.Draw(mHealthBar, new Rectangle(ScreenManager.Game.GraphicsDevice.Viewport.Width - mHealthBar.Width - 30, 30, (int)(mHealthBar.Width * ((double)mCurrentHealth / 100)), 25), new Rectangle(0, 45, mHealthBar.Width, 25), Color.Red);
                    prevElapsedTime = elapsedTime;
                    flicker = false;
                }
                else if ((flicker == false) && (elapsedTime != prevElapsedTime))
                {
                    mBatch.Draw(mHealthBar, new Rectangle(ScreenManager.Game.GraphicsDevice.Viewport.Width - mHealthBar.Width - 30, 30, (int)(mHealthBar.Width * ((double)mCurrentHealth / 100)), 25), new Rectangle(0, 45, mHealthBar.Width, 25), Color.Transparent);
                    flicker = true;
                }
            }

            //for (int index = 0; index < initialNumberOfDogs; index++)
            //{
            //    if (!healed[index])
            //    {
            //        if (rabidDogHealths[index] > 50)
            //            mBatch.Draw(mHealthBar, new Rectangle((int)rabidDogEntities[index].Position.X - 30, (int)rabidDogEntities[index].Position.Z - 30, (int)(mHealthBar.Width * ((double)mCurrentHealth / 100)), 25), new Rectangle(0, 45, mHealthBar.Width, 25), Color.DarkRed);
            //        else if ((rabidDogHealths[index] <= 50) && (rabidDogHealths[index] > 25))
            //            mBatch.Draw(mHealthBar, new Rectangle((int)rabidDogEntities[index].Position.X - 30, (int)rabidDogEntities[index].Position.Z - 30, (int)(mHealthBar.Width * ((double)mCurrentHealth / 100)), 25), new Rectangle(0, 45, mHealthBar.Width, 25), Color.Red);
            //    }

            //}

            // Draw the box around the health bar.
            mBatch.Draw(mHealthBar, new Rectangle(ScreenManager.Game.GraphicsDevice.Viewport.Width - mHealthBar.Width - 30, 30, mHealthBar.Width, 25), new Rectangle(0, 0, mHealthBar.Width, 25), Color.White);

            mBatch.End();

            // Our player and enemy are both actually just text strings.
            SpriteBatch spriteBatchAlpha = ScreenManager.SpriteBatch;

            spriteBatchAlpha.Begin(0, BlendState.AlphaBlend);

            //spriteBatchAlpha.DrawString(gameFont, "// TODO", playerPosition, Color.Green);

            //spriteBatchAlpha.DrawString(gameFont, "Insert Gameplay Here", enemyPosition, Color.DarkRed);

            // Draw timer text.
            string secs = (timeLeft % 60).ToString();
            if ((timeLeft % 60) < 10)
            {
                secs = "0" + secs;
            }
            Color timeLeftColor = Color.White;
            if (timeLeft < 60)
            {
                timeLeftColor = Color.Red;
            }
            spriteBatchAlpha.DrawString(gameFont, (timeLeft / 60) + ":" + secs, new Vector2(screenWidth / 2 - 45, 10), timeLeftColor);

            spriteBatchAlpha.End();
            base.Draw(gameTime);

            IncrementFrameCounter(); // Increment counter for frames per second.

            // If the game is transitioning on or off, fade it out to black.
            if (TransitionPosition > 0 || pauseAlpha > 0)
            {
                float alpha = MathHelper.Lerp(1f - TransitionAlpha, 1f, pauseAlpha / 2);

                ScreenManager.FadeBackBufferToBlack(alpha);
            }
        }

        protected override void UpdateFrameRate(GameTime gameTime)
        {
            elapsedTime += gameTime.ElapsedGameTime;

            // Update Frame Rate
            if (elapsedTime > TimeSpan.FromSeconds(1))
            {
                elapsedTime -= TimeSpan.FromSeconds(1);
                framesPerSecond = frames;
                frames = 0;
            }


            elapsedUpdateTime += gameTime.ElapsedGameTime.Milliseconds;

            // Update Time Left
            if (elapsedUpdateTime >= 1000)
            {
                timeLeft--;
                elapsedUpdateTime = 0; // reset counter
                if (timeLeft <= 0)
                {
                    // Game is over, so go to continue or quit screen.
                    jacksonwincryInstance.Play();
                    barkInstance.Stop();
                    ScreenManager.AddScreen(new GameOverLevel2Screen(), ControllingPlayer);
                }
            }
        }

        protected override void UpdatePlayer(GameTime gameTime)
        {
            if (timeLeft == 600)
                onfireInstance.Play();
            float pitch = 0.0f;
            float heading = 0.0f;
            float forwardSpeed = 0.0f;
            Vector3 playerPrevPosition = playerEntity.Position;
            curMouseState = new MouseState();

            if (curKeyboardState.IsKeyDown(Keys.W) ||
                curKeyboardState.IsKeyDown(Keys.Up))
            {
                forwardSpeed = PLAYER_FORWARD_SPEED;
                pitch = -PLAYER_ROLLING_SPEED;
            }

            if (curKeyboardState.IsKeyDown(Keys.S) ||
                curKeyboardState.IsKeyDown(Keys.Down))
            {
                forwardSpeed = -PLAYER_FORWARD_SPEED;
                pitch = PLAYER_ROLLING_SPEED;
            }

            if (curKeyboardState.IsKeyDown(Keys.D) ||
                curKeyboardState.IsKeyDown(Keys.Right))
            {
                heading = -PLAYER_HEADING_SPEED;
            }

            if (curKeyboardState.IsKeyDown(Keys.A) ||
                curKeyboardState.IsKeyDown(Keys.Left))
            {
                heading = PLAYER_HEADING_SPEED;
            }

            if (((curMouseState.LeftButton == ButtonState.Pressed) && (prevMouseState.LeftButton == ButtonState.Released)) || curKeyboardState.IsKeyDown(Keys.RightControl))
            {
                double currentTime = gameTime.TotalGameTime.TotalMilliseconds;
                if ((currentTime - lastBulletTime) > 100)
                {
                    Bullet newBullet = new Bullet();
                    newBullet.position = playerEntity.Position;
                    newBullet.rotation = playerEntity.Rotation;
                    bulletList.Add(newBullet);

                    lastBulletTime = currentTime;
                }
            }
            prevMouseState = curMouseState;

            // Prevent the player from moving off the edge of the floor.
            float floorBoundaryZ = TERRAIN_HEIGHT * 0.5f - playerRadius;
            float floorBoundaryX = TERRAIN_WIDTH * 0.5f - playerRadius;
            float elapsedTimeSec = (float)gameTime.ElapsedGameTime.TotalSeconds;
            float velocity = forwardSpeed * elapsedTimeSec;
            Vector3 newPlayerPos = playerEntity.Position + playerEntity.Forward * velocity;

            if (newPlayerPos.Z > floorBoundaryZ)
                forwardSpeed = 0.0f;
            else if (newPlayerPos.Z < -floorBoundaryZ)
                forwardSpeed = 0.0f;
            else if (newPlayerPos.X > floorBoundaryX)
                forwardSpeed = 0.0f;
            else if (newPlayerPos.X < -floorBoundaryX)
                forwardSpeed = 0.0f;

            // Update the player's state.
            playerEntity.Velocity = new Vector3(0.0f, 0.0f, forwardSpeed);
            playerEntity.Orient(heading, 0.0f, 0.0f);
            //playerEntity.Rotate(0.0f, pitch, 0.0f);
            playerEntity.Update(gameTime);

            // Then move the camera based on where the player has moved to.
            // When the player is moving backwards rotations are inverted to
            // match the direction of travel. Consequently the camera's
            // rotation needs to be inverted as well.

            Vector3 camup = new Vector3(0, 1, 0);
            camup = Vector3.Transform(camup, Matrix.CreateFromQuaternion(playerEntity.Rotation));
            cameraUpDirection = camup;
            camera.Rotate((forwardSpeed >= 0.0f) ? heading : -heading, 0.0f);
            camera.LookAt(playerEntity.Position);
            camera.Update(gameTime);

            bool enemiesDeadChk = true;
            // keep player from colliding with enemies and decrease health if collision
            for (int k = 0; k < initialNumberOfDogs; k++)
            {
                if (!healed[k])
                {
                    if ((((Math.Abs(playerEntity.Position.X - rabidDogEntities[k].Position.X) + Math.Abs(playerEntity.Position.Z - rabidDogEntities[k].Position.Z)) / 2) < (playerRadius + rabidDogRadii[k] - 19.5)))
                    {
                        playerEntity.Position = playerPrevPosition;
                        playerHealthDecr += 0.1f; // add up damage to player
                        if (playerHealthDecr >= 1.0f)
                        {
                            mCurrentHealth -= 1; // decrease player health by 1 unit
                            //Force the health to remain between 0 and 100.           
                            mCurrentHealth = (int)MathHelper.Clamp(mCurrentHealth, 0, 100);
                            playerHealthDecr = 0.0f;
                        }
                    }
                    if ((enemiesDeadChk == true) && (rabidDogHealths[k] > 0))
                    {
                        enemiesDeadChk = false;
                    }
                }
            }
            //detects collision with disease sample, represented by a set of bottles
            for (int index = 0; index < bottleEntities.Count; index++)
            {
                if ((((Math.Abs(playerEntity.Position.X - bottleEntities[index].Position.X) + Math.Abs(playerEntity.Position.Z - bottleEntities[index].Position.Z)) / 2) < (playerRadius + bottleRadii[index] - 19.5)))
                {
                    if (!collected[index])
                    {
                        collected[index] = true;
                        numberOfCollectedSamples++;
                        collectsampleInstance.Play();
                    }
                }
            }

            if ((enemiesDeadChk == true) && (numberOfCollectedSamples == initialNumberOfDogs) && (((Math.Abs(playerEntity.Position.X - indicatorPos.X) + Math.Abs(playerEntity.Position.Z - indicatorPos.Z)) / 2) < (playerRadius + indicatorScale * 2 - 19.5)))
            {
                // player can advance to next level
                soundInstance.Stop();
                winInstance.Play();
                jacksonlosecryInstance.Play();
                barkInstance.Stop();
                ScreenManager.AddScreen(new NextLevelScreen(), ControllingPlayer);
            }

            // Test current health bar.

            // If Page Up is pressed, increase the health bar.
            if (curKeyboardState.IsKeyDown(Keys.PageUp) == true)
            {
                mCurrentHealth += 1;
            }
            // If Page Down is pressed, decrease the health bar.
            if (curKeyboardState.IsKeyDown(Keys.PageDown) == true)
            {
                mCurrentHealth -= 1;
            }
            //Force the health to remain between 0 and 100.           
            mCurrentHealth = (int)MathHelper.Clamp(mCurrentHealth, 0, 100);

            if (mCurrentHealth == 0)
            {
                // Game is over, so go to continue or quit screen.
                jacksonwincryInstance.Play();
                barkInstance.Stop();
                ScreenManager.AddScreen(new GameOverLevel2Screen(), ControllingPlayer);
            }
        }

        protected override void DrawText()
        {
            StringBuilder buffer = new StringBuilder();
            string jacksonTaunt = "";

            if (displayHelp)
            {
                buffer.AppendLine("Press W or UP to move the player forwards");
                buffer.AppendLine("Press S or DOWN to roll the player backwards");
                buffer.AppendLine("Press D or RIGHT to turn the player to the right");
                buffer.AppendLine("Press A or LEFT to turn the player to the left");
                buffer.AppendLine();
                buffer.AppendLine("Press RIGHT CONTROL to shoot player weaponry.");
                buffer.AppendLine("Press PAGEUP and PAGEDOWN to change the player's health bar.");
                buffer.AppendLine("Press SPACE to enable and disable the camera's spring system");
                buffer.AppendLine("Press + and - to change the camera's spring constant");
                buffer.AppendLine("Press ALT and ENTER to toggle full screen");
                buffer.AppendLine("Press ESCAPE to exit");
                buffer.AppendLine();
                buffer.AppendLine("Press H to hide help");
            }
            else
            {
                buffer.AppendFormat("FPS: {0}\n", framesPerSecond);

                bool springOn = camera.EnableSpringSystem;
                float springConstant = camera.SpringConstant;
                float dampingConstant = camera.DampingConstant;

                buffer.AppendLine();
                buffer.AppendLine("Camera");
                buffer.AppendFormat("  Spring {0}\n", (springOn ? "enabled" : "disabled"));
                buffer.AppendFormat("  Spring constant: {0}\n", springConstant.ToString("f2"));
                buffer.AppendFormat("  Damping constant: {0}\n", dampingConstant.ToString("f2"));
                buffer.AppendLine();
                if (timeLeft <= 300 && timeLeft >= 240)
                    jacksonTaunt = "Merry Gerry - prepare to be MAULED by twice as many dogs! Get him, boys!\n\n";
                if (timeLeft < 240 && timeLeft >= 180)
                    jacksonTaunt = "So you survived for one minute. Big wuff. Speaking of wuff - get him, boys!\n\n";
                if (timeLeft < 180 && timeLeft >= 120)
                    jacksonTaunt = "Two minutes? Purrrrrrlease. Speaking of purr, pretend Gerry's a cat and get him, boys!!\n\n";
                if (timeLeft < 120 && timeLeft >= 60)
                    jacksonTaunt = "Geraldo Araguz, you survived for more than half the time. You got some bones.\nSpeaking of bones, pretend he's one and get him, boys!!\n\n";
                if (timeLeft < 60 && timeLeft >= 0)
                    jacksonTaunt = "One minute to lose, Geraldo Araguz! You still have a chance to get him, boys!!\n\n";
                if (timeLeft <= 0)
                    jacksonTaunt = "NOOOOOOOOOOOOOOOOOOOOOO!\n\n";
                buffer.AppendFormat(jacksonTaunt);
                buffer.AppendLine("Press H to display help");
            }

            spriteBatch.Begin(SpriteSortMode.Deferred, BlendState.AlphaBlend);
            spriteBatch.DrawString(spriteFont, buffer.ToString(), fontPos, Color.Yellow);
            spriteBatch.End();
        }
    }
}
