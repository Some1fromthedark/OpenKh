using System;
using System.Collections.Generic;
using System.Text;

namespace OpenKh.Engine.Parsers.Kddf2
{
    public class Kkdf2MdlxBuiltModel
    {
        public SortedDictionary<int, Model> textureIndexBasedModelDict;
        public Kkdf2MdlxParser parser;

        public List<CI> MeshDescriptors { get; } = new List<CI>();

        public class CI
        {
            public int[] Indices;
            public int TextureIndex, SegmentIndex;
        }


    }
}
