using UnityEngine;
using System.Collections;
using System.IO;
using System.Collections.Generic;
using System.Text.RegularExpressions;
using UnityEngine.UI;
using System;

namespace pointcloud_objects {
    public class PointCloudManagerNet : MonoBehaviour {

        // File
        public string dataPath;
        private string filename;
        public bool hasAge = true;
        public Material matVertex;
        public float ageBreak = 227583032.617F;
        // GUI
        private float progress = 0;
        private string guiText;
        private bool loaded = false;

        // PointCloud
        private GameObject pointCloud;
        private GameObject pointCloudSel;

        public float scale = 1;
        public float offSet = 300;
        public bool invertYZ = false;
        public bool forceReload = false;
        public bool countLines = false;

        public int numPoints;
        public int numPointGroups;
        private int limitPoints = 65000;


        //HG01..
        public float starAge = 0.0F;
        private float starAgeLow = 10000000000.0F;
        private float starAgeHigh = 0.0F;
        private float starAgeMean = 0.0F;
        private float starAgeSum = 0.0F;

        public Color planetColor = new Color(0.1F, 0.1F, 0.7F);
        public float planetTransparency = 0.3F;
        public float planetScale = 0.01F;
        public int planetShowOneOutOf = 1000;
        public float planetVolumeScale = 1.0F;

        public GameObject sun;
        private GameObject markerRoot;

        //..HG01

        private Vector3[] points;
        private float[] ages;
        private Color[] colors;
        private Vector3 dataColor;
        //private float r, g, b;
        //private Color pointRGB = new Color(0.9F, 0.9F, 0.9F);
        private Vector3 minValue;

        // Planet
        //private celestial_body body;
        //private List<celestial_body> bodys;
        private List<GameObject> planets;
        private List<LabeledPlanet> planetData;

        private bool hasPlanets = false;     //HG03

        private Color transblue = new Color(0.5F, 0.0F, 0.9F);
        private Color transred = new Color(0.9F, 0.1F, 0.1F);
        private Color transtotal = new Color(0.0F, 0.0F, 0.0F);
        private Color pointRGB = new Color(0.9F, 0.9F, 0.9F);

        private float r, g, b;
        private float age_rgb = 227583032.617F;

        private RaycastHit hit;

        public Color c1 = Color.yellow;
        public Color c2 = Color.red;
        public int lengthOfLineRenderer = 2000;

        //TODO a list?
        private float farAwayNegX;
        private float farAwayNegY;
        private float farAwayNegZ;
        private float farAwayPosX;
        private float farAwayPosY;
        private float farAwayPosZ;
        private bool firstStar = true;
        private bool outsideBounds = true;
        private List<LabeledPlanet> selectedPlanets;
        GameObject currentGalaxy;

        public float oldestPlanet;
        public float youngestPlanet;
        public float averageAge;
        public GameObject spaceManager;
        float accumulatedPlanetAge = 0;

        public bool colorGrading;
        private List<LabeledPlanet> selectedForVisualisation;
        private List<Color> visualisationColors;

        private List<GameObject> currentDatasets;

        public GameObject netManager;
        public GameObject pointCloudPrefab;

        void Start() {
            // Create Resources folder
            createFolders();

            // Get Filename
            filename = Path.GetFileName(dataPath + ".off");
            planets = new List<GameObject>();
            planetData = new List<LabeledPlanet>();
            markerRoot = gameObject;  //HG: skip?            

            currentDatasets = new List<GameObject>();

            string dataPathEx = dataPath + ".off";  //TODO call file .dat
            if (File.Exists(Path.Combine(Application.dataPath, dataPathEx)))
                StartCoroutine(ReadDataFromFile(dataPathEx));
            else
                Debug.Log("File '" + dataPath + "' could not be found");
           
            Debug.Log("timetocallnet");
            //netManager.GetComponent<NetworkInteractions>().CreateDataSetAll();
        }

        void Update() {
            //HG01 test..
            if (!hasPlanets) {
                // TODO test typ:body.StartCoroutine("InitiatePlanets");
                InitiatePlanets(0, planetData.Count, planetShowOneOutOf);
            }

            int lenght = Input.inputString.Length;
            if (lenght > 0) {
                //Debug.Log("Pressed:   " + Input.inputString);
                string keypressedStr = Input.inputString.Substring(lenght - 1, 1);
                switch (keypressedStr) {
                    case "ö":
                        //pointPaint(1, 100);
                        InitiatePlanets(5000, 6000, 1);  //planetShowOneOutOf a Group of points as spheres
                        break;
                }
            }
        }

        //TODO borken
        public void AddNewDataSet() {
            currentGalaxy = null;
            //ReLoadScene(viewID);
            currentDatasets.Add(currentGalaxy);
        }

        /// <summary>
        /// Starts the load process of the galaxy again, deleting the current galaxy and loading in a new one by forcing a complete reload.
        /// </summary>
        public void ReLoadScene(int viewID) {
            //TODO should try to make small optimization in which we just load already stored galaxy if not changes has been made
            Debug.Log("reload");
            StartCoroutine(ReLoadGalaxy(viewID));
        }

        /// <summary>
        /// Loads a new dataset in which the only data visible are inside the area which is the vector coordinates of the 6 sides of a box.
        /// </summary>
        /// <param name="area">Vector coordinates for the 6 sides of a box and the center position of the box</param>
        /// <param name="directions">Right, Up, and Forward vectors of the box</param>
        /// <param name="pos">Old position of the box</param>
        /// <param name="rot">Old rotation of the box</param>
        public void loadScene(Vector3[] area, Vector3[] directions, Vector3 pos, Quaternion rot) {
            StartCoroutine(loadData_Galaxy_Sizes(area, directions, pos, rot));
        }

