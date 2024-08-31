using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using TMPro;

public class CarControl : MonoBehaviour
{
    [Space(5)]
    // Wheels colliders and transforms
    [Header("Wheels colliders and transforms")]
    public WheelCollider wheelFL;
    public WheelCollider wheelFR;
    public WheelCollider wheelRL;
    public WheelCollider wheelRR;
    public Transform wheelFLTrans, wheelFRTrans, wheelRLTrans, wheelRRTrans; // Transforms of all wheels
    public Transform metalFLTrans, metalFRTrans, metalRLTrans, metalRRTrans; // Transforms of all wheels' metal parts 
    
    [Space(5)]
    // Inputs
    [Header("Inputs")]
    public inputManager input_Manager;
    private float horizontal,vertical;
    public bool handBrake; // Handbrake input from the input_Manager script
    private float brake; // Brake input from the input_Manager script
    private float ClutchInput; // Clutch input from the input_Manager script
    public Rigidbody carRigidbody; // Rigidbody of the car
    public GameObject center_Of_Mass; // Center of mass of the car
    public GameObject steeringWheel; // The steering wheel object

    [Space(5)]
    // Car properties
    [Header("Car Properties")]
    [SerializeField]private float velocity; // Velocity of the car
    [SerializeField]private float targetSpeed; // Target speed of the car
    [SerializeField]private float wheelFLRPM;
    [SerializeField]private float wheelFRRPM;
    [SerializeField]private float wheelRLRPM;
    [SerializeField]private float wheelRRRPM;
    [SerializeField]private float wheelFLTorque;
    [SerializeField]private float wheelFRTorque;
    [SerializeField]private float wheelRLTorque;
    [SerializeField]private float wheelRRTorque;
    [SerializeField]public float engineTorque; // Engine torque
    public float engineRPM; // Engine RPM
    [SerializeField]private float newRPM; // New RPM of the engine
    public float speed = 0; // Speed of the car
    public TextMeshProUGUI speedText;
    public TextMeshProUGUI gearTextUI;
    public float previousSpeed; // Previous speed
    [SerializeField]private float speedDiff; // Speed difference
    public int gear; // Gear input from the input_Manager script
    public bool hasStartedDriving = false; // Has the car started driving
    public bool stall = false; // Is the car stalled
    public bool wasClutchPressed; // Was the clutch pressed
    public float previousClutchInput; // Previous clutch input
    public float clutchReleaseRate; // Clutch release rate
    public int previousGear; // Previous gear
    public const long BRAKE_FORCE_MULTIPLIER_HANDBRAKE = 100000000000000; // Brake force multiplier when the handbrake is applied
    public const long BRAKE_FORCE_MULTIPLIER_NORMAL = 1000000000000; // Normal brake force multiplier
    List<float> speedValues = new List<float>(); // Create a list to store the speed values
    List<float> clutchValues = new List<float>(); // Create a list to store the clutch input values

    [Space(5)]
    // Car Settings
    [Header("Car Settings")]
    public float maxEngineTorque = 500f; // Maximum torque of the engine
    public float idleRPM = 1; // Idle RPM of the engine
    public float redlineRPM = 7000f; // Maximum RPM of the engine
    public float brakeForce; // Brake force
    public float downForce = 200; // Downforce to keep the car on the ground
    public float oppositeForce = 70f;
    public float rollOverSpeed = 10f; // value to roll over the car at the beginning
    public float[] gearRatios; // Gear ratios
    public float[] maxSpeed; // Speed limits for each gear in KPH
    public bool engine = false;
    public bool seatBelt = false;
    public float engineOffDrag = 2f; // Drag value when the engine is off
    public bool leftTurn;
    public bool rightTurn;
    public bool hazardLights;
    public Animator Driver;
    public float animatorTurnAngle;
    int lastAnimatedGear = -2;
    private AudioSource audioSource;

    private Vector3 previousPosition;
    private Quaternion previousRotation;

