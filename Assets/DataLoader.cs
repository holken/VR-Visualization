using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using System.IO;
using System;
using System.Text;
using UnityEngine.UI;

public class LabeledData 
{
    public LabeledData() { }
    public string Label;
    public Vector3 Position;
    public Color Color;
    public float[] features;
}

public class DataLoader : MonoBehaviour {
    public static DataLoader dataLoader;
    // GUI
    private float progress = 0;
    private string guiText;
    private bool loaded = false;

    // File
    public string dataPath;
    private string filename;
    public Material matVertex;

    private List<LabeledData> dataPoints; //All read data from the file
    private List<LabeledData> currentDataPoints; //All currently selected data
    private List<GameObject> loadedDataSets; //We can have many datasets at one time (this functionality should be moved out)
    GameObject currentDataSet; //The latest spawned dataset

    public bool countLines = false;
    public int numPoints;
    public int numPointGroups;
    private int limitPoints = 65000;

    private Vector3[] points;
    private Vector3 minValue;

    public GameObject spaceManager;

    public float scale = 1;
    public float offSet = 300; //Sometimes the data have a specific offset
    //TODO instead of setting offset yourself, try to normalize so it fits into 0 in an option on the menu
    public bool invertYZ = false;

    private float farAwayNegX;
    private float farAwayNegY;
    private float farAwayNegZ;
    private float farAwayPosX;
    private float farAwayPosY;
    private float farAwayPosZ;
    private bool firstData = true;
    private int numLines;

    private float[] accumulatedData;
    public float[] avgData;
    public float[] maxData;
    public float[] minData;
    public float[] topPercentile;
    public float[] minPercentile;
    public float[] avgPercentile;
    private int featureLength;

    public string[] labelNames;
    public string[] unitNames;

    public int maxValueForGameObjects = 250;
    private float[] maxSizeTotal;

    public List<GraphHandler> graphs;
    private Dictionary<Vector3, Boolean> selected;

    // PointCloud
    private GameObject pointCloud;


    public float[] DataEdges()
    {
        float[] edges = new float[6];
        edges[0] = farAwayPosX;
        edges[1] = farAwayNegX;
        edges[2] = farAwayPosY;
        edges[3] = farAwayNegY;
        edges[4] = farAwayPosZ;
        edges[5] = farAwayNegY;
        return edges;
    }

    void Awake()
    {
        if (dataLoader == null)
            dataLoader = this;
    }

    void Start()
    {
        filename = Path.GetFileName(dataPath);
        dataPoints = new List<LabeledData>();         
        loadedDataSets = new List<GameObject>();

        loadScene();
    }


    void loadScene()
    {
        string dataPathEx = dataPath + ".off";  //TODO call file .dat
#if UNITY_EDITOR
        if (File.Exists(Path.Combine(Application.dataPath, dataPathEx)))
            StartCoroutine("loadData", dataPathEx);
        else
            Debug.Log("File '" + dataPath + "' could not be found");
#else
         if (File.Exists(Application.dataPath + "/" + "starparticles.001.off"))
            StartCoroutine("loadData", Application.dataPath + "/" + "starparticles.001.off");
#endif
    }

    //Reads in the data from a file and creates an initial mesh
    IEnumerator loadData(string dPath)
    {
        LoadInData(dPath);
        CalculateAverageData();
        CalculateStdAndPercentiles(currentDataPoints);
        SetEdgePlayerPrefs();

        pointCloudParts = new List<GameObject>();
        numPointGroups = Mathf.CeilToInt(numLines * 1.0f / limitPoints * 1.0f);

        pointCloud = CreateDataGameObject(filename);
        spaceManager.GetComponent<PlayerParts>().dataPoints = pointCloud;
        currentDataSet = pointCloud;

        for (int i = 0; i < numPointGroups - 1; i++)
        {
            InstantiateMesh(i, limitPoints);
            if (i % 10 == 0)
            {
                guiText = i.ToString() + " out of " + numPointGroups.ToString() + " PointGroups loaded";
                yield return null;
            }
        }
        InstantiateMesh(numPointGroups - 1, numLines - (numPointGroups - 1) * limitPoints);

        foreach (GameObject group in pointCloudParts)
        {
            group.transform.parent = pointCloud.transform;
        }
        //pointCloud.transform.rotation = Quaternion.Euler(90f, 0f, 0f);
        //UpdateScreenData(); 

        loaded = true;
        ActionBuffer.actionBufferInstance.setManipulationAction("data");
        yield return null;
    }

    bool firstSelection = false;
    public void resetSelected()
    {
        selected = new Dictionary<Vector3, bool>();
        firstSelection = true;
    }

    private void SetEdgePlayerPrefs()
    {
        PlayerPrefs.SetFloat("farAwayPosX", farAwayPosX);
        PlayerPrefs.SetFloat("farAwayNegX", farAwayNegX);
        PlayerPrefs.SetFloat("farAwayPosY", farAwayPosY);
        PlayerPrefs.SetFloat("farAwayNegY", farAwayNegY);
        PlayerPrefs.SetFloat("farAwayPosZ", farAwayPosZ);
        PlayerPrefs.SetFloat("farAwayNegZ", farAwayNegZ);
    }

