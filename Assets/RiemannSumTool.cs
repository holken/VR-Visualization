using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using pointcloud_objects;
using UnityEngine.UI;

public class RiemannSumTool : MonoBehaviour {
    public int nbrOfColumns = 100;
    private GameObject dataLoader;
    private List<List<LabeledData>> splitArray;
    private List<Color> barColors;
    public Vector3 spawnArea;
    public float columnSize;
    private float normalizationValue;
    float columnAgeWidth;
    int maxAmountStars = 0;
    public Material backGroundColor;
    public Material barColor;
    public bool saveList;
    private float minAge;
    private float maxAge;

    private string labelX = "Age";
    private string labelY = "amount";
    private string title = "Star-Age distribution";
    private int featureIndex;
    private Vector3 startPos;
    private Vector3 endPos;

    private bool isLogarithmic = false;
    private float[] logAges;


    private int graphIndex = 0;

    void Start() {
        dataLoader = GetComponent<SpaceUtilities>().dataLoader;
        
        spawnArea = GetComponent<PlayerParts>().body.transform.position + Vector3.up;
    }

    public void SetIsLogarithmic(bool logarithmic) { isLogarithmic = logarithmic; }

    public void UpdateColumnSize(float sizeIncr) {
        float tmp = columnSize + sizeIncr;
        if (!(tmp <= 0)) {
            columnSize = columnSize + sizeIncr;
        }
    }

    public void UpdateNbrOfColumns(int columnIncr) {
        int tmp = nbrOfColumns + columnIncr;
        if (!(tmp <= 0)) {
            nbrOfColumns = nbrOfColumns + columnIncr;
        }
    }

    public void CreateGraph(float minValue, float maxValue) {
        this.minAge = minValue;
        this.maxAge = maxValue;
        graphIndex = SpaceUtilities.Instance.currentVariableForGraph;
        labelX = MenuHandler.Instance.graphVariableText.GetComponent<Text>().text;
        this.title = DataLoader.dataLoader.labelNames[SpaceUtilities.Instance.GetComponent<SpaceUtilities>().currentVariableForGraph];
        featureIndex = SpaceUtilities.Instance.GetComponent<SpaceUtilities>().currentVariableForGraph;
        if (!isLogarithmic) { SplitArrayLinear(); }
        if (isLogarithmic) { SplitArrayLog(); }
        
    }

