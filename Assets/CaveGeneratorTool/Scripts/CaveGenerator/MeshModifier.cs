using System.Collections.Generic;
using UnityEngine;

public class MeshModifier
{
    private const int NoConnectionVertex = -1;

    private Grid cellGrid;
    private List<Vector3> meshVertices;
    private List<int> meshTriangles;
    private List<List<int>> borderVertexConnections;
    private Dictionary<int, List<Triangle>> vertexTriangleGroup;

    private HashSet<int> repeatedVertexHelper;

    private float scale;
    private float wallMeshHeight;

    public float Scale => scale;
    public float WallHeight => wallMeshHeight;


    public MeshModifier()
    {
        meshVertices = new List<Vector3>();
        meshTriangles = new List<int>();
        borderVertexConnections = new List<List<int>>();
        repeatedVertexHelper = new HashSet<int>();
        vertexTriangleGroup = new Dictionary<int, List<Triangle>>();
        scale = 1;
        wallMeshHeight = 1;
    }

    public MeshModifier(float scale, float height) : this()
    {
        SetProperties(scale, height);
    }

    public void SetProperties(float scale, float height)
    {
        this.scale = scale;
        wallMeshHeight = height;
    }

    private void ClearHelpers()
    {
        borderVertexConnections.Clear();
        repeatedVertexHelper.Clear();
        meshVertices.Clear();
        meshTriangles.Clear();
        vertexTriangleGroup.Clear();
    }
    
    public void ModifyMesh(Mesh shape, Mesh edges, int[,] mapData, float scale, float height)
    {
        SetProperties(scale, height);
        ModifyMesh(shape, edges, mapData);   
    }

    public void ModifyMesh(Mesh shape, Mesh edges, int[,] mapData)
    {
        ClearHelpers();
        cellGrid = new Grid(mapData, scale);

        Triangulate();

        shape.vertices = meshVertices.ToArray();
        shape.triangles = meshTriangles.ToArray();
        shape.RecalculateNormals();
        ModifyUVs(shape, mapData, scale, 10);
        ModifyMeshEdges(edges, wallMeshHeight);
    }

    private void ModifyUVs(Mesh mesh, int[,] mapData, float scale, int tiling)
    {
        int width = mapData.GetLength(0);
        Vector2[] UVs = new Vector2[meshVertices.Count];
        for(int i = 0; i < meshVertices.Count; i++)
        {
            float start = -width / 2 * scale;
            float end = width / 2 * scale;
            float gradientX = Mathf.InverseLerp(start, end, meshVertices[i].x) * tiling;
            float grandientY = Mathf.InverseLerp(start, end, meshVertices[i].z) * tiling;
            UVs[i] = new Vector2(gradientX, grandientY);
        } // end for
        mesh.uv = UVs;
    }

    public void SetMeshCollider(GameObject meshGO, Mesh mesh)
    {
        MeshCollider colliders = meshGO.AddComponent<MeshCollider>();
        colliders.sharedMesh = mesh;
    }

    private void ModifyMeshEdges(Mesh edges, float edgesAltitude)
    {
        CreateEdges();
        List<Vector3> borderVertices = new List<Vector3>();
        List<int> bordersTriangles = new List<int>();

        for (int i = 0; i < borderVertexConnections.Count; i++)
        {
            List<int> currentBorderConnection = borderVertexConnections[i];
            for (int j = 0; j < currentBorderConnection.Count - 1; j++)
            {
                int startingIndex = borderVertices.Count;

                borderVertices.Add(meshVertices[currentBorderConnection[j]]);
                borderVertices.Add(meshVertices[currentBorderConnection[j + 1]]);
                borderVertices.Add(meshVertices[currentBorderConnection[j]] - Vector3.up * edgesAltitude);
                borderVertices.Add(meshVertices[currentBorderConnection[j + 1]] - Vector3.up * edgesAltitude);

                bordersTriangles.Add(startingIndex + 0);
                bordersTriangles.Add(startingIndex + 2);
                bordersTriangles.Add(startingIndex + 3);

                bordersTriangles.Add(startingIndex + 3);
                bordersTriangles.Add(startingIndex + 1);
                bordersTriangles.Add(startingIndex + 0);


            } // end for
        } // end for

        edges.vertices = borderVertices.ToArray();
        edges.triangles = bordersTriangles.ToArray();
        edges.RecalculateNormals();

    }



    private void Triangulate()
    {
        for (int row = 0; row < cellGrid.Rows; row++)
        {
            for (int col = 0; col < cellGrid.Columns; col++)
            {
                TriangulateCell(cellGrid.Cells[row, col]);
            } // end for
        } // end for
    }

