using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class SpaceUtilities : MonoBehaviour {
    public GameObject dataLoader;
    private GameObject selectedGalaxy;

    public float lightyearPerUnit;
    public float klightYearPerUnit;
    public float parsecPerUnit;
    public float kparsecPerUnit;
    private List<float> units;
    public int unitPointer = 0;

	// Use this for initialization
	void Awake () {
        units = new List<float>();
        klightYearPerUnit = lightyearPerUnit / 1000;
        parsecPerUnit = lightyearPerUnit / 3.26f;
        kparsecPerUnit = parsecPerUnit / 1000;
        units.Add(lightyearPerUnit);
        units.Add(klightYearPerUnit);
        units.Add(parsecPerUnit);
        units.Add(kparsecPerUnit);
    }
	

    public List<float> getUnits() {
        return units;
    }

    public GameObject CurrentDataset() { return selectedGalaxy; }

    public void SetCurrentDataset(GameObject selectedGalaxy) {
        this.selectedGalaxy = selectedGalaxy;
        dataLoader.GetComponent<DataLoader>().SetCurrentDataset(selectedGalaxy);

    }
}
