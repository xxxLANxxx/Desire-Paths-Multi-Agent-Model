using UnityEngine;
using System;
using System.Collections.Generic;
using UnityEngine.EventSystems;
using System.Net;

/// <summary> A modular and easily customisable Unity MonoBehaviour for handling swipe and pinch motions on mobile. </summary>
public class PanZoomAndRotate : MonoBehaviour
{

    /// <summary> Called as soon as the player touches the screen. The argument is the screen position. </summary>
    public event Action<Vector2> onStartTouch;
    /// <summary> Called as soon as the player stops touching the screen. The argument is the screen position. </summary>
    public event Action<Vector2> onEndTouch;
    /// <summary> Called if the player completed a quick tap motion. The argument is the screen position. </summary>
    public event Action<Vector2> onTap;
    /// <summary> Called if the player swiped the screen. The argument is the screen movement delta. </summary>
    public event Action<Vector2> onSwipe;
    /// <summary> Called if the player pinched the screen. The arguments are the distance between the fingers before and after. </summary>
    public event Action<float, float> onPinch;

    public GameObject target = null;

    [Header("Tap")]
    [Tooltip("The maximum movement for a touch motion to be treated as a tap")]
    public float maxDistanceForTap = 40;
    [Tooltip("The maximum duration for a touch motion to be treated as a tap")]
    public float maxDurationForTap = 0.4f;

    [Header("Desktop debug")]
    [Tooltip("Use the mouse on desktop?")]
    public bool useMouse = true;
    [Tooltip("The simulated pinch speed using the scroll wheel")]
    public float mouseScrollSpeed = 2f;

    [Header("Camera control")]
    [Tooltip("Does the script control camera movement?")]
    public bool controlCamera = true;
    [Tooltip("The controlled camera, ignored of controlCamera=false")]
    public Camera cam;
    public float rotationSpeed = 100.0f;

    [Header("UI")]
    [Tooltip("Are touch motions listened to if they are over UI elements?")]
    public bool ignoreUI = false;

    [Header("Bounds")]
    [Tooltip("Is the camera bound to an area?")]
    public bool useBounds;

    public float boundMinX = -150;
    public float boundMaxX = 150;
    public float boundMinY = -150;
    public float boundMaxY = 150;

    private Vector3 previousPosition;
    private Vector3 camDistance;

    public Vector3 camPosition2DMode;
    public Vector3 camEulerRotation2DMode;
    public Vector3 camPosition3DMode;
    public Vector3 camEulerRotation3DMode;
    public float transitionDuration = 1.0f;

    private float transitionStartTime;
    private bool is2DMode = true;
    private bool isTransitioning = false;

    private Vector3 startPointPosition;
    private Quaternion startPointRotation;
    private Vector3 endPointPosition;
    private Quaternion endPointRotation;

    Vector2 touch0StartPosition;
    Vector2 touch0LastPosition;
    float touch0StartTime;

    bool cameraControlEnabled = true;

    bool canUseMouse;

    /// <summary> Has the player at least one finger on the screen? </summary>
    public bool isTouching { get; private set; }

    /// <summary> The point of contact if it exists in Screen space. </summary>
    public Vector2 touchPosition { get { return touch0LastPosition; } }

    void Start()
    {
        canUseMouse = Application.platform != RuntimePlatform.Android && Application.platform != RuntimePlatform.IPhonePlayer && Input.mousePresent;

        camDistance = target.transform.position - cam.transform.position;
    }

