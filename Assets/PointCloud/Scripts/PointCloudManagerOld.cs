using UnityEngine;
using System.Collections;
using System.IO;

public class PointCloudManagerOld : MonoBehaviour {

	// File
	public string dataPath;
	private string filename;
	public Material matVertex;

	// GUI
	private float progress = 0;
	private string guiText;
	private bool loaded = false;

	// PointCloud
	private GameObject pointCloud;

	public float scale = 1;
	public bool invertYZ = false;
	public bool forceReload = false;

	public int numPoints;
	public int numPointGroups;
	private int limitPoints = 65000;

	private Vector3[] points;
	private Color[] colors;
	private Vector3 minValue;

    private float farAwayNegX;
    private float farAwayNegY;
    private float farAwayNegZ;
    private float farAwayPosX;
    private float farAwayPosY;
    private float farAwayPosZ;



    void Start () {
		// Create Resources folder
		createFolders ();

		// Get Filename
		filename = Path.GetFileName(dataPath);

		loadScene ();
	}



	void loadScene(){
#if UNITY_EDITOR
        Debug.Log(Application.dataPath + "/Resources/PointCloudMeshes/" + filename);
		// Check if the PointCloud was loaded previously
		if(!Directory.Exists (Application.dataPath + "/Resources/PointCloudMeshes/" + filename)){
            UnityEditor.AssetDatabase.CreateFolder ("Assets/Resources/PointCloudMeshes", filename);
			loadPointCloud ();
           
        } else if (forceReload){
            UnityEditor.FileUtil.DeleteFileOrDirectory(Application.dataPath + "/Resources/PointCloudMeshes/" + filename);
			UnityEditor.AssetDatabase.Refresh();
			UnityEditor.AssetDatabase.CreateFolder ("Assets/Resources/PointCloudMeshes", filename);
			loadPointCloud ();
            
        } else
            // Load stored PointCloud
            
        loadStoredMeshes();
#else
        loadStoredMeshes();
#endif
    }


    void loadPointCloud(){
        // Check what file exists
        if (File.Exists(Application.dataPath + dataPath + ".off"))
            // load off
            StartCoroutine("loadOFF", dataPath + ".off");
        else
			Debug.Log ("File '" + dataPath + "' could not be found"); 
		
	}
	
	// Load stored PointCloud
	void loadStoredMeshes(){
        Debug.Log("hesadjsdas4");
        Debug.Log ("Using previously loaded PointCloud: " + filename);

		GameObject pointGroup = Instantiate(Resources.Load ("PointCloudMeshes/" + filename)) as GameObject;

        //In the original program the pivot point are way off, so I solve this by creating a new empty moving it to the player

		loaded = true;
	}
	