    private bool laneChangeInProgress = false;
    private float laneChangeThreshold;
    private float rotationThreshold;
    private float laneChangeCooldown;
    private float steeringThreshold;
    [SerializeField]private float laneChangeTimer = 0.0f;
    public bool isChangingLaneRightWithoutIndicator = false;
    public bool isChangingLaneLeftWithoutIndicator = false; 
    public DrivingAssistanceSystemHighway drivingAssistanceSystemHighway;
    public DrivingAssistanceSystemPB drivingAssistanceSystemPB;
    
    public enum RotationAxis
    {
        XAxis,
        YAxis,
        ZAxis
    }

    public enum CarModel
    {
        VWGolf,
        BMW,
        FiatSpider
    }

    public RotationAxis steeringWheelRotationAxis = RotationAxis.YAxis; // Default to YAxis
    public CarModel carModel = CarModel.BMW; 
    private Quaternion currentRotation;
    Quaternion targetRotation = Quaternion.identity;

    // Start is called before the first frame update
    void Start()
    {
        carRigidbody.centerOfMass = center_Of_Mass.transform.localPosition;
        engineRPM = idleRPM;

        if (steeringWheel != null)
        {
            currentRotation = steeringWheel.transform.localRotation;
        }

        audioSource = gameObject.AddComponent<AudioSource>();
        audioSource.playOnAwake = false;

        previousPosition = transform.position;
        previousRotation = transform.rotation;
    }

    // Update is called once per frame
    void Update()
    {
        Inputs();

        setRPM();
        calculateSpeed();
        
        clutchControl();

        startDriving();
        
        Drive();
        Steer();

        addDownForce();
        
        DetectLaneChange();

        previousPosition = transform.position;
        previousRotation = transform.rotation;

        if (!engine)
        {
            carRigidbody.drag = engineOffDrag;
        }
        else
        {
            carRigidbody.drag = 0;
        }

        // Set the y-axis rotation of the steering wheel object to the rotation of the Logitech steering wheel
        switch (steeringWheelRotationAxis)
        {
            case RotationAxis.XAxis:
                targetRotation = Quaternion.Euler(input_Manager.xAxes * 360, currentRotation.eulerAngles.y, currentRotation.eulerAngles.z);
                break;
            case RotationAxis.YAxis:
                targetRotation = Quaternion.Euler(currentRotation.eulerAngles.x, input_Manager.xAxes * 360, currentRotation.eulerAngles.z);
                break;
            case RotationAxis.ZAxis:
                targetRotation = Quaternion.Euler(currentRotation.eulerAngles.x, currentRotation.eulerAngles.y, input_Manager.xAxes * 360);
                break;
        }

        steeringWheel.transform.localRotation = targetRotation;

        wheelFLTorque = wheelFL.motorTorque;
        wheelFRTorque = wheelFR.motorTorque;
        wheelRLTorque = wheelRL.motorTorque;
        wheelRRTorque = wheelRR.motorTorque;

        if (speedText != null)
        {
            speedText.text = "Speed : " + ((int)speed);
        }

        if (gearTextUI != null)
        {
            gearTextUI.text = "Gear : " + gear;
        }
    }

    public void AssignTextComponents(GameObject activeCanvas)
    {
        if (activeCanvas.activeInHierarchy)
        {
            speedText = FindChildWithTag(activeCanvas.transform, "SpeedText").GetComponent<TextMeshProUGUI>();
            gearTextUI = FindChildWithTag(activeCanvas.transform, "GearText").GetComponent<TextMeshProUGUI>();
        }
    }

    private Transform FindChildWithTag(Transform parent, string tag)
    {
        foreach (Transform child in parent)
        {
            if (child.CompareTag(tag))
            {
                return child;
            }
            Transform result = FindChildWithTag(child, tag);
            if (result != null)
            {
                return result;
            }
        }
        return null;
    }

    void SetLaneChangeParameters()
    {
        if (drivingAssistanceSystemPB != null)
        {
            laneChangeThreshold = 0.02f;
            rotationThreshold = 0.2f;
            laneChangeCooldown = 10f;
            steeringThreshold = 0.2f;
        }
        else if (drivingAssistanceSystemHighway != null)
        {
            laneChangeThreshold = 0.0001f;
            rotationThreshold = 0.001f;
            laneChangeCooldown = 5f;
            steeringThreshold = 0.05f;
        }
    }

