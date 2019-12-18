using System.Collections;
using System.Collections.Generic;

public class PlayerTracker 
{
    private const int PlayerValue = 10;

    private int range;

    public PlayerTracker(int range)
    {
        this.range = range;
    }

    public int[,] TrackPlayer(CaveTile center, Cave map)
    {
        int[,] trackedValues = new int[range, range];
        int startingRow = center.X - range / 2;  
        int startingCol = center.Y - range / 2;
        //TextureCreator.print("Center tile [" + center.X.ToString() + "," + center.Y.ToString() +"]");
        //TextureCreator.print("Starting value [" + startingRow.ToString() + "," + startingCol.ToString());
        for(int row = 0; row < range; row++)
        {
            startingCol = center.Y - range / 2;
            for (int col = 0; col < range; col++)
            {
                if (startingRow == center.X && startingCol == center.Y)
                {
                    trackedValues[row, col] = PlayerValue;
                    //TextureCreator.print("En player value");
                }
                else
                {
                    trackedValues[row, col] = map.GetValueAt(startingRow, startingCol);
                   // TextureCreator.print("Tile [" + startingRow.ToString() + "," +startingCol.ToString() + "] = " + map.GetValueAt(startingRow, startingCol).ToString());
                }
                    
                startingCol++;
            } // end if
            startingRow++;
        } // end if

        return trackedValues;
    }

}
