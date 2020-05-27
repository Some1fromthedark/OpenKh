using Microsoft.Xna.Framework.Graphics;
using OpenKh.Game.Infrastructure;
using System;

namespace OpenKh.Game.Models
{
    public class Mesh
    {
        public class Segment
        {
            public VertexPositionColorTexture[] Vertices { get; set; }
        }

        public class Part
        {
            public int[] Indices { get; set; }
            public int TextureId { get; set; }
            public int SegmentId { get; set; }
        }

        public Part[] Parts { get; set; }
        public Texture2D[] Textures { get; set; }

        public Func<double, Segment[]> GetSegmentsByTime;
    }
}
