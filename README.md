Sharp2D
=======

A game framework for making 2D games in C#.

## Getting Started

### via Git Setup

1. Install the [.NET SDK](https://dotnet.microsoft.com/) (version 8.0 as specified in `global.json`).
2. Clone the repository and build the solution:

   ```bash
   git clone <repo-url>
   cd Sharp2D/Sharp2D
   dotnet build
   ```

3. Run one of the sample projects:

   ```bash
   dotnet run --project SomeGame
   ```

### via NuGet Setup

1. Create a new .NET project and add the Sharp2D package:

   ```bash
   dotnet new console -n MyGame
   cd MyGame
   dotnet add package Sharp2D
   ```

2. Initialize a screen and display a world:

   ```csharp
   using Sharp2D;
   using Sharp2D.Game;

   Screen.DisplayScreen(() =>
   {
       var world = new GameWorld();
       world.Load();
       world.Display();
   });
   ```

## API Overview

### Screen and Worlds

The entry point of a game is `Screen.DisplayScreen`, which sets up the window and executes a callback where you load and display a world. Worlds are derived from `GenericWorld` and handle rendering and logic loops. A typical setup looks like:

```csharp
var settings = new ScreenSettings
{
    GameSize = new Size(1280, 720),
    WindowTitle = "Some Game"
};

Screen.DisplayScreen(() =>
{
    Input.Keyboard.SetDefaults(new Dictionary<string, Keys>()
    {
        {"moveLeft", Keys.D},
        {"moveRight", Keys.A},
        {"moveUp", Keys.W},
        {"moveDown", Keys.S}
    });

    var world = new GameWorld();
    world.Load();
    world.Display();
});
```

### Sprites

Sprites represent entities in the world. Each sprite has a position, size and
texture and can optionally use modules for features like animation or physics.
To introduce a new sprite type, subclass `Sprite` (or a derived class such as
`PhysicsSprite`) and override members as needed.

#### Simple Image Sprite

For a quick way to display an image without creating a subclass, use the
static `Sprite.FromImage` helper:

```csharp
Sprite moon = Sprite.FromImage("sprites/moon.png");
moon.X = (3.5f * 16f) + (moon.Width / 2f);
moon.Y = (7.5f * 16f) + (moon.Height / 2f) + 3f;
moon.Layer = 0.5f;
moon.IgnoreLights = true;
moon.NeverClip = true;
world.AddSprite(moon);
```

If you need a reusable type or custom logic, subclass `Sprite`:

```csharp
public class Tree : Sprite
{
    public override string Name => "tree";

    public Tree()
    {
        Texture = Texture.NewTexture("sprites/tree.png");
        Texture.LoadTextureFromFile();
        Width = Texture.TextureWidth;
        Height = Texture.TextureHeight;
    }
}

var tree = new Tree();
tree.SetPosition(new Vector2(400, 300));
world.AddSprite(tree);
```

#### Complex Sprite with Animation and Movement

For interactive characters, inherit from `PhysicsSprite` and attach modules:

```csharp
public class Enemy : PhysicsSprite
{
    private AnimationModule animations;

    public override string Name => "enemy";

    public Enemy()
    {
        Texture = Texture.NewTexture("sprites/enemy_idle.png");
        Texture.LoadTextureFromFile();
        Width = Texture.TextureWidth;
        Height = Texture.TextureHeight;

        animations = AttachModule<AnimationModule>();
        animations.Animations["walk"].Play();
    }

    public override void Update()
    {
        base.Update();
        X += 2f;
        if (X > 400)
            X = 0;
    }
}

var enemy = new Enemy();
world.AddSprite(enemy);
```

### Animation

Attach an `AnimationModule` to a sprite to play animations:

```csharp
var animations = player.AttachModule<AnimationModule>();
animations.Animations["walk"].Play();
```

### Camera

Each world exposes a camera for panning, zooming, and following sprites:

```csharp
world.Camera.Z = 200;
world.Camera.Follow2D(player);
world.Camera.Bounds = new SKRect(-1000000, 6 * 32, -11 * 32, 48 * 32);
```

### Lights

Add dynamic or static lights to create atmosphere:

```csharp
var light = new Light(456, 680, 1f, 50f, LightType.DynamicPointLight);
light.Radius = 100;
world.AddLight(light);
world.AmbientBrightness = 0.5f;
```

### Input

Use the `Input` class to query keyboard and mouse state. Bind named actions to keys and read them in your update loop.

### SDF Text Rendering

Signed distance field fonts provide crisp text at any scale:

```csharp
var font = SdfFont.Load("path/to/font.fnt", "path/to/font.png");
var text = new TextSprite(font);
text.SetPosition(new Vector2(10, 10));
text.SetText("Hello SDF");
TextRenderJob.Add(text);
```

### Audio

Play background music or sound effects with `MusicPlayer`:

```csharp
MusicPlayer.Play("bg.ogg");
```

## License

Sharp2D is released under the MIT License.
