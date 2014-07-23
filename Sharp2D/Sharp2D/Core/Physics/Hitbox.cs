using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Reflection;
using Newtonsoft.Json;
using OpenTK;
using Newtonsoft.Json.Linq;

namespace Sharp2D.Core.Physics
{
    public sealed class Hitbox
    {
        private static List<ICollidable> _collidableCache = new List<ICollidable>();
        private static List<ICollidable> toRemove = new List<ICollidable>();
        private static List<ICollidable> toAdd = new List<ICollidable>();

        private static bool looping = false;
        private static object cache_lock = new object();

        public static void ForEachCollidable(Action<ICollidable> action)
        {
            lock (cache_lock)
            {
                looping = true;
                foreach (ICollidable c in _collidableCache)
                {
                    action(c);
                }
                looping = false;

                foreach (ICollidable c in toRemove)
                {
                    _collidableCache.Remove(c);
                }
                toRemove.Clear();
                _collidableCache.AddRange(toAdd);
                toAdd.Clear();
            }
        }

        public static void AddCollidable(ICollidable collidable)
        {
            lock (cache_lock)
            {
                if (!looping)
                    _collidableCache.Add(collidable);
                else
                    toAdd.Add(collidable);
            }
        }

        public static void RemoveCollidable(ICollidable collidable)
        {
            lock (cache_lock)
            {
                if (!looping)
                    _collidableCache.Remove(collidable);
                else
                    toRemove.Add(collidable);
            }
        }

        public string Name { get; set; }

        public List<Vector2> Vertices { get; set; }

        public List<Vector2> ConstructEdges()
        {
            Vector2 p1, p2;
            var vec = new List<Vector2>();

            for (var i = 0; i < VertexCount; i++)
            {
                p1 = Vertices[i];

                p2 = i + 1 >= VertexCount ? Vertices[0] : Vertices[i + 1];

                vec.Add(p2 - p1);
            }

            return vec;
        }

        public List<Vector2> ConstructEdgesRelative(ICollidable owner)
        {
            var list = ConstructEdges();
            var add = new Vector2(owner.X - owner.Width / 2f, owner.Y - owner.Height / 2f);
            list = list.Select(v => (v + add)).ToList();

            return list;
        }

        public int VertexCount
        {
            get { return Vertices.Count; }
        }

        public Hitbox(string name, List<Vector2> vertices)
        {
            Name = name;
            Vertices = vertices;
        }

        public static List<Hitbox> Read(string filePath)
        {
            string json;

            if (File.Exists("sprites/" + filePath))
            {
                json = File.ReadAllText("sprites/" + filePath);
            }
            else
            {
                using (var stream = Assembly.GetEntryAssembly().GetManifestResourceStream(filePath))
                {
                    if (stream == null) { return null; }

                    using (var reader = new StreamReader(stream))
                    {
                        json = reader.ReadToEnd();
                    }
                }
            }

            JObject obj = JObject.Parse(json);
            return obj["hitboxes"].Select(t => t.ToObject<Hitbox>()).ToList();
        }

        private static void Project(Vector2 axis, Hitbox hitbox, ref float min, ref float max)
        {
            var dotProduct = Vector2.Dot(axis, hitbox[0]);
            min = dotProduct;
            max = dotProduct;

            for (var i = 1; i < hitbox.VertexCount; i++)
            {
                dotProduct = Vector2.Dot(hitbox[i], axis);

                if (dotProduct < min) { min = dotProduct; }
                else if (dotProduct > max) { max = dotProduct; }
            }
        }

        private static float IntervalDistance(float minA, float maxA, float minB, float maxB)
        {
            return ((minA < minB) ? minB - maxA : minA - maxB);
        }

        private static Vector2 Center(Hitbox box)
        {
            float x = 0;
            float y = 0;

            for (var i = 0; i < box.VertexCount; i++)
            {
                x += box[i].X;
                y += box[i].Y;
            }

            return new Vector2(x / box.VertexCount, y / box.VertexCount);
        }

        public Hitbox GetRelativeHitbox(ICollidable owner)
        {
            var hitbox = new Hitbox(Name, Vertices);
            var add = new Vector2(owner.X - owner.Width / 2f, owner.Y - owner.Height / 2f);
            hitbox.Vertices = hitbox.Vertices.Select(v => (v + add)).ToList();

            return hitbox;
        }

        /// <summary>
        ///     Checks if two ICollidables are colliding using the Hyperplane separation theorem.
        ///     Note that this method may produce incorrect results if either one of the hitboxes is
        ///     nonconvex.
        /// </summary>
        /// <param name="ca">The first collidable</param>
        /// <param name="cb">The second collidable</param>
        /// <param name="relativeVelocity">The velocity of the first collidable relative to the velocity of the other</param>
        /// <returns></returns>
        public static CollisionResult CheckCollision(ICollidable ca, ICollidable cb, Vector2 relativeVelocity)
        {
            var ha = ca.Hitbox.GetRelativeHitbox(ca);
            var hb = cb.Hitbox.GetRelativeHitbox(cb);

            var ea = ha.ConstructEdges();
            var eb = hb.ConstructEdges();

            var result = new CollisionResult {Intersecting = true, WillIntersect = true};

            var minIntervalDistance = Single.PositiveInfinity;
            var translationalAxis = new Vector2(0, 0);

            for (var i = 0; i < ea.Count + eb.Count; i++)
            {
                var edge = i < ea.Count ? ea[i] : eb[i - ea.Count];

                var axis = edge.PerpendicularLeft;
                axis = axis.Normalized();

                float minA = 0, minB = 0, maxA = 0, maxB = 0;
                Project(axis, ha, ref minA, ref maxA);
                Project(axis, hb, ref minB, ref maxB);

                if (IntervalDistance(minA, maxA, minB, maxB) > 0) { result.Intersecting = false; }

                var velocityProjection = Vector2.Dot(axis, relativeVelocity);

                if (velocityProjection < 0) { minA += velocityProjection; }
                else
                { maxA += velocityProjection; }

                var intervalDistance = IntervalDistance(minA, maxA, minB, maxB);

                if (intervalDistance > 0) { result.WillIntersect = false; }

                if (!result.Intersecting && !result.WillIntersect) { break; }

                intervalDistance = Math.Abs(intervalDistance);
                if (!(intervalDistance < minIntervalDistance)) { continue; }
                minIntervalDistance = intervalDistance;
                translationalAxis = axis;

                var dist = Center(ha) - Center(hb);
                if (Vector2.Dot(dist, translationalAxis) < 0) { translationalAxis = -translationalAxis; }
            }

            if (result.WillIntersect) { result.TranslationVector = translationalAxis * minIntervalDistance; }

            return result;
        }

        public float[] GetEndpoitns(float offset = 0)
        {
            var x = Vertices.Select(v => v.X).ToArray();
            var y = Vertices.Select(v => v.Y).ToArray();

            return new[] { x.Min() - offset, x.Max() + offset, y.Min() - offset, y.Max() + offset };
        }

        public Vector2 this[int index]
        {
            get { return Vertices[index]; }
        }
    }

    internal sealed class HitboxContainer
    {
        [JsonProperty(PropertyName = "hitboxes")]
        public List<HitboxHolder> Hitboxes { get; set; }
    }

    internal sealed class HitboxHolder
    {
        [JsonProperty(PropertyName = "name")]
        public String Name { get; set; }

        [JsonProperty(PropertyName = "vertices")]
        public List<float> Vertices { get; set; }
    }
}