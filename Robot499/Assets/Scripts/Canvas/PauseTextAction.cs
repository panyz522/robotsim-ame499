using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;

public class PauseTextAction : MonoBehaviour {

	// Use this for initialization
	void Start () {
		
	}
	
	// Update is called once per frame
	void Update () {
	}

    public void Toggle()
    {
        var t = GetComponent<Text>();

        if (t.text == "Pause")
        {
            t.text = "Resume";
        }
        else
        {
            t.text = "Pause";
        }
    }
}
