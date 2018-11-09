using System.Collections;
using System.Collections.Generic;
using UnityEngine;

[System.Serializable] //This tells Unity that this class can be serialized, which means you can turn it into a stream of bytes and save it to a file on disk.
public class SaveManager{

    /*The player*/

    //Current player position
    public float playerPosX;
    public float playerPosY;
    public float playerPosZ;
    //Current player rotation
    public float playerRotX;
    public float playerRotY;
    public float playerRotZ;
    public float playerRotW;
    //Current player scale
    public float playerScaleX;
    public float playerScaleY;
    public float playerScaleZ;

    /*The marks */

    public List<int> typeId = new List<int>();
    //Current mark position
    public List<float> markPosX = new List<float>();
    public List<float> markPosY = new List<float>();
    public List<float> markPosZ = new List<float>();
    //Current mark rotation
    public List<float> markRotX = new List<float>();
    public List<float> markRotY = new List<float>();
    public List<float> markRotZ = new List<float>();
    public List<float> markRotW = new List<float>();
    //Current mark scale
    public List<float> markScaleX = new List<float>();
    public List<float> markScaleY = new List<float>();
    public List<float> markScaleZ = new List<float>();

    //Saves the text for measurements
    public List<string> measureText = new List<string>();
    public List<float> textPosX = new List<float>();
    public List<float> textPosY = new List<float>();
    public List<float> textPosZ = new List<float>();

    public List<float> textRotX = new List<float>();
    public List<float> textRotY = new List<float>();
    public List<float> textRotZ = new List<float>();
    public List<float> textRotW = new List<float>();

    public List<float> textScaleX = new List<float>();
    public List<float> textScaleY = new List<float>();
    public List<float> textScaleZ = new List<float>();
    //Saves a bool if a mark has a note or not
    public List<bool> hasNote = new List<bool>();

    //Saves the text for notes
    public List<string> notes = new List<string>();
    //We don't need pos, it's relative to the object it is attached to
    public List<float> noteRotX = new List<float>();
    public List<float> noteRotY = new List<float>();
    public List<float> noteRotZ = new List<float>();
    public List<float> noteRotW = new List<float>();


}