    private void TriangulateCell(Cell cell)
    {
        const int MidLeftToBottomLeftTriangle = 1;
        const int BottomRightToMidRightTriangle = 2;
        const int MidBottomSquare = 3;
        const int TopRightToMidTopTriangle = 4;
        const int RightHexagon = 5;
        const int MidRightSquare = 6;
        const int BottomRightPentagon = 7;
        const int TopLeftToMidLeftTriangle = 8;
        const int MidLeftSquare = 9;
        const int LeftHexagon = 10;
        const int BottomLeftPentagon = 11;
        const int MidAboveSquare = 12;
        const int TopLeftPentagon = 13;
        const int TopRightPentagon = 14;
        const int CompleteSquare = 15;

        Node[] nodes = new Node[0];
        switch (cell.ConfigurationValue)
        {
            case MidLeftToBottomLeftTriangle:
                nodes = new Node[] { cell.MidLeft, cell.MidBottom, cell.BottomLeft };
                break;
            case BottomRightToMidRightTriangle:
                nodes = new Node[] { cell.BottomRight, cell.MidBottom, cell.MidRight };
                break;
            case TopRightToMidTopTriangle:
                nodes = new Node[] { cell.TopRight, cell.MidRight, cell.MidTop };
                break;
            case TopLeftToMidLeftTriangle:
                nodes = new Node[] { cell.TopLeft, cell.MidTop, cell.MidLeft };
                break;

            case MidBottomSquare:
                nodes = new Node[] { cell.MidRight, cell.BottomRight, cell.BottomLeft, cell.MidLeft };
                break;
            case MidRightSquare:
                nodes = new Node[] { cell.MidTop, cell.TopRight, cell.BottomRight, cell.MidBottom };
                break;
            case MidLeftSquare:
                nodes = new Node[] { cell.TopLeft, cell.MidTop, cell.MidBottom, cell.BottomLeft };
                break;
            case MidAboveSquare:
                nodes = new Node[] { cell.TopLeft, cell.TopRight, cell.MidRight, cell.MidLeft };
                break;
            case RightHexagon:
                nodes = new Node[] { cell.MidTop, cell.TopRight, cell.MidRight, cell.MidBottom, cell.BottomLeft, cell.MidLeft };
                break;
            case LeftHexagon:
                nodes = new Node[] { cell.TopLeft, cell.MidTop, cell.MidRight, cell.BottomRight, cell.MidBottom, cell.MidLeft };

                break;

            case BottomRightPentagon:
                nodes = new Node[] { cell.MidTop, cell.TopRight, cell.BottomRight, cell.BottomLeft, cell.MidLeft };
                break;
            case BottomLeftPentagon:
                nodes = new Node[] { cell.TopLeft, cell.MidTop, cell.MidRight, cell.BottomRight, cell.BottomLeft };
                break;
            case TopLeftPentagon:
                nodes = new Node[] { cell.TopLeft, cell.TopRight, cell.MidRight, cell.MidBottom, cell.BottomLeft };
                break;
            case TopRightPentagon:
                nodes = new Node[] { cell.TopLeft, cell.TopRight, cell.BottomRight, cell.MidBottom, cell.MidLeft };
                break;

            case CompleteSquare:
                nodes = new Node[] { cell.TopLeft, cell.TopRight, cell.BottomRight, cell.BottomLeft };
                repeatedVertexHelper.Add(cell.TopLeft.VertexIndex);
                repeatedVertexHelper.Add(cell.TopRight.VertexIndex);
                repeatedVertexHelper.Add(cell.BottomRight.VertexIndex);
                repeatedVertexHelper.Add(cell.BottomLeft.VertexIndex);
                break;
        } // end switch

        if (nodes.Length > 0)
            SetMeshData(nodes);
    }

    private void SetMeshData(Node[] nodes)
    {
        const int TriangleShape = 3;
        const int SquareShape = 4;
        const int PentagonShape = 5;
        const int HexagonShape = 6;
        SetVertices(nodes);
        switch (nodes.Length)
        {
            case TriangleShape:
                AddTriangle(nodes[0], nodes[1], nodes[2]);
                break;
            case SquareShape:
                AddSquare(nodes[0], nodes[1], nodes[2], nodes[3]);
                break;
            case PentagonShape:
                AddPentagon(nodes[0], nodes[1], nodes[2], nodes[3], nodes[4]);
                break;
            case HexagonShape:
                AddHexagon(nodes[0], nodes[1], nodes[2], nodes[3], nodes[4], nodes[5]);
                break;
        } // end switch

    }
    private void AddTriangle(Node a, Node b, Node c)
    {
        meshTriangles.Add(a.VertexIndex);
        meshTriangles.Add(b.VertexIndex);
        meshTriangles.Add(c.VertexIndex);

        Triangle meshTriangle = new Triangle(a.VertexIndex, b.VertexIndex, c.VertexIndex);
        AddToVertexTriangles(meshTriangle.VertexA, meshTriangle);
        AddToVertexTriangles(meshTriangle.VertexB, meshTriangle);
        AddToVertexTriangles(meshTriangle.VertexC, meshTriangle);
    }

