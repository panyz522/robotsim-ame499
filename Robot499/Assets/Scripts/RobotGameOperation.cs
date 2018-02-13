using System.Collections;
using System;
using System.IO;
using System.Collections.Generic;
using UnityEngine;

public class RobotGameOperation : MonoBehaviour {
    public GameObject body;
    public bool recordAngles;
    public float angleMeasureDeltaTime;

    private IRobotController controller;
    private List<float>[] angles;
    private List<float> times;
    private float lastMeasureTime;

	// Use this for initialization
	void Start () {
        controller = GetComponent<IRobotController>();
        angles = new List<float>[8];
        for (int i = 0; i < 8; i++)
            angles[i] = new List<float>();
        times = new List<float>();
	}
	
	// Update is called once per frame
	void Update () {
        if (recordAngles)
        {
            RecordAngles();
        }
	}

    public void OutputAngles()
    {
        recordAngles = false;

        for (int i_leg = 0; i_leg < 8; i_leg++)
        {
            using (var fs = File.Open(string.Format("c:\\Users\\Turnip\\Desktop\\Angles_{0}.txt", i_leg), FileMode.Create))
            {
                using (StreamWriter sw = new StreamWriter(fs))
                {
                    for (int i = 0; i < angles[i_leg].Count; i++)
                    {
                        sw.WriteLine(angles[i_leg][i]);
                    }
                }
            }
        }
        using (var fs = File.Open("c:\\Users\\Turnip\\Desktop\\Times.txt", FileMode.Create))
        {
            using (StreamWriter sw = new StreamWriter(fs))
            {
                for (int i = 0; i < times.Count; i++)
                {
                    sw.WriteLine(times[i]);
                }
            }
        }
    }

    private void RecordAngles()
    {
        var time = Time.realtimeSinceStartup;
        if (time < lastMeasureTime + angleMeasureDeltaTime)
        {
            return;
        }
        lastMeasureTime = time;
        for (int i = 0; i < 8; i++)
        {
            angles[i].Add(controller.GetServoAngle((i) / 2, i % 2));
        }
        times.Add(time);
        Debug.Log(angles[1].Count);
    }

    public void SetFree()
    {
        var rb = body.GetComponent<Rigidbody>();
        rb.isKinematic = false;
    }
}