        /// <summary>
        /// Loads a new dataset by either loading a old mesh already created, or creating a new mesh from scratch.
        /// </summary>
        void loadScene() {
            // Check if the PointCloud was loaded previously
            bool inEditor = false;

            //TODO, we need to get rid of the dependency on the editor!
#if UNITY_EDITOR
            inEditor = true;
            if (!Directory.Exists(Application.dataPath + "/Resources/PointCloudMeshes/" + filename)) {

                UnityEditor.AssetDatabase.CreateFolder("Assets/Resources/PointCloudMeshes", filename);

                loadPointCloud();
            } else if (forceReload) {
                UnityEditor.FileUtil.DeleteFileOrDirectory(Application.dataPath + "/Resources/PointCloudMeshes/" + filename);
                UnityEditor.AssetDatabase.Refresh();
                UnityEditor.AssetDatabase.CreateFolder("Assets/Resources/PointCloudMeshes", filename);
                // Load data from raw ascii file and create meshed PointCloud
                loadPointCloud();
            } else {
                // Load stored and meshed PointCloud
                loadStoredMeshes();

            }         
#endif
            if (!inEditor) { //TODO, this was added because editor trouble, should work better in the future
                loadStoredMeshes();
            }

        }

        void loadPointCloud() {
            // Check what file exists
            string dataPathEx = dataPath + ".off";  //TODO call file .dat
            if (File.Exists(Path.Combine(Application.dataPath, dataPathEx)))
                StartCoroutine("loadData_Galaxy", dataPathEx);
            else
                Debug.Log("File '" + dataPath + "' could not be found");
        }

        // Load stored PointCloud
        void loadStoredMeshes() {

            Debug.Log("Using previously loaded PointCloud: " + filename);

            currentDatasets.Remove(currentGalaxy);
            Destroy(currentGalaxy);

            pointCloud = Instantiate(Resources.Load("PointCloudMeshes/" + filename)) as GameObject;
            spaceManager.GetComponent<PlayerParts>().dataPoints = pointCloud;
            loadStandardData();

            currentGalaxy = pointCloud;
            currentDatasets.Add(currentGalaxy);
            pointCloud.transform.parent = pointCloud.transform;
            UpdateScreenData();

            string dataPathEx = dataPath + ".off";  //TODO call file .dat
            if (File.Exists(Path.Combine(Application.dataPath, dataPathEx)))
                StartCoroutine("ReadDataFromFile", dataPathEx);
            else
                Debug.Log("File '" + dataPath + "' could not be found");


            loaded = true;
        }

        public void ReReadData() {
            string dataPathEx = dataPath + ".off";  //TODO call file .dat
            if (File.Exists(Path.Combine(Application.dataPath, dataPathEx)))
                StartCoroutine("ReadDataFromFile", dataPathEx);
            else
                Debug.Log("File '" + dataPath + "' could not be found");
        }

        // Start Coroutine of reading the points from the OFF file and creating the meshes
        IEnumerator loadOFF(string dPath) {

            // Read file
            StreamReader sr = new StreamReader(Application.dataPath + dPath);
            sr.ReadLine(); // OFF
            string[] buffer = sr.ReadLine().Split(); // nPoints, nFaces

            numPoints = int.Parse(buffer[0]);
            points = new Vector3[numPoints];
            colors = new Color[numPoints];
            minValue = new Vector3();

            for (int i = 0; i < numPoints; i++) {
                buffer = sr.ReadLine().Split();

                if (!invertYZ)
                    points[i] = new Vector3(float.Parse(buffer[0]) * scale, float.Parse(buffer[1]) * scale, float.Parse(buffer[2]) * scale);
                else
                    points[i] = new Vector3(float.Parse(buffer[0]) * scale, float.Parse(buffer[2]) * scale, float.Parse(buffer[1]) * scale);

                if (firstStar) {
                    farAwayNegX = points[i].x;
                    farAwayPosX = points[i].x;
                    farAwayNegY = points[i].y;
                    farAwayPosY = points[i].y;
                    farAwayNegZ = points[i].z;
                    farAwayPosZ = points[i].z;
                    firstStar = false;
                }

                if (points[i].x < farAwayNegX) {
                    farAwayNegX = points[i].x;
                }
                if (points[i].x > farAwayPosX) {
                    farAwayPosX = points[i].x;
                }
                if (points[i].y < farAwayNegY) {
                    farAwayNegY = points[i].y;
                }
                if (points[i].y > farAwayPosY) {
                    farAwayPosY = points[i].y;
                }
                if (points[i].z < farAwayNegZ) {
                    farAwayNegZ = points[i].z;
                }
                if (points[i].z > farAwayPosZ) {
                    farAwayPosZ = points[i].z;
                }

                if (buffer.Length >= 5) {
                    if (hasAge) {
                        ages[i] = float.Parse(buffer[3]);
                        if (ages[i] > ageBreak)
                            colors[i] = Color.cyan;
                        else
                            colors[i] = Color.blue;
                    } else
                        colors[i] = new Color(int.Parse(buffer[3]) / 255.0f, int.Parse(buffer[4]) / 255.0f, int.Parse(buffer[5]) / 255.0f);
                }

                // Relocate Points near the origin
                //calculateMin(points[i]);

                // GUI
                progress = i * 1.0f / (numPoints - 1) * 1.0f;
                if (i % Mathf.FloorToInt(numPoints / 20) == 0) {
                    guiText = i.ToString() + " out of " + numPoints.ToString() + " loaded";
                    yield return null;
                }
            }

            float sizeX = farAwayPosX - farAwayNegX;
            float sizeY = farAwayPosY - farAwayNegY;
            float sizeZ = farAwayPosZ - farAwayNegZ;

            // Instantiate Point Groups
            numPointGroups = Mathf.CeilToInt(numPoints * 1.0f / limitPoints * 1.0f);
            if (currentGalaxy) {
                currentDatasets.Remove(currentGalaxy);
                Destroy(currentGalaxy);
            }
            pointCloud = new GameObject(filename);
            currentGalaxy = pointCloud;
            currentDatasets.Add(currentGalaxy);
            for (int i = 0; i < numPointGroups - 1; i++) {
                InstantiateMesh(i, limitPoints);
                if (i % 10 == 0) {
                    guiText = i.ToString() + " out of " + numPointGroups.ToString() + " PointGroups loaded";
                    yield return null;
                }
            }
            InstantiateMesh(numPointGroups - 1, numPoints - (numPointGroups - 1) * limitPoints);

            //Store PointCloud
#if UNITY_EDITOR
            UnityEditor.PrefabUtility.CreatePrefab("Assets/Resources/PointCloudMeshes/" + filename + ".prefab", pointCloud);
#endif

            loaded = true;
            UpdateScreenData();
        }

