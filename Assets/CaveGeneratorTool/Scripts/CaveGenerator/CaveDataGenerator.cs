using System;
using System.Collections.Generic;

public class CaveDataGenerator
{
    private int[,] cave;
    private Random random;
    private int width, height;
    private int wallDensityPercentaje;
    private int smoothIterations;
    private int borderDensity;
    private int wallRegionMinDensity;
    private int roomRegionMinDensity;
    private int roomConnectionsDensity;
    private List<List<RectPoint>> originalRooms;

    public CaveDataGenerator()
    {
        random = new Random();
        originalRooms = new List<List<RectPoint>>();
    }

    public CaveDataGenerator(int seed, int width, int height, int wallDensityPercentaje, int smoothIterations, int borderDensity, 
                            int wallRegionMinDensity, int roomRegionMinDensity, int roomConnectionsDensity)
    {
        originalRooms = new List<List<RectPoint>>();
        SetSeed(seed);
        SetConfiguration(width, height, wallDensityPercentaje, smoothIterations, borderDensity, wallRegionMinDensity,
                            roomRegionMinDensity, roomConnectionsDensity);
        
    }
    #region Configuration
    public void SetSeed(int seed)
    {
        random = new Random(seed);
        originalRooms.Clear();
    }

    public void SetConfiguration(int width, int height, int wallDensityPercentaje, int smoothIterations, int borderDensity,
                                    int wallRegionMinDensity, int roomRegionMinDensity, int roomConnectionsDensity)
    {
        this.width = width;
        this.height = height;
        this.wallDensityPercentaje = wallDensityPercentaje;
        this.smoothIterations = smoothIterations;
        this.borderDensity = borderDensity;
        this.wallRegionMinDensity = wallRegionMinDensity;
        this.roomRegionMinDensity = roomRegionMinDensity;
        this.roomConnectionsDensity = roomConnectionsDensity; 
        cave = new int[width, height];
    }
    #endregion

    #region CaveGeneration
    public int[,] GenerateCave(int seed)
    {
        SetSeed(seed);
        int[,] map = GenerateCave();

        return map;
    }

    public int[,] GetBorderedCave(int[,] cave)
    {
        int[,] borderedCave = new int[width + borderDensity * 2 + 1, height + borderDensity * 2 + 1];

        for (int row = 0; row < borderedCave.GetLength(0); row++)
        {
            for (int col = 0; col < borderedCave.GetLength(1); col++)
            {
                if (row >= borderDensity && row < width + borderDensity && col >= borderDensity && col < height + borderDensity)
                {
                    borderedCave[row, col] = cave[row - borderDensity, col - borderDensity];
                } // end if
                else
                {
                    if (IsBorderTile(row, col))
                        borderedCave[row, col] = CaveGeneratorUtilities.DataConstants.Ground;
                    else
                        borderedCave[row, col] = CaveGeneratorUtilities.DataConstants.Passage;
                } // end else
            } // end for
        } // end for
        return borderedCave;
    }

    public int[,] GenerateCave()
    {
        GenerateRandomValues();
        for (int i = 0; i < smoothIterations; i++)
        {
            ApplyCaveGenAlgorithm();
        } // end for
        TuneSmallAreas();
        SetConnectionState();
        SetCollectableAreas();
        return cave;
    }

    #region Cave Modifiers

    private void GenerateRandomValues()
    {
        for(int row = 0; row < width; row++ )
        {
            for(int col = 0; col < height; col++)
            {
                if(IsBorderTile(row, col))
                {
                    cave[row, col] = CaveGeneratorUtilities.DataConstants.Wall;
                } // end if
                else
                {
                    cave[row, col] = (random.Next(0, 100) < wallDensityPercentaje) ? CaveGeneratorUtilities.DataConstants.Wall : CaveGeneratorUtilities.DataConstants.Ground;
                } // end else
            } // end for
        } // end for
    }

    private void ApplyCaveGenAlgorithm()
    {
        //int[,] newMap = cave;
        for(int row = 0; row < width; row++)
        {
            for(int col = 0; col < height; col++)
            {
                
                int currentSurroundingWalls = GetNeighborWallCount(row, col);
                
                //=====================First algorithm==============================//
                if (cave[row, col] == CaveGeneratorUtilities.DataConstants.Ground)
                {
                    if (currentSurroundingWalls >= 5)
                        cave[row, col] = CaveGeneratorUtilities.DataConstants.Wall;
                }
                else
                {
                    if (currentSurroundingWalls <= 2)
                        cave[row, col] = CaveGeneratorUtilities.DataConstants.Ground;
                }
                //==================================================================//
            } // end for
        } // end for
        //cave = newMap;
    }

