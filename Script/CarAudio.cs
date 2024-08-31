using UnityEngine;
using System.Collections;
using System;
using TMPro;
using UnityEngine.UI;

public class CarAudio : MonoBehaviour
{
    // Reference to the CarControl script
    public CarControl carControl;

    private inputManager input_manager;

    // Reference to the AudioSource component
    private AudioSource engineStart;
    private AudioSource engineStarting;
    private bool wasEngineOff = true;
    public bool engineSoundPlayed = false;
    public bool brakeSoundPlayed = false;
    private AudioSource engineIdle;
    private AudioSource horn;
    private AudioSource gearShift;
    private  AudioSource seatBelt;
    private AudioSource seatBeltWarning;
    private AudioSource skid;
    private AudioSource stallSound;
    private AudioSource leftTurnIndicator;
    private AudioSource rightTurnIndicator;
    private AudioSource clickSound;

    public AudioClip engineStartClip;
    public AudioClip engineIdleClip;
    public AudioClip hornClip;
    public AudioClip gearShiftClip;
    public AudioClip seatBeltClip;
    public AudioClip seatBeltWarningClip;
    public AudioClip skidClip;
    public AudioClip stallSoundClip;
    public AudioClip leftTurnIndicatorClip;
    public AudioClip rightTurnIndicatorClip;
    public AudioClip clickSoundClip;
    public AudioClip engineStartingClip;

    public float currentEngineRPM;
    public float currentSpeed;
    public bool stall;
    private bool isStallSoundPlaying = false;
    private int lastGear = -1;
    private int currentGear;
    private float brakeIntensity;
    private bool isSkidSoundScheduled = false;
    public Material brakeLightMaterial;
    public Material frontLightMaterial;
    public Material gaugeMaterial;
    public Light frontLightLeft;
    public Light frontLightRight;
    public Light leftTurnLight;
    public Light rightTurnLight;
    public TextMeshProUGUI speedText;
    public TextMeshProUGUI gearText;
    public Image highBeamImage;
    public Image lowBeamImage;
    public Image leftTurnImage;
    public Image rightTurnImage;
    public Image parkingLightImage;
    public Image seatbeltImage; 

    private bool isSeatBeltWarningPlaying = false;
    private bool isleftTurningIndicatorPlaying = false;
    private bool isrightTurningIndicatorPlaying = false;
    private bool isHazardBlinking = false;

    private Coroutine leftBlinkCoroutine, rightBlinkCoroutine;

    enum LightState
    {
        Off,
        Standing,
        Daylight,
        Nightlight
    }

    [SerializeField]private LightState currentLightState = LightState.Off;

    // Start is called before the first frame update
    private void Start()
    {
        input_manager = GetComponent<inputManager>();
        // Add the audio sources to the GameObject
        engineStart = gameObject.AddComponent<AudioSource>();
        engineIdle = gameObject.AddComponent<AudioSource>();
        horn = gameObject.AddComponent<AudioSource>();
        gearShift = gameObject.AddComponent<AudioSource>();
        seatBelt = gameObject.AddComponent<AudioSource>();
        seatBeltWarning = gameObject.AddComponent<AudioSource>();
        skid = gameObject.AddComponent<AudioSource>();
        stallSound = gameObject.AddComponent<AudioSource>();
        leftTurnIndicator = gameObject.AddComponent<AudioSource>();
        rightTurnIndicator = gameObject.AddComponent<AudioSource>();
        clickSound = gameObject.AddComponent<AudioSource>();
        engineStarting = gameObject.AddComponent<AudioSource>();

        // Set the audio clips for each audio source
        engineStart.clip = engineStartClip;
        engineIdle.clip = engineIdleClip;
        horn.clip = hornClip;
        gearShift.clip = gearShiftClip;
        seatBelt.clip = seatBeltClip;
        seatBeltWarning.clip = seatBeltWarningClip;
        skid.clip = skidClip;
        stallSound.clip = stallSoundClip;
        leftTurnIndicator.clip = leftTurnIndicatorClip;
        rightTurnIndicator.clip = rightTurnIndicatorClip;
        clickSound.clip = clickSoundClip;
        engineStarting.clip = engineStartingClip;

        // Initialize the turning indicator states to false
        isleftTurningIndicatorPlaying = false;
        isrightTurningIndicatorPlaying = false;

        // Ensure turn signals are visually off at start
        leftTurnImage.gameObject.SetActive(false);
        leftTurnLight.enabled = false;
        rightTurnImage.gameObject.SetActive(false);
        rightTurnLight.enabled = false;

        // To hide the images
        highBeamImage.gameObject.SetActive(false);
        lowBeamImage.gameObject.SetActive(false);
        leftTurnImage.gameObject.SetActive(false);
        rightTurnImage.gameObject.SetActive(false);
        parkingLightImage.gameObject.SetActive(false);
        seatbeltImage.gameObject.SetActive(true);
    }

