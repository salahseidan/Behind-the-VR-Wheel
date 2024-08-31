using UnityEngine;
using UnityEngine.SceneManagement;
using System.Collections;
using System;
using TMPro;

public class DrivingAssistanceSystem : MonoBehaviour
{
    public CarControl carControl;
    public inputManager inputManager;
    public CarAudio carAudio;
    public GameObject arrow;
    private ScoreManager scoreManager;
    private AudioSource audioSource;
    public AudioClip startEngineClip;
    public AudioClip slowDownClip;
    public AudioClip pressClutchClip;
    public AudioClip pressGasClip;
    public AudioClip pressBrakeClip;
    public AudioClip TrainingAreaClip;
    public AudioClip StallClip;
    public AudioClip seatBeltClip;
    public AudioClip firstWaypointClip;
    public AudioClip secondWaypointClip;
    public AudioClip parkingClip;
    public AudioClip lastClip;
    public AudioClip coneClip;
    public AudioClip carColisionClip;
    public Animator arrowAnimator;
    public Transform[] waypoints;
    [SerializeField]private int currentWaypointIndex = 0;
    [SerializeField]private bool hasPlayedClutchClip;
    [SerializeField]private bool hasPlayedSeatBeltClip;
    [SerializeField]private bool hasPlayedStartEngineClip;
    [SerializeField]private bool hasPlayedgasClip;
    [SerializeField]private bool hasPlayedBrakeClip;
    [SerializeField]private bool hasPlayedFirstWaypoint;
    [SerializeField]private bool hasPlayedSecondWaypoint;
    [SerializeField]private bool hasPlayedParkingClip;
    [SerializeField]private bool hasPlayedLastClip;
    private bool isStallActive = false;
    private bool wasBrake = false;
    public TextMeshProUGUI speedLimitText;
    private bool allowPlayerActions = false;
    private float cooldown = 1f;
    private float lastPlayedTime = 0f;
    [SerializeField]private int speedLimit = 0;
    [SerializeField]private bool parkedSuccessfully = false;    
    private bool isAtWaypoint = false;

    void Start()
    {
        audioSource = GetComponent<AudioSource>();
        audioSource.playOnAwake = false;
        scoreManager = FindObjectOfType<ScoreManager>();
        hasPlayedClutchClip = false;
        hasPlayedSeatBeltClip = false;
        hasPlayedStartEngineClip = false;
        hasPlayedgasClip = false;
        hasPlayedBrakeClip = false;
        hasPlayedFirstWaypoint = false;
        hasPlayedSecondWaypoint = false;
        hasPlayedParkingClip = false;
        hasPlayedLastClip = false;

        Vector3 startPosition = waypoints[0].position + Vector3.up * 4; 
        Quaternion startRotaion = Quaternion.Euler(0, 0, 90); 
        arrow.transform.position = startPosition;
        arrow.transform.rotation = startRotaion;
        arrowAnimator = arrow.GetComponent<Animator>(); 

        StartCoroutine(PlayTrainingClipAndWait());
        }

