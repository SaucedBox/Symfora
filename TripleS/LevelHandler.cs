using System;
using System.Linq;
using System.Collections.Generic;
using TripleS.Scripting;
using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Graphics;
using Microsoft.Xna.Framework.Content;
using TripleS.Tiled;
using TripleS.Physics;
using System.Data;
using TripleS.Lighting;
using System.ComponentModel;
using System.Reflection.Emit;

namespace TripleS {
    public class LevelHandler {

        const uint FLIPPED_HORIZONTALLY_FLAG = 0b10000000000000000000000000000000;
        const uint FLIPPED_VERTICALLY_FLAG = 0b01000000000000000000000000000000;
        const int SHIFT_FLIP_FLAG_TO_BYTE = 29;
        const float bgBlack = 0.8f;

        public bool Active { get; set; }
        public bool DrawLevel { get; set; }
        public bool BackCull { get; set; }

        public static int CurrentLevel { get; private set; }
        public static int TileWidth { get; private set; }
        public static int TileHeight { get; private set; }
        public static int MapTileWidth { get; private set; }
        public static bool MainMenu { get; private set; }
        public static int MapTileHeight { get; private set; }
        public static Rectangle MapBounds { get; private set; }
        public static List<Collider> Colliders { get; private set; }
        public static TileDrawData[] DrawData { get; private set; }
        //public static Dictionary<string, Rectangle> AreaPortals { get; private set; }
        //public static string CurrentAP { get; set; }
        public static Vector2 WorldPos { get; set; }
        public static List<Vert> Verts { get; set; }
        public static Tilemapper Mapper { get; set; }

        public EntityManager EntityMan { get; private set; }
        private bool loaded;
        private string levFilePath;
        private string tilFilePath;
        private string scrFilePath;
        private List<Texture2D> tileTextures;
        public TiledMap map;
        private Dictionary<int, TiledTileset> tilesets;
        private Type[] eTypes;

        public string levName;
        private string levFileName;
        private int layerName;

        public LevelHandler(string levelFolderPath, string tileFolderPath, string scriptFolderPath, int layer, Type[] entityTypes)
        {
            levFilePath = levelFolderPath;
            tilFilePath = tileFolderPath;
            scrFilePath = scriptFolderPath;
            layerName = layer;

            Active = true;
            BackCull = true;
            DrawLevel = true;

            var engineTypes = new Type[5]
            {
                typeof(EntNavNode),
                typeof(EntReverbZone),
                typeof(EntTileRegion),
                typeof(EntLight),
                typeof(EntAreaLight)
            };
            eTypes = engineTypes.Concat(entityTypes).ToArray();
        }

        public void Initialize(int levelParamId)
        {
            if (SSS.PreviewMode)
            {
                foreach(ParamMarker em in ParamLoader.GetMarkers(MarkerType.Entry, "level"))
                {
                    string fileName = ParamLoader.GetParam<string>("level", em.EntryID, "file");
                    if(fileName == SSS.PreviewMapFile)
                    {
                        CurrentLevel = em.EntryID;
                        break;
                    }
                }
            }
            else
                CurrentLevel = levelParamId;
        }

        public void Load(ContentManager content, Renderer renderer)
        {
            if (Active)
            {
                MainMenu = CurrentLevel == 1;
                levName = ParamLoader.GetParam<string>("level", CurrentLevel, "name");
                levFileName = ParamLoader.GetParam<string>("level", CurrentLevel, "file");

                if (!MainMenu)
                {
                    renderer.Lighting.ResetLights();
                    var tileEntries = ParamLoader.GetMarkers(MarkerType.Entry, "tilesets");
                    tileTextures = new List<Texture2D>();
                    foreach (ParamMarker em in tileEntries)
                    {
                        string tileLoc = ParamLoader.GetParam<string>("tilesets", em.EntryID, "location");
                        var tex = content.Load<Texture2D>($"{tilFilePath}/{tileLoc}");
                        tileTextures.Add(tex);
                    }

                    string mapPath = content.RootDirectory + "/" + levFilePath + "/" + levFileName + ".tmx";
                    map = new TiledMap(mapPath);
                    tilesets = map.GetTiledTilesets(content.RootDirectory + "/" + tilFilePath + "/");

                    TileWidth = map.TileWidth;
                    TileHeight = map.TileHeight;
                    MapTileWidth = map.Width;
                    MapTileHeight = map.Height;
                    var poses = ParamLoader.GetParam<string>("level", CurrentLevel, "pos").Split(',');
                    WorldPos = new Vector2(int.Parse(poses[0]), int.Parse(poses[1]));

                    MapBounds = GetBounds(map.Layers.Where(x => x.name == "collision").First());

                    NodegraphBuiler.ResetGraph();
                    var objLayers = map.Layers.Where(x => x.type == TiledLayerType.ObjectLayer && x.name != "notes");
                    EntityMan = new EntityManager(content, eTypes);
                    EntityMan.Load(objLayers, WorldPos, 0);

                    ScriptManager.Init(content.RootDirectory + "/" + scrFilePath, ParamLoader.GetParam<string>("level", CurrentLevel, "script"));

                    ColliderAlgorithm(map.Layers.Where(x => x.name == "collision").First());
                    DrawData = LoadDrawData(map, content);
                }

                Debug.Log("Loaded level: " + levFileName, LogType.Log);
                loaded = true;
            }
        }

        public void Update(GameTime time)
        {
            if (Active && loaded && !MainMenu)
            {
                EntityMan.UpdateEnts(time);
            }
        }

