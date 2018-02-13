using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;

public class RobotController2 : MonoBehaviour, IRobotController
{
    public Text debugText;
    public List<float> lengthLegFront;
    public List<float> lengthLegRear;
    public float frontOffset;
    public float rearOffset;
    public float steplength;
    public float height;
    public float moveV;

    private GameObject[,] Legs = new GameObject[4, 2];
    private MotorController[,] Motors = new MotorController[4, 2];
    private LegGroupInfo[] LegGroups = new LegGroupInfo[4];
    private float time;
    private float stepOffset = 0.5f;
    private Vector2[] footPositions;

    // Use this for initialization
    void Start()
    {
        // Init storage for exporting data
        footPositions = new Vector2[4];

        // Init Legs' data
        for (int i = 0; i < 4; i++)
        {
            for (int j = 0; j < 2; j++)
            {
                Legs[i, j] = GameObject.Find("Leg" + i + j);
                Motors[i, j] = Legs[i, j].GetComponent<MotorController>();
            }
            LegGroups[i] = new LegGroupInfo()
            {
                programData = new Dictionary<string, float>(),
                moveProgram = MoveProgram.None
            };
            footPositions[i] = new Vector2();
        }
    }

    private int state = 0;
    private bool isReady = false;
    private bool isStart = false;
    private bool isPause = false;
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
        foreach (var leg in LegGroups)
        {
            if (leg.moveEndTime > time)
            {
                isLegsAvailable = false;
                break;
            }
        }

        var topStepPos = steplength * 3 / 2;
        if (isLegsAvailable)
        {
            switch (state)
            {
                case 0:
                    FootMove(0, topStepPos - 3 * steplength + frontOffset, moveV, MoveProgram.Horizontal);
                    FootMove(1, topStepPos - 1 * steplength + frontOffset, moveV, MoveProgram.Horizontal);
                    FootMove(2, topStepPos - 0 * steplength + rearOffset, moveV, MoveProgram.Horizontal);
                    FootMove(3, topStepPos - 2 * steplength + rearOffset, moveV, MoveProgram.Horizontal);
                    state = 1;
                    break;
                case 1:
                    FootMove(0, topStepPos - 0 * steplength + frontOffset, moveV, MoveProgram.Step);
                    FootMove(1, topStepPos - 2 * steplength + frontOffset, moveV, MoveProgram.Horizontal);
                    FootMove(2, topStepPos - 1 * steplength + rearOffset, moveV, MoveProgram.Horizontal);
                    FootMove(3, topStepPos - 3 * steplength + rearOffset, moveV, MoveProgram.Horizontal);
                    state = 3;
                    break;
                case 3:
                    FootMove(0, topStepPos - 1 * steplength + frontOffset, moveV, MoveProgram.Horizontal);
                    FootMove(1, topStepPos - 3 * steplength + frontOffset, moveV, MoveProgram.Horizontal);
                    FootMove(2, topStepPos - 2 * steplength + rearOffset, moveV, MoveProgram.Horizontal);
                    FootMove(3, topStepPos - 0 * steplength + rearOffset, moveV, MoveProgram.Step);
                    state = 4;
                    break;
                case 4:
                    FootMove(0, topStepPos - 2 * steplength + frontOffset, moveV, MoveProgram.Horizontal);
                    FootMove(1, topStepPos - 0 * steplength + frontOffset, moveV, MoveProgram.Step);
                    FootMove(2, topStepPos - 3 * steplength + rearOffset, moveV, MoveProgram.Horizontal);
                    FootMove(3, topStepPos - 1 * steplength + rearOffset, moveV, MoveProgram.Horizontal);
                    state = 6;
                    break;
                case 6:
                    FootMove(0, topStepPos - 3 * steplength + frontOffset, moveV, MoveProgram.Horizontal);
                    FootMove(1, topStepPos - 1 * steplength + frontOffset, moveV, MoveProgram.Horizontal);
                    FootMove(2, topStepPos - 0 * steplength + rearOffset, moveV, MoveProgram.Step);
                    FootMove(3, topStepPos - 2 * steplength + rearOffset, moveV, MoveProgram.Horizontal);
                    state = 1;
                    break;
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

    public Vector2[] GetFootPositions()
    {
        return footPositions;
    }

    public float GetHeight()
    {
        return height;
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
            y = height;
        }
        else
        {
            var r = stepOffset;
            var dir = (x3 - x0 > 0) ? 1 : -1;

            var s = Math.PI * r + Math.Abs(x3 - x0) - 2 * r;
            double t1, t2; t1 = Math.PI * r / s / 2 * (t3 - t0) + t0;
            t2 = Math.PI * r / s / 2 * (t0 - t3) + t3;
            leg.programData["t1"] = (float)t1;
            leg.programData["t2"] = (float)t2;

            if (time < t1)
            {
                x = x0 + dir * r * (1.0 - Math.Cos((time - t0) / (t1 - t0) * Math.PI / 2));
                y = height - r * Math.Sin((time - t0) / (t1 - t0) * Math.PI / 2);
            }
            else if (time < t2)
            {
                x = (time - t1) / (t2 - t1) * (Math.Abs(x3 - x0) - 2.0 * r) * dir + dir * r + x0;
                y = height - r;
            }
            else
            {
                x = x3 + dir * r * (-1.0 + Math.Sin((time - t2) / (t3 - t2) * Math.PI / 2));
                y = height - r * Math.Cos((time - t2) / (t3 - t2) * Math.PI / 2);
            }
        }

        SetMotorByPos(i, (float)x, (float)y, i);
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
            leg.programData = new Dictionary<string, float>();
            x = x1;
        }
        else
            x = (time - t0) / (t1 - t0) * (x1 - x0) + x0;

        // Update motor state
        SetMotorByPos(i, x, height, i);
    }

