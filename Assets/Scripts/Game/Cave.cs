using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class Cave : MonoBehaviour
{
    public List<CaveTile> Data = new List<CaveTile>();
    public float Scale;
    public int Width;
    public int Height;

    private Vector3 Position => transform.position;
    private Dictionary<Vector3, CaveTile> tilesPosition;

    private void Awake()
    {
        if (Application.isPlaying)
        {
            tilesPosition = new Dictionary<Vector3, CaveTile>();
            CalculatePositions();
        }
           
    }

    private void CalculatePositions()
    {
        for(int i = 0; i < Data.Count; i++)
        {
            float x = Position.x + ((-Width / 2 * Scale) + (Scale * Data[i].X));
            float z = Position.z + ((-Height / 2 * Scale) + (Scale * Data[i].Y));
            Vector3 tilePos = new Vector3(x, transform.position.y, z);
            tilesPosition.Add(tilePos, Data[i]);
        } // end for
    }

    public int GetValueAt(int x, int y)
    {
        CaveTile tile = new CaveTile(x, y, -1);
        
        if (!IsOutOfBounds(tile.X, tile.Y))
        {
            for(int i = 0; i < Data.Count; i++)
            {
                if (Data[i].Equals(tile))
                    tile.Value = Data[i].Value;
            } // end for
        }
        
        return tile.Value;
    }

    private bool IsOutOfBounds(int row, int col)
    {
        return row < 0 || row > Width - 1 || col < 0 || col > Height - 1;
    }

    //public Vector3 GetPositionAtTile()
    public Vector3 GetPositionAtTile(int x, int y)
    {
        Vector3 position = Vector3.zero;
        CaveTile tile = new CaveTile(x, y, -1);
        foreach (KeyValuePair<Vector3, CaveTile> keyValue in tilesPosition)
        {
            if(keyValue.Value.Equals(tile))
            {
                position = keyValue.Key;
                break;
            } // end if
        } // end for

        return position;
    }

    public CaveTile GetTileAtPosition(Vector3 pos)
    {
        float shortestDistance = 0;
        bool isPosibleTileFound = false;
        CaveTile tile = new CaveTile(-1, -1, -1);
        foreach (KeyValuePair<Vector3, CaveTile> keyValue in tilesPosition)
        {
            float currentDistance = Vector3.Distance(keyValue.Key, pos);
            if (currentDistance < shortestDistance || !isPosibleTileFound)
            {
                CaveTile currentTile = keyValue.Value;
                if (currentTile.Value == CaveGeneratorUtilities.DataConstants.Ground)
                {
                    shortestDistance = currentDistance;
                    isPosibleTileFound = true;
                    tile = new CaveTile(currentTile.X, currentTile.Y, currentTile.Value);
                } // end if
            } // end for
        }

        return tile;
    }

    public List<CaveTile> GetTilesWithValue(int value)
    {
        List<CaveTile> tiles = new List<CaveTile>();
        for(int i = 0; i < Data.Count; i++)
        {
            if (Data[i].Value == value)
                tiles.Add(Data[i]);
        } // end for

        return tiles;
    }
}
