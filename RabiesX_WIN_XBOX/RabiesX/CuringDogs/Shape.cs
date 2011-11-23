using System;
using System.Collections.Generic;
using System.Text;
using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Graphics;

//Credit goes to Mr. Jose Baez-Franceschi. This is from his squirrel game.

//Use this class to transform and draw objects with the basic effect 
namespace RabiesX
{
    class Shape
    {
        protected Model model;
        protected Transform[] objects;
       

        public Shape(Model model, Transform[] objects)
        {
            this.model = model;
            this.objects = objects;
        }

        public void updateObjects(Transform[] objects)
        {
            this.objects = objects;
        }

        public void draw(Matrix view, Matrix projection)
        {
            foreach (Transform obj in objects)
            {
                Matrix[] xforms = new Matrix[model.Bones.Count];
                model.CopyAbsoluteBoneTransformsTo(xforms);
                Matrix world = xforms[model.Meshes[0].ParentBone.Index]
                        * obj.RotationMatrix * obj.TransalationMatrix;
                foreach (ModelMesh mesh in model.Meshes)
                {
                    foreach (BasicEffect bEff in mesh.Effects)
                    {
                        // Set the effect for drawing the component
                        bEff.EnableDefaultLighting();
                        bEff.SpecularColor = new Vector3(0.25f);
                        bEff.SpecularPower = 16;


                        bEff.World = world;
                        bEff.View = view;
                        bEff.Projection = projection;
                    }
                    mesh.Draw();
                }
            }
        }
    }
}
