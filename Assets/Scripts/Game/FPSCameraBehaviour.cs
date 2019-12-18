using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class FPSCameraBehaviour : MonoBehaviour
{
    [SerializeField]
    private float sensitivity = 5.0f;
    [SerializeField]
    private float smoothing = 2.0f;
    [SerializeField]
    private GameObject fpCharacter;
    private Vector2 mouseView;
    private Vector2 smoothV;

    private const string MouseXAxis = "Mouse X";
    private const string MouseYAxis = "Mouse Y";

    // Use this for initialization
    void Awake()
    {
        fpCharacter = this.transform.parent.gameObject;
    }

    // Update is called once per frame
    void Update()
    {
        // md is mosue delta
        Vector2 mouseDirection = new Vector2(Input.GetAxisRaw(MouseXAxis), Input.GetAxisRaw(MouseYAxis));
        mouseDirection = Vector2.Scale(mouseDirection, new Vector2(sensitivity * smoothing, sensitivity * smoothing));
        smoothV.x = Mathf.Lerp(smoothV.x, mouseDirection.x, 1f / smoothing);
        smoothV.y = Mathf.Lerp(smoothV.y, mouseDirection.y, 1f / smoothing);
        mouseView += smoothV;

        transform.localRotation = Quaternion.AngleAxis(-mouseView.y, Vector3.right);
        fpCharacter.transform.localRotation = Quaternion.AngleAxis(mouseView.x, fpCharacter.transform.up);
    }
}
