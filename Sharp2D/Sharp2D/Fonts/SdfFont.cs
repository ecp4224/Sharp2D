using System;
using System.Collections.Generic;
using System.Globalization;
using System.IO;
using System.Xml.Linq;
using OpenTK.Graphics.OpenGL;
using OpenTK.Mathematics;

namespace Sharp2D.Fonts
{
    /// <summary>
    /// Represents a bitmap font backed by a single-channel SDF atlas.
    /// </summary>
    public class SdfFont
    {
        public struct Glyph
        {
            public int Codepoint;
            public float Width;
            public float Height;
            public float XOffset;
            public float YOffset;
            public float XAdvance;
            public Vector2[] UV; // 4 corners TL, TR, BR, BL
        }

        private readonly Dictionary<int, Glyph> _glyphs = new Dictionary<int, Glyph>();
        private readonly Dictionary<(int, int), float> _kernings = new Dictionary<(int, int), float>();

        public float LineHeight { get; private set; }
        public float Baseline { get; private set; }
        public int AtlasWidth { get; private set; }
        public int AtlasHeight { get; private set; }
        public Texture AtlasTexture { get; private set; }

        private SdfFont() { }

        public static SdfFont Load(string fntPath, string atlasPath)
        {
            var font = new SdfFont();
            ParseFont(fntPath, font);
            LoadTexture(atlasPath, font);
            return font;
        }

        private static void ParseFont(string fntPath, SdfFont font)
        {
            using (var stream = File.OpenRead(fntPath))
            using (var reader = new StreamReader(stream))
            {
                string start = reader.ReadLine();
                stream.Position = 0;
                reader.DiscardBufferedData();

                if (start != null && start.TrimStart().StartsWith("<"))
                {
                    ParseXml(fntPath, font);
                }
                else
                {
                    ParseText(reader, font);
                }
            }
        }

        private static void ParseXml(string fntPath, SdfFont font)
        {
            XDocument doc = XDocument.Load(fntPath);
            var common = doc.Root?.Element("common");
            if (common == null) throw new InvalidDataException("Invalid BMFont XML format");

            font.LineHeight = float.Parse(common.Attribute("lineHeight")!.Value, CultureInfo.InvariantCulture);
            font.Baseline = float.Parse(common.Attribute("base")!.Value, CultureInfo.InvariantCulture);
            font.AtlasWidth = int.Parse(common.Attribute("scaleW")!.Value, CultureInfo.InvariantCulture);
            font.AtlasHeight = int.Parse(common.Attribute("scaleH")!.Value, CultureInfo.InvariantCulture);

            var chars = doc.Root?.Element("chars");
            if (chars != null)
            {
                foreach (var ch in chars.Elements("char"))
                {
                    int id = int.Parse(ch.Attribute("id")!.Value, CultureInfo.InvariantCulture);
                    float x = float.Parse(ch.Attribute("x")!.Value, CultureInfo.InvariantCulture);
                    float y = float.Parse(ch.Attribute("y")!.Value, CultureInfo.InvariantCulture);
                    float w = float.Parse(ch.Attribute("width")!.Value, CultureInfo.InvariantCulture);
                    float h = float.Parse(ch.Attribute("height")!.Value, CultureInfo.InvariantCulture);
                    float xo = float.Parse(ch.Attribute("xoffset")!.Value, CultureInfo.InvariantCulture);
                    float yo = float.Parse(ch.Attribute("yoffset")!.Value, CultureInfo.InvariantCulture);
                    float xa = float.Parse(ch.Attribute("xadvance")!.Value, CultureInfo.InvariantCulture);

                    var glyph = new Glyph
                    {
                        Codepoint = id,
                        Width = w,
                        Height = h,
                        XOffset = xo,
                        YOffset = yo,
                        XAdvance = xa,
                        UV = new[]
                        {
                            new Vector2(x / font.AtlasWidth, y / font.AtlasHeight),
                            new Vector2((x + w) / font.AtlasWidth, y / font.AtlasHeight),
                            new Vector2((x + w) / font.AtlasWidth, (y + h) / font.AtlasHeight),
                            new Vector2(x / font.AtlasWidth, (y + h) / font.AtlasHeight)
                        }
                    };
                    font._glyphs[id] = glyph;
                }
            }

            var kernings = doc.Root?.Element("kernings");
            if (kernings != null)
            {
                foreach (var k in kernings.Elements("kerning"))
                {
                    int first = int.Parse(k.Attribute("first")!.Value, CultureInfo.InvariantCulture);
                    int second = int.Parse(k.Attribute("second")!.Value, CultureInfo.InvariantCulture);
                    float amount = float.Parse(k.Attribute("amount")!.Value, CultureInfo.InvariantCulture);
                    font._kernings[(first, second)] = amount;
                }
            }
        }

