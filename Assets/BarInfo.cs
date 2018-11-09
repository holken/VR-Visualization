using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class BarInfo {

    public BarInfo(GameObject bar, int index)
    {
        this.bar = bar;
        this.index = index;
    }

    public GameObject bar;
    public int index;
    public bool selected = false;
}
