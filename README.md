Sharp2D
=======

A game framework for making 2d games in C#

## SDF Text Rendering

```csharp
var font = SdfFont.Load("path/to/font.fnt", "path/to/font.png");
var text = new TextSprite(font);
text.SetPosition(new Vector2(10, 10));
text.SetText("Hello SDF");
TextRenderJob.Add(text);
```