        /// <summary>
        /// Creates a new mesh out of a already loaded data set
        /// </summary>
        /// <returns></returns>
        IEnumerator ReLoadGalaxy(int viewID) {
            firstStar = true;
            selectedPlanets = new List<LabeledPlanet>();
            SetUpLoadParameters();
            int numLines = 0;
            int nbrStars = 0;
            Debug.Log("amount of stars: " + planetData.Count);
            //TODO since this just takes the galaxy back to normal state we can skip this forloop and save the values at the beginning, and then just load the already made galaxy
            foreach (LabeledPlanet planet in planetData) {
                CheckIfEdgeStar(planet);
                accumulatedPlanetAge += planet.age;
                selectedPlanets.Add(planet);
                nbrStars++;
            }
            averageAge = accumulatedPlanetAge / nbrStars;
            if (selectedPlanets.Count != 0) {

                numLines = nbrStars; //HG: to be sure numlines is right for the following

                // Instantiate Point Groups
                numPointGroups = Mathf.CeilToInt(numLines * 1.0f / limitPoints * 1.0f);
                if (currentGalaxy) {
                    currentDatasets.Remove(currentGalaxy);
                    Destroy(currentGalaxy);
                }
                pointCloud = CreateGalaxyGameObject(filename);
                for (int i = 0; i < numPointGroups - 1; i++) {
                    InstantiateMeshSpecial(i, limitPoints);
                    if (i % 10 == 0) {
                        guiText = i.ToString() + " out of " + numPointGroups.ToString() + " PointGroups loaded";
                        yield return null;
                    }
                }

                InstantiateMeshSpecial(numPointGroups - 1, numLines - (numPointGroups - 1) * limitPoints);
                pointCloud.transform.rotation = Quaternion.Euler(90f, 0f, 0f);



            }

            UpdateScreenData();
            loaded = true;
        }



        //TODO this does not belong here
        private void UpdateScreenData() {
            GameObject[] tmp = spaceManager.GetComponent<PlayerParts>().dataScreenTexts;
            tmp[0].GetComponent<Text>().text = oldestPlanet.ToString();
            tmp[1].GetComponent<Text>().text = averageAge.ToString();
            tmp[2].GetComponent<Text>().text = youngestPlanet.ToString();

        }

        /*
         * This method let's us show only parts of the galaxy.
         */
        IEnumerator loadData_Galaxy_Sizes(Vector3[] area, Vector3[] directions, Vector3 oldPos, Quaternion oldRot) {
            bool insideBox = false; ;
            firstStar = true;
            selectedPlanets = new List<LabeledPlanet>();
            SetUpLoadParameters();
            int numLines = 0;
            int nbrStars = 0;

            //Since the box can be in an angle we need to calculate it's local vectors
            float distanceRightVector = Vector3.Distance(area[0], area[6]);
            float distanceUpVector = Vector3.Distance(area[2], area[6]);
            float distanceForwardVector = Vector3.Distance(area[4], area[6]);


            foreach (LabeledPlanet planet in planetData) {
                Vector3 vectorDistance = area[6] - planet.Position;
                float distancePlanetRight = new Vector3(vectorDistance.x * directions[0].x, vectorDistance.y * directions[0].y, vectorDistance.z * directions[0].z).magnitude;
                float distancePlanetUp = new Vector3(vectorDistance.x * directions[1].x, vectorDistance.y * directions[1].y, vectorDistance.z * directions[1].z).magnitude;
                float distancePlanetForward = new Vector3(vectorDistance.x * directions[2].x, vectorDistance.y * directions[2].y, vectorDistance.z * directions[2].z).magnitude;

                if (distanceRightVector >= distancePlanetRight) {
                    if (distanceUpVector >= distancePlanetUp) {
                        if (distanceForwardVector >= distancePlanetForward) {
                            insideBox = true;
                        }
                    }
                }

                if (insideBox) {
                    CheckIfEdgeStar(planet);
                    selectedPlanets.Add(planet);
                    nbrStars++;
                    checkIfYoungestOldest(planet.age);
                    accumulatedPlanetAge += planet.age;
                }


                insideBox = false;
            }

            averageAge = accumulatedPlanetAge / nbrStars;
            if (selectedPlanets.Count != 0) {

                numLines = nbrStars; //HG: to be sure numlines is right for the following

                // Instantiate Point Groups
                numPointGroups = Mathf.CeilToInt(numLines * 1.0f / limitPoints * 1.0f);
                if (currentGalaxy) {
                    currentDatasets.Remove(currentGalaxy);
                    Destroy(currentGalaxy);
                }
                pointCloud = CreateGalaxyGameObject(filename);
                spaceManager.GetComponent<PlayerParts>().dataPoints = pointCloud;
                for (int i = 0; i < numPointGroups - 1; i++) {
                    InstantiateMeshSpecial(i, limitPoints);
                    if (i % 10 == 0) {
                        guiText = i.ToString() + " out of " + numPointGroups.ToString() + " PointGroups loaded";
                        yield return null;
                    }
                }

                InstantiateMeshSpecial(numPointGroups - 1, numLines - (numPointGroups - 1) * limitPoints);

            }

            pointCloud.transform.rotation = Quaternion.Euler(oldRot.eulerAngles.x, oldRot.eulerAngles.y, oldRot.eulerAngles.z);
            pointCloud.transform.position = oldPos;
            UpdateScreenData();
            loaded = true;
        }