    private void ResetValues()
    {
        accumulatedData = new float[featureLength];
        minData = new float[featureLength];
        maxData = new float[featureLength];
        avgData = new float[featureLength];
        for (int i = 0; i < featureLength; i++)
        {
            minData[i] = float.MaxValue;
            maxData[i] = float.MinValue;
        }
    }

    private void LoadInData(string dPath)
    {
        // Load data from text file (no headers, line 1 is data with whitespaces)
        Debug.Log(Application.dataPath + "/" + "starparticles.001.off");
#if UNITY_EDITOR
        StreamReader sr = new StreamReader(Application.dataPath + "/" + dPath);
#else
        Debug.Log(Application.dataPath + "/" + "starparticles.001.off");
        StreamReader sr = new StreamReader(dPath);
        
        
#endif
        string[] buffer;
        string line;

        line = sr.ReadLine();
        buffer = line.Split(null);
        for (int fish = 0; fish < buffer.Length; fish++)
            Debug.Log(buffer[fish]);

        //HG: An unaccurate line counter (is bigger):
        numLines = (int)((sr.BaseStream.Length) / sr.ReadLine().Length);

        //HG: An unaccurate but slower counter:
        if (countLines)
        {
            numLines = 0;
            while (!sr.EndOfStream)
            {
                line = sr.ReadLine();
                numLines += 1;
            }
            numPoints = numLines;

#if UNITY_EDITOR
            sr = new StreamReader(Application.dataPath + "/" + dPath);
#else
            sr = new StreamReader(dPath);
            
#endif
        }

        points = new Vector3[numLines];
        minValue = new Vector3();
        
        int i = 0;
        dataPoints.Capacity = numLines;
        featureLength = (labelNames.Length); 
        ResetValues();
        unitNames = new string[buffer.Length];
        
        //Debug.Log(buffer.Length);
        for (i = 0; i < labelNames.Length; i++){
            if (i == 0)
                MenuHandler.Instance.graphVariableText.GetComponent<Text>().text = labelNames[i];
        }


        buffer = sr.ReadLine().Split(null);
        Debug.Log("featLength: " + featureLength);

        i = 0;
        string flag = "";
        int count = 0;
        while (!sr.EndOfStream && i < numLines)
        {
            try
            {
                flag = "point";
                if (!invertYZ)
                    points[i] = new Vector3(float.Parse(buffer[1]) * scale - offSet, float.Parse(buffer[2]) * scale - offSet, float.Parse(buffer[3]) * scale - offSet);
                else
                    points[i] = new Vector3(float.Parse(buffer[1]) * scale - offSet, float.Parse(buffer[2]) * scale - offSet, float.Parse(buffer[3]) * scale - offSet);

                if (buffer.Length >= 6) //HG: Chrashing after !
                {
                    LabeledData d = new LabeledData();
                    d.Label = i.ToString();
                    d.Color = new Color(0, 0, 0);
                    d.Position = points[i];

                    
                    //Multidata
                    int j = 0;
                    d.features = new float[featureLength];
                    while (j < featureLength)
                    {
                        try {
                            
                            d.features[j] = float.Parse(buffer[j]);
                        } catch(Exception e)
                        {
                            Debug.Log("this failed: " + buffer[j]);
                            //We don't seem to like negative values in our document
                        }
                        
                        accumulatedData[j] += d.features[j];
                        j++;
                    }

                    if (count == 0)
                    {
                        for (j = 0; j < labelNames.Length; j++)
                            Debug.Log(j +": " + d.features[j]);
                    }
                    dataPoints.Add(d);
                    CheckIfEdgeData(d);
                    count++;
                }

            }
            catch (IOException)
            {
                Debug.Log("Reading data interrupted at line " + i.ToString() + " at " + flag);
            }
            numPoints = numLines;

            // GUI progress:
            progress = i * 1.0f / (numLines - 1) * 1.0f;
            if (i % Mathf.FloorToInt(numLines / 20) == 0)
            {
                guiText = i.ToString() + " out of " + numLines.ToString() + " loaded!!";
                //TODO removed yield will it break anything?
            }

            buffer = sr.ReadLine().Split(null);
            i += 1;

        }
        CalculateStdAndPercentiles(dataPoints);
        numLines = i;
        maxSizeTotal = new List<float>(topPercentile).ToArray();
        currentDataPoints = new List<LabeledData>(dataPoints);
    }

    public List<LabeledData> GetCurrentDataPoints()
    {
        return currentDataPoints;
    }

    public void ReLoadScene()
    {
        //TODO should try to make small optimization in which we just load already stored galaxy if not changes has been made
        StartCoroutine(ReLoadDataSet());
    }

    public void LoadDataFromBuffer(List<LabeledData> dataToLoad, Vector3 position, Quaternion rotation)
    {
        StartCoroutine(LoadData(dataToLoad, position, rotation));
    }

    IEnumerator LoadData(List<LabeledData> dataToLoad, Vector3 position, Quaternion rotation)
    {
        firstData = true;
        currentDataPoints = new List<LabeledData>();
        SetUpLoadParameters();
        int numLines = 0;
        int nbrDataPoints = 0;

        //TODO since this just takes the galaxy back to normal state we can skip this forloop and save the values at the beginning, and then just load the already made galaxy
        foreach (LabeledData d in dataToLoad)
        {
            CheckIfEdgeData(d);

            int j = 0;
            while (j < featureLength)
            {
                accumulatedData[j] += d.features[j];
                j++;
            }

            currentDataPoints.Add(d);
            nbrDataPoints++;
        }

        CalculateAverageData();

        if (currentDataPoints.Count != 0)
        {

            numLines = nbrDataPoints;
            if (currentDataSet)
            {
                loadedDataSets.Remove(currentDataSet);
                Destroy(currentDataSet);
            }
            StartCoroutine(CreateDataMesh(filename, numLines, position, rotation));
        }
        yield return null;
        //UpdateScreenData(); atode
        loaded = true;
    }