        //0 (0.5f) - back, 1 (0.8f) - mid, 2 (1f) - surface
        //0 - left, 1 - right, 2 - up, 3 - down, 4 - tl, 5 - tr, 6 - bl, 7 - br, 8 - cbr, 9 - cbl, 10 - ctr, 11 - ctl
        public void DrawLightmaps(Renderer renderer)
        {
            if (Active && DrawLevel && loaded && !MainMenu)
            {
                for (int i = 0; i < DrawData.Length; i++)
                {
                    TileDrawData data = DrawData[i];
                    if (data.GID != 0 && data.LayerName == "collision" && data.NormGid != -1)
                    {
                        Texture2D tex = tileTextures[DecideTexture("devSet")];
                        var mapTileset = map.GetTiledMapTileset(data.NormGid);
                        var tileset = tilesets[mapTileset.firstgid];
                        var rect = map.GetSourceRect(mapTileset, tileset, data.NormGid);
                        renderer.BasicDraw(tex, data.Destination.Location.ToVector2(), 1, 1, source: new Rectangle(rect.x, rect.y, rect.width, rect.height), col: Color.White);
                        /*else
                        {
                            renderer.BasicDraw(tileTextures[data.Tileset], data.Destination.Location.ToVector2(), 1, 1, 1, 0, data.Mirror, Color.Black, data.Source);
                        }*/
                    }
                }
            }
        }

        public void StandardDraw(Renderer renderer)
        {
            if (Active && DrawLevel && loaded && !MainMenu)
            {
                var bl = new string[2] { "collision", "foreground" };
                ProperDrawLevel(renderer, bl, DrawData);
                EntityMan.DrawEnts(renderer);
            }
        }

        public void DrawBackground(Renderer renderer)
        {
            if (Active && DrawLevel && loaded && !MainMenu)
            {
                var bl = new string[1] { "background_back" };
                ProperDrawLevel(renderer, bl, DrawData, true);
            }
        }

        public void DrawForeground(Renderer renderer)
        {
            if (Active && DrawLevel && loaded && !MainMenu)
            {
                var bl = new string[1] { "foreground_front" };
                ProperDrawLevel(renderer, bl, DrawData, true);
            }
        }

        private void ProperDrawLevel(Renderer renderer, string[] blacklistNames, TileDrawData[] drawData, bool whitelist = false)
        {
            if (!MainMenu)
            {
                Rectangle trueCull = new Rectangle((int)renderer.View.Position.X, (int)renderer.View.Position.Y, renderer.View.viewPortWidth, renderer.View.viewPortHeight);
                float drawLayer = 0.9f;
                for (int i = 0; i < drawData.Length; i++)
                {
                    Color col = Color.White;
                    TileDrawData data = drawData[i];
                    bool blist = !blacklistNames.Contains(data.LayerName);
                    blist = whitelist ? !blist : blist;

                    if (data.LayerName == "background")
                        col = new Color(bgBlack, bgBlack, bgBlack);

                    if (data.GID != 0 && blist)
                    {
                        bool cull = CollisionEngine.RectInRect(trueCull, data.Destination);
                        if (!BackCull)
                            cull = true;
                        if (cull)
                        {
                            Texture2D tex = data.Mapped ? Mapper.TilesetTextures[data.Tileset] : tileTextures[data.Tileset]; 
                            renderer.BasicDraw(tex, data.Destination.Location.ToVector2(), layerName, drawLayer, 1, 0, data.Mirror, col, data.Source);
                        }
                        else
                            continue;
                    }
                }
            }
        }

        private int DecideTexture(string name)
        {
            int i = -1;
            foreach (Texture2D tex in tileTextures)
            {
                i++;
                if ("til/" + name == tex.Name)
                    return i;
            }
            return 0;
        }

        public object GetTileProperty(int tilesetParamId, int tileId, string propertyName)
        {
            string tileLoc = ParamLoader.GetParam<string>("tilesets", tilesetParamId, "location");
            TiledTileset set = null;
            foreach (KeyValuePair<int, TiledTileset> keySet in tilesets)
            {
                if (keySet.Value.Name == tileLoc)
                {
                    set = keySet.Value;
                    break;
                }
            }

            foreach (TiledTile tile in set.Tiles)
            {
                if (tile.id == tileId)
                {
                    return GetProp(tile.properties, propertyName);
                }
            }
            return null;
        }

        private object GetProp(TiledProperty[] props, string name)
        {
            foreach (TiledProperty prop in props)
            {
                if (prop.name == name)
                {
                    switch (prop.type)
                    {
                        case TiledPropertyType.Int:
                            return int.Parse(prop.value);
                        case TiledPropertyType.Float:
                            return float.Parse(prop.value);
                        case TiledPropertyType.Bool:
                            return bool.Parse(prop.value);
                        case TiledPropertyType.String:
                            return prop.value;
                    }
                }
            }
            return null;
        }

        public bool ChunkHorizontal(TiledChunk chunk, int dataIndex)
        {
            return (chunk.dataRotationFlags[dataIndex] & (FLIPPED_HORIZONTALLY_FLAG >> SHIFT_FLIP_FLAG_TO_BYTE)) > 0;
        }

        public bool ChunkVertical(TiledChunk chunk, int dataIndex)
        {
            return (chunk.dataRotationFlags[dataIndex] & (FLIPPED_VERTICALLY_FLAG >> SHIFT_FLIP_FLAG_TO_BYTE)) > 0;
        }

