using UnityEngine;
using UnityEngine.Tilemaps;

namespace CursedDungeon.CoreAI.CellularAutomata
{
    public static class LevelPainter
    {
        public static void Paint(bool[,] wallMap, Tilemap floorTilemap, Tilemap wallTilemap, TileBase floorTile, TileBase wallTile)
        {
            var w = wallMap.GetLength(0);
            var h = wallMap.GetLength(1);

            for (var x = 0; x < w; x++)
            {
                for (var y = 0; y < h; y++)
                {
                    var cell = new Vector3Int(x, y, 0);
                    if (wallMap[x, y])
                    {
                        wallTilemap.SetTile(cell, wallTile);
                        floorTilemap.SetTile(cell, null);
                    }
                    else
                    {
                        floorTilemap.SetTile(cell, floorTile);
                        wallTilemap.SetTile(cell, null);
                    }
                }
            }
        }
    }
}