    /// <summary>
    /// Creates a new mesh out of a already loaded data set
    /// </summary>
    /// <returns></returns>
    IEnumerator ReLoadDataSet()
    {
        firstData = true;
        currentDataPoints = new List<LabeledData>();
        SetUpLoadParameters();
        int numLines = 0;
        int nbrDataPoints = 0;

        //TODO since this just takes the galaxy back to normal state we can skip this forloop and save the values at the beginning, and then just load the already made galaxy
        foreach (LabeledData d in dataPoints)
        {
            CheckIfEdgeData(d);

            int j = 0;
            while (j < featureLength)
            {
                accumulatedData[j] += d.features[j];
                j++;
            }
            
            currentDataPoints.Add(d);
            nbrDataPoints++;
        }

        CalculateAverageData();

        if (currentDataPoints.Count != 0)
        {

            numLines = nbrDataPoints; 
            if (currentDataSet)
            {
                loadedDataSets.Remove(currentDataSet);
                Destroy(currentDataSet);
            }
            StartCoroutine(CreateDataMesh(filename, numLines, Vector3.zero, Quaternion.identity));
        }
        ActionBuffer.actionBufferInstance.setManipulationAction("data");
        yield return null;
        //UpdateScreenData(); atode
        loaded = true;
    }

    /// <summary>
    /// Loads a new dataset in which the only data visible are inside the area which is the vector coordinates of the 6 sides of a box.
    /// </summary>
    /// <param name="area">Vector coordinates for the 6 sides of a box and the center position of the box</param>
    /// <param name="directions">Right, Up, and Forward vectors of the box</param>
    /// <param name="pos">Old position of the box</param>
    /// <param name="rot">Old rotation of the box</param>
    public void loadScene(Vector3 pos, Quaternion rot, GameObject tempBox)
    {
        StartCoroutine(loadData_Sizes(pos, rot, tempBox));
    }

    /// <summary>
    /// Loads a new dataset in which the only data visible are within the max and min value.
    /// </summary>
    /// <param name="maxAge">Max value for the data point</param>
    /// <param name="minAge">Min value for the data point</param>
    /// <param name="lessThan">Outdated to be removed</param>
    /// <param name="pos">Old position of dataset</param>
    /// <param name="rot">Old rotation of dataset</param>
    public void loadScene(bool lessThan, Vector3 pos, Quaternion rot)
    {
        StartCoroutine(loadData_MinMax(lessThan, pos, rot));
    }

    public void DimData(Vector3 pos, Quaternion rot)
    {
        StartCoroutine(DimAllData(pos, rot));
    }

    public GameObject dimObject;
    public bool dimDone = false;
    public List<LabeledData> tempDataPoints; 
    IEnumerator DimAllData(Vector3 pos, Quaternion rot)
    {
        dimDone = false;
        dataToDim = new List<LabeledData>();
        SetUpLoadParameters();
        int numLines = 0;
        int nbrDataPoints = 0;


        if (currentDataSet)
        {
            loadedDataSets.Remove(currentDataSet);
            Destroy(currentDataSet);
        }

        foreach (LabeledData d in currentDataPoints)
        {
            dataToDim.Add(d);
            nbrDataPoints++;
        }

        if (dataToDim.Count != 0)
        {

            numLines = nbrDataPoints; //HG: to be sure numlines is right for the following

            // Instantiate Point Groups
            numPointGroups = Mathf.CeilToInt(numLines * 1.0f / limitPoints * 1.0f);


            pointCloudParts = new List<GameObject>();
            dimObject = CreateDataGameObject(filename);

            for (int i = 0; i < numPointGroups - 1; i++)
            {
                InstantiateDimMesh(i, limitPoints);
                if (i % 10 == 0)
                {
                    guiText = i.ToString() + " out of " + numPointGroups.ToString() + " PointGroups loaded";
                    yield return null;
                }
            }
            InstantiateDimMesh(numPointGroups - 1, numLines - (numPointGroups - 1) * limitPoints);

            foreach (GameObject group in pointCloudParts)
            {
                group.transform.parent = dimObject.transform;
            }


        }
        dimObject.transform.position = pos;
        dimObject.transform.rotation = rot;
        dimDone = true;
        loaded = true;
        yield return null;
    }

