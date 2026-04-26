using System;
using System.Collections.Generic;
using UnityEngine;
using Random = UnityEngine.Random;
using UnityEngine.Tilemaps;

namespace CursedDungeon.CoreAI.CellularAutomata
{
    // idea from https://www.roguebasin.com/index.php/Cellular_Automata_Method_for_Generating_Random_Cave-Like_Levels
    public class LevelGenerator : MonoBehaviour
    {
        [SerializeField]
        private int width = 80;
        [SerializeField]
        private int height = 50;

        [SerializeField]
        private int smoothIterations = 5;

        [SerializeField]
        [Range(0, 100)]
        private int initialWallPercent = 45;

        [SerializeField]
        private bool useFixedSeed;
        [SerializeField]
        private int seed = 12345;

        [SerializeField]
        private Tilemap floorTilemap;

        [SerializeField]
        private Tilemap wallTilemap;

        [SerializeField]
        private TileBase floorTile;

        [SerializeField]
        private TileBase wallTile;

        [SerializeField]
        private GameObject exitPrefab;

        [SerializeField]
        private int skeletonSpawnCount = 20;

        // dont spawn skeletons too close to player or its instadeath
        [SerializeField]
        private int skeletonMinDistanceFromPlayer = 8;

        [SerializeField]
        private int bonfireCount = 1;

        // bonfires should be near boss so orc can run to them when low hp
        [SerializeField]
        private int bonfireRadiusAroundBoss = 10;

        private GameObject exitInstance;

        private List<Vector3> skeletonSpawnPoints = new List<Vector3>();
        private List<Vector3> bonfirePoints = new List<Vector3>();

        public Vector3 PlayerSpawn { get; private set; }
        public Vector3 BossSpawn { get; private set; }
        public Vector3 ExitPosition { get; private set; }
        public IReadOnlyList<Vector3> SkeletonSpawnPoints => skeletonSpawnPoints;
        public IReadOnlyList<Vector3> BonfirePoints => bonfirePoints;

        public event Action OnLevelReady;

        private void Start()
        {
            Generate();
        }

        public void Generate()
        {
            if (!Application.isPlaying) return;

            if (useFixedSeed)
            {
                Random.InitState(seed);
            }
            else
            {
                // just want different result each time
                Random.InitState((int)(Time.realtimeSinceStartup * 1000f) ^ Environment.TickCount);
            }

            skeletonSpawnPoints.Clear();
            bonfirePoints.Clear();

            var map = new bool[width, height];
            for (var x = 0; x < width; x++)
            {
                for (var y = 0; y < height; y++)
                {
                    // border is always wall so player cant escape
                    if (x == 0 || y == 0 || x == width - 1 || y == height - 1)
                    {
                        map[x, y] = true;
                    }
                    else
                    {
                        map[x, y] = Random.Range(0, 100) < initialWallPercent;
                    }
                }
            }

            for (var i = 0; i < smoothIterations; i++) map = Smooth(map);

            // keep biggest cave
            var mainFloors = LargestFloorRegion(map);
            if (mainFloors.Count == 0)
            {
                Debug.LogWarning("No floor found");
                return;
            }

            // fill all other rooms with walls
            for (var x = 0; x < width; x++)
            {
                for (var y = 0; y < height; y++)
                {
                    if (!map[x, y] && !mainFloors.Contains(new Vector2Int(x, y))) map[x, y] = true;
                }
            }

            // spawn points
            var floorsList = new List<Vector2Int>(mainFloors);
            var playerCell = floorsList[Random.Range(0, floorsList.Count)];
            var distFromPlayer = BfsFloorDistance(map, playerCell);

            var exitCell = playerCell;
            var bestDist = -1;
            foreach (var c in floorsList)
            {
                var d = distFromPlayer[c.x, c.y];
                if (d > bestDist)
                {
                    bestDist = d;
                    exitCell = c;
                }
            }

            var minBossExit = Mathf.Max(5, width / 4);
            var bossCell = playerCell;
            var bestBossScore = -1;
            foreach (var c in floorsList)
            {
                if (c == exitCell || c == playerCell) continue;
                var dPlayer = distFromPlayer[c.x, c.y];
                // manhattan distance, not real path distance but it works
                var dExit = Mathf.Abs(c.x - exitCell.x) + Mathf.Abs(c.y - exitCell.y);
                if (dExit < minBossExit) continue;
                if (dPlayer > bestBossScore)
                {
                    bestBossScore = dPlayer;
                    bossCell = c;
                }
            }

            if (bestBossScore < 0)
            {
                foreach (var c in floorsList)
                {
                    if (c == exitCell || c == playerCell) continue;
                    var dPlayer = distFromPlayer[c.x, c.y];
                    if (dPlayer > bestBossScore)
                    {
                        bestBossScore = dPlayer;
                        bossCell = c;
                    }
                }
            }

            // shuffle floors so skeleton positions are random each game
            var shuffled = new List<Vector2Int>(floorsList);
            for (var i = shuffled.Count - 1; i > 0; i--)
            {
                var j = Random.Range(0, i + 1);
                (shuffled[i], shuffled[j]) = (shuffled[j], shuffled[i]);
            }

            foreach (var c in shuffled)
            {
                if (skeletonSpawnPoints.Count >= skeletonSpawnCount) break;
                if (c == playerCell || c == exitCell || c == bossCell) continue;
                if (distFromPlayer[c.x, c.y] < skeletonMinDistanceFromPlayer) continue;
                skeletonSpawnPoints.Add(CellToWorld(c));
            }

            // bonfire near boss
            var nearBoss = new List<Vector2Int>();
            foreach (var c in floorsList)
            {
                var dx = Mathf.Abs(c.x - bossCell.x);
                var dy = Mathf.Abs(c.y - bossCell.y);
                if (Mathf.Max(dx, dy) <= bonfireRadiusAroundBoss)
                {
                    nearBoss.Add(c);
                }
            }

            if (nearBoss.Count == 0) nearBoss = floorsList;
            for (var b = 0; b < bonfireCount && nearBoss.Count > 0; b++)
            {
                var idx = Random.Range(0, nearBoss.Count);
                bonfirePoints.Add(CellToWorld(nearBoss[idx]));
                nearBoss.RemoveAt(idx);
            }

            PlayerSpawn = CellToWorld(playerCell);
            BossSpawn = CellToWorld(bossCell);
            ExitPosition = CellToWorld(exitCell);

            LevelPainter.Paint(map, floorTilemap, wallTilemap, floorTile, wallTile);

            if (exitInstance != null) Destroy(exitInstance);
            if (exitPrefab != null) exitInstance = Instantiate(exitPrefab, ExitPosition, Quaternion.identity);

            Debug.Log("Floors: " + mainFloors.Count + ", skeletons: " + skeletonSpawnPoints.Count);

            if (OnLevelReady != null) OnLevelReady.Invoke();
        }

