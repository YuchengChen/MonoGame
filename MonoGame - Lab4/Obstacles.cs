﻿using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Graphics;
using System.Diagnostics;

namespace MonoGame___Lab4 {
   class Obstacles : GameComponent {
      private Model objModel;
      private Matrix worldMatrix, orientation;
      private Vector3 position;
      private Vector3 rotation;
      private Vector3 lookAt;
      private float scaleSize;
      private float moveSpeed;
      private Character main;
      private BoundingSphere collider = new BoundingSphere();

      public Vector3 Position {
         get { return position; }
         set {
            position = value;
            UpdateLookAt();
         }
      }

      public Vector3 Rotation {
         get { return rotation; }
         set {
            rotation = value;
            UpdateLookAt();
         }
      }

      //create new character/obstacles
      public Obstacles(Game1 game, Model model, Vector3 pos, float speed, float scale, Matrix world, Character target) : base(game) {
         objModel = model;
         position = pos;
         orientation = world;
         moveSpeed = speed;
         scaleSize = scale;
         main = target;
      }

      //move character to position
      private void MoveTo(Vector3 pos, Vector3 rot) {
         Position = pos;
         Rotation = rot;
      }

      //Preview position to detect collision
      private Vector3 PreviewMove(Vector3 amount) {
         Matrix rotate = Matrix.CreateRotationY(rotation.Y);
         Vector3 movement = new Vector3(amount.X, amount.Y, amount.Z);
         movement = Vector3.Transform(movement, rotate);
         var previewPos = position + movement;
         previewPos.X = MathHelper.Clamp(previewPos.X, -Game1.MAPSIZE * 0.95f, Game1.MAPSIZE * 0.95f);
         return previewPos;
      }

      private void Move(Vector3 scale) {
         MoveTo(PreviewMove(scale), Rotation);
      }

      //change look at direction
      private void UpdateLookAt() {
         Matrix rotationMatrix = Matrix.CreateRotationY(rotation.Y);
         Vector3 lookAtOffset = Vector3.Transform(Vector3.UnitZ, rotationMatrix);
         lookAt = position + lookAtOffset;
      }

      //moving obstacles(bullets)
      private void Fire(float deltaTime) {
         Vector3 moveVector = new Vector3();
         moveVector.Z = -0.1f;
         moveVector *= deltaTime * moveSpeed;
         Move(moveVector);
      }

      public override void Update(GameTime gameTime) {
         float dt = (float)gameTime.ElapsedGameTime.TotalSeconds;
         Fire(dt);

         main.onCollision(collider);

         base.Update(gameTime);
      }

      public void Draw(Camera camera, Character main) {
         Matrix[] transforms = new Matrix[objModel.Bones.Count];
         objModel.CopyAbsoluteBoneTransformsTo(transforms);

         foreach (var mesh in objModel.Meshes) {
            foreach (BasicEffect effect in mesh.Effects) {
               effect.EnableDefaultLighting();
               effect.DiffuseColor = Color.White.ToVector3();

               var scale = Matrix.CreateScale(scaleSize);
               worldMatrix = transforms[mesh.ParentBone.Index] * scale * orientation * Matrix.CreateRotationY(Rotation.Y)
                 * Matrix.CreateTranslation(Position);
               effect.World = worldMatrix;

               if (collider.Radius == 0)
                  collider = mesh.BoundingSphere;
               else
                  collider = BoundingSphere.CreateMerged(collider, mesh.BoundingSphere);

               camera.Display(effect);
            }
            mesh.Draw();
         }
         collider.Center = position;
         //collider.Radius *= scaleSize;
         collider.Transform(worldMatrix);
         Debug.WriteLine(collider.Center);
      }
   }
}