using System.Collections.Generic;
using OpenTK.Mathematics;
using Sharp2D.Fonts;

namespace Sharp2D.Text
{
    /// <summary>
    /// Generates quad vertex data for a string using an <see cref="SdfFont"/>.
    /// </summary>
    public static class TextLayout
    {
        public struct LayoutResult
        {
            public float[] Vertices; // interleaved position.xy, uv.xy
            public int QuadCount;
        }

        public static LayoutResult Build(SdfFont font, string text, Vector2 origin, float scale, bool useKerning)
        {
            var verts = new List<float>(text.Length * 6 * 4);
            float x = origin.X;
            float y = origin.Y;
            int last = -1;
            foreach (char c in text)
            {
                if (c == '\n')
                {
                    x = origin.X;
                    y -= font.LineHeight * scale;
                    last = -1;
                    continue;
                }

                if (!font.TryGetKerning(last, c, out float kern)) kern = 0f;
                x += kern * scale;

                var glyph = font.GetGlyph(c);
                float x0 = x + glyph.XOffset * scale;
                float y0 = y - glyph.YOffset * scale;
                float w = glyph.Width * scale;
                float h = glyph.Height * scale;

                // positions
                Vector2 p0 = new Vector2(x0, y0 - h);
                Vector2 p1 = new Vector2(x0 + w, y0 - h);
                Vector2 p2 = new Vector2(x0 + w, y0);
                Vector2 p3 = new Vector2(x0, y0);

                Vector2[] uv = glyph.UV;

                // triangle 1
                verts.Add(p0.X); verts.Add(p0.Y); verts.Add(uv[0].X); verts.Add(uv[0].Y);
                verts.Add(p1.X); verts.Add(p1.Y); verts.Add(uv[1].X); verts.Add(uv[1].Y);
                verts.Add(p2.X); verts.Add(p2.Y); verts.Add(uv[2].X); verts.Add(uv[2].Y);
                // triangle 2
                verts.Add(p0.X); verts.Add(p0.Y); verts.Add(uv[0].X); verts.Add(uv[0].Y);
                verts.Add(p2.X); verts.Add(p2.Y); verts.Add(uv[2].X); verts.Add(uv[2].Y);
                verts.Add(p3.X); verts.Add(p3.Y); verts.Add(uv[3].X); verts.Add(uv[3].Y);

                x += glyph.XAdvance * scale;
                last = c;
            }

            return new LayoutResult { Vertices = verts.ToArray(), QuadCount = verts.Count / (6 * 4) };
        }
    }
}
