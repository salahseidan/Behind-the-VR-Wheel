using UnityEngine;
using System;
using UnityEngine.Rendering.PostProcessing;

public class InputManagerNormal : MonoBehaviour
{
    LogitechGSDK.LogiControllerPropertiesData properties;

    public float xAxes, GasInput, BrakeInput, ClutchInput;

    public static bool seatBelt = false;

    public bool seatBeltButton = false;

    public bool leftTurnIndicator = false;

    public bool rightTurnIndicator = false;

    public static bool LeftIndicator = false;

    public static bool RightIndicator = false;

    public bool X = false;

    public bool horn;

    public bool minus = false;

    public bool circle;

    public bool up = false;

    public static bool hazardLights = false;

    public bool down = false;

    public bool left = false;

    public bool right = false;

    public bool engineOnButton; 

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

    bool wasXPressed = false;

    bool wasUpPressed = false;

    bool wasDownPressed = false;

    bool wasLeftPressed = false;

    bool wasRightPressed = false;

    uint dPadState;

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
                if (!Hshift || ClutchInput > 0.9)
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
            if (!seatBelt)
            {
                seatBelt = true;
            }
            else
            {
                seatBelt = false;
            }
        }

        if (up)
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

        LogitechGSDK.LogiPlaySpringForce(0, 0, 30, 50);
        LogitechGSDK.LogiPlayDamperForce(0, 10);
        LogitechGSDK.LogiHasForceFeedback(0);  

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
        

        // Method to get the button pressed in the console window
            // LogitechGSDK.DIJOYSTATE2ENGINES steer;
            // steer = LogitechGSDK.LogiGetStateUnity(0);
            // for(int i = 0; i < 128; i++){
            //     if (steer.rgbButtons[i] == 128)
            //     {
            //         Debug.Log("Button " + i + " is pressed");
            //     }    
            // }

            // if (steer.rgdwPOV[0] != -1f)
            // {
            //     Debug.Log("D-Pad state: " + steer.rgdwPOV[0]);
            // }
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
    }

    public bool AnyButtonPressed()
    {
        for (int i = 0; i < 128; i++)
        {
            if (LogitechGSDK.LogiButtonIsPressed(0, i))
            {
                return true;
            }
        }
        return false;
    }
    
    void HandleContinuousInputs()
    {
        LogitechGSDK.DIJOYSTATE2ENGINES rec = LogitechGSDK.LogiGetStateUnity(0);
        xAxes = rec.lX / 32768f; // Normalize to 0-1 range
        horn = LogitechGSDK.LogiButtonIsPressed(0,23);
        circle = LogitechGSDK.LogiButtonIsPressed(0,2);
        engineOnButton = LogitechGSDK.LogiButtonIsPressed(0, 19);
    }
}
