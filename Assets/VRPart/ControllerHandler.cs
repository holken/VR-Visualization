using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.XR;
using pointcloud_objects;

public class ControllerHandler : MonoBehaviour {

    private SteamVR_TrackedObject trackedObj;
    public GameObject collidingObject;
    public GameObject objectInHand;
    public GameObject spaceManager;
    private Vector3 initialPos;
    public Material lineMaterial;
    private int mode = 0;
    private int photoIndex = 0;
    private bool saveObjects = false;
	private bool pointingOnMenu = false;
    private GameObject slider;
    public Material red; //TODO might be annoying to have material as public on a controller?

    private bool firstPointSet = false;
    private bool hitBar = false;
    private float time = 0f;
    private List<GameObject> tempSelectedBars;
    private float tempMaxAge = 0f;
    private float tempMinAge = float.MaxValue;
    private bool selecting = false;

    private Vector3 tempPos; //TODO CHANGE
    private Quaternion tempRot;

    private GameObject currentRayObject;

    private SteamVR_Controller.Device Controller {
        get { return SteamVR_Controller.Input((int)trackedObj.index); }

    }

    void Awake() {

        trackedObj = GetComponent<SteamVR_TrackedObject>();


    }

    void Start() {
        tempSelectedBars = new List<GameObject>();

    }