    public void ApplyChanges()
    {
        Debug.Log("Applying changes");
        Vector3 pos = pointCloud.transform.position;
        Quaternion rot = pointCloud.transform.rotation;
        Debug.Log("pos: " + pos);
        Debug.Log("rot: " + rot);
        StartCoroutine(ApplyChangesRoutine(pos, rot));
    }
    IEnumerator ApplyChangesRoutine(Vector3 pos, Quaternion rot)
    {
        loaded = false;
        int numLines = 0;
        int nbrDataPoints = currentDataPoints.Count;

        if (graphs.Count != 0)
        {
            List<LabeledData> tmp = currentDataPoints;
            nbrDataPoints = 0;
            currentDataPoints = new List<LabeledData>();
            foreach (LabeledData d in tmp)
            {
                bool toBeVisualized = true;
                foreach (GraphHandler graph in graphs)
                {
                    if (!(d.features[graph.feature] <= graph.maxValue && d.features[graph.feature] >= graph.minValue))
                        toBeVisualized = false;
                }
                if (toBeVisualized)
                {
                    CheckIfEdgeData(d);
                    currentDataPoints.Add(d);
                    nbrDataPoints++;
                }
            }
        }
        
        if (currentDataPoints.Count < maxValueForGameObjects)
        {
            createGameObjects();
        } else {
            if (currentDataPoints.Count != 0)
            {

                numLines = nbrDataPoints;
                if (currentDataSet)
                {
                    loadedDataSets.Remove(currentDataSet);
                    Destroy(currentDataSet);
                }
                StartCoroutine(CreateDataMesh(filename, numLines, pos, rot));
            }

        }

        
        loaded = true;
        yield return null;
    }

    public void CircleSelection(Vector3 pos, float distance, Vector3 oldPos, Quaternion oldRot)
    {
        StartCoroutine(CircleSelectionRoutine(pos, distance, oldPos, oldRot));
    }

    IEnumerator CircleSelectionRoutine(Vector3 pos, float distance, Vector3 oldPos, Quaternion oldRot)
    {
        while (!loaded) { yield return null; }
        if (firstSelection)
        {
            
            while (!dimDone) { yield return null; }
            loaded = false;

            pointCloud = CreateDataGameObject(filename);           
            spaceManager.GetComponent<PlayerParts>().dataPoints = pointCloud;
            currentDataSet = pointCloud;
            firstSelection = false;
            firstData = true;
            SetUpLoadParameters();
            yield return null;
        }
        pointCloud.transform.position = Vector3.zero;
        pointCloud.transform.rotation = Quaternion.identity;

        
        dataToVisualize = new List<LabeledData>();
        /*if (Settings.Instance.dimStars)
            dataToDim = new List<LabeledData>();*/
        
        int numLines = 0;
        int nbrDataPoints = 0;

        foreach (LabeledData d in currentDataPoints)
        {

            if (Vector3.Distance(d.Position, pos) < distance && !selected.ContainsKey(d.Position)) 
            {
                CheckIfEdgeData(d);
                selected.Add(d.Position, true);
                nbrDataPoints++;
                tempDataPoints.Add(d);
                dataToVisualize.Add(d);
            }

        }

        //TODO have to accumulate for each frame we select
        CalculateAverageData();
        if (dataToVisualize.Count != 0)
        {

            numLines = nbrDataPoints; //HG: to be sure numlines is right for the following

            // Instantiate Point Groups

            pointCloudParts = new List<GameObject>();
            numPointGroups = Mathf.CeilToInt(numLines * 1.0f / limitPoints * 1.0f);

            for (int i = 0; i < numPointGroups - 1; i++)
            {
                InstantiateMeshVis(i, limitPoints);
                if (i % 10 == 0)
                {
                    guiText = i.ToString() + " out of " + numPointGroups.ToString() + " PointGroups loaded";
                    yield return null;
                }
            }
            InstantiateMeshVis(numPointGroups - 1, numLines - (numPointGroups - 1) * limitPoints);

            foreach (GameObject group in pointCloudParts)
            {
                group.transform.parent = pointCloud.transform;
            }


        }

        pointCloud.transform.rotation = oldRot;
        pointCloud.transform.position = oldPos;
        loaded = true;
        yield return null;
    }

    public void CircleSelectDone()
    {
        Destroy(dimObject);
        currentDataPoints = tempDataPoints;
        tempDataPoints = null;

        float sizeX = farAwayPosX - farAwayNegX;
        float sizeY = farAwayPosY - farAwayNegY;
        float sizeZ = farAwayPosZ - farAwayNegZ;
        Vector3 posi = Vector3.Lerp(new Vector3(farAwayPosX, farAwayPosY, farAwayPosZ), new Vector3(farAwayNegX, farAwayNegY, farAwayNegZ), 0.5f);
        pointCloud.GetComponent<BoxCollider>().center = posi;
        pointCloud.GetComponent<BoxCollider>().size = new Vector3(sizeX, sizeY, sizeZ);
        Debug.Log("farAwayPosX: " +farAwayPosX);
        Debug.Log("farAwayPosY: " + farAwayPosY);
        Debug.Log("farAwayPosZ: " + farAwayPosZ);
        //ApplyChanges();
        if (currentDataPoints.Count < maxValueForGameObjects)
            createGameObjects();
    }
    public void CircleSelectStart(Vector3 pos, Quaternion rot)
    {
        resetSelected();
        tempDataPoints = new List<LabeledData>();
        DimData(pos, rot);
        firstData = true;
    }