    void DetectLaneChange()
    {
        SetLaneChangeParameters();

        Vector3 currentPosition = transform.position;
        Quaternion currentRotation = transform.rotation;

        Vector3 sidewayAxis = transform.right; 

        float lateralMovement = Vector3.Dot(currentPosition - previousPosition, sidewayAxis);
        float rotationChange = Quaternion.Angle(currentRotation, previousRotation);

        if (laneChangeTimer > 0)
        {
            laneChangeTimer -= Time.deltaTime;
        }
        else
        {   
            if (!laneChangeInProgress && Mathf.Abs(lateralMovement) > laneChangeThreshold && rotationChange > rotationThreshold && Mathf.Abs(horizontal) > steeringThreshold)
            {
                laneChangeInProgress = true;

                if (lateralMovement > 0)
                {
                    if (!rightTurn)
                    {
                        isChangingLaneRightWithoutIndicator = true;
                    }
                }
                else if (lateralMovement < 0)
                {
                    if (!leftTurn)
                    {
                        isChangingLaneLeftWithoutIndicator = true;
                    }
                }

                laneChangeTimer = laneChangeCooldown; 
            }
            else if (laneChangeInProgress && Mathf.Abs(lateralMovement) < laneChangeThreshold && rotationChange < rotationThreshold)
            {
                laneChangeInProgress = false;
                isChangingLaneRightWithoutIndicator = false;
                isChangingLaneLeftWithoutIndicator = false;

                if (drivingAssistanceSystemHighway != null)
                {
                    drivingAssistanceSystemHighway.hasPlayedRightLaneChangeAudio = false; 
                    drivingAssistanceSystemHighway.hasPlayedLeftLaneChangeAudio = false;
                }
                if (drivingAssistanceSystemPB != null)
                {
                    drivingAssistanceSystemPB.hasPlayedRightLaneChangeAudio = false;
                    drivingAssistanceSystemPB.hasPlayedLeftLaneChangeAudio = false;
                }
            }
        }
    }

    // Update is called once per frame
    void FixedUpdate()
    {
        UpdateWheelPoses(wheelFL, wheelFLTrans,metalFLTrans);
        UpdateWheelPoses(wheelFR, wheelFRTrans,metalFRTrans);
        UpdateWheelPoses(wheelRL, wheelRLTrans,metalRLTrans);
        UpdateWheelPoses(wheelRR, wheelRRTrans,metalRRTrans);

        // Check for gear change
        if (gear != previousGear)
        {
            // Play the shift animation only if it hasn't been played for the current gear
            if (gear != lastAnimatedGear)
            {
                if (carModel == CarModel.BMW){Driver.Play("Shifter");}
                if (carModel == CarModel.VWGolf){Driver.Play("ShifterGolf");}
                if (carModel == CarModel.FiatSpider){Driver.Play("ShifterSpider");}
                
                // Update lastAnimatedGear after playing the animation
                lastAnimatedGear = gear;
            }
        }

        // Continue updating steering animation and pedal animations as before
        animatorTurnAngle = Mathf.Lerp(animatorTurnAngle, input_Manager.xAxes, Time.deltaTime * 30f);
        Driver.SetFloat("turnAngle", animatorTurnAngle);

        Driver.SetFloat("gas", vertical);
        Driver.SetFloat("brake", brake);
        Driver.SetFloat("clutch", ClutchInput);
    }
    // Get the inputs from the input manager
    void Inputs()
    {
        gear = input_Manager.CurrentGear; // Use the CurrentGear property of the input_Manager script
        previousGear = input_Manager.previousGear; // Use the PreviousGear property of the input_Manager script
        horizontal = input_Manager.xAxes; // Use the xAxes property of the input_Manager script
        vertical = input_Manager.GasInput; // Use the GasInput property of the input_Manager script
        brake = input_Manager.BrakeInput; // Use the BreakInput property of the input_Manager script
        ClutchInput = input_Manager.ClutchInput; // Use the ClutchInput property of the input_Manager script
        engine = inputManager.engineOn; // Use the carEngine property of the input_Manager script
        seatBelt = inputManager.seatBelt; // Use the seatBelt property of the input_Manager script
        leftTurn = inputManager.LeftIndicator; // Use the leftTurnIndicator property of the input_Manager script
        rightTurn = inputManager.RightIndicator; // Use the rightTurnIndicator property of the input_Manager script
        hazardLights = inputManager.hazardLights; // Use the hazardLights property of the input_Manager script
        handBrake = input_Manager.circle; // Use the handBrake property of the input_Manager script
    }
    
