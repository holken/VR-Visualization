using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class CursorTarget : MonoBehaviour {

    public GameObject cursor;
    private bool cursorTouched;
    public GameObject rightController;
    private LineRenderer line;
    public float tileAmount = 1.0f;
    public float animationAmount = 1.0f;
    private float originalAnimationAmount;
    private float time = 0f;
    public AudioClip[] audios;
    // Use this for initialization
    void Start() {
        line = GetComponent<LineRenderer>();
        originalAnimationAmount = animationAmount;
    }

    public float GetOriginalAnimationAmount() {
        return originalAnimationAmount;
    }

    void LateUpdate() {
        if (!cursorTouched) {
            cursor.SetActive(false);
            line.enabled = false;
        } else {
            if (!cursor.activeSelf) {
                cursor.SetActive(true);
            }
            time += Time.deltaTime; //TODO reset so we dont count forever?
            line.SetPosition(0, cursor.transform.position);
            line.SetPosition(1, rightController.transform.position);
            line.material.SetTextureScale("_MainTex", new Vector2(tileAmount, 1.0f));
            line.material.SetTextureOffset("_MainTex", new Vector2(animationAmount*time, 1.0f));
        }
        cursorTouched = false;
    }

    public void UpdateLocation(Vector3 hitLocation) {
        cursor.transform.position = hitLocation;
        cursorTouched = true;
        line.enabled = true;
    }

    public void PlayHoverSound() {
        GetComponent<AudioSource>().clip = audios[1];
        GetComponent<AudioSource>().Play();
    }

    public void PlaySelectSound() {
        GetComponent<AudioSource>().clip = audios[0];
        GetComponent<AudioSource>().Play();
    }
}
