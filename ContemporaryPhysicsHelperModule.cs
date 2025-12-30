namespace Celeste.Mod.ContemporaryPhysicsHelper;
public class ContemporaryPhysicsHelperModule : EverestModule
{
    public static SpriteBank PQSpriteBank;
    public static SpriteBank NQSpriteBank;


    public override void LoadContent(bool firstLoad)
    {
        PQSpriteBank = new SpriteBank(GFX.Game, "Graphics/PQSprites.xml");
        NQSpriteBank = new SpriteBank(GFX.Game, "Graphics/NQSprites.xml");
    }
    public override void Load()
    {
    }

    public override void Unload()
    {
    }
}