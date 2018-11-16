using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using pointcloud_objects;
public class GraphBarHandler : MonoBehaviour {

    public List<LabeledData> dataSet;
    public float maxAge;
    public float minAge;
    public bool selected = false;
    private Color defaultColor;
    public int index;
    public int count;

    public void HighlightBar() {
        GetComponent<Renderer>().material.color = Color.yellow;
    }

    public void DeHighlight()
    {
        if (!selected)
        {
            GetComponent<Renderer>().material.color = defaultColor;
        }
        
    }

    public void Deselect() {
        GetComponent<Renderer>().material.color = defaultColor;
    }

    public void SetDefaultColor(Color color) {
        defaultColor = color;
    }

    public void SelectBar()
    {
        GetComponent<Renderer>().material.color = Color.red;
        selected = true;
    }

}
