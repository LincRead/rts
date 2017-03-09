using UnityEngine;
using System.Collections;
using UnityEngine.EventSystems;

public class InputHoveringUI : MonoBehaviour {

    bool hoveringUI = true;

    void Start () {
	
	}

    void Update()
    {
        ValidateInput();
    }

    void ValidateInput()
    {
        if (Input.GetMouseButtonDown(0))
        {
            if (EventSystem.current.IsPointerOverGameObject())
                hoveringUI = true;
            else
                hoveringUI = false;
        }
    }

    public bool IsHoveringUI()
    {
        return hoveringUI;
    }
}
