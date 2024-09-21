using Celeste.Mod.Entities;
using System.Linq;

namespace ContemporaryPhysicsHelper;

[CustomEntity("ContemporaryPhysicsHelper/Quantum")]
[Tracked(true)]
public class Quantum : Actor
{
    public Boolean isPositive;
    public Boolean isInited;
    public Vector2 Speed;
    public static int attractMultiplier = 250000;
    public float attractSpeed = 1f;
    public Holdable Hold;
    private float noGravityTimer = 0.1f;
    private SimpleCurve returnCurve;
    private float goneTimer;
    private Vector2 startPosition;
    private bool hasGravity;
    private Collision onCollideH;
    private Collision onCollideV;
    private float hardVerticalHitSoundCooldown = 0.5f;
    private Vector2 prevLiftSpeed;
    public Vector2 previousPosition;
    private Level Level;
    public Quantum.States state;
    public Sprite sprite;
    public bool doesRemoveOnExplode;
    private float cantExplodeTimer;
    public static int quantumCnt = 0;
    public int quantumId;
    public enum States
    {
        Idle,
        Exploding,
        Gone
    }

    public Quantum(Vector2 position, Vector2 size) : base(position)
    {
        this.previousPosition = position;
        this.startPosition = this.Position;
        Hitbox hitbox = new(size.X, size.Y);
        Collider = hitbox;
        this.Collider = new Hitbox(16f, 16f, -8f, -8f);
        base.Depth = 100;
        base.Add(this.Hold = new Holdable(0.1f));
        this.Hold.PickupCollider = new Hitbox(24f, 28f, -12f, -14f);
        this.Hold.SlowFall = false;
        this.Hold.SlowRun = true;
        this.Hold.OnRelease = new Action<Vector2>(this.OnRelease);
        this.Hold.OnPickup = new Action(this.OnPickup);
        this.LiftSpeedGraceTime = 0.1f;
        base.Add(new VertexLight(base.Collider.Center, Color.White, 1f, 32, 64));
        base.Tag = Tags.TransitionUpdate;
        base.Add(new MirrorReflection());
        this.onCollideH = new Collision(this.OnCollideH);
        this.onCollideV = new Collision(this.OnCollideV);
        this.startPosition = position;
        this.isInited = true;
        base.Add(this.sprite = GFX.SpriteBank.Create("pufferFish"));
    }



    public Quantum(Vector2 position, Vector2 size,  float attractSpeed) : this(position, size)
    {
        this.attractSpeed = attractSpeed;
    }

    public Quantum(Vector2 position, Vector2 size, float attractSpeed, bool hasGravity) : this(position, size, attractSpeed)
    {
        this.hasGravity = hasGravity;
    }

    public Quantum(Vector2 position, Vector2 size, float attractSpeed, bool hasGravity, bool doesRemoveOnExplode) : 
        this(position, size, attractSpeed, hasGravity)
    {
        this.doesRemoveOnExplode = doesRemoveOnExplode;
    }

    public Quantum(EntityData data, Vector2 offset)
        : this(data.Position + offset, new Vector2(data.Width, data.Height), data.Float("attractSpeed"), data.Bool("hasGravity"),
              data.Bool("doesRemoveOnExplode"))
    { }
    

    public override void Added(Scene scene)
    {
        base.Added(scene);
        this.Level = base.SceneAs<Level>();
        this.quantumId = quantumCnt++;
    }

