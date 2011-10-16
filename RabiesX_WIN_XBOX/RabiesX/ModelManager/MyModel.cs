#region File Description
//-----------------------------------------------------------------------------
// MyModel.cs
//
// Microsoft XNA Community Game Platform
// Copyright (C) Microsoft Corporation. All rights reserved.
//-----------------------------------------------------------------------------
#endregion

#region Using Statements
using System;
using System.Collections.Generic;
using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Audio;
using Microsoft.Xna.Framework.Content;
using Microsoft.Xna.Framework.Graphics;
using Microsoft.Xna.Framework.Input;
using Microsoft.Xna.Framework.Storage;
using System.Linq;
using System.Text;
#endregion

namespace RabiesX
{
    public class MyModel
    {
        private Model model;
        private Texture2D texture;
        private Vector3 position = Vector3.Zero;        
        private Vector3 rotation = Vector3.Zero;
        private Vector3 velocity = Vector3.Zero;
        private float rotationf = 0.0f;

        public MyModel(String AssetPath, ContentManager content)        
        {           
            model = content.Load<Model>(AssetPath);
        }         
        
        public Model ModelHeld        
        {   
            get            
            {                
                return model;            
            }        
        }

        public void Texture(String AssetPath, ContentManager content)
        {
            texture = content.Load<Texture2D>(AssetPath);
        }
            
        #region position         
        
        public Vector3 Position        
        {            
            get
            {
                return position;
            }
            set
            {
                position = value;
            }
        }         
        
        public float pX        
        {            
            set
            {
                position.X = value;
            }            
            get
            {
                return position.X;
            }        
        }         
        
        public float pY        
        {            
            set
            { 
                position.Y = value;
            }            
            get 
            { 
                return position.Y;
            }        
        }        
        
        public float pZ        
        {            
            set
            { 
                position.Z = value;
            }            
            get
            { 
                return position.Z;
            }        
        }         
        #endregion #position         
        
        #region rotation         
        
        public Vector3 Rotation
        {
            get
            {
                return rotation;
            }
            set
            {
                rotation = value;
            }
        }

        public float Rotationf
        {
            get
            {
                return rotationf;
            }
            set
            {
                rotationf = value;
            }
        }         

        public float rX        
        {            
            get
            { 
                return rotation.X;
            }            
            set 
            { 
                rotation.X = value; 
            }       
        }         
        
        public float rY       
        {       
            get
            { 
                return rotation.Y; 
            }            
            set 
            { 
                rotation.Y = value;
            }        
        }        
        
        public float rZ      
        {            
            get
            { 
                return rotation.Z;
            }            
            set
            { 
                rotation.Z = value; 
            }       
        }        
        #endregion rotation         
        
        #region velocity

        public Vector3 Velocity
        {
            get
            {
                return velocity;
            }
            set
            {
                velocity = value;
            }
        }
        
        #endregion velocity

        public Matrix getRotation
        {
            get
            {
                return Matrix.CreateRotationX(MathHelper.ToRadians(rX)) * Matrix.CreateRotationY(MathHelper.ToRadians(rY)) * Matrix.CreateRotationZ(MathHelper.ToRadians(rZ));
            }
        }

        public Texture2D getTexture
        {
            get
            {
                return (texture);
            }
        }
    }
}
