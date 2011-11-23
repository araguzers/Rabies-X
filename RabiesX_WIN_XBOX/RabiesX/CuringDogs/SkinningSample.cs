

using System;
using System.Collections.Generic;
using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Audio;
using Microsoft.Xna.Framework.Content;
using Microsoft.Xna.Framework.GamerServices;
using Microsoft.Xna.Framework.Graphics;
using Microsoft.Xna.Framework.Input;
using Microsoft.Xna.Framework.Net;
using Microsoft.Xna.Framework.Storage;
using SkinnedModel;//need to use the SkinnedModel resource
using SkinningSampleWindows.Entity;//using Entity classes


/// Remember to load SkinnedModel to the top references and also load SkinnedModelPipeline
/// to Content/References. You can find this files inside the SkinnedModel Sample from 
/// creators.xna.com
namespace SkinningSample
{
    /// <summary>
    /// This is the main type for your game
    /// </summary>
    public class SkinningSampleGame : Microsoft.Xna.Framework.Game
    {
        GraphicsDeviceManager graphics;
        SpriteBatch spriteBatch;
        SpriteFont spriteFont; //Load Sprite font for scoring or any text on the screen.

        public Texture2D BlankTexture { get; private set; }
        
        GraphicsDevice device;


        //Controller variables from Controller class
        #region Controller Variables
        Controller con;
        List<Controller.Action> actionList;
        #endregion

        #region Rendering

        Model skyboxModel;

        //We can use the AnimPlayer to play whole clips, like in XNA SkinnedModel Sample,
        //but we will used the ClipPlayer class to play our different clips instead.

        Model dude; //Load the dude from the SkinnedSample from creators.xna.com
        SkinningData skd;
        SkinningData skdTarget;
        //AnimationPlayer animationPlayer;
        ClipPlayer clipPlayer;

        //Character normal map, glow map, reflective map and shadow
        //Texture2D[] normalMap = new Texture2D[17];
       // Texture2D[] glowMap = new Texture2D[17];
        //Texture envMap;
        //Matrix shadow;
        Vector3 lightDir1 = new Vector3(1, 1, 1);

        
        Random rand = new Random();

        #endregion

        //Class to load spaceship and make it move on the Z direction. 
        //Spaceship model is not loaded yet, find a model and load it in the content.
        //Model ship;
        //float shipPosZ;//Spaceship will move on the Z direction.

        Model level; // Load level
        BoundingBox[] bbLevel;// Load level BB collision min max values

        Model dual;
        

        Vector3 dudePos; //dude position
        Quaternion dudeQRot; //Quaternion rotation
        float dudeRotY;
        float dudeRotX;

        List<Bullet> bulletList = new List<Bullet>();//bullet list
        Shape bulletShape;

        #region

        List<Target> targetList = new List<Target>();//target list
        Shape targetShape;

        int numTarget = 0;

        int score_enemies = 0;
        

        //used for collectables
        const int NUM_OF_Target = 50; //total number of collectables on the screen
        List<Enemy> targets = new List<Enemy>(); //list of collectables
        List<Vector2> usedTargetXZ; //used locations (so we don't have multiple targets on 1 spot)
        const int tMAX = 1;
        const int tMIN = 0;
        int[] tXValues = { -2000, 20000 };
        int[] tZValues = { -2000, 40000 };

    
        const int NUM_Targets_TO_WIN = 50;


        #endregion
        #region Collectables

        List<Collectable> collectableList = new List<Collectable>();//target list.















        Shape collectableShape;

        int numCollected = 0;

        int score = 0;
        const int MAX_TIME_LEFT = 140;
        int timeLeft = MAX_TIME_LEFT;
        int elapsedUpdateTime = 0;

        //used for collectables
        const int NUM_OF_COLLECTABLES = 100; //total number of collectables on the screen
        List<Collectable> collectables = new List<Collectable>(); //list of collectables
        List<Vector2> usedXZ; //used locations (so we don't have multiple targets on 1 spot)
        const int MAX = 1;
        const int MIN = 0;
        int[] XValues = { -1000, 20000 };
        int[] ZValues = { -1000, 40000 };

        bool gamePaused = false;
        Vector2 screenCenter;
        Vector3 STARTING_POSITION = Vector3.Zero;
        const int NUM_COLLECTED_TO_WIN = 50;
        Matrix view, projection;

