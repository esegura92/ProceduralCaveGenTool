using UnityEngine;

public class Grid
{
    private Cell[,] cells;

    public Cell[,] Cells => cells;

    public int Rows => cells.GetLength(0);
    public int Columns => cells.GetLength(1);

    public Grid(int[,] map, float cellSize)
    {
        CornerNode[,] cornerNodes = GetCornerNodes(map, cellSize);

        int rowCount = map.GetLength(0);
        int columnCount = map.GetLength(1);
        cells = GetCells(cornerNodes, rowCount, columnCount);
    }

    private CornerNode[,] GetCornerNodes(int[,] statesMap, float cellSize)
    {

        int rowCount = statesMap.GetLength(0);
        int columnCount = statesMap.GetLength(1);
        float gridWidth = rowCount * cellSize;
        float gridHeight = columnCount * cellSize;

        CornerNode[,] cornerNodes = new CornerNode[rowCount, columnCount];

        for (int row = 0; row < rowCount; row++)
        {
            for (int col = 0; col < columnCount; col++)
            {
                Vector3 pos = new Vector3(-gridWidth / 2 + row * cellSize / 2, 0, -gridHeight / 2 + col * cellSize + cellSize / 2);
                bool isOn = (statesMap[row, col] == CaveGeneratorUtilities.DataConstants.Wall);
                cornerNodes[row, col] = new CornerNode(pos, isOn, cellSize);
            } // end for
        } // end for

        return cornerNodes;
    }

    private Cell[,] GetCells(CornerNode[,] nodes, int rowCount, int columnCount)
    {
        Cell[,] cells = new Cell[rowCount - 1, columnCount - 1];
        for(int row = 0; row < rowCount - 1; row++)
        {
            for(int col = 0; col < columnCount - 1; col++ )
            {
                Cell cell = new Cell(nodes[row, col + 1], nodes[row + 1, col + 1], nodes[row, col], nodes[row + 1, col]);

                cells[row, col] = cell;
            } // end for
        } // end for

        return cells;
    }
}