        void InstantiateMeshSpecial(int meshInd, int nPoints) {
            // Create Mesh
            GameObject pointGroup = new GameObject(filename + meshInd);
            pointGroup.AddComponent<MeshFilter>();
            pointGroup.AddComponent<MeshRenderer>();
            pointGroup.GetComponent<Renderer>().material = matVertex;

            pointGroup.GetComponent<MeshFilter>().mesh = CreateMeshSpecial(meshInd, nPoints, limitPoints);
            pointGroup.transform.parent = pointCloud.transform;

        }

        Mesh CreateMeshSpecial(int id, int nPoints, int limitPoints) {

            Mesh mesh = new Mesh();

            Vector3[] myPoints = new Vector3[nPoints];
            int[] indecies = new int[nPoints];
            Color[] myColors = new Color[nPoints];

            for (int i = 0; i < nPoints; ++i) {
                int starIndex = id * limitPoints + i;
                myPoints[i] = selectedPlanets[starIndex].Position;
                indecies[i] = i;
                if (colorGrading) {

                    selectedPlanets[starIndex].Color = spaceManager.GetComponent<GradientManager>().getColor(planetData[starIndex].age / (oldestPlanet - youngestPlanet));
                }
                myColors[i] = selectedPlanets[starIndex].Color;
            }

            mesh.vertices = myPoints;
            mesh.colors = myColors;
            mesh.SetIndices(indecies, MeshTopology.Points, 0);
            mesh.uv = new Vector2[nPoints];
            mesh.normals = new Vector3[nPoints];

            return mesh;
        }



        //HG