    public void Update()
{
    currentEngineRPM = carControl.engineRPM;
    currentSpeed = carControl.speed;
    stall = carControl.getStall();
    currentGear = input_manager.CurrentGear;
    if(gearText != null){gearText.text = currentGear.ToString();}
    if(speedText != null){speedText.text = ((int)currentSpeed).ToString();}

    UpdateEngineSound();

    UpdateGearSound();

    UpdateHornSound();

    UpdateStallSound();

    UpdateBrakeSound();

    updateFrontLights();

    UpdateSeatBelt();

    updateTurnLights();

    }

    private void UpdateEngineSound()
    {
        if(carControl.engine)
        {
            gaugeMaterial.EnableKeyword("_EMISSION");
        }

        if (!carControl.engine)
        {
            gaugeMaterial.DisableKeyword("_EMISSION");
        }

        // Check if the engine was off and now is on
        if (wasEngineOff && carControl.engine)
        {
            engineStarting.Play(); // Play the engine starting sound
            // Update wasEngineOff for the next frame
            wasEngineOff = false;
            engineSoundPlayed = true;
        }

        if (!carControl.engine && !wasEngineOff)
        {
            // Update wasEngineOff for the next frame
            engineStarting.Stop();
            wasEngineOff = true;
        }

        // If the engineOnButton is pressed and the car's engine is off, play the engine start sound
        if (input_manager.engineOnButton && !engineIdle.isPlaying)
        {
            if (!engineStart.isPlaying)
            {
                engineStart.Play();
                engineStart.volume = 0.7f;
            }
        }
        
        if (!input_manager.engineOnButton)
        {
            // Stop the engine start sound if the button is released or the car's engine is on
            engineStart.Stop();
        }

        // If the car's engine is on, manage the engine idle sound
        if (carControl.engine)
        {
            if (!engineIdle.isPlaying)
            {
                // Play the engine idle sound if it's not already playing
                engineIdle.Play();
                engineIdle.loop = true;
                engineIdle.volume = 0.3f;
            }

            // Adjust the pitch of the engine idle sound based on the engine RPM and speed
            float rpmPitch = Mathf.Lerp(0.8f, 1.6f, currentEngineRPM / 6000);
            float speedPitch = Mathf.Lerp(0.8f, 1.6f, currentSpeed / 260);
            engineIdle.pitch = rpmPitch + speedPitch;
        }
        else if (engineIdle.isPlaying)
        {
            // Stop the engine idle sound if the car's engine is off
            engineIdle.Stop();
        }
    }

    private void UpdateGearSound()
    {
        if (currentGear != lastGear && carControl.engine) 
        {
            if (input_manager.shiftButtons)
            {
                if (!gearShift.isPlaying)
                {
                    gearShift.Play();
                    gearShift.volume = 0.6f;
                    gearShift.playOnAwake = false;
                }
            }
            else
            {
                if (gearShift.isPlaying)
                {
                    gearShift.Stop();
                }
            }
            lastGear = currentGear; // Update lastGear to the current gear after handling sound
        }
    }

    private void UpdateHornSound()
    {
        if (input_manager.horn && engineIdle.isPlaying)
        {
            // Only play the horn sound if it's not already playing
            if (!horn.isPlaying)
            {
                horn.Play();
                horn.volume = 0.9f;
            }
        }
        if (!input_manager.horn)
        {
            // Stop the horn sound if the horn button is not pressed
            horn.Stop();
        }
    }

    private void UpdateStallSound()
    {
        if (stall)
        {
            if(!stallSound.isPlaying && !isStallSoundPlaying)
            {
                // Play the stall sound if the car has stalled
                stallSound.Play();
                stallSound.volume = 1f;
                // Set the flag to true
                isStallSoundPlaying = true;
            }
        }
        if (!stall)
        {
            // Stop the stall sound if the car has not stalled
            stallSound.Stop();
            // Set the flag back to false
            isStallSoundPlaying = false;
        }
    }

