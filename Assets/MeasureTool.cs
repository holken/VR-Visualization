using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;

public class MeasureTool : MonoBehaviour {
    private bool firstPointSet;
    private bool secondPointSet;
    private bool saveObjects;
    private Vector3 initialPos;
    private GameObject cubeToScale;
    private GameObject measureText;
    private List<Color> colors;

    public int fontSize = 4;
    public GameObject sizeText;

    private int fontColorPointer = 0;
    public GameObject fontColorImage;

    private int lineColorPointer = 0;
    public GameObject lineColorImage;

    public int unitPointer = 0;
    public GameObject unitText;
    private List<float> units;

    private GameObject player;


    public void DestroyCurrentMeasuring() {
        if (IsCurrentlyMeasuring()) {
            Destroy(cubeToScale);
            Destroy(measureText);
            firstPointSet = false;
            secondPointSet = false;
        }
        
    }

    public bool IsCurrentlyMeasuring() {
        if (firstPointSet && !secondPointSet) {
            return true;
        }
        return false;
    }

    public void ToggleSaveObjects() {
        if (!saveObjects) {
            saveObjects = true;
        } else {
            saveObjects = false;
        }
    }

	public void MeasureToolAction(Vector3 initialPos) {
        if (!firstPointSet) {
            this.initialPos = initialPos;
            firstPointSet = true;
            cubeToScale = (GameObject)Instantiate(Resources.Load("MeasureCube"));
            cubeToScale.transform.position = transform.position;
            cubeToScale.transform.localScale = new Vector3(0.002f, 0.002f, 0.1f) * player.transform.localScale.x; //Fixes so it's same scale as player
            cubeToScale.GetComponent<Renderer>().material.color = colors[lineColorPointer];
            measureText = (GameObject)Instantiate(Resources.Load("ControllerText"));
            measureText.GetComponent<TextMesh>().fontSize = fontSize * 50;
            measureText.transform.localScale = measureText.transform.localScale * player.transform.localScale.x;
            measureText.GetComponent<TextMesh>().color = colors[fontColorPointer];
        } else if (firstPointSet && !secondPointSet) {
            secondPointSet = false;
            firstPointSet = false;
            float distance = Vector3.Distance(transform.position, initialPos);
            initialPos = Vector3.zero;
            if (!saveObjects) {
                Destroy(cubeToScale);
                Destroy(measureText);
            } else {
                cubeToScale.tag = "Mark";
                cubeToScale.layer = 8;
                cubeToScale.AddComponent<Rigidbody>();
                cubeToScale.GetComponent<Rigidbody>().isKinematic = true;
                cubeToScale.GetComponent<Rigidbody>().useGravity = false;
                cubeToScale.AddComponent<MarkType>();
                cubeToScale.GetComponent<MarkType>().typeId = 2;
                cubeToScale.AddComponent<HasA>().hasObj = (measureText);
            }
            cubeToScale = null;
            measureText = null;
        }
        
    } 

    public void UpdateMeasureTool(Vector3 controllerPos) {
        if (firstPointSet && !secondPointSet) {
            float distance = Vector3.Distance(controllerPos, initialPos);
            double distanceUnit = distance * units[unitPointer];
            distanceUnit = System.Math.Round(distanceUnit, 1);
            cubeToScale.transform.localScale = new Vector3(0.002f * player.transform.localScale.x, 0.002f * player.transform.localScale.y, distance); 
            Vector3 vectorBetween = Vector3.Lerp(controllerPos, initialPos, 0.5f);
            cubeToScale.transform.localPosition = vectorBetween;
            cubeToScale.transform.LookAt(controllerPos);

            measureText.transform.position = controllerPos;
            measureText.transform.LookAt(GetComponent<PlayerParts>().head.transform);
            measureText.transform.Rotate(0, 180f, 0f);
            measureText.GetComponent<TextMesh>().text = "Dist: " + distanceUnit + unitText.GetComponent<Text>().text;
        }
    }

    public void UpdateSize(int incr) {
        if ((incr == -1 && fontSize > 0) || (incr == 1 && fontSize < 10)) {
            fontSize += incr;
        }
        sizeText.GetComponent<Text>().text = fontSize.ToString();
    }

    public void UpdateFontColor(int incr) {
        fontColorPointer += incr;
        if (fontColorPointer < 0) {
            fontColorPointer = colors.Count-1;
        } else if (fontColorPointer >= colors.Count) {
            fontColorPointer = 0;
        }
        fontColorImage.GetComponent<Image>().color = colors[fontColorPointer];
    }

    public void UpdateLineColor(int incr) {
        lineColorPointer += incr;
        if (colors.Count < 0) {
            lineColorPointer = colors.Count - 1;
        } else if (lineColorPointer >= colors.Count) {
            lineColorPointer = 0;
        }
        lineColorImage.GetComponent<Image>().color = colors[lineColorPointer];
    }


    public void UpdateUnitPointer(int incr) {
        unitPointer += incr;
        if (unitPointer >= units.Count) {
            unitPointer = 0;
        } else if (unitPointer < 0) {
            unitPointer = units.Count - 1;
        }
        if (unitPointer == 0) {
            unitText.GetComponent<Text>().text = "ly";
        } else if (unitPointer == 1) {
            unitText.GetComponent<Text>().text = "Kly";
        } else if (unitPointer == 2) {
            unitText.GetComponent<Text>().text = "pc";
        } else if (unitPointer == 3) {
            unitText.GetComponent<Text>().text = "Kpc";
        }

    }

    void Start() {
        if (sizeText == null) Debug.Log("Please give the measureTool a sizeText from the menu");

        sizeText.GetComponent<Text>().text = fontSize.ToString();

        if (fontColorImage == null) Debug.Log("Please give the measureTool a fontColorImage from the menu");

        if (lineColorImage == null) Debug.Log("Please give the measureTool a lineColorImage from the menu");

        colors = new List<Color>();
        colors.Add(Color.white);
        colors.Add(Color.red);
        colors.Add(Color.yellow);
        colors.Add(Color.green);
        colors.Add(Color.magenta);
        colors.Add(Color.cyan);
        colors.Add(Color.blue);

        fontColorImage.GetComponent<Image>().color = colors[fontColorPointer];
        lineColorImage.GetComponent<Image>().color = colors[lineColorPointer];

        units = GetComponent<SpaceUtilities>().getUnits();
        UpdateUnitPointer(0);
        player = GetComponent<PlayerParts>().body;
    }
}
