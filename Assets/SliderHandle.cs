using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;

public class SliderHandle : MonoBehaviour {
    public GameObject filler;
    public GameObject superMenu;
    private float fillLength;
    private Vector3 startPos;
	// Use this for initialization
	void Start () {
        startPos = GetComponent<RectTransform>().anchoredPosition;
        fillLength = filler.GetComponent<RectTransform>().rect.width;
        Debug.Log("fillLength: " + fillLength);
        Debug.Log("startpos: " + startPos.x);
    }
	

    public void UpdateSliderLocation(Vector3 hitPoint) {

        //TODO, remake and put another collider on the whole slider and get position from there and update the image from there
        //TODO, if that doesn't work, make the user press
        Vector3 globalStartPos = superMenu.transform.position + startPos;
        Vector3 rightVector = transform.right;
        Vector3 vectorBetweenHitAndStart = globalStartPos - hitPoint;
        Vector3 xPositionHitPoint = new Vector3 (vectorBetweenHitAndStart.x*rightVector.x, vectorBetweenHitAndStart.y * rightVector.y, vectorBetweenHitAndStart.z * rightVector.z);
        Vector3 vectorBetween = globalStartPos - new Vector3(globalStartPos.x + fillLength, globalStartPos.y + globalStartPos.z) ;
        if (xPositionHitPoint.x < 0f && xPositionHitPoint.x < 0.1f) {
            float distance = xPositionHitPoint.magnitude;
            Debug.Log("distance; " + distance);
            //float distance = vectorBetween.magnitude;
            //TODO might do this in a clamp instead
            if (distance > fillLength) {
                distance = fillLength - GetComponent<RectTransform>().rect.width;
            }
            //TODO testa recttrans istället för trans
            transform.GetComponent<RectTransform>().anchoredPosition = startPos + new Vector3(distance, 0f, 0f);
            filler.GetComponent<Image>().fillAmount = distance * 10f; //TODO göra 10f dynamiskt?
        }
       
    }
}