        private Vector3 CellToWorld(Vector2Int cell)
        {
            return floorTilemap.GetCellCenterWorld(new Vector3Int(cell.x, cell.y, 0));
        }

        // if more than 4 of 8 neighbors are walls become wall, less than 4 become floor
        private bool[,] Smooth(bool[,] map)
        {
            var w = map.GetLength(0);
            var h = map.GetLength(1);
            var next = new bool[w, h];

            for (var x = 0; x < w; x++)
            {
                for (var y = 0; y < h; y++)
                {
                    var wallNeighbors = 0;
                    for (var nx = -1; nx <= 1; nx++)
                    {
                        for (var ny = -1; ny <= 1; ny++)
                        {
                            if (nx == 0 && ny == 0) continue;
                            var cx = x + nx;
                            var cy = y + ny;
                            // out of bounds counts as wall
                            if (cx < 0 || cy < 0 || cx >= w || cy >= h)
                            {
                                wallNeighbors++;
                            }
                            else if (map[cx, cy]) wallNeighbors++;
                        }
                    }

                    if (wallNeighbors > 4) next[x, y] = true;
                    else if (wallNeighbors < 4) next[x, y] = false;
                    else next[x, y] = map[x, y];
                }
            }

            return next;
        }

        // find biggest connected room
        private HashSet<Vector2Int> LargestFloorRegion(bool[,] map)
        {
            var w = map.GetLength(0);
            var h = map.GetLength(1);
            var visited = new bool[w, h];
            HashSet<Vector2Int> best = null;
            var bestCount = 0;

            for (var x = 0; x < w; x++)
            {
                for (var y = 0; y < h; y++)
                {
                    if (map[x, y] || visited[x, y]) continue;
                    var region = new HashSet<Vector2Int>();
                    var q = new Queue<Vector2Int>();
                    visited[x, y] = true;
                    q.Enqueue(new Vector2Int(x, y));

                    while (q.Count > 0)
                    {
                        var c = q.Dequeue();
                        region.Add(c);

                        // try all 4 neighbors
                        TryVisit(map, visited, q, c.x + 1, c.y);
                        TryVisit(map, visited, q, c.x - 1, c.y);
                        TryVisit(map, visited, q, c.x, c.y + 1);
                        TryVisit(map, visited, q, c.x, c.y - 1);
                    }

                    if (region.Count > bestCount)
                    {
                        bestCount = region.Count;
                        best = region;
                    }
                }
            }

            return best ?? new HashSet<Vector2Int>();
        }

        private void TryVisit(bool[,] map, bool[,] visited, Queue<Vector2Int> q, int x, int y)
        {
            var w = map.GetLength(0);
            var h = map.GetLength(1);

            if (x < 0 || y < 0 || x >= w || y >= h) return;
            if (map[x, y]) return;
            if (visited[x, y]) return;

            visited[x, y] = true;
            q.Enqueue(new Vector2Int(x, y));
        }

        // returns 2d array where each cell has number of steps from start
        private int[,] BfsFloorDistance(bool[,] map, Vector2Int start)
        {
            var w = map.GetLength(0);
            var h = map.GetLength(1);
            var dist = new int[w, h];
            for (var x = 0; x < w; x++)
            for (var y = 0; y < h; y++) dist[x, y] = -1;

            var q = new Queue<Vector2Int>();
            dist[start.x, start.y] = 0;
            q.Enqueue(start);

            while (q.Count > 0)
            {
                var c = q.Dequeue();
                var d = dist[c.x, c.y];
                TryEnqueue(map, dist, q, c.x + 1, c.y, d + 1);
                TryEnqueue(map, dist, q, c.x - 1, c.y, d + 1);
                TryEnqueue(map, dist, q, c.x, c.y + 1, d + 1);
                TryEnqueue(map, dist, q, c.x, c.y - 1, d + 1);
            }

            return dist;
        }

        private void TryEnqueue(bool[,] map, int[,] dist, Queue<Vector2Int> q, int x, int y, int nd)
        {
            var w = map.GetLength(0);
            var h = map.GetLength(1);
            if (x < 0 || y < 0 || x >= w || y >= h) return;
            if (map[x, y]) return;
            if (dist[x, y] >= 0) return;

            dist[x, y] = nd;
            q.Enqueue(new Vector2Int(x, y));
        }
    }
}
