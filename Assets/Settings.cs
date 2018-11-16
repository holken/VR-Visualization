using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class Settings : MonoBehaviour {
    public static Settings Instance;
    public bool dimStars = false;
	

	void Start () {
		if (Instance == null)
        {
            Instance = this;
        } else
        {
            Destroy(this);
        }
	}
	
	void Update () {
		
	}
}