    private void AddSquare(Node a, Node b, Node c, Node d)
    {
        AddTriangle(a, b, c);
        AddTriangle(a, c, d);
    }

    private void AddPentagon(Node a, Node b, Node c, Node d, Node e)
    {
        AddSquare(a, b, c, d);
        AddTriangle(a, d, e);
    }

    private void AddHexagon(Node a, Node b, Node c, Node d, Node e, Node f)
    {
        AddPentagon(a, b, c, d, e);
        AddTriangle(a, e, f);
    }

    private void SetVertices(Node[] nodes)
    {
        for (int i = 0; i < nodes.Length; i++)
        {
            if (!nodes[i].IsIndexAssigned)
            {
                nodes[i].VertexIndex = meshVertices.Count;
                meshVertices.Add(nodes[i].Position);
            } // end if
        } // end for
    }

    private void AddToVertexTriangles(int vertexKey, Triangle triangle)
    {
        if (vertexTriangleGroup.ContainsKey(vertexKey))
        {
            vertexTriangleGroup[vertexKey].Add(triangle);
        } // end if
        else
        {
            List<Triangle> vertexTriangles = new List<Triangle>();
            vertexTriangles.Add(triangle);
            vertexTriangleGroup.Add(vertexKey, vertexTriangles);
        } // end else
    }

    private void CreateEdges()
    {
        for (int vertex = 0; vertex < meshVertices.Count; vertex++)
        {
            if (!repeatedVertexHelper.Contains(vertex))
            {
                int connectionVertex = GetConnectedEdgeVertex(vertex);
                if (connectionVertex != NoConnectionVertex)
                {
                    repeatedVertexHelper.Add(vertex);
                    List<int> borderConnection = new List<int>();
                    borderConnection.Add(connectionVertex);
                    borderVertexConnections.Add(borderConnection);
                    SetEdgePath(connectionVertex, borderVertexConnections.Count - 1);
                    borderVertexConnections[borderVertexConnections.Count - 1].Add(vertex);
                } // end if
            } // end if
        }
    }

    private void SetEdgePath(int vertex, int borderIndex)
    {
        do
        {
            borderVertexConnections[borderIndex].Add(vertex);
            repeatedVertexHelper.Add(vertex);
            vertex = GetConnectedEdgeVertex(vertex);
        } while (vertex != NoConnectionVertex);
    }


    private int GetConnectedEdgeVertex(int vertex)
    {
        int connectedVertex = NoConnectionVertex;

        List<Triangle> vertexTriangles = vertexTriangleGroup[vertex];

        for (int i = 0; i < vertexTriangles.Count && connectedVertex == NoConnectionVertex; i++)
        {
            Triangle currentTriangle = vertexTriangles[i];
            int currentVertex = currentTriangle.VertexA;

            if (connectedVertex == NoConnectionVertex && vertex != currentVertex && !repeatedVertexHelper.Contains(currentVertex))
            {
                if (IsMeshEdge(vertex, currentVertex))
                    connectedVertex = currentVertex;
            } // end if
            currentVertex = currentTriangle.VertexB;

            if (connectedVertex == NoConnectionVertex && vertex != currentVertex && !repeatedVertexHelper.Contains(currentVertex))
            {
                if (IsMeshEdge(vertex, currentVertex))
                    connectedVertex = currentVertex;
            } // end if
            currentVertex = currentTriangle.VertexC;

            if (connectedVertex == NoConnectionVertex && vertex != currentVertex && !repeatedVertexHelper.Contains(currentVertex))
            {
                if (IsMeshEdge(vertex, currentVertex))
                    connectedVertex = currentVertex;
            } // end if

        } // end for

        return connectedVertex;
    }

    private bool IsMeshEdge(int vertexA, int vertexB)
    {
        const  int sharedTrianglesToBeEdge = 1;
        bool isEdge = false;
        int sharedTrianglesCount = 0;
        List<Triangle> vertexATriangles = vertexTriangleGroup[vertexA];

        for (int i = 0; i < vertexATriangles.Count && sharedTrianglesCount <= sharedTrianglesToBeEdge; i++)
        {
            if (IsVertexContained(vertexB, vertexATriangles[i]))
                sharedTrianglesCount++;
        } // end for

        isEdge = (sharedTrianglesCount == sharedTrianglesToBeEdge);
        return isEdge;
    }

    private bool IsVertexContained(int vertex, Triangle triangle)
    {
        return vertex == triangle.VertexA || vertex == triangle.VertexB || vertex == triangle.VertexC;
    }
}
