using System;
using System.Linq;
using System.Collections.Generic;
using TripleS.Scripting;
using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Graphics;
using Microsoft.Xna.Framework.Content;
using TripleS.Tiled;
using System.Data;
using System.Xml;
using TripleS;
using System.IO;
using System.Collections;
using FMOD;

namespace TripleS {
    public class Tilemapper {

        public Texture2D[] TilesetTextures { get; private set; }
        private Random rng;
        private XmlDocument[] regionMaps;
        private EntTileRegion[] regions;
        private List<Rectangle> pannelRequest;
        private List<string> pannelRequestSlot;
        private List<int[]> pannelRequestID;
        private static readonly int[] secOverlIDs = new int[13] { 96, 98, 91, 106, 87, 89, 105, 107, 97, 113, 114, 115, 116 };

        public TileDrawData? MapTile(string layer, int gid, Vector2 drawPos, float drawLayer, int next, int below, int exposeness)
        {
            rng ??= new Random(69420);
            string targetSlot = GIDToSlotName(gid);
            string targetXmlType = layer switch { "collision" => "ground", "background" => "backgr", "foreground" => "overlay", _ => throw new NotImplementedException() };
            if (layer == "foreground")
                targetXmlType += secOverlIDs.Contains(gid) ? "2" : "1";

            int sr = GetRegionFromPos(drawPos, out string transSide);
            targetXmlType = transSide == "" ? targetXmlType : "trans";
            string addition = transSide == "" ? "" : $"[@side='{transSide}']";
            if(transSide != "")
                addition += layer == "background" ? "[@bg='true']" : "";

            var baseNode = regionMaps[sr].SelectSingleNode($"regionset/{targetXmlType}[@slot='{targetSlot}']{addition}");
            var selImg = TilesetTextures.Where(x => x.Name[4..] == regionMaps[sr].SelectSingleNode("regionset").Attributes["image"].Value).First(); //or just use completeTilesets[sr]

            if (baseNode != null)
            {
                Vector2 offset = Vector2.Zero;
                if (baseNode.Attributes["offset"].Value != "0,0")
                {
                    var things = baseNode.Attributes["offset"].Value.Split(',');
                    offset = new Vector2(int.Parse(things[0]), int.Parse(things[1]));
                }

                int eVal = SelectExtraGID(sr, targetXmlType, drawPos, targetSlot, next, below);
                int trueGid;
                if (eVal == -1)
                {
                    if (baseNode.Attributes["setID"].Value.Contains(','))
                    {
                        var things = baseNode.Attributes["setID"].Value.Split(',');
                        trueGid = int.Parse(things[rng.Next(things.Length)]);
                    }
                    else
                        trueGid = int.Parse(baseNode.Attributes["setID"].Value);
                }
               else
                    trueGid = eVal;

                int w = selImg.Width / 16;
                int y = (int)MathF.Floor((trueGid + 1) / w);
                y = (trueGid + 1) == w * y ? y - 1 : y;
                int x = trueGid - (w * y);
                y = (y * 16) - (int)offset.Y;
                x = (x * 16) - (int)offset.X;

                layer = layer == "collision" ? "ground" : layer;
                return new TileDrawData(drawLayer, SpriteEffects.None, layer, sr, (int)MathUtil.MinClamp(trueGid, 1), new Rectangle(x, y, 16 + (int)offset.Y, 16 + (int)offset.X), new Rectangle((int)drawPos.X, (int)drawPos.Y, 16, 16), -1, true, exposeness);
            }
            else
                return null;
        }

        public Texture2D[] GetTilesets(ContentManager content)
        {
            Texture2D[] textures = new Texture2D[regionMaps.Length];
            for (int i = 0; i < regionMaps.Length; i++)
            {
                string path = "til/" + regionMaps[i].SelectSingleNode("regionset").Attributes["image"].Value;
                textures[i] = content.Load<Texture2D>(path);
            }
            return textures;
        }

        public void LoadMapRegions(int def, Entity[] reg, ContentManager content)
        {
            regionMaps = new XmlDocument[reg.Length + 1];

            string path = content.RootDirectory + "/til/region" + def + ".xml";
            if (File.Exists(path))
                regionMaps[0] = LoadRegionFile(path);

            regions = new EntTileRegion[reg.Length];
            for (int i = 0; i < reg.Length; i++)
            {
                var realEnt = (EntTileRegion)reg[i];
                path = content.RootDirectory + "/til/region" + realEnt.RegionSetID + ".xml";
                if (File.Exists(path))
                    regionMaps[i + 1] = LoadRegionFile(path);
                regions[i] = realEnt;
            }

            TilesetTextures = GetTilesets(content);
        }

        public static XmlDocument LoadRegionFile(string path)
        {
            string contents = "";
            using (FileStream fs = File.OpenRead(path))
            {
                using (StreamReader sr = new StreamReader(fs))
                {
                    string line;
                    while ((line = sr.ReadLine()) != null)
                    {
                        contents += line;
                    }
                }
            }

            if (contents != "")
            {
                XmlDocument doc = new XmlDocument();
                doc.LoadXml(contents);
                return doc;
            }
            else
                return null;
        }

