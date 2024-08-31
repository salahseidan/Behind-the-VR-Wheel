using UnityEngine;
using UnityEngine.SceneManagement;
using System.Collections;
using System;
using TMPro;

public class DrivingAssistanceSystemPB : MonoBehaviour
{
    public CarControl carControl;
    public inputManager inputManager;
    public CarAudio carAudio;
    private AudioSource audioSource;
    public AudioClip PBClip, slowDownClip, speedUpClip, carColisionClip, pedColisionClip, redLightClip, laneChangeClip, stallClip;
    public bool hasPlayedRightLaneChangeAudio = false;
    public bool hasPlayedLeftLaneChangeAudio = false;
    private ScoreManager scoreManager;
    private DateTime lastSpeedingWarningTime = DateTime.MinValue;
    private DateTime lastRedLightWarningTime = DateTime.MinValue;
    private DateTime lastcollisionCarWarningTime = DateTime.MinValue;
    private DateTime lastcollisionPedWarningTime = DateTime.MinValue;
    [SerializeField]private bool isCollisionCarWarningActive = false;
    [SerializeField]private bool isSpeedingWarningActive = false;
    [SerializeField]private bool isRedLightWarningActive = false;
    [SerializeField]private bool isCollisionPedWarningActive = false;
    [SerializeField]private bool isStallActive = false;
    [SerializeField]private int speedLimit = 0;
    public TextMeshProUGUI speedLimitText;

    [SerializeField] private TrafficLightController currentTrafficLight;
    [SerializeField] private TrafficLightController2 currentTrafficLight2; 
    [SerializeField] private TrafficLightController3 currentTrafficLight3;

    Quaternion lastRotation;
    Vector3 lastPosition; 


    void Start()
    {
        audioSource = GetComponent<AudioSource>();
        scoreManager = FindObjectOfType<ScoreManager>();
        audioSource.playOnAwake = false;
        StartCoroutine(PlayPBClipAndWait());
        lastRotation = transform.rotation;
        lastPosition = transform.position;
    }

    void Update()
    {   
        // if (inputManager.reset) ResetScene();
        CheckSpeedResetCondition();
        ResetCollisionWarnings();

        if (speedLimitText != null)
        {
            speedLimitText.text = "Speed Limit: " + speedLimit;
        }

        if (!carControl.stall && isStallActive)
        {
            isStallActive = false;
        }

        if (carControl.stall && !isStallActive)
        {
            scoreManager.DecreaseScore("stalling");
            PlayFeedback(stallClip);
            isStallActive = true;
        }

        if (carControl.isChangingLaneRightWithoutIndicator && !hasPlayedRightLaneChangeAudio)
        {
            audioSource.PlayOneShot(laneChangeClip);
            scoreManager.DecreaseScore("noBlinker");
            hasPlayedRightLaneChangeAudio = true;
        }

        if (carControl.isChangingLaneLeftWithoutIndicator && !hasPlayedLeftLaneChangeAudio)
        {
            audioSource.PlayOneShot(laneChangeClip);
            scoreManager.DecreaseScore("noBlinker");
            hasPlayedLeftLaneChangeAudio = true;
        }
    }