    private void SplitArrayLog() {
        List<LabeledData> tmp = dataLoader.GetComponent<DataLoader>().GetDataSet();
        barColors = new List<Color>();
        splitArray = new List<List<LabeledData>>();
        maxAge = dataLoader.GetComponent<DataLoader>().maxData[graphIndex];
        minAge = dataLoader.GetComponent<DataLoader>().minData[graphIndex];
        float ageGap = maxAge - minAge;
        maxAmountStars = 0;

        int logMaxAge = Mathf.CeilToInt(Mathf.Log10(maxAge));
        int logMinAgeNeg = 0;
        int divisionPerLog = 9;
        int nbrOfLogColumns = logMaxAge * divisionPerLog ;
        if ( minAge < 0)
        {
            logMinAgeNeg = Mathf.CeilToInt(Mathf.Log10(-minAge));
            nbrOfLogColumns += logMinAgeNeg * divisionPerLog;
        }
        
        logAges = new float[nbrOfLogColumns];
        for (int i = 0; i < nbrOfLogColumns; i++) {
            splitArray.Add(new List<LabeledData>());
            //TODO fix log
            float averageColor = ((float)i / (float)nbrOfLogColumns);
            barColors.Add(GetComponent<GradientManager>().getColor(averageColor));
        }
        int count = 0;
        Debug.Log("logMaxAge: " + logMaxAge);
        Debug.Log("logMinAgeNeg: " + logMinAgeNeg);
        Debug.Log("logMaxAge * divisionPerLog: " + logMaxAge * divisionPerLog);
        Debug.Log("nbrOfLogColumns: " + nbrOfLogColumns);
        foreach (LabeledData data in tmp) {
            /*Basically what is done in the following lines are just a way to index the planet into the different graphs
             * For example, the age 2100 gives us Log10(2100) = 3.322... We want to split up the graph in 1, 10, 100, 1000, but
             * that will be very low detail, therefore we split up the graphs into 1, 2, 3,...,10,20,30,..., 100,... etc
             * Therefore we have to do the following: 2100 / 10^3 = 2.1 (where 10 is because log10 and 3 is the closest flooring int to 3.322...)
             * Round it down to 2, we can now calculate the index in which the planet should be placed
             * Index = 3*10 + 2 = 32; to be adjusted*/
            float planetLogAge = 0;
            float planetAge = data.features[graphIndex];
            int subIndex = 0;
            if (planetAge <= 1) { 
                if (planetAge >= 0)
                {
                    splitArray[logMinAgeNeg* divisionPerLog].Add(data);
                } else
                {
                    if (planetAge >= -1)
                    {
                        splitArray[0].Add(data);
                    } else
                    {
                        planetLogAge = Mathf.Log10(-planetAge);
                        int planetLogAgeInt = Mathf.FloorToInt(planetLogAge);
                        //int restPart = Mathf.FloorToInt(planetAge / Mathf.Pow(10, planetLogAgeInt));
                        subIndex = (planetLogAgeInt * 10) + (int)(-planetAge) / (int)(Mathf.Pow(10, planetLogAgeInt)) - planetLogAgeInt;
                        if (subIndex < 0)
                        {
                            //Debug.Log("hmm too low");
                            subIndex = 0;
                        }
                        if (subIndex >= nbrOfLogColumns)
                        {
                            //Debug.Log("hmm too high");
                            subIndex = nbrOfLogColumns - 1;
                        }
                    }
                    
                    
                }
                
            } else {
                planetLogAge = Mathf.Log10(planetAge);
                int planetLogAgeInt = Mathf.FloorToInt(planetLogAge);
                //int restPart = Mathf.FloorToInt(planetAge / Mathf.Pow(10, planetLogAgeInt));
                subIndex = (planetLogAgeInt * 10) + (int)planetAge / (int)(Mathf.Pow(10, planetLogAgeInt)) - planetLogAgeInt + (logMinAgeNeg* divisionPerLog);

                if (subIndex < 0)
                {
                    //Debug.Log("hmm too low");
                    subIndex = 0;
                }
                if (subIndex >= nbrOfLogColumns)
                {
                    //Debug.Log("hmm too high");
                    subIndex = nbrOfLogColumns - 1;
                }

            }
            splitArray[subIndex].Add(data);
            if (splitArray[subIndex].Count > maxAmountStars)
            {
                maxAmountStars = splitArray[subIndex].Count;
            }




            count++;
        }
 
        PaintGraphLog();
    }


    private void SplitArrayLinear() {
        List<LabeledData> tmp = dataLoader.GetComponent<DataLoader>().GetDataSet();
        barColors = new List<Color>();
        splitArray = new List<List<LabeledData>>();
        float maxAge = dataLoader.GetComponent<DataLoader>().maxData[graphIndex];
        float minAge = dataLoader.GetComponent<DataLoader>().minData[graphIndex];
        maxAmountStars = 0;
        float ageGap = Mathf.Abs(maxAge - minAge);
        columnAgeWidth = ageGap / nbrOfColumns;
        Debug.Log("maxAge: " + maxAge);
        Debug.Log("minAge: " + minAge);
        Debug.Log("columnAgeWidth: " + columnAgeWidth);
        
        
        for (int i = 0; i < nbrOfColumns; i++) {
            splitArray.Add(new List<LabeledData>());
            //TODO better way to make color?
            float averageColor = (((columnAgeWidth * i) + (columnAgeWidth + 1 * i)) / ageGap);
            Debug.Log("getColor: " + averageColor);
            barColors.Add(GetComponent<GradientManager>().getColor(averageColor));

        }

        int count = 0;
        //Goes through each planet, check it's age and put it in the right category
        foreach (LabeledData data in tmp) {
            float dataValue = data.features[graphIndex];
            
            if (dataValue <= maxAge && dataValue >= minAge)
            {
                int index = (int)((dataValue - minAge) / columnAgeWidth);

 
                if (index < 0)
                {
                    index = 0;
                }
                if (index >= nbrOfColumns)
                {
                    index = 99;
                }

                splitArray[index].Add(data);
                if (splitArray[index].Count > maxAmountStars)
                {
                    maxAmountStars = splitArray[index].Count;
                }
                count++;
            }

            
        }

        PaintGraph();
    }