    // Update is called once per frame
    void Update() {

        if (slider) {
            if (Controller.GetHairTriggerUp()) {
                slider = null;
            }
        }

		pointingOnMenu = false;
        RaycastHit[] hits;
        hits = Physics.RaycastAll(transform.position, transform.forward, 5.0F);

        for (int i = 0; i < hits.Length; i++) {
            RaycastHit hit = hits[i];
            GameObject hitObj = hit.collider.gameObject;

            if (currentRayObject != null)
            {
                if (currentRayObject.GetComponent<IUIButton>() != null)
                    currentRayObject.GetComponent<IUIButton>().DeSelect();
            }

            //Checks if we hit a button or a checkbox
            if (hitObj.GetComponent<UIButton>()) {
                hitObj.GetComponent<UIButton>().HighLightButton();
                if (Controller.GetHairTriggerDown()) {

                    hitObj.GetComponent<UIButton>().SelectButton();
                }
				pointingOnMenu = true;
            } else if (hitObj.GetComponent<UICheckBox>()) {
                hitObj.GetComponent<UICheckBox>().HighLightButton();
                if (Controller.GetHairTriggerDown()) {

                    hitObj.GetComponent<UICheckBox>().SelectButton();
                }
				pointingOnMenu = true;
            } else if (hitObj.GetComponent<UIRadioButton>()) {
                hitObj.GetComponent<UIRadioButton>().HighLightButton();
                if (Controller.GetHairTriggerDown()) {

                    hitObj.GetComponent<UIRadioButton>().SelectButton();
                }
                pointingOnMenu = true;
            }



            if (hitObj.GetComponent<GraphBarHandler>()) {

                if (Controller.GetHairTriggerDown() && tempSelectedBars != null) {

                    foreach (GameObject o in tempSelectedBars) {
                        if (o) { //This check stops nullpointer when we have a graph and delete it (Objects will still be here since we dont reset until we try to select new objects)
                            o.GetComponent<GraphBarHandler>().Deselect();
                            o.GetComponent<GraphBarHandler>().selected = false;
                        }
                        
                    }
                    tempSelectedBars = new List<GameObject>();
                    selecting = true;
                }

                if (Controller.GetHairTriggerDown() && selecting) {
                        hitObj.GetComponent<Renderer>().sharedMaterial.color = Color.red;
                        hitObj.GetComponent<GraphBarHandler>().selected = true;
                        if (tempMaxAge < hitObj.GetComponent<GraphBarHandler>().maxAge) {
                            tempMaxAge = hitObj.GetComponent<GraphBarHandler>().maxAge;
                        }
                        if (tempMinAge > hitObj.GetComponent<GraphBarHandler>().minAge) {
                            tempMinAge = hitObj.GetComponent<GraphBarHandler>().minAge;
                        }
                        tempSelectedBars.Add(hitObj);
                   

                } else if (!hitObj.GetComponent<GraphBarHandler>().selected && !Controller.GetHairTrigger()) {
                    hitObj.GetComponent<GraphBarHandler>().HighlightBar();

                }

                if (Controller.GetHairTriggerDown() && !selecting)
                {
                    selecting = true;
                }



                GetComponent<LineRenderer>().enabled = true;
                Vector3[] positions = new Vector3[2];
                positions[1] = transform.position;
                positions[0] = hit.point;
                GetComponent<LineRenderer>().SetPositions(positions);
                GetComponent<LineRenderer>().material.SetTextureScale("_MainTex", new Vector2(10, 1.0f));
                GetComponent<LineRenderer>().material.SetTextureOffset("_MainTex", new Vector2(1 * time, 1.0f));

                hitBar = true;
            }


            if (hitObj.GetComponent<SliderHandle>()) {
                if (Controller.GetHairTriggerDown()) {
                    slider = hitObj;
                }
            }

            if (hitObj.GetComponent<CursorTarget>()) {
                hitObj.GetComponent<CursorTarget>().UpdateLocation(hit.point);
				pointingOnMenu = true;

                //The check before sees that we are on the panel so we can actually get a hit.point, and here we update the slider location below if we are currently grabbing it
                if (slider) {
                    slider.GetComponent<SliderHandle>().UpdateSliderLocation(hit.point);
                }
            }

            if (!hitBar && GetComponent<LineRenderer>().enabled) {
                GetComponent<LineRenderer>().enabled = false;
                time = 0f;
            } else {
                time += Time.deltaTime;
            }
            

        }

        //If we have selected and release trigger then we will display (observe that it needs to be outside of the getcomponent if)
        if (Controller.GetHairTriggerUp() && tempSelectedBars.Count != 0 && selecting) {
            tempPos = spaceManager.GetComponent<SpaceUtilities>().dataLoader.GetComponent<DataLoader>().GetCurrentDataSet().transform.position;
            tempRot = spaceManager.GetComponent<SpaceUtilities>().dataLoader.GetComponent<DataLoader>().GetCurrentDataSet().transform.rotation;
            spaceManager.GetComponent<SpaceUtilities>().dataLoader.GetComponent<DataLoader>().loadScene(tempMaxAge, tempMinAge, true, tempPos, tempRot);
            tempMinAge = float.MaxValue;
            tempMaxAge = 0f;
            selecting = false;
            
        }

        hitBar = false;
        if (!pointingOnMenu && !selecting) {
            if (mode == 7) {
                if (Controller.GetHairTriggerDown()) {
                    if (collidingObject) {
                        if (collidingObject.tag.Equals("Mark")) {
                            spaceManager.GetComponent<Resizer>().resizeToolAction(transform, gameObject, collidingObject);
                        }
                    } 

                }
                if (Controller.GetHairTriggerUp()) {
                    if (spaceManager.GetComponent<Resizer>().IsResizing()) {
                        spaceManager.GetComponent<Resizer>().resizeToolAction(transform, gameObject, null);
                    }
                    
                }
            }


			if (mode == 6) {
				if (Controller.GetHairTriggerDown ()) {
					if (collidingObject) {
						if (collidingObject.tag.Equals ("Mark")) {
                            spaceManager.GetComponent<MarkTool>().MarkToolAction(collidingObject);

						}
					}
				}
			}

			if (mode == 5) {
				//TODO we want to highlight selection
				if (Controller.GetHairTriggerDown ()) {
					if (collidingObject) {
						if (collidingObject.tag.Equals ("Mark")) {
							if (collidingObject.GetComponent<HasA> ()) {
								Destroy (collidingObject.GetComponent<HasA> ().hasObj);
							}
                            if (collidingObject.GetComponent<MarkType>()) {
                                if (collidingObject.GetComponent<MarkType>().hasNote) {
                                    Destroy(collidingObject.GetComponent<MarkType>().note);
                                }
                            }
							
							Destroy (collidingObject);
							collidingObject = null;
						}
					}
				}
			}

			if (mode == 4) {
				if (Controller.GetHairTriggerDown ()) {
					ScreenCapture.CaptureScreenshot ("screenshot" + photoIndex, 1);
                    //Debug.Log("screenshotted");
					photoIndex++;
				}

			}

			if (mode == 3) {
				if (Controller.GetHairTriggerDown ()) {
					//Debug.Log ("trigger down");
					if (collidingObject) {
						ReleaseObject ();
						GrabObject ();
                        if (objectInHand.tag.Equals("Star")) { spaceManager.GetComponent<SpaceUtilities>().SetCurrentDataset(objectInHand); }
					}
				}

				if (Controller.GetHairTriggerUp ()) {
					//Debug.Log ("trigger down");
					if (objectInHand) {
						ReleaseObject ();
					}
				}
			}
		}

        if (mode == 2) {
            
            if (Controller.GetHairTriggerDown() && !pointingOnMenu && !selecting) {
                if (!firstPointSet) {
                    spaceManager.GetComponent<BoxSelection>().setFirstPosition(transform.position);
                    firstPointSet = true;
                } else {
                    spaceManager.GetComponent<BoxSelection>().setSecondPosition(transform.position);
                    spaceManager.GetComponent<BoxSelection>().drawBox();
                    saveObjects = spaceManager.GetComponent<BoxSelection>().GetSaveObjects();
                    //This make us reload while only showing stuff in the selected area
                    GameObject tempBox = spaceManager.GetComponent<BoxSelection>().GetBox();
                    if (!saveObjects) {
                        
                        GameObject tmp = spaceManager.GetComponent<PlayerParts>().dataPoints;
                        tempPos = tmp.transform.position;
                        tempRot = tmp.transform.rotation;
                        tempBox.transform.parent = tmp.transform;
                        tmp.transform.position = new Vector3(0, 0, 0);
                        tmp.transform.rotation = Quaternion.Euler(0, 0, 0);
                        tempBox.transform.parent = null;
                        tmp.transform.rotation = tempRot;
                        tmp.transform.position = tempPos;
                        

                        spaceManager.GetComponent<SpaceUtilities>().dataLoader.GetComponent<DataLoader>().loadScene(tempPos, tempRot, tempBox); 
                        spaceManager.GetComponent<BoxSelection>().destroyBox();
                    } else {
                        spaceManager.GetComponent<BoxSelection>().saveBox();
                    }
                    firstPointSet = false;
                  
                }
            }
            if (firstPointSet) {
                spaceManager.GetComponent<BoxSelection>().UpdateBoxSelection(transform.position);
            }
            

        } else {
            firstPointSet = false;
        }

        if (mode == 1) {
			if (Controller.GetHairTriggerDown() && !pointingOnMenu && !selecting) {
                spaceManager.GetComponent<MeasureTool>().MeasureToolAction(transform.position);
            }

            spaceManager.GetComponent<MeasureTool>().UpdateMeasureTool(transform.position);
        }

        //This is temporary to let us scale ourself up/down and move ourself in the y-axis
        if (Controller.GetPress(SteamVR_Controller.ButtonMask.Touchpad)) {
            float tiltX = Controller.GetAxis(Valve.VR.EVRButtonId.k_EButton_Axis0).x;
            float tiltY = Controller.GetAxis(Valve.VR.EVRButtonId.k_EButton_Axis0).y;
            if (transform.parent.localScale.x > 0.5f && tiltY < -0.5) { //TODO might tweak this value
                transform.parent.localScale = transform.parent.localScale + new Vector3(1f,1f,1f) *(tiltY * 1f * Time.deltaTime);
                spaceManager.GetComponent<PlayerParts>().ScaleWithPlayer();


            } else if (transform.parent.localScale.x < 10f && tiltY > 0.5) {
                transform.parent.localScale = transform.parent.localScale + new Vector3(1f, 1f, 1f) * (tiltY * 1f * Time.deltaTime); //TODO might add speed variable instead
                spaceManager.GetComponent<PlayerParts>().ScaleWithPlayer();
            }

            //TODO temporary to test scale and height etc
            if (tiltX > 0.5 || tiltX < -0.5) {
                transform.parent.position = transform.parent.position + new Vector3(0f, 1f, 0f) * (tiltX * 1f * Time.deltaTime);
            }
            
        }

        if (Controller.GetPress(SteamVR_Controller.ButtonMask.ApplicationMenu)) {
            //TODO something
        }
    }

