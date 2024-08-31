using UnityEngine;
using UnityEngine.UI;
using UnityEngine.SceneManagement;

public class inputManager : MonoBehaviour
{
    LogitechGSDK.LogiControllerPropertiesData properties;

    public CarControl carControl;

    public float xAxes, GasInput, BrakeInput, ClutchInput;

    public static bool seatBelt = false;

    public bool seatBeltButton = false;

    public bool X = false;

    public bool reset = false;

    public bool leftTurnIndicator = false;

    public bool rightTurnIndicator = false;

    public static bool LeftIndicator = false;

    public static bool RightIndicator = false;

    public bool horn;

    public bool minus = false;

    public bool hazard = false;

    public bool cameraChange = false;

    public bool circle;

    public bool up = false;

    public static bool hazardLights = false;

    public bool down = false;

    public bool left = false;

    public bool right = false;

    public bool engineOnButton; 

    public bool menuButton;

    public static bool engineOn = false;
    
    public bool Hshift = true;

    public bool shiftButtons = false;

    public bool frontLightsButton = false;
    
    public int CurrentGear;

    public int previousGear;

    public float shiftUpSpeed; // Speed value to shift up in automatic mode
    
    public float shiftDownSpeed; // Speed value to shift down in automatic mode

    bool wasFrontLightPressed = false;
    
    bool wasMinusPressed = false;

    bool wasseatBeltPressed = false;

    bool wasleftIndicatorPressed = false;

    bool wasrightIndicatorPressed = false;

    bool wasHazardsPressed = false;

    bool wasXPressed = false;

    bool wasUpPressed = false;

    bool wasDownPressed = false;

    bool wasLeftPressed = false;

    bool wasRightPressed = false;

    bool wasResetPressed = false;

    bool wasEngineonPressed = false;

    bool wascameraPressed = false;

    bool wasMenuPressed = false;

    uint dPadState;

    private bool showConfirmationWindow = false;

    public GameObject confirmationPanel;

    private void Start()
    {
        print(LogitechGSDK.LogiSteeringInitialize(true));

    }

