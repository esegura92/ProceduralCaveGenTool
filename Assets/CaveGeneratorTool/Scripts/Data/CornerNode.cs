using UnityEngine;

public class CornerNode : Node
{
    private Node horizontal;
    private Node vertical;
    private bool isOn;

    public Node Horizontal => horizontal;
    public Node Vertical => vertical;
    public bool IsOn => isOn;

    public CornerNode(Vector3 position, bool isOn, float cellSize) : base(position)
    {
        horizontal = new Node(Position + Vector3.right * cellSize / 2f);
        vertical = new Node(Position + Vector3.forward * cellSize / 2f);
        this.isOn = isOn;
    }
}
