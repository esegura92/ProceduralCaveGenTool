using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class FPSCharacterController : MonoBehaviour
{
    [SerializeField]
    private float speed = 5.0f;
    private float zMove;
    private float xMove;

    private const string VerticalAxis = "Vertical";
    private const string HorizontalAxis = "Horizontal";

    // Update is called once per frame
    void Update()
    {
        zMove = Input.GetAxis(VerticalAxis) * speed * Time.deltaTime;
        xMove = Input.GetAxis(HorizontalAxis) * speed * Time.deltaTime;
        transform.Translate(xMove, 0, zMove);
    }
}