    private void PaintGraphLog() {
        
        int arraySize = splitArray.Count;
        Debug.Log("arraySize: " + arraySize);
        float halfPoint = (arraySize * columnSize) / 2;

        GameObject emptyParent = new GameObject();
        emptyParent.AddComponent<GraphHandler>();
        emptyParent.GetComponent<GraphHandler>().bars = new List<GameObject>();
        //TODO fix it so it paints out a log scale instead of linear
        emptyParent.GetComponent<GraphHandler>().Configure();
        emptyParent.transform.position = spawnArea; 
        emptyParent.AddComponent<BoxCollider>();
        emptyParent.AddComponent<Rigidbody>();
        emptyParent.GetComponent<Rigidbody>().isKinematic = true;
        emptyParent.GetComponent<Rigidbody>().useGravity = false;

        emptyParent.GetComponent<BoxCollider>().size = new Vector3(((splitArray.Count * columnSize) - halfPoint) * 2 * emptyParent.transform.localScale.x + 0.2f
            , 1f * emptyParent.transform.localScale.y + 0.2f, columnSize * 2 - 0.001f * emptyParent.transform.localScale.z); //TODO columnSize?

        emptyParent.GetComponent<BoxCollider>().center = new Vector3(emptyParent.GetComponent<BoxCollider>().center.x,
            emptyParent.GetComponent<BoxCollider>().center.y + 0.5f, emptyParent.GetComponent<BoxCollider>().center.z + columnSize);

        emptyParent.layer = 8;
        emptyParent.tag = "Mark";

        float maxHeight = 0f;
        int count = 0;
        GameObject yAxis = null;

        float startTime = Time.time;
        int count2 = 0;
        bool incrementedCount2 = false;
        foreach (List<LabeledData> data in splitArray) {
            GameObject tmp = GameObject.CreatePrimitive(PrimitiveType.Cube);

            float height = (float)data.Count / maxAmountStars;
            if (height > maxHeight) {
                maxHeight = height;
            }

            tmp.transform.localScale = new Vector3(columnSize, height, columnSize); 
            tmp.transform.position = new Vector3(spawnArea.x + ((count * columnSize) - halfPoint), spawnArea.y + height / 2, spawnArea.z);

            tmp.transform.parent = emptyParent.transform;
            tmp.GetComponent<Renderer>().material = barColor;

            //TODO make the text scale with size of columns etc
            if (count == arraySize - 1) {
                GameObject txt = (GameObject)Instantiate(Resources.Load("ControllerText"));
                endPos = tmp.transform.position;
                txt.transform.position = endPos + new Vector3(0f, -height / 2 - 0.05f, 0f);
                txt.GetComponent<TextMesh>().text = (maxAge).ToString();
                txt.transform.parent = emptyParent.transform;

            } else if (count == 0) {
                GameObject txt = (GameObject)Instantiate(Resources.Load("ControllerText"));
                startPos = tmp.transform.position;
                txt.transform.position = startPos + new Vector3(0f, -height / 2 - 0.05f, 0f);
                txt.GetComponent<TextMesh>().text = (minAge).ToString();
                txt.transform.parent = emptyParent.transform;

                //This is to get the right position in X,Y. We need to change height though to the max
                yAxis = (GameObject)Instantiate(Resources.Load("ControllerText"));
                yAxis.transform.position = tmp.transform.position;
                yAxis.transform.rotation = Quaternion.Euler(0, 0, 90);
                yAxis.GetComponent<TextMesh>().text = (maxAmountStars).ToString();

            }
            tmp.GetComponent<Renderer>().material.color = barColors[count];
            //Debug.Log("colors: " + barColors[count]);
            if (saveList) {
                tmp.AddComponent<GraphBarHandler>();
                tmp.GetComponent<GraphBarHandler>().dataSet = data;
                emptyParent.GetComponent<GraphHandler>().bars.Add(tmp);
                tmp.GetComponent<GraphBarHandler>().index = count;
                tmp.GetComponent<GraphBarHandler>().count = data.Count;

                if ((count + 1 + count2) % 10 == 0) { count2++; incrementedCount2 = true; }

                int indexDivisible = (count  + count2) / 10;
                int indexRest = (count  + count2) % 10;
                tmp.GetComponent<GraphBarHandler>().maxAge = Mathf.Pow(10,indexDivisible) + indexRest* Mathf.Pow(10, indexDivisible); 
                tmp.GetComponent<GraphBarHandler>().SetDefaultColor(barColors[count]);
                if (count == 0) {
                    tmp.GetComponent<GraphBarHandler>().minAge = 0;
                } else {
                    if (incrementedCount2) {
                        indexDivisible = (count - 1 + count2 - 1) / 10;
                        indexRest = (count - 1 + count2 - 1) % 10;
                        incrementedCount2 = false;
                    } else {
                        indexDivisible = (count - 1 + count2) / 10;
                        indexRest = (count - 1 + count2) % 10;
                    }
                    
                    tmp.GetComponent<GraphBarHandler>().minAge = Mathf.Pow(10, indexDivisible) + indexRest * Mathf.Pow(10, indexDivisible);
                }
                if ((count + 1 + count2 - 1) % 10 == 0) {
                    GameObject txt = (GameObject)Instantiate(Resources.Load("ControllerText"));
                    endPos = tmp.transform.position;
                    txt.transform.position = endPos + new Vector3(0f, -height / 2 - 0.1f, 0f);
                    txt.GetComponent<TextMesh>().text = (tmp.GetComponent<GraphBarHandler>().maxAge).ToString();
                    txt.GetComponent<TextMesh>().fontSize = 150;
                    txt.transform.parent = emptyParent.transform;
                    txt.transform.rotation = Quaternion.Euler(0, 0, 90);
                }

                //TODO tweak stuff, I just noticed that the first index is just stars that are age 0
                tmp.layer = 8;
                tmp.tag = "GraphBar";
                
            } else {
                Destroy(tmp.GetComponent<BoxCollider>());
            }

            count++;
        }
        GameObject labels = (GameObject)Instantiate(Resources.Load("ControllerText"));
        labels.transform.position = startPos + new Vector3((endPos.x - startPos.x) / 2, maxHeight, 0f);
        labels.GetComponent<TextMesh>().text = title;
        labels.transform.parent = emptyParent.transform;

        labels = (GameObject)Instantiate(Resources.Load("ControllerText"));
        labels.transform.position = startPos + new Vector3((endPos.x - startPos.x) / 2, -0.1f, 0f);
        labels.GetComponent<TextMesh>().text = labelX;
        labels.transform.parent = emptyParent.transform;

        // different positions because we rotate it 90 degrees
        labels = (GameObject)Instantiate(Resources.Load("ControllerText"));
        labels.transform.position = startPos + new Vector3(-0.05f, maxHeight / 2, 0f);
        labels.GetComponent<TextMesh>().text = labelY;
        labels.transform.rotation = Quaternion.Euler(0, 0, 90);
        labels.transform.parent = emptyParent.transform;


        //TODO, make the yAxis etc scale with number of stars
        if (yAxis) {
            yAxis.transform.position = new Vector3(yAxis.transform.position.x - 0.064f, yAxis.transform.position.y + maxHeight, yAxis.transform.position.z);
            yAxis.transform.parent = emptyParent.transform;
        }
        GameObject background = GameObject.CreatePrimitive(PrimitiveType.Cube);

        background.transform.localScale = emptyParent.GetComponent<BoxCollider>().size;
        background.transform.position = emptyParent.transform.position + new Vector3(0f, 0.5f, columnSize);


        //background.GetComponent<LineRenderer>().SetPosition(4, startPos);
        background.transform.parent = emptyParent.transform;
        background.GetComponent<Renderer>().sharedMaterial = backGroundColor;
        Destroy(background.GetComponent<BoxCollider>());
        emptyParent.transform.localScale = GetComponent<PlayerParts>().body.transform.localScale * 0.5f;
        emptyParent.transform.position = GetComponent<PlayerParts>().head.transform.position + GetComponent<PlayerParts>().head.transform.forward * 2
            - GetComponent<PlayerParts>().head.transform.up;
        emptyParent.transform.LookAt(GetComponent<PlayerParts>().head.transform);
        emptyParent.transform.Rotate(new Vector3(0f, 180f, 0f));
    
}