    private void TuneSmallAreas()
    {
        List<List<RectPoint>> wallRegions = GetCaveRegionsOfType(CaveGeneratorUtilities.DataConstants.Wall);
        

        for (int i = 0; i < wallRegions.Count; i++)
        {
            if (wallRegions[i].Count < wallRegionMinDensity)
                ReplaceRegion(wallRegions[i], CaveGeneratorUtilities.DataConstants.Ground);
        } // end for

        List<List<RectPoint>> roomRegions = GetCaveRegionsOfType(CaveGeneratorUtilities.DataConstants.Ground);
        for (int i = 0; i < roomRegions.Count; i++)
        {
            if (roomRegions[i].Count < roomRegionMinDensity)
                ReplaceRegion(roomRegions[i], CaveGeneratorUtilities.DataConstants.Wall);
        } // end for
        
    }

    private void SetConnectionState()
    {
        List<List<RectPoint>> roomRegions = GetCaveRegionsOfType(CaveGeneratorUtilities.DataConstants.Ground);
        List<List<RectPoint>> allConnections = new List<List<RectPoint>>();
        
        while (roomRegions.Count > 1)
        {
            List<RectPoint> roomToConnect = roomRegions[0];
            roomRegions.RemoveAt(0);
            List<RectPoint> connection = ConnectNearestRegions(roomToConnect, roomRegions);
            allConnections.Add(connection);
            roomRegions = GetCaveRegionsOfType(CaveGeneratorUtilities.DataConstants.Ground);
            
        } // end while
        TuneSmallAreas();
        
        for(int i = 0; i < allConnections.Count; i++)
        {
            for (int j = 0; j < allConnections[i].Count; j++)
                cave[allConnections[i][j].X, allConnections[i][j].Y] = CaveGeneratorUtilities.DataConstants.Passage;
        }
        originalRooms = GetCaveRegionsOfType(CaveGeneratorUtilities.DataConstants.Ground);
    }

    private void SetCollectableAreas()
    {
        
        for(int i = 0; i < originalRooms.Count; i++)
        {
            int collectableAmountPerRoom = originalRooms[i].Count / roomRegionMinDensity;
            SetCollectablesRandomlyInRoom(collectableAmountPerRoom, originalRooms[i]);
        } // end for
    }

    private void SetCollectablesRandomlyInRoom(int amount, List<RectPoint> room)
    {
        int collectablesLeft = amount;
        while(collectablesLeft > 0)
        {
            int placePoint = random.Next(0, room.Count - 1);
            RectPoint point = new RectPoint(room[placePoint].X, room[placePoint].Y);
            if (cave[point.X, point.Y] == CaveGeneratorUtilities.DataConstants.Ground)
            {
                cave[point.X, point.Y] = CaveGeneratorUtilities.DataConstants.Collectable;
                collectablesLeft--;
            }
            
        }
    }

    private List<RectPoint> ConnectNearestRegions(List<RectPoint> regionToConnect, List<List<RectPoint>> regions)
    {
        List<RectPoint> connectionTiles = new List<RectPoint>();
        double closestDistance = (int)Math.Pow(width, 2) + (int)Math.Pow(height, 2);

        RectPoint connectionPoint = new RectPoint();
        RectPoint closestPoint = new RectPoint();

        List<RectPoint> connectEdgePoints = GetFloorRegionBorderTiles(regionToConnect);
        for (int i = 0; i < regions.Count; i++)
        {
            List<RectPoint> edgePoints = GetFloorRegionBorderTiles(regions[i]);
            
            for(int connectPointsIndex = 0; connectPointsIndex < connectEdgePoints.Count; connectPointsIndex++)
            {
                for(int currentPointsIndex = 0; currentPointsIndex < edgePoints.Count; currentPointsIndex++)
                {
                    RectPoint connectionCandidate = connectEdgePoints[connectPointsIndex];
                    RectPoint currentPoint = edgePoints[currentPointsIndex];
                    int xDif = connectionCandidate.X - currentPoint.X;
                    int yDif = connectionCandidate.Y - currentPoint.Y;
                    int pointsDistance = (int)Math.Pow(xDif, 2) + (int)Math.Pow(yDif, 2);
                    if(pointsDistance < closestDistance)
                    {
                        closestDistance = pointsDistance;
                        connectionPoint = connectionCandidate;
                        closestPoint = currentPoint;
                    }
                } // end for
            } // end for
            
        } // end for

        connectionTiles = GetConnectionTiles(connectionPoint, closestPoint);
        for(int i = 0; i < connectionTiles.Count; i++)
        {
            int row = connectionTiles[i].X;
            int col = connectionTiles[i].Y;
            cave[row, col] = CaveGeneratorUtilities.DataConstants.Ground;
        } // end if

        return connectionTiles;
    }