    void Update()
    {   
        if (speedLimitText != null)
        {
            speedLimitText.text = "Speed Limit: " + speedLimit;
        }

        if (!allowPlayerActions)
        {
            return;
        }
        CheckPlayerActions();

        // if (inputManager.reset)
        // {
        //     ResetScene();
        // }

        if (!carControl.stall && isStallActive)
        {
            isStallActive = false;
        }
        
        if (carControl.stall && !isStallActive)
        {
            scoreManager.DecreaseScore("stalling");
            PlayFeedback(StallClip);
            isStallActive = true;
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
        if (other.CompareTag("Waypoint") && !isAtWaypoint)
        {
            isAtWaypoint = true;

            if (currentWaypointIndex < waypoints.Length)
            {
                currentWaypointIndex++;
                PositionArrow(waypoints[currentWaypointIndex]);
            }
        }

        if (other.CompareTag("Park") && hasPlayedParkingClip && !hasPlayedLastClip)
        {
            PlayFeedbackNormal(lastClip);
            hasPlayedLastClip = true;
        }

        if (other.CompareTag("Cone"))
        {
            // Check if enough time has passed since the last sound was played
            if (Time.time - lastPlayedTime >= cooldown)
            {
                PlayFeedbackNormal(coneClip);
                scoreManager.DecreaseScore("collision with cones");
                // Update the last played time to the current time
                lastPlayedTime = Time.time;
            }
        }

        if (other.CompareTag("Car"))
        {
            // Check if enough time has passed since the last sound was played
            if (Time.time - lastPlayedTime >= cooldown)
            {
                PlayFeedbackNormal(carColisionClip);
                scoreManager.DecreaseScore("collision with cars");
                lastPlayedTime = Time.time;
            }
        }

        if (other.CompareTag("ParkingSpace"))
        {
            CheckParkingSuccess();
        }
    }

    void OnTriggerExit(Collider other)
    {
        if (other.CompareTag("Waypoint"))
        {
            isAtWaypoint = false; 
        }
    }

    void PositionArrow(Transform waypoint)
    {
        Vector3 newPosition = waypoint.position + Vector3.up * 5;
        Quaternion newRotaion = Quaternion.Euler(0, 0, 90);
        arrow.transform.position = newPosition;
        arrow.transform.rotation = newRotaion;
    }

    IEnumerator PlayFeedback(AudioClip clip)
    {
        yield return new WaitForSeconds(3); // Wait for 3 seconds before playing the next sound
        audioSource.clip = clip;
        audioSource.Play();
    }

    IEnumerator PlayTrainingClipAndWait()
    {
        if (TrainingAreaClip != null)
        {
            audioSource.clip = TrainingAreaClip;
            audioSource.Play();
            yield return new WaitForSeconds(TrainingAreaClip.length);
        }
        allowPlayerActions = true; 
    }

    void PlayFeedbackNormal(AudioClip clip)
    {
        audioSource.clip = clip;
        audioSource.Play();
    }

    void CheckPlayerActions()
    {  
        if (!carControl.engine && !hasPlayedStartEngineClip)
        {
            StartCoroutine(PlayFeedback(startEngineClip));
            hasPlayedStartEngineClip = true;
        }

        if (carControl.engine && !carControl.seatBelt && !hasPlayedSeatBeltClip && hasPlayedStartEngineClip && carAudio.engineSoundPlayed)
        {
            StartCoroutine(PlayFeedback(seatBeltClip));
            hasPlayedSeatBeltClip = true;
        }

        if (carControl.engine && !hasPlayedClutchClip && hasPlayedSeatBeltClip && carControl.seatBelt)
        {
            StartCoroutine(PlayFeedback(pressClutchClip));
            hasPlayedClutchClip = true;
            hasPlayedgasClip = false;
            hasPlayedBrakeClip = false;
        }

        if (carControl.stall && hasPlayedClutchClip)
        {
            scoreManager.DecreaseScore("stalling");
            StartCoroutine(PlayFeedback(StallClip));
            hasPlayedClutchClip = false;
        }

        if (carControl.engine && hasPlayedClutchClip && carControl.seatBelt 
        && !carControl.stall && carControl.speed > 0.1 && carControl.speed < 4 && inputManager.ClutchInput < 0.1 && !hasPlayedgasClip)
        {
            StartCoroutine(PlayFeedback(pressGasClip));
            hasPlayedgasClip = true;
        }

        if (carControl.engine && hasPlayedClutchClip && carControl.seatBelt && !carControl.stall
        && carControl.speed > 8f && !hasPlayedBrakeClip)
        {
            StartCoroutine(PlayFeedback(pressBrakeClip));
            hasPlayedBrakeClip = true;
        }

        if (inputManager.BrakeInput > 0.4)
        {
            wasBrake = true;
        }

        if (carControl.engine && hasPlayedBrakeClip && !hasPlayedFirstWaypoint && currentWaypointIndex == 0
        && carAudio.brakeSoundPlayed && wasBrake)
        {
            StartCoroutine(PlayFeedback(firstWaypointClip));
            hasPlayedFirstWaypoint = true;
        }

        if (currentWaypointIndex == 1 && hasPlayedFirstWaypoint && !hasPlayedSecondWaypoint)
        {
            StartCoroutine(PlayFeedback(secondWaypointClip));
            hasPlayedSecondWaypoint = true;
        }

        if (currentWaypointIndex == 2 && hasPlayedSecondWaypoint && !hasPlayedParkingClip && !parkedSuccessfully)
        {
            StartCoroutine(PlayFeedback(parkingClip));
            hasPlayedParkingClip = true;
        }

        if (currentWaypointIndex == 3 && hasPlayedParkingClip && !hasPlayedLastClip)
        {
            StartCoroutine(PlayFeedback(lastClip));
            hasPlayedLastClip = true;
        }
    }

    // void ResetScene()
    // {
    //     string sceneName = SceneManager.GetActiveScene().name;
    //     SceneManager.LoadScene(sceneName);
    // }

    void OnTriggerStay(Collider other)
    {
        if (other.CompareTag("ParkingSpace"))
        {
            CheckParkingSuccess();
        }
    }

    void CheckParkingSuccess()
    {
        BoxCollider carCollider = GetComponent<BoxCollider>();
        Collider parkingSpaceCollider = GameObject.FindGameObjectWithTag("ParkingSpace").GetComponent<BoxCollider>();

        // Check if the car's collider is completely within the parking space's collider
        if (parkingSpaceCollider.bounds.Contains(carCollider.bounds.min) && parkingSpaceCollider.bounds.Contains(carCollider.bounds.max))
        {
            parkedSuccessfully = true;
        }
    }
}