using UnityEngine;
using System.Collections;

public class CameraRTS : MonoBehaviour {

    public float scrollSpeed = 1f;
    public GameController gameController;
    public Grid grid;

    Vector2 oldMousePosition = Vector2.zero;

    private float rightBound;
    private float leftBound;
    private float topBound;
    private float bottomBound;

    bool movingCamera = false;

    // Use this for initialization
    void Start() {
        gameController = GameObject.FindGameObjectWithTag("GameController").GetComponent<GameController>();
        grid = GameObject.FindGameObjectWithTag("Grid").GetComponent<Grid>();

        float vertExtent = Camera.main.orthographicSize;
        float horzExtent = vertExtent * Screen.width / Screen.height;

        leftBound = (float)(horzExtent - (grid.GetGridWorldSizeX() / 2));
        rightBound = (float)((grid.GetGridWorldSizeX() / 2) - horzExtent);
        bottomBound = (float)(vertExtent - grid.GetGridWorldSizeY() / 2.0f);
        topBound = (float)(grid.GetGridWorldSizeY() / 2.0f - vertExtent);
    }

    // Update is called once per frame
    void Update() {

        if (Input.GetMouseButtonDown(0))
        {
            oldMousePosition = Input.mousePosition;
        }

        else if (Input.GetMouseButtonUp(0))
        {
            
        }

        else if (Input.GetMouseButton(0) && gameController.IsHoveringUI())
        {
            Vector2 newMousePosition = Input.mousePosition;

            Vector3 newCameraPos = transform.position;
            newCameraPos += transform.TransformDirection((Vector3)((oldMousePosition - newMousePosition) * 1f * Time.deltaTime));
            newCameraPos.x = Mathf.Clamp(newCameraPos.x, leftBound, rightBound);
            newCameraPos.y = Mathf.Clamp(newCameraPos.y, bottomBound, topBound);
            transform.position = newCameraPos;

            if (oldMousePosition != newMousePosition)
                movingCamera = true;

            oldMousePosition = newMousePosition;
        }

        else
        {
            Reset();
        }
    }

    void Reset()
    {
        oldMousePosition = Vector2.zero;
        movingCamera = false;
    }

    public bool IsMoving()
    {
        return movingCamera;
    }
}