        // Start Coroutine of reading the points and parameters from text file and creating meshes
        IEnumerator loadData_Galaxy(string dPath) {


            // Load data from text file (no headers, line 1 is data with whitespaces)
            StreamReader sr = new StreamReader(Application.dataPath + "/" + dPath);
            string[] buffer;
            string line;
            int numLines;

            line = sr.ReadLine();
            buffer = line.Split(null);

            //HG: An unaccurate line counter (is bigger):
            numLines = (int)((sr.BaseStream.Length) / sr.ReadLine().Length);

            //HG: An unaccurate but slower counter:
            if (countLines) {
                numLines = 0;
                while (!sr.EndOfStream) {
                    line = sr.ReadLine();
                    numLines += 1;
                }
                numPoints = numLines;
                // ALTERNATIVE II:
                // Reload data 
                //sr = new StreamReader(Application.dataPath + "/" + dPath);
                //string[] allLines = sr.ReadToEnd().Split();
                //numLines = allLines.Length;

                //TESTING:
                // Reload data 
                //sr = new StreamReader(Application.dataPath + "/" + dPath);
                //Regex regex = new Regex(@"\s");  //HG doesnt work as expected
                //buffer = regex.Split(sr.ReadLine().Trim()); //using regular expression

                // Reload data 
                sr = new StreamReader(Application.dataPath + "/" + dPath);
            }
            // HG: 'off' files contains file length in line2 buffer[0]
            //sr.ReadLine(); // 1st line contains 'off'
            //buffer = sr.ReadLine().Split(); //null, nPoints, nFaces  
            //numPoints = int.Parse(buffer[0]);  

            points = new Vector3[numLines];
            colors = new Color[numLines];
            ages = new float[numLines];
            minValue = new Vector3();

            float age_light;
            float age_col;
            int i = 0;

            accumulatedPlanetAge = 0;
            averageAge = 0;
            youngestPlanet = float.MaxValue;
            oldestPlanet = 0;

            //for (int i = 0; i < numLines; i++)
            bool readingDone;
            string flag = "";
            while (!sr.EndOfStream && i < numLines) {
                try {
                    flag = "point";
                    if (!invertYZ)
                        points[i] = new Vector3(float.Parse(buffer[2]) * scale - offSet, float.Parse(buffer[4]) * scale - offSet, float.Parse(buffer[6]) * scale - offSet);
                    else
                        points[i] = new Vector3(float.Parse(buffer[2]) * scale - offSet, float.Parse(buffer[6]) * scale - offSet, float.Parse(buffer[4]) * scale - offSet);

                    colors[i] = new Color(55.0f, 0f, 0f);
                    if (buffer.Length >= 6 && i < 681536) //HG: Chrashing after !
                    {
                        flag = "age";
                        // ages[i] = 1.1f; // > 100k
                        ages[i] = float.Parse(buffer[8]);
                        age_light = ages[i] / 50000000000.0f;  // Red if > 50 mio year of age
                        if (age_light < 1.0f)
                            age_light = age_light * 255.0f;
                        else
                            age_light = 255.0f;
                        colors[i] = new Color(255.0f, age_light, age_light);
                    }



                } catch (IOException) {
                    readingDone = false;
                    Debug.Log("Reading data interrupted at line " + i.ToString() + " at " + flag);
                }
                numPoints = numLines;


                // Relocate Points near the origin
                //calculateMin(points[i]);

                // GUI progress:
                progress = i * 1.0f / (numLines - 1) * 1.0f;
                if (i % Mathf.FloorToInt(numLines / 20) == 0) {
                    guiText = i.ToString() + " out of " + numLines.ToString() + " loaded!!";
                    yield return null;
                }


                //HG always all! if (i < 50000)
                {
                    //HG02.. build planetData list here
                    //RTClient.cs:             List<Q3D> markerData = packet.Get3DMarkerData();
                    LabeledPlanet p = new LabeledPlanet();
                    p.Label = i.ToString();

                    pointRGB = new Color(r, g, b);
                    pointRGB.a = 0.3f;
                    p.Color = pointRGB; // transred; //dataColor;

                    p.Color = transblue; //HG colors[i];
                    //p.Color.a = planetTransparency;
                    p.Position = points[i];
                    p.age = ages[i];
                    //LabeledPlanet p = new LabeledPlanet();
                    //p.Id = 0;
                    //p.Residual = -1;
                    //p.Position = BitConvert.GetPoint(mData, ref position);

                    //body..planetData.Add(p);
                    planetData.Add(p);
                    //..HG02
                    CheckIfEdgeStar(p);

                    checkIfYoungestOldest(p.age);
                    accumulatedPlanetAge += p.age;
                }

                buffer = sr.ReadLine().Split(null);
                i += 1;



            }

            //Vector3 pos = Vector3.Lerp(new Vector3(farAwayPosX, farAwayPosY, farAwayPosZ), new Vector3(farAwayNegX, farAwayNegY, farAwayNegZ), 0.5f);
            //GetComponent<BoxCollider>().center = pos; //TODO Test for now
            //GetComponent<BoxCollider>().size = new Vector3(sizeX, sizeY, sizeZ); //TODO Test for now

            readingDone = true;
            numLines = i; //HG: to be sure numlines is right for the following

            // Instantiate Point Groups
            numPointGroups = Mathf.CeilToInt(numLines * 1.0f / limitPoints * 1.0f);

            if (currentGalaxy) {
                currentDatasets.Remove(currentGalaxy);
                Destroy(currentGalaxy);
            }


            averageAge = accumulatedPlanetAge / planetData.Count;
            pointCloud = CreateGalaxyGameObject(filename);
            spaceManager.GetComponent<PlayerParts>().dataPoints = pointCloud;
            currentGalaxy = pointCloud;
            currentDatasets.Add(currentGalaxy);


            PlayerPrefs.SetFloat("farAwayPosX", farAwayPosX);
            PlayerPrefs.SetFloat("farAwayNegX", farAwayNegX);
            PlayerPrefs.SetFloat("farAwayPosY", farAwayPosY);
            PlayerPrefs.SetFloat("farAwayNegY", farAwayNegY);
            PlayerPrefs.SetFloat("farAwayPosZ", farAwayPosZ);
            PlayerPrefs.SetFloat("farAwayNegZ", farAwayNegZ);
            PlayerPrefs.SetFloat("minAge", youngestPlanet);
            PlayerPrefs.SetFloat("maxAge", oldestPlanet);
            PlayerPrefs.SetFloat("avgAge", averageAge);

            for (i = 0; i < numPointGroups - 1; i++) {
                InstantiateMesh(i, limitPoints);
                if (i % 10 == 0) {
                    guiText = i.ToString() + " out of " + numPointGroups.ToString() + " PointGroups loaded";
                    yield return null;
                }
            }
            InstantiateMesh(numPointGroups - 1, numLines - (numPointGroups - 1) * limitPoints);

            pointCloud.transform.rotation = Quaternion.Euler(90f, 0f, 0f);
            //Store PointCloud
#if UNITY_EDITOR
            UnityEditor.PrefabUtility.CreatePrefab("Assets/Resources/PointCloudMeshes/" + filename + ".prefab", pointCloud);
#endif
            UpdateScreenData();

            loaded = true;
            selectedPlanets = planetData;
        }


        //..HG)


        void InstantiateMesh(int meshInd, int nPoints) {
            // Create Mesh
            GameObject pointGroup = new GameObject(filename + meshInd);
            pointGroup.AddComponent<MeshFilter>();
            pointGroup.AddComponent<MeshRenderer>();
            pointGroup.GetComponent<Renderer>().material = matVertex;

            pointGroup.GetComponent<MeshFilter>().mesh = CreateMesh(meshInd, nPoints, limitPoints);
            pointGroup.transform.parent = pointCloud.transform;


            // Store Mesh
            //TODO remove dependencies on editor!
#if UNITY_EDITOR
            UnityEditor.AssetDatabase.CreateAsset(pointGroup.GetComponent<MeshFilter>().mesh, "Assets/Resources/PointCloudMeshes/" + filename + @"/" + filename + meshInd + ".asset");
            UnityEditor.AssetDatabase.SaveAssets();
            UnityEditor.AssetDatabase.Refresh();
#endif
        }


        Mesh CreateMesh(int id, int nPoints, int limitPoints) {

            Mesh mesh = new Mesh();

            Vector3[] myPoints = new Vector3[nPoints];
            int[] indecies = new int[nPoints];
            Color[] myColors = new Color[nPoints];

            for (int i = 0; i < nPoints; ++i) {
                myPoints[i] = points[id * limitPoints + i] - minValue;
                indecies[i] = i;
                myColors[i] = spaceManager.GetComponent<GradientManager>().getColor(planetData[id * limitPoints + i].age / (oldestPlanet - youngestPlanet));
            }

            mesh.vertices = myPoints;
            mesh.colors = myColors;
            mesh.SetIndices(indecies, MeshTopology.Points, 0);
            mesh.uv = new Vector2[nPoints];
            mesh.normals = new Vector3[nPoints];

            return mesh;
        }