    public void SetMode(int modeIndex) {
        mode = modeIndex;
        spaceManager.GetComponent<MeasureTool>().DestroyCurrentMeasuring();
        spaceManager.GetComponent<BoxSelection>().DestroyCurrentSelection();
    }

    public void ToggleSaveObject() {
        //Temporary solution, probably save induvidual parts later?
        if (saveObjects) {
            saveObjects = false;
            
        } else {
            saveObjects = true;
        }
        spaceManager.GetComponent<BoxSelection>().ToggleSaveObjects();
        
    }

    public void VibrateController(ushort vibrationTime) {
        Controller.TriggerHapticPulse(vibrationTime);
    }

    //Controller mechanism
    private void SetCollidingObject(Collider col) {
        if (!col.GetComponent<Rigidbody>()) {
            return;
        }
        if (collidingObject) {
            if (!collidingObject.tag.Equals("Mark") && col.tag.Equals("Mark")) {
                collidingObject = col.gameObject; 
            } else if (!collidingObject.tag.Equals("Mark")) { 
                if (Vector3.Distance(col.gameObject.transform.position, transform.position) < Vector3.Distance(collidingObject.transform.position, transform.position)) {

                    collidingObject = col.gameObject; 
                }
            }
            
        } else {
            collidingObject = col.gameObject;
        }
        

        

    }

