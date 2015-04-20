using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

using Microsoft.Xna.Framework;

namespace Omron.Framework
{
    public enum TileState : byte
    {
        Undiscovered,
        Inactive,
        Active
    }

    public delegate void TileDiscovered(int u, int v);

    public class FogOfWarGrid
    {
        public event TileDiscovered NotifyTileDiscovered;

        public byte[,] fogGrid;

        public int usize, vsize;

        public FogOfWarGrid(int usize, int vsize)
        {
            this.usize = usize;
            this.vsize = vsize;
            fogGrid = new byte[usize, vsize];

            for (int u = 0; u < usize; u++)
                for (int v = 0; v < vsize; v++)
                {
                    fogGrid[u, v] = 255;
                }
        }
        public TileState GetTileState(Point uv)
        {
            return GetTileState(uv.X, uv.Y);
        }
        public TileState GetTileState(int u, int v)
        {
            if (u < 0 || u >= usize || v < 0 || v >= vsize) return TileState.Undiscovered; //corruption error checking

            byte fog = fogGrid[u, v];
            if (fog == 255)
                return TileState.Undiscovered;
            else if (fog == 0)
                return TileState.Inactive;
            else
                return TileState.Active;
        }
        public void Discover(int u, int v)
        {
            if (fogGrid[u, v] == 255)
            {
                fogGrid[u, v] = 0;

                if (NotifyTileDiscovered != null)
                    NotifyTileDiscovered(u, v);
            }
        }
        public void DiscoverAll()
        {
            for (int u = 0; u < usize; u++)
                for (int v = 0; v < vsize; v++)
                {
                    Discover(u, v);
                }
        }
        public void DeactivateAll()
        {
            for (int u = 0; u < usize; u++)
                for (int v = 0; v < vsize; v++)
                {
                    if (fogGrid[u, v] != 255)
                        fogGrid[u, v] = 0;
                }
        }
        public void ActivateAll()
        {
            for (int u = 0; u < usize; u++)
                for (int v = 0; v < vsize; v++)
                {
                    if (fogGrid[u, v] == 255 || fogGrid[u, v] == 0)
                        fogGrid[u, v] = 1;
                }
        }
        public void IncrementVis(int u, int v)
        {
            Discover(u, v);
            fogGrid[u, v]++;
        }
        public void DecrementVis(int u, int v)
        {
            fogGrid[u, v]--;
        }
    }
}