    void Update()
    {

        if (IsMouseOverGameWindow())
        {

            cameraControlEnabled = true;
        }
        else
        {
            cameraControlEnabled = false;
        }


        if (useMouse && canUseMouse)
        {
            UpdateWithMouse();
        }
        else
        {
            UpdateWithTouch();
        }

        if(Input.GetKeyDown(KeyCode.V) && !isTransitioning)
        {
            // Start the transition
            isTransitioning = true;
            transitionStartTime = Time.time;

            // Set the start and end points based on the current position
            if (is2DMode)
            {
                startPointPosition = camPosition2DMode;
                startPointRotation = Quaternion.Euler(camEulerRotation2DMode);
                endPointPosition = camPosition3DMode;
                endPointRotation = Quaternion.Euler(camEulerRotation3DMode);
            }
            else
            {
                startPointPosition = camPosition3DMode;
                startPointRotation = Quaternion.Euler(camEulerRotation3DMode);
                endPointPosition = camPosition2DMode;
                endPointRotation = Quaternion.Euler(camEulerRotation2DMode);
            }

            // Toggle the current point flag
            is2DMode = !is2DMode;

            Debug.Log(is2DMode);

            // Perform the transition if in progress
            if (isTransitioning)
            {
                // Calculate the elapsed time since the start of the transition
                float elapsedTime = Time.time - transitionStartTime;

                // Calculate the fraction of completion of the transition
                float t = elapsedTime / transitionDuration;

                // Smoothly interpolate the camera's position between startPoint and endPoint
                cam.transform.position = Vector3.Lerp(startPointPosition, endPointPosition, t);

                // Smoothly interpolate the camera's rotation between startPoint and endPoint
                cam.transform.rotation = Quaternion.Slerp(startPointRotation, endPointRotation, t);

                // Stop the transition when it reaches the end point
                if (t >= 1.0f)
                {
                    // Ensure the camera exactly matches the end position and rotation
                    cam.transform.position = endPointPosition;
                    cam.transform.rotation = endPointRotation;

                    // Stop the transition
                    isTransitioning = false;
                }

                Debug.Log(isTransitioning);
            }



            //if(!is3DMode)
            //{
            //    AnimateCameraForChangingViewMode(camTransform2DMode, camTransform3DMode);
            //    is3DMode = true;
            //}
            //else
            //{
            //    AnimateCameraForChangingViewMode(camTransform3DMode, camTransform2DMode);
            //    is3DMode = false;
            //}
        }


    }

    void LateUpdate()
    {
        CameraInBounds();
    }