    public void OnTriggerEnter(Collider other) {
        SetCollidingObject(other);
    }


    public void OnTriggerStay(Collider other) {
        SetCollidingObject(other);
    }

    public void OnTriggerExit(Collider other) {
        if (!collidingObject) {
            return;
        }
        collidingObject = null;
    }

    private void GrabObject() {
        objectInHand = collidingObject;
        collidingObject = null;

        if (objectInHand.GetComponent<Rigidbody>().isKinematic) {
            objectInHand.GetComponent<Rigidbody>().isKinematic = false;
            objectInHand.GetComponent<Rigidbody>().useGravity = true;
        }


        var joint = AddFixedJoint();
        joint.connectedBody = objectInHand.GetComponent<Rigidbody>();
    }


    private FixedJoint AddFixedJoint() {
        FixedJoint fx = gameObject.AddComponent<FixedJoint>();

        fx.breakForce = 200000000;
        fx.breakTorque = 200000000;
        return fx;
    }

    public void ReleaseObject() {


        if (GetComponent<FixedJoint>()) {


            GetComponent<FixedJoint>().connectedBody = null;
            Destroy(GetComponent<FixedJoint>());

            if (objectInHand.tag.Equals("Star") || objectInHand.tag.Equals("Mark")) { //TODO could optimize here?
                objectInHand.GetComponent<Rigidbody>().isKinematic = true;
                objectInHand.GetComponent<Rigidbody>().useGravity = false;
                objectInHand.GetComponent<Rigidbody>().velocity = Vector3.zero;
                objectInHand.GetComponent<Rigidbody>().angularVelocity = Vector3.zero;
            } else {
                objectInHand.GetComponent<Rigidbody>().velocity = Controller.velocity;
                objectInHand.GetComponent<Rigidbody>().angularVelocity = Controller.angularVelocity;
            }
            
        }

        objectInHand = null;
    }

    public void putObjectInHand(GameObject newObject) {
        objectInHand = newObject;

        var joint = AddFixedJoint();
        joint.connectedBody = objectInHand.GetComponent<Rigidbody>();
    }
}
