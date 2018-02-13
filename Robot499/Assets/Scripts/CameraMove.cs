using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class CameraMove : MonoBehaviour {

    private Vector3 offset;
    public GameObject robotBody;

	// Use this for initialization
	void Start () {
        offset = transform.position - robotBody.transform.position;
	}
	
	// Update is called once per frame
	void Update () {
        transform.position = robotBody.transform.position + offset;
	}
}
