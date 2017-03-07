using UnityEngine;
using System.Collections;

public class RTSCamera : MonoBehaviour {

    public float scrollSpeed = 1f;
    public Grid grid;

    Vector2 oldMousePosition = Vector2.zero;

    private float rightBound;
    private float leftBound;
    private float topBound;
    private float bottomBound;

    // Use this for initialization
    void Start () {
        grid = GameObject.FindGameObjectWithTag("Grid").GetComponent<Grid>();

        float vertExtent = Camera.main.orthographicSize;
        float horzExtent = vertExtent * Screen.width / Screen.height;

        leftBound = (float)(horzExtent - (grid.GetGridWorldSizeX() / 2));
        rightBound = (float)((grid.GetGridWorldSizeX() / 2) - horzExtent);
        bottomBound = (float)(vertExtent - grid.GetGridWorldSizeY() / 2.0f);
        topBound = (float)(grid.GetGridWorldSizeY() / 2.0f - vertExtent);

        Debug.Log(leftBound);
    }
	
	// Update is called once per frame
	void Update () {

	    if(Input.GetMouseButtonDown(0))
        {
            oldMousePosition = Input.mousePosition;
        }

        else if(Input.GetMouseButtonUp(0))
        {
            oldMousePosition = Vector2.zero;
        }

        else if(Input.GetMouseButton(0))
        {
            Vector2 newMousePosition = Input.mousePosition;

            Vector3 newCameraPos = transform.position;
            newCameraPos += transform.TransformDirection((Vector3)((oldMousePosition - newMousePosition) * 1f * Time.deltaTime));
            newCameraPos.x = Mathf.Clamp(newCameraPos.x, leftBound, rightBound);
            newCameraPos.y = Mathf.Clamp(newCameraPos.y, bottomBound, topBound);
            transform.position = newCameraPos;

            oldMousePosition = newMousePosition;
        }

        else
        {
            oldMousePosition = Vector2.zero;
        }
	}
}
