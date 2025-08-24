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

Sprites represent entities in the world. Create a sprite, assign a texture, then add it to the world:

```csharp
var player = new Player();
player.Texture = Texture.NewTexture("sprites/player.png");
player.Texture.LoadTextureFromFile();
world.AddSprite(player);
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