    private List<RectPoint> GetConnectionTiles(RectPoint startTile, RectPoint endTile)
    {
        List<RectPoint> linearPath = GetLinePoints(startTile, endTile);

        return GetCompletePathTiles(linearPath);
    }

    private List<RectPoint> GetCompletePathTiles(List<RectPoint> rootPath)
    {
        List<RectPoint> completePath = new List<RectPoint>();
        for(int i = 0; i < rootPath.Count; i++)
        {
            for(int row = -roomConnectionsDensity; row <= roomConnectionsDensity; row++)
            {
                for(int col = -roomConnectionsDensity; col <= roomConnectionsDensity; col++)
                {
                    if(row * row + col * col <= roomConnectionsDensity * roomConnectionsDensity)
                    {
                        RectPoint currentTile = rootPath[i];
                        int circleX = currentTile.X + row;
                        int circleY = currentTile.Y + col;
                        if(!IsOutOfBounds(circleX, circleY))
                        {
                            RectPoint pathTile = new RectPoint(circleX, circleY);
                            completePath.Add(pathTile);
                        } // end if
                    } // end if
                } // end for
            } // end for
        } // end for
        completePath.AddRange(rootPath);
        return completePath;
    }

    private List<RectPoint> GetLinePoints(RectPoint p1, RectPoint p2)
    {
        List<RectPoint> linePath = new List<RectPoint>();
        int x = p1.X;
        int y = p1.Y;

        int dx = p2.X - x;
        int dy = p2.Y - y;
        bool isVerticalFunction = Math.Abs(dx) < Math.Abs(dy);

        int move;
        int gradientMove;
        int biggerAxis;
        int smallerAxis;

        if(isVerticalFunction)
        {
            biggerAxis = Math.Abs(dy);
            smallerAxis = Math.Abs(dx);
            move = Math.Sign(dy);
            gradientMove = Math.Sign(dx);
        } // end if
        else
        {
            biggerAxis = Math.Abs(dx);
            smallerAxis = Math.Abs(dy);
            move = Math.Sign(dx);
            gradientMove = Math.Sign(dy);
        } // end else

        int totalGradient = biggerAxis / 2;
        for(int i = 0; i < biggerAxis; i++)
        {
            RectPoint point = new RectPoint(x, y);
            linePath.Add(point);
            if (isVerticalFunction)
                y += move;
            else
                x += move;

            totalGradient += smallerAxis;
            if(totalGradient >= biggerAxis)
            {
                if (isVerticalFunction)
                    x += gradientMove;
                else
                    y += gradientMove;
                totalGradient -= biggerAxis;
            }
        } // end for
        return linePath;
    }

    private void ApplyBorderDensity()
    {
        for(int row = 0; row < borderDensity; row++)
        {
            for (int col = 0; col < height; col++)
            {
                cave[row, col] = CaveGeneratorUtilities.DataConstants.Wall;
                cave[width - 1 - row, col] = CaveGeneratorUtilities.DataConstants.Wall;
            } // end for
        } // end for
    }

    private void ReplaceRegion(List<RectPoint> region, int replaceState)
    {
        for(int i = 0; i < region.Count; i++)
        {
            cave[region[i].X, region[i].Y] = replaceState;
        } // end for
    }

    #endregion

    #region Cave Modifiers Utilities
    private int GetNeighborWallCount( int row, int col, int rowNeighborLevel = 1, int colNeighborLevel = 1)
    {
        int wallCount = 0;

        int firstRowNeighbor = row - rowNeighborLevel;
        int lastRowNeighbor = row + rowNeighborLevel;
        int firstColNeighbor = col - colNeighborLevel;
        int lastColNeighbor = col + colNeighborLevel;

        for(int rowNeighbor = firstRowNeighbor; rowNeighbor <= lastRowNeighbor; rowNeighbor++)
        {
            for(int colNeighbor = firstColNeighbor; colNeighbor <= lastColNeighbor; colNeighbor++)
            {
                if(rowNeighbor != row || colNeighbor != col)
                {
                    if(IsOutOfBounds(rowNeighbor, colNeighbor) || cave[rowNeighbor, colNeighbor] == CaveGeneratorUtilities.DataConstants.Wall)
                    {
                        wallCount++;
                    } // end if
                } // end if
            } // end for
        } // end for

        return wallCount;
    }

