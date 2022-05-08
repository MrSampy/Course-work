using CourseWork.Map.Helpers;

namespace CourseWork.Map;

public class MapBuilder
{
    private static Random Rnd = Random.Shared;
    private int Seed { get; }
    public int Width { get; }
    public int Height { get; }
    public int MaxRiverCount { get; }
    public Tile[,] Tiles;
    public List<Tile[]> Rivers = new List<Tile[]>();

    public MapBuilder(int width, int height)
    {
        Width = width;
        Height = height;
        MaxRiverCount = Rnd.Next(1, 5);
        Tiles = new Tile[Width, Height];
        Seed = Rnd.Next(1, 1000);
        PerlinNoise perlinNoise = new PerlinNoise(Seed, Width, Height);
        for (int x = 0; x < width; x++)
        {
            for (int y = 0; y < height; y++)
            {
                Tiles[x, y] = new Tile(x, y, perlinNoise.MakeNumber(x, y));
            }
        }

        FindNeighbours();
        GenerateRivers();
        UpdateBitmasks();
    }

    private void FindNeighbours()
    {
        for (int x = 0; x < Width; x++)
        {
            for (int y = 0; y < Height; y++)
            {
                Tiles[x, y].LeftTile = (y - 1 < 0) ? null : Tiles[x, y - 1];
                Tiles[x, y].RightTile = (y + 1 >= Height) ? null : Tiles[x, y + 1];
                Tiles[x, y].TopTile = (x - 1 < 0) ? null : Tiles[x - 1, y];
                Tiles[x, y].BottomTile = (x + 1 >= Width) ? null : Tiles[x + 1, y];
                Tiles[x, y].Neighbours = new Tile?[4]
                    {Tiles[x, y].LeftTile, Tiles[x, y].RightTile, Tiles[x, y].TopTile, Tiles[x, y].BottomTile};
            }
        }
    }

    private void UpdateBitmasks()
    {
        for (int x = 0; x < Width; x++)
        for (int y = 0; y < Height; y++)
            Tiles[x, y].UpdateBitmask();
    }

    private void FindPath(Tile starttile, ref List<Tile> river)
    {
        ref Tile temptile = ref Tiles[starttile.X, starttile.Y];
        if (river.Contains(temptile))
        {
            river.Clear();
            return;
        }

        if (temptile.IsLand && !temptile.HasRiver)
        {
            river.Add(temptile);
            Tile nexttile = temptile.GetNextPixRiver(Convert.ToBoolean(river.Count) ? river.Last() : temptile);
            temptile.HasRiver = true;
            FindPath(nexttile, ref river);
        }
    }

    private void GenerateRivers()
    {
        int riverCount = MaxRiverCount;
        while (riverCount != 0)
        {
            int x = Rnd.Next(0, Width - 1), y = Rnd.Next(0, Height - 1);
            if (Tiles[x, y].HeightValue < Constants.MinRiverHeight)
                continue;
            List<Tile> river = new List<Tile>();
            FindPath(Tiles[x, y], ref river);
            if (river.Count == 0 || river.Last().Biome.TBiome != Constants.Biomes.Sand)
                continue;
            Rivers.Add(river.ToArray());
            --riverCount;
        }

        for (int i = 0; i < Rivers.Count; i++)
        for (int j = 0; j < Rivers[i].Length; j++)
            Tiles[Rivers[i][j].X, Rivers[i][j].Y].Biome =
                new TilesBiome(Constants.Biomes.River, Constants.ShallowWater);
    }
}