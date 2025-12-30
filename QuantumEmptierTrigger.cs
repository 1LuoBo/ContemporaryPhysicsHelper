using Celeste.Mod.Entities;
using System.Linq;

namespace ContemporaryPhysicsHelper;

[CustomEntity("ContemporaryPhysicsHelper/QuantumEmptierTrigger")]
[Tracked(true)]
public class QuantumEmptierTrigger : Trigger
{

    public QuantumEmptierTrigger(EntityData data, Vector2 offset)
        : base(data, offset)
    {

    }

    public override void OnEnter(Player player)
    {
        base.OnEnter(player);
        foreach (Entity entity in Scene.Tracker.GetEntities<NegativeQuantum>())
            entity.RemoveSelf();
        foreach (Entity entity in Scene.Tracker.GetEntities<PositiveQuantum>())
            entity.RemoveSelf();
        foreach (Entity entity in Scene.Tracker.GetEntities<Fluctuation>())
            entity.RemoveSelf();
    }

}