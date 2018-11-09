using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class SliderHandler : MonoBehaviour {
    private Vector3 startPos;
    // Use this for initialization
    void Start () {
        startPos = GetComponent<RectTransform>().anchoredPosition;
    }


    void UpdateSliderLocation(Vector3 hitPoint) {

    }
}