    // Drive the car
    void Drive()
    {   
         // If the engine is not on, return immediately
        if (!engine)
        {
            SetWheelTorques(0,0);
            return;
        }

        // If the clutch was pressed, set the motor torque to 0
        if(wasClutchPressed)
        {
            if (speed < 2f)
            {
            SetWheelTorques(0,0);
            return;
            }
        }

        // If the car is in neutral, set the engine RPM based on the throttle input
        if (gear == 0 && !hasStartedDriving)
        {
            engineRPM = vertical * redlineRPM;
            SetWheelTorques(0, 0);
            return;
        }

        // Check if the car is stalled
        if (stall)
        {
            SetWheelTorques(0, 0);
            return;
        }

        // Handbrake function of the car 
        if (handBrake)
        {
            SetRearWheelBrakeTorques(BRAKE_FORCE_MULTIPLIER_HANDBRAKE);
            return;
        }

        // Move the car forward when the clutch is released
        if (!wasClutchPressed && !stall && (gear == 1 || gear == -1) && !hasStartedDriving)
        {
            SetWheelTorques(rollOverSpeed, brakeForce * brake);

        }
    }

    // Steer the car
    void Steer()
    {
        float radius = 6f; // Radius of the car wheels

        if (horizontal > 0)
        {
            wheelFR.steerAngle = Mathf.Rad2Deg * Mathf.Atan(2.55f / (radius + (1.5f / 2))) * horizontal;
            wheelFL.steerAngle = Mathf.Rad2Deg * Mathf.Atan(2.55f / (radius - (1.5f / 2))) * horizontal;
        }
        else if (horizontal < 0)
        {
            wheelFR.steerAngle = Mathf.Rad2Deg * Mathf.Atan(2.55f / (radius - (1.5f / 2))) * horizontal;
            wheelFL.steerAngle = Mathf.Rad2Deg * Mathf.Atan(2.55f / (radius + (1.5f / 2))) * horizontal;
        }
        else
        {
            wheelFR.steerAngle = 0;
            wheelFL.steerAngle = 0;
        }

    }

    void setRPM()
    {
        if (!engine)
        {
            return;
        }

        // If the car is stalled, stop the engine
        if (stall)
        {
            engine = false;
            return;
        }
        else if (!wasClutchPressed && vertical > 0)
        {
            // Calculate new engine RPM based on throttle input
            newRPM = Mathf.Lerp(idleRPM, redlineRPM, vertical) + UnityEngine.Random.Range(-50 , 50);
        }
        else if (wasClutchPressed)
        {
            if (vertical > 0)
            {
                // Calculate new engine RPM when gas and clutch are pressed
                newRPM = 6000f + UnityEngine.Random.Range(-200, 200);
                // engineRPM = 7500f;
            }
            else
            {
                // If the clutch is pressed and no throttle input, set RPM to idle
                newRPM = idleRPM;
            }
        }

        // Clamp newRPM to redlineRPM
        newRPM = Mathf.Min(newRPM, redlineRPM);

        // Gradually interpolate the engine RPM from its current value to its new value
        engineRPM = Mathf.Lerp(engineRPM, newRPM, Time.deltaTime * (gear == 0 ? 1 : gearRatios[Mathf.Abs(gear) - 1]) * 0.7f);    }