    private void UpdateSeatBelt()
    {
        if (input_manager.seatBeltButton)
        {
            seatBelt.Play();
            seatBelt.volume = 0.5f;
        }

        if (carControl.seatBelt)
        {
            if (isSeatBeltWarningPlaying)
            {
            seatbeltImage.gameObject.SetActive(false);
            seatBeltWarning.Stop();
            isSeatBeltWarningPlaying = false;
            }
        }
        if (!carControl.seatBelt && !isSeatBeltWarningPlaying && carControl.engine)
        {
            seatbeltImage.gameObject.SetActive(true);
            seatBeltWarning.volume = 0.4f; 
            seatBeltWarning.Play();
            seatBeltWarning.loop = true;
            isSeatBeltWarningPlaying = true;
        }
    }

    private void UpdateBrakeSound()
    {
        brakeIntensity = input_manager.BrakeInput; 
        float speedFactor = Mathf.Clamp01(currentSpeed / 260f); 

        // Directly activate or deactivate brake light emission based on conditions
        if (brakeIntensity > 0 && carControl.engine)
        {
            // Activate emission
            brakeLightMaterial.EnableKeyword("_EMISSION");
        }
        else
        {
            // Deactivate emission
            brakeLightMaterial.DisableKeyword("_EMISSION");
        }

        // Determine if the skid sound should be playing
        bool shouldPlaySkidSound = brakeIntensity > 0 && currentSpeed > 0.1f && !stall && carControl.engine && carControl.hasStartedDriving;

        if (shouldPlaySkidSound)
        {
            if (!skid.isPlaying && !isSkidSoundScheduled)
            {
                skid.Play(); 
                skid.loop = true;
                skid.volume = 0.3f;
                brakeSoundPlayed = true;
                isSkidSoundScheduled = true;
            }
            skid.pitch = 1.0f + (speedFactor * brakeIntensity); // Adjust pitch based on speed and brake intensity
            skid.volume = 0.5f + (0.5f * brakeIntensity); // Adjust volume based on brake intensity
        }
        else if (isSkidSoundScheduled)
        {
            // Optionally, fade out the sound here instead of stopping abruptly
            skid.Stop();
            isSkidSoundScheduled = false;
        }
    }

    private void updateFrontLights()
    {
        if (input_manager.frontLightsButton && carControl.engine)
        {
            // Move to the next light state each time the button is pressed
            currentLightState = (LightState)(((int)currentLightState + 1) % Enum.GetNames(typeof(LightState)).Length);

            // Update the lights based on the current state
            switch (currentLightState)
            {
                case LightState.Off:
                    clickSound.Play();
                    // Deactivate emission
                    parkingLightImage.gameObject.SetActive(false);
                    lowBeamImage.gameObject.SetActive(false);
                    highBeamImage.gameObject.SetActive(false);
                    frontLightMaterial.DisableKeyword("_EMISSION");
                    frontLightLeft.enabled = false;
                    frontLightRight.enabled = false;
                    // Disable any additional light effects for standing, daylight, and nightlight
                    break;
                case LightState.Standing:
                    clickSound.Play();
                    parkingLightImage.gameObject.SetActive(true);
                    lowBeamImage.gameObject.SetActive(false);
                    highBeamImage.gameObject.SetActive(false);
                    frontLightMaterial.EnableKeyword("_EMISSION");
                    frontLightLeft.enabled = true;
                    frontLightLeft.intensity = 30f;
                    frontLightRight.enabled = true;
                    frontLightRight.intensity = 100f;
                    break;
                case LightState.Daylight:
                    clickSound.Play();
                    lowBeamImage.gameObject.SetActive(true);
                    parkingLightImage.gameObject.SetActive(false);
                    highBeamImage.gameObject.SetActive(false);
                    frontLightMaterial.EnableKeyword("_EMISSION");
                    frontLightLeft.enabled = true;
                    frontLightLeft.intensity = 100f;
                    frontLightRight.enabled = true;
                    frontLightRight.intensity = 500f;
                    break;
                case LightState.Nightlight:
                    clickSound.Play();  
                    highBeamImage.gameObject.SetActive(true);
                    lowBeamImage.gameObject.SetActive(false);
                    parkingLightImage.gameObject.SetActive(false);
                    frontLightMaterial.EnableKeyword("_EMISSION");
                    frontLightLeft.enabled = true;
                    frontLightLeft.intensity = 200f;
                    frontLightRight.enabled = true;
                    frontLightRight.intensity = 1000f;
                    break;
            }
        }
        
    }

