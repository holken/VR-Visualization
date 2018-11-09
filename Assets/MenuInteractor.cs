using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class MenuInteractor : MonoBehaviour {
    private SteamVR_TrackedObject trackedObj;

    private SteamVR_Controller.Device Controller {
        get { return SteamVR_Controller.Input((int)trackedObj.index); }

    }

    // Use this for initialization
    void Start () {
		
	}
	
	// Update is called once per frame
	void Update () {
        RaycastHit[] hits;
        hits = Physics.RaycastAll(transform.position, transform.forward, 1.0F);

        for (int i = 0; i < hits.Length; i++) {
            RaycastHit hit = hits[i];
            GameObject hitObj = hit.collider.gameObject;
            //Checks if we hit a button
            if (hitObj.GetComponent<UIButton>()) {
                hitObj.GetComponent<UIButton>().HighLightButton();
                if (Controller.GetHairTriggerDown()) {

                    hitObj.GetComponent<UIButton>().SelectButton();
                }

            }
            if (hitObj.GetComponent<CursorTarget>()) {
                hitObj.GetComponent<CursorTarget>().UpdateLocation(hit.point);
            }


        }
    }
}
