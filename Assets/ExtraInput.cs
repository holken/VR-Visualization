using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class ExtraInput : MonoBehaviour {

    private void Update()
    {
        if (Input.GetKeyDown(KeyCode.Space))
        {
            Debug.Log("space pressed");
            if (!(ActionBuffer.actionBuffer.Count <= 1))
            {
                ActionBuffer.actionBuffer[ActionBuffer.actionBuffer.Count - 1].Undo();
                Debug.Log("Deleted");
            }
                
        }
    }
}
