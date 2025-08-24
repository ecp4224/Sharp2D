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