        public TileDrawData[] LoadDrawData(TiledMap map, ContentManager content)
        {
            Mapper = new Tilemapper();
            Mapper.LoadMapRegions(ParamLoader.GetParam<int>("level", CurrentLevel, "defaultTiles"), EntityMan.Entities.Where(x => x.ID == "tileRegion").ToArray(), content);

            var tileLayers = map.Layers.Where(x => x.type == TiledLayerType.TileLayer);
            var colLayer = map.Layers.Where(x => x.name == "collision").First();
            var dd = new List<TileDrawData>[tileLayers.Count()];
            float drawLayer = 0.9f;
            int layerIndex = -1;
            int groundColLayerIndex = 0;
            var gdd = new List<TileDrawData>();

            foreach (var layer in tileLayers)
            {
                layerIndex++;
                dd[layerIndex] = new List<TileDrawData>();

                if (layer.name == "collision")
                    groundColLayerIndex = layerIndex;

                drawLayer -= 0.1f;
                int chunkIndex = -1;
                int chunkWidth = layer.chunks.Where(x => x.y == 0).Count();
                foreach (TiledChunk ch in layer.chunks)
                {
                    chunkIndex++;
                    for (int y = 0; y < ch.width; y++)
                    {
                        for (int x = 0; x < ch.height; x++)
                        {
                            var index = (y * 16) + x;
                            var gid = ch.data[index];
                            var tileX = (ch.x + x) * map.TileWidth + (int)WorldPos.X;
                            var tileY = (ch.y + y) * map.TileHeight + (int)WorldPos.Y;

                            if (gid == 0 && layer.name != "collision")
                                continue;

                            var destination = new Rectangle(tileX, tileY, map.TileWidth, map.TileHeight);

                            if (layer.name == "ground")
                            {
                                var mapTileset = map.GetTiledMapTileset(gid);
                                var tileset = tilesets[mapTileset.firstgid];
                                var rect = map.GetSourceRect(mapTileset, tileset, gid);
                                if (gid == 0)
                                {
                                    rect = new TiledSourceRect();
                                    rect.x = 0;
                                    rect.y = 0;
                                    rect.width = map.TileWidth;
                                    rect.height = map.TileHeight;
                                }

                                var source = new Rectangle(rect.x, rect.y, rect.width, rect.height);

                                SpriteEffects effects = SpriteEffects.None;
                                if (ChunkHorizontal(ch, index))
                                {
                                    effects |= SpriteEffects.FlipHorizontally;
                                }
                                if (ChunkVertical(ch, index))
                                {
                                    effects |= SpriteEffects.FlipVertically;
                                }
                                var tex = DecideTexture(tileset.Name);

                                dd[layerIndex].Add(new TileDrawData(drawLayer, effects, layer.name, tex, gid, source, destination, -1, false));
                            }
                            else
                            {
                                if (gid != 0)
                                {
                                    int nextIndex = Math.Clamp(index + 1, 0, ch.width * ch.height);
                                    int nextGid = x == ch.width - 1 ? (ch.x - (chunkWidth * ch.width * 16) == 0 || chunkIndex == layer.chunks.Length - 1 ? -1 : layer.chunks[chunkIndex + 1].data[nextIndex - ch.width]) : ch.data[nextIndex];
                                    //int lastIndex = index - 1;
                                    //int lastGid = x == 0 ? (ch.x == 0 || chunkIndex == 0 ? -1 : layer.chunks[chunkIndex - 1].data[lastIndex + ch.width]) : ch.data[lastIndex];
                                    int belowIndex = index + ch.width;
                                    int belowGid = y == ch.height - 1 ? (chunkIndex + chunkWidth >= layer.chunks.Length ? -1 : layer.chunks[chunkIndex + chunkWidth].data[belowIndex - (ch.width * ch.height)]) : ch.data[belowIndex];
                                    //int aboveIndex = index - ch.width;
                                    //int aboveGid = y == 0 ? (chunkIndex - chunkWidth < 0 ? -1 : layer.chunks[chunkIndex - chunkWidth].data[aboveIndex + (ch.width * ch.height)]) : ch.data[aboveIndex];

                                    /*make sure you add exp into Mapper.MapTile's args
                                    int exp = -1;
                                    if ((belowGid == 0 || aboveGid == 0 || nextGid == 0 || lastGid == 0) && layer.name == "collision")
                                        exp = 0;*/

                                    var preSet = Mapper.MapTile(layer.name, gid, destination.Location.ToVector2(), drawLayer, nextGid, belowGid, -1);
                                    if (preSet.HasValue)
                                        dd[layerIndex].Add(preSet.Value);
                                    //else
                                        //dd[layerIndex].Add(new TileDrawData(0, 0, "collision", 0, gid, Rectangle.Empty, destination, -1, false, -1));
                                }
                                //else if (layer.name == "collision")
                                    //dd[layerIndex].Add(new TileDrawData(0, 0, "collision", 0, gid, Rectangle.Empty, destination, -1, false, -1));
                                if (layer.name == "collision")
                                    gdd.Add(new TileDrawData(0, 0, "collision", 0, gid, Rectangle.Empty, destination, -1, false));
                            }
                        }
                    }
                }
            }

            gdd = gdd.OrderBy(x => x.Destination.Y).ToList();

            if (gdd.Count != MapBounds.Width * MapBounds.Height)
                Debug.Log("Shading maps will be incorect due to missing chunk reminders!", LogType.Warning);

            for (int i = 0; i < gdd.Count; i++)
            {
                if (gdd[i].GID == 71)
                {
                    bool bleft = gdd[i].Destination.X == WorldPos.X;
                    bool btop = gdd[i].Destination.Y == WorldPos.Y;
                    bool bright = gdd[i].Destination.X + map.TileWidth == WorldPos.X + MapBounds.Width * map.TileWidth;
                    bool bbottom = gdd[i].Destination.Y + map.TileHeight == WorldPos.Y + MapBounds.Height * map.TileHeight;

                    //0 - top left  1 - top     2 - top right
                    //3 - right                 4 - left
                    //5 - bottom L  6 - bottom  7 - bottom right    
                    int[] brothers = new int[8];
                    int cg = 0;
                    int dex = 0;

                    if (!bleft && !btop)
                    {
                        dex = i - 1 - MapBounds.Width;
                        if (!gdd.ElementAtOrDefault(dex).Equals(new TileDrawData()))
                        {
                            cg = gdd[dex].GID;
                            if (cg != 71)
                                brothers[0] = cg;
                        }
                    }

                    if (!btop)
                    {
                        dex = i - MapBounds.Width;
                        if (!gdd.ElementAtOrDefault(dex).Equals(new TileDrawData()))
                        {
                            cg = gdd[dex].GID;
                            if (cg != 71)
                                brothers[1] = cg;
                        }
                    }

                    if (!bright && !btop)
                    {
                        dex = i - MapBounds.Width + 1;
                        if (!gdd.ElementAtOrDefault(dex).Equals(new TileDrawData()))
                        {
                            cg = gdd[dex].GID;
                            if (cg != 71)
                                brothers[2] = cg;
                        }
                    }

                    if (!bleft)
                    {
                        dex = i - 1;
                        if (!gdd.ElementAtOrDefault(dex).Equals(new TileDrawData()))
                        {
                            cg = gdd[dex].GID;
                            if (cg != 71)
                                brothers[3] = cg;
                        }
                    }

                    if (!bright)
                    {
                        dex = i + 1;
                        if (!gdd.ElementAtOrDefault(dex).Equals(new TileDrawData()))
                        {
                            cg = gdd[dex].GID;
                            if (cg != 71)
                                brothers[4] = cg;
                        }
                    }

                    if (!bleft && !bbottom)
                    {
                        dex = i - 1 + MapBounds.Width;
                        if (!gdd.ElementAtOrDefault(dex).Equals(new TileDrawData()))
                        {
                            cg = gdd[dex].GID;
                            if (cg != 71)
                                brothers[5] = cg;
                        }
                    }

                    if (!bbottom)
                    {
                        dex = i + MapBounds.Width;
                        if (!gdd.ElementAtOrDefault(dex).Equals(new TileDrawData()))
                        {
                            cg = gdd[dex].GID;
                            if (cg != 71)
                                brothers[6] = cg;
                        }
                    }

                    if (!bright && !bbottom)
                    {
                        dex = i + MapBounds.Width + 1;
                        if (!gdd.ElementAtOrDefault(dex).Equals(new TileDrawData()))
                        {
                            cg = gdd[dex].GID;
                            if (cg != 71)
                                brothers[7] = cg;
                        }
                    }

                    var nfb = brothers.ToList();
                    nfb.RemoveAll(x => x == 0);

                    /* bool notShallow = (brothers[3] != 0 && brothers[4] != 0) || 
                         (brothers[1] != 0 && brothers[6] != 0) ||
                         (brothers[0] != 0 && brothers[7] != 0) ||
                         (brothers[5] != 0 && brothers[2] != 0) ||
                         nfb.Count > 3 || nfb.Count == 0;*/
                    bool notShallow = (brothers[3] != 0 && brothers[4] != 0) ||
                         (brothers[1] != 0 && brothers[6] != 0);

                    if (nfb.Count == 0)
                    {
                        var preMake = gdd[i];
                        preMake.NormGid = 36;
                        gdd[i] = preMake;
                    }
                    else if (!notShallow)
                    {
                        int preNormGid = -1;
                        if (brothers[1] != 0)
                            preNormGid = 65;
                        if (brothers[6] != 0)
                            preNormGid = 64;
                        if (brothers[3] != 0)
                            preNormGid = 55;
                        if (brothers[4] != 0)
                            preNormGid = 56;

                        if (brothers[2] == 63 || brothers[2] == 11 || brothers[2] == 15)
                            preNormGid = 66;
                        if (brothers[5] == 79 || brothers[5] == 27 || brothers[5] == 82)
                            preNormGid = 58;
                        if (brothers[7] == 81 || brothers[7] == 18 || brothers[7] == 85)
                            preNormGid = 57;
                        if (brothers[0] == 61 || brothers[0] == 10 || brothers[0] == 12)
                            preNormGid = 67;

                        if (brothers[2] == 68)
                            preNormGid = 53;
                        if (brothers[5] == 60)
                            preNormGid = 52;
                        if (brothers[7] == 59)
                            preNormGid = 45;
                        if (brothers[0] == 69)
                            preNormGid = 54;

                        var preMake = gdd[i];
                        preMake.NormGid = preNormGid;
                        gdd[i] = preMake;
                    }
                }
                /*else if(gdd[i].GID != 0)
                {
                    var preMake = gdd[i];
                    preMake.NormGid = 46;
                    gdd[i] = preMake;
                }*/
            }

            /*dd[groundColLayerIndex] = dd[groundColLayerIndex].OrderBy(x => x.Destination.Y).ToList();
            int max = dd[groundColLayerIndex].Count - 1;
            int width = dd[groundColLayerIndex].Count(x => x.Destination.Y == 0);

            for (int i = 0; i < dd[groundColLayerIndex].Count; i++)
            {
                if (dd[groundColLayerIndex][i].Exposness == 0)
                    continue;

                var neighbours = new List<int>(4) {
                    dd[groundColLayerIndex][Math.Clamp(i - 1, 0, max)].Exposness,
                    dd[groundColLayerIndex][Math.Clamp(i + 1, 0, max)].Exposness,
                    dd[groundColLayerIndex][Math.Clamp(i - width, 0, max)].Exposness,
                    dd[groundColLayerIndex][Math.Clamp(i + width, 0, max)].Exposness,
                    dd[groundColLayerIndex][Math.Clamp(i - width - 1, 0, max)].Exposness,
                    dd[groundColLayerIndex][Math.Clamp(i - width + 1, 0, max)].Exposness,
                    dd[groundColLayerIndex][Math.Clamp(i + width + 1, 0, max)].Exposness,
                    dd[groundColLayerIndex][Math.Clamp(i + width - 1, 0, max)].Exposness
                };

                neighbours.RemoveAll(x => x == -1);
                //int best = neighbours.Contains(0) ? 0 : -2;
                int best = 3;
                if(neighbours.Count > 0)
                    best = Math.Clamp(neighbours.Min(), 0, 4);

                var tThing = dd[groundColLayerIndex][i];
                tThing.Exposness = best + 1;
                dd[groundColLayerIndex][i] = tThing;
            }
            dd[groundColLayerIndex].RemoveAll(x => x.GID == 0);*/

            gdd.RemoveAll(x => x.GID == 0);
            List<TileDrawData> master = new List<TileDrawData>();

            for (int i = 0; i < dd.Length; i++)
            {
                if (dd[i].Count > 0 && dd[i][0].LayerName != "collision")
                    master = master.Concat(dd[i]).ToList();
            }
            master = master.Concat(gdd).ToList();


            return master.ToArray();
        }

