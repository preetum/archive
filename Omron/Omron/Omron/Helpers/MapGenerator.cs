using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using Microsoft.Xna.Framework;
using Omron.Actors;

namespace Omron
{
    static class MapGenerator
    {

        enum TileType
        {
            Empty,
            Wall,
            R1,
            R2
        }
        static int sx, sy;
        public static void renewSeed()
        {
            sx = MathHelper.Rand.Next();
            sy = MathHelper.Rand.Next(); 
        }
        /// <summary>
        /// returns floats from (-1,1)
        /// </summary>
        /// <param name="x"></param>
        /// <param name="y"></param>
        /// <returns></returns>
        public static float noise(int xN, int yN)
        {
            int x = xN + sx; //offset x and y by seed values
            int y = yN + sy;

            int n = x + y * 57;
            n = (n << 13) ^ n;
            return (1.0f - ((n * (n * n * 15731 + 789221) + 1376312589) & 0x7fffffff) / 1073741824.0f);
        }
        public static float smoothNoise(int x, int y)
        {
            float neighbors =
                noise(x, y + 1) +
                noise(x, y - 1) +
                noise(x + 1, y) +
                noise(x - 1, y) +
                noise(x + 1, y - 1) +
                noise(x - 1, y + 1)
                / 6f;

            float root =
                noise(x, y);

            float smooth = 0.5f;
            return (1f - smooth) * root + smooth * neighbors;
        }
        public static float interp(float a, float b, float x)
        {
            var f = 0.5f * (1f - (float)Math.Cos((float)Math.PI * x)); //map [0,1](x) -> [0,1](f) smoothly via cosine
            return a * (1f - f) + b * f;
        }
        public static float interpSmoothNoise(float x, float y)
        {
            int xI = (int)x;
            float xF = x - xI;
            int yI = (int)y;
            float yF = y - yI;

            float v1 = smoothNoise(xI, yI);
            float v2 = smoothNoise(xI + 1, yI);
            float v3 = smoothNoise(xI, yI + 1);
            float v4 = smoothNoise(xI + 1, yI + 1);

            var m1 = interp(v1, v2, xF);
            var m2 = interp(v3, v4, xF);

            return interp(m1, m2, yF);
        }
        public static float perlinNoise(float x, float y, float persistence, int octaves)
        {
            var p = persistence;
            var n = octaves;

            float sum = 0.0f;
            for (int i = 0; i < n; i++)
            {
                var f = (float)Math.Pow(2, i);
                var A = (float)Math.Pow(p, i);
                sum += A * interpSmoothNoise(x * f, y * f);
            }
            return sum;
        }
        public static float[,] genPerlinField(int xsize, int ysize, float persistence, int octaves)
        {
            float[,] field = new float[xsize, ysize];

            for (int x = 0; x < xsize; x++)
                for (int y = 0; y < ysize; y++)
                {
                    field[x, y] = perlinNoise(x, y, persistence, octaves);
                }
            return field;
        }

        static TileType[,] genTileMap(int xsize, int ysize)
        {
            float p = 0.16f;
            int n = 2;
            float threshWall = -0.1f;
            float threshR1 = 1.3f;
            float threshR2 = -1.7f;

            TileType[,] tiles = new TileType[xsize, ysize];
            for (int x = 0; x < xsize; x++)
            {
                for (int y = 0; y < ysize; y++)
                {
                    float k = perlinNoise(x, y, p, n);

                    tiles[x, y] = TileType.Empty;

                    if (k < threshR2)
                        tiles[x, y] = TileType.R2;
                    else if (k > threshR1)
                        tiles[x, y] = TileType.R1;
                    else if (k > threshWall)
                        tiles[x, y] = TileType.Wall;
                }
            }
            return tiles;
        }
        public static void GeneratePerlinMap(World world)
        {
            var tiles = genTileMap(world.TileGrid.U_length, world.TileGrid.V_length);

            for (int u = 0; u < world.TileGrid.U_length; u++)
            {
                for (int v = 0; v < world.TileGrid.V_length; v++)
                {
                    Point uv = new Point(u, v);
                    var pos = world.TileGrid.UVToScreen(uv);

                    if (u == 0 || v == 0 || u == world.TileGrid.U_length - 1 || v == world.TileGrid.V_length - 1)
                    {
                        //the border
                        makeMonolith(pos, world);
                    }
                    else if (tiles[u, v] == TileType.Wall)
                    {
                        makeWall(pos, world);
                    }
                    else if (tiles[u, v] == TileType.R1)
                    {
                        makeMetal(pos, world);
                    }
                    else if (tiles[u, v] == TileType.R2)
                    {
                        makeCrystalFamily(pos, world);
                    }
                }
            }
        }

        static void makeWall(Vector2 pos, World world)
        {
            FatherBuilding build = (FatherBuilding)UnitConverter.CreateActor("Wall", pos, world.Faction);
            build.OnWorkedOn(build.WorkNeeded);
            world.AddActor(build);
        }

        static void makeMonolith(Vector2 pos, World world)
        {
            FatherBuilding build = (FatherBuilding)UnitConverter.CreateActor("Monolith", pos, world.Faction);
            build.OnWorkedOn(build.WorkNeeded);
            world.AddActor(build);
        }

        static void makeMetal(Vector2 pos, World world)
        {
            FatherBuilding build = (FatherBuilding)UnitConverter.CreateActor("Metal", pos, world.Faction);
            build.OnWorkedOn(build.WorkNeeded);
            world.AddActor(build);
        }

        static void makeCrystal(Vector2 pos, World world)
        {
            FatherBuilding build = (FatherBuilding)UnitConverter.CreateActor("Crystal", pos, world.Faction);
            build.OnWorkedOn(build.WorkNeeded);
            world.AddActor(build);
        }

        static void makeCrystalFamily(Vector2 pos, World world)
        {
            int num = new Random().Next(3, 7);
            float x = pos.X - 0.4f;
            float y = pos.Y - 0.2f;
            int count = 0;
            for (int i = 0; i < num; i++)
            {
                makeCrystal(new Vector2(x, y), world);
                x += 0.25f;
                count++;
                if (count == 3)
                {
                    count = 0;
                    y += 0.2f;
                    x = x - 0.25f * 3 + 0.1f;
                }
            }
        }
    }
}
