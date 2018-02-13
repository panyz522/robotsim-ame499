using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;

public class RobotController : MonoBehaviour
{
    public Text debugText;

    private GameObject[,] Legs = new GameObject[4, 2];
    private MotorController[,] Motors = new MotorController[4, 2];
    private LegGroupInfo[] LegGroups = new LegGroupInfo[4];
    private float time;
    private float stepOffset = 1;
    private float moveV = 0.45f;

    // Use this for initialization
    void Start()
    {
        for (int i = 0; i < 4; i++)
        {
            for (int j = 0; j < 2; j++)
            {
                Legs[i, j] = GameObject.Find("Leg" + i + j);
                Motors[i, j] = Legs[i, j].GetComponent<MotorController>();
            }
            LegGroups[i] = new LegGroupInfo() {
                programData = new Dictionary<string, float>(),
                moveProgram = MoveProgram.None };
        }
    }

    private int state = 0;
    private bool isReady = false;
    private bool isStart = false;
    private bool isPause = false;
    private bool method = false;
    private float comOffset = -0.2f;
    // Update is called once per frame
    void Update()
    {
        time = Time.realtimeSinceStartup;

        if (!isReady)
        {
            state = 0;
            return;
        }
        if (!isStart)
            state = 0;
        if (isPause)
            return;

        bool isLegsAvailable = true;
        foreach(var leg in LegGroups)
        {
            if (leg.moveEndTime > time)
            {
                isLegsAvailable = false;
                break;
            }
        }

        if (isLegsAvailable)
        {
            if (method)
            {
                switch (state)
                {
                    case 0:
                        FootMove(0, -3, moveV, MoveProgram.Horizontal);
                        FootMove(2, 3, moveV, MoveProgram.Horizontal);
                        FootMove(1, 0.5f, moveV, MoveProgram.Horizontal);
                        FootMove(3, -0.5f, moveV, MoveProgram.Horizontal);
                        state = 1;
                        break;
                    case 1:
                        FootMove(0, 4, moveV, MoveProgram.Step);
                        state = 2;
                        break;
                    case 2:
                        FootMove(0, 0.5f, moveV, MoveProgram.Horizontal);
                        FootMove(1, -3f, moveV, MoveProgram.Horizontal);
                        FootMove(2, -0.5f, moveV, MoveProgram.Horizontal);
                        FootMove(3, -4f, moveV, MoveProgram.Horizontal);
                        state = 3;
                        break;
                    case 3:
                        FootMove(3, 4, moveV, MoveProgram.Step);
                        state = 4;
                        break;
                    case 4:
                        FootMove(1, 4, moveV, MoveProgram.Step);
                        state = 5;
                        break;
                    case 5:
                        FootMove(1, 0.5f, moveV, MoveProgram.Horizontal);
                        FootMove(0, -3f, moveV, MoveProgram.Horizontal);
                        FootMove(3, -0.5f, moveV, MoveProgram.Horizontal);
                        FootMove(2, -4f, moveV, MoveProgram.Horizontal);
                        state = 6;
                        break;
                    case 6:
                        FootMove(2, 4, moveV, MoveProgram.Step);
                        state = 1;
                        break;
                }
            }
            else
            {
                switch (state)
                {
                    case 0:
                        FootMove(0, -2.5f + comOffset, moveV, MoveProgram.Horizontal);
                        FootMove(1, 2.167f + comOffset, moveV, MoveProgram.Horizontal);
                        FootMove(2, 4.5f + comOffset, moveV, MoveProgram.Horizontal);
                        FootMove(3, 0.167f + comOffset, moveV, MoveProgram.Horizontal);
                        state = 1;
                        break;
                    case 1:
                        FootMove(0, 4.5f + comOffset, moveV, MoveProgram.Step);
                        FootMove(1, -0.167f + comOffset, moveV, MoveProgram.Horizontal);
                        FootMove(2, 2.167f + comOffset, moveV, MoveProgram.Horizontal);
                        FootMove(3, -2.5f + comOffset, moveV, MoveProgram.Horizontal);
                        state = 3;
                        break;
                    case 3:
                        FootMove(0, 2.167f + comOffset, moveV, MoveProgram.Horizontal);
                        FootMove(1, -2.5f + comOffset, moveV, MoveProgram.Horizontal);
                        FootMove(2, 0.167f + comOffset, moveV, MoveProgram.Horizontal);
                        FootMove(3, 4.5f + comOffset, moveV, MoveProgram.Step);
                        state = 4;
                        break;
                    case 4:
                        FootMove(0, 0.167f + comOffset, moveV, MoveProgram.Horizontal);
                        FootMove(1, 4.5f + comOffset, moveV, MoveProgram.Step);
                        FootMove(2, -2.5f + comOffset, moveV, MoveProgram.Horizontal);
                        FootMove(3, 2.167f + comOffset, moveV, MoveProgram.Horizontal);
                        state = 6;
                        break;
                    case 6:
                        FootMove(0, -2.5f + comOffset, moveV, MoveProgram.Horizontal);
                        FootMove(1, 2.167f + comOffset, moveV, MoveProgram.Horizontal);
                        FootMove(2, 4.5f + comOffset, moveV, MoveProgram.Step);
                        FootMove(3, 0.167f + comOffset, moveV, MoveProgram.Horizontal);
                        state = 1;
                        break;
                }
            }
        }

        for (int i = 0; i < 4; i++)
        {
            if (LegGroups[i].moveProgram == MoveProgram.Horizontal)
            {
                Update_FootHorizontalMove(i);
            }
            if (LegGroups[i].moveProgram == MoveProgram.Step)
            {
                Update_FootStepMove(i);
            }
        }
    }

