using UnityEngine;
using System.Collections;
using UnityEngine.EventSystems;

public class ValidInput : MonoBehaviour {

    bool validInput = true;

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
                validInput = false;
            else
                validInput = true;
        }
    }

    public bool GetValidInput()
    {
        return validInput;
    }
}
