using System;
using Microsoft.Xna.Framework;
using System.Collections.Generic;
using Microsoft.Xna.Framework.Graphics;
using System.Linq;
using System.Text;

namespace RabiesX
{
    public class Quad
    {
 
        #region Fields
 
        public VertexPositionTexture[] vertices;
        public short[] indices;
 
        Vector3 Origin;
        Vector3 Up;
 
        Vector3 Left;
        Vector3 UpperLeft;
        Vector3 UpperRight;
        Vector3 LowerLeft;
        Vector3 LowerRight;
 
        VertexBuffer vertexBuffer;
        IndexBuffer indexBuffer;
 
        GraphicsDevice Device;
        #endregion
 
        #region Properties
        public VertexBuffer VertexBuffer
        {
            get
            {
                return vertexBuffer;
            } 
        }
 
        public IndexBuffer IndexBuffer
        {
            get
            { 
                return indexBuffer;
            } 
        }
 
        #endregion
        public Quad(GraphicsDevice device, Vector3 origin, Vector3 up, float width, float height)
        {
 
            vertices = new VertexPositionTexture[4];
            indices = new short[6];
            Origin = origin;
            Up = up;
            Device = device;
 
            // Calculate the quad corners
            Left = Vector3.Cross(Vector3.Backward, Up);
            Vector3 uppercenter = (Up * height / 2) + origin;
            UpperLeft = uppercenter + (Left * width / 2);
            UpperRight = uppercenter - (Left * width / 2);
            LowerLeft = UpperLeft - (Up * height);
            LowerRight = UpperRight - (Up * height);
 
            SetUpVertices(); 
        }

        private void SetUpVertices()
        {
            // Fill in texture coordinates to display full texture
            // on quad
            Vector2 textureUpperLeft = new Vector2(0.0f, 0.0f);
            Vector2 textureUpperRight = new Vector2(1.0f, 0.0f);
            Vector2 textureLowerLeft = new Vector2(0.0f, 1.0f);
            Vector2 textureLowerRight = new Vector2(1.0f, 1.0f);
 
            // Create 4 vertices
            vertices[0] = new VertexPositionTexture(LowerLeft, textureLowerLeft);
            vertices[1] = new VertexPositionTexture(UpperLeft, textureUpperLeft);
            vertices[2] = new VertexPositionTexture(LowerRight, textureLowerRight);
            vertices[3] = new VertexPositionTexture(UpperRight, textureUpperRight);
 
            // Set the index buffer for each vertex, using
            // clockwise winding
            indices[0] = 0;
            indices[1] = 1;
            indices[2] = 2;
            indices[3] = 2;
            indices[4] = 1;
            indices[5] = 3;

            // Initialize the VertexBuffer, and insert the data
            this.vertexBuffer = new VertexBuffer(Device, typeof(VertexPositionTexture), vertices.Length, BufferUsage.WriteOnly);
            this.vertexBuffer.SetData(vertices);
 
            // Initialize the IndexBuffer, and insert the data
            indexBuffer = new IndexBuffer(Device, typeof(short), indices.Length, BufferUsage.WriteOnly);
            indexBuffer.SetData(indices);
         } 
    }
}