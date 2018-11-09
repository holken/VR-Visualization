using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class ActionBuffer : MonoBehaviour {
    public static ActionBuffer actionBufferInstance;
    public static List<ManipulationAction> actionBuffer;
    public static List<List<LabeledData>> dataBuffer;
    public static List<Vector3> positionBuffer;
    public static List<Quaternion> rotationBuffer;
    public static List<int> colorBuffer;

    private void Start()
    {
        actionBuffer = new List<ManipulationAction>();
        dataBuffer = new List<List<LabeledData>>();
        positionBuffer = new List<Vector3>();
        rotationBuffer = new List<Quaternion>();
        colorBuffer = new List<int>();
        actionBufferInstance = this;
    }

    public void setManipulationAction(string type)
    {
        if (type == "data")
        {
            actionBuffer.Add(new DataManipulationAction());
            dataBuffer.Add(GetComponent<DataLoader>().GetCurrentDataPoints());
            positionBuffer.Add(GetComponent<DataLoader>().GetCurrentDataSet().transform.position);
            rotationBuffer.Add(GetComponent<DataLoader>().GetCurrentDataSet().transform.rotation);
        }
        if (type == "color")
        {
            actionBuffer.Add(new ColorManipulationAction());
            colorBuffer.Add(GetComponent<DataLoader>().spaceManager.GetComponent<GradientManager>().getIndex());
        }
        Debug.Log("Adding, " + ActionBuffer.dataBuffer.Count + " items in dataBuffer");
    }

    
}

public interface ManipulationAction
{
    void Undo();
}

public class DataManipulationAction : MonoBehaviour, ManipulationAction
{
    public void Undo()
    {
        if (ActionBuffer.dataBuffer != null)
        {
            List<LabeledData> dataToLoad = ActionBuffer.dataBuffer[ActionBuffer.dataBuffer.Count - 2];
            ActionBuffer.dataBuffer.RemoveAt(ActionBuffer.dataBuffer.Count - 2);

            Vector3 positionToLoad = ActionBuffer.positionBuffer[ActionBuffer.positionBuffer.Count - 2];
            ActionBuffer.positionBuffer.RemoveAt(ActionBuffer.positionBuffer.Count - 2);

            Quaternion rotationToLoad = ActionBuffer.rotationBuffer[ActionBuffer.rotationBuffer.Count - 2];
            ActionBuffer.rotationBuffer.RemoveAt(ActionBuffer.rotationBuffer.Count - 2);

            ActionBuffer.actionBuffer.RemoveAt(ActionBuffer.actionBuffer.Count - 2);
            DataLoader.dataLoader.LoadDataFromBuffer(dataToLoad, positionToLoad, rotationToLoad);
        }
        Debug.Log("Deleting, " + ActionBuffer.dataBuffer.Count + " items in dataBuffer");
    }
}

public class ColorManipulationAction : MonoBehaviour, ManipulationAction
{
    public void Undo()
    {
        if (ActionBuffer.colorBuffer != null)
        {
            int colorToLoad = ActionBuffer.colorBuffer[ActionBuffer.colorBuffer.Count - 1];
            ActionBuffer.colorBuffer.RemoveAt(ActionBuffer.colorBuffer.Count - 1);
            ActionBuffer.actionBuffer.RemoveAt(ActionBuffer.actionBuffer.Count - 1);
            GetComponent<DataLoader>().spaceManager.GetComponent<GradientManager>().setGradient(colorToLoad);
        }
    }
}