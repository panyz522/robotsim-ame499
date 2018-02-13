using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;

public class GlobalScript : MonoBehaviour {
    public Text timeText;
    public GameObject workingRobot;

    private float startTime = 0;
	// Use this for initialization
	void Start () {
	}

    private void ShowTime()
    {
        if (startTime == 0)
        {
            timeText.text = "0";
            return;
        }
        timeText.text = String.Format("{0:0.0}",Time.realtimeSinceStartup - startTime);
    }

    // Update is called once per frame
    void Update ()
    {
        ShowTime();
    }

    public void StartCountTime()
    {
        startTime = Time.realtimeSinceStartup;
    }
}