    void UpdateWithMouse()
    {
        if (Input.GetMouseButtonDown(1))
        {
            if (ignoreUI || !IsPointerOverUIObject())
            {
                touch0StartPosition = Input.mousePosition;
                touch0StartTime = Time.time;
                touch0LastPosition = touch0StartPosition;

                isTouching = true;

                if (onStartTouch != null) onStartTouch(Input.mousePosition);
            }
        }

        if (Input.GetMouseButton(1) && isTouching)
        {
            Vector2 move = (Vector2)Input.mousePosition - touch0LastPosition;
            touch0LastPosition = Input.mousePosition;

            if (move != Vector2.zero)
            {
                OnSwipe(move);
            }
        }

        if (Input.GetMouseButtonUp(1) && isTouching)
        {

            if (Time.time - touch0StartTime <= maxDurationForTap
               && Vector2.Distance(Input.mousePosition, touch0StartPosition) <= maxDistanceForTap)
            {
                OnClick(Input.mousePosition);
            }

            if (onEndTouch != null) onEndTouch(Input.mousePosition);
            isTouching = false;
            cameraControlEnabled = true;
        }

        if (Input.mouseScrollDelta.y != 0)
        {
            OnPinch(Input.mousePosition, 1, Input.mouseScrollDelta.y < 0 ? (1 / mouseScrollSpeed) : mouseScrollSpeed, Vector2.right);
        }

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
            cam.transform.Rotate(new Vector3(1, 0, 0), -direction.y * rotationSpeed);
            cam.transform.Rotate(new Vector3(0, 0, 1), direction.x * rotationSpeed, Space.World);
            cam.transform.Translate(new Vector3(0f, 0f, -camDistance.z));

            previousPosition = cam.ScreenToViewportPoint(Input.mousePosition);
        }

    }

    void UpdateWithTouch()
    {
        int touchCount = Input.touches.Length;

        if (touchCount == 1)
        {
            Touch touch = Input.touches[0];

            switch (touch.phase)
            {
                case TouchPhase.Began:
                    {
                        if (ignoreUI || !IsPointerOverUIObject())
                        {
                            touch0StartPosition = touch.position;
                            touch0StartTime = Time.time;
                            touch0LastPosition = touch0StartPosition;

                            isTouching = true;

                            if (onStartTouch != null) onStartTouch(touch0StartPosition);
                        }

                        break;
                    }
                case TouchPhase.Moved:
                    {
                        touch0LastPosition = touch.position;

                        if (touch.deltaPosition != Vector2.zero && isTouching)
                        {
                            OnSwipe(touch.deltaPosition);
                        }
                        break;
                    }
                case TouchPhase.Ended:
                    {
                        if (Time.time - touch0StartTime <= maxDurationForTap
                            && Vector2.Distance(touch.position, touch0StartPosition) <= maxDistanceForTap
                            && isTouching)
                        {
                            OnClick(touch.position);
                        }

                        if (onEndTouch != null) onEndTouch(touch.position);
                        isTouching = false;
                        cameraControlEnabled = true;
                        break;
                    }
                case TouchPhase.Stationary:
                case TouchPhase.Canceled:
                    break;
            }
        }
        else if (touchCount == 2)
        {
            Touch touch0 = Input.touches[0];
            Touch touch1 = Input.touches[1];

            if (touch0.phase == TouchPhase.Ended || touch1.phase == TouchPhase.Ended) return;

            isTouching = true;

            float previousDistance = Vector2.Distance(touch0.position - touch0.deltaPosition, touch1.position - touch1.deltaPosition);

            float currentDistance = Vector2.Distance(touch0.position, touch1.position);

            if (previousDistance != currentDistance)
            {
                OnPinch((touch0.position + touch1.position) / 2, previousDistance, currentDistance, (touch1.position - touch0.position).normalized);
            }
        }
        else
        {
            if (isTouching)
            {
                if (onEndTouch != null) onEndTouch(touch0LastPosition);
                isTouching = false;
            }

            cameraControlEnabled = true;
        }
    }

    void OnClick(Vector2 position)
    {
        if (onTap != null && (ignoreUI || !IsPointerOverUIObject()))
        {
            onTap(position);
        }
    }
    void OnSwipe(Vector2 deltaPosition)
    {
        if (onSwipe != null)
        {
            onSwipe(deltaPosition);
        }

        if (controlCamera && cameraControlEnabled)
        {
            if (cam == null) cam = Camera.main;

            cam.transform.position -= (cam.ScreenToWorldPoint(deltaPosition) - cam.ScreenToWorldPoint(Vector2.zero));
        }
    }
    void OnPinch(Vector2 center, float oldDistance, float newDistance, Vector2 touchDelta)
    {
        if (onPinch != null)
        {
            onPinch(oldDistance, newDistance);
        }

        if (controlCamera && cameraControlEnabled)
        {
            if (cam == null) cam = Camera.main;

            if (cam.orthographic)
            {
                var currentPinchPosition = cam.ScreenToWorldPoint(center);

                cam.orthographicSize = Mathf.Max(0.1f, cam.orthographicSize * oldDistance / newDistance);

                var newPinchPosition = cam.ScreenToWorldPoint(center);

                cam.transform.position -= newPinchPosition - currentPinchPosition;
            }
            else
            {
                cam.fieldOfView = Mathf.Clamp(cam.fieldOfView * oldDistance / newDistance, 0.1f, 179.9f);
            }
        }
    }

    /// <summary> Checks if the the current input is over canvas UI </summary>
    public bool IsPointerOverUIObject()
    {

        if (EventSystem.current == null) return false;
        PointerEventData eventDataCurrentPosition = new PointerEventData(EventSystem.current);
        eventDataCurrentPosition.position = new Vector2(Input.mousePosition.x, Input.mousePosition.y);
        List<RaycastResult> results = new List<RaycastResult>();
        EventSystem.current.RaycastAll(eventDataCurrentPosition, results);
        return results.Count > 0;
    }

    /// <summary> Cancels camera movement for the current motion. Resets to use camera at the end of the touch motion.</summary>
    public void CancelCamera()
    {
        cameraControlEnabled = false;
    }

    void CameraInBounds()
    {
        if (controlCamera && useBounds && cam != null && cam.orthographic)
        {
            cam.orthographicSize = Mathf.Min(cam.orthographicSize, ((boundMaxY - boundMinY) / 2) - 0.001f);
            cam.orthographicSize = Mathf.Min(cam.orthographicSize, (Screen.height * (boundMaxX - boundMinX) / (2 * Screen.width)) - 0.001f);

            Vector2 margin = cam.ScreenToWorldPoint((Vector2.up * Screen.height / 2) + (Vector2.right * Screen.width / 2)) - cam.ScreenToWorldPoint(Vector2.zero);

            float marginX = margin.x;
            float marginY = margin.y;

            float camMaxX = boundMaxX - marginX;
            float camMaxY = boundMaxY - marginY;
            float camMinX = boundMinX + marginX;
            float camMinY = boundMinY + marginY;

            float camX = Mathf.Clamp(cam.transform.position.x, camMinX, camMaxX);
            float camY = Mathf.Clamp(cam.transform.position.y, camMinY, camMaxY);

            cam.transform.position = new Vector3(camX, camY, cam.transform.position.z);
        }
    }

    bool IsMouseOverGameWindow()
    {
        Vector2 mousePosition = Input.mousePosition;
        return mousePosition.x >= 0 && mousePosition.x <= Screen.width &&
               mousePosition.y >= 0 && mousePosition.y <= Screen.height;
    }
}