        #endregion

        #region Audio
        // Audio objects
        AudioEngine engine;
        SoundBank soundBank;
        WaveBank waveBank;
        #endregion


        public SkinningSampleGame()
        {
            graphics = new GraphicsDeviceManager(this);
            Content.RootDirectory = "Content";
            //Character shadow
           // graphics.PreferredDepthStencilFormat = DepthFormat.Depth24Stencil8;
        }

        /// <summary>
        /// Allows the game to perform any initialization it needs to before starting to run.
        /// This is where it can query for any required services and load any non-graphic
        /// related content.  Calling base.Initialize will enumerate through any components
        /// and initialize them as well.
        /// </summary>
        protected override void Initialize()
        {
            // TODO: Add your initialization logic here
            graphics.SynchronizeWithVerticalRetrace = false;
            graphics.ApplyChanges();
            IsFixedTimeStep = true;//this 3 lines are for the frames per second.

            screenCenter = new Vector2(graphics.PreferredBackBufferWidth / 2.0f, graphics.PreferredBackBufferHeight / 2.0f);

            // TODO: Add your initialization logic here
            // Initialize audio objects.
            // Initialize audio objects.
            engine = new AudioEngine("Content\\Original.xgs");
            soundBank = new SoundBank(engine, "Content\\Sound Bank.xsb");
            waveBank = new WaveBank(engine, "Content\\Wave Bank.xwb");


            // Play the sound.
            soundBank.PlayCue("Capture2");

            //Controller list
            actionList = new List<Controller.Action>();
            actionList.Add(new Controller.Action("LUp", Buttons.LeftThumbstickUp, Keys.W));
            actionList.Add(new Controller.Action("LDown", Buttons.LeftThumbstickDown, Keys.S));
            actionList.Add(new Controller.Action("LLeft", Buttons.LeftThumbstickLeft, Keys.A));
            actionList.Add(new Controller.Action("LRight", Buttons.LeftThumbstickRight, Keys.D));
            actionList.Add(new Controller.Action("RUp", Buttons.RightThumbstickUp, Keys.Up));
            actionList.Add(new Controller.Action("RDown", Buttons.RightThumbstickDown, Keys.Down));
            actionList.Add(new Controller.Action("RLeft", Buttons.RightThumbstickRight, Keys.Left));
            actionList.Add(new Controller.Action("RRight", Buttons.RightThumbstickRight, Keys.Right));
            actionList.Add(new Controller.Action("LT", Buttons.LeftTrigger, Keys.LeftShift));
            actionList.Add(new Controller.Action("RT", Buttons.RightTrigger, Keys.E));
            actionList.Add(new Controller.Action("A", Buttons.A, Keys.C));
            actionList.Add(new Controller.Action("B", Buttons.B, Keys.V));
            actionList.Add(new Controller.Action("Reset", Buttons.A, Keys.R));
            con = new Controller(PlayerIndex.One, actionList);

            //One BoundingBox level collision, you can add as many as you want.
            //This info comes from making collision boxes in your 3D application
            //and getting the Bounding box min and max values.
            bbLevel = new BoundingBox[] {
                new BoundingBox( new Vector3(-135,175,129), new Vector3(45,355,309) ), 
                new BoundingBox( new Vector3(-270,-5,129), new Vector3(-90,175,309) ), 
                new BoundingBox( new Vector3(-90,-5,129), new Vector3(90,175,309) ), 
                new BoundingBox( new Vector3(90,-5,129), new Vector3(270,175,309) ), 
                new BoundingBox( new Vector3(-720,-5,-501), new Vector3(720,625,-411) ), 
                new BoundingBox( new Vector3(977,-291,-1153), new Vector3(1500,187,-498) ), 
                new BoundingBox( new Vector3(1064,-113,-5691), new Vector3(1701,1750,-2585) ), 
                new BoundingBox( new Vector3(1415,-99,-5859), new Vector3(2154,2064,-2254) ), 
                new BoundingBox( new Vector3(1822,-83,-6053), new Vector3(2680,2428,-1868) ), 
                new BoundingBox( new Vector3(2295,-64,-6279), new Vector3(3290,2851,-1421) ), 
                new BoundingBox( new Vector3(2843,-42,-6541), new Vector3(3999,3341,-902) ), 
                new BoundingBox( new Vector3(-674,-218,-10093), new Vector3(-372,76,-9253) ), 
                new BoundingBox( new Vector3(-139,-240,-10382), new Vector3(1416,602,-9040) ), 
                new BoundingBox( new Vector3(336,-80,-6425), new Vector3(949,390,-5788) ), 
                new BoundingBox( new Vector3(-1702,-113,-5691), new Vector3(-1065,1750,-2585) ), 
                new BoundingBox( new Vector3(-2458,-74,-6154), new Vector3(-1539,2618,-1668) ), 
                new BoundingBox( new Vector3(3823,-167,2719), new Vector3(5008,444,3513) ), 
                new BoundingBox( new Vector3(3602,-80,4158), new Vector3(4215,390,4795) ), 
                new BoundingBox( new Vector3(1142,-41,6497), new Vector3(3609,360,7150) ), 
                new BoundingBox( new Vector3(5067,-41,4043), new Vector3(6482,1657,5057) ), 
                new BoundingBox( new Vector3(4195,-37,4215), new Vector3(10020,2801,29366) ), 
                new BoundingBox( new Vector3(19913,-12,450), new Vector3(21519,1340,632) ), 
                new BoundingBox( new Vector3(3154,-15,20674), new Vector3(4161,787,21772) ), 
                new BoundingBox( new Vector3(-3185,-1951,-11980), new Vector3(-2145,1092,74861) ), 
                new BoundingBox( new Vector3(-3459,3,-12484), new Vector3(83382,3047,-11444) ), 
                new BoundingBox( new Vector3(15490,-132,35105), new Vector3(20535,4172,47827) )

                };


            //Initialize Collection variables
            usedTargetXZ = new List<Vector2>();
            Random rand = new Random((int)DateTime.Now.Ticks);

            Viewport vp = GraphicsDevice.Viewport;//Viewport
            float aspectRatio = (float)vp.Width / vp.Height;

            
            projection = Matrix.CreatePerspectiveFieldOfView(MathHelper.PiOver4, aspectRatio,
                        1, 30000);


            //Add targets to the game
            for (int i = 0; i < NUM_OF_Target; i++)
            {
                int tx = 0;
                int tz = 0;
                do
                {
                    tx = rand.Next(tXValues[MIN], tXValues[MAX]);
                    tz = rand.Next(tZValues[MIN], tZValues[MAX]);
                } while (OriginalTXZ(tx, tz)); //Get Original Value for collectable

                usedTargetXZ.Add(new Vector2(tx, tz)); //Mark collectable as used (reserves position on the game world)
                targets.Add(new Enemy(new Vector3(tx, 10, tz), Content.Load<Model>("squirrel_enemy"), projection)); //add collectable to the game
            }

            //Initialize Collection variables
            usedXZ = new List<Vector2>();
            
            
            //Add all collectables to the game
            for (int i = 0; i < NUM_OF_COLLECTABLES; i++)
            {
                int x = 0;
                int z = 0;
                do
                {
                    x = rand.Next(XValues[MIN], XValues[MAX]);
                    z = rand.Next(ZValues[MIN], ZValues[MAX]);
                } while (OriginalXZ(x, z)); //Get Original Value for collectable

                usedXZ.Add(new Vector2(x, z)); //Mark collectable as used (reserves position on the game world)
                collectables.Add(new Collectable(new Vector3(x, 10, z))); //add collectable to the game
            }

            //Target list and null position values from your 3D application.
            //Of course you can guess this values from the XNA application.
            //targetList.Add(new Target(new Vector3(-310, 10, 12250)));
            //targetList.Add(new Target(new Vector3(-310, 10, 6404)));
            //targetList.Add(new Target(new Vector3(-310, 10, 2957)));
            //targetList.Add(new Target(new Vector3(-310, 10, 1359)));
            //targetList.Add(new Target(new Vector3(-310, 10, 326)));

            base.Initialize();
        

#if WINDOWS_PHONE
            // Frame rate is 30 fps by default for Windows Phone.
            TargetElapsedTime = TimeSpan.FromTicks(333333);
            
            graphics.IsFullScreen = true;            
#endif
        }