        void pointPaint(int iStart, int iEnd) {
            int i;
            string cloudName = "latest_sel";
            pointCloudSel = new GameObject(cloudName);

            for (i = iStart; i < iEnd - 1; i++) {
                colors[i] = new Color(0.0f, 255.0f, 0.0f);
            }
            InstantiateMesh(iStart, iEnd - iStart);
            // Store PointCloud like this:
            //#if UNITY_EDITOR
            //            UnityEditor.PrefabUtility.CreatePrefab("Assets/Resources/PointCloudMeshes/" + filename + ".prefab", pointCloud);
            //#endif            
        }

        //HG01..
        private void InitiatePlanets(int iStart, int iEnd, int iStep) {
            foreach (var planet in planets) {
                Destroy(planet);
            }

            planets.Clear();
            //HG01a markerData = rtClient.Markers;

            //HGttt for (int i = 0; i < planetData.Count; i = i + planetShowOneOutOf)  // Turn 1% of redish dots into a semi transparent sphere
            for (int i = iStart; i < iEnd; i = i + iStep) {
                GameObject newPlanet = GameObject.CreatePrimitive(PrimitiveType.Sphere);
                newPlanet.layer = 10;
                newPlanet.name = planetData[i].Label;
                newPlanet.transform.parent = markerRoot.transform;

                //planetData[i].Color.a = planetTransparency;
                newPlanet.GetComponent<Renderer>().material.color = planetData[i].Color;

                //HG 21/5
                //OK: newPlanet.GetComponent<Renderer>().material.shader = Shader.Find("Standard");

                //TODO Try this I:
                //Default-Material (Instance)
                newPlanet.GetComponent<Renderer>().material.shader = Shader.Find("GUI/Text Shader");
                //Font texture "default-Particle"

                //TODO Try this II:

                newPlanet.GetComponent<Renderer>().material.SetColor("_EmissionColor", planetData[i].Color); //Color.Lerp(0.6F, 0.2F, 0.0F));

                newPlanet.transform.localPosition = planetData[i].Position;
                newPlanet.SetActive(true);
                newPlanet.GetComponent<Renderer>().enabled = true; // visibleMarkers;
                newPlanet.transform.localScale = Vector3.one * planetScale;

                //newPlanet.tag = "QTM_marker";  //HG                    

                planets.Add(newPlanet);
                //QTM: markers' pos are normaly streamed an set in update..

                //TODO: 
                //if (Physics.Raycast(transform.position, planets[planets.Count - 1].transform.position, out hit))
                //{
                //    Vector3 dir = planets[planets.Count - 1].transform.position - transform.position;
                //    Debug.DrawRay(transform.position, dir, transblue, 0.2f);
                //}

            }
            if (planetData.Count > 0)
                hasPlanets = true;
        }
        //..HG01


        void createFolders() {
#if UNITY_EDITOR
            if (!Directory.Exists(Application.dataPath + "/Resources/"))
                UnityEditor.AssetDatabase.CreateFolder("Assets", "Resources");

            if (!Directory.Exists(Application.dataPath + "/Resources/PointCloudMeshes/"))
                UnityEditor.AssetDatabase.CreateFolder("Assets/Resources", "PointCloudMeshes");
#endif
        }


        void OnGUI() {
            if (!loaded) {
                GUI.BeginGroup(new Rect(Screen.width / 2 - 100, Screen.height / 2, 400.0f, 20));
                GUI.Box(new Rect(0, 0, 200.0f, 20.0f), guiText);
                GUI.Box(new Rect(0, 0, progress * 200.0f, 20), "");
                GUI.EndGroup();
            }
        }

        /// <summary>
        /// Loads a new dataset in which the only data visible are within the max and min value.
        /// </summary>
        /// <param name="maxAge">Max value for the data point</param>
        /// <param name="minAge">Min value for the data point</param>
        /// <param name="lessThan">Outdated to be removed</param>
        /// <param name="pos">Old position of dataset</param>
        /// <param name="rot">Old rotation of dataset</param>
        public void loadScene(float maxAge, float minAge, bool lessThan, Vector3 pos, Quaternion rot) {
            StartCoroutine(loadData_Galaxy_Sizes(maxAge, minAge, lessThan, pos, rot));
        }


        IEnumerator loadData_Galaxy_Sizes(float maxAge, float minAge, bool lessThan, Vector3 pos, Quaternion rot) {

            firstStar = true;
            selectedForVisualisation = new List<LabeledPlanet>();
            visualisationColors = new List<Color>();
            SetUpLoadParameters();
            int numLines = 0;
            int nbrStars = 0;

            foreach (LabeledPlanet planet in selectedPlanets) {

                if (lessThan && planet.age <= maxAge && planet.age >= minAge) {

                    CheckIfEdgeStar(planet);
                    selectedForVisualisation.Add(planet);
                    visualisationColors.Add(planet.Color);
                    nbrStars++;
                }
            }
            averageAge = accumulatedPlanetAge / nbrStars;
            if (selectedPlanets.Count != 0) {
                numLines = nbrStars; //HG: to be sure numlines is right for the following

                // Instantiate Point Groups
                numPointGroups = Mathf.CeilToInt(numLines * 1.0f / limitPoints * 1.0f);
                if (currentGalaxy) {
                    currentDatasets.Remove(currentGalaxy);
                    Destroy(currentGalaxy);
                }
                pointCloud = CreateGalaxyGameObject(filename);
                spaceManager.GetComponent<PlayerParts>().dataPoints = pointCloud;
                for (int i = 0; i < numPointGroups - 1; i++) {
                    InstantiateDataMesh(i, limitPoints);
                    if (i % 10 == 0) {
                        guiText = i.ToString() + " out of " + numPointGroups.ToString() + " PointGroups loaded";
                        yield return null;
                    }
                }

                InstantiateDataMesh(numPointGroups - 1, numLines - (numPointGroups - 1) * limitPoints);

            }
            pointCloud.transform.rotation = Quaternion.Euler(rot.eulerAngles.x, rot.eulerAngles.y, rot.eulerAngles.z);
            pointCloud.transform.position = pos;
            UpdateScreenData();
            loaded = true;
        }