	// Start Coroutine of reading the points from the OFF file and creating the meshes
	IEnumerator loadOFF(string dPath){
        Debug.Log(Application.dataPath + dPath);
	    // Read file
	    StreamReader sr = new StreamReader (Application.dataPath + dPath);
	    sr.ReadLine (); // OFF
	    string[] buffer = sr.ReadLine ().Split(); // nPoints, nFaces
		
	    numPoints = int.Parse (buffer[0]);
	    points = new Vector3[numPoints];
	    colors = new Color[numPoints];
	    minValue = new Vector3();

       bool firstStar = true;

        //Since the galaxy is a bit offset from origo we set the initial values to the first star and go from there


        /* if (x > position.x + size.x || x < position.x - size.x ||
             y > position.y + size.y || y < position.y + size.y ||
             z > position.z + size.z || z < position.z + size.z) {
             return;
         }*/

        Debug.Log("we got this far");
            for (int i = 0; i< numPoints; i++){
			buffer = sr.ReadLine ().Split ();

            if (!invertYZ) { 
                Debug.Log("we gonna read");
				points[i] = new Vector3 (float.Parse (buffer[0])*scale, float.Parse (buffer[1])*scale,float.Parse (buffer[2])*scale) ;
                Debug.Log("we read");
            } else
				points[i] = new Vector3 (float.Parse (buffer[0])*scale, float.Parse (buffer[2])*scale,float.Parse (buffer[1])*scale) ;

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

            if (buffer.Length >= 5)
				colors[i] = new Color (int.Parse (buffer[3])/255.0f,int.Parse (buffer[4])/255.0f,int.Parse (buffer[5])/255.0f);
			else
				colors[i] = Color.cyan;

			// Relocate Points near the origin
			//calculateMin(points[i]);

			// GUI
			progress = i *1.0f/(numPoints-1)*1.0f;
			if (i%Mathf.FloorToInt(numPoints/20) == 0){
				guiText=i.ToString() + " out of " + numPoints.ToString() + " loaded";
				yield return null;
			}
		}

        Debug.Log("negX: " + farAwayNegX + " posX: " + farAwayPosY + " negY: "
               + farAwayNegX + " posY: " + farAwayPosY + " negZ : " + farAwayNegZ + " posZ : " + farAwayPosZ);
        float sizeX = farAwayPosX - farAwayNegX;
        float sizeY = farAwayPosY - farAwayNegY;
        float sizeZ = farAwayPosZ - farAwayNegZ;
        Vector3 pos = Vector3.Lerp(new Vector3(farAwayPosX, farAwayPosY, farAwayPosZ), new Vector3(farAwayNegX, farAwayNegY, farAwayNegZ), 0.5f);
        //GetComponent<BoxCollider>().center = pos; //TODO Test for now
        //GetComponent<BoxCollider>().size = new Vector3(sizeX, sizeY, sizeZ); //TODO Test for now

        // Instantiate Point Groups
        numPointGroups = Mathf.CeilToInt (numPoints*1.0f / limitPoints*1.0f);

		pointCloud = new GameObject (filename);

		for (int i = 0; i < numPointGroups-1; i ++) {
			InstantiateMesh (i, limitPoints);
			if (i%10==0){
				guiText = i.ToString() + " out of " + numPointGroups.ToString() + " PointGroups loaded";
				yield return null;
			}
		}
		InstantiateMesh (numPointGroups-1, numPoints- (numPointGroups-1) * limitPoints);

        //Store PointCloud
#if UNITY_EDITOR
        UnityEditor.PrefabUtility.CreatePrefab ("Assets/Resources/PointCloudMeshes/" + filename + ".prefab", pointCloud);
#endif
        loaded = true;
	}

	
	void InstantiateMesh(int meshInd, int nPoints){
		// Create Mesh
		GameObject pointGroup = new GameObject (filename + meshInd);
		pointGroup.AddComponent<MeshFilter> ();
		pointGroup.AddComponent<MeshRenderer> ();
		pointGroup.GetComponent<Renderer>().material = matVertex;

		pointGroup.GetComponent<MeshFilter> ().mesh = CreateMesh (meshInd, nPoints, limitPoints);
		pointGroup.transform.parent = pointCloud.transform;


        // Store Mesh
#if UNITY_EDITOR
        UnityEditor.AssetDatabase.CreateAsset(pointGroup.GetComponent<MeshFilter> ().mesh, "Assets/Resources/PointCloudMeshes/" + filename + @"/" + filename + meshInd + ".asset");
		UnityEditor.AssetDatabase.SaveAssets ();
		UnityEditor.AssetDatabase.Refresh();
#endif
    }

	Mesh CreateMesh(int id, int nPoints, int limitPoints){
		
		Mesh mesh = new Mesh ();
		
		Vector3[] myPoints = new Vector3[nPoints]; 
		int[] indecies = new int[nPoints];
		Color[] myColors = new Color[nPoints];

		for(int i=0;i<nPoints;++i) {
			myPoints[i] = points[id*limitPoints + i] - minValue;
			indecies[i] = i;
			myColors[i] = colors[id*limitPoints + i];
		}


		mesh.vertices = myPoints;
		mesh.colors = myColors;
		mesh.SetIndices(indecies, MeshTopology.Points,0);
		mesh.uv = new Vector2[nPoints];
		mesh.normals = new Vector3[nPoints];


		return mesh;
	}

	void calculateMin(Vector3 point){
		if (minValue.magnitude == 0)
			minValue = point;


		if (point.x < minValue.x)
			minValue.x = point.x;
		if (point.y < minValue.y)
			minValue.y = point.y;
		if (point.z < minValue.z)
			minValue.z = point.z;
	}

	void createFolders(){
#if UNITY_EDITOR
        if (!Directory.Exists (Application.dataPath + "/Resources/"))
			UnityEditor.AssetDatabase.CreateFolder ("Assets", "Resources");

		if (!Directory.Exists (Application.dataPath + "/Resources/PointCloudMeshes/"))
			UnityEditor.AssetDatabase.CreateFolder ("Assets/Resources", "PointCloudMeshes");
#endif
    }


	void OnGUI(){


		if (!loaded){
			GUI.BeginGroup (new Rect(Screen.width/2-100, Screen.height/2, 400.0f, 20));
			GUI.Box (new Rect (0, 0, 200.0f, 20.0f), guiText);
			GUI.Box (new Rect (0, 0, progress*200.0f, 20), "");
			GUI.EndGroup ();
		}
	}

}
