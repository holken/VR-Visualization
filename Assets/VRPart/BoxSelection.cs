using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class BoxSelection : MonoBehaviour {

    private Vector3 firstPosition;
    private Vector3 secondPosition;
    private bool firstPointSet;
    private bool secondPointSet;
    private bool saveObjects;

    private Vector3 sizes;
    private Vector3 centerPosition;
    private GameObject selectionBox;
    public Material selectionBoxMaterial;

 

    public void DestroyCurrentSelection() {
        destroyBox();
        firstPointSet = false;
        secondPointSet = false;
    }

    public void SetSaveObjects(bool save) {
        Debug.Log("Setting save Objects to: " + save);
        saveObjects = save;
    }

    public bool GetSaveObjects() {
        return saveObjects;
    }

    public void ToggleSaveObjects() {
        if (!saveObjects) {
            saveObjects = true;
        } else {
            saveObjects = false;
        }
    }

    public void UpdateBoxSelection(Vector3 controllerPosition) {
        if (firstPointSet) {
            setSecondPosition(controllerPosition);
            drawBox();
        }
        
    }

    public GameObject BoxSelectionAction(Vector3 controllerTransform) {
        if (!firstPointSet) {
            setFirstPosition(controllerTransform);
            firstPointSet = true;
        } else if (!secondPointSet) {
            setSecondPosition(controllerTransform);
            drawBox();

            if (!saveObjects) {
                //destroyBox();
            } else {
                selectionBox.AddComponent<Rigidbody>();
                selectionBox.GetComponent<Rigidbody>().isKinematic = true;
                selectionBox.GetComponent<Rigidbody>().useGravity = false;
                selectionBox.AddComponent<MarkType>();
                selectionBox.GetComponent<MarkType>().typeId = 1;
                selectionBox = null;
            }
            firstPointSet = false;
            secondPointSet = false; //Redundant
        }
        return selectionBox;
    }

    public void saveBox() {
        selectionBox.AddComponent<Rigidbody>();
        selectionBox.GetComponent<Rigidbody>().isKinematic = true;
        selectionBox.GetComponent<Rigidbody>().useGravity = false;
        selectionBox.AddComponent<MarkType>();
        selectionBox.GetComponent<MarkType>().typeId = 1;
        selectionBox = null;
    }

    /// <summary>
    /// Sets the first position of the selectionbox when you want to copy something and creates a primtive cube that will be our visual representation when we scale.
    /// The box receives a selectionbox script which is just there to check for triggers when blocks enters and leaves.
    /// </summary>
    /// <param name="controllerTransform">The transform of the controller who activated copy to determine the starting position of our selection</param>
    public void setFirstPosition(Vector3 controllerTransform) {
        firstPosition = controllerTransform;
        selectionBox = GameObject.CreatePrimitive(PrimitiveType.Cube);
        selectionBox.GetComponent<Renderer>().material = selectionBoxMaterial;
        selectionBox.tag = "Mark";
        selectionBox.layer = 8;
        firstPointSet = true;
    }

    /// <summary>
    /// Sets the second postion when we copy
    /// </summary>
    /// <param name="controllerTransform">The transform of the controller who activated copy to determine the end position of our selection</param>
    public void setSecondPosition(Vector3 controllerTransform) {
        secondPosition = controllerTransform;
        secondPointSet = true;
    }

    public void destroyBox() {
        Destroy(selectionBox);
    }

    /// <summary>
    /// Sets start and end position immediately, used by for example the save function when we just want to select everything and creates a big box
    /// </summary>
    /// <param name="firstPos">start pos</param>
    /// <param name="secondPos">end pos</param>
    public void setPositions(Vector3 firstPos, Vector3 secondPos) {
        setFirstPosition(firstPos);
        setSecondPosition(secondPos);


    }
    /// <summary>
    /// This method is used to "draw" the box which basically means to update our cube with the new scale and calculating the middle point (transform.position).
    /// So basically we just use a primitive cube and stretch it out when they player drag the cube and it acts like a selection box.
    /// The reason why we check which point is closest to the origio is to make it easier for the calculateBox method. We would have to do the check anyways
    /// since the center position calculations depends on it.
    /// </summary>
    public void drawBox() {
        if (Vector3.Distance(firstPosition, new Vector3(0f, 0f, 0f)) >= Vector3.Distance(secondPosition, new Vector3(0f, 0f, 0f))) {
            calculateBox(firstPosition, secondPosition);
        } else {
            calculateBox(secondPosition, firstPosition);
        }

        selectionBox.transform.position = centerPosition;
        selectionBox.transform.localScale = sizes;

    }

    /// <summary>
    /// Calculates the sizes by measuring distances from the two points.
    /// Also calculates the centerpoint by taking the point closest to origo and subtracting with half the size of the box.
    /// </summary>
    /// <param name="closestPosition">Closest point to origo</param>
    /// <param name="furthestPosition">Closest point to origo</param>
    private void calculateBox(Vector3 closestPosition, Vector3 furthestPosition) {
        Vector3 distanceOrigoClosestPoint = closestPosition;
        float sizeX = closestPosition.x - furthestPosition.x;
        float sizeY = closestPosition.y - furthestPosition.y;
        float sizeZ = closestPosition.z - furthestPosition.z;
        sizes = new Vector3(sizeX, sizeY, sizeZ);
        centerPosition = new Vector3(closestPosition.x - sizeX / 2, closestPosition.y - sizeY / 2, closestPosition.z - sizeZ / 2);
    }

    /// <summary>
    /// Returns the vector coordinates for all the six edges of the box and the center position
    /// </summary>
    /// <returns name="area">Vector coordinates of the box and the center position</returns>
    public Vector3[] selectedArea() {
        Vector3[] area = new Vector3[7];
        area[0] = selectionBox.transform.position + (sizes.x / 2) * selectionBox.transform.right;
        area[1] = selectionBox.transform.position - (sizes.x / 2) * selectionBox.transform.right;
        area[2] = selectionBox.transform.position + (sizes.y / 2) * selectionBox.transform.up;
        area[3] = selectionBox.transform.position - (sizes.y / 2) * selectionBox.transform.up;
        area[4] = selectionBox.transform.position + (sizes.z / 2) * selectionBox.transform.forward;
        area[5] = selectionBox.transform.position - (sizes.z / 2) * selectionBox.transform.forward;
        area[6] = selectionBox.transform.position;


        Debug.Log("area[0]: " + area[0]);
        Debug.Log("area[1]: " + area[1]);
        Debug.Log("area[2]: " + area[2]);
        Debug.Log("area[3]: " + area[3]);
        Debug.Log("area[4]: " + area[4]);
        Debug.Log("area[5]: " + area[5]);
        Debug.Log("scale: " + selectionBox.transform.localScale);
        return area;
    } 

    public GameObject GetBox() {
        return selectionBox;
    }
}