        public int GetGIDFromPos(Vector2 pos, string layerName)
        {
            var layer = map.Layers.Where(x => x.name == layerName).First();
            var chpos = new Vector2(layer.chunks[0].x, layer.chunks[0].y);
            var tx = (int)MathF.Round(pos.X / 16) * 16;
            var ty = (int)MathF.Round(pos.Y / 16) * 16;

            var cor = new Vector2(tx - 16, ty - 16);

            var ground = DrawData.Where(x => x.LayerName == layerName).ToArray();
            foreach (TileDrawData data in ground)
            {
                if (data.Destination.Location.ToVector2() == cor)
                    return data.GID;
            }

            return 0;
        }

        private void ColliderAlgorithm(TiledLayer layer)
        {
            Colliders = new List<Collider>();

            Dictionary<int, Rectangle?> vertRequest = new Dictionary<int, Rectangle?>();
            Dictionary<int, Rectangle?> horiRequest = new Dictionary<int, Rectangle?>();
            Verts = new List<Vert>();

            for (int i = 0; i < layer.chunks.Length; i++)
            {
                TiledChunk ch = layer.chunks[i];
                for (int y = 0; y < ch.width; y++)
                {
                    for (int x = 0; x < ch.height; x++)
                    {
                        var index = (y * ch.width) + x;
                        var gid = ch.data[index];

                        if (gid == 71)
                            continue;

                        //have fun with this one fucko
                        var nextGid = index == (ch.width * ch.height) - 1 && i == layer.chunks.Length - 1 ? 0 :
                            index - (y * ch.height) == ch.width - 1 && i != layer.chunks.Length - 1 ? layer.chunks[i + 1].data[index - (ch.width - 1)] : ch.data[index + 1];
                        var bottomGid = -1;
                        if (gid == 10 || gid == 11 || gid == 12 || gid == 15)
                            bottomGid = y == ch.height - 1 ? layer.chunks.Where(c => c.x == ch.x && c.y == ch.y + ch.height).First().data[x] : ch.data[index + ch.height];

                        var tileX = (ch.x + x) * map.TileWidth + (int)WorldPos.X;
                        var tileY = (ch.y + y) * map.TileHeight + (int)WorldPos.Y;
                        var dest = new Rectangle(tileX, tileY, map.TileWidth, map.TileHeight);

                        var vertkey = x + ch.x;
                        var horikey = y + ch.y;

                        var h = dest.Width / 2;
                        switch (gid)
                        {
                            case 3:
                                Colliders.Add(new Collider(dest));
                                AddVert(new Vert(new Vector2(tileX, tileY), -1, -1));
                                AddVert(new Vert(new Vector2(tileX + map.TileWidth, tileY), 1, -1));
                                AddVert(new Vert(new Vector2(tileX, tileY + map.TileHeight), -1, 1));
                                AddVert(new Vert(new Vector2(tileX + map.TileWidth, tileY + map.TileHeight), 1, 1));
                                break;

                            //Top Horizontal opener, left vertical opener
                            case 61:
                                horiRequest[horikey] = dest;
                                vertRequest[vertkey] = dest;
                                AddVert(new Vert(new Vector2(tileX, tileY), -1, -1));
                                break;
                            //Top Horizontal continuer
                            case 62:
                                if (horiRequest.ContainsKey(horikey))
                                {
                                    Rectangle o = horiRequest[horikey].Value;
                                    horiRequest[horikey] = new Rectangle(o.X, o.Y, o.Width + map.TileWidth, o.Height);
                                }
                                break;
                            //Bottom Horizontal continuer
                            case 80:
                                if (horiRequest.ContainsKey(horikey))
                                {
                                    Rectangle o = horiRequest[horikey].Value;
                                    horiRequest[horikey] = new Rectangle(o.X, o.Y, o.Width + map.TileWidth, o.Height);
                                    if (nextGid == 0)
                                    {
                                        Colliders.Add(new Collider(horiRequest[horikey].Value, 0, 0, false));
                                        horiRequest.Remove(horikey);
                                    }
                                }
                                break;
                            //Top Horizontal closer, right vertical opener
                            case 63:
                                vertRequest[vertkey] = dest;

                                if (horiRequest.ContainsKey(horikey))
                                {
                                    Rectangle o = horiRequest[horikey].Value;
                                    horiRequest[horikey] = new Rectangle(o.X, o.Y, o.Width + map.TileWidth, o.Height);
                                    Colliders.Add(new Collider(horiRequest[horikey].Value, 0, 0, false));
                                    horiRequest.Remove(horikey);
                                }

                                AddVert(new Vert(new Vector2(tileX + map.TileWidth, tileY), 1, -1));
                                break;
                            //Bottom horizontal opener, left vertical closer
                            case 79:
                                horiRequest[horikey] = dest;

                                if (vertRequest.ContainsKey(vertkey))
                                {
                                    Rectangle o = vertRequest[vertkey].Value;
                                    vertRequest[vertkey] = new Rectangle(o.X, o.Y, o.Width, o.Height + map.TileHeight);
                                    Colliders.Add(new Collider(vertRequest[vertkey].Value, 2, 0, false));
                                    vertRequest.Remove(vertkey);
                                }

                                AddVert(new Vert(new Vector2(tileX, tileY + map.TileHeight), -1, 1));
                                break;
                            //Left vertical continuer
                            case 70:
                                if (vertRequest.ContainsKey(vertkey))
                                {
                                    Rectangle o = vertRequest[vertkey].Value;
                                    vertRequest[vertkey] = new Rectangle(o.X, o.Y, o.Width, o.Height + map.TileHeight);
                                }
                                break;
                            //Right vertical continuer
                            case 72:
                                if (vertRequest.ContainsKey(vertkey))
                                {
                                    Rectangle o = vertRequest[vertkey].Value;
                                    vertRequest[vertkey] = new Rectangle(o.X, o.Y, o.Width, o.Height + map.TileHeight);
                                }
                                break;
                            //Bottom Horizontal closer, right vertical closer
                            case 81:
                                if (horiRequest.ContainsKey(horikey))
                                {
                                    Rectangle o = horiRequest[horikey].Value;
                                    horiRequest[horikey] = new Rectangle(o.X, o.Y, o.Width + map.TileWidth, o.Height);
                                    Colliders.Add(new Collider(horiRequest[horikey].Value, 1, 0, false));
                                    horiRequest.Remove(horikey);
                                }
                                if (vertRequest.ContainsKey(vertkey))
                                {
                                    Rectangle o = vertRequest[vertkey].Value;
                                    vertRequest[vertkey] = new Rectangle(o.X, o.Y, o.Width, o.Height + map.TileHeight);
                                    Colliders.Add(new Collider(vertRequest[vertkey].Value, 3, 0, false));
                                    vertRequest.Remove(vertkey);
                                }

                                AddVert(new Vert(new Vector2(tileX + map.TileWidth, tileY + map.TileHeight), 1, 1));
                                break;
                            //Bottom horizontal opener, right vertical opener
                            case 59:
                                horiRequest[horikey] = dest;
                                vertRequest[vertkey] = dest;
                                AddVert(new Vert(new Vector2(tileX + map.TileWidth, tileY + map.TileHeight), 1, 1));
                                break;
                            //Bottom horizontal closer, left vertical opener
                            case 60:
                                vertRequest[vertkey] = dest;

                                if (horiRequest.ContainsKey(horikey))
                                {
                                    Colliders.Add(new Collider(horiRequest[horikey].Value, 1, 0, false));
                                    horiRequest.Remove(horikey);
                                }

                                AddVert(new Vert(new Vector2(tileX, tileY + map.TileHeight), -1, 1));
                                break;
                            //Top horizontal opener, right vertical closer
                            case 68:
                                horiRequest[horikey] = dest;

                                if (vertRequest.ContainsKey(vertkey))
                                {
                                    Colliders.Add(new Collider(vertRequest[vertkey].Value, 3, 0, false));
                                    vertRequest.Remove(vertkey);
                                }

                                AddVert(new Vert(new Vector2(tileX + map.TileWidth, tileY), 1, -1));
                                break;
                            //Top horizontal closer, left vertical closer
                            case 69:
                                if (horiRequest.ContainsKey(horikey))
                                {
                                    Colliders.Add(new Collider(horiRequest[horikey].Value, 0, 0, false));
                                    horiRequest.Remove(horikey);
                                }
                                if (vertRequest.ContainsKey(vertkey))
                                {
                                    Colliders.Add(new Collider(vertRequest[vertkey].Value, 2, 0, false));
                                    vertRequest.Remove(vertkey);
                                }

                                AddVert(new Vert(new Vector2(tileX, tileY), -1, -1));
                                break;

                            /*case 19:
                                Colliders.Add(new Collider(dest, -1, 1, true, false));
                                break;
                            case 20:
                                Colliders.Add(new Collider(dest, 1, 0, true, false));
                                break;
                            case 21:
                                Colliders.Add(new Collider(dest, -0.5f, 1, true, false));
                                break;
                            case 22:
                                Colliders.Add(new Collider(dest, -0.5f, 0.5f, true, false));
                                break;
                            case 23:
                                Colliders.Add(new Collider(dest, 0.5f, 0, true, false));
                                break;
                            case 24:
                                Colliders.Add(new Collider(dest, 0.5f, 0.5f, true, false));
                                break;*/

                            #region Platforms
                            case 25:
                                bool cont = horiRequest.ContainsKey(horikey);
                                if (nextGid == 25)
                                {
                                    if (cont)
                                    {
                                        Rectangle o = horiRequest[horikey].Value;
                                        horiRequest[horikey] = new Rectangle(o.X, o.Y, o.Width + map.TileWidth, o.Height);
                                    }
                                    else
                                    {
                                        horiRequest[horikey] = dest;
                                    }
                                }
                                else
                                {
                                    if (cont)
                                    {
                                        Rectangle o = horiRequest[horikey].Value;
                                        horiRequest[horikey] = new Rectangle(o.X, o.Y, o.Width + map.TileWidth, o.Height);
                                        Colliders.Add(new Collider(horiRequest[horikey].Value, 0, 0, true));
                                        horiRequest.Remove(horikey);
                                    }
                                    else
                                        Colliders.Add(new Collider(dest, 0, 0, true));
                                }
                                break;
                            case 16:
                                cont = horiRequest.ContainsKey(horikey);
                                if (nextGid == 16)
                                {
                                    if (cont)
                                    {
                                        Rectangle o = horiRequest[horikey].Value;
                                        horiRequest[horikey] = new Rectangle(o.X, o.Y, o.Width + map.TileWidth, o.Height);
                                    }
                                    else
                                    {
                                        horiRequest[horikey] = dest;
                                    }
                                }
                                else
                                {
                                    if (cont)
                                    {
                                        Rectangle o = horiRequest[horikey].Value;
                                        horiRequest[horikey] = new Rectangle(o.X, o.Y, o.Width + map.TileWidth, o.Height);
                                        Colliders.Add(new Collider(horiRequest[horikey].Value, 1, 0, true));
                                        horiRequest.Remove(horikey);
                                    }
                                    else
                                        Colliders.Add(new Collider(dest, 1, 0, true));
                                }
                                break;
                            case 17:
                                cont = vertRequest.ContainsKey(vertkey);
                                if (nextGid == 17)
                                {
                                    if (cont)
                                    {
                                        Rectangle o = vertRequest[vertkey].Value;
                                        vertRequest[vertkey] = new Rectangle(o.X, o.Y, o.Width, o.Height + map.TileHeight);
                                    }
                                    else
                                    {
                                        vertRequest[vertkey] = dest;
                                    }
                                }
                                else
                                {
                                    if (cont)
                                    {
                                        Rectangle o = vertRequest[vertkey].Value;
                                        vertRequest[vertkey] = new Rectangle(o.X, o.Y, o.Width, o.Height + map.TileHeight);
                                        Colliders.Add(new Collider(vertRequest[vertkey].Value, 2, 0, true));
                                        vertRequest.Remove(vertkey);
                                    }
                                    else
                                        Colliders.Add(new Collider(dest, 2, 0, true));
                                }
                                break;
                            case 26:
                                cont = vertRequest.ContainsKey(vertkey);
                                if (nextGid == 26)
                                {
                                    if (cont)
                                    {
                                        Rectangle o = vertRequest[vertkey].Value;
                                        vertRequest[vertkey] = new Rectangle(o.X, o.Y, o.Width, o.Height + map.TileHeight);
                                    }
                                    else
                                    {
                                        vertRequest[vertkey] = dest;
                                    }
                                }
                                else
                                {
                                    if (cont)
                                    {
                                        Rectangle o = vertRequest[vertkey].Value;
                                        vertRequest[vertkey] = new Rectangle(o.X, o.Y, o.Width, o.Height + map.TileHeight);
                                        Colliders.Add(new Collider(vertRequest[vertkey].Value, 3, 0, true));
                                        vertRequest.Remove(vertkey);
                                    }
                                    else
                                        Colliders.Add(new Collider(dest, 3, 0, true));
                                }
                                break;
                            #endregion

                            case 10: //rightside up slope
                                Colliders.Add(new Collider(dest, -1, 1, false, false));

                                if (nextGid == 62)
                                {
                                    horiRequest[horikey] = new Rectangle(tileX + map.TileWidth, tileY, 0, 16);
                                    AddVert(new Vert(new Vector2(tileX + map.TileWidth, tileY), -1, -1));
                                }
                                if (bottomGid == 70)
                                {
                                    vertRequest[vertkey] = new Rectangle(tileX, tileY + map.TileWidth, 16, 0);
                                    AddVert(new Vert(new Vector2(tileX, tileY + map.TileHeight), -1, -1));
                                }
                                break;
                            case 11:
                                Colliders.Add(new Collider(dest, 1, 0, false, false));

                                if (horiRequest.ContainsKey(horikey))
                                {
                                    if (horiRequest[horikey].Value.Height > map.TileHeight)
                                        AddVert(new Vert(new Vector2(tileX, tileY), 1, -1));

                                    Colliders.Add(new Collider(horiRequest[horikey].Value, 0, 0, false));
                                    horiRequest.Remove(horikey);
                                }
                                if (bottomGid == 72)
                                {
                                    vertRequest[vertkey] = new Rectangle(tileX, tileY + map.TileWidth, 16, 0);
                                    AddVert(new Vert(new Vector2(tileX + map.TileWidth, tileY + map.TileHeight), -1, -1));
                                }
                                break;
                            case 12:
                                Colliders.Add(new Collider(dest, -0.5f, 1, false, false));

                                if (bottomGid == 70)
                                {
                                    vertRequest[vertkey] = new Rectangle(tileX, tileY + map.TileWidth, 16, 0);
                                    AddVert(new Vert(new Vector2(tileX, tileY + map.TileHeight), -1, -1));
                                }
                                break;
                            case 13:
                                Colliders.Add(new Collider(dest, -0.5f, 0.5f, false, false));

                                if (nextGid == 62)
                                {
                                    horiRequest[horikey] = new Rectangle(tileX + map.TileWidth, tileY, 0, 16);
                                    AddVert(new Vert(new Vector2(tileX + map.TileWidth, tileY), -1, -1));
                                }
                                break;
                            case 14:
                                Colliders.Add(new Collider(dest, 0.5f, 0, false, false));

                                if (horiRequest.ContainsKey(horikey))
                                {
                                    if (horiRequest[horikey].Value.Height > map.TileHeight)
                                        AddVert(new Vert(new Vector2(tileX, tileY), 1, -1));

                                    Colliders.Add(new Collider(horiRequest[horikey].Value, 0, 0, false));
                                    horiRequest.Remove(horikey);
                                }
                                break;
                            case 15:
                                Colliders.Add(new Collider(dest, 0.5f, 0.5f, false, false));

                                if (bottomGid == 72)
                                {
                                    vertRequest[vertkey] = new Rectangle(tileX, tileY + map.TileWidth, 16, 0);
                                    AddVert(new Vert(new Vector2(tileX + map.TileWidth, tileY + map.TileHeight), -1, -1));
                                }
                                break;

                           case 18: //upsidedown up slope 
                                Colliders.Add(new Collider(dest, -1, 1, false, true));

                                if (vertRequest.ContainsKey(vertkey))
                                {
                                    if (vertRequest[vertkey].Value.Height > map.TileHeight) 
                                        AddVert(new Vert(new Vector2(tileX + map.TileWidth, tileY), 1, 1));

                                    Colliders.Add(new Collider(vertRequest[vertkey].Value, 3, 0, false));
                                    vertRequest.Remove(vertkey);
                                }
                                if (horiRequest.ContainsKey(horikey))
                                {
                                    if (horiRequest[horikey].Value.Width > map.TileWidth)
                                        AddVert(new Vert(new Vector2(tileX, tileY + map.TileHeight), 1, 1));

                                    Colliders.Add(new Collider(horiRequest[horikey].Value, 1, 0, false));
                                    horiRequest.Remove(horikey);
                                }
                                break;
                            case 27: //upsidedown down slope
                                Colliders.Add(new Collider(dest, 1, 0, false, true));

                                if (vertRequest.ContainsKey(vertkey))
                                {
                                    if (vertRequest[vertkey].Value.Height > map.TileHeight)
                                        AddVert(new Vert(new Vector2(tileX, tileY), -1, 1));

                                    Colliders.Add(new Collider(vertRequest[vertkey].Value, 2, 0, false));
                                    vertRequest.Remove(vertkey);
                                }

                                if(nextGid == 80)
                                {
                                    horiRequest[horikey] = new Rectangle(tileX + map.TileWidth, tileY, 0, 16);
                                    AddVert(new Vert(new Vector2(tileX + map.TileWidth, tileY + map.TileHeight), -1, 1));
                                }
                                break;
                            case 82: //upsidedown down small half slope
                                Colliders.Add(new Collider(dest, 0.5f, 0, false, true));

                                if (vertRequest.ContainsKey(vertkey))
                                {
                                    if (vertRequest[vertkey].Value.Height > map.TileHeight)
                                        AddVert(new Vert(new Vector2(tileX, tileY), -1, 1));

                                    Colliders.Add(new Collider(vertRequest[vertkey].Value, 2, 0, false));
                                    vertRequest.Remove(vertkey);
                                }
                                break;
                            case 83: //upsidedown down big half slope
                                Colliders.Add(new Collider(dest, 0.5f, 0.5f, false, true));

                                if (nextGid == 80)
                                {
                                    horiRequest[horikey] = new Rectangle(tileX + map.TileWidth, tileY, 0, 16);
                                    AddVert(new Vert(new Vector2(tileX + map.TileWidth, tileY + map.TileHeight), -1, 1));
                                }
                                break;
                            case 84: //upsidedown up big half slope
                                Colliders.Add(new Collider(dest, -0.5f, 1, false, true));

                                if (horiRequest.ContainsKey(horikey))
                                {
                                    if (horiRequest[horikey].Value.Width > map.TileWidth)
                                        AddVert(new Vert(new Vector2(tileX, tileY + map.TileHeight), 1, 1));

                                    Colliders.Add(new Collider(horiRequest[horikey].Value, 1, 0, false));
                                    horiRequest.Remove(horikey);
                                }
                                break;
                            case 85: //upsidedown up small half slope
                                Colliders.Add(new Collider(dest, -0.5f, 0.5f, false, true));

                                if (vertRequest.ContainsKey(vertkey))
                                {
                                    if (vertRequest[vertkey].Value.Height > map.TileHeight)
                                        AddVert(new Vert(new Vector2(tileX + map.TileWidth, tileY), 1, 1));

                                    Colliders.Add(new Collider(vertRequest[vertkey].Value, 3, 0, false));
                                    vertRequest.Remove(vertkey);
                                }
                                break;
                        }
                    }
                }
            }
        }