        bool OriginalTXZ(int tx, int tz)
        {

            for (int i = 0; i < usedTargetXZ.Count; i++)
            {
                if (usedTargetXZ[i].X == tx && usedTargetXZ[i].Y == tz)
                {
                    return true;
                }
            }

            //make sure your targets aren't inside a bounding box.  We need to be able to get to them!
            for (int i = 0; i < bbLevel.Length; i++)
            {
                if (bbLevel[i].Contains(new Vector3(tx, 10, tz)) != ContainmentType.Disjoint) //Temporary Location of collectable not inside bounding box
                {
                    return true;
                }
            }

            return false;
        }

        bool OriginalXZ(int x, int z)
        {
            
            for (int i = 0; i < usedXZ.Count; i++)
            {
                if (usedXZ[i].X == x && usedXZ[i].Y == z)
                {
                    return true;
                }
            }

            //make sure your collectables aren't inside a bounding box.  We need to be able to get to them!
            for (int i = 0; i < bbLevel.Length; i++)
            {
                if(bbLevel[i].Contains(new Vector3(x, 10, z)) != ContainmentType.Disjoint) //Temporary Location of collectable not inside bounding box
                {
                    return true;
                }
            }

            return false;
        }

        /// <summary>
        /// LoadContent will be called once per game and is the place to load
        /// all of your content.
        /// </summary>
        protected override void LoadContent()
        {

            device = graphics.GraphicsDevice;
            // Create a new SpriteBatch, which can be used to draw textures.
            spriteBatch = new SpriteBatch(GraphicsDevice);

            //Create the spritefont
            spriteFont = Content.Load<SpriteFont>("Arial");

            // TODO: use this.Content to load your game content here

            dude = this.Content.Load<Model>("squirrel");//Load the dude
            skd = dude.Tag as SkinningData;//you need to tag is animated character

           
           // animationPlayer = new AnimationPlayer(skd);
            clipPlayer = new ClipPlayer(skd, 60);//ClipPlayer running at 24 frames/sec
            AnimationClip clip = skd.AnimationClips["Take 001"]; //Take name from the dude.fbx file
            //animationPlayer.StartClip(clip);

            //Clip ranges
            //idle 1-24
            //walk 36-86
            //run 99-124

            clipPlayer.play(clip, 1, 24, true); //game starts with idle animation frames 1 to 3

            //Load Skybox
            skyboxModel = Content.Load<Model>("skybox");//Load Skybox

            //Load the level
            level = Content.Load<Model>("road");

            //Load dual Texture Model
            dual = Content.Load<Model>("model");
            //grey = new Texture2D(GraphicsDevice, 1, 1);
            //grey.SetData(new Color[] { new Color(128, 128, 128, 255) });
            //Load the bullet
            Model bulletModel = Content.Load<Model>("laser_m");
            bulletShape = new Shape(bulletModel, bulletList.ToArray());

            //Load the targets and collectables
            Model collectableModel = Content.Load<Model>("target");
            collectableShape = new Shape(collectableModel, collectables.ToArray());

            //Model targetModel = Content.Load<Model>("squirrel_enemy");
            //skdTarget = targetModel.Tag as SkinningData;//you need to tag is animated character


            //// animationPlayer = new AnimationPlayer(skd);
            //clipPlayer = new ClipPlayer(skdTarget, 60);//ClipPlayer running at 24 frames/sec
            //AnimationClip modelClip = skdTarget.AnimationClips["Take 002"]; //Take name from the dude.fbx file
            //targetShape = new Shape(targetModel, targets.ToArray());
            


        }

