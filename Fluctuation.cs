using Celeste.Mod.Entities;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace ContemporaryPhysicsHelper
{
    [CustomEntity("ContemporaryPhysicsHelper/Fluctuation")]
    public class Fluctuation : Entity
    {
        private float initTime = 2f;
        private float initTimer;
        private Vector2 initVelocity = new Vector2(10f, 0f);
        private Scene scene;
        public Fluctuation(Vector2 position, Vector2 size) : base(position)
        {
            this.initTimer = this.initTime;
        }
        
        public Fluctuation(Vector2 position, Vector2 size, float initTime, Vector2 initVelocity) : 
            this(position, size)
        {
            this.initTime = initTime;
            this.initVelocity = initVelocity;
        }

        public Fluctuation(EntityData data, Vector2 offset) : this(data.Position + offset, new Vector2(data.Width, data.Height),
            data.Float("initTime"), new Vector2(data.Float("XSpeed"), data.Float("YSpeed")))
        { }

        public override void Update()
        {
            base.Update();
            if (this.initTimer > 0)
            {
                this.initTimer -= Engine.DeltaTime;
            }
            else
            {
                this.initTimer = this.initTime;
                Vector2 pqPos = this.Position + 8 * this.initVelocity / this.initVelocity.Length();
                Vector2 nqPos = 2 * this.Position - pqPos;
                Vector2 size = new Vector2(16, 16);
                PositiveQuantum positiveQuantum = new PositiveQuantum(pqPos, size, 1.0f, false, true);
                NegativeQuantum negativeQuantum = new NegativeQuantum(nqPos, size, 1.0f, false, true);
                positiveQuantum.Speed = this.initVelocity;
                negativeQuantum.Speed = -1 * this.initVelocity;
                scene.Add(negativeQuantum);
                scene.Add(positiveQuantum);
            }
        }
        public override void Added(Scene scene)
        {
            base.Added(scene);
            this.scene = scene;
        }
    }
}