    IEnumerator loadData_Sizes(Vector3 oldPos, Quaternion oldRot, GameObject tempBox)
    {
        firstData = true;
        dataToVisualize = new List<LabeledData>();
        if (Settings.Instance.dimStars)
            dataToDim = new List<LabeledData>();
        SetUpLoadParameters();
        int numLines = 0;
        int nbrDataPoints = 0;

        foreach (LabeledData d in currentDataPoints)
        {

            Vector3 localPos = tempBox.transform.InverseTransformPoint(d.Position);
            if (Mathf.Abs(localPos.x) < 0.5f && Mathf.Abs(localPos.y) < 0.5f && Mathf.Abs(localPos.z) < 0.5f)
            {
                CheckIfEdgeData(d);
                dataToVisualize.Add(d);
                nbrDataPoints++;
            } else if (Settings.Instance.dimStars)
            {
                dataToDim.Add(d);
                nbrDataPoints++;
            }

        }

        CalculateAverageData();

        if (nbrDataPoints < maxValueForGameObjects)
        {
            currentDataPoints = dataToVisualize;
            createGameObjects();

        } else
        {
            if (dataToVisualize.Count != 0)
            {

                numLines = nbrDataPoints; //HG: to be sure numlines is right for the following

                // Instantiate Point Groups
                numPointGroups = Mathf.CeilToInt(numLines * 1.0f / limitPoints * 1.0f);
                if (currentDataSet)
                {
                    loadedDataSets.Remove(currentDataSet);
                    Destroy(currentDataSet);
                }

                pointCloudParts = new List<GameObject>();
                numPointGroups = Mathf.CeilToInt(numLines * 1.0f / limitPoints * 1.0f);

                pointCloud = CreateDataGameObject(filename);
                spaceManager.GetComponent<PlayerParts>().dataPoints = pointCloud;
                currentDataSet = pointCloud;

                for (int i = 0; i < numPointGroups - 1; i++)
                {
                    InstantiateMeshVis(i, limitPoints);
                    if (i % 10 == 0)
                    {
                        guiText = i.ToString() + " out of " + numPointGroups.ToString() + " PointGroups loaded";
                        yield return null;
                    }
                }
                InstantiateMeshVis(numPointGroups - 1, numLines - (numPointGroups - 1) * limitPoints);

                foreach (GameObject group in pointCloudParts)
                {
                    group.transform.parent = pointCloud.transform;
                }


            }

            pointCloud.transform.rotation = oldRot;
            pointCloud.transform.position = oldPos;
            ActionBuffer.actionBufferInstance.setManipulationAction("data");
            currentDataPoints = dataToVisualize;
        }
            
        loaded = true;
        yield return null;
    }

    List<LabeledData> dataToDim;
    List<LabeledData> dataToVisualize;
    IEnumerator loadData_MinMax(bool lessThan, Vector3 pos, Quaternion rot)
    {
        ResetValues();
        firstData = true;
        List<LabeledData> tempDataPoints = new List<LabeledData>();
        dataToVisualize = new List<LabeledData>();
        dataToDim = null;
        if (Settings.Instance.dimStars)
            dataToDim = new List<LabeledData>();
        SetUpLoadParameters();
        int numLines = 0;
        int nbrData = 0;
        foreach (LabeledData d in currentDataPoints)
        {
            bool toBeVisualized = true;
            foreach(GraphHandler graph in graphs)
            {
                if (!(d.features[graph.feature] <= graph.maxValue && d.features[graph.feature] >= graph.minValue))
                    toBeVisualized = false;


            }
            if (toBeVisualized)
            {
                CheckIfEdgeData(d);
                tempDataPoints.Add(d);
                nbrData++;
            } else if(Settings.Instance.dimStars)
            {
                dataToDim.Add(d);
                nbrData++;
            }
            
        }

        CalculateAverageData();

        if (nbrData < maxValueForGameObjects)
        {
            
            createGameObjects(tempDataPoints);
        } else
        {
            if (tempDataPoints.Count != 0)
            {
                numLines = nbrData; //HG: to be sure numlines is right for the following
                dataToVisualize = tempDataPoints;
                CalculateStdAndPercentiles(dataToVisualize);
                // Instantiate Point Groups
                numPointGroups = Mathf.CeilToInt(numLines * 1.0f / limitPoints * 1.0f);
                if (currentDataSet)
                {
                    loadedDataSets.Remove(currentDataSet);
                    Destroy(currentDataSet);
                }

                currentDataSet = pointCloud;
                StartCoroutine(CreateDataMeshVis(filename, numLines, pos, rot));

            }
           
        }

        //UpdateScreenData();
        loaded = true;
        ActionBuffer.actionBufferInstance.setManipulationAction("data");
        yield return null;
    }

    public GameObject GetCurrentDataSet()
    {
        return currentDataSet;
    }

    public void SetCurrentDataset(GameObject currDataset)
    {
        currentDataSet = currDataset;
    }

    private void CalculateAverageData()
    {
        for (int i = 0; i < accumulatedData.Length; i++)
        {
            avgData[i] = accumulatedData[i] / currentDataPoints.Count;
        }
    }

    static int SortByValue(LabeledData d1, LabeledData d2)
    {
        return d1.features[0].CompareTo(d2.features[0]);
    }

    private void CalculateStdAndPercentiles(List<LabeledData> datas)
    {
        datas.Sort(SortByValue);
        minPercentile = new float[featureLength];
        topPercentile = new float[featureLength];
        avgPercentile = new float[featureLength];
        for (int i = 0; i < featureLength; i++)
        {
            minPercentile[i] = datas[(int)(datas.Count * 0.1f)].features[i];
            topPercentile[i] = datas[(int)(datas.Count * 0.9f)].features[i];
            avgPercentile[i] = datas[(int)(datas.Count * 0.5f)].features[i];
        }
    }

