using System;
using System.Collections.Generic;
using OpenTK.Mathematics;

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
                    y -= font.LineHeight * scale; // If your screen is Y-down, make this +=
                    last = -1;
                    continue;
                }

                float kern = 0f;
                if (useKerning && last != -1 && font.TryGetKerning(last, c, out var k)) kern = k;
                x += kern * scale;

                var glyph = font.GetGlyph(c);

                float w = glyph.Width * scale;
                float h = glyph.Height * scale;

                // Skip empty quads (e.g., spaces)
                if (w <= 0f || h <= 0f)
                {
                    x += glyph.XAdvance * scale;
                    last = c;
                    continue;
                }

                // Assumes Y-up. For Y-down, use y + glyph.YOffset * scale and adjust corners accordingly.
                float x0 = x + glyph.XOffset * scale;
                float y0 = y + glyph.YOffset * scale;  // top edge

                // corners (Y-up): p0=BL, p1=BR, p2=TR, p3=TL
                var p0 = new Vector2(x0, y0 - h);
                var p1 = new Vector2(x0 + w, y0 - h);
                var p2 = new Vector2(x0 + w, y0);
                var p3 = new Vector2(x0, y0);

                // Expect glyph.UV ordered BL, BR, TR, TL in [0..1], V from bottom.
                var uv = glyph.UV; // Vector2[4] : [bl, br, tr, tl]

                // tri 1: p0,p1,p2
                verts.Add(p0.X); verts.Add(p0.Y); 
                verts.Add(uv[0].X); verts.Add(uv[0].Y);
                
                verts.Add(p1.X); verts.Add(p1.Y);
                verts.Add(uv[1].X); verts.Add(uv[1].Y);
                
                verts.Add(p2.X); verts.Add(p2.Y);
                verts.Add(uv[2].X); verts.Add(uv[2].Y);
                
                // tri 2: p0,p2,p3
                verts.Add(p0.X); verts.Add(p0.Y);
                verts.Add(uv[0].X); verts.Add(uv[0].Y);
                
                verts.Add(p2.X); verts.Add(p2.Y); 
                verts.Add(uv[2].X); verts.Add(uv[2].Y);
                
                verts.Add(p3.X); verts.Add(p3.Y);
                verts.Add(uv[3].X); verts.Add(uv[3].Y);

                x += glyph.XAdvance * scale;
                last = c;
            }

            // Each quad consists of 6 vertices, each vertex has 4 floats (position.xy, uv.xy), so 24 floats per quad.
            return new LayoutResult { Vertices = verts.ToArray(), QuadCount = verts.Count / 24 };
        }
    }
}
