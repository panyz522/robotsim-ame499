using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using UnityEngine;

public interface IRobotController
{
    void SetReady();
    void SetStart();
    void TogglePause();

    T GetComponent<T>();
    float GetHeight();
    float GetServoAngle(int i, int j);
    Vector2[] GetFootPositions();
}