    public override void Update()
    {
        base.Update();
        if (this.state == Quantum.States.Gone)
        {
            float num = this.goneTimer;
            this.goneTimer -= Engine.DeltaTime;
            if (this.goneTimer <= 0.5f)
            {
                if (num > 0.5f)
                {
                    Vector2 vector = this.Position + (this.startPosition - this.Position) * 0.5f;
                    if ((this.startPosition - this.Position).LengthSquared() > 100f)
                    {
                        if (Math.Abs(this.Position.Y - this.startPosition.Y) > Math.Abs(this.Position.X - this.startPosition.X))
                        {
                            if (this.Position.X > this.startPosition.X)
                            {
                                vector += Vector2.UnitX * -24f;
                            }
                            else
                            {
                                vector += Vector2.UnitX * 24f;
                            }
                        }
                        else if (this.Position.Y > this.startPosition.Y)
                        {
                            vector += Vector2.UnitY * -24f;
                        }
                        else
                        {
                            vector += Vector2.UnitY * 24f;
                        }
                    }
                    this.returnCurve = new SimpleCurve(this.Position, this.startPosition, vector);
                    if (this.returnCurve.GetLengthParametric(8) > 8f)
                    {
                        Audio.Play("event:/new_content/game/10_farewell/puffer_return", this.Position);
                    }
                }
                this.Position = this.returnCurve.GetPoint(Ease.CubeInOut(Calc.ClampedMap(this.goneTimer, 0.5f, 0f, 0f, 1f)));
            }
            if (this.goneTimer <= 0f)
            {
                this.Visible = (this.Collidable = true);
                this.GotoIdle();
                return;
            }
        }
        if (this.Hold.IsHeld)
        {
            this.isInited = true;
        }
        if (!this.isInited)
        {
            return;
        }
        if (this.state != Quantum.States.Gone && this.cantExplodeTimer > 0f)
        {
            this.cantExplodeTimer -= Engine.DeltaTime;
        }
        this.hardVerticalHitSoundCooldown -= Engine.DeltaTime;
        base.Depth = 100;
        if (this.Hold.IsHeld)
        {
            this.prevLiftSpeed = Vector2.Zero;
        }
        else
        {
            if (base.OnGround(1))
            {
                if (hasGravity)
                {
                    float target;
                    if (!base.OnGround(this.Position + Vector2.UnitX * 3f, 1))
                    {
                        target = 20f;
                    }
                    else if (!base.OnGround(this.Position - Vector2.UnitX * 3f, 1))
                    {
                        target = -20f;
                    }
                    else
                    {
                        target = 0f;
                    }
                    this.Speed.X = Calc.Approach(this.Speed.X, target, 800f * Engine.DeltaTime);
                    Vector2 liftSpeed = base.LiftSpeed;
                    if (liftSpeed == Vector2.Zero && this.prevLiftSpeed != Vector2.Zero)
                    {
                        this.Speed = this.prevLiftSpeed;
                        this.prevLiftSpeed = Vector2.Zero;
                        this.Speed.Y = Math.Min(this.Speed.Y * 0.6f, 0f);
                        if (this.Speed.X != 0f && this.Speed.Y == 0f)
                        {
                            this.Speed.Y = -60f;
                        }
                        if (this.Speed.Y < 0f)
                        {
                            this.noGravityTimer = 0.15f;
                        }
                    }
                    else
                    {
                        this.prevLiftSpeed = liftSpeed;
                        if (liftSpeed.Y < 0f && this.Speed.Y < 0f)
                        {
                            this.Speed.Y = 0f;
                        }
                    }
                }
            }
            else if (this.Hold.ShouldHaveGravity && hasGravity)
            {
                float num = 800f;
                if (Math.Abs(this.Speed.Y) <= 30f)
                {
                    num *= 0.5f;
                }
                float num2 = 350f;
                if (this.Speed.Y < 0f)
                {
                    num2 *= 0.5f;
                }
                this.Speed.X = Calc.Approach(this.Speed.X, 0f, num2 * Engine.DeltaTime);
                if (this.noGravityTimer > 0f)
                {
                    this.noGravityTimer -= Engine.DeltaTime;
                }
                else
                {
                    this.Speed.Y = Calc.Approach(this.Speed.Y, 200f, num * Engine.DeltaTime);
                }
            }
            this.previousPosition = base.ExactPosition;
            base.MoveH(this.Speed.X * Engine.DeltaTime, this.onCollideH, null);
            base.MoveV(this.Speed.Y * Engine.DeltaTime, this.onCollideV, null);
            if (base.Center.X > (float)this.Level.Bounds.Right)
            {
                base.MoveH(32f * Engine.DeltaTime, null, null);
                if (base.Left - 8f > (float)this.Level.Bounds.Right)
                {
                    base.RemoveSelf();
                }
            }
            else if (base.Left < (float)this.Level.Bounds.Left)
            {
                base.Left = (float)this.Level.Bounds.Left;
                this.Speed.X = this.Speed.X * -0.4f;
            }
            else if (base.Top < (float)(this.Level.Bounds.Top - 4))
            {
                base.Top = (float)(this.Level.Bounds.Top + 4);
                this.Speed.Y = 0f;
            }
            else if (base.Bottom > (float)this.Level.Bounds.Bottom && SaveData.Instance.Assists.Invincible)
            {
                base.Bottom = (float)this.Level.Bounds.Bottom;
                this.Speed.Y = -300f;
                Audio.Play("event:/game/general/assist_screenbottom", this.Position);
            }
            if (base.X < (float)(this.Level.Bounds.Left + 10))
            {
                base.MoveH(32f * Engine.DeltaTime, null, null);
            }
            Player entity = base.Scene.Tracker.GetEntity<Player>();
            TempleGate templeGate = base.CollideFirst<TempleGate>();
            if (templeGate != null && entity != null)
            {
                templeGate.Collidable = false;
                base.MoveH((float)(Math.Sign(entity.X - base.X) * 32) * Engine.DeltaTime, null, null);
                templeGate.Collidable = true;
            }
        }
            /*if (this.hitSeeker != null && this.swatTimer <= 0f && !this.hitSeeker.Check(this.Hold))
            {
                this.hitSeeker = null;
            }
            if (this.tutorialGui != null)
            {
                if (!this.Hold.IsHeld && base.OnGround(1) && this.Level.Session.GetFlag("foundTheoInCrystal"))
                {
                    this.tutorialTimer += Engine.DeltaTime;
                }
                else
                {
                    this.tutorialTimer = 0f;
                }
                this.tutorialGui.Open = (this.tutorialTimer > 0.25f);
            }*/
    }

