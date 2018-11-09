using System.Collections;
using System.Collections.Generic;
using System.Runtime.Serialization.Formatters.Binary;
using System.IO;
using UnityEngine;
using UnityEngine.UI;

public class SaveHandler : MonoBehaviour {

    //TODO optimize SaveGame hard
    public void SaveGame(int savePosition) {
        SaveManager saveManager = new SaveManager();


        object[] obj = GameObject.FindObjectsOfType(typeof(GameObject));
        int count = 0;
        int count2 = 0;
        foreach (object o in obj) {

            GameObject g = (GameObject)o;

            if (g.tag.Equals("Mark")) {
                saveManager.typeId.Add(g.GetComponent<MarkType>().typeId);

                saveManager.markPosX.Add(g.transform.position.x);
                saveManager.markPosY.Add(g.transform.position.y);
                saveManager.markPosZ.Add(g.transform.position.z);

                saveManager.markRotX.Add(g.transform.rotation.x);
                saveManager.markRotY.Add(g.transform.rotation.y);
                saveManager.markRotZ.Add(g.transform.rotation.z);
                saveManager.markRotW.Add(g.transform.rotation.w);

                saveManager.markScaleX.Add(g.transform.localScale.x);
                saveManager.markScaleY.Add(g.transform.localScale.y);
                saveManager.markScaleZ.Add(g.transform.localScale.z);

                if (g.GetComponent<MarkType>().hasNote) {
                    saveManager.notes.Add(g.GetComponent<MarkType>().note.transform.GetChild(2).GetComponent<Text>().text);
                    saveManager.noteRotX.Add(g.GetComponent<MarkType>().note.transform.rotation.x);
                    saveManager.noteRotY.Add(g.GetComponent<MarkType>().note.transform.rotation.y);
                    saveManager.noteRotZ.Add(g.GetComponent<MarkType>().note.transform.rotation.z);
                    saveManager.noteRotW.Add(g.GetComponent<MarkType>().note.transform.rotation.w);
                    saveManager.hasNote.Add(true); 

                } else {
                    saveManager.hasNote.Add(false);
                }
                

                if (g.GetComponent<MarkType>().typeId == 2) {
                    GameObject hasObj = g.GetComponent<HasA>().hasObj;
                    saveManager.measureText.Add(hasObj.GetComponent<TextMesh>().text);

                    saveManager.textPosX.Add(hasObj.transform.position.x);
                    saveManager.textPosY.Add(hasObj.transform.position.y);
                    saveManager.textPosZ.Add(hasObj.transform.position.z);

                    saveManager.textRotX.Add(hasObj.transform.rotation.x);
                    saveManager.textRotY.Add(hasObj.transform.rotation.y);
                    saveManager.textRotZ.Add(hasObj.transform.rotation.z);
                    saveManager.textRotW.Add(hasObj.transform.rotation.w);

                    saveManager.textScaleX.Add(hasObj.transform.localScale.x);
                    saveManager.textScaleY.Add(hasObj.transform.localScale.y);
                    saveManager.textScaleZ.Add(hasObj.transform.localScale.z);
                    count2++;
                }

                //TODO check if have note

            } else if (g.tag.Equals("Player")) {
                saveManager.playerPosX = g.transform.position.x;
                saveManager.playerPosY = g.transform.position.y;
                saveManager.playerPosZ = g.transform.position.z;

                saveManager.playerRotX = g.transform.rotation.x;
                saveManager.playerRotY = g.transform.rotation.y;
                saveManager.playerRotZ = g.transform.rotation.z;
                saveManager.playerRotW = g.transform.rotation.w;

                saveManager.playerScaleX = g.transform.localScale.x;
                saveManager.playerScaleY = g.transform.localScale.y;
                saveManager.playerScaleZ = g.transform.localScale.z;
            }
            
            count++;
        }
        BinaryFormatter bf = new BinaryFormatter();
        FileStream file = File.Create(Application.persistentDataPath + "/gamesave" + savePosition + ".save");
        bf.Serialize(file, saveManager);
        file.Close();
        Debug.Log("Game Saved");

    }

