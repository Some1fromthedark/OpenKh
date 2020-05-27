using OpenKh.Engine.Maths;
using OpenKh.Engine.Parsers.Kddf2;
using OpenKh.Engine.Parsers.Kddf2.Mset;
using OpenKh.Engine.Parsers.Kddf2.Mset.EmuRunner;
using OpenKh.Kh2;
using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;

namespace OpenKh.Engine.Parsers
{
    public class MdlxParser
    {
        public MdlxParser(Mdlx mdlx)
        {
            if (IsEntity(mdlx))
            {
                var builder = FromEntity(mdlx);
                var builtModel = builder.Build(0);
                Model = new Model
                {
                    GetSegmentsByTime = absTime =>
                    {
                        var nextBuild = builder.Build(absTime);

                        return nextBuild.textureIndexBasedModelDict.Values.Select(x => new Model.Segment
                        {
                            Vertices = x.Vertices.Select(vertex => new PositionColoredTextured
                            {
                                X = vertex.X,
                                Y = vertex.Y,
                                Z = vertex.Z,
                                U = vertex.Tu,
                                V = vertex.Tv,
                                Color = vertex.Color
                            }).ToArray()
                        }).ToArray();
                    },
                    Parts = builtModel.MeshDescriptors.Select(x => new Model.Part
                    {
                        Indices = x.Indices,
                        SegmentIndex = x.SegmentIndex,
                        TextureIndex = x.TextureIndex
                    }).ToArray(),
                };
            }
            else if (IsMap(mdlx))
            {
                var myParser = new NewModelParser(mdlx);
                Model = new Model
                {
                    GetSegmentsByTime = absTime => new Model.Segment[]
                    {
                        new Model.Segment
                        {
                            Vertices = myParser.Vertices.Select(vertex => new PositionColoredTextured
                            {
                                X = vertex.X,
                                Y = vertex.Y,
                                Z = vertex.Z,
                                U = vertex.Tu,
                                V = vertex.Tv,
                                Color = vertex.Color
                            }).ToArray()
                        }
                    },
                    Parts = myParser.MeshDescriptors.Select(x => new Model.Part
                    {
                        Indices = x.Indices,
                        SegmentIndex = x.SegmentIndex,
                        TextureIndex = x.TextureIndex
                    }).ToArray()
                };
            }
        }

        private static Kkdf2MdlxModelBuilder FromEntity(Mdlx mdlx)
        {
            var parser = new Kddf2.Kkdf2MdlxParser(mdlx.SubModels.First());
            System.Func<double, Matrix[]> matrixGenerator;

            {
                var mdlxFile = @"obj\P_EX100.mdlx";
                var msetFile = @"obj\P_EX100.mset";

                var mdlxStream = new MemoryStream(File.ReadAllBytes(mdlxFile), false);
                var msetStream = new MemoryStream(File.ReadAllBytes(msetFile), false);

                var msetBar = Bar.Read(msetStream);
                var anbBar = msetBar
                    .First(it => it.Type == Bar.EntryType.Bar);
                var animEntry = Bar.Read(anbBar.Stream)
                    .First(it => it.Type == Bar.EntryType.AnimationData);
                var animStream = animEntry.Stream;

                var anbAbsOff = (uint)(anbBar.Offset + animEntry.Offset);

                mdlxStream.Position = 0;
                msetStream.Position = 0;

                var animParser = new AnimReader(animStream);
                var maxTick = animParser.model.t11List.Concat(new float[] { 0 }).Max();
                var emuRunner = new Mlink();
                matrixGenerator = tick =>
                {
                    var matrixOutStream = new MemoryStream();
                    emuRunner.Permit(
                        mdlxStream, animParser.cntb1,
                        msetStream, animParser.cntb2,
                        anbAbsOff, (float)((100 * tick) % maxTick), matrixOutStream
                    );

                    BinaryReader br = new BinaryReader(matrixOutStream);
                    matrixOutStream.Position = 0;
                    var matrixOut = new Matrix[animParser.cntb1];
                    for (int t = 0; t < animParser.cntb1; t++)
                    {
                        Matrix M1 = new Matrix();
                        M1.M11 = br.ReadSingle(); M1.M12 = br.ReadSingle(); M1.M13 = br.ReadSingle(); M1.M14 = br.ReadSingle();
                        M1.M21 = br.ReadSingle(); M1.M22 = br.ReadSingle(); M1.M23 = br.ReadSingle(); M1.M24 = br.ReadSingle();
                        M1.M31 = br.ReadSingle(); M1.M32 = br.ReadSingle(); M1.M33 = br.ReadSingle(); M1.M34 = br.ReadSingle();
                        M1.M41 = br.ReadSingle(); M1.M42 = br.ReadSingle(); M1.M43 = br.ReadSingle(); M1.M44 = br.ReadSingle();
                        matrixOut[t] = M1;
                    }

                    return matrixOut;
                };
            }

            var builder = new Kkdf2MdlxModelBuilder
            {
                Build = (tick) =>
                {
                    var builtModel = parser
                        .ProcessVerticesAndBuildModel(
                            matrixGenerator(tick) //MdlxMatrixUtil.BuildTPoseMatrices(mdlx.SubModels.First(), Matrix.Identity)
                        );

                    var ci = builtModel.textureIndexBasedModelDict.Values.Select((model, i) => new Kkdf2MdlxBuiltModel.CI
                    {
                        Indices = model.Vertices.Select((_, index) => index).ToArray(),
                        TextureIndex = i,
                        SegmentIndex = i
                    });

                    builtModel.MeshDescriptors.AddRange(ci);

                    return builtModel;
                }
            };

            return builder;
        }

        private static bool IsEntity(Mdlx mdlx) => mdlx.SubModels != null;

        private static bool IsMap(Mdlx mdlx) => mdlx.MapModel != null;

        public Model Model { get; }
    }
}
