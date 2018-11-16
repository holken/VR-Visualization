using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class Settings : MonoBehaviour {
    public static Settings Instance;
    public bool dimStars = false;
    public int currGradientIndex = 0;


    void Awake () {
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
