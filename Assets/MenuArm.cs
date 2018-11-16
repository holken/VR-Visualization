using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class MenuArm : MonoBehaviour {
    public GameObject menu;
    private GameObject currMenu;
	// Use this for initialization
	void Start () {
        currMenu = menu;
        currMenu.transform.position = transform.position + transform.forward * 0.1f * transform.localScale.x + transform.up * 0.1f * transform.localScale.x;
        currMenu.transform.rotation = transform.rotation;
        currMenu.transform.Rotate(60f, 0f, 0f);
        currMenu.transform.parent = transform;
	}
	
}