        public Rectangle GetBounds(TiledLayer layer)
        {
            if (MainMenu)
                return Rectangle.Empty;
            int horiChunks = layer.chunks.Where(x => x.y == 0).Count() * layer.chunks[0].width;
            int vertChunks = layer.chunks.Where(x => x.x == 0).Count() * layer.chunks[0].height;
            return new Rectangle((int)WorldPos.X, (int)WorldPos.Y, horiChunks, vertChunks);
        }

        public Entity[] GetEntByInterface(Type inter)
        {
            List<Entity> tents = new List<Entity>();
            foreach (Entity ent in EntityMan.Entities)
            {
                if (ent.GetType().GetInterfaces().Contains(inter))
                    tents.Add(ent);
            }
            return tents.ToArray();
        }

        public int GetCurentLev()
        {
            return CurrentLevel;
        }

        private Rectangle VecToRec(int x, int y)
        {
            return new Rectangle(x, y, 0, 0);
        }

        private void AddVert(Vert vert)
        {
            if(Verts.Where(x => x.Position == vert.Position).Count() == 0)
            {
                Verts.Add(vert);
            }
        }
    }

    public struct TileDrawData {
        public string LayerName { get; }
        public int Tileset { get; }
        public int GID { get; }
        public Rectangle Source { get; }
        public Rectangle Destination { get; }
        public float DrawLayer { get; }
        public SpriteEffects Mirror { get; }
        public int NormGid { get; set; }
        public bool Mapped { get; set; }
        public int Exposness  { get; set; }
        public TileDrawData(float dl, SpriteEffects mir, string layer, int ts, int gid, Rectangle source, Rectangle dest, int normGid, bool mapped, int exp = -1)
        {
            Mirror = mir;
            DrawLayer = dl;
            LayerName = layer;
            Tileset = ts;
            GID = gid;
            Source = source;
            Destination = dest;
            NormGid = normGid;
            Mapped = mapped;
            Exposness = exp;
        }

        public TileDrawData(Rectangle dest, int exp)
        {
            Mirror = 0;
            DrawLayer = 0;
            LayerName = null;
            Tileset = 0;
            GID = 0;
            Source = Rectangle.Empty;
            Destination = dest;
            NormGid = 0;
            Mapped = false;
            Exposness = exp;
        }
    }
}