        private static void ParseText(StreamReader reader, SdfFont font)
        {
            string? line;
            while ((line = reader.ReadLine()) != null)
            {
                if (line.StartsWith("common"))
                {
                    var map = ParseKeyValues(line);
                    font.LineHeight = float.Parse(map["lineHeight"], CultureInfo.InvariantCulture);
                    font.Baseline = float.Parse(map["base"], CultureInfo.InvariantCulture);
                    font.AtlasWidth = int.Parse(map["scaleW"], CultureInfo.InvariantCulture);
                    font.AtlasHeight = int.Parse(map["scaleH"], CultureInfo.InvariantCulture);
                }
                else if (line.StartsWith("char "))
                {
                    var map = ParseKeyValues(line);
                    int id = int.Parse(map["id"], CultureInfo.InvariantCulture);
                    float x = float.Parse(map["x"], CultureInfo.InvariantCulture);
                    float y = float.Parse(map["y"], CultureInfo.InvariantCulture);
                    float w = float.Parse(map["width"], CultureInfo.InvariantCulture);
                    float h = float.Parse(map["height"], CultureInfo.InvariantCulture);
                    float xo = float.Parse(map["xoffset"], CultureInfo.InvariantCulture);
                    float yo = float.Parse(map["yoffset"], CultureInfo.InvariantCulture);
                    float xa = float.Parse(map["xadvance"], CultureInfo.InvariantCulture);

                    var glyph = new Glyph
                    {
                        Codepoint = id,
                        Width = w,
                        Height = h,
                        XOffset = xo,
                        YOffset = yo,
                        XAdvance = xa,
                        UV = new[]
                        {
                            new Vector2(x / font.AtlasWidth, y / font.AtlasHeight),
                            new Vector2((x + w) / font.AtlasWidth, y / font.AtlasHeight),
                            new Vector2((x + w) / font.AtlasWidth, (y + h) / font.AtlasHeight),
                            new Vector2(x / font.AtlasWidth, (y + h) / font.AtlasHeight)
                        }
                    };
                    font._glyphs[id] = glyph;
                }
                else if (line.StartsWith("kerning"))
                {
                    var map = ParseKeyValues(line);
                    int first = int.Parse(map["first"], CultureInfo.InvariantCulture);
                    int second = int.Parse(map["second"], CultureInfo.InvariantCulture);
                    float amount = float.Parse(map["amount"], CultureInfo.InvariantCulture);
                    font._kernings[(first, second)] = amount;
                }
            }
        }

        private static Dictionary<string, string> ParseKeyValues(string line)
        {
            var map = new Dictionary<string, string>();
            string[] parts = line.Split(' ');
            foreach (string part in parts)
            {
                if (!part.Contains('=')) continue;
                var kv = part.Split('=');
                if (kv.Length != 2) continue;
                map[kv[0]] = kv[1].Trim('"');
            }
            return map;
        }

        private static void LoadTexture(string atlasPath, SdfFont font)
        {
            var tex = Texture.NewTexture(atlasPath);
            tex.MinFilter = (int)TextureMinFilter.Linear;
            tex.MagFilter = (int)TextureMagFilter.Linear;
            tex.LoadTextureFromFile();
            tex.Create();
            GL.BindTexture(TextureTarget.Texture2D, tex.ID);
            GL.TexParameter(TextureTarget.Texture2D, TextureParameterName.TextureWrapS, (int)TextureWrapMode.ClampToEdge);
            GL.TexParameter(TextureTarget.Texture2D, TextureParameterName.TextureWrapT, (int)TextureWrapMode.ClampToEdge);

            font.AtlasTexture = tex;
            font.AtlasWidth = tex.ImageWidth;
            font.AtlasHeight = tex.ImageHeight;
        }

        public Glyph GetGlyph(int codepoint)
        {
            return _glyphs[codepoint];
        }

        public bool TryGetKerning(int left, int right, out float amount)
        {
            return _kernings.TryGetValue((left, right), out amount);
        }

        public int TextureHandle => AtlasTexture.ID;
    }
}

