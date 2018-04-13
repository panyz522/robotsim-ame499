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
    public float stepLength;
    public float subStepHoriRatio;
    public float rightScale;
    public float height;
    public float moveV;

    private GameObject[,] Legs = new GameObject[4, 2];
    private MotorController[,] Motors = new MotorController[4, 2];
    private LegGroupInfo[] LegGroups = new LegGroupInfo[4];
    private float time;
    private float stepOffset = 1f;
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
    private bool isGettingReady = false;
    private float topStepPos;
    private float subStepH;
    private float subStepS;
    // Update is called once per frame
    void Update()
    {
        time = Time.realtimeSinceStartup;

        if (!isReady)
        {
            state = 0;
            return;
        }
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

        topStepPos = stepLength / 2;
        subStepH = stepLength * subStepHoriRatio / 2;
        subStepS = stepLength * (1.0f - subStepHoriRatio) / 3;

        if (isLegsAvailable)
        {
            switch (state)
            {
                case 0:
                    if (isGettingReady)
                    {
                        if (isStart)
                            state = 1;
                        break;
                    }
                    FootMoveAbbr(0, 2, 3, state, MoveProgram.Horizontal);
                    FootMoveAbbr(1, 1, 1, state, MoveProgram.Horizontal);
                    FootMoveAbbr(2, 0, 0, state, MoveProgram.Horizontal);
                    FootMoveAbbr(3, 1, 2, state, MoveProgram.Horizontal);
                    isGettingReady = true;
                    break;
                case 1:
                    FootMoveAbbr(0, 0, 0, state, MoveProgram.Step);
                    FootMoveAbbr(1, 1, 2, state, MoveProgram.Horizontal);
                    FootMoveAbbr(2, 0, 1, state, MoveProgram.Horizontal);
                    FootMoveAbbr(3, 1, 3, state, MoveProgram.Horizontal);
                    state = 2;
                    break;
                case 2:
                    FootMoveAbbr(0, 1, 0, state, MoveProgram.Horizontal);
                    FootMoveAbbr(1, 2, 2, state, MoveProgram.Horizontal);
                    FootMoveAbbr(2, 1, 1, state, MoveProgram.Horizontal);
                    FootMoveAbbr(3, 2, 3, state, MoveProgram.Horizontal);
                    state = 3;
                    break;
                case 3:
                    FootMoveAbbr(0, 1, 1, state, MoveProgram.Horizontal);
                    FootMoveAbbr(1, 2, 3, state, MoveProgram.Horizontal);
                    FootMoveAbbr(2, 1, 2, state, MoveProgram.Horizontal);
                    FootMoveAbbr(3, 0, 0, state, MoveProgram.Step);
                    state = 4;
                    break;
                case 4:
                    FootMoveAbbr(0, 1, 2, state, MoveProgram.Horizontal);
                    FootMoveAbbr(1, 0, 0, state, MoveProgram.Step);
                    FootMoveAbbr(2, 1, 3, state, MoveProgram.Horizontal);
                    FootMoveAbbr(3, 0, 1, state, MoveProgram.Horizontal);
                    state = 5;
                    break;
                case 5:
                    FootMoveAbbr(0, 2, 2, state, MoveProgram.Horizontal);
                    FootMoveAbbr(1, 1, 0, state, MoveProgram.Horizontal);
                    FootMoveAbbr(2, 2, 3, state, MoveProgram.Horizontal);
                    FootMoveAbbr(3, 1, 1, state, MoveProgram.Horizontal);
                    state = 6;
                    break;
                case 6:
                    FootMoveAbbr(0, 2, 3, state, MoveProgram.Horizontal);
                    FootMoveAbbr(1, 1, 1, state, MoveProgram.Horizontal);
                    FootMoveAbbr(2, 0, 0, state, MoveProgram.Step);
                    FootMoveAbbr(3, 1, 2, state, MoveProgram.Horizontal);
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

    private void FootMoveAbbr(int i, int horiBack, int stepBack, int state, MoveProgram program)
    {
        float t = moveV;
        if (state == 2 || state == 5)
        {
            t = moveV / subStepS * subStepH;
        }
        FootMove(i, (topStepPos - stepBack * subStepS - horiBack * subStepH + ((i < 2) ? frontOffset : rearOffset)) * ((i % 2 > 0) ? rightScale : 1), 
            t, program);
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
            leg.programData = new Dictionary<string, float>();
            x = x1;
        }
        else
            x = (time - t0) / (t1 - t0) * (x1 - x0) + x0;

        // Update motor state
        SetMotorByPos(i, x, height);
    }

    private bool GetBendDir(int i)
    {
        bool bendDir = true;
        if (i < 2)
            bendDir = false;
        return bendDir;
    }

    private void SetMotorByPos(int i, float x, float y)
    {
        // Set angles
        double alpha1 = 0, alpha2 = 0;
        GetAngleByDist(ref alpha1, ref alpha2, x, y, i);
        Motors[i, 0].targetAngle = (float)(alpha1);
        Motors[i, 1].targetAngle = (float)(alpha2);

        // Log positions
        footPositions[i] = new Vector2(x, y);
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
