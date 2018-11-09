using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class TestTrig : MonoBehaviour {

	// Use this for initialization
	void Start () {
		
	}
	
	// Update is called once per frame
	void Update () {
		
	}

    void OnTriggerEnter(Collider col) {
        Debug.Log("lol");
    }

    void OnTriggerStay(Collider col) {
        //Debug.Log("lolstay");
    }
}