    private void Update()
    {
        if (LogitechGSDK.LogiUpdate() && LogitechGSDK.LogiIsConnected(0))
        {
        
        LogitechGSDK.DIJOYSTATE2ENGINES rec;
        
        rec = LogitechGSDK.LogiGetStateUnity(0);

        dPadState = rec.rgdwPOV[0];

        UpdateButtonStates();

        HandleContinuousInputs();

        // If the engine Start button is pressed, toggle the engine state
        if (engineOnButton)
        {
            if (!engineOn)
            {
                // Allow engine to start without clutch in automatic mode or with clutch in manual mode
                if (!Hshift || (ClutchInput > 0.9 && (CurrentGear == 1 || CurrentGear == -1)))
                {
                    engineOn = true;
                }
            }
            else
            {
                engineOn = false;
            }
        }

        if (seatBeltButton)
        {
            if (!seatBelt && engineOn)
            {
                seatBelt = true;
            }
            else if (seatBelt && engineOn)
            {
                seatBelt = false;
            }
        }

        if (hazard)
        {
            if (!hazardLights)
            {
                hazardLights = true;
            }
            else
            {
                hazardLights = false;
            }
        }

        if (leftTurnIndicator)
        {
            if (!LeftIndicator)
            {
                LeftIndicator = true;
            }
            else
            {
                LeftIndicator = false;
            }
        }

        if (rightTurnIndicator)
        {
            if (!RightIndicator)
            {
                RightIndicator = true;
            }
            else
            {
                RightIndicator = false;
            }
        }

        if (menuButton)
        {
            showConfirmationWindow = true;
            confirmationPanel.SetActive(true);
        }

        if (showConfirmationWindow)
        {
            if (X) 
            {
                SceneManager.LoadScene("MainMenu", LoadSceneMode.Single);
                showConfirmationWindow = false; 
                confirmationPanel.SetActive(false);
            }
            else if (circle) 
            {
                showConfirmationWindow = false;
                confirmationPanel.SetActive(false); 
            }
        }

        LogitechGSDK.LogiPlaySpringForce(0, 0, 30, 50);
        LogitechGSDK.LogiPlayDamperForce(0, 10);
        LogitechGSDK.LogiHasForceFeedback(0);

            if (!Hshift)
            {
                AutomaticTransmission(rec);
            }
            else
            {
                Hshifter(rec);
            }      

            if(minus)
            {
                if (Hshift)
                {
                    Hshift = false;
                }
                else
                {
                    Hshift = true;
                }
            }
            
            if(rec.lY < 0)
            {
                GasInput = rec.lY / -32768f; // Normalize to 0-1 range
            }

            if(rec.lY >= 0)
            {
                GasInput = 0;
            }

            if(rec.lRz > 0)
            {
                BrakeInput = 0;
            }
            else if(rec.lRz < 0)
            {
                BrakeInput = rec.lRz / -32768f; // Normalize to 0-1 range
            }

            if (rec.rglSlider[0] > 0)
            {
                ClutchInput = 0;
            }
            else if (rec.rglSlider[0] < 0)
            {
                ClutchInput = rec.rglSlider[0] / -32768f; // Normalize to 0-1 range
            }

            // Reset states
            up = false;
            down = false;
            left = false;
            right = false;

            switch (dPadState)
            {
                case 0:
                    if (!wasUpPressed) { 
                        up = true; 
                    } else {
                        up = false;
                    }
                    wasUpPressed = up;
                    break;
                case 9000:
                    if (!wasRightPressed) { 
                        right = true; 
                    } else {
                        right = false; 
                    }
                    wasRightPressed = right;
                    break;
                case 18000:
                    if (!wasDownPressed) { 
                        down = true; 
                    } else {
                        down = false; 
                    }
                    wasDownPressed = down;
                    break;
                case 27000:
                    if (!wasLeftPressed) { 
                        left = true; 
                    } else {
                        left = false; 
                    }
                    wasLeftPressed = left;
                    break;
                default:
                    // No D-Pad button is pressed
                    up = false;
                    down = false;
                    left = false;
                    right = false;
                    wasLeftPressed = false;
                    wasRightPressed = false;
                    wasUpPressed = false;
                    wasDownPressed = false;
                    break;
            }
        }
        else
        {
            print("Steering Wheel is not connected!");
        }
        

        // // Method to get the button pressed in the console window
        //     LogitechGSDK.DIJOYSTATE2ENGINES steer;
        //     steer = LogitechGSDK.LogiGetStateUnity(0);
        //     for(int i = 0; i < 128; i++){
        //         if (steer.rgbButtons[i] == 128)
        //         {
        //             Debug.Log("Button " + i + " is pressed");
        //         }    
        //     }

    //         // if (steer.rgdwPOV[0] != -1f)
    //         // {
    //         //     Debug.Log("D-Pad state: " + steer.rgdwPOV[0]);
    //         // }
    }

    void UpdateButtonStates()
    {
        bool currentMinus = LogitechGSDK.LogiButtonIsPressed(0, 20);
        if (currentMinus && !wasMinusPressed) {
            minus = true; 
        } else {
            minus = false; 
        }
        wasMinusPressed = currentMinus;

        bool currentFrontLight = LogitechGSDK.LogiButtonIsPressed(0, 1);
        if (currentFrontLight && !wasFrontLightPressed) { 
            frontLightsButton = true; 
        } else {
            frontLightsButton = false; 
        }
        wasFrontLightPressed = currentFrontLight;

        bool currentSeatBelt = LogitechGSDK.LogiButtonIsPressed(0, 3);
        if (currentSeatBelt && !wasseatBeltPressed) { 
            seatBeltButton = true; 
        } else {
            seatBeltButton = false; 
        }
        wasseatBeltPressed = currentSeatBelt; 

        bool currentleftTurn = LogitechGSDK.LogiButtonIsPressed(0, 5);
        if (currentleftTurn && !wasleftIndicatorPressed) { 
            leftTurnIndicator = true; 
        } else {
            leftTurnIndicator = false; 
        }
        wasleftIndicatorPressed = currentleftTurn;

        bool currentrightTurn = LogitechGSDK.LogiButtonIsPressed(0, 4);
        if (currentrightTurn && !wasrightIndicatorPressed) { 
            rightTurnIndicator = true; 
        } else {
            rightTurnIndicator = false; 
        }
        wasrightIndicatorPressed = currentrightTurn;

        bool currentX = LogitechGSDK.LogiButtonIsPressed(0, 0);
        if (currentX && !wasXPressed) { 
            X = true; 
        } else {
            X = false; 
        }
        wasXPressed = currentX;

        bool currentcamera = LogitechGSDK.LogiButtonIsPressed(0, 8);
        if (currentcamera && !wascameraPressed) { 
            cameraChange = true; 
        } else {
            cameraChange = false; 
        }
        wascameraPressed = currentcamera;

        bool currentReset = LogitechGSDK.LogiButtonIsPressed(0, 9);
        if (currentReset && !wasResetPressed) { 
            reset = true; 
        } else {
            reset = false; 
        }
        wasResetPressed = currentReset;

        bool currenEngineon = LogitechGSDK.LogiButtonIsPressed(0, 19);
        if (currenEngineon && !wasEngineonPressed) { 
            engineOnButton = true; 
        } else {
            engineOnButton = false; 
        }
        wasEngineonPressed = currenEngineon;

        bool currentMenu = LogitechGSDK.LogiButtonIsPressed(0, 24);
        if (currentMenu && !wasMenuPressed) { 
            menuButton = true; 
        } else {
            menuButton = false; 
        }
        wasMenuPressed = currentMenu;

        bool currentHazards = LogitechGSDK.LogiButtonIsPressed(0, 7);
        if (currentHazards && !hazard) { 
            hazard = true; 
        } else {
            hazard = false; 
        }
        wasHazardsPressed = currentHazards;
    }

