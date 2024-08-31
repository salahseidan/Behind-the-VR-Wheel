using UnityEngine;
using UnityEngine.SceneManagement;
using System.Collections;
using System;
using TMPro;

public class DrivingAssistanceSystemHighway : MonoBehaviour
{
    public CarControl carControl;
    public inputManager inputManager;
    public CarAudio carAudio;
    private AudioSource audioSource;
    public AudioClip HighwayClip;
    public AudioClip slowDownClip;
    public AudioClip speedUpClip;
    public AudioClip carColisionClip;
    public AudioClip stallClip;
    public AudioClip laneChangeClip;
    public bool hasPlayedRightLaneChangeAudio = false;
    public bool hasPlayedLeftLaneChangeAudio = false;
    [SerializeField]private ScoreManager scoreManager;
    public TextMeshProUGUI speedLimitText;
    private DateTime lastSpeedingWarningTime = DateTime.MinValue;
    private DateTime lastcollisionCarWarningTime = DateTime.MinValue;
    [SerializeField]private bool isSpeedingWarningActive = false;
    private bool isStallActive = false;
    private float laneChangeCooldown = 10f; 
    private float lastLaneChangeTime = 2f;
    [SerializeField]private int speedLimit = 0;
    [SerializeField]private bool isCollisionCarWarningActive = false;
    private bool leftTurn;
    private bool rightTurn;
    Quaternion lastRotation;
    Vector3 lastPosition;

    void Start()
    {
        audioSource = GetComponent<AudioSource>();
        scoreManager = FindObjectOfType<ScoreManager>();
        audioSource.playOnAwake = false;
        StartCoroutine(PlayHighwayClipAndWait());
        lastRotation = transform.rotation;
        lastPosition = transform.position;
        leftTurn = carControl.leftTurn;
        rightTurn = carControl.rightTurn;
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
    }

    private void HandleCollisions(Collider other)
    {
        if (other.CompareTag("Car"))
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
    }

    private void HandleSpeedingWarnings(Collider other)
    {
        if (other.CompareTag("80") || other.CompareTag("130") || other.CompareTag("260"))
        {
            speedLimit = int.Parse(other.tag);
            double secondsSinceLastWarning = (DateTime.Now - lastSpeedingWarningTime).TotalSeconds;

            if (!isSpeedingWarningActive && carControl.speed > speedLimit + 10 && secondsSinceLastWarning > 5)
            {
                scoreManager.DecreaseScore("speeding");
                PlayFeedback(slowDownClip);
                lastSpeedingWarningTime = DateTime.Now;
                isSpeedingWarningActive = true;
            }
            else if (!isSpeedingWarningActive && carControl.speed > speedLimit / 2 - 5 && carControl.speed < speedLimit / 2 + 10 && secondsSinceLastWarning > 5)
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

    private void ResetCollisionWarnings()
    {
        if (isCollisionCarWarningActive && (DateTime.Now - lastcollisionCarWarningTime).TotalSeconds > 5)
        {
            isCollisionCarWarningActive = false;
        }
    }

    void PlayFeedback(AudioClip clip)
    {
        audioSource.clip = clip;
        audioSource.Play();
    }

    IEnumerator PlayHighwayClipAndWait()
    {
        if (HighwayClip != null)
        {
            audioSource.clip = HighwayClip;
            audioSource.Play();
            yield return new WaitForSeconds(HighwayClip.length);
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