    private void SetUpLoadParameters()
    {
        farAwayNegX = 0;
        farAwayNegY = 0;
        farAwayNegZ = 0;
        farAwayPosX = 0;
        farAwayPosY = 0;
        farAwayPosZ = 0;

        //Multidata
        for (int i = 0; i < featureLength; i++)
        {
            minData[i] = float.MaxValue;
            maxData[i] = float.MinValue;
            avgData[i] = 0;
            accumulatedData[i] = 0;
        }

    }

    public void AddNewDataSet()
    {
        Debug.Log("adding new dataset");
        currentDataSet = null;
        ReLoadScene();
        loadedDataSets.Add(currentDataSet);
    }

    public void DeleteDataSet()
    {
        if (spaceManager.GetComponent<SpaceUtilities>().CurrentDataset())
        {
            loadedDataSets.Remove(spaceManager.GetComponent<SpaceUtilities>().CurrentDataset());
            Destroy(currentDataSet);
        }
    }

    private void createGameObjects()
    {
        Vector3 pos = currentDataSet.transform.position;
        Quaternion rot = currentDataSet.transform.rotation;
        Vector3 colSize = currentDataSet.GetComponent<BoxCollider>().size;

        if (currentDataSet)
        {
            loadedDataSets.Remove(currentDataSet);
            Destroy(currentDataSet);
        }

        currentDataSet = CreateDataGameObject("PointCloud GameObjs");
        GradientManager gradMan = spaceManager.GetComponent<GradientManager>();
        int featIndex = spaceManager.GetComponent<SpaceUtilities>().currentVariableForGraph;
        foreach (LabeledData d in currentDataPoints)
        {
            GameObject tmp = GameObject.CreatePrimitive(PrimitiveType.Sphere);
            //Stell mass divided with max to normalize and then divided with 10 to get better shapes
            tmp.transform.localScale = (new Vector3 (d.features[0], d.features[0], d.features[0]) / maxSizeTotal[0]) / 50; //Stellar mass
            if (tmp.transform.localScale.x > 0.1)
                tmp.transform.localScale = new Vector3(0.1f, 0.1f, 0.1f);
            tmp.GetComponent<Renderer>().material.color = gradMan.getColor(d.features[featIndex] / maxData[featIndex]);
            tmp.transform.position = d.Position;
            tmp.transform.parent = currentDataSet.transform;
        }
        currentDataSet.transform.position = pos;
        currentDataSet.transform.rotation = rot;
    }

    private void createGameObjects(List<LabeledData> tmpSet)
    {
        Vector3 pos = currentDataSet.transform.position;
        Quaternion rot = currentDataSet.transform.rotation;
        Vector3 colSize = currentDataSet.GetComponent<BoxCollider>().size;

        if (currentDataSet)
        {
            loadedDataSets.Remove(currentDataSet);
            Destroy(currentDataSet);
        }

        currentDataSet = CreateDataGameObject("PointCloud GameObjs");
        GradientManager gradMan = spaceManager.GetComponent<GradientManager>();
        int featIndex = spaceManager.GetComponent<SpaceUtilities>().currentVariableForGraph;
        foreach (LabeledData d in tmpSet)
        {
            GameObject tmp = GameObject.CreatePrimitive(PrimitiveType.Sphere);
            //Stell mass divided with max to normalize and then divided with 10 to get better shapes
            tmp.transform.localScale = (new Vector3(d.features[0], d.features[0], d.features[0]) / maxSizeTotal[0]) / 50; //Stellar mass
            if (tmp.transform.localScale.x > 0.1)
                tmp.transform.localScale = new Vector3(0.1f, 0.1f, 0.1f);
            tmp.GetComponent<Renderer>().material.color = gradMan.getColor(d.features[featIndex] / maxData[featIndex]);
            tmp.transform.position = d.Position;
            tmp.transform.parent = currentDataSet.transform;
        }
        currentDataSet.transform.position = pos;
        currentDataSet.transform.rotation = rot;
    }

    public List<LabeledData> GetDataSet()
    {
        if (dataPoints != null)
        {
            return currentDataPoints;
        }
        return dataPoints;
    }



    List<GameObject> pointCloudParts;
    IEnumerator CreateDataMesh(string filename, int numLines, Vector3 pos, Quaternion rot)
    {
        // Instantiate Point Groups
        pointCloudParts = new List<GameObject>();
        numPointGroups = Mathf.CeilToInt(numLines * 1.0f / limitPoints * 1.0f);

        pointCloud = CreateDataGameObject(filename);
        spaceManager.GetComponent<PlayerParts>().dataPoints = pointCloud;
        currentDataSet = pointCloud;

        for (int i = 0; i < numPointGroups - 1; i++)
        {
            InstantiateMesh(i, limitPoints);
            if (i % 10 == 0)
            {
                guiText = i.ToString() + " out of " + numPointGroups.ToString() + " PointGroups loaded";
                yield return null;
            }
        }
        InstantiateMesh(numPointGroups - 1, numLines - (numPointGroups - 1) * limitPoints);

        foreach (GameObject group in pointCloudParts)
        {
            group.transform.parent = pointCloud.transform;
        }
        pointCloud.transform.position = pos;
        pointCloud.transform.rotation = rot;

    }

