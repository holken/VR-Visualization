using UnityEngine;
using System.Collections;
using System.IO;
using UnityEngine.EventSystems;
using System.Threading;
using System.Collections.Generic; //HG01


namespace pointcloud_objects
{
    // Class for Celestial Body with unity data types
    public class LabeledPlanet  //TODO: rename to: celestial body
    {
        public LabeledPlanet() { }
        public string Label;
        public Vector3 Position;
        public Color Color;
        public float age; //Added by Simon
    }

    // Class for Orbit with unity data types
    // Orbit: The curved path through which objects in space move around a planet or star. (ref cambridge dic)
    //        Area or sphere of activity, interest, or influence. (ref wiki)
    public class Orbit
    {
        public Orbit() { }
        public string From;
        public LabeledPlanet FromPlanet;  //TODO: rename to: celestial body
        public string To;
        public LabeledPlanet ToPlanet;  //TODO: rename to: celestial body
        public Color Color = Color.yellow;
    }

    public class celestial_body : MonoBehaviour
    {
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

        private GameObject markerRoot;
        private List<GameObject> planets;  //HG: from private to public
        private List<LabeledPlanet> planetData;
        private Color transblue = new Color(0.5F, 0.0F, 0.9F);
        private Color transred = new Color(0.9F, 0.1F, 0.1F);
        private Color transtotal = new Color(0.0F, 0.0F, 0.0F);
        private Color pointRGB = new Color(0.9F, 0.9F, 0.9F);

        private RaycastHit hit;
        private bool hasPlanets = false;     //HG03

        public Color c1 = Color.yellow;
        public Color c2 = Color.red;
        public int lengthOfLineRenderer = 2000;


        // Use this for initialization
        void Start()
        {
            planets = new List<GameObject>();
            planetData = new List<LabeledPlanet>();

        }

        // Update is called once per frame
        void Update()
        {
            if (!hasPlanets)
                InitiatePlanets();

            if (Input.GetKey(KeyCode.Space))
            {
                //Cube.transform.position += transform.right;
                InitiatePlanets();
            }

            //HG04.. LINE test
            if (Input.GetKey(KeyCode.L))
            {
                LineRenderer lineRenderer = GetComponent<LineRenderer>();
                var points = new Vector3[lengthOfLineRenderer];
                var t = Time.time;

                int i = 0;
                foreach (GameObject p in planets)
                {
                    //Vector3 dir = p.transform.position - transform.position;
                    //float angle = Mathf.Atan2(dir.y, dir.x) * Mathf.Rad2Deg;  // For display
                    if (i < lengthOfLineRenderer)
                    {
                        points[i] = p.transform.position; // new Vector3(i * 0.5f, Mathf.Sin(i + t), 0.0f);
                    }
                    i++;
                }
                lineRenderer.SetPositions(points);
            }
            //..HG04

        }
        private void InitiatePlanets()
        {
            foreach (var planet in planets)
            {
                Destroy(planet);
            }

            planets.Clear();
            //HG01a markerData = rtClient.Markers;

            for (int i = 0; i < planetData.Count; i = i + planetShowOneOutOf)  // Turn 1% of redish dots into a semi transparent sphere
            {
                GameObject newPlanet = GameObject.CreatePrimitive(PrimitiveType.Sphere);
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

    }
}