using System;
using System.Collections.Generic;
using System.Text;
using Microsoft.Xna.Framework;

//Credit goes to Mr. Jose Baez-Franceschi. This is from his squirrel game.

namespace RabiesX
{
    class Target : Transform
    {
        
        float spin = 0;
        float respawnTimer = 0;


        public Target(Vector3 pos)
        {
            position = pos + new Vector3(0,120,0);
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

        public BoundingSphere GetBoundingSphere()
        {
            return new BoundingSphere(position, 150); ;
        }

    }
}
