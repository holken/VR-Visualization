using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using pointcloud_objects;
public class GraphBarHandler : MonoBehaviour {

    public List<LabeledData> dataSet;
    public float maxAge;
    public float minAge;
    public bool selected = false;
    public bool highLighted = false;
    private Color defaultColor;
    public int count;



    public void HighlightBar() {
        GetComponent<Renderer>().material.color = Color.yellow;
        highLighted = true;

    }

    void LateUpdate() {
        if (!highLighted && !selected) {
            GetComponent<Renderer>().material.color = defaultColor;
        }

        highLighted = false;
    }

    public void Deselect() {
        GetComponent<Renderer>().material.color = defaultColor;
        highLighted = false;
        selected = false;

    }

    public void SetDefaultColor(Color color) {
        defaultColor = color;
    }


}
