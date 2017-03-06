using UnityEngine;
using System.Collections;

public class RTSCamera : MonoBehaviour {

    public float scrollSpeed = 1f;

    Vector2 oldMousePosition = Vector2.zero;

	// Use this for initialization
	void Start () {
	    
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

            transform.position += transform.TransformDirection((Vector3)((oldMousePosition - newMousePosition) * 1f * Time.deltaTime));

            oldMousePosition = newMousePosition;
        }

        else
        {
            oldMousePosition = Vector2.zero;
        }
	}
}
