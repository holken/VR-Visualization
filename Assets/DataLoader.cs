using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using System.IO;
using System;
using System.Text;

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
    private int featureLength;

    public string[] labelNames;
    public string[] unitNames;

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
        if (File.Exists(Path.Combine(Application.dataPath, dataPathEx)))
            StartCoroutine("loadData", dataPathEx);
        else
            Debug.Log("File '" + dataPath + "' could not be found");
    }

    //Reads in the data from a file and creates an initial mesh
    IEnumerator loadData(string dPath)
    {
        LoadInData(dPath);
        CalculateAverageData();
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
        pointCloud.transform.rotation = Quaternion.Euler(90f, 0f, 0f);
        //UpdateScreenData(); 

        loaded = true;
        ActionBuffer.actionBufferInstance.setManipulationAction("data");
        yield return null;
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

    private void LoadInData(string dPath)
    {
        // Load data from text file (no headers, line 1 is data with whitespaces)
        StreamReader sr = new StreamReader(Application.dataPath + "/" + dPath);
        string[] buffer;
        string line;

        line = sr.ReadLine();
        buffer = line.Split(null);

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

            sr = new StreamReader(Application.dataPath + "/" + dPath);
        }

        points = new Vector3[numLines];
        minValue = new Vector3();

        int i = 0;
        dataPoints.Capacity = numLines;
        featureLength = (buffer.Length - 3); //Removes 3 first 
        accumulatedData = new float[featureLength];
        minData = new float[featureLength]; 
        maxData = new float[featureLength];
        avgData = new float[featureLength];
        labelNames = new string[buffer.Length];
        unitNames = new string[buffer.Length];
        for (i = 0; i < featureLength; i++)
        {
            minData[i] = float.MaxValue;
            maxData[i] = float.MinValue;
        }
        Debug.Log(buffer.Length-1);
        for (i = 0; i <= buffer.Length-1; i++){
            Debug.Log(i);
            labelNames[i] = buffer[i];
            Debug.Log(labelNames[i]);
        }
        buffer = sr.ReadLine().Split(null);
        buffer = sr.ReadLine().Split(null);
        for (i = 0; i < buffer.Length; i++)
        {
            unitNames[i] = buffer[i];
            Debug.Log(unitNames[i]);
        }
        buffer = sr.ReadLine().Split(null);


        Debug.Log("featLength: " + featureLength);

        i = 0;
        string flag = "";
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
                    int j = 1;
                    d.features = new float[featureLength];
                    while (j < featureLength)
                    {
                        d.features[0] = float.Parse(buffer[0]);
                        try {
                            d.features[j] = float.Parse(buffer[3 + j]);
                        } catch(Exception e)
                        {
                            //We don't seem to like negative values in our document
                        }
                        
                        accumulatedData[j] += d.features[j];
                        j++;
                    }
                    dataPoints.Add(d);
                    CheckIfEdgeData(d);
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
        numLines = i;
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
            StartCoroutine(CreateDataMesh(filename, numLines));
        }
        pointCloud.transform.position = position;
        pointCloud.transform.rotation = rotation;
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
            StartCoroutine(CreateDataMesh(filename, numLines));
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
    public void loadScene(float maxAge, float minAge, bool lessThan, Vector3 pos, Quaternion rot)
    {
        Debug.Log("maxAge: " + maxAge + " minAge: " + minAge);
        StartCoroutine(loadData_MinMax(maxAge, minAge, lessThan, pos, rot));
    }

    /*
     * This method let's us show only parts of the galaxy.
     */
    IEnumerator loadData_Sizes(Vector3 oldPos, Quaternion oldRot, GameObject tempBox)
    {
        firstData = true;
        currentDataPoints = new List<LabeledData>();
        SetUpLoadParameters();
        int numLines = 0;
        int nbrDataPoints = 0;

        foreach (LabeledData d in dataPoints)
        {

            Vector3 localPos = tempBox.transform.InverseTransformPoint(d.Position);
            if (Mathf.Abs(localPos.x) < 0.5f && Mathf.Abs(localPos.y) < 0.5f && Mathf.Abs(localPos.z) < 0.5f)
            {
                CheckIfEdgeData(d);
                currentDataPoints.Add(d);
                nbrDataPoints++;
            }

            /*if (tempBox.GetComponent<Collider>().bounds.Contains(d.Position))
            {
                CheckIfEdgeData(d);
                currentDataPoints.Add(d);
                nbrDataPoints++;
            }*/

        }

        CalculateAverageData();
        if (currentDataPoints.Count != 0)
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


        }

        pointCloud.transform.rotation = oldRot;
        pointCloud.transform.position = oldPos;
        ActionBuffer.actionBufferInstance.setManipulationAction("data");
        //UpdateScreenData();
        loaded = true;
        yield return null;
    }

    IEnumerator loadData_MinMax(float maxData, float minData, bool lessThan, Vector3 pos, Quaternion rot)
    {

        firstData = true;
        List<LabeledData> tempDataPoints = new List<LabeledData>();
        SetUpLoadParameters();
        int numLines = 0;
        int nbrData = 0;

        foreach (LabeledData d in currentDataPoints)
        {

            if (lessThan && d.features[0] <= maxData && d.features[0] >= minData)
            {
                CheckIfEdgeData(d);
                tempDataPoints.Add(d);
                nbrData++;
            }
        }

        CalculateAverageData();

        if (tempDataPoints.Count != 0)
        {
            numLines = nbrData; //HG: to be sure numlines is right for the following

            // Instantiate Point Groups
            numPointGroups = Mathf.CeilToInt(numLines * 1.0f / limitPoints * 1.0f);
            if (currentDataSet)
            {
                loadedDataSets.Remove(currentDataSet);
                Destroy(currentDataSet);
            }
            
            currentDataSet = pointCloud;
            StartCoroutine(CreateDataMesh(filename, numLines));

        }
        pointCloud.transform.rotation = Quaternion.Euler(rot.eulerAngles.x, rot.eulerAngles.y, rot.eulerAngles.z);
        pointCloud.transform.position = pos;
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
            maxData[i] = 0;
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

    public List<LabeledData> GetDataSet()
    {
        if (dataPoints == null)
        {
            return dataPoints;
        }
        return currentDataPoints;
    }

    List<GameObject> pointCloudParts;
    IEnumerator CreateDataMesh(string filename, int numLines)
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
        pointCloud.transform.rotation = Quaternion.Euler(90f, 0f, 0f);

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
            myPoints[i] = currentDataPoints[dataIndex].Position;
            indecies[i] = i;
            
            //Painted after first data point to begin with
            myColors[i] = spaceManager.GetComponent<GradientManager>().getColor((dataPoints[id * limitPoints + i].features[0] - minData[0]) / (maxData[0] - minData[0]));
            currentDataPoints[i].Color = myColors[i];
        }

        mesh.vertices = myPoints;
        mesh.colors = myColors;
        mesh.SetIndices(indecies, MeshTopology.Points, 0);
        mesh.uv = new Vector2[nPoints];
        mesh.normals = new Vector3[nPoints];

        return mesh;
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
            else if (data.features[i] < minData[i])
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
