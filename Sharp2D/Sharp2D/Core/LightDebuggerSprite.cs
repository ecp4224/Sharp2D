using Sharp2D.Core.Interfaces;

namespace Sharp2D.Core;

public class LightDebuggerSprite : Sprite, ILogical
{
    public override string Name { get; }
    public TextSprite textSprite;

    public LightDebuggerSprite(Sprite owner)
    {
        Name = $"{owner.Name}-Light-Debugger";
        
        owner.Attach(this);
    }

    protected override void BeforeDraw()
    {
    }

    protected override void OnLoad()
    {
        textSprite = Text.CreateTextSprite("Lights: 0");
        CurrentWorld.AddSprite(textSprite);
    }

    protected override void OnUnload()
    {
    }

    protected override void OnDispose()
    {
    }

    protected override void OnDisplay()
    {
    }

    public void Update()
    {
        
    }
}