    public void Explode()
    {
        Collider collider = base.Collider;
        Audio.Play("event:/new_content/game/10_farewell/puffer_splode", this.Position);
        this.sprite.Play("explode", false, false);
        Player player = base.CollideFirst<Player>();
        if (player != null && !base.Scene.CollideCheck<Solid>(this.Position, player.Center))
        {
            player.ExplodeLaunch(this.Position, false, true);
        }
        TheoCrystal theoCrystal = base.CollideFirst<TheoCrystal>();
        if (theoCrystal != null && !base.Scene.CollideCheck<Solid>(this.Position, theoCrystal.Center))
        {
            theoCrystal.ExplodeLaunch(this.Position);
        }
        foreach (Entity entity in base.Scene.Tracker.GetEntities<TempleCrackedBlock>())
        {
            TempleCrackedBlock templeCrackedBlock = (TempleCrackedBlock)entity;
            if (base.CollideCheck(templeCrackedBlock))
            {
                templeCrackedBlock.Break(this.Position);
            }
        }
        foreach (Entity entity2 in base.Scene.Tracker.GetEntities<TouchSwitch>())
        {
            TouchSwitch touchSwitch = (TouchSwitch)entity2;
            if (base.CollideCheck(touchSwitch))
            {
                touchSwitch.TurnOn();
            }
        }
        foreach (Entity entity3 in base.Scene.Tracker.GetEntities<FloatingDebris>())
        {
            FloatingDebris floatingDebris = (FloatingDebris)entity3;
            if (base.CollideCheck(floatingDebris))
            {
                floatingDebris.OnExplode(this.Position);
            }
        }
        base.Collider = collider;
        Level level = base.SceneAs<Level>();
        level.Shake(0.3f);
        level.Displacement.AddBurst(this.Position, 0.4f, 12f, 36f, 0.5f, null, null);
        level.Displacement.AddBurst(this.Position, 0.4f, 24f, 48f, 0.5f, null, null);
        level.Displacement.AddBurst(this.Position, 0.4f, 36f, 60f, 0.5f, null, null);
        for (float num = 0f; num < 6.2831855f; num += 0.17453292f)
        {
            Vector2 position = base.Center + Calc.AngleToVector(num + Calc.Random.Range(-0.034906585f, 0.034906585f), (float)Calc.Random.Range(12, 18));
            level.Particles.Emit(Seeker.P_Regen, position, num);
        }
    }

    public void Explode(Vector2 center)
    {
        Collider collider = base.Collider;
        Audio.Play("event:/new_content/game/10_farewell/puffer_splode", center);
        this.sprite.Play("explode", false, false);
        Player player = base.CollideFirst<Player>();
        if (player != null && !base.Scene.CollideCheck<Solid>(center, player.Center))
        {
            player.ExplodeLaunch(center, false, true);
        }
        TheoCrystal theoCrystal = base.CollideFirst<TheoCrystal>();
        if (theoCrystal != null && !base.Scene.CollideCheck<Solid>(center, theoCrystal.Center))
        {
            theoCrystal.ExplodeLaunch(center);
        }
        foreach (Entity entity in base.Scene.Tracker.GetEntities<TempleCrackedBlock>())
        {
            TempleCrackedBlock templeCrackedBlock = (TempleCrackedBlock)entity;
            if (base.CollideCheck(templeCrackedBlock))
            {
                templeCrackedBlock.Break(center);
            }
        }
        foreach (Entity entity2 in base.Scene.Tracker.GetEntities<TouchSwitch>())
        {
            TouchSwitch touchSwitch = (TouchSwitch)entity2;
            if (base.CollideCheck(touchSwitch))
            {
                touchSwitch.TurnOn();
            }
        }
        foreach (Entity entity3 in base.Scene.Tracker.GetEntities<FloatingDebris>())
        {
            FloatingDebris floatingDebris = (FloatingDebris)entity3;
            if (base.CollideCheck(floatingDebris))
            {
                floatingDebris.OnExplode(center);
            }
        }
        base.Collider = collider;
        Level level = base.SceneAs<Level>();
        level.Shake(0.3f);
        level.Displacement.AddBurst(center, 0.4f, 12f, 36f, 0.5f, null, null);
        level.Displacement.AddBurst(center, 0.4f, 24f, 48f, 0.5f, null, null);
        level.Displacement.AddBurst(center, 0.4f, 36f, 60f, 0.5f, null, null);
        for (float num = 0f; num < 6.2831855f; num += 0.17453292f)
        {
            Vector2 position = base.Center + Calc.AngleToVector(num + Calc.Random.Range(-0.034906585f, 0.034906585f), (float)Calc.Random.Range(12, 18));
            level.Particles.Emit(Seeker.P_Regen, position, num);
        }
        if (this.doesRemoveOnExplode)
        {
            base.RemoveSelf();
        }
    }
    private void OnPickup()
    {
        this.Speed = Vector2.Zero;
        base.AddTag(Tags.Persistent);
    }

