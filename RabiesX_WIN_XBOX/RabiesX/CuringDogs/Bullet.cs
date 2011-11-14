using System;
using System.Collections.Generic;
using System.Text;
using Microsoft.Xna.Framework;

//Credit goes to Mr. Jose Baez-Franceschi. This is from his squirrel game.

namespace RabiesX
{
    class Bullet : Transform
    {
        public float age;

        public Bullet(Vector3 initPos, Quaternion initQRot)
        {
            age = 0;
            position = initPos;
            qRotation = initQRot;
            velocity = new Vector3(0, 0, 400);
        }

        new public void update()
        {
            age += 1;
            position += Vector3.Transform(velocity, qRotation);
        }

        public Ray GetRay()
        {
            return new Ray(position, Vector3.Normalize(position));
        }
    }
}