    public void LoadGame(int savePosition) {

        //TODO, add support for multiple saves
        if (File.Exists(Application.persistentDataPath + "/gamesave" + savePosition + ".save")) {
            ClearScene();
            BinaryFormatter bf = new BinaryFormatter();
            FileStream file = File.Open(Application.persistentDataPath + "/gamesave" + savePosition + ".save", FileMode.Open);
            SaveManager save = (SaveManager)bf.Deserialize(file);
            file.Close();

           
            //Set player position, rotation, and scale;
            GameObject player = GetComponent<PlayerParts>().body;
            player.transform.position = new Vector3(save.playerPosX, save.playerPosY, save.playerPosZ);
            player.transform.eulerAngles = new Vector3(save.playerRotX, save.playerRotY, save.playerRotZ);
            player.transform.localScale = new Vector3(save.playerScaleX, save.playerScaleY, save.playerScaleZ);
            int count2 = 0;
            int count3 = 0;
            for (int i = 0; i < save.typeId.Count; i++) {

                GameObject tempObj;
                int tempTypeId = save.typeId[i];
                
                if (tempTypeId == 1) {
                    tempObj = (GameObject)Instantiate(GameObject.CreatePrimitive(PrimitiveType.Cube));
                    tempObj.AddComponent<MarkType>();
                    tempObj.GetComponent<MarkType>().typeId = 1;
                } else if (tempTypeId == 2) {
                    tempObj = (GameObject)Instantiate(Resources.Load("MeasureCube"));
                    tempObj.AddComponent<MarkType>();
                    tempObj.GetComponent<MarkType>().typeId = 2;
                } else {
                    tempObj = new GameObject();
                }

                tempObj.transform.position = new Vector3(save.markPosX[i], save.markPosY[i], save.markPosZ[i]);
                tempObj.transform.rotation = new Quaternion(save.markRotX[i], save.markRotY[i], save.markRotZ[i], save.markRotW[i]);
                tempObj.transform.localScale = new Vector3(save.markScaleX[i], save.markScaleY[i], save.markScaleZ[i]);
                tempObj.AddComponent<Rigidbody>();
                tempObj.GetComponent<Rigidbody>().isKinematic = true;
                tempObj.GetComponent<Rigidbody>().useGravity = false;
                if (tempTypeId == 2) {
                    tempObj.AddComponent<HasA>();
                    GameObject tempText = (GameObject)Instantiate(Resources.Load("ControllerText"));
                    tempText.GetComponent<TextMesh>().text = save.measureText[count2];
                    tempText.transform.position = new Vector3(save.textPosX[count2], save.textPosY[count2], save.textPosZ[count2]);
                    tempText.transform.rotation = new Quaternion(save.textRotX[count2], save.textRotY[count2], save.textRotZ[count2], save.textRotW[count2]);
                    tempText.transform.localScale = new Vector3(save.textScaleX[count2], save.textScaleY[count2], save.textScaleZ[count2]);
                    tempObj.GetComponent<HasA>().hasObj = tempText;
                    count2++;
                } else {
                    tempObj.tag = "Mark";
                    tempObj.layer = 8;
                }

                if (save.hasNote[i]) {
                    tempObj.GetComponent<MarkType>().hasNote = true;
                    GameObject tempNote = (GameObject)Instantiate(Resources.Load("Note"));
                    tempNote.transform.position = tempObj.transform.position;
                    tempNote.transform.rotation = new Quaternion(save.noteRotX[count3], save.noteRotY[count3], save.noteRotZ[count3], save.noteRotW[count3]);
                    tempObj.GetComponent<MarkType>().note = tempNote;
                    count3++;
                }

            }

            Debug.Log("Game Loaded");
        } else {
            Debug.Log("No saved game");
        }
    }

    private void ClearScene() {

        object[] obj = GameObject.FindObjectsOfType(typeof(GameObject));
        foreach (object o in obj) {
            GameObject g = (GameObject)o;

            if (g.tag.Equals("Mark")) {
                if (g.GetComponent<HasA>()) {
                    Destroy(g.GetComponent<HasA>().hasObj);
                }
                if (g.GetComponent<MarkType>().hasNote) {
                    Destroy(g.GetComponent<MarkType>().note);
                }
                Destroy(g);
            }
            
        }
    }
}
