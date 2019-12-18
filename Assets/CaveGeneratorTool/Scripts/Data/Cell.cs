
public class Cell
{
    private const int BottomLeftActiveValue = 1;
    private const int BottomRightActiveValue = 2;
    private const int TopRightActiveValue = 4;
    private const int TopLeftActiveValue = 8;

    private CornerNode topLeft, topRight, bottomLeft, bottomRight;
    private Node midLeft, midTop, midRight, midBottom;
    private int configValue;

    public CornerNode TopLeft => topLeft;
    public CornerNode TopRight => topRight;
    public CornerNode BottomLeft => bottomLeft;
    public CornerNode BottomRight => bottomRight;
    public Node MidLeft => midLeft;
    public Node MidTop => midTop;
    public Node MidRight => midRight;
    public Node MidBottom => midBottom;
    public int ConfigurationValue => configValue;

    public Cell(CornerNode topLeft, CornerNode topRight, CornerNode bottomLeft, CornerNode bottomRight)
    {
        this.topLeft = topLeft;
        this.topRight = topRight;
        this.bottomLeft = bottomLeft;
        this.bottomRight = bottomRight;

        midLeft = this.bottomLeft.Vertical;
        midTop = this.topLeft.Horizontal;
        midRight = this.bottomRight.Vertical;
        midBottom = this.bottomLeft.Horizontal;

        if (bottomLeft.IsOn)
            configValue += BottomLeftActiveValue;
        if (bottomRight.IsOn)
            configValue += BottomRightActiveValue;
        if (topRight.IsOn)
            configValue += TopRightActiveValue;
        if (TopLeft.IsOn)
            configValue += TopLeftActiveValue;
    }

}
