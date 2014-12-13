using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Sharp2D.Game.Sprites;

namespace Sharp2D.Game.Worlds
{
    public abstract class SpriteChunkWorld : TiledWorld
    {
        public const float ChunkSize = 64f; 

        private readonly List<Sprite> offScreenSpriteChunkData = new List<Sprite>();
        private List<Sprite>[,] spriteChunkData;

        public int XChunkCount { get; private set; }
        public int YChunkCount { get; private set; }

        protected override void OnLoad()
        {
            base.OnLoad();

            int xChunkSize = (int) Math.Ceiling(PixelWidth/ChunkSize);
            int yChunkSize = (int) Math.Ceiling(PixelHeight/ChunkSize);

            XChunkCount = xChunkSize;
            YChunkCount = yChunkSize;

            spriteChunkData = new List<Sprite>[xChunkSize,yChunkSize];
        }

        protected override void OnDispose()
        {
            base.OnDispose();

            offScreenSpriteChunkData.Clear();
            spriteChunkData = null;
        }

        /// <summary>
        /// Returns the chunk this sprite /should/ belong to. The sprite in the parameter may not be in the actual chunk returned
        /// </summary>
        /// <param name="s">The sprite to get the chunk for</param>
        /// <returns>A list of sprites inside chunk</returns>
        public List<List<Sprite>> GetChunksForSprite(Sprite s)
        {
            var xmin = (int)  ((s.X - (s.Width / 2f))/ChunkSize);
            var ymin = (int)  ((s.Y - (s.Height / 2f))/ChunkSize);

            var xmax = (int)  ((s.X + (s.Width/2f))/ChunkSize);
            var ymax = (int)  ((s.Y + (s.Height/2f))/ChunkSize);

            var chunks = new List<List<Sprite>>();
            for (int x = xmin; x <= xmax; x++)
            {
                for (int y = ymin; y <= ymax; y++)
                {
                    if (x >= XChunkCount || y >= YChunkCount || x < 0 || y < 0)
                    {
                        chunks.Add(offScreenSpriteChunkData);
                        break;
                    }

                    if (spriteChunkData[x, y] != null) chunks.Add(spriteChunkData[x, y]);
                    else
                    {
                        spriteChunkData[x, y] = new List<Sprite>();
                        chunks.Add(spriteChunkData[x, y]);
                    }

                }
                if (x >= XChunkCount || x < 0)
                    break;
            }

            return chunks;
        }

        public List<List<Sprite>> GetChunksAtLocation(float X, float Y, float Width, float Height)
        {
            var xmin = (int) ((X - (Width / 2f)) / ChunkSize);
            var ymin = (int) ((Y - (Height / 2f)) / ChunkSize);

            var xmax = (int) ((X + (Width / 2f)) / ChunkSize);
            var ymax = (int) ((Y + (Height / 2f)) / ChunkSize);

            var chunks = new List<List<Sprite>>();
            for (int x = xmin; x <= xmax; x++)
            {
                for (int y = ymin; y <= ymax; y++)
                {
                    if (x >= XChunkCount || y >= YChunkCount || x < 0 || y < 0)
                    {
                        chunks.Add(offScreenSpriteChunkData);
                        break;
                    }

                    if (spriteChunkData[x, y] != null) chunks.Add(spriteChunkData[x, y]);
                    else
                    {
                        spriteChunkData[x, y] = new List<Sprite>();
                        chunks.Add(spriteChunkData[x, y]);
                    }

                }
                if (x >= XChunkCount || x < 0)
                    break;
            }

            return chunks;
        }

        public bool InSameChunks(Sprite one, Sprite two)
        {
            var x1 = (int) ((one.X - (one.Width / 2f)) / ChunkSize);
            var y1 = (int) ((one.Y - (one.Height / 2f)) / ChunkSize);

            var x1_max = (int) ((one.X + (one.Width / 2f)) / ChunkSize);
            var y1_max = (int) ((one.Y + (one.Height / 2f)) / ChunkSize);

            var x2 = (int) ((two.X - (two.Width / 2f)) / ChunkSize);
            var y2 = (int) ((two.Y - (two.Height / 2f)) / ChunkSize);

            var x2_max = (int) ((two.X + (two.Width / 2f)) / ChunkSize);
            var y2_max = (int) ((two.Y + (two.Height / 2f)) / ChunkSize);

            return x1 == x2 && y1 == y2 && x1_max == x2_max && y1_max == y2_max;
        }

        public bool InSameChunks(float X1, float Y1, float Width1, float Height1, float X2, float Y2, float Width2, float Height2)
        {
            var x1 = (int)((X1 - (Width1 / 2f)) / ChunkSize);
            var y1 = (int) ((Y1 - (Height1 / 2f)) / ChunkSize);

            var x1_max = (int) ((X1 + (Width1 / 2f)) / ChunkSize);
            var y1_max = (int) ((Y1 + (Height1 / 2f)) / ChunkSize);

            var x2 = (int) ((X2 - (Width2 / 2f)) / ChunkSize);
            var y2 = (int) ((Y2 - (Height2 / 2f)) / ChunkSize);

            var x2_max = (int) ((X2 + (Width2 / 2f)) / ChunkSize);
            var y2_max = (int) ((Y2 + (Height2 / 2f)) / ChunkSize);

            return x1 == x2 && y1 == y2 && x1_max == x2_max && y1_max == y2_max;
        }

        public override void AddSprite(Sprite s)
        {
            base.AddSprite(s);

            var chunks = GetChunksForSprite(s);
            chunks.ForEach(c => c.Add(s));

            if (!s.IsStatic)
            {
                s.Moved += SOnMoved;
            }
        }

        public override void AddSprite(Sprite s, SpriteRenderJob job)
        {
            base.AddSprite(s, job);

            var chunks = GetChunksForSprite(s);
            chunks.ForEach(c => c.Add(s));

            if (!s.IsStatic)
            {
                s.Moved += SOnMoved;
            }
        }

        public override void RemoveSprite(Sprite s)
        {
            base.RemoveSprite(s);

            var chunks = GetChunksForSprite(s);
            chunks.ForEach(c => c.Remove(s));

            if (!s.IsStatic)
            {
                s.Moved -= SOnMoved;
            }
        }

        private void SOnMoved(object sender, OnSpriteMoved eventArgs)
        {
            var s = eventArgs.Sprite;
            if (InSameChunks(s.X, s.Y, s.Width, s.Height, eventArgs.OldX, eventArgs.OldY, s.Width, s.Height)) return;

            var chunks = GetChunksAtLocation(eventArgs.OldX, eventArgs.OldY, s.Width, s.Height);
            chunks.ForEach(c => c.Remove(s));

            chunks = GetChunksForSprite(s);
            
            chunks.ForEach(c => c.Add(s));
        }
    }
}