        /// <summary>
        /// UnloadContent will be called once per game and is the place to unload
        /// all content.
        /// </summary>
        protected override void UnloadContent()
        {
            // TODO: Unload any non ContentManager content here
        }

        public void WinGame()
        {
            gamePaused = true;
        }

        public void LoseGame()
        {
            gamePaused = true;
        }

        public void ResetGame()
        {
            //Initialize Collection variables
            usedTargetXZ = new List<Vector2>();
            Random trand = new Random((int)DateTime.Now.Ticks);

            //Add all collectables to the game
            for (int i = 0; i < NUM_OF_Target; i++)
            {
                int tx = 0;
                int tz = 0;
                do
                {
                    tx = trand.Next(tXValues[MIN], tXValues[MAX]);
                    tz = trand.Next(tZValues[MIN], tZValues[MAX]);
                } while (OriginalXZ(tx, tz)); //Get Original Value for collectable

                usedTargetXZ.Add(new Vector2(tx, tz)); //Mark target as used (reserves position on the game world)
                targets.Add(new Enemy(new Vector3(tx, 10, tz), Content.Load<Model>("squirrel_enemy"), projection)); //add targets to the game
            }

            //Initialize Collection variables
            usedXZ = new List<Vector2>();
            Random rand = new Random((int)DateTime.Now.Ticks);
           

            //Add all collectables to the game
            for (int i = 0; i < NUM_OF_COLLECTABLES; i++)
            {
                int x = 0;
                int z = 0;
                do
                {
                    x = rand.Next(XValues[MIN], XValues[MAX]);
                    z = rand.Next(ZValues[MIN], ZValues[MAX]);
                } while (OriginalXZ(x, z)); //Get Original Value for collectable

                usedXZ.Add(new Vector2(x, z)); //Mark collectable as used (reserves position on the game world)
                collectables.Add(new Collectable(new Vector3(x, 10, z))); //add collectable to the game
            }

            //targetShape.updateObjects(targets.ToArray());
            collectableShape.updateObjects(collectables.ToArray());

            dudePos = Vector3.Zero;
            dudeQRot = Quaternion.CreateFromAxisAngle(Vector3.Up,//Quaternion rotation
                                    MathHelper.ToRadians(0.0f));

            gamePaused = false;
            timeLeft = MAX_TIME_LEFT;
            elapsedUpdateTime = 0;
            numCollected = 0;
            numTarget = 0;

        }

