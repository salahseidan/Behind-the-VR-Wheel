using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class GameManager : MonoBehaviour
{
    public CarControl carControl;

    public GameObject tacho_needle;

    public GameObject speed_needle;

    public float desiredSpeed;

    public float rpmAngle;

    void Start()
    {
        speed_needle.transform.localRotation = Quaternion.Euler(0, 0, 0);
        tacho_needle.transform.localRotation = Quaternion.Euler(0, 0, 0);
    }
    // Update is called once per frame
    void Update()
    {
        float targetSpeed = carControl.getSpeed();

        // Interpolate desiredSpeed from its current value to targetSpeed over time
        desiredSpeed = Mathf.Lerp(desiredSpeed, targetSpeed, Time.deltaTime * 7f);

    updateNeedles();
    }

    public void updateNeedles()
    {

        float rpmPosition = Mathf.Clamp(carControl.getEngineRPM(), 10f, carControl.redlineRPM);

        
            // Map the engineRPM from its range (0 to 8000) to the needle's range (0 to -260)
            rpmAngle = -rpmPosition * 220f / carControl.redlineRPM;

        // Apply the new rotations
        tacho_needle.transform.localRotation = Quaternion.Euler(0, 0, rpmAngle);

        if(carControl.carModel == CarControl.CarModel.VWGolf)
        {
            speed_needle.transform.localRotation = Quaternion.Euler(0, 0, desiredSpeed);
        }

        speed_needle.transform.localRotation = Quaternion.Euler(0, 0, -desiredSpeed);
    }
}
