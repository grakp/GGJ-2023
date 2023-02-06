using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class CameraController : MonoBehaviour
{
    
    [SerializeField] private int edgeScrollSize = 30;
    [SerializeField] private float smoothTime = 0.25f;

    private Vector3 offset = new Vector3(0f, 0f, -10f);
    private Vector3 velocity = new Vector3(0, 0, 0);

    private Transform target;

    public float minCameraLocationX = -43;
    public float maxCameraLocationX = 43;

    public float minCameraLocationY = -43;
    public float maxCameraLocationY = 43;

    // Mostly for debugging purposes
    public bool pauseScrolling = false;

    void Start()
    {
        PlayerController player = GameManager.Instance.gameController.GetMyPlayer();
        if (player == null)
        {
            StartCoroutine(WaitForPlayer());
        }
        else
        {
            Initialize();
        }
    }

    void Initialize()
    {
        target = GameManager.Instance.gameController.GetMyPlayer().transform;
        transform.position = new Vector3(target.transform.position.x, target.transform.position.y, transform.position.z);
    }

    private IEnumerator WaitForPlayer()
    {
        while (GameManager.Instance.gameController.GetMyPlayer() == null)
        {
            yield return null;
        }

        Initialize();
    }

    // Update is called once per frame
    void Update()
    {
        if (Input.GetKeyDown(KeyCode.Alpha0))
        {
            pauseScrolling = !pauseScrolling;
        }

        if (!pauseScrolling)
        {
            if (IsMouseInBounds())
            {
                FollowTarget();
            }
            else
            {
                EdgeScrolling();
            }
        }
    }

    // Helpers

    // Camera pushes in the direction the mouse is in
    void EdgeScrolling() 
    {
        if (target == null)
        {
            return;
        }

        if (Input.mousePosition.x < edgeScrollSize) // Left edge
        {
            velocity.x = -10f;
        }
        else if (Input.mousePosition.x > Screen.width - edgeScrollSize) // Right edge
        {
            velocity.x = 10f;
        }
        if (Input.mousePosition.y < edgeScrollSize) // Bottom edge
        {
            velocity.y = -10f;
        }
        else if (Input.mousePosition.y > Screen.height - edgeScrollSize) // Top edge
        {
            velocity.y = 10f;
        }
        Vector3 targetPosition = target.position + offset;
        Vector3 movedPosition = Vector3.SmoothDamp(transform.position, targetPosition, ref velocity, smoothTime);

        movedPosition.x = Mathf.Clamp(movedPosition.x, minCameraLocationX, maxCameraLocationX);
        movedPosition.y = Mathf.Clamp(movedPosition.y, minCameraLocationY, maxCameraLocationY);

        transform.position = movedPosition;
    }

    // Camera follows the target
    void FollowTarget()
    {
        if (target == null)
        {
            return;
        }
        
        Vector3 targetPosition = target.position + offset;

        Vector3 movedPosition = Vector3.SmoothDamp(transform.position, targetPosition, ref velocity, smoothTime);
        movedPosition.x = Mathf.Clamp(movedPosition.x, minCameraLocationX, maxCameraLocationY);
        movedPosition.y = Mathf.Clamp(movedPosition.y, minCameraLocationY, maxCameraLocationY);

        transform.position = movedPosition;
    }

    // Checks if the mouse is within the area in which the camera follows the target
    bool IsMouseInBounds()
    {
        return Input.mousePosition.x > edgeScrollSize && 
               Input.mousePosition.x < Screen.width - edgeScrollSize &&
               Input.mousePosition.y > edgeScrollSize && 
               Input.mousePosition.y < Screen.height - edgeScrollSize;
    }

}
