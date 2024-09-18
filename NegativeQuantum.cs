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
        }

        public NegativeQuantum(Vector2 position, Vector2 size, float attractSpeed) : base(position, size, attractSpeed) { }

        public NegativeQuantum(Vector2 position, Vector2 size, float attractSpeed, bool hasGravity) :
            base(position, size, attractSpeed, hasGravity){ }
        public NegativeQuantum(Vector2 position, Vector2 size, float attractSpeed, bool hasGravity, bool doesRemoveOnExplode) :
            base(position, size, attractSpeed, hasGravity, doesRemoveOnExplode)
        { }
        public NegativeQuantum(EntityData data, Vector2 offset) : base(data, offset) { }

        public override void Render()
        {
            base.Render();
            Color c = Color.Black;
            c.A = 255;
            Draw.Rect(new Vector2(Position.X - 4, Position.Y - 4), 8, 8, c);
        }
        public override void Update()
        {
            foreach (Entity entity in Scene.Tracker.GetEntities<PositiveQuantum>())
            {
                PositiveQuantum positiveQuantum = entity as PositiveQuantum;
                if (this.Coincides(positiveQuantum))
                {
                    this.Position = this.previousPosition;
                }
                if (positiveQuantum != null && positiveQuantum.state != Quantum.States.Gone && this.isInited && !this.Hold.IsHeld)
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
                if (negativeQuantum != null && negativeQuantum != this && negativeQuantum.state != Quantum.States.Gone && this.isInited && !this.Hold.IsHeld)
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
        }
    }
}