        void InstantiateDataMesh(int meshInd, int nPoints) {
            // Create Mesh
            GameObject pointGroup = new GameObject(filename + meshInd);
            pointGroup.AddComponent<MeshFilter>();
            pointGroup.AddComponent<MeshRenderer>();
            pointGroup.GetComponent<Renderer>().material = matVertex;

            pointGroup.GetComponent<MeshFilter>().mesh = CreateDataMesh(meshInd, nPoints, limitPoints);
            pointGroup.transform.parent = pointCloud.transform;

        }

        Mesh CreateDataMesh(int id, int nPoints, int limitPoints) {
            Mesh mesh = new Mesh();

            Vector3[] myPoints = new Vector3[nPoints];
            int[] indecies = new int[nPoints];
            Color[] myColors = new Color[nPoints];

            for (int i = 0; i < nPoints; ++i) {
                myPoints[i] = selectedForVisualisation[i].Position;
                indecies[i] = i;
                if (colorGrading) {
                    selectedForVisualisation[i].Color = visualisationColors[i];
                }
                myColors[i] = selectedForVisualisation[i].Color;

            }

            mesh.vertices = myPoints;
            mesh.colors = myColors;
            mesh.SetIndices(indecies, MeshTopology.Points, 0);
            mesh.uv = new Vector2[nPoints];
            mesh.normals = new Vector3[nPoints];

            return mesh;
        }

        public bool dataRead = false;
        IEnumerator ReadDataFromFile(string dPath) {
            Debug.Log("loading data");
            dataRead = false;
            StreamReader sr = new StreamReader(Application.dataPath + "/" + dPath);
            string[] buffer;
            string line;
            int numLines;

            line = sr.ReadLine();
            buffer = line.Split(null);

            //HG: An unaccurate line counter (is bigger):
            numLines = (int)((sr.BaseStream.Length) / sr.ReadLine().Length);

            //HG: An unaccurate but slower counter:
            if (countLines) {
                numLines = 0;
                while (!sr.EndOfStream) {
                    line = sr.ReadLine();
                    numLines += 1;
                }
                numPoints = numLines;
                // ALTERNATIVE II:
                // Reload data 
                //sr = new StreamReader(Application.dataPath + "/" + dPath);
                //string[] allLines = sr.ReadToEnd().Split();
                //numLines = allLines.Length;

                //TESTING:
                // Reload data 
                //sr = new StreamReader(Application.dataPath + "/" + dPath);
                //Regex regex = new Regex(@"\s");  //HG doesnt work as expected
                //buffer = regex.Split(sr.ReadLine().Trim()); //using regular expression

                // Reload data 
                sr = new StreamReader(Application.dataPath + "/" + dPath);
            }
            // HG: 'off' files contains file length in line2 buffer[0]
            //sr.ReadLine(); // 1st line contains 'off'
            //buffer = sr.ReadLine().Split(); //null, nPoints, nFaces  
            //numPoints = int.Parse(buffer[0]);  

            points = new Vector3[numLines];
            colors = new Color[numLines];
            ages = new float[numLines];
            minValue = new Vector3();

            float age_light;
            float age_col;
            int i = 0;

            accumulatedPlanetAge = 0;
            averageAge = 0;
            youngestPlanet = float.MaxValue;
            oldestPlanet = 0;

            //for (int i = 0; i < numLines; i++)
            bool readingDone;
            string flag = "";
            while (!sr.EndOfStream && i < numLines) {
                try {
                    flag = "point";
                    if (!invertYZ)
                        points[i] = new Vector3(float.Parse(buffer[2]) * scale - offSet, float.Parse(buffer[4]) * scale - offSet, float.Parse(buffer[6]) * scale - offSet);
                    else
                        points[i] = new Vector3(float.Parse(buffer[2]) * scale - offSet, float.Parse(buffer[6]) * scale - offSet, float.Parse(buffer[4]) * scale - offSet);

                    colors[i] = new Color(55.0f, 0f, 0f);
                    if (buffer.Length >= 6 && i < 681536) //HG: Chrashing after !
                    {
                        flag = "age";
                        // ages[i] = 1.1f; // > 100k
                        ages[i] = float.Parse(buffer[8]);
                        age_light = ages[i] / 50000000000.0f;  // Red if > 50 mio year of age
                        if (age_light < 1.0f)
                            age_light = age_light * 255.0f;
                        else
                            age_light = 255.0f;
                        colors[i] = new Color(255.0f, age_light, age_light);
                    }



                } catch (IOException) {
                    readingDone = false;
                    Debug.Log("Reading data interrupted at line " + i.ToString() + " at " + flag);
                }
                numPoints = numLines;


                // Relocate Points near the origin
                //calculateMin(points[i]);

                // GUI progress:
                /*progress = i * 1.0f / (numLines - 1) * 1.0f;
                if (i % Mathf.FloorToInt(numLines / 20) == 0) {
                    guiText = i.ToString() + " out of " + numLines.ToString() + " loaded!!";
                    yield return null;
                }*/


                //HG always all! if (i < 50000)
                {
                    //HG02.. build planetData list here
                    //RTClient.cs:             List<Q3D> markerData = packet.Get3DMarkerData();
                    LabeledPlanet p = new LabeledPlanet();
                    p.Label = i.ToString();

                    pointRGB = new Color(r, g, b);
                    pointRGB.a = 0.3f;
                    p.Color = pointRGB; // transred; //dataColor;

                    p.Color = transblue; //HG colors[i];
                    //p.Color.a = planetTransparency;
                    p.Position = points[i];
                    p.age = ages[i];
                    //LabeledPlanet p = new LabeledPlanet();
                    //p.Id = 0;
                    //p.Residual = -1;
                    //p.Position = BitConvert.GetPoint(mData, ref position);

                    //body..planetData.Add(p);
                    planetData.Add(p);
                    //..HG02
                    CheckIfEdgeStar(p);

                    checkIfYoungestOldest(p.age);
                    accumulatedPlanetAge += p.age;
                }

                buffer = sr.ReadLine().Split(null);
                i += 1;



            }
            selectedPlanets = planetData;
            dataRead = true;
            Debug.Log("selectedPlanetsCount: " + selectedPlanets.Count);
            yield return null;
        }

