using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class Resizer : MonoBehaviour {
    private Vector3 lastGrabbedLocation;
    private Quaternion lastGrabbedRotation;
    private Vector3 lastScale;
    private Vector3 lastPosition;
    private GameObject controller;

    private bool xPosActive;
    private bool xNegActive;
    private bool yPosActive;
    private bool yNegActive;
    private bool zPosActive;
    private bool zNegActive;

    private GameObject resizeBlock;

    static Vector3 CURRENT_SCALE = new Vector3(0.1f, 0.1f, 0.1f);


    // Update is called once per frame
    void Update() {
        if (controller != null) {

            //scales X
            resizeBlock.transform.localScale = lastScale;
            Vector3 tempScale = resizeBlock.transform.localScale;
            Vector3 vecDistance = lastGrabbedLocation - controller.transform.position;
            float distance = new Vector3(vecDistance.x * resizeBlock.transform.right.x, vecDistance.y * resizeBlock.transform.right.y, vecDistance.z * resizeBlock.transform.right.z).magnitude;
            if (Vector3.Distance(lastGrabbedLocation + resizeBlock.transform.right, controller.transform.position) >
                Vector3.Distance(lastGrabbedLocation - resizeBlock.transform.right, controller.transform.position)) {
                xPosActive = true;
                xNegActive = false;
            } else {
                xPosActive = false;
                xNegActive = true;
            }

            if (xPosActive) {
                Debug.Log("xPos");
                if (lastScale.x - distance >= 0.01f) {
                    resizeBlock.transform.localScale = tempScale + new Vector3(-distance, 0f, 0f);
                } else {
                    resizeBlock.transform.localScale = new Vector3(0.01f, resizeBlock.transform.localScale.y, resizeBlock.transform.localScale.z);
                }
            } else if (xNegActive) {
                Debug.Log("xNeg");
                if (lastScale.x + distance >= 0.01f) {
                    resizeBlock.transform.localScale = tempScale + new Vector3(distance, 0f, 0f);
                } else {
                    resizeBlock.transform.localScale = new Vector3(0.01f, resizeBlock.transform.localScale.y, resizeBlock.transform.localScale.z);
                }
            }

            //scales Y
            tempScale = resizeBlock.transform.localScale;
            distance = new Vector3(vecDistance.x * resizeBlock.transform.up.x, vecDistance.y * resizeBlock.transform.up.y, vecDistance.z * resizeBlock.transform.up.z).magnitude;
            if (Vector3.Distance(lastGrabbedLocation + resizeBlock.transform.up, controller.transform.position) >
                Vector3.Distance(lastGrabbedLocation - resizeBlock.transform.up, controller.transform.position)) {
                yPosActive = true;
                yNegActive = false;
            } else {
                yPosActive = false;
                yNegActive = true;
            }
            if (yPosActive) {
                if (lastScale.y - distance >= 0.01f) {

                    resizeBlock.transform.localScale = tempScale + new Vector3(0f, -distance, 0f);
                } else {
                    resizeBlock.transform.localScale = new Vector3(resizeBlock.transform.localScale.x, 0.01f, resizeBlock.transform.localScale.z);
                }

            } else if (yNegActive) {
                if (lastScale.y + distance >= 0.01f) {
                    resizeBlock.transform.localScale = tempScale + new Vector3(0f, distance, 0f);
                } else {
                    resizeBlock.transform.localScale = new Vector3(resizeBlock.transform.localScale.x, 0.01f, resizeBlock.transform.localScale.z);
                }
            }

            //scales Z
            tempScale = resizeBlock.transform.localScale;
            distance = new Vector3(vecDistance.x * resizeBlock.transform.forward.x, vecDistance.y * resizeBlock.transform.forward.y, vecDistance.z * resizeBlock.transform.forward.z).magnitude;
            if (Vector3.Distance(lastGrabbedLocation + resizeBlock.transform.forward, controller.transform.position) >
                Vector3.Distance(lastGrabbedLocation - resizeBlock.transform.forward, controller.transform.position)) {
                zPosActive = true;
                zNegActive = false;
            } else {
                zPosActive = false;
                zNegActive = true;
            }
            if (zPosActive) {
                if (lastScale.z - distance >= 0.01f) {

                    resizeBlock.transform.localScale = tempScale + new Vector3(0f, 0f, -distance);
                } else {
                    resizeBlock.transform.localScale = new Vector3(resizeBlock.transform.localScale.x, resizeBlock.transform.localScale.y, 0.01f);
                }

            } else if (zNegActive) {
                if (lastScale.z + distance >= 0.01f) {

                    resizeBlock.transform.localScale = tempScale + new Vector3(0f, 0f, distance);
                } else {
                    resizeBlock.transform.localScale = new Vector3(resizeBlock.transform.localScale.x, resizeBlock.transform.localScale.y, 0.01f);
                }
            }


        }
    }

    public void SendGrabCoord(Transform coords, GameObject controller, GameObject resizeBlocky) {
        resizeBlock = resizeBlocky;
        lastGrabbedLocation = coords.position;
        lastGrabbedRotation = coords.rotation;
        lastScale = resizeBlock.transform.lossyScale;
        lastPosition = resizeBlock.GetComponent<BoxCollider>().center;
        this.controller = controller;
        Debug.Log("lastGrabbedLoc: " + lastGrabbedLocation + " thisObjectPos: " + transform.position);
        if (Vector3.Distance(lastGrabbedLocation, lastPosition + resizeBlock.transform.right) > Vector3.Distance(lastGrabbedLocation, lastPosition - resizeBlock.transform.right)) {
            xPosActive = true;
        } else {
            xNegActive = true;
        }

        if (Vector3.Distance(lastGrabbedLocation, lastPosition + resizeBlock.transform.up) > Vector3.Distance(lastGrabbedLocation, lastPosition - resizeBlock.transform.up)) {
            yPosActive = true;
        } else {
            yNegActive = true;
        }

        if (Vector3.Distance(lastGrabbedLocation, lastPosition + resizeBlock.transform.forward) > Vector3.Distance(lastGrabbedLocation, lastPosition - resizeBlock.transform.forward)) {
            zPosActive = true;
        } else {
            zNegActive = true;
        }
    }

    public void resizeToolAction(Transform coords, GameObject controller, GameObject resizeBlock) {
        if (this.controller) {
            RemoveController();
        } else {
            SendGrabCoord(coords, controller, resizeBlock);
        }
    }

    public bool IsResizing() {
        if (controller) {
            return true;
        }
        return false;
    }

    public void RemoveController() {
        xPosActive = false;
        xNegActive = false;
        yPosActive = false;
        yNegActive = false;
        zPosActive = false;
        zNegActive = false;
        this.controller = null;
        CURRENT_SCALE = resizeBlock.transform.localScale;
    }
}
