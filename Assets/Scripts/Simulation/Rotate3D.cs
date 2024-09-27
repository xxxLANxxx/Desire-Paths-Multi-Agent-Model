using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using static UnityEngine.GraphicsBuffer;

public class Rotate3D : MonoBehaviour
{

    public Camera cam = null;
    public GameObject target = null;
    private Vector3 previousPosition;
    private Vector3 camDistance;

    private void Start()
    {
        camDistance = target.transform.position - cam.transform.position;
    }


    private void Update()
    {
        //mouse down starts the drag
        if (Input.GetMouseButtonDown(0))
        {
            previousPosition = cam.ScreenToViewportPoint(Input.mousePosition);
            
        }

        //drag mouse to perform rotations of camera around target object
        if (Input.GetMouseButton(0))
        {
            Vector3 direction = previousPosition - cam.ScreenToViewportPoint(Input.mousePosition);

            //need to use space.world to keep y axis straight
            cam.transform.position = target.transform.position;
            cam.transform.Rotate(new Vector3(1, 0, 0), direction.y * 180f);
            cam.transform.Rotate(new Vector3(0, 0, 1), direction.x * 180f, Space.World);
            cam.transform.Translate(new Vector3(0f, 0f, -camDistance.z));

            previousPosition = cam.ScreenToViewportPoint(Input.mousePosition);
        }
    }
}