    private void PaintGraph() {
        int arraySize = splitArray.Count;
        float halfPoint = (arraySize * columnSize) / 2;

        GameObject emptyParent = new GameObject();
        emptyParent.AddComponent<GraphHandler>();
        emptyParent.GetComponent<GraphHandler>().bars = new List<GameObject>();
        emptyParent.GetComponent<GraphHandler>().Configure();
        emptyParent.transform.position = spawnArea; //TODO remove spawnArea variable completely?
        emptyParent.AddComponent<BoxCollider>();
        emptyParent.AddComponent<Rigidbody>();
        emptyParent.GetComponent<Rigidbody>().isKinematic = true;
        emptyParent.GetComponent<Rigidbody>().useGravity = false;
        
        emptyParent.GetComponent<BoxCollider>().size = new Vector3(((splitArray.Count * columnSize) - halfPoint)*2 * emptyParent.transform.localScale.x+0.2f
            , 1f * emptyParent.transform.localScale.y +0.2f, columnSize*2 - 0.001f * emptyParent.transform.localScale.z);

        emptyParent.GetComponent<BoxCollider>().center = new Vector3(emptyParent.GetComponent<BoxCollider>().center.x,
            emptyParent.GetComponent<BoxCollider>().center.y + 0.5f, emptyParent.GetComponent<BoxCollider>().center.z + columnSize);

        emptyParent.layer = 8;
        emptyParent.tag = "Mark";

        float maxHeight = 0f;
        int count = 0;
        GameObject yAxis = null;

        float startTime = Time.time;

        foreach (List<LabeledData> data in splitArray) {
            GameObject tmp = GameObject.CreatePrimitive(PrimitiveType.Cube);
            float height = (float)data.Count / maxAmountStars;
            if (height > maxHeight) {
                maxHeight = height;
            }

            tmp.transform.localScale = new Vector3(columnSize, height, columnSize); //TODO replace planets.Count/1000 with something smarter
            tmp.transform.position = new Vector3(spawnArea.x + ((count * columnSize) - halfPoint), spawnArea.y + height/2, spawnArea.z);
            
            tmp.transform.parent = emptyParent.transform;
            tmp.GetComponent<Renderer>().material = barColor;
            
            //TODO make the text scale with size of columns etc
            if (count == arraySize-1) {
                GameObject txt = (GameObject)Instantiate(Resources.Load("ControllerText"));
                endPos = tmp.transform.position;
                txt.transform.position = endPos + new Vector3(0f, -height/2 - 0.05f, 0f);
                txt.GetComponent<TextMesh>().text = (maxAge).ToString();
                txt.transform.parent = emptyParent.transform;

            } else if (count == 0) {
                GameObject txt = (GameObject)Instantiate(Resources.Load("ControllerText"));
                startPos = tmp.transform.position;
                txt.transform.position = startPos + new Vector3(0f, -height / 2 - 0.05f, 0f);
                txt.GetComponent<TextMesh>().text = (minAge).ToString();
                txt.transform.parent = emptyParent.transform;

                //This is to get the right position in X,Y. We need to change height though to the max
                yAxis = (GameObject)Instantiate(Resources.Load("ControllerText"));
                yAxis.transform.position = tmp.transform.position;
                yAxis.transform.rotation = Quaternion.Euler(0, 0, 90);
                yAxis.GetComponent<TextMesh>().text = (maxAmountStars).ToString();
                
            }
            tmp.GetComponent<Renderer>().material.color = barColors[count];
            //Debug.Log("colors: " + barColors[count]);
            if (saveList) {
                emptyParent.GetComponent<GraphHandler>().bars.Add(tmp);
                tmp.AddComponent<GraphBarHandler>();
                tmp.GetComponent<GraphBarHandler>().dataSet = data;
                tmp.GetComponent<GraphBarHandler>().maxAge = minAge + (count+1) * columnAgeWidth;
                tmp.GetComponent<GraphBarHandler>().SetDefaultColor(barColors[count]);
                tmp.GetComponent<GraphBarHandler>().index = count;
                tmp.GetComponent<GraphBarHandler>().count = data.Count;
                if (count == 0) {
                    tmp.GetComponent<GraphBarHandler>().minAge = 0;
                }
                tmp.GetComponent<GraphBarHandler>().minAge = minAge + columnAgeWidth*count; //+1 so we dont get the bracket before
                //TODO tweak stuff, I just noticed that the first index is just stars that are age 0
                tmp.layer = 8;
                tmp.tag = "GraphBar";
            } else {
                Destroy(tmp.GetComponent<BoxCollider>());
            }

            count++;
        }
        GameObject labels = (GameObject)Instantiate(Resources.Load("ControllerText"));
        labels.transform.position = startPos + new Vector3((endPos.x - startPos.x) / 2, maxHeight, 0f);
        labels.GetComponent<TextMesh>().text = title;
        labels.transform.parent = emptyParent.transform;

        labels = (GameObject)Instantiate(Resources.Load("ControllerText"));
        labels.transform.position = startPos + new Vector3((endPos.x - startPos.x) / 2, -0.1f, 0f);
        labels.GetComponent<TextMesh>().text = labelX;
        labels.transform.parent = emptyParent.transform;

        // different positions because we rotate it 90 degrees
        labels = (GameObject)Instantiate(Resources.Load("ControllerText"));
        labels.transform.position = startPos + new Vector3(-0.05f, maxHeight/2, 0f);
        labels.GetComponent<TextMesh>().text = labelY;
        labels.transform.rotation = Quaternion.Euler(0, 0, 90);
        labels.transform.parent = emptyParent.transform;

       
        //TODO, make the yAxis etc scale with number of stars
        if (yAxis) {
            yAxis.transform.position = new Vector3(yAxis.transform.position.x - 0.064f, yAxis.transform.position.y + maxHeight, yAxis.transform.position.z); 
            yAxis.transform.parent = emptyParent.transform;
        }
        GameObject background = GameObject.CreatePrimitive(PrimitiveType.Cube);

        background.transform.localScale = emptyParent.GetComponent<BoxCollider>().size;
        background.transform.position = emptyParent.transform.position + new Vector3(0f,0.5f, columnSize);

        
        //background.GetComponent<LineRenderer>().SetPosition(4, startPos);
        background.transform.parent = emptyParent.transform;
        background.GetComponent<Renderer>().sharedMaterial = backGroundColor;
        Destroy(background.GetComponent<BoxCollider>());
        emptyParent.transform.localScale = GetComponent<PlayerParts>().body.transform.localScale*0.5f;
        emptyParent.transform.position = GetComponent<PlayerParts>().head.transform.position + GetComponent<PlayerParts>().head.transform.forward*2
            - GetComponent<PlayerParts>().head.transform.up;
        emptyParent.transform.LookAt(GetComponent<PlayerParts>().head.transform);
        emptyParent.transform.Rotate(new Vector3(0f, 180f, 0f));
    }

}
