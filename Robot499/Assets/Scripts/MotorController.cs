using System;
using UnityEngine;

public class MotorController : MonoBehaviour {
    public float targetAngle;

    private HingeJoint hinge;

	// Use this for initialization
	void Start () {
        hinge = GetComponent<HingeJoint>();
        var motor = hinge.motor;
        motor.force = 0.1469f;
        hinge.motor = motor;
	}
	
	// Update is called once per frame
	void Update () {
    }

    void FixedUpdate()
    {
        // Get angle
        float currentAngle = hinge.angle;
        var deltaAngle = targetAngle - currentAngle;

        // Get speed (P controller)
        var speed = deltaAngle * 120;
        speed = (speed > 0) ? speed : -speed;
        speed = (speed > 600) ? 600 : speed;

        // Set motor
        var motor = hinge.motor;
        motor.targetVelocity = speed * ((deltaAngle > 0) ? 1 : -1);
        hinge.motor = motor;
    }
}
