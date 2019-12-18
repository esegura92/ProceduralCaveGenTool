using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;

public class UIMinimapRenderer : MonoBehaviour
{ 
    [SerializeField]
    private RawImage miniMapContainer;
    [SerializeField]
    private Color wallColor = Color.black;
    [SerializeField]
    private Color groundColor = Color.grey;
    [SerializeField]
    private Color playerColor = Color.green;
    [SerializeField]
    private Color collectableColor = Color.cyan;
    // Start is called before the first frame update
    public void SetMiniMap(int[,] pixels)
    {
        
        Texture2D miniMapTexture = CaveGeneratorUtilities.TextureUtilities.CreateMapTexture(pixels, wallColor, groundColor, collectableColor, playerColor, 
                                                                                            (int)miniMapContainer.gameObject.GetComponent<RectTransform>().sizeDelta.x, 
                                                                                            (int)miniMapContainer.gameObject.GetComponent<RectTransform>().sizeDelta.y);
        miniMapContainer.texture = miniMapTexture;
    }

}
