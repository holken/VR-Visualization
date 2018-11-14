using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class GraphHandler : MonoBehaviour {
    public List<GameObject> bars;
    public string title;
    public int feature;
    public int firstIndex;
    public int secondIndex;
    public float maxValue;
    public float minValue;

    public void Configure()
    {
        Debug.Log("conf graph");
        feature = SpaceUtilities.Instance.currentVariableForGraph;
        maxValue = DataLoader.dataLoader.maxData[feature];
        minValue = DataLoader.dataLoader.minData[feature];
        title = DataLoader.dataLoader.labelNames[feature];
        DataLoader.dataLoader.graphs.Add(this);
        
        Debug.Log("nbr of graphs: " + DataLoader.dataLoader.graphs.Count);
    }

    private void OnDestroy()
    {
        DataLoader.dataLoader.graphs.Remove(this);
        Debug.Log("nbr of graphs: " + DataLoader.dataLoader.graphs.Count);
    }

  
}
