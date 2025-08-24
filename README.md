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

### Sprite Modules

Sprites that inherit from `ModuleSprite` can be extended with reusable pieces of
functionality called *modules*. Attach a module with `AttachModule<T>()` and it
will receive update calls each frame.

```csharp
public class BlinkModule : IModule
{
    public Sprite Owner { get; private set; }
    public string ModuleName => "Blink";
    public ModuleRules Rules => ModuleRules.None;

    public void InitializeWith(Sprite sprite)
    {
        Owner = sprite;
    }

    public void OnUpdate()
    {
        Owner.IsVisible = !Owner.IsVisible;
    }

    public void Dispose() { }
}

// Attach the custom module
player.AttachModule<BlinkModule>();
```

### Animation

Animations are configured through a JSON file that describes the sprite sheet
and each animation's frames. Place the file in `animations/` and by default the
module will look for a file matching the sprite's name, e.g.
`animations/player.conf`:

```json
{
  "width": 32,
  "height": 32,
  "animations": {
    "walk": { "row": 0, "framecount": 4, "speed": 100 }
  }
}
```

Attach an `AnimationModule` to the sprite and trigger an animation:

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

### Custom Render Pipelines

Rendering in Sharp2D is organized into *render jobs*. A render job implements
`IRenderJob` and performs the OpenGL work needed to draw a particular type of
content.

```csharp
public class MyRenderJob : IRenderJob
{
    public void PerformJob()
    {
        // issue custom draw calls here
    }

    public void Dispose() { }
}

var job = new MyRenderJob();
world.AddRenderJob(job);
```

The built-in `TextRenderJob` is a complete example of a render pipeline for
signed distance field text. It batches `TextSprite` meshes and draws them in
`PerformJob`. Register the job with your world and add sprites to it:

```csharp
var textJob = new TextRenderJob(world);
world.AddRenderJob(textJob);
textJob.Add(textSprite);
```

If you're using the `Sharp2D.Text` namespace, the `AddTextSprite` extension on
`GenericWorld` demonstrates how a small render job API can be wrapped for
convenience. It ensures a `TextRenderJob` is present and adds the sprite in one
call:

```csharp
using Sharp2D.Text;

var sprite = world.AddTextSprite(font, "Hello");
```

### SDF Text Rendering

Signed distance field fonts provide crisp text at any scale:

```csharp
using Sharp2D.Text;

var font = SdfFont.Load("path/to/font.fnt", "path/to/font.png");
var text = world.AddTextSprite(font, "Hello SDF");
text.SetPosition(new Vector2(10, 10));
```

### Audio

Play background music or sound effects with `MusicPlayer`:

```csharp
MusicPlayer.Play("bg.ogg");
```

## License

Sharp2D is released under the MIT License.
