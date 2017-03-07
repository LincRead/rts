using UnityEngine;
using System.Collections;
using UnityEngine.UI;

public class ShowGold : MonoBehaviour {

    Text text;
    GameController gameController;

	// Use this for initialization
	void Start () {
        text = GetComponent<Text>();
        gameController = GetComponentInParent<GameController>();
        Debug.Log(gameController);
    }
	
	// Update is called once per frame
	void Update () {
        text.text = gameController.GetSquadLocalPlayer().GetGold().ToString();
    }
}
