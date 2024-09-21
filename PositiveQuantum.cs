using Celeste.Mod.Entities;
using System;
using System.Collections.Generic;
using System.Drawing;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace ContemporaryPhysicsHelper
{
    [CustomEntity("ContemporaryPhysicsHelper/PositiveQuantum")]
    [Tracked]
    public class PositiveQuantum : Quantum
    {
        public PositiveQuantum(Vector2 position, Vector2 size) : base(position, size)
        {
            base.isPositive = true;
        }

        public PositiveQuantum(Vector2 position, Vector2 size, float attractSpeed) : base(position, size, attractSpeed) { }
        
        public PositiveQuantum(Vector2 position, Vector2 size, float attractSpeed, bool hasGravity) :
            base(position, size, attractSpeed, hasGravity) { }
        public PositiveQuantum(Vector2 position, Vector2 size, float attractSpeed, bool hasGravity, bool doesRemoveOnExplode) :
            base(position, size, attractSpeed, hasGravity, doesRemoveOnExplode) { }

        public PositiveQuantum(EntityData data, Vector2 offset) : base(data, offset) { }

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
            } else
            {
                base.Render();
                Microsoft.Xna.Framework.Color c = Microsoft.Xna.Framework.Color.White;
                c.A = 255;
                Draw.Rect(new Vector2(Position.X - 8, Position.Y - 8), 16, 16, c);
            }
        }

        public override void Update()
        {
            foreach (Entity entity in Scene.Tracker.GetEntities<NegativeQuantum>())
            {
                NegativeQuantum negativeQuantum = entity as NegativeQuantum;
                if (negativeQuantum != null && this.CollideCheck(negativeQuantum))
                {
                    base.Explode((this.Position + negativeQuantum.Position) / 2);
                    base.GotoGone();
                    negativeQuantum.Explode((this.Position + negativeQuantum.Position) / 2);
                    negativeQuantum.GotoGone();
                    if (negativeQuantum.doesRemoveOnExplode)
                    {
                        negativeQuantum.RemoveSelf();
                    }
                }
                if (this.Coincides(negativeQuantum))
                {
                    this.Position = this.previousPosition;
                }
                if (negativeQuantum != null && negativeQuantum.state == Quantum.States.Idle && this.isInited && !this.Hold.IsHeld
                    && this.state == Quantum.States.Idle)
                {
                    Vector2 r;
                    if (negativeQuantum.quantumId < this.quantumId)
                    {
                        r = negativeQuantum.previousPosition - this.Position;
                    }
                    else
                    {
                        r = negativeQuantum.Position - this.Position;
                    }
                    this.Speed = this.Speed + attractMultiplier * (this.attractSpeed * Engine.DeltaTime / (r.Length() * r.LengthSquared())) * r;
                }
            }
            foreach (Entity entity in Scene.Tracker.GetEntities<PositiveQuantum>())
            {
                PositiveQuantum positiveQuantum = entity as PositiveQuantum;
                if (positiveQuantum != this && this.Coincides(positiveQuantum))
                {
                    this.Position = this.previousPosition;
                }
                if (positiveQuantum != null && positiveQuantum != this && positiveQuantum.state != Quantum.States.Gone && this.isInited && !this.Hold.IsHeld
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
                    this.Speed = this.Speed - attractMultiplier * (this.attractSpeed * Engine.DeltaTime / (r.Length() * r.LengthSquared())) * r;
                }
            }
            base.Update();
        }
    }
}