    private void updateTurnLights()
    {   
        if (!carControl.engine && isHazardBlinking)
        {
            isHazardBlinking = false;
            leftTurnImage.gameObject.SetActive(false);
            leftTurnLight.enabled = false;
            rightTurnImage.gameObject.SetActive(false);
            rightTurnLight.enabled = false;
            leftTurnIndicator.Stop();
            rightTurnIndicator.Stop();
            if (leftBlinkCoroutine != null)
            {
                StopCoroutine(leftBlinkCoroutine);
            }
            if (rightBlinkCoroutine != null)
            {
                StopCoroutine(rightBlinkCoroutine);
            }
        }
        else if (carControl.hazardLights && !isHazardBlinking && carControl.engine)
        {
            isHazardBlinking = true;
            leftTurnImage.gameObject.SetActive(true);
            leftTurnLight.enabled = true;
            rightTurnImage.gameObject.SetActive(true);
            rightTurnLight.enabled = true;
            leftTurnIndicator.volume = 0.4f;
            leftTurnIndicator.Play();
            rightTurnIndicator.volume = 0.4f;
            rightTurnIndicator.Play();
            rightBlinkCoroutine = StartCoroutine(BlinkTurnSignal(rightTurnImage.gameObject, rightTurnLight));
            leftBlinkCoroutine = StartCoroutine(BlinkTurnSignal(leftTurnImage.gameObject, leftTurnLight));
        }
        else if (!carControl.hazardLights && isHazardBlinking)
        {
            isHazardBlinking = false;
            leftTurnImage.gameObject.SetActive(false);
            leftTurnLight.enabled = false;
            rightTurnImage.gameObject.SetActive(false);
            rightTurnLight.enabled = false;
            leftTurnIndicator.Stop();
            rightTurnIndicator.Stop();
            if (leftBlinkCoroutine != null)
            {
                StopCoroutine(leftBlinkCoroutine);
            }
            if (rightBlinkCoroutine != null)
            {
                StopCoroutine(rightBlinkCoroutine);
            }
        }
        if (!carControl.leftTurn && isleftTurningIndicatorPlaying)
        {
            leftTurnIndicator.Stop();
            isleftTurningIndicatorPlaying = false;
            if (leftBlinkCoroutine != null)
            {
                StopCoroutine(leftBlinkCoroutine);
                leftTurnImage.gameObject.SetActive(false);
                leftTurnLight.enabled = false;
            }
        }
        else if (carControl.leftTurn && !isleftTurningIndicatorPlaying && carControl.engine)
        {
            leftTurnIndicator.volume = 0.4f;
            leftTurnIndicator.Play();
            isleftTurningIndicatorPlaying = true;
            leftBlinkCoroutine = StartCoroutine(BlinkTurnSignal(leftTurnImage.gameObject, leftTurnLight));
        }

        if (!carControl.rightTurn && isrightTurningIndicatorPlaying)
        {
            rightTurnIndicator.Stop();
            isrightTurningIndicatorPlaying = false;
            if (rightBlinkCoroutine != null)
            {
                StopCoroutine(rightBlinkCoroutine);
                // Ensure both are off when stopping
                rightTurnImage.gameObject.SetActive(false);
                rightTurnLight.enabled = false;
            }
        }
        else if (carControl.rightTurn && !isrightTurningIndicatorPlaying && carControl.engine)
        {
            rightTurnIndicator.volume = 0.4f;
            rightTurnIndicator.Play();
            isrightTurningIndicatorPlaying = true;
            rightBlinkCoroutine = StartCoroutine(BlinkTurnSignal(rightTurnImage.gameObject, rightTurnLight));
        }
    }

    IEnumerator BlinkTurnSignal(GameObject turnImage, Light turnLight)
    {
        bool isActive = false;
        while (true)
        {
            isActive = !isActive;
            turnImage.gameObject.SetActive(isActive);
            turnLight.enabled = isActive;
            yield return new WaitForSeconds(0.37f); 
        }
    }
}