    public void AssignTextComponents(GameObject activeCanvas)
    {
        if (activeCanvas.activeInHierarchy)
        {
            speedLimitText = FindChildWithTag(activeCanvas.transform, "SpeedLimitText").GetComponent<TextMeshProUGUI>();
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

    void OnTriggerEnter(Collider other)
    {
        HandleSpeedingWarnings(other);
        HandleCollisions(other);
        AssignTrafficLight(other);
    }

    void OnTriggerExit(Collider other)
    {
        HandleTrafficLightExit(other);
    }

    private void HandleSpeedingWarnings(Collider other)
    {
        if (other.gameObject.CompareTag("30") || other.gameObject.CompareTag("50") || other.gameObject.CompareTag("100"))
        {
            speedLimit = int.Parse(other.tag);
            double secondsSinceLastWarning = (DateTime.Now - lastSpeedingWarningTime).TotalSeconds;

            if (!isSpeedingWarningActive && carControl.speed > speedLimit && secondsSinceLastWarning > 20)
            {
                scoreManager.DecreaseScore("speeding");
                PlayFeedback(slowDownClip);
                lastSpeedingWarningTime = DateTime.Now;
                isSpeedingWarningActive = true;
            }
            else if (!isSpeedingWarningActive && carControl.speed > speedLimit / 2 - 5 && carControl.speed < speedLimit / 2 + 10 && secondsSinceLastWarning > 20)
            {
                scoreManager.DecreaseScore("speeding");
                PlayFeedback(speedUpClip);
                lastSpeedingWarningTime = DateTime.Now;
                isSpeedingWarningActive = true;
            }
        }
    }

    private void CheckSpeedResetCondition()
    { 
        double secondsSinceLastWarning = (DateTime.Now - lastSpeedingWarningTime).TotalSeconds;
        if (secondsSinceLastWarning > 5 && isSpeedingWarningActive) 
        {
            isSpeedingWarningActive = false;
        }
    }

    private void HandleCollisions(Collider other)
    {
        if (other.gameObject.CompareTag("Car"))
        {
            double secondsSinceLastWarning = (DateTime.Now - lastcollisionCarWarningTime).TotalSeconds;
            if (!isCollisionCarWarningActive && secondsSinceLastWarning > 10)
            {
                scoreManager.DecreaseScore("collision with cars");
                PlayFeedback(carColisionClip);
                lastcollisionCarWarningTime = DateTime.Now;
                isCollisionCarWarningActive = true;
            }
        }
        else if (other.gameObject.CompareTag("Pedestrian"))
        {
            double secondsSinceLastWarning = (DateTime.Now - lastcollisionPedWarningTime).TotalSeconds;
            if (!isCollisionPedWarningActive && secondsSinceLastWarning > 5)
            {
                scoreManager.DecreaseScore("collision with pedestrians");
                PlayFeedback(pedColisionClip);
                lastcollisionPedWarningTime = DateTime.Now;
                isCollisionPedWarningActive = true;
            }
        }
    }

    private void HandleTrafficLightExit(Collider other)
    {
        if (other.gameObject.CompareTag("Car Stop") && !isRedLightWarningActive)
        {
            double secondsSinceLastRedLightWarning = (DateTime.Now - lastRedLightWarningTime).TotalSeconds;
            if (CheckTrafficLight() && secondsSinceLastRedLightWarning > 10)
            {
                scoreManager.DecreaseScore("redLight");
                PlayFeedback(redLightClip);
                lastRedLightWarningTime = DateTime.Now;
                isRedLightWarningActive = true;
            }
        }
        else ResetTrafficLight(other);
    }

    private bool CheckTrafficLight()
    {
        return (currentTrafficLight != null && !currentTrafficLight.IsCarGreen()) ||
               (currentTrafficLight2 != null && !currentTrafficLight2.IsCarGreen()) ||
               (currentTrafficLight3 != null && !currentTrafficLight3.IsCarGreen());
    }

    private void ResetTrafficLight(Collider other)
    {
        if (other.gameObject.CompareTag("TrafficLight") || other.gameObject.CompareTag("TrafficLight2") || other.gameObject.CompareTag("TrafficLight3"))
        {
            isRedLightWarningActive = false;
            currentTrafficLight = null;
            currentTrafficLight2 = null;
            currentTrafficLight3 = null;
        }
    }

    private void ResetCollisionWarnings()
    {
        // Reset car collision warning if more than 5 seconds have passed
        if (isCollisionCarWarningActive && (DateTime.Now - lastcollisionCarWarningTime).TotalSeconds > 5)
        {
            isCollisionCarWarningActive = false;
        }

        // Reset pedestrian collision warning if more than 5 seconds have passed
        if (isCollisionPedWarningActive && (DateTime.Now - lastcollisionPedWarningTime).TotalSeconds > 5)
        {
            isCollisionPedWarningActive = false;
        }
    }

    private void AssignTrafficLight(Collider trafficLightCollider)
    {
        // If any traffic light is already assigned, return without reassigning
        if (currentTrafficLight != null || currentTrafficLight2 != null || currentTrafficLight3 != null)
        {
            return;
        }

        // Assign the traffic light based on its tag
        if (trafficLightCollider.gameObject.CompareTag("TrafficLight"))
        {
            TrafficLight trafficLight = trafficLightCollider.GetComponent<TrafficLight>();
            if (trafficLight != null)
            {
                currentTrafficLight = trafficLight.trafficLightController;
            }
        }
        else if (trafficLightCollider.gameObject.CompareTag("TrafficLight2"))
        {
            TrafficLight2 trafficLight2 = trafficLightCollider.GetComponent<TrafficLight2>();
            if (trafficLight2 != null)
            {
                currentTrafficLight2 = trafficLight2.trafficLightController2;
            }
        }
        else if (trafficLightCollider.gameObject.CompareTag("TrafficLight3"))
        {
            TrafficLight3 trafficLight3 = trafficLightCollider.GetComponent<TrafficLight3>();
            if (trafficLight3 != null)
            {
                currentTrafficLight3 = trafficLight3.trafficLightController3;
            }
        }
    }

    void PlayFeedback(AudioClip clip)
    {
        audioSource.clip = clip;
        audioSource.Play();
    }

    IEnumerator PlayPBClipAndWait()
    {
        if (PBClip != null)
        {
            audioSource.clip = PBClip;
            audioSource.Play();
            yield return new WaitForSeconds(PBClip.length);
        }
    }

    // void ResetScene()
    // {
    //     // Get the current scene name
    //     string sceneName = SceneManager.GetActiveScene().name;
    //     // Reload the current scene
    //     SceneManager.LoadScene(sceneName);
    //     scoreManager.score = 20000;
    // }
}