    void InstantiateDimMesh(int meshInd, int nPoints)
    {
        // Create Mesh
        GameObject pointGroup = new GameObject(filename + meshInd);
        pointGroup.AddComponent<MeshFilter>();
        pointGroup.AddComponent<MeshRenderer>();
        pointGroup.GetComponent<Renderer>().material = matVertex;

        pointGroup.GetComponent<MeshFilter>().mesh = CreateDimMesh(meshInd, nPoints, limitPoints);
        pointCloudParts.Add(pointGroup);


    }

    Mesh CreateDimMesh(int id, int nPoints, int limitPoints)
    {

        Mesh mesh = new Mesh();

        Vector3[] myPoints = new Vector3[nPoints];
        int[] indecies = new int[nPoints];
        Color[] myColors = new Color[nPoints];

        for (int i = 0; i < nPoints; ++i)
        {
            int dataIndex = id * limitPoints + i;
            myPoints[i] = dataToDim[dataIndex].Position;
            indecies[i] = i;
            //Painted after first data point to begin with
            myColors[i] = new Color(1f, 1f, 1f, 0.05f);
        }

        mesh.vertices = myPoints;
        mesh.colors = myColors;
        mesh.SetIndices(indecies, MeshTopology.Points, 0);
        mesh.uv = new Vector2[nPoints];
        mesh.normals = new Vector3[nPoints];

        return mesh;
    }

    void InstantiateMesh(int meshInd, int nPoints)
    {
        // Create Mesh
        GameObject pointGroup = new GameObject(filename + meshInd);
        pointGroup.AddComponent<MeshFilter>();
        pointGroup.AddComponent<MeshRenderer>();
        pointGroup.GetComponent<Renderer>().material = matVertex;

        pointGroup.GetComponent<MeshFilter>().mesh = CreateMesh(meshInd, nPoints, limitPoints);
        pointCloudParts.Add(pointGroup);


    }

    Mesh CreateMesh(int id, int nPoints, int limitPoints)
    {

        Mesh mesh = new Mesh();

        Vector3[] myPoints = new Vector3[nPoints];
        int[] indecies = new int[nPoints];
        Color[] myColors = new Color[nPoints];

        for (int i = 0; i < nPoints; ++i)
        {
            int dataIndex = id * limitPoints + i;
            if (dataToDim != null && currentDataPoints.Count <= dataIndex)
            {
                myPoints[i] = dataToDim[dataIndex - currentDataPoints.Count].Position;
                indecies[i] = i;
                myColors[i] = new Color(0.5f, 0.5f, 0.5f, 0.05f);
            } else
            {
                myPoints[i] = currentDataPoints[dataIndex].Position;
                indecies[i] = i;
                //Painted after first data point to begin with
                int featIndex = spaceManager.GetComponent<SpaceUtilities>().currentVariableForGraph;
                myColors[i] = spaceManager.GetComponent<GradientManager>().getColor((dataPoints[id * limitPoints + i].features[featIndex] - minPercentile[featIndex]) / (topPercentile[featIndex] - minPercentile[featIndex]));
                currentDataPoints[i].Color = myColors[i];
            }
               
        }

        mesh.vertices = myPoints;
        mesh.colors = myColors;
        mesh.SetIndices(indecies, MeshTopology.Points, 0);
        mesh.uv = new Vector2[nPoints];
        mesh.normals = new Vector3[nPoints];

        return mesh;
    }

    IEnumerator CreateDataMeshVis(string filename, int numLines, Vector3 pos, Quaternion rot)
    {
        // Instantiate Point Groups
        pointCloudParts = new List<GameObject>();
        numPointGroups = Mathf.CeilToInt(numLines * 1.0f / limitPoints * 1.0f);

        pointCloud = CreateDataGameObject(filename);
        spaceManager.GetComponent<PlayerParts>().dataPoints = pointCloud;
        currentDataSet = pointCloud;

        for (int i = 0; i < numPointGroups - 1; i++)
        {
            InstantiateMeshVis(i, limitPoints);
            if (i % 10 == 0)
            {
                guiText = i.ToString() + " out of " + numPointGroups.ToString() + " PointGroups loaded";
                yield return null;
            }
        }
        InstantiateMeshVis(numPointGroups - 1, numLines - (numPointGroups - 1) * limitPoints);

        foreach (GameObject group in pointCloudParts)
        {
            group.transform.parent = pointCloud.transform;
        }
        pointCloud.transform.position = pos;
        pointCloud.transform.rotation = rot;

    }

    void InstantiateMeshVis(int meshInd, int nPoints)
    {
        // Create Mesh
        GameObject pointGroup = new GameObject(filename + meshInd);
        pointGroup.AddComponent<MeshFilter>();
        pointGroup.AddComponent<MeshRenderer>();
        pointGroup.GetComponent<Renderer>().material = matVertex;

        pointGroup.GetComponent<MeshFilter>().mesh = CreateMeshVis(meshInd, nPoints, limitPoints);
        pointCloudParts.Add(pointGroup);


    }