    void HandleContinuousInputs()
    {
        LogitechGSDK.DIJOYSTATE2ENGINES rec = LogitechGSDK.LogiGetStateUnity(0);
        xAxes = rec.lX / 32768f; // Normalize to 0-1 range
        horn = LogitechGSDK.LogiButtonIsPressed(0,23);
        circle = LogitechGSDK.LogiButtonIsPressed(0,2);
    }

    bool Hshifter(LogitechGSDK.DIJOYSTATE2ENGINES shifter)
    {
        if (LogitechGSDK.LogiUpdate() && LogitechGSDK.LogiIsConnected(0) && Hshift == true)
        {

        for(int i = 0; i < 128; i++)
        {
            if (shifter.rgbButtons[i] == 128)
            {
                shiftButtons = true;

                if (ClutchInput > 0.5)
                {
                    if (i >= 12 && i <= 18)
                    {
                        int newGear = 0;
                        switch (i)
                        {
                            case 12:
                                newGear = 1;
                                break;
                            case 13:
                                newGear = 2;
                                break;
                            case 14:
                                newGear = 3;
                                break;
                            case 15:
                                newGear = 4;
                                break;
                            case 16:
                                newGear = 5;
                                break;
                            case 17:
                                newGear = 6;
                                break;
                            case 18:
                                newGear = -1; 
                                break;
                            }
                            if (CurrentGear != newGear)
                        {
                            previousGear = CurrentGear;
                            CurrentGear = newGear;
                        }
                    }
                        
                    }   
                }
            }

            if (!shiftButtons && carControl.getSpeed() == 0)
            {
                CurrentGear = 0;
            }
        }
        return shiftButtons;
    }

    void AutomaticTransmission(LogitechGSDK.DIJOYSTATE2ENGINES shifter)
    {
        float speed = carControl.getSpeed();
        bool brakePressed = BrakeInput > 0;

        if (brakePressed)
        {
            carControl.SetWheelTorques(0, 10000000 * BrakeInput);
        }

        if (speed == 0)
        {
            if (shifter.rgbButtons[18] == 128) // Reverse gear
            {
                CurrentGear = -1;
            }

            if (shifter.rgbButtons[12] == 128)
            {
                CurrentGear = 1;
            }
        }
        else
        {
            // Shift up
            if (CurrentGear < carControl.maxSpeed.Length && speed > carControl.maxSpeed[CurrentGear - 1])
            {
                CurrentGear++;
            }
            // Shift down
            else if (CurrentGear > 1 && speed < carControl.maxSpeed[CurrentGear - 2])
            {
                CurrentGear--;
            }
        }
        float motorTorque = carControl.engineTorque;
        carControl.SetWheelTorques(motorTorque, 0);
    }
}
