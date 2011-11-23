using System;
using System.Collections.Generic;
using System.Text;
using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Graphics;
using Microsoft.Xna.Framework.Content;
//using SkinnedModel;

//Credit goes to Mr. Jose Baez-Franceschi. This is from his squirrel game.

namespace RabiesX
{
    public enum State { Idle, Chase };
    
    class Enemy : Transform
    {
        State currentState = State.Idle;
        float spin = 0;
        float respawnTimer = 0;
        BoundingSphere boundingSphere;
        BoundingSphere chaseSphere; 
        BoundingSphere intersectionTestSphere;
        Vector3 oldPosition = Vector3.Zero;
        BoundingSphere playerBoundingPosition;


        //Enemy Speed Settings
        const float starting_speed = 90.0f;
        const int distance_to_search_for_player = 3000;
        const int minimum_distance_to_player = 500;

        //Enemy Animation Items
        Model model;
        Matrix viewMatrix;
        Matrix worldMatrix;
        Matrix[] bones;
        SkinningData skd;
        SkinningData skdTarget;
        ClipPlayer clipPlayer;
        Matrix enemyMatrix;
        Matrix projection;
        

        //For turning enemy
        Vector3 dirToPlayer = Vector3.Zero;
        float currentSpeed = 0;
        Vector3 newVelocity = Vector3.Zero;

        public BoundingSphere BoundingSphere {
            get { return BoundingSphere; }
        }


        public Enemy(Vector3 pos, Model model, Matrix projection)
        {
            position = pos + new Vector3(0, 0, 0);
            intersectionTestSphere = new BoundingSphere(Vector3.Zero, 10);
            playerBoundingPosition = new BoundingSphere(Vector3.Zero, minimum_distance_to_player);
            chaseSphere = new BoundingSphere(position, distance_to_search_for_player);
            boundingSphere = new BoundingSphere(position, 150);
            this.model = model;
            viewMatrix = Matrix.Identity;
            worldMatrix = Matrix.Identity;
            skd = model.Tag as SkinningData;
            clipPlayer = new ClipPlayer(skd, 60);//ClipPlayer running at 24 frames/sec
            AnimationClip clip = skd.AnimationClips["Take 002"]; //Take name from the dude.fbx file
            clipPlayer.play(clip, 99, 124, true);
            this.projection = projection;

            enemyMatrix = Matrix.CreateScale(1) * Matrix.CreateRotationY((float)MathHelper.Pi * 2) *
                //Matrix.CreateFromQuaternion(dudeQRot) *
                               Matrix.CreateTranslation(position);
        }

        new public void update()
        {
            spin += 0.01f;


            qRotation = Quaternion.CreateFromAxisAngle(
                            Vector3.Up,
                            spin);

            if (position.Y > 999) respawnTimer += 1;

            if (respawnTimer > 200)
            {
                position.Y = 120;
                respawnTimer = 0;
            }
        }

        public void Update(GameTime gameTime, Vector3 playerPosition)
        {
            //Check if player within bounds
            intersectionTestSphere.Center = playerPosition;
            if (chaseSphere.Intersects(intersectionTestSphere))
            {
                currentState = State.Chase;
                if (!clipPlayer.inRange(99, 124))
                    clipPlayer.switchRange(99, 124);
            }
            else
            {
                currentState = State.Idle;
                velocity = Vector3.Zero;
                if (!clipPlayer.inRange(1, 24))
                    clipPlayer.switchRange(1, 24);
            }

            //Currently Chasing Player
            if(currentState == State.Chase)
            {
                TurnToPlayer(playerPosition);
            }

            //if position has changed, then 
            if (position != oldPosition)
            {
                chaseSphere = new BoundingSphere(position, distance_to_search_for_player);
                boundingSphere = new BoundingSphere(position, 150);
                oldPosition = position;
 
                LookDirectionMoving(playerPosition);
                enemyMatrix = Matrix.CreateScale(1) * Matrix.CreateRotationY((float)MathHelper.Pi * 2) *
                                Matrix.CreateFromQuaternion(qRotation) *
                                Matrix.CreateTranslation(position);

                
            }

            clipPlayer.update(gameTime.ElapsedGameTime, true, enemyMatrix);

            base.update();
        }

        public BoundingSphere GetBoundingSphere()
        {
            return boundingSphere;
        }

        private void TurnToPlayer(Vector3 playerPosition)
        {
            dirToPlayer = Vector3.Normalize(playerPosition - position);
            currentSpeed = velocity.Length();
            if (currentSpeed == 0)
            {
                currentSpeed = starting_speed;
            }
            newVelocity = dirToPlayer * currentSpeed;
            velocity = (0.10f * newVelocity) + (0.90f * velocity);

            playerBoundingPosition.Center = playerPosition;
            intersectionTestSphere.Center = position + velocity;
            if (!playerBoundingPosition.Intersects(intersectionTestSphere))
            {
                velocity.Y = 0.0f;
                position += velocity;
            }
            else
            {
                //enemy is really close to the player, ATTACK!
            }
        }

        private void LookDirectionMoving(Vector3 playerPosition)
        {
            float slerp = 1.0f;
            Matrix matrixModel = Matrix.CreateFromQuaternion(qRotation);
            Matrix matrixLook = Matrix.CreateWorld(position, (position - playerPosition), matrixModel.Up);
            Quaternion finalRotation = Quaternion.CreateFromRotationMatrix(matrixLook);
            Quaternion.Slerp(ref qRotation, ref finalRotation, slerp, out qRotation);
        }

        public void Draw(GameTime gameTime, Matrix view)
        {
            bones = clipPlayer.GetSkinTransforms(); 

            foreach (ModelMesh mesh in model.Meshes)
            {
                foreach (SkinnedEffect effect in mesh.Effects)
                {

                    effect.SetBoneTransforms(bones);
                    effect.View = view;
                    effect.Projection = projection;
                }
                mesh.Draw();
            }

        }

    }
}