    private void OnRelease(Vector2 force)
    {
        base.RemoveTag(Tags.Persistent);
        if (force.X != 0f && force.Y == 0f)
        {
            force.Y = -0.4f;
        }
        this.Speed = force * 200f;
        if (this.Speed != Vector2.Zero)
        {
            this.noGravityTimer = 0.1f;
        }
    }

    private void OnCollideH(CollisionData data)
    {
        if (data.Hit is DashSwitch)
        {
            (data.Hit as DashSwitch).OnDashCollide(null, Vector2.UnitX * (float)Math.Sign(this.Speed.X));
        }
        Audio.Play("event:/game/05_mirror_temple/crystaltheo_hit_side", this.Position);
        if (Math.Abs(this.Speed.X) > 100f)
        {
            this.ImpactParticles(data.Direction);
        }
        this.Speed.X = this.Speed.X * -0.4f;
    }

    private void OnCollideV(CollisionData data)
    {
        if (data.Hit is DashSwitch)
        {
            (data.Hit as DashSwitch).OnDashCollide(null, Vector2.UnitY * (float)Math.Sign(this.Speed.Y));
        }
        if (this.Speed.Y > 0f)
        {
            if (this.hardVerticalHitSoundCooldown <= 0f)
            {
                Audio.Play("event:/game/05_mirror_temple/crystaltheo_hit_ground", this.Position, "crystal_velocity", Calc.ClampedMap(this.Speed.Y, 0f, 200f, 0f, 1f));
                this.hardVerticalHitSoundCooldown = 0.5f;
            }
            else
            {
                Audio.Play("event:/game/05_mirror_temple/crystaltheo_hit_ground", this.Position, "crystal_velocity", 0f);
            }
        }
        if (this.Speed.Y > 160f)
        {
            this.ImpactParticles(data.Direction);
        }
        if (this.Speed.Y > 140f && !(data.Hit is SwapBlock) && !(data.Hit is DashSwitch))
        {
            this.Speed.Y = this.Speed.Y * -0.6f;
            return;
        }
        this.Speed.Y = 0f;
    }

    private void ImpactParticles(Vector2 dir)
    {
        float direction;
        Vector2 position;
        Vector2 positionRange;
        if (dir.X > 0f)
        {
            direction = 3.1415927f;
            position = new Vector2(base.Right, base.Y - 4f);
            positionRange = Vector2.UnitY * 6f;
        }
        else if (dir.X < 0f)
        {
            direction = 0f;
            position = new Vector2(base.Left, base.Y - 4f);
            positionRange = Vector2.UnitY * 6f;
        }
        else if (dir.Y > 0f)
        {
            direction = -1.5707964f;
            position = new Vector2(base.X, base.Bottom);
            positionRange = Vector2.UnitX * 6f;
        }
        else
        {
            direction = 1.5707964f;
            position = new Vector2(base.X, base.Top);
            positionRange = Vector2.UnitX * 6f;
        }
        this.Level.Particles.Emit(TheoCrystal.P_Impact, 12, position, positionRange, direction);
    }
    public void GotoGone()
    {
        this.Collidable = false;
        this.goneTimer = 2.5f;
        this.state = Quantum.States.Gone;
    }
    private void GotoIdle()
    {
        this.isInited = true;
        this.Speed = Vector2.Zero;
        if (this.state == Quantum.States.Gone)
        {
            this.Position = (this.previousPosition = this.startPosition);
            this.cantExplodeTimer = 0.5f;
            this.sprite.Play("recover", false, false);
            Audio.Play("event:/new_content/game/10_farewell/puffer_reform", this.Position);
        }
        this.state = Quantum.States.Idle;
    }

    public Boolean Coincides(Quantum quantum)
    {
        return (this.Position.Equals(quantum.Position));
    }
}
