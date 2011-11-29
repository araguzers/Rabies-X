#region File Description
//-----------------------------------------------------------------------------
// GameplayScreen.cs
//
// Microsoft XNA Community Game Platform
// Copyright (C) Microsoft Corporation. All rights reserved.
//-----------------------------------------------------------------------------
#endregion

//Joe Eid: voicing Russell Jackson
//Danny Neumann: voicing male government agent
//Sassa: voicing female government agent (the "be careful" phrase).

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
    /// This screen implements the actual game logic. It is just a
    /// placeholder to get the idea across: you'll probably want to
    /// put some more interesting gameplay in here!
    /// </summary>
    /// 
    class GameplayScreen : GameScreen
    {
        #region Fields

        protected const float PLAYER_FORWARD_SPEED = 120.0f;
        protected const float PLAYER_HEADING_SPEED = 120.0f;
        protected const float PLAYER_ROLLING_SPEED = 280.0f;

        protected const float TERRAIN_WIDTH = 3500.0f;
        protected const float TERRAIN_HEIGHT = 3500.0f;

        protected const float CAMERA_FOVX = 80.0f;
        protected const float CAMERA_ZFAR = TERRAIN_WIDTH * 2.0f;
        protected const float CAMERA_ZNEAR = 1.0f;
        protected const float CAMERA_MAX_SPRING_CONSTANT = 100.0f;
        protected const float CAMERA_MIN_SPRING_CONSTANT = 1.0f;

        protected const int MAX_TIME_LEFT = 300;
        protected int timeLeft = MAX_TIME_LEFT;
        protected int elapsedUpdateTime = 0;

        protected int level;

        protected int TOTAL_RABID_DOGS = 10;

        protected int initialNumberOfDogs;

        protected AnimationPlayer animationPlayer;

        protected ContentManager content;
        protected SpriteFont gameFont;

        //Sets the sounds.
        protected SoundEffect sound;
        protected SoundEffectInstance soundInstance;

        protected SoundEffect cry;
        protected SoundEffectInstance cryInstance;

        protected SoundEffect win;
        protected SoundEffectInstance winInstance;

        protected SoundEffect bark;
        protected SoundEffectInstance barkInstance;

        protected SoundEffect gameMusic;
        protected SoundEffectInstance gameMusicInstance;

        protected SoundEffect donotfear;
        protected SoundEffectInstance donotfearInstance;

        protected SoundEffect motherearth;
        protected SoundEffectInstance motherearthInstance;

        protected SoundEffect callmegerry;
        protected SoundEffectInstance callmegerryInstance;

        protected SoundEffect jacksonwincry;
        protected SoundEffectInstance jacksonwincryInstance;

        protected SoundEffect jacksonlosecry;
        protected SoundEffectInstance jacksonlosecryInstance;

        protected SoundEffect taunt;
        protected SoundEffectInstance tauntInstance;

        protected SoundEffect becareful;
        protected SoundEffectInstance becarefulInstance;

        protected SoundEffect dogsinpark;
        protected SoundEffectInstance dogsinparkInstance;

        protected SoundEffect plasmaray;
        protected SoundEffectInstance plasmarayInstance;

        protected SoundEffect totherescue;
        protected SoundEffectInstance totherescueInstance;

        protected SoundEffect telepath;
        protected SoundEffectInstance telepathInstance;

        protected SoundEffect collectsample;
        protected SoundEffectInstance collectsampleInstance;

        protected SpriteBatch spriteBatch;
        protected SpriteFont spriteFont;

        protected Vector2 playerPosition = new Vector2(100, 100);
        protected Vector2 enemyPosition = new Vector2(100, 100);

        // Set triangle indicator for level.
        protected Effect effect;
        protected Vector3 indicatorPos;
        protected int indicatorScale = 5;
        protected List<VertexPositionColor[]> vertices;

        protected float angle = 0.0f;

        protected int numberOfCollectedSamples;

        // Set bullets for level;
        protected Quad quad;
        protected Effect quadEffect;
        protected struct Bullet
        {
            public Vector3 position;
            public Quaternion rotation;
        }
        protected List<Bullet> bulletList;
        protected double lastBulletTime = 0;
        protected float bulletSpeed = 1.0f;
        protected Texture2D bullets;

        // Set health bar for level.
        protected SpriteBatch mBatch;
        protected Texture2D mHealthBar;

        // Set current health for level.
        protected int mCurrentHealth = 100;
        protected float playerHealthDecr = 0.0f;
        
        // Set sky and terrain for level.
        protected Model grassy_level;
        protected Sky sky;

        // Set the 3D model to draw.
        protected MyModel playerModel;
        protected List<MyModel> rabidDogModels;

        // Aspect ratio determines how to scale 3d to 2d projection.
        protected float aspectRatio;

        protected AnimationClip clip;
        protected Model dogModel;
        protected Model araguzModel;
        protected MyModel levelModel;

        protected Random random = new Random();

        protected float pauseAlpha;

        protected int screenWidth;
        protected int screenHeight;
        
        protected Vector2 fontPos;
        protected int frames;
        protected int framesPerSecond;
        protected Entity playerEntity;
        protected Entity terrainEntity;
        protected Entity levelEntity;
        protected List<int> rabidDogHealths;
        protected List<float> rabidDogHealthDecrs;
        protected List<Entity> rabidDogEntities;
        protected List<Entity> bottleEntities;
        protected List<MyModel> bottleModels;
        protected List<Vector3> rabidDogPreviousPositions;
        protected float playerRadius;
        protected float terrainRadius;
        protected float levelRadius;
        protected List<float> rabidDogRadii;
        protected List<float> bottleRadii;
        protected Matrix[] modelTransforms;
        protected List<Matrix[]> modelEnemyTransforms;
        protected bool[] healed;
        protected bool[] collected;
        protected TimeSpan elapsedTime = TimeSpan.Zero;
        protected TimeSpan prevElapsedTime = TimeSpan.Zero;
        protected bool displayHelp;

        protected ThirdPersonCamera camera;
        protected Vector3 cameraUpDirection;

        protected MouseState curMouseState;
        protected MouseState prevMouseState;
        protected KeyboardState curKeyboardState;
        protected KeyboardState prevKeyboardState;

        protected BoundingSphere playerBounds;
        protected BoundingSphere terrainBounds;
        protected BoundingSphere levelBounds;
        protected List<BoundingSphere> rabidDogBounds;
        protected List<BoundingSphere> bottleBounds;

        protected bool flicker;

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

            totherescue = content.Load<SoundEffect>("Audio\\Waves\\totherescue");
            totherescueInstance = totherescue.CreateInstance();

            telepath = content.Load<SoundEffect>("Audio\\Waves\\telepath");
            telepathInstance = telepath.CreateInstance();

            collectsample = content.Load<SoundEffect>("Audio\\Waves\\collectsample");
            collectsampleInstance = collectsample.CreateInstance();

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
                rabidDogModels.Add(new MyModel("Models\\troubled_canine", content));
                rabidDogModels[i].Texture("Textures\\DogEyes", content);
                rabidDogModels[i].Texture("Textures\\DogPupil", content);
                rabidDogModels[i].Texture("Textures\\DogSkin", content);
                bottleModels.Add(new MyModel("Models\\sample_bottle", content));
                bottleModels[i].Texture("Textures\\Chemical1", content);
                bottleModels[i].Texture("Textures\\Chemical2", content);
                bottleModels[i].Texture("Textures\\RedLiquid", content);
                bottleModels[i].Texture("Textures\\TopOfFlask", content);
                bottleModels[i].Texture("Textures\\gray", content);
                //rabidDogModels.Add(new MyModel("Models\\ball", content));
                //rabidDogModels[i].Texture("Textures\\wedge_p1_diff_v1", content);
            }

            sound = content.Load<SoundEffect>("Audio\\Waves\\clockisticking");
            soundInstance = sound.CreateInstance();
            soundInstance.IsLooped.Equals(true);
            soundInstance.Play();

            // Load terrain and sky.
            grassy_level = content.Load<Model>("terrain");
            //content.Load<Texture2D>("Textures\\parkgrass");
            sky = content.Load<Sky>("sky");

            levelModel = new MyModel("Models\\grassy_level", content);
            levelModel.Texture("Textures\\parkgrass", content);

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
                rabidDogEntities[i].Position = new Vector3(random.Next(1000) * -1.0f + 200.0f, 20.0f + rabidDogRadii[i], random.Next(1000));

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


        /// <summary>
        /// Unload graphics content used by the game.
        /// </summary>
        public override void UnloadContent()
        {
            content.Unload();
        }

        protected void SetUpVertices(Vector3 pos, int scale)
        {
            Vector3 vertex1 = new Vector3(pos.X - scale, pos.Y - scale, pos.Z + 2*scale);
            Vector3 vertex2 = new Vector3(pos.X + scale, pos.Y + scale, pos.Z);
            Vector3 vertex3 = new Vector3(pos.X + scale, pos.Y - scale, pos.Z + 2*scale);
            Vector3 vertex4 = new Vector3(pos.X - scale, pos.Y - scale, pos.Z);

            vertices.Add(new VertexPositionColor[3]); // front face
            vertices[0][0] = new VertexPositionColor(vertex1, Color.Blue); // bottom-left
            vertices[0][1] = new VertexPositionColor(vertex2, Color.Red); // top vertex
            vertices[0][2] = new VertexPositionColor(vertex3, Color.Green); // bottom-right

            vertices.Add(new VertexPositionColor[3]); // left face
            vertices[1][0] = new VertexPositionColor(vertex4, Color.Blue); // bottom-left
            vertices[1][1] = new VertexPositionColor(vertex2, Color.Red); // top vertex
            vertices[1][2] = new VertexPositionColor(vertex1, Color.Green); // bottom-right

            vertices.Add(new VertexPositionColor[3]); // right face
            vertices[2][0] = new VertexPositionColor(vertex3, Color.Blue); // bottom-left
            vertices[2][1] = new VertexPositionColor(vertex2, Color.Red); // top vertex
            vertices[2][2] = new VertexPositionColor(vertex4, Color.Green); // bottom-right

            vertices.Add(new VertexPositionColor[3]); // bottom face
            vertices[3][0] = new VertexPositionColor(vertex1, Color.Blue); // bottom-left
            vertices[3][1] = new VertexPositionColor(vertex4, Color.Red); // top vertex
            vertices[3][2] = new VertexPositionColor(vertex3, Color.Green); // bottom-right
        }

        #endregion

        protected void LoadAraguz()
        {
            playerModel = new MyModel("Models\\geraldo_araguz", content);

            // Look up our custom skinning information.
            SkinningData skinningData = playerModel.ModelHeld.Tag as SkinningData;

            if (skinningData == null)
                throw new InvalidOperationException
                    ("This model does not contain a SkinningData tag.");

            // Create an animation player, and start decoding an animation clip.
            animationPlayer = new AnimationPlayer(skinningData);

            AnimationClip clip = skinningData.AnimationClips["Take 001"];

            animationPlayer.StartClip(clip);
        }

        protected void LoadDog()
        {
            dogModel = content.Load<Model>("sick_dog");

            // Look up our custom skinning information.
            SkinningData skinningData = dogModel.Tag as SkinningData;

            if (skinningData == null)
                throw new InvalidOperationException
                    ("This model does not contain a SkinningData tag.");

            // Create an animation player, and start decoding an animation clip.
            AnimationPlayer animationPlayer = new AnimationPlayer(skinningData);

            AnimationClip clip = skinningData.AnimationClips["Take 001"];

            animationPlayer.StartClip(clip);
        }

        #region Update and Draw


        /// <summary>
        /// Updates the state of the game. This method checks the GameScreen.IsActive
        /// property, so the game will stop updating when the pause menu is active,
        /// or if you tab away to a different application.
        /// </summary>
        public override void Update(GameTime gameTime, bool otherScreenHasFocus,
                                                       bool coveredByOtherScreen)
        {
            animationPlayer.Update(gameTime.ElapsedGameTime, true, playerEntity.WorldMatrix);
            base.Update(gameTime, otherScreenHasFocus, false);
            if (IsActive)
            {
                if (timeLeft == 300)
                    dogsinparkInstance.Play();
                if (timeLeft == 282)
                    telepathInstance.Play();
                if (timeLeft == 270)
                    becareful.Play();
                if (timeLeft == 268)
                    totherescueInstance.Play();
                if ((timeLeft % 12) == 0 && timeLeft >= 0)
                    if (!barkInstance.IsDisposed && (dogsinparkInstance.State != SoundState.Playing))
                        barkInstance.Play();
                if (timeLeft == 240)
                    cryInstance.Play();
                if (timeLeft == 180)
                    donotfearInstance.Play();
                if (timeLeft == 120)
                    motherearthInstance.Play();
                if (timeLeft == 60)
                    callmegerryInstance.Play();
                if (timeLeft <= 0)
                    if (!barkInstance.IsDisposed)
                        barkInstance.Stop();
            }

            // Gradually fade in or out depending on whether we are covered by the pause screen.
            if (coveredByOtherScreen)
                pauseAlpha = Math.Min(pauseAlpha + 1f / 32, 1);
            else
                pauseAlpha = Math.Max(pauseAlpha - 1f / 32, 0);

            if (IsActive)
            {
                float moveSpeed = gameTime.ElapsedGameTime.Milliseconds / 500.0f * bulletSpeed;

                ProcessKeyboard();
                UpdatePlayer(gameTime);
                UpdateEnemies(gameTime);
                UpdateFrameRate(gameTime);
                UpdateBulletPositions(moveSpeed);
                //UpdateBottles(gameTime);

                // Rotate triangle level indicator.
                angle += 0.005f;

                // Apply some random jitter to make the enemy move around.
                const float randomization = 10;

                enemyPosition.X += (float)(random.NextDouble() - 0.5) * randomization;
                enemyPosition.Y += (float)(random.NextDouble() - 0.5) * randomization;

                // Apply a stabilizing force to stop the enemy moving off the screen.
                Vector2 targetPosition = new Vector2(
                    ScreenManager.GraphicsDevice.Viewport.Width / 2 - gameFont.MeasureString("").X / 2, 
                    200);

                enemyPosition = Vector2.Lerp(enemyPosition, targetPosition, 0.05f);
                
                base.Update(gameTime, otherScreenHasFocus, false);

                // TODO: this game isn't very fun! You could probably improve
                // it by inserting something more interesting in this space :-)
            }
        }
        
        protected bool KeyJustPressed(Keys key)
        {
            return curKeyboardState.IsKeyDown(key) && prevKeyboardState.IsKeyUp(key);
        }

        protected void ProcessKeyboard()
        {
            prevKeyboardState = curKeyboardState;
            curKeyboardState = Keyboard.GetState();
            
            if (KeyJustPressed(Keys.H))
                displayHelp = !displayHelp;

            if (KeyJustPressed(Keys.Space))
                camera.EnableSpringSystem = !camera.EnableSpringSystem;

            if (curKeyboardState.IsKeyDown(Keys.LeftAlt) ||
                curKeyboardState.IsKeyDown(Keys.RightAlt))
            {
                if (KeyJustPressed(Keys.Enter))
                    ToggleFullScreen();
            }

            if (KeyJustPressed(Keys.Add))
            {
                float springConstant = camera.SpringConstant + 0.1f;

                springConstant = Math.Min(CAMERA_MAX_SPRING_CONSTANT, springConstant);
                camera.SpringConstant = springConstant;
            }

            if (KeyJustPressed(Keys.Subtract))
            {
                float springConstant = camera.SpringConstant - 0.1f;

                springConstant = Math.Max(CAMERA_MIN_SPRING_CONSTANT, springConstant);
                camera.SpringConstant = springConstant;
            }
        }

        protected void ToggleFullScreen()
        {
            int newWidth = 0;
            int newHeight = 0;

            GameStateManagementGame.graphics.IsFullScreen = !GameStateManagementGame.graphics.IsFullScreen;

            if (GameStateManagementGame.graphics.IsFullScreen)
            {
                newWidth = ScreenManager.GraphicsDevice.DisplayMode.Width;
                newHeight = ScreenManager.GraphicsDevice.DisplayMode.Height;
            }
            else
            {
                newWidth = screenWidth;
                newHeight = screenHeight;
            }

            GameStateManagementGame.graphics.PreferredBackBufferWidth = newWidth;
            GameStateManagementGame.graphics.PreferredBackBufferHeight = newHeight;
            GameStateManagementGame.graphics.ApplyChanges();

            camera.Perspective(CAMERA_FOVX, (float)newWidth / (float)newHeight,
                CAMERA_ZNEAR, CAMERA_ZFAR);

            // Set current screen width and height.
            screenWidth = newWidth;
            screenHeight = newHeight;
        }

        protected virtual void UpdatePlayer(GameTime gameTime)
        {
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
                if ((currentTime - lastBulletTime) > 0)
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
                    if(!collected[index])
                    {
                        collected[index] = true;
                        numberOfCollectedSamples++;
                        collectsampleInstance.Play();
                    }
                }
            }

            if ((enemiesDeadChk == true) && (numberOfCollectedSamples == initialNumberOfDogs) && (((Math.Abs(playerEntity.Position.X - indicatorPos.X) + Math.Abs(playerEntity.Position.Z - indicatorPos.Z)) / 2) < (playerRadius + indicatorScale*2 - 19.5)))
            {
                // player can advance to next level
                soundInstance.Stop();
                winInstance.Play();
                jacksonlosecryInstance.Play();
                if (!barkInstance.IsDisposed)
                    barkInstance.Dispose();
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
                if(!barkInstance.IsDisposed)
                     barkInstance.Dispose();
                ScreenManager.AddScreen(new GameOverScreen(), ControllingPlayer);
            }
        }

        protected void MoveForward(ref Vector3 position, Quaternion rotationQuat, float speed)
        {
            Vector3 addVector = Vector3.Transform(new Vector3(0.0f, /*32*/-0.50f, 1.0f), rotationQuat);
            position += addVector * (speed * 10);
        }
     
        protected void UpdateBulletPositions(float moveSpeed)
        {
            for (int i = 0; i < bulletList.Count; i++)
            {
                Bullet currentBullet = bulletList[i];
                MoveForward(ref currentBullet.position, currentBullet.rotation, moveSpeed * 2.0f);
                bulletList[i] = currentBullet;

                // check if bullet hits an enemy and decrease the enemy's health
                BoundingSphere bulletSphere = new BoundingSphere(currentBullet.position, 0.05f);
                for (int j = 0; j < initialNumberOfDogs; j++)
                {
                    if (!healed[j])
                    {
                        BoundingSphere enemySphere = new BoundingSphere(rabidDogEntities[j].Position, rabidDogModels[j].ModelHeld.Meshes[0].BoundingSphere.Radius);
                        if (bulletSphere.Intersects(enemySphere))
                        {
                            rabidDogEntities[j].Position = rabidDogPreviousPositions[j];
                            bottleEntities[j].Position = rabidDogPreviousPositions[j];

                            bulletList.RemoveAt(i);
                            i--;

                            rabidDogHealthDecrs[j] += 0.5f; // add up damage to enemy
                            if (rabidDogHealthDecrs[j] >= 1.0f)
                            {
                                rabidDogHealths[j] -= 10; // decrease enemy health by 1 unit
                                //Force the health to remain between 0 and 100.           
                                rabidDogHealthDecrs[j] = 0.0f;
                            }
                            // check enemy health and destroy enemy if no health left
                            if (rabidDogHealths[j] <= 0)
                            {
                                healed[j] = true;
                                TOTAL_RABID_DOGS--;
                                if (TOTAL_RABID_DOGS == 0)
                                {
                                    if (!barkInstance.IsDisposed)
                                        barkInstance.Dispose();
                                }
                                //CreateBottle(currentPosition);
                                break;
                            }
                        }
                    }
            }
            if (TOTAL_RABID_DOGS == 0 && numberOfCollectedSamples == initialNumberOfDogs)
            {
                soundInstance.Stop();
                winInstance.Play();
                jacksonlosecryInstance.Play();
                if (!barkInstance.IsDisposed)
                    barkInstance.Dispose();
                ScreenManager.AddScreen(new NextLevelScreen(), ControllingPlayer);
            }
        }
        }

        //protected void UpdateBottles(GameTime gameTime)
        //{
        //    for (int i = 0; i < bottleEntities.Count; i++)
        //    {
        //        if ((((Math.Abs(bottleEntities[i].Position.X - playerEntity.Position.X) + Math.Abs(bottleEntities[i].Position.Z - playerEntity.Position.Z)) / 2) < (bottleRadii[i] + playerRadius - 2.0)))
        //        {
        //            bottleRadii.RemoveAt(i);
        //            bottleModels.RemoveAt(i);
        //            bottleBounds.RemoveAt(i);
        //            bottleEntities.RemoveAt(i);
        //        }
        //    }
        //}

        protected void UpdateEnemies(GameTime gameTime)
        {
            for (int i = 0; i < initialNumberOfDogs; i++)
            {
                if (!healed[i])
                {
                    rabidDogPreviousPositions[i] = rabidDogEntities[i].Position;
                    double xDiff = rabidDogEntities[i].Position.X - playerEntity.Position.X;
                    double zDiff = rabidDogEntities[i].Position.Z - playerEntity.Position.Z;
                    double distFromPlayer = Math.Sqrt(Math.Pow(xDiff, 2) + Math.Pow(zDiff, 2));
                    if (distFromPlayer < 700)
                    {
                        if (rabidDogEntities[i].Position.X < playerEntity.Position.X)
                        {
                            rabidDogEntities[i].Position = new Vector3(rabidDogEntities[i].Position.X + 1, rabidDogEntities[i].Position.Y, rabidDogEntities[i].Position.Z);
                            if (!healed[i])
                                bottleEntities[i].Position = new Vector3(bottleEntities[i].Position.X + 1, bottleEntities[i].Position.Y, bottleEntities[i].Position.Z);
                        }
                        else if (rabidDogEntities[i].Position.X > playerEntity.Position.X)
                        {
                            rabidDogEntities[i].Position = new Vector3(rabidDogEntities[i].Position.X - 1, rabidDogEntities[i].Position.Y, rabidDogEntities[i].Position.Z);
                            if (!healed[i])
                                bottleEntities[i].Position = new Vector3(bottleEntities[i].Position.X - 1, bottleEntities[i].Position.Y, bottleEntities[i].Position.Z);
                        }
                        if (rabidDogEntities[i].Position.Z < playerEntity.Position.Z)
                        {
                            rabidDogEntities[i].Position = new Vector3(rabidDogEntities[i].Position.X, rabidDogEntities[i].Position.Y, rabidDogEntities[i].Position.Z + 1);
                            if (!healed[i])
                                bottleEntities[i].Position = new Vector3(bottleEntities[i].Position.X, bottleEntities[i].Position.Y, bottleEntities[i].Position.Z + 1);
                        }
                        else if (rabidDogEntities[i].Position.Z > playerEntity.Position.Z)
                        {
                            rabidDogEntities[i].Position = new Vector3(rabidDogEntities[i].Position.X, rabidDogEntities[i].Position.Y, rabidDogEntities[i].Position.Z - 1);
                            if (!healed[i])
                                bottleEntities[i].Position = new Vector3(bottleEntities[i].Position.X, bottleEntities[i].Position.Y, bottleEntities[i].Position.Z - 1);
                        }
                    }
                    else
                    {
                        // enemies stay where they are, and separate if they are too close to each other
                    }
                    for (int j = 0; j < TOTAL_RABID_DOGS; j++)
                    {
                        if (j != i)
                        {
                            // keep enemies from colliding with each other
                            if ((((Math.Abs(rabidDogEntities[i].Position.X - rabidDogEntities[j].Position.X) + Math.Abs(rabidDogEntities[i].Position.Z - rabidDogEntities[j].Position.Z)) / 2) < (rabidDogRadii[i] + rabidDogRadii[j] - 5)))
                            {
                                rabidDogEntities[i].Position = rabidDogPreviousPositions[i];
                                bottleEntities[i].Position = rabidDogPreviousPositions[i];
                                break;
                            }
                        }
                    }
                    // keep enemies from colliding with the player and decrease health if collision
                    if ((((Math.Abs(rabidDogEntities[i].Position.X - playerEntity.Position.X) + Math.Abs(rabidDogEntities[i].Position.Z - playerEntity.Position.Z)) / 2) < (rabidDogRadii[i] + playerRadius - 19.5)))
                    {
                        rabidDogEntities[i].Position = rabidDogPreviousPositions[i];
                        bottleEntities[i].Position = rabidDogPreviousPositions[i];
                        playerHealthDecr += 0.1f; // add up damage to player
                        if (playerHealthDecr >= 1.0f)
                        {
                            mCurrentHealth -= 1; // decrease player health by 1 unit
                            //Force the health to remain between 0 and 100.           
                            mCurrentHealth = (int)MathHelper.Clamp(mCurrentHealth, 0, 100);
                            playerHealthDecr = 0.0f;
                        }
                    }
                    rabidDogEntities[i].Update(gameTime);
                    bottleEntities[i].Update(gameTime);
                }
            }
        }

        protected virtual void UpdateFrameRate(GameTime gameTime)
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
                    if(!barkInstance.IsDisposed)
                         barkInstance.Dispose();
                    ScreenManager.AddScreen(new GameOverScreen(), ControllingPlayer);
                }
            }
        }

        protected void IncrementFrameCounter()
        {
            ++frames;
        }

        protected void DrawIndicator()
        {
            effect.CurrentTechnique = effect.Techniques["ColoredNoShading"];
            effect.Parameters["xView"].SetValue(camera.ViewMatrix);
            effect.Parameters["xProjection"].SetValue(camera.ProjectionMatrix);
            Matrix worldMatrix = Matrix.CreateRotationY(3 * angle);
            effect.Parameters["xWorld"].SetValue(worldMatrix);

            foreach (EffectPass pass in effect.CurrentTechnique.Passes)
            {
                pass.Apply();
                foreach (VertexPositionColor[] v in vertices)
                {
                    ScreenManager.GraphicsDevice.DrawUserPrimitives(PrimitiveType.TriangleList, v, 0, 1, VertexPositionColor.VertexDeclaration);
                }
            }
        }

        protected void DrawBullets()
        {
            if (bulletList.Count > 0)
            {
                quadEffect.CurrentTechnique = quadEffect.Techniques["Textured_2_0"];
                quadEffect.Parameters["xView"].SetValue(camera.ViewMatrix);
                quadEffect.Parameters["xProjection"].SetValue(camera.ProjectionMatrix);
                quadEffect.Parameters["xTexture"].SetValue(bullets);
                ScreenManager.GraphicsDevice.RasterizerState = RasterizerState.CullNone;
                
                for (int i = 0; i < bulletList.Count; i++)
                {
                    // scale down the quad
                    Matrix worldMatrix = Matrix.CreateScale(0.05f, 0.05f, 0.05f) * Matrix.CreateBillboard(bulletList[i].position, camera.Position, cameraUpDirection, Vector3.Forward);

                    foreach (EffectPass pass in quadEffect.CurrentTechnique.Passes)
                    {
                        quadEffect.Parameters["xWorld"].SetValue(worldMatrix);

                        pass.Apply();

                        ScreenManager.GraphicsDevice.SetVertexBuffer(quad.VertexBuffer);
                        ScreenManager.GraphicsDevice.Indices = quad.IndexBuffer;
                        ScreenManager.GraphicsDevice.DrawIndexedPrimitives(PrimitiveType.TriangleList, 0, 0, 4, 0, 2);                        
                    }
                }

                ScreenManager.GraphicsDevice.RasterizerState = RasterizerState.CullCounterClockwise;
            }
        }

        protected void DrawAraguz()
        {
            if (modelTransforms == null)
                modelTransforms = new Matrix[playerModel.ModelHeld.Bones.Count];

            //playerModel.ModelHeld.CopyAbsoluteBoneTransformsTo(modelTransforms);
            modelTransforms = animationPlayer.GetSkinTransforms();
            foreach (ModelMesh mesh in playerModel.ModelHeld.Meshes)
            {
                foreach (SkinnedEffect effect in mesh.Effects)
                {
                    effect.SetBoneTransforms(modelTransforms);

                    effect.View = camera.ViewMatrix;
                    effect.Projection = camera.ProjectionMatrix;

                    effect.EnableDefaultLighting();

                    effect.SpecularColor = new Vector3(0.25f);
                    effect.SpecularPower = 16;
                }

                mesh.Draw();
            }
        }

        protected void DrawPlayer()
        {
            if (modelTransforms == null)
                modelTransforms = new Matrix[playerModel.ModelHeld.Bones.Count];

            playerModel.ModelHeld.CopyAbsoluteBoneTransformsTo(modelTransforms);

            foreach (ModelMesh m in playerModel.ModelHeld.Meshes)
            {
                foreach (BasicEffect e in m.Effects)
                {
                    e.PreferPerPixelLighting = true;
                    e.TextureEnabled = true;
                    e.EnableDefaultLighting();
                    e.World = modelTransforms[m.ParentBone.Index] * playerEntity.WorldMatrix;
                    e.View = camera.ViewMatrix;
                    e.Projection = camera.ProjectionMatrix;
                }

                m.Draw();
            }
        }

        protected void DrawEnemies()
        {
            for (int i = 0; i < initialNumberOfDogs; i++)
            {
                if (!healed[i])
                {
                    if (modelEnemyTransforms[i] == null)
                        modelEnemyTransforms[i] = new Matrix[rabidDogModels[i].ModelHeld.Bones.Count];

                    rabidDogModels[i].ModelHeld.CopyAbsoluteBoneTransformsTo(modelEnemyTransforms[i]);

                    foreach (ModelMesh m in rabidDogModels[i].ModelHeld.Meshes)
                    {
                        foreach (BasicEffect e in m.Effects)
                        {
                            e.PreferPerPixelLighting = true;
                            e.TextureEnabled = true;
                            e.EnableDefaultLighting();
                            e.World = modelEnemyTransforms[i][m.ParentBone.Index] * rabidDogEntities[i].WorldMatrix;
                            e.View = camera.ViewMatrix;
                            e.Projection = camera.ProjectionMatrix;
                        }

                        m.Draw();
                    }
                }
            }
        }

        protected void DrawLevel()
        {
            foreach (ModelMesh m in levelModel.ModelHeld.Meshes)
            {
                foreach (BasicEffect e in m.Effects)
                {
                    e.PreferPerPixelLighting = true;
                    e.TextureEnabled = true;
                    e.EnableDefaultLighting();
                    e.World = levelEntity.WorldMatrix;
                    e.View = camera.ViewMatrix;
                    e.Projection = camera.ProjectionMatrix;
                }

                m.Draw();
            }
        }

        protected void DrawBottles()
        {
            for (int i = 0; i < healed.Length; i++)
            {
                //if (modelEnemyTransforms[i] == null)
                //    modelEnemyTransforms[i] = new Matrix[rabidDogModels[i].ModelHeld.Bones.Count];

                //rabidDogModels[i].ModelHeld.CopyAbsoluteBoneTransformsTo(modelEnemyTransforms[i]);

                if (healed[i] && !collected[i])
                {
                    foreach (ModelMesh m in bottleModels[i].ModelHeld.Meshes)
                    {
                        foreach (BasicEffect e in m.Effects)
                        {
                            e.PreferPerPixelLighting = true;
                            e.TextureEnabled = true;
                            e.EnableDefaultLighting();
                            e.World = bottleEntities[i].WorldMatrix;
                            e.View = camera.ViewMatrix;
                            e.Projection = camera.ProjectionMatrix;
                        }

                        m.Draw();
                    }
                }
            }
        }

        protected virtual void DrawText()
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
                    jacksonTaunt = "I, Russell Jackson, challenge you, Geraldo \"Merry Gerry\" Araguz, to survive\n for 5 minutes in this park with these diseased dogs!\n I can guarantee that you will not survive for even one.\nGet him, boys!!\n\n";
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

        /// <summary>
        /// Lets the game respond to player input. Unlike the Update method,
        /// this will only be called when the gameplay screen is active.
        /// </summary>
        public override void HandleInput(InputState input)
        {
            //if (input == null)
            //    throw new ArgumentNullException("input");

            //// Look up inputs for the active player profile.
            //int playerIndex = (int)ControllingPlayer.Value;

            //KeyboardState keyboardState = input.CurrentKeyboardStates[playerIndex];
            //GamePadState gamePadState = input.CurrentGamePadStates[playerIndex];

            //// The game pauses either if the user presses the pause button, or if
            //// they unplug the active gamepad. This requires us to keep track of
            //// whether a gamepad was ever plugged in, because we don't want to pause
            //// on PC if they are playing with a keyboard and have no gamepad at all!
            //bool gamePadDisconnected = !gamePadState.IsConnected &&
            //                           input.GamePadWasConnected[playerIndex];

            //if (input.IsPauseGame(ControllingPlayer) || gamePadDisconnected)
            //{
            //    ScreenManager.AddScreen(new PauseMenuScreen(), ControllingPlayer);
            //}
            //else
            //{
            //    // Otherwise move the player position.
            //    Vector2 movement = Vector2.Zero;
                
            //    if (keyboardState.IsKeyDown(Keys.A))
            //        movement.X--;

            //    if (keyboardState.IsKeyDown(Keys.D))
            //        movement.X++;

            //    if (keyboardState.IsKeyDown(Keys.W))
            //        movement.Y--;

            //    if (keyboardState.IsKeyDown(Keys.S))
            //        movement.Y++;
                
            //    Vector2 thumbstick = gamePadState.ThumbSticks.Left;

            //    movement.X += thumbstick.X;
            //    movement.Y -= thumbstick.Y;

            //    if (movement.Length() > 1)
            //        movement.Normalize();

            //    playerPosition += movement * 2;
            //}
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

                if (keyboardState.IsKeyDown(Keys.W))
                {
                    if (OtherKeysUp(keyboardState, Keys.W))
                        soundInstance.Play();
                    movement.Y--;
                }
                if (keyboardState.IsKeyDown(Keys.S))
                {
                    if (OtherKeysUp(keyboardState, Keys.S))
                        soundInstance.Play();
                    movement.Y++;
                }
                if (keyboardState.IsKeyDown(Keys.A))
                {
                    if (OtherKeysUp(keyboardState, Keys.A))
                        soundInstance.Play();
                    movement.X--;
                }
                if (keyboardState.IsKeyDown(Keys.D))
                {
                    if (OtherKeysUp(keyboardState, Keys.D))
                        soundInstance.Play();
                    movement.X++;
                }
                if (keyboardState.IsKeyDown(Keys.Up))
                {
                    if (OtherKeysUp(keyboardState, Keys.Up))
                        soundInstance.Play();
                    movement.Y--;
                }
                if (keyboardState.IsKeyDown(Keys.Down))
                {
                    if (OtherKeysUp(keyboardState, Keys.Down))
                        soundInstance.Play();
                    movement.Y++;
                }
                if (keyboardState.IsKeyDown(Keys.Left))
                {
                    if (OtherKeysUp(keyboardState, Keys.Left))
                        soundInstance.Play();
                    movement.X--;
                }
                if (keyboardState.IsKeyDown(Keys.Right))
                {
                    if (OtherKeysUp(keyboardState, Keys.Right))
                        soundInstance.Play();
                    movement.X++;
                }               
                if (keyboardState.IsKeyDown(Keys.RightControl))
                {
                    if (OtherKeysUp(keyboardState, Keys.RightControl) || keyboardState.IsKeyDown(Keys.W) || keyboardState.IsKeyDown(Keys.Up)
                        || keyboardState.IsKeyDown(Keys.S) || keyboardState.IsKeyDown(Keys.Down) || keyboardState.IsKeyDown(Keys.A)
                        || keyboardState.IsKeyDown(Keys.Left) || keyboardState.IsKeyDown(Keys.D) || keyboardState.IsKeyDown(Keys.Right))
                        plasmarayInstance.Play();
                }
                if (keyboardState.IsKeyUp(Keys.W) && OtherKeysUp(keyboardState, Keys.W))
                    soundInstance.Stop();
                if (keyboardState.IsKeyUp(Keys.Up) && OtherKeysUp(keyboardState, Keys.Up))
                    soundInstance.Stop();
                if (keyboardState.IsKeyUp(Keys.S) && OtherKeysUp(keyboardState, Keys.S))
                    soundInstance.Stop();
                if (keyboardState.IsKeyUp(Keys.Down) && OtherKeysUp(keyboardState, Keys.Down))
                    soundInstance.Stop();
                if (keyboardState.IsKeyUp(Keys.A) && OtherKeysUp(keyboardState, Keys.A))
                    soundInstance.Stop();
                if (keyboardState.IsKeyUp(Keys.Left) && OtherKeysUp(keyboardState, Keys.Left))
                    soundInstance.Stop();
                if (keyboardState.IsKeyUp(Keys.D) && OtherKeysUp(keyboardState, Keys.D))
                    soundInstance.Stop();
                if (keyboardState.IsKeyUp(Keys.Right) && OtherKeysUp(keyboardState, Keys.Right))
                    soundInstance.Stop();
                if (keyboardState.IsKeyUp(Keys.RightControl) && (OtherKeysUp(keyboardState, Keys.RightControl) || keyboardState.IsKeyDown(Keys.W) || keyboardState.IsKeyDown(Keys.Up)
                        || keyboardState.IsKeyDown(Keys.S) || keyboardState.IsKeyDown(Keys.Down) || keyboardState.IsKeyDown(Keys.A)
                        || keyboardState.IsKeyDown(Keys.Left) || keyboardState.IsKeyDown(Keys.D) || keyboardState.IsKeyDown(Keys.Right)))
                    plasmarayInstance.Stop();
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

            DrawEnemies();

            DrawAraguz();

            DrawLevel();

            DrawBullets();

            DrawBottles();

            DrawIndicator();

            sky.Draw(camera.ViewMatrix, camera.ProjectionMatrix);

            DrawTerrain(camera.ViewMatrix, camera.ProjectionMatrix);
            
            DrawText();

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
            string secs = (timeLeft%60).ToString();
            if ((timeLeft % 60) < 10)
            {
                secs = "0" + secs;
            }
            Color timeLeftColor = Color.White;
            if (timeLeft < 60)
            {
                timeLeftColor = Color.Red;
            }
            spriteBatchAlpha.DrawString(gameFont, (timeLeft / 60) + ":" + secs, new Vector2(screenWidth/2 - 45, 10), timeLeftColor);

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

        /// <summary>
        /// Helper for drawing the terrain model.
        /// </summary>
        protected void DrawTerrain(Matrix view, Matrix projection)
        {
            foreach (ModelMesh mesh in grassy_level.Meshes)
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

        protected bool OtherKeysUp(KeyboardState state, Keys theKey)
        {
            Keys[] gameKeys = {Keys.H, Keys.Space, Keys.LeftAlt, Keys.RightAlt, Keys.Enter, Keys.Add,
                                  Keys.Subtract, Keys.A, Keys.W, Keys.S, Keys.D, Keys.Up, Keys.Left,
                                  Keys.Right, Keys.Down, Keys.E, Keys.R, Keys.RightControl};
            bool keysAreUp = true;
            foreach (Keys key in gameKeys)
            {
                if (key != theKey)
                    keysAreUp = keysAreUp && state.IsKeyUp(key);
            }
            return keysAreUp;
        }
    }
}