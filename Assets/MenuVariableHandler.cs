using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class MenuVariableHandler : MonoBehaviour {
    public GameObject variableContainerPrefab;
    public GameObject[] variableContainers;
    private void OnEnable()
    {
        string[] variables = DataLoader.dataLoader.labelNames;
        int i = 0;
        variableContainers = new GameObject[variables.Length > 4 ? 4 : variables.Length];
        while (i < variables.Length)
        {
            if (i < 4)
            {
                GameObject variableContainer = (GameObject)Instantiate(variableContainerPrefab);
                variableContainer.transform.parent = transform;
                variableContainer.transform.position = new Vector3(0f, 0.0316f - i * 0.021f, 0f);
                variableContainer.GetComponent<VariableContainer>().text.text = variables[i];
                if (i == 0) { variableContainer.GetComponent<VariableContainer>().radioBtn.SelectButton(); }
                variableContainers[i] = variableContainer;
            }
            
            
        }
    }

    // Update is called once per frame
    void Update () {
		
	}
}