    private bool GetBendDir(int i)
    {
        bool bendDir = true;
        if (i < 2)
            bendDir = false;
        return bendDir;
    }

    private void SetMotorByPos(int i, float x, float y, int legGroup)
    {
        // Set angles
        double alpha1 = 0, alpha2 = 0;
        GetAngleByDist(ref alpha1, ref alpha2, x, y, legGroup);
        Motors[i, 0].targetAngle = (float)(alpha1);
        Motors[i, 1].targetAngle = (float)(alpha2);

        // Log positions
        footPositions[legGroup] = new Vector2(x, y);
    }

    private void GetAngleByDist(ref double alpha1, ref double alpha2, float len, float hei, int legGroup)
    {
        float dirSign = GetBendDir(legGroup) ? 1 : -1;
        float g1, g2;
        GetLegLength(legGroup, out g1, out g2);
        var g1p2 = g1 * g1;
        var g2p2 = g2 * g2;
        var g1g2 = g1 * g2;
        var hl2p2sum = len * len + hei * hei;
        //(g1^2 - 2 g1 g2 + g2^2 - h^2 - l^2) (g1^2 + 2 g1 g2 + g2^2 - h^2 - l ^ 2)
        var mid = Math.Sqrt(-(g1p2 - 2 * g1g2 + g2p2 - hl2p2sum) * (g1p2 + 2 * g1g2 + g2p2 - hl2p2sum));

        alpha1 = 2 * Math.Atan((dirSign * mid + 2 * g1 * len) / (g1p2 + 2 * g1 * hei - g2p2 + hl2p2sum));
        alpha2 = 2 * Math.Atan(mid / (g1p2 - 2 * g1g2 + g2p2 - hl2p2sum)) * dirSign;
        alpha1 *= 180 / Math.PI;
        alpha2 *= 180 / Math.PI;
    }

    private void GetLegLength(int legGroup, out float g1, out float g2)
    {
        if (legGroup < 2)
        {
            g1 = lengthLegFront[0];
            g2 = lengthLegFront[1];
        }
        else
        {
            g1 = lengthLegRear[0];
            g2 = lengthLegRear[1];
        }
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