    private List<List<RectPoint>> GetCaveRegionsOfType(int type)
    {
        List<List<RectPoint>> regions = new List<List<RectPoint>>();
        bool[,] inspectedTiles = new bool[width, height];

        for(int row = 0; row < width; row++)
        {
            for(int col = 0; col < height; col++)
            {
                if(cave[row, col] == type && !inspectedTiles[row, col])
                {
                    List<RectPoint> region = GetCaveRegion(row, col);
                    regions.Add(region);
                    //CaveGeneratorControllerTest.PrintDebug("Region con tile inicial [" + row.ToString() + "," + col.ToString() + "] Elementos: " + region.Count);
                   
                    for (int i = 0; i < region.Count; i++)
                    {
                        inspectedTiles[region[i].X, region[i].Y] = true;
                    } // end for
                } // end if
            } // end for
        } // end for
        return regions;
    }

    /// <summary>
    /// Implementation of the flood fill algorithm to get a cave region
    /// </summary>
    /// <param name="startRow"></param>
    /// <param name="startCol"></param>
    /// <returns></returns>
    private List<RectPoint> GetCaveRegion(int startRow, int startCol )
    {
        List<RectPoint> region = new List<RectPoint>();

        int regionType = cave[startRow, startCol];
        bool[,] inspectedTiles = new bool[width, height];

        Queue<RectPoint> fillQueue = new Queue<RectPoint>();
        RectPoint firstTile = new RectPoint(startRow, startCol);
        fillQueue.Enqueue(firstTile);
        inspectedTiles[startRow, startCol] = true;// Visited;
        while (fillQueue.Count > 0)
        {
            RectPoint currentTile = fillQueue.Dequeue();
            region.Add(currentTile);

            int rightTileX = currentTile.X + 1;
            if (CanEnterQueue(rightTileX, currentTile.Y, inspectedTiles, regionType))
            {
                inspectedTiles[rightTileX, currentTile.Y] = true;
                fillQueue.Enqueue(new RectPoint(rightTileX, currentTile.Y));
            } // end if
            int leftTileX = currentTile.X - 1;
            if (CanEnterQueue(leftTileX, currentTile.Y, inspectedTiles, regionType))
            {
                inspectedTiles[leftTileX, currentTile.Y] = true;
                fillQueue.Enqueue(new RectPoint(leftTileX, currentTile.Y));
            } // end if
            int bottomTileY = currentTile.Y + 1;
            if (CanEnterQueue(currentTile.X, bottomTileY, inspectedTiles, regionType))
            {
                inspectedTiles[currentTile.X, bottomTileY] = true;
                fillQueue.Enqueue(new RectPoint(currentTile.X, bottomTileY));
            } // end if
            int topTileY = currentTile.Y - 1;
            if (CanEnterQueue(currentTile.X, topTileY, inspectedTiles, regionType))
            {
                inspectedTiles[currentTile.X, topTileY] = true;
                fillQueue.Enqueue(new RectPoint(currentTile.X, topTileY));
            } // end if
        } // end while
            return region;
    }

    private bool CanEnterQueue(int x, int y, bool[,] inspectedTiles, int acceptedTiles)
    {
        return !IsOutOfBounds(x, y) && cave[x, y] == acceptedTiles && !inspectedTiles[x, y];
    }

    private bool IsBorderTile(int row, int col)
    {
        return row == 0 || row == width - 1 || col == 0 || col == height - 1;
    }

    private bool IsOutOfBounds(int row, int col)
    {
        return row < 0 || row > width - 1 || col < 0 || col > height - 1;
    }

    private List<RectPoint> GetFloorRegionBorderTiles(List<RectPoint> region)
    {
        List<RectPoint> borderTiles = new List<RectPoint>();

        for(int i = 0; i < region.Count; i++)
        {
            RectPoint currentTile = region[i];
            for(int row = currentTile.X - 1; row <= currentTile.X + 1; row++)
            {
                for (int col = currentTile.Y - 1; col <= currentTile.Y + 1; col++)
                {
                    if(row == currentTile.X || col == currentTile.Y)
                    {
                        if (cave[row, col] == CaveGeneratorUtilities.DataConstants.Wall)
                            borderTiles.Add(currentTile);
                    } // end if
                } // end for
            } // end for
        } // end for

        return borderTiles;
    }
    #endregion

    #endregion


}
