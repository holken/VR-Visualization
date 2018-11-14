using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class DDDHistogram : MonoBehaviour {
    public int nbrOfColumns = 100;
    public float graphSize = 1;
    float columnAgeWidthX;
    float columnAgeWidthY;
    float columnSizeX;
    float columnSizeY;

    private DataLoader dataLoader;
    private List<LabeledData>[,] splitMatrix;
    public Vector3 spawnArea;

    private float normalizationValue;
    
    int maxAmountData = 0;
    public Material backGroundColor;
    public Material barMaterial;
    public bool saveList;

    private float minPosX;
    private float minNegX;
    private float minPosY;
    private float minNegY;
    float xGap;
    float yGap;


    private string labelX = "X-axis";
    private string labelY = "Y-axis";
    private string labelz = "amount";
    private string title = "Star-Age distribution";
    private Vector3 startPos;
    private Vector3 endPos;

    public bool isLogarithmic = false;
    private float[] logAges;

    private int graphIndex = 0;

    void Start()
    {
        dataLoader = DataLoader.dataLoader;
        spawnArea = GetComponent<PlayerParts>().body.transform.position + Vector3.up;
    }

    public void CreateGraph()
    {
        graphIndex = SpaceUtilities.Instance.currentVariableForGraph;
        SplitArrayLinear(); 

    }

    private void SplitArrayLinear()
    {
        List<LabeledData> tmp = dataLoader.GetDataSet();
        splitMatrix = new List<LabeledData>[nbrOfColumns, nbrOfColumns];
        minPosX = dataLoader.GetComponent<DataLoader>().DataEdges()[0];
        minNegX = dataLoader.GetComponent<DataLoader>().DataEdges()[1];
        minPosY = dataLoader.GetComponent<DataLoader>().DataEdges()[2];
        minNegY = dataLoader.GetComponent<DataLoader>().DataEdges()[3];

        maxAmountData = 0;
        xGap = Mathf.Abs(minPosX - minNegX);
        yGap = Mathf.Abs(minPosY - minNegY);
        columnAgeWidthX = (float)xGap / nbrOfColumns;
        columnAgeWidthY = (float)yGap / nbrOfColumns;
        columnSizeX = (float)graphSize / nbrOfColumns;
        columnSizeY = (float)graphSize / nbrOfColumns;
        Debug.Log("columnSizeX: " + columnSizeX);

        for (int i = 0; i < splitMatrix.GetLength(0); i++)
            for (int j = 0; j < splitMatrix.GetLength(1); j++)
                splitMatrix[i, j] = new List<LabeledData>();

        //Goes through each planet, check it's age and put it in the right category
        int count = 0;
        foreach (LabeledData data in tmp)
        {
            float xIndex = Mathf.Abs(data.Position.x - minPosX);
            float yIndex = Mathf.Abs(data.Position.y - minPosY);
            
            
            //Debug.Log("index: " + ((planetAge / columnAgeWidth) -1));
            int indexX = (int)(xIndex / columnAgeWidthX);
            int indexY = (int)(yIndex / columnAgeWidthX);

            if (indexX < 0)
            {
                indexX = 0;
            }
            if (indexX >= nbrOfColumns)
            {
                indexX = 99;
            }
            if (indexY < 0)
            {
                indexY = 0;
            }
            if (indexY >= nbrOfColumns)
            {
                indexY = 99;
            }

            //Debug.Log("the data: " + data);
            //Debug.Log("SplitMatrix: " + splitMatrix);
            //Debug.Log("splitMatrix[indexX,indexY]: " + splitMatrix[indexX, indexY]);
            splitMatrix[indexX,indexY].Add(data);
            if (splitMatrix[indexX,indexY].Count > maxAmountData)
            {
                maxAmountData = splitMatrix[indexX,indexY].Count;
            }
            count++;
        }
        if (!isLogarithmic)
        {
            PaintGraph();
        } else
        {
            PaintGraphLog();
        }
        
    }

    private void PaintGraphLog()
    {
        GameObject emptyParent = new GameObject();
        maxAmountData = (int) Mathf.Log10(maxAmountData);
        emptyParent.transform.position = spawnArea; //TODO remove spawnArea variable completely?
        emptyParent.AddComponent<Rigidbody>();
        emptyParent.GetComponent<Rigidbody>().isKinematic = true;
        emptyParent.GetComponent<Rigidbody>().useGravity = false;
        emptyParent.AddComponent<BoxCollider>();
        emptyParent.GetComponent<BoxCollider>().size = new Vector3(graphSize, graphSize, 1); //TODO change 1
        //TODO change center of collider

        emptyParent.layer = 8;
        emptyParent.tag = "Mark";

        for (int i = 0; i < splitMatrix.GetLength(0); i++)
        {
            for (int j = 0; j < splitMatrix.GetLength(1); j++)
            {
                float amountData = splitMatrix[i, j].Count;
                float height = 0;
                if (amountData != 0)
                    height = (float)Mathf.Log10(amountData) / maxAmountData;
                
                List<LabeledData> data = splitMatrix[i, j];

                GameObject tmp = GameObject.CreatePrimitive(PrimitiveType.Cube);
                Destroy(tmp.GetComponent<BoxCollider>());
                Destroy(tmp.GetComponent<Rigidbody>());
                tmp.transform.localScale = new Vector3(columnSizeX, columnSizeY, height);
                tmp.transform.position = new Vector3(spawnArea.x + ((i * columnSizeX) - (1 / 2)),
                    spawnArea.y + ((j * columnSizeY) - (1 / 2)),
                    spawnArea.z - height / 2);
                tmp.transform.parent = emptyParent.transform;

                tmp.GetComponent<Renderer>().material = barMaterial;
                Color barColor = GetComponent<GradientManager>().getColor(height);
                tmp.GetComponent<Renderer>().material.color = barColor;

                if (saveList)
                {
                    tmp.AddComponent<GraphBarHandler>();
                    tmp.GetComponent<GraphBarHandler>().dataSet = data;
                    tmp.GetComponent<GraphBarHandler>().SetDefaultColor(barColor);
                }
            }

        }
        emptyParent.transform.rotation = Quaternion.Euler(90f, 0f, 0f);

        emptyParent.transform.position = GetComponent<PlayerParts>().head.transform.position + GetComponent<PlayerParts>().head.transform.forward * 2
            - GetComponent<PlayerParts>().head.transform.up;
    }

    private void PaintGraph()
    {
        GameObject emptyParent = new GameObject();

        emptyParent.transform.position = spawnArea; //TODO remove spawnArea variable completely?
        emptyParent.AddComponent<Rigidbody>();
        emptyParent.GetComponent<Rigidbody>().isKinematic = true;
        emptyParent.GetComponent<Rigidbody>().useGravity = false;
        emptyParent.AddComponent<BoxCollider>();
        emptyParent.GetComponent<BoxCollider>().size = new Vector3(graphSize, graphSize, 1); //TODO change 1

        emptyParent.layer = 8;
        emptyParent.tag = "Mark";

        for (int i = 0; i < splitMatrix.GetLength(0); i++)
        {
            for (int j = 0; j < splitMatrix.GetLength(1); j++)
            {
                float amountData = splitMatrix[i, j].Count;
                float height = (float)amountData / maxAmountData;
                List<LabeledData> data = splitMatrix[i, j];

                GameObject tmp = GameObject.CreatePrimitive(PrimitiveType.Cube);
                Destroy(tmp.GetComponent<BoxCollider>());
                Destroy(tmp.GetComponent<Rigidbody>());
                tmp.transform.localScale = new Vector3(columnSizeX, columnSizeY, height);
                tmp.transform.position = new Vector3(spawnArea.x + ((i * columnSizeX) - (1 / 2)),
                    spawnArea.y + ((j * columnSizeY) - (1 / 2)),
                    spawnArea.z - height / 2);
                tmp.transform.parent = emptyParent.transform;

                tmp.GetComponent<Renderer>().material = barMaterial;
                if (height > 0.5f)
                    Debug.Log("height: " + height);
                Color barColor = GetComponent<GradientManager>().getColor(height);
                tmp.GetComponent<Renderer>().material.color = barColor;

                if (saveList)
                {
                    tmp.AddComponent<GraphBarHandler>();
                    tmp.GetComponent<GraphBarHandler>().dataSet = data;
                    tmp.GetComponent<GraphBarHandler>().SetDefaultColor(barColor);
                }
            }

        }
        emptyParent.transform.rotation = Quaternion.Euler(90f, 0f, 0f);

        emptyParent.transform.position = GetComponent<PlayerParts>().head.transform.position + GetComponent<PlayerParts>().head.transform.forward * 2
            - GetComponent<PlayerParts>().head.transform.up;
    }
}
