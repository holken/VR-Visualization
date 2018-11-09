using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class MarkTool : MonoBehaviour {

	public void MarkToolAction(GameObject collidingObject) {
        GameObject note = (GameObject)Instantiate(Resources.Load("Note"));
        note.transform.position = collidingObject.transform.position;
        note.transform.LookAt(GetComponent<PlayerParts>().head.transform);
        note.transform.rotation = Quaternion.Euler(new Vector3(0f, note.transform.rotation.eulerAngles.y + 180f, 0f));
        note.transform.localScale = note.transform.localScale * GetComponent<PlayerParts>().body.transform.localScale.x;
        collidingObject.GetComponent<MarkType>().note = note;
        collidingObject.GetComponent<MarkType>().hasNote = true;
    }
}
