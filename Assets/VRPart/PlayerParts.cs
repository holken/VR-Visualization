using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class PlayerParts : MonoBehaviour {
    public GameObject body;
    public GameObject head;
    public GameObject leftHand;
    public GameObject rightHand;
    public GameObject[] dataScreenTexts;
    public GameObject dataBoard;
    public GameObject dataPoints;

    public static PlayerParts Instance;

	void Awake() {
        if (Instance == null) {
            Instance = this;
        }
    }

    void OnDestroy() {
        if (Instance = this) {
            Instance = null;
        }
    }
	
	// Update is called once per frame
	void Update () {
		
	}

    public void ScaleWithPlayer() {
        //TODO should not teleport the screen when shrinking, should try to calculate its current pos
        dataBoard.transform.localScale = body.transform.localScale;
        dataBoard.transform.position = new Vector3(body.transform.position.x  , 
            body.transform.position.y + (Vector3.up.y * body.transform.localScale.y)
            , body.transform.position.z - (Vector3.forward.z * body.transform.localScale.x));
    }
}