    // Calculate the speed of the car
    void calculateSpeed()
    {
        // Get the car's velocity in meters per second
        velocity = carRigidbody.velocity.magnitude;
        
        // Convert the velocity to kilometers per hour
        targetSpeed = velocity * 3.6f;
        float accelerationTime = 2f; 

        if (gear != 0 && gear <= gearRatios.Length)
        {
            if (wasClutchPressed && !hasStartedDriving)
            {
                stall = false;
                // Interpolate between current speed and target speed
                speed = Mathf.Lerp(speed, targetSpeed, Time.deltaTime * accelerationTime);
                SetWheelTorques(4, 0);
            }
            
            // If speed is less than a small threshold, set it to zero
            if (targetSpeed < 0.001f) 
            {
                    targetSpeed = 0f;
            }

            // Interpolate between current speed and target speed
            
            speed = Mathf.Lerp(speed, targetSpeed, Time.deltaTime / accelerationTime);

            // If the car's velocity is effectively zero, set speed to zero
            if (carRigidbody.velocity.magnitude < 0.001f) 
            {
                speed = 0f;
                hasStartedDriving = false; // Set hasStartedDriving to false when the car is stopped
            }

            // Add the current speed to the list
            speedValues.Add(speed);

            // If the list has more than 100 elements, remove the oldest one
            if (speedValues.Count > 800)
            {
                speedValues.RemoveAt(0);
            }

            // Get the 20th last speed value
            if (speedValues.Count >= 500)
            {
                previousSpeed = speedValues[speedValues.Count - 500]; // Get the previous speed value
            }
            
            speedDiff = Mathf.Abs(speed - previousSpeed); // Calculate the speed difference
            
            // Check if the car's speed is within the limit of the current gear
            if (Mathf.RoundToInt(speed) <= (gear == -1 ? maxSpeed[0] : maxSpeed[gear - 1]))
            {
                // Calculate the engine torque
                if (vertical > 0)
                {
                    engineTorque = (engineRPM / redlineRPM) * maxEngineTorque * vertical; // Calculate the engine torque
                }
                else if (!wasClutchPressed && (gear == 1 || gear == -1)) // Check if the clutch is released and the gear is not neutral
                {
                    engineTorque = rollOverSpeed; // Apply a small amount of torque for idle creep
                }
                
                float acc_torque = engineTorque * (gear == -1 ? gearRatios[0] : gearRatios[gear - 1]); // Torque to accelerate the car

                // Apply torque to the car's rigidbody
                SetWheelTorques(acc_torque, brakeForce * BRAKE_FORCE_MULTIPLIER_NORMAL * brake);
            }

            int speedIndex = gear == -1 ? 1 : gear;
            float maxSpeedForGear = maxSpeed[speedIndex - 1];
            float speedDifference = speed - maxSpeedForGear;

            if (speedDifference > 0)
            {
            // If speed is greater than the max speed for the current gear, gradually reduce it
            speed = Mathf.Lerp(speed, maxSpeedForGear, Time.deltaTime * 0.5f);

            // Calculate the braking force
            Vector3 brakeForce = -carRigidbody.velocity.normalized / oppositeForce;

            // Apply the braking force
            carRigidbody.AddForce(brakeForce, ForceMode.Acceleration);
            }
        }
    }       

    // Control the clutch
    void clutchControl()
    {
        // Check if the car is in automatic mode at the beginning
        if (!input_Manager.Hshift) // Assuming Hshift == false means automatic mode
        {
            stall = false; // Prevent stalling in automatic mode
            return; // Skip the rest of the clutch control logic
        }

        // Add the current clutch input to the list
        clutchValues.Add(ClutchInput);

        // If the list has more than 800 elements, remove the oldest one
        if (clutchValues.Count > 100)
        {
            clutchValues.RemoveAt(0);
        }

        // Get the 500th last clutch input value
        if (clutchValues.Count >= 20)
        {
            previousClutchInput = clutchValues[clutchValues.Count - 20]; // Get the previous clutch input value
        }

        clutchReleaseRate = Math.Abs(previousClutchInput - ClutchInput) * 10f;

        if (ClutchInput > 0.9)
        {
            wasClutchPressed = true;
        }
        else
        {
            wasClutchPressed = false;
        }
        
        // Check if the car is stopped and the clutch is not pressed
        if (speedDiff > 10f && speed < 20f && ClutchInput < 0.5f && brake != 0)
        {
            // Set the stall variable to true
            stall = true;
        }

        if (!wasClutchPressed && !hasStartedDriving)
        {
            // The clutch was previously pressed and is now being released
            if (clutchReleaseRate < 2.3f && (previousClutchInput - ClutchInput) < 0)
            {
                stall = false;
            }       
            
            else if (clutchReleaseRate >= 2.3f && (previousClutchInput - ClutchInput) > 0)
            {
                stall = true;
            }
        }

        // Prevent stalling while driving and changing gear
        if (hasStartedDriving)
        {
            stall = false;
        }

        previousClutchInput = ClutchInput;

        // If the car stalls, turn the engine off
        if (stall)
        {
            inputManager.engineOn = false;
        }
    }