        /// <summary>
        /// Allows the game to run logic such as updating the world,
        /// checking for collisions, gathering input, and playing audio.
        /// </summary>
        /// <param name="gameTime">Provides a snapshot of timing values.</param>
        /// 
        float shotDelay = 0.25f;
        protected override void Update(GameTime gameTime)
        {
            //Update our controller
            con.update();


            if (gamePaused)
            {
                if (con.isActionPressed("Reset"))
                {
                    ResetGame();
                }
                return;
            }

            //Update Time Left
            elapsedUpdateTime += gameTime.ElapsedGameTime.Milliseconds; //get the number of milliseconds from all
            if (elapsedUpdateTime >= 1000) //Now we have 1 second
            {
                timeLeft--;
                if (timeLeft <= 0)
                    LoseGame();
                elapsedUpdateTime = 0; //reset our second counter
            }

            // Update the audio engine.
            engine.Update();


            shotDelay -= (float)gameTime.ElapsedGameTime.TotalSeconds;

            // Allows the game to exit
            if (GamePad.GetState(PlayerIndex.One).Buttons.Back == ButtonState.Pressed)
                this.Exit();

            // TODO: Add your update logic here

            //Let me see how fast my game is running
            float fps = (1000.0f / (float)gameTime.ElapsedGameTime.TotalMilliseconds);
            Window.Title = "FPS: " + fps.ToString();//This 2 lines are to show how fast 
            //your game is running (frames per second) 

            
            dudeRotX += gameTime.ElapsedGameTime.Milliseconds;

         
            //dude velocity at the start of the game 
            Vector3 dudeVel = new Vector3(0, 0, 0);
            //animationPlayer.Update(gameTime.ElapsedGameTime, true, Matrix.Identity);

            if (con.isActionPressed("LUp") & (con.isActionPressed("RT")))//shoots while walking or running
            {
                if (con.isActionPressed("LT"))
                {
                    dudeVel = new Vector3(0, 0, 20);
                    if (!clipPlayer.inRange(99, 124))//if clipplayer is not in frame ranges 
                        clipPlayer.switchRange(99, 124);//play this frames when you press LUp+LT
                    else if (shotDelay < 0.0f)
                    {
                        shotDelay = 0.2f;
                        soundBank.PlayCue("Laser");
                        Matrix gunarm = clipPlayer.getWorldTransform(29);//uses bone 29 of dude.fbx to bullets
                        //you can chage this bone to any other bone of 59 bones.
                        bulletList.Add(new Bullet(
                                        gunarm.Translation,
                                        dudeQRot));
                        bulletShape.updateObjects(bulletList.ToArray());
                        GamePad.SetVibration(con.PlayerIndex, 0.2f, 0.5f);//Sets GamePad vibration motors left and right
                    }
                }
                else
                {
                    dudeVel = new Vector3(0, 0, 10);
                    if (!clipPlayer.inRange(123, 125))
                        clipPlayer.switchRange(123, 125);
                    else if (shotDelay < 0.0f)
                    {
                        shotDelay = 0.2f;
                        soundBank.PlayCue("Laser");
                        Matrix gunarm = clipPlayer.getWorldTransform(29);
                        bulletList.Add(new Bullet(
                                        gunarm.Translation,
                                        dudeQRot));
                        bulletShape.updateObjects(bulletList.ToArray());
                        GamePad.SetVibration(con.PlayerIndex, 0.2f, 0.5f);
                    }
                }
            }
            if (con.isActionPressed("LUp"))//walks and runs without shooting
            {
                if (con.isActionPressed("LT"))
                {
                    dudeVel = new Vector3(0, 0, 20);
                    if (!clipPlayer.inRange(99, 124))
                        clipPlayer.switchRange(99, 124);
                    GamePad.SetVibration(con.PlayerIndex, 0.0f, 0.0f);
                }
                else
                {
                    dudeVel = new Vector3(0, 0, 10);
                    if (!clipPlayer.inRange(36, 86))
                        clipPlayer.switchRange(36, 86);
                    GamePad.SetVibration(con.PlayerIndex, 0.0f, 0.0f);
                }
            }

            else if (con.isActionPressed("RT"))//shots without moving
            {
                if (!clipPlayer.inRange(123, 125))
                {
                    clipPlayer.switchRange(123, 125);
                }
                else if (shotDelay < 0.0f)
                {
                    shotDelay = 0.2f;
                    soundBank.PlayCue("Laser");
                    Matrix gunarm = clipPlayer.getWorldTransform(29); //bone number from location of bullet
                    bulletList.Add(new Bullet(
                                    gunarm.Translation,
                                    dudeQRot));
                    bulletShape.updateObjects(bulletList.ToArray());
                    GamePad.SetVibration(con.PlayerIndex, 0.2f, 0.5f);
                }
            }

            else
            {
                if (!clipPlayer.inRange(1, 24))//if not moving call idle animation
                    clipPlayer.switchRange(1, 24);
                GamePad.SetVibration(con.PlayerIndex, 0.0f, 0.0f);
            }

            if (con.isActionPressed("LLeft"))//rotate
            {
                dudeRotY += 1.0f;
            }
            else if (con.isActionPressed("LRight"))
            {
                dudeRotY += -1.0f;
            }

            if (con.isActionPressed("LDown"))//walk back at different speeds
            {
                if (con.isActionPressed("LT"))
                {
                    dudeVel = new Vector3(0, 0, -10);
                }
                else
                {
                    dudeVel = new Vector3(0, 0, -5);
                }
            }
            if (con.isActionPressed("LLeft"))
            {
                dudeRotY += 1.0f;
            }
            else if (con.isActionPressed("LRight"))
            {
                dudeRotY += -1.0f;
            }



            dudeQRot = Quaternion.CreateFromAxisAngle(Vector3.Up,//Quaternion rotation
                                    MathHelper.ToRadians(dudeRotY));
            //dude movement
            dudePos += Vector3.Transform(dudeVel, Matrix.CreateFromQuaternion(dudeQRot));


            //Check each bullet and update it
            for (int i = 0; i < bulletList.Count; i++)
            {
                Bullet b = bulletList[i];
                b.update();

                //Check for bullets and world collisions
                foreach (BoundingBox bbox in bbLevel)
                {

                    if (bbox.Intersects(b.GetRay()) < 1)
                    {
                        b.age = 201;
                        break;
                    }
                }
                //Check for bullet and target collisions
                foreach (Enemy t in targets)
                {

                    if (t.GetBoundingSphere().Intersects(b.GetRay()) < 1)
                    {
                        b.age = 201;
                        t.Position = new Vector3(t.Position.X,
                                                t.Position.Y + 1000,
                                                t.Position.Z);
                        score_enemies += 500;
                        soundBank.PlayCue("kabloo");

                    }
                }
                //If the bullet is old, remove it
                if (b.age > 200) bulletList.Remove(b);
            }
            bulletShape.updateObjects(bulletList.ToArray());

            foreach (Enemy t in targets)
            {
                t.Update(gameTime, dudePos);
            }

            foreach (Collectable t in collectables)
            {
                t.update();
            }

            //Collision Detection(BB Main character)
            BoundingBox bbdude = new BoundingBox(new Vector3(-40, 0, -20) + dudePos,
                                                 new Vector3(40, 100, 20) + dudePos);

            foreach (BoundingBox bb in bbLevel)
            {
                if (bbdude.Intersects(bb))
                {
                    dudePos -= Vector3.Transform(dudeVel, Matrix.CreateFromQuaternion(dudeQRot));
                }

            }

            foreach (Enemy t in targets)
            {
                if (t.GetBoundingSphere().Intersects(bbdude))
                {
                    dudePos -= Vector3.Transform(dudeVel, Matrix.CreateFromQuaternion(dudeQRot));
                }

            }

            

            List<int> intersected = new List<int>();
            for(int i = 0; i < collectables.Count; i++)
            {
                if (collectables[i].GetBoundingSphere().Intersects(bbdude))
                {
                    numCollected++;
                    intersected.Add(i);
                    score_enemies += 100;
                    soundBank.PlayCue("shields1");
                    break;
                }
            }

            for (int i = 0; i < intersected.Count; i++)
                collectables.Remove(collectables[intersected[i]]);

            if (intersected.Count > 0)
                collectableShape.updateObjects(collectables.ToArray());

            if (numCollected >= NUM_COLLECTED_TO_WIN) //You Win!
            {
                WinGame();
                return;
            }

            // TODO: Add your update logic here
            Matrix dudeMatrix = Matrix.CreateScale(1) * Matrix.CreateRotationY((float)MathHelper.Pi * 2) *
                                Matrix.CreateFromQuaternion(dudeQRot) *
                                Matrix.CreateTranslation(dudePos);


            //animationPlayer.Update(gameTime.ElapsedGameTime, true, dudeMatrix);
            clipPlayer.update(gameTime.ElapsedGameTime, true, dudeMatrix);


            base.Update(gameTime);
        }

