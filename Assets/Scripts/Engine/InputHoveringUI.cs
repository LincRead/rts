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
        // Suspect failure on Android
        if (Input.GetMouseButtonDown(0))
        {
            if (EventSystem.current.IsPointerOverGameObject())
                hoveringUI = true;
            else
                hoveringUI = false;
        }

        hoveringUI = false;
    }

    public bool IsHoveringUI()
    {
        return hoveringUI;
    }
}