    public void SetReady()
    {
        isReady = true;
    }

    public void SetStart()
    {
        isStart = true;
    }

    public void TogglePause()
    {
        isPause = (isPause) ? false : true;
    }

    public float GetServoAngle(int i, int j)
    {
        return Legs[i, j].GetComponent<HingeJoint>().angle;
    }

    // Give leg moving order
    private bool FootMove(int i_leg, float dist, float estTime, MoveProgram prog)
    {
        // If not finish last step
        if (LegGroups[i_leg].moveEndTime > time)
            return false;
        // If leg is available
        else
        {
            var leg = LegGroups[i_leg];
            leg.moveEndTime = estTime + time;
            leg.moveStartTime = time;
            leg.moveProgram = prog;
            leg.startPosition = leg.targetPosition;
            leg.targetPosition = dist / 100;
        }
        return true;
    }

    private void Update_FootStepMove(int i)
    {
        var leg = LegGroups[i];
        var t0 = leg.moveStartTime;
        var t3 = leg.moveEndTime;
        var x0 = leg.startPosition * 100;
        var x3 = leg.targetPosition * 100;

        double x = 0, y = 0;

        // Check if completed and get x, y
        if (time > t3)
        {
            leg.moveProgram = MoveProgram.None;
            leg.programData = new Dictionary<string, float>();
            x = x3;
            y = 8;
        }
        else
        {
            var r = stepOffset;
            var dir = (x3 - x0 > 0) ? 1 : -1;

            var s = Math.PI * r + Math.Abs(x3 - x0) - 2 * r;
            double t1, t2;t1 = Math.PI * r / s / 2 * (t3 - t0) + t0;
            t2 = Math.PI * r / s / 2 * (t0 - t3) + t3;
            leg.programData["t1"] = (float)t1;
            leg.programData["t2"] = (float)t2;

            if (time < t1)
            {
                x = x0 + dir * r * (1.0 - Math.Cos((time - t0) / (t1 - t0) * Math.PI / 2));
                y = 8.0 - r * Math.Sin((time - t0) / (t1 - t0) * Math.PI / 2);
            }
            else if(time < t2)
            {
                x = (time - t1) / (t2 - t1) * (Math.Abs(x3 - x0) - 2.0 * r) * dir + dir * r + x0;
                y = 8.0 - r;
            }
            else
            {
                x = x3 + dir * r * (-1.0 + Math.Sin((time - t2) / (t3 - t2) * Math.PI / 2));
                y = 8 - r * Math.Cos((time - t2) / (t3 - t2) * Math.PI / 2);
            }
        }

        SetMotorByPos(i, (float)x, (float)y);
    }

    private void Update_FootHorizontalMove(int i)
    {
        var leg = LegGroups[i];
        var t0 = leg.moveStartTime;
        var t1 = leg.moveEndTime;
        var x0 = leg.startPosition * 100;
        var x1 = leg.targetPosition * 100;
        float x;

        // Find horizontal distance by time
        if (time > t1)
        {
            leg.moveProgram = MoveProgram.None;
            leg.programData = new Dictionary<string ,float>();
            x = x1;
        }
        else
            x = (time - t0) / (t1 - t0) * (x1 - x0) + x0;

        // Update motor state
        SetMotorByPos(i, x, 8.0f);
    }

    private void SetMotorByPos(int i, float x, float y)
    {
        double alpha1 = 0, alpha2 = 0;
        GetAngleByDist(ref alpha1, ref alpha2, x, y);
        Motors[i, 0].targetAngle = (float)(alpha1);
        Motors[i, 1].targetAngle = (float)(alpha2);
    }

    private void GetAngleByDist(ref double alpha1, ref double alpha2, float hori, float vert)
    {
        var mid1 = Math.Sqrt(-Math.Pow(vert, 4) - 2 * Math.Pow(vert, 2) * Math.Pow(hori, 2) + 100 * Math.Pow(vert, 2) - Math.Pow(hori, 4) + 100 * Math.Pow(hori, 2));
        var mid2 = Math.Pow(hori, 2) + Math.Pow(vert, 2);
        alpha1 = 2 * Math.Atan((mid1 + 10 * hori) / (mid2 + 10 * vert));
        alpha2 = -2 * Math.Atan(mid1 / mid2);
        alpha1 *= 180 / Math.PI;
        alpha2 *= 180 / Math.PI;
    }



    class LegGroupInfo
    {
        public float moveStartTime;
        public float moveEndTime;

        public float startPosition;
        public float targetPosition;

        public MoveProgram moveProgram;
        public Dictionary<string, float> programData;
    }

    enum MoveProgram
    {
        None, Horizontal, Step
    }
}