        /// <summary>
        /// This is called when the game should draw itself.
        /// </summary>
        /// <param name="gameTime">Provides a snapshot of timing values.</param>
        protected override void Draw(GameTime gameTime)
        {
            //GraphicsDevice.BlendState = BlendState.AlphaBlend;
            graphics.GraphicsDevice.Clear(Color.CornflowerBlue);
           
            Matrix[] bones;//This info comes from the effect(.fx)file
            //Matrix[] transforms = new Matrix[dude.Bones.Count];
            //dude.CopyAbsoluteBoneTransformsTo(transforms);

           

            //move the camera with the dude movement

            //view = Matrix.CreateLookAt(new Vector3(0, 200, -500),
            //new Vector3(0, 0, 0), Vector3.Up);

            //view = Matrix.CreateLookAt(new Vector3(0, 15000, -100),
            //        new Vector3(0, 0, 100), Vector3.Up);


            //view = Matrix.CreateLookAt(dudePos + new Vector3(0, 400, -500),
            //                           dudePos + new Vector3(0, 0, 500), Vector3.Up);

            view = Matrix.CreateLookAt(dudePos + Vector3.Transform(new Vector3(0, 600, -800), Matrix.CreateFromQuaternion(dudeQRot)),
                                       dudePos + Vector3.Transform(new Vector3(0, 200, 800), Matrix.CreateFromQuaternion(dudeQRot)),
                                           Vector3.Up);


            //bones = animPlayer.GetSkinTransforms();
            bones = clipPlayer.GetSkinTransforms();

            
            #region Draw Skybox

            graphics.GraphicsDevice.Clear(ClearOptions.Target | ClearOptions.DepthBuffer, Color.Black, 1, 0);

            //device.SamplerStates[0].AddressU = TextureAddressMode.Clamp;
            //device.SamplerStates[0].AddressV = TextureAddressMode.Clamp;
            GraphicsDevice.SamplerStates[0] = SamplerState.LinearClamp;

            device.BlendState = BlendState.AlphaBlend;
            Matrix skyBox = skyboxModel.Meshes[0].ParentBone.Transform;
            foreach (ModelMesh mesh in skyboxModel.Meshes)
            {
                foreach (BasicEffect effect in mesh.Effects)
                {
                    effect.World = skyBox * Matrix.CreateTranslation(dudePos);
                    effect.View = view;
                    effect.Projection = projection;
                }
                mesh.Draw();
            }

            #endregion

            //device.BlendState = BlendState.AlphaBlend;
            //device.SamplerStates[0].AddressU = TextureAddressMode.Wrap;
            //device.SamplerStates[0].AddressV = TextureAddressMode.Wrap;

            //GraphicsDevice.RenderState.FillMode = FillMode.WireFrame;

            GraphicsDevice.BlendState = BlendState.Opaque;
            GraphicsDevice.DepthStencilState = DepthStencilState.Default;
            GraphicsDevice.RasterizerState = RasterizerState.CullCounterClockwise;
            GraphicsDevice.SamplerStates[0] = SamplerState.LinearWrap;

            #region Draw Level

            Matrix levelWorld = level.Meshes[0].ParentBone.Transform *
                                Matrix.CreateTranslation(0, -100, 0);
            foreach (ModelMesh mesh in level.Meshes)
            {
                foreach (BasicEffect effect in mesh.Effects)
                {

                    effect.World = levelWorld;
                    effect.View = view;
                    effect.Projection = projection;
                }
                mesh.Draw();
            }
            #endregion

            #region Draw Dual Texture Level


            foreach (ModelMesh mesh in dual.Meshes)
            {
                List<Texture2D> textures = new List<Texture2D>();

                foreach (DualTextureEffect effect in mesh.Effects)
                {

                    Matrix dualWorld = level.Meshes[0].ParentBone.Transform * Matrix.CreateScale(1.0f) *
                                Matrix.CreateTranslation(0, 0, 0);

                    effect.World = dualWorld;
                    effect.View = view;
                    effect.Projection = projection;

                    effect.DiffuseColor = new Vector3(0.75f);

                    //effect.Parameters["Texture"].SetValue(1);
                    //effect.Parameters["Texture2"].SetValue(1); 
                    // Store the previous textures.
                    textures.Add(effect.Texture);
                    textures.Add(effect.Texture2);

                }
                
                mesh.Draw();
            }
            #endregion
            //Use the Shape class to draw the bullets and targets using XNA build-in BasicEffect
            #region Draw Bullets, Collectables and Targets
            bulletShape.draw(view, projection);
            //targetShape.draw(view, projection);
            collectableShape.draw(view, projection);
            #endregion


            // TODO: Add your drawing code here
            //Red parameters come from the effect
            #region Draw Main Character

            foreach (ModelMesh mesh in dude.Meshes)
            {
                foreach (SkinnedEffect effect in mesh.Effects)
                {

                    effect.SetBoneTransforms(bones);
                    effect.View = view;
                    effect.Projection = projection;
                }
                mesh.Draw();
            }

            #endregion

            #region Draw Enemies

            foreach (Enemy e in targets)
            {
                e.Draw(gameTime, view);
            }

            #endregion

           


            spriteBatch.Begin(SpriteSortMode.Deferred, BlendState.AlphaBlend
                                );
            spriteBatch.DrawString(spriteFont, "Score: " + score_enemies.ToString(),
                                Vector2.Zero, Color.White, 0, Vector2.Zero , 1,
                                SpriteEffects.None, 0);

            spriteBatch.DrawString(spriteFont, "Score: " + score.ToString(),
                                Vector2.Zero, Color.White, 0, new Vector2(10, 30), 1,
                                SpriteEffects.None, 0);

            Color timeLeftColor = Color.White;
            if (timeLeft < 10)
                timeLeftColor = Color.Red;
            spriteBatch.DrawString(spriteFont, timeLeft + " seconds left", new Vector2(10, 20), timeLeftColor);
            spriteBatch.DrawString(spriteFont, numCollected + " out of " + NUM_COLLECTED_TO_WIN + " collected", new Vector2(10, 40), Color.White);

            if (gamePaused)
            {
                if (timeLeft <= 0) //Lost the game!
                {
                    Vector2 center = screenCenter - (spriteFont.MeasureString("Game Over!") / 2.0f);
                    spriteBatch.DrawString(spriteFont, "Game Over!", center, Color.White);
                }
                else if (numCollected >= NUM_COLLECTED_TO_WIN)
                {
                    Vector2 center = screenCenter - (spriteFont.MeasureString("You Win!") / 2.0f);
                    spriteBatch.DrawString(spriteFont, "You Win!", center, Color.White);
                }

            }
            spriteBatch.End();
            base.Draw(gameTime);
        }
    }
}