        public static string GIDToSlotName(int gid)
        {
            gid--;
            return gid switch
            {
                2 => "single",
                69 or 99 or 96 => "side_left",
                71 or 101 or 98 => "side_right",
                61 or 88 or 91 => "side_top",
                79 or 109 or 106 => "side_bottom",
                60 or 90 or 87 => "corner_tl",
                62 or 92 or 89 => "corner_tr",
                78 or 108 or 105 => "corner_bl",
                80 or 110 or 107 => "corner_br",
                70 or 100 or 97 => "centre",
                58 or 93 or 113 => "corner_inv_br",
                59 or 94 or 114 => "corner_inv_bl",
                67 or 102 or 115 => "corner_inv_tr",
                68 or 103 or 116 => "corner_inv_tl",
                25 => "plat_right",
                24 => "plat_top",
                15 => "plat_bottom",
                16 => "plat_left",
                9 => "slope_tu",
                10 => "slope_td",
                17 => "slope_bu",
                26 => "slope_bd",
                11 => "slope_tu_half_1",
                12 => "slope_tu_half_2",
                14 => "slope_td_half_1",
                13 => "slope_td_half_2",
                83 => "slope_bu_half_1",
                84 => "slope_bu_half_2",
                81 => "slope_bd_half_1",
                82 => "slope_bd_half_2",
                _ => "",
            };
        }

        private int GetRegionFromPos(Vector2 pos, out string transSide)
        {
            int preTransSide = 0;
            int best = 0;

            for (int i = 0; i < regions.Length; i++)
            {
                Rectangle bounds = regions[i].Bounds.Value;
                if (pos.X < bounds.Right - 1 && pos.X >= bounds.Left && pos.Y >= bounds.Top && pos.Y < bounds.Bottom - 1)
                {
                    best = i + 1;
                    if (pos.X == bounds.Left)
                        preTransSide = 1;
                    else if (pos.X + 16 == bounds.Right)
                        preTransSide = 2;
                    else if (pos.Y == bounds.Top)
                    {
                        preTransSide = preTransSide == 1 ? 5 : preTransSide;
                        preTransSide = preTransSide == 2 ? 6 : 3;
                    }
                    else if (pos.Y + 16 == bounds.Bottom - 1)
                    {
                        preTransSide = preTransSide == 1 ? 7 : preTransSide;
                        preTransSide = preTransSide == 2 ? 8 : 4;
                    }
                }
            }

            transSide = preTransSide switch { 0 => "", 5 => "topLeft", 3 => "top", 6 => "topRight", 1 => "left", 2 => "right", 7 => "bottomLeft", 4 => "bottom", 8 => "bottomRight", _ => "" };
            return best;
        }

        private int SelectExtraGID(int region, string targetNodeName, Vector2 pos, string currentSlotID, int nextGid, int belowGid)
        {
            if (targetNodeName == "backgr" || targetNodeName == "ground")
            {
                pannelRequest ??= new List<Rectangle>();
                pannelRequestSlot ??= new List<string>();
                pannelRequestID ??= new List<int[]>();

                bool canRequest = true;

                for (int i = 0; i < pannelRequest.Count; i++)
                {
                    var req = pannelRequest[i];
                    var slot = pannelRequestSlot[i].Split(',');
                    if (pos.X < req.Right - 1 && pos.X >= req.Left && pos.Y >= req.Top && pos.Y < req.Bottom - 1 && slot[1] == targetNodeName)
                    {
                        canRequest = false;
                        int index = ((int)((pos.Y - req.Y) / 16) * (req.Width / 16)) + (int)((pos.X - req.X) / 16);
                        return pannelRequestID[i][index - 1];
                    }
                }

                if (canRequest)
                {
                    string nextSlot = GIDToSlotName(nextGid);
                    string belowSlot = GIDToSlotName(belowGid);
                    foreach (XmlNode node in regionMaps[region].SelectNodes("regionset/" + targetNodeName + "Extra"))
                    {
                        int r = rng.Next(int.Parse(node.Attributes["chance"].Value));
                        string neighbour = node.Attributes["neighbour"].Value;

                        if (r <= 1 && currentSlotID == neighbour && nextSlot == neighbour && belowSlot == neighbour)
                        {
                            if (node.Attributes["size"].Value.Contains(','))
                            {
                                var thingy = node.Attributes["size"].Value.Split(',');
                                pannelRequest.Add(new Rectangle((int)pos.X, (int)pos.Y, int.Parse(thingy[0]) * 16, int.Parse(thingy[1]) * 16));
                                pannelRequestSlot.Add(neighbour + "," + targetNodeName);

                                thingy = node.Attributes["setID"].Value.Split(',');
                                int[] adds = new int[thingy.Length - 1];
                                for (int i = 1; i < thingy.Length; i++) 
                                    adds[i - 1] = int.Parse(thingy[i]);

                                pannelRequestID.Add(adds);
                                return int.Parse(thingy[0]);
                            }
                            else
                                return int.Parse(node.Attributes["setID"].Value);
                        }
                    }
                }
            }

            return -1;
        }
    }
}