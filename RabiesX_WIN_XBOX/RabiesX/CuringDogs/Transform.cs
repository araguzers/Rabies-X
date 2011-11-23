using System;
using System.Collections.Generic;
using System.Text;
using Microsoft.Xna.Framework;

//Credit goes to Mr. Jose Baez-Franceschi. This is from his squirrel game.

//get and set tranformations
namespace RabiesX
{
    class Transform
    {
        protected Vector3 position = Vector3.Zero;
        protected Vector3 velocity = Vector3.Zero;
        protected Quaternion qRotation = Quaternion.Identity;

        public Vector3 Position
        {
            get { return position; }
            set { position = value; }
        }

        public Vector3 Velocity
        {
            get { return velocity; }
            set { velocity = value; }
        }

        public Quaternion QRotation
        {
            get { return qRotation; }
            set { qRotation = value; }
        }

        public Matrix TransalationMatrix
        {
            get { return Matrix.CreateTranslation(position); }
        }

        public Matrix RotationMatrix
        {
            get { return Matrix.CreateFromQuaternion(qRotation); }
        }

        public Transform()
        {

        }

        public void update()
        {

        }
    }
}
