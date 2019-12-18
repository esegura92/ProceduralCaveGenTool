using UnityEngine;

public class Node
{
    
    private Vector3 position;
    private int vertexIndex;
    private bool isIndexAssigned;

    

    public Vector3 Position => position;

    public int VertexIndex
    {
        set
        {
            vertexIndex = value;
            isIndexAssigned = true;
        }
        get => vertexIndex;
    }

    public bool IsIndexAssigned => isIndexAssigned;

    public Node(Vector3 position)
    {
        this.position = position;
        isIndexAssigned = false;
    }

}