    // Determine if the car has started driving
    void startDriving()
    {
        if (!engine)
        {
            return;
        }

        // Determine if the car has started driving
        if (!hasStartedDriving && (gear == 1 || gear == -1) && !stall && previousClutchInput >= ClutchInput && speed > 0.5f && !wasClutchPressed)
        {
            hasStartedDriving = true;
            carRigidbody.AddForce(new Vector3(0, 0, rollOverSpeed));
        }
        // Determine if the car has come to a complete stop
        else if (hasStartedDriving && speed < 0.5f)
        {
            hasStartedDriving = false;
            carRigidbody.velocity = new Vector3(0, 0, 0f);
        }

        // // If the car has not started driving and the gear change is more than one step, set the gear to 0
        // if (!hasStartedDriving && Math.Abs(previousGear - gear) > 1)
        // {
        //     gear = 0;
        // }
    }

    // Set the motor and brake torques for all wheels
    public void SetWheelTorques(float motorTorque, float brakeTorque)
    {   
        // If the car is in reverse, set the engine torque to a negative value
        if (gear == -1)
        {
            motorTorque = -motorTorque;
        }

        // If the brake is being applied, set the motor torque to 0
        if (brake > 0)
        {
            motorTorque = 0;
            brakeTorque = BRAKE_FORCE_MULTIPLIER_NORMAL * brake; // Set brakeTorque based on brake input
        }

        // If the car is at a full stop and the gear is not the first one, prevent the car from moving
        if (speed == 0 && Math.Abs(gear) != 1)
        {
            motorTorque = 0;
        }

        wheelRL.motorTorque = motorTorque;
        wheelRR.motorTorque = motorTorque;
        wheelFL.motorTorque = motorTorque;
        wheelFR.motorTorque = motorTorque;

        wheelRL.brakeTorque = brakeTorque;
        wheelRR.brakeTorque = brakeTorque;
        wheelFL.brakeTorque = brakeTorque;
        wheelFR.brakeTorque = brakeTorque;
    }

    void SetRearWheelBrakeTorques(float brakeTorque)
    {
        wheelRL.brakeTorque = brakeTorque;
        wheelRR.brakeTorque = brakeTorque;
    }

    // Update the wheel poses
    void UpdateWheelPoses(WheelCollider wheel, Transform wheelTransform, Transform metalTransform)
    {
        Vector3 pos = wheelTransform.position;
        Quaternion rot = wheelTransform.rotation;

        wheel.GetWorldPose(out pos, out rot); 

        if (carModel == CarModel.BMW || carModel == CarModel.FiatSpider)
        {
        wheelTransform.position = pos;
        wheelTransform.rotation = rot;
        metalTransform.position = pos;
        metalTransform.rotation = rot;
        }

        if (carModel == CarModel.VWGolf)
        {
        // Apply the world pose to the wheel transform
        wheelTransform.position = pos;
        wheelTransform.rotation = rot * Quaternion.Euler(0, 0, -90); // Apply corrective rotation here
        metalTransform.position = pos;
        metalTransform.rotation = rot * Quaternion.Euler(0, 0, -90); // Apply the same corrective rotation to the metal part
        }

        wheelFLRPM = wheelFL.rpm;
        wheelFRRPM = wheelFR.rpm;
        wheelRLRPM = wheelRL.rpm;
        wheelRRRPM = wheelRR.rpm;
    }

    // Add downforce to the car to keep it on the ground
    public void addDownForce()
    {
        carRigidbody.AddForce(-transform.up * downForce * carRigidbody.velocity.magnitude);
    }

    // get car speed 
    public float getSpeed()
    {
        return speed;
    }

    // get car engine RPM
    public float getEngineRPM()
    {
        return engineRPM;
    }

    // get car stall status
    public bool getStall()
    {
        return stall;
    }
    
}