        /// <summary>
        /// Check if the data point is of extreme values. For example if the data point is furthest away in a direction.
        /// </summary>
        /// <param name="filename">Data point</param>
        void CheckIfEdgeStar(LabeledPlanet planet) {
            if (firstStar) {

                farAwayNegX = planet.Position.x;
                farAwayPosX = planet.Position.x;
                farAwayNegY = planet.Position.y;
                farAwayPosY = planet.Position.y;
                farAwayNegZ = planet.Position.z;
                farAwayPosZ = planet.Position.z;
                firstStar = false;
            }
            if (planet.age > oldestPlanet) {
                oldestPlanet = planet.age;
            } else if (planet.age < youngestPlanet) {
                youngestPlanet = planet.age;
            }


            if (planet.Position.x < farAwayNegX) {
                farAwayNegX = planet.Position.x;
            }
            if (planet.Position.x > farAwayPosX) {
                farAwayPosX = planet.Position.x;
            }
            if (planet.Position.y < farAwayNegY) {
                farAwayNegY = planet.Position.y;
            }
            if (planet.Position.y > farAwayPosY) {
                farAwayPosY = planet.Position.y;
            }
            if (planet.Position.z < farAwayNegZ) {
                farAwayNegZ = planet.Position.z;
            }
            if (planet.Position.z > farAwayPosZ) {
                farAwayPosZ = planet.Position.z;
            }
        }

        /// <summary>
        /// Creates the base dataset object and adds all relevant settings.
        /// </summary>
        /// <param name="filename">Filename of the loaded file</param>
        /// <returns name="pointCloud">The base dataset</returns>
        GameObject CreateGalaxyGameObject(string filename) {
            float sizeX = farAwayPosX - farAwayNegX;
            float sizeY = farAwayPosY - farAwayNegY;
            float sizeZ = farAwayPosZ - farAwayNegZ;
            pointCloud = Instantiate(pointCloudPrefab);
            currentGalaxy = pointCloud;
            currentDatasets.Add(currentGalaxy);
            pointCloud.layer = 11;
            pointCloud.AddComponent<BoxCollider>();
            pointCloud.AddComponent<Rigidbody>();
            pointCloud.GetComponent<Rigidbody>().isKinematic = true;
            pointCloud.GetComponent<Rigidbody>().useGravity = false;
            pointCloud.tag = "Star";
            Vector3 pos = Vector3.Lerp(new Vector3(farAwayPosX, farAwayPosY, farAwayPosZ), new Vector3(farAwayNegX, farAwayNegY, farAwayNegZ), 0.5f);
            pointCloud.GetComponent<BoxCollider>().center = pos;
            pointCloud.GetComponent<BoxCollider>().size = new Vector3(sizeX, sizeY, sizeZ);
            spaceManager.GetComponent<PlayerParts>().dataPoints = pointCloud;
            return pointCloud;
        }

        public GameObject GetCurrentGalaxy() {
            return currentGalaxy;
        }

        public List<LabeledPlanet> GetPlanetData() {
            if (selectedPlanets == null) {
                return planetData;
            }
            return selectedPlanets;
        }

        /// <summary>
        /// Updates all dataset data information to the last saved ones.
        /// </summary>
        void loadStandardData() {
            farAwayPosX = PlayerPrefs.GetFloat("farAwayPosX");
            farAwayNegX = PlayerPrefs.GetFloat("farAwayNegX");
            farAwayPosY = PlayerPrefs.GetFloat("farAwayPosY");
            farAwayNegY = PlayerPrefs.GetFloat("farAwayNegY");
            farAwayPosZ = PlayerPrefs.GetFloat("farAwayPosZ");
            farAwayNegZ = PlayerPrefs.GetFloat("farAwayNegZ");
            youngestPlanet = PlayerPrefs.GetFloat("minAge");
            oldestPlanet = PlayerPrefs.GetFloat("maxAge");
            averageAge = PlayerPrefs.GetFloat("avgAge");
        }

        private void SetUpLoadParameters() {
            farAwayNegX = 0;
            farAwayNegY = 0;
            farAwayNegZ = 0;
            farAwayPosX = 0;
            farAwayPosY = 0;
            farAwayPosZ = 0;
            averageAge = 0;
            youngestPlanet = float.MaxValue;
            oldestPlanet = 0;
            accumulatedPlanetAge = 0;
        }

        void checkIfYoungestOldest(float age) {
            if (age > oldestPlanet) {
                oldestPlanet = age;
            } else if (age < youngestPlanet) {
                youngestPlanet = age;
            }
        }

        public void DeleteDataSet() {
            if (spaceManager.GetComponent<SpaceUtilities>().CurrentDataset()) {
                currentDatasets.Remove(spaceManager.GetComponent<SpaceUtilities>().CurrentDataset());
                Destroy(currentGalaxy);
            }

        }

        public void SetCurrentDataset(GameObject currDataset) {
            currentGalaxy = currDataset;
        }

    }

}