    Mesh CreateMeshVis(int id, int nPoints, int limitPoints)
    {

        Mesh mesh = new Mesh();

        Vector3[] myPoints = new Vector3[nPoints];
        int[] indecies = new int[nPoints];
        Color[] myColors = new Color[nPoints];

        for (int i = 0; i < nPoints; ++i)
        {
            int dataIndex = id * limitPoints + i;
            if (Settings.Instance.dimStars && dataToVisualize.Count <= dataIndex)
            {
                myPoints[i] = dataToDim[dataIndex - dataToVisualize.Count].Position;
                indecies[i] = i;
                myColors[i] = new Color(1f, 1f, 1f, 0.05f);
            }
            else
            {
                myPoints[i] = dataToVisualize[dataIndex].Position;
                indecies[i] = i;
                //Painted after first data point to begin with
                int featIndex = spaceManager.GetComponent<SpaceUtilities>().currentVariableForGraph;
                myColors[i] = spaceManager.GetComponent<GradientManager>().getColor((dataPoints[id * limitPoints + i].features[featIndex] - minPercentile[featIndex]) / (topPercentile[featIndex] - minPercentile[featIndex]));
                dataToVisualize[i].Color = myColors[i];
            }

        }

        mesh.vertices = myPoints;
        mesh.colors = myColors;
        mesh.SetIndices(indecies, MeshTopology.Points, 0);
        mesh.uv = new Vector2[nPoints];
        mesh.normals = new Vector3[nPoints];

        return mesh;
    }

    public bool changingTrans = false;
    public void ChangeTransparency(float a)
    {
        StartCoroutine(ChangeTransparencyNum(a));

    }
    public IEnumerator ChangeTransparencyNum(float a)
    {

        if (!changingTrans)
        {
            changingTrans = true;
            for (int n = 0; n < currentDataSet.transform.childCount; n++)
            {
                Mesh tmp = currentDataSet.transform.GetChild(n).GetComponent<MeshFilter>().mesh;
                int len = tmp.colors.Length;
                Color[] myColors = new Color[len];
                int i = 0;
                while (i < len)
                {
                    myColors[i] = spaceManager.GetComponent<GradientManager>().getColor((dataPoints[n * limitPoints + i].features[0] - minPercentile[0]) / (topPercentile[0] - minPercentile[0]));
                    myColors[i].a = a;
                    i++;
                }

                tmp.colors = myColors;
                yield return null;
            }
            changingTrans = false;
        } 
        
        yield return null;
        

    }

    /// <summary>
    /// Check if the data point is of extreme values. For example if the data point is furthest away in a direction.
    /// </summary>
    /// <param name="filename">Data point</param>
    void CheckIfEdgeData(LabeledData data)
    {
        if (firstData)
        {

            farAwayNegX = data.Position.x;
            farAwayPosX = data.Position.x;
            farAwayNegY = data.Position.y;
            farAwayPosY = data.Position.y;
            farAwayNegZ = data.Position.z;
            farAwayPosZ = data.Position.z;
            firstData = false;
        }
        
        //Multidata
        for (int i = 0; i < data.features.Length; i++)
        {
            
            if (data.features[i] > maxData[i])
            {
                maxData[i] = data.features[i];
            }

            if (data.features[i] < minData[i])
            {
                minData[i] = data.features[i];

            }

            accumulatedData[i] += data.features[i];
        }

        if (data.Position.x < farAwayNegX)
        {
            farAwayNegX = data.Position.x;
        }
        if (data.Position.x > farAwayPosX)
        {
            farAwayPosX = data.Position.x;
        }
        if (data.Position.y < farAwayNegY)
        {
            farAwayNegY = data.Position.y;
        }
        if (data.Position.y > farAwayPosY)
        {
            farAwayPosY = data.Position.y;
        }
        if (data.Position.z < farAwayNegZ)
        {
            farAwayNegZ = data.Position.z;
        }
        if (data.Position.z > farAwayPosZ)
        {
            farAwayPosZ = data.Position.z;
        }
    }

    /// <summary>
    /// Creates the base dataset object and adds all relevant settings.
    /// </summary>
    /// <param name="filename">Filename of the loaded file</param>
    /// <returns name="pointCloud">The base dataset</returns>
    GameObject CreateDataGameObject(string filename)
    {
        float sizeX = farAwayPosX - farAwayNegX;
        float sizeY = farAwayPosY - farAwayNegY;
        float sizeZ = farAwayPosZ - farAwayNegZ;

        pointCloud = new GameObject(filename);
        currentDataSet = pointCloud;
        loadedDataSets.Add(currentDataSet);

        pointCloud.layer = 11;
        pointCloud.AddComponent<Rigidbody>();
        pointCloud.GetComponent<Rigidbody>().isKinematic = true;
        pointCloud.GetComponent<Rigidbody>().useGravity = false;
        pointCloud.tag = "Star";

        pointCloud.AddComponent<BoxCollider>();
        Vector3 pos = Vector3.Lerp(new Vector3(farAwayPosX, farAwayPosY, farAwayPosZ), new Vector3(farAwayNegX, farAwayNegY, farAwayNegZ), 0.5f);
        pointCloud.GetComponent<BoxCollider>().center = pos;
        pointCloud.GetComponent<BoxCollider>().size = new Vector3(sizeX, sizeY, sizeZ);
        spaceManager.GetComponent<PlayerParts>().dataPoints = pointCloud;
        return pointCloud;
    }
}
