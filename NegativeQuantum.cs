using Celeste.Mod.Entities;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace ContemporaryPhysicsHelper
{
    [CustomEntity("ContemporaryPhysicsHelper/NegativeQuantum")]
    [Tracked]
    public class NegativeQuantum : Quantum
    {
        public NegativeQuantum(Vector2 position, Vector2 size) : base(position, size) 
        {
            base.isPositive = false;
            //MTexture tex = GFX.Game["objects/NegativeQuantum/NegativeQuantum"];
            //Image image = new(tex);
            //image.SetOrigin(8f, 8f);
            //this.Add(image);
            base.Add(base.sprite = Celeste.Mod.ContemporaryPhysicsHelper.ContemporaryPhysicsHelperModule.NQSpriteBank.Create("negativeQuantum"));
            base.sprite.Position = base.sprite.Position - new Vector2(32f, 32f);
        }

        public NegativeQuantum(Vector2 position, Vector2 size, float attractSpeed) : base(position, size, attractSpeed) 
        {
            //MTexture tex = GFX.Game["objects/NegativeQuantum/NegativeQuantum"];
            //Image image = new(tex);
            //image.SetOrigin(8f, 8f);
            //this.Add(image);
            base.Add(base.sprite = Celeste.Mod.ContemporaryPhysicsHelper.ContemporaryPhysicsHelperModule.NQSpriteBank.Create("negativeQuantum"));
            base.sprite.Position = base.sprite.Position - new Vector2(32f, 32f);
        }

        public NegativeQuantum(Vector2 position, Vector2 size, float attractSpeed, bool hasGravity) :
            base(position, size, attractSpeed, hasGravity)
        {
            //MTexture tex = GFX.Game["objects/NegativeQuantum/NegativeQuantum"];
            //Image image = new(tex);
            //image.SetOrigin(8f, 8f);
            //this.Add(image);
            base.Add(base.sprite = Celeste.Mod.ContemporaryPhysicsHelper.ContemporaryPhysicsHelperModule.NQSpriteBank.Create("negativeQuantum"));
            base.sprite.Position = base.sprite.Position - new Vector2(32f, 32f);
        }
        public NegativeQuantum(Vector2 position, Vector2 size, float attractSpeed, bool hasGravity, bool doesRemoveOnExplode) :
            base(position, size, attractSpeed, hasGravity, doesRemoveOnExplode)
        {
            //MTexture tex = GFX.Game["objects/NegativeQuantum/NegativeQuantum"];
            //Image image = new(tex);
            //image.SetOrigin(8f, 8f);
            //this.Add(image);
            base.Add(base.sprite = Celeste.Mod.ContemporaryPhysicsHelper.ContemporaryPhysicsHelperModule.NQSpriteBank.Create("negativeQuantum"));
            base.sprite.Position = base.sprite.Position - new Vector2(32f, 32f);
        }
        public NegativeQuantum(EntityData data, Vector2 offset) : base(data, offset)
        {
            //MTexture tex = GFX.Game["objects/NegativeQuantum/NegativeQuantum"];
            //Image image = new(tex);
            //image.SetOrigin(8f, 8f);
            //this.Add(image);//
            base.Add(base.sprite = Celeste.Mod.ContemporaryPhysicsHelper.ContemporaryPhysicsHelperModule.NQSpriteBank.Create("negativeQuantum"));
            base.sprite.Position = base.sprite.Position - new Vector2(32f, 32f);
        }

        public override void Render()
        {
            bool flag = false;
            if (this.sprite.CurrentAnimationID == "explode")
            {
                flag = true;
            }
            else if (this.sprite.CurrentAnimationID == "hidden")
            {
                flag = true;
            }
            if (flag)
            {
                this.sprite.DrawSimpleOutline();
                base.Render();
            }
            else
            {
                base.Render();
                //
                //Microsoft.Xna.Framework.Color c = Microsoft.Xna.Framework.Color.Black;
                //c.A = 255;
                //Draw.Rect(new Vector2(Position.X - 8, Position.Y - 8), 16, 16, c);
            }
        }
        public override void Update()
        {
            List<Entity> NegativeBarrier = this.Scene.Tracker.GetEntities<NegativeBarrier>().ToList();
            NegativeBarrier.ForEach(entity => entity.Collidable = true);
            List<Entity> QuantumBarrier = this.Scene.Tracker.GetEntities<QuantumBarrier>().ToList();
            QuantumBarrier.ForEach(entity => entity.Collidable = true);
            foreach (Entity entity in Scene.Tracker.GetEntities<PositiveQuantum>())
            {
                PositiveQuantum positiveQuantum = entity as PositiveQuantum;
                if (this.Coincides(positiveQuantum))
                {
                    this.Position = this.previousPosition;
                }
                if (positiveQuantum != null && positiveQuantum.state != Quantum.States.Gone && this.isInited && !this.Hold.IsHeld
                    && this.state == Quantum.States.Idle)
                {
                    Vector2 r;
                    if (positiveQuantum.quantumId < this.quantumId)
                    {
                        r = positiveQuantum.previousPosition - this.Position;
                    }
                    else
                    {
                        r = positiveQuantum.Position - this.Position;
                    }
                    this.Speed = this.Speed + attractMultiplier * (this.attractSpeed * Engine.DeltaTime / (r.Length() * r.LengthSquared())) * r;
                }
            }
            foreach (Entity entity in Scene.Tracker.GetEntities<NegativeQuantum>())
            {
                NegativeQuantum negativeQuantum = entity as NegativeQuantum;
                if (negativeQuantum != this && this.Coincides(negativeQuantum))
                {
                    this.Position = this.previousPosition;
                }
                if (negativeQuantum != null && negativeQuantum != this && negativeQuantum.state != Quantum.States.Gone && this.isInited && !this.Hold.IsHeld
                    && this.state == Quantum.States.Idle)
                {
                    Vector2 r;
                    if(negativeQuantum.quantumId < this.quantumId)
                    {
                        r = negativeQuantum.previousPosition - this.Position;
                    }
                    else
                    {
                        r = negativeQuantum.Position - this.Position;
                    }
                    this.Speed = this.Speed - attractMultiplier * (this.attractSpeed * Engine.DeltaTime / (r.Length() * r.LengthSquared())) * r;
                }
            }
            base.Update();
            NegativeBarrier.ForEach(entity => entity.Collidable = false);
            QuantumBarrier.ForEach(entity => entity.Collidable = false);
        }
    }
}
