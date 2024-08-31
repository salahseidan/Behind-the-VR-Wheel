using System.Collections.Generic;
using UnityEngine;
using UnityEngine.AI;

public class CarNPCController : MonoBehaviour
{
    public NavMeshAgent agent;
    public int avoidancePriority;

    public GameObject Path; 
    public GameObject player;
    private float activationDistance = 1000;
    [SerializeField] private Transform[] pathPoints;

    public List<GameObject> nearbyCars = new List<GameObject>(); 

    public int index; 

    public int spawnIndex;

    private float minimumDistance = 2f;

    public bool shouldStop = false;

    private Vector3? storedDestination = null;

    [SerializeField] private TrafficLightController currentTrafficLight;
    [SerializeField] private TrafficLightController2 currentTrafficLight2; 
    [SerializeField] private TrafficLightController3 currentTrafficLight3; 
    [SerializeField]private bool shouldStopForPlayer = false;
    [SerializeField]private bool shouldStopForNPC = false;

    public bool isSplineClosed = true; 

    [SerializeField] private SmartIntersectionController currentSmartIntersection;

    void Start()
    {
        agent = GetComponent<NavMeshAgent>();
        agent.avoidancePriority = avoidancePriority;

        if (player == null)
        {
            player = GameObject.FindGameObjectWithTag("VW");
        }

        if (player == null)
        {
            player = GameObject.FindGameObjectWithTag("BMW");
        }

        if (player == null)
        {
            player = GameObject.FindGameObjectWithTag("FIAT");
        }

        pathPoints = new Transform[Path.transform.childCount];
        for (int i = 0; i < Path.transform.childCount; i++)
        {
            pathPoints[i] = Path.transform.GetChild(i);
        }
    }

    void Update()
    {
        CheckAndToggleNavMeshAgent();
        CheckDistanceToPlayer();
        CheckDistanceToNearbyCars();
        CheckTrafficLightState();

        if (shouldStopForPlayer)
        {
            StopCar();
        }
        else if (shouldStop) 
        {
            StopCar();
        }
        else if (shouldStopForNPC)
        {
            StopCar();
        }
        else
        {
            ResumeCarMovement();
            roam();
        }
    }

    void CheckDistanceToPlayer()
    {
        if (player != null)
        {
            float distanceToPlayer = Vector3.Distance(transform.position, player.transform.position);
            if (distanceToPlayer <= 20)
            {
                shouldStopForPlayer = true;
            }
            else if (distanceToPlayer > minimumDistance + 2) 
            {
                    shouldStopForPlayer = false;
            }
        }
    }

    void CheckDistanceToNearbyCars()
    {
        shouldStopForNPC = false; 

        foreach (GameObject car in nearbyCars)
        {
            if (car != null)
            {
                
                Vector3 directionToCar = car.transform.position - transform.position;
                float angle = Vector3.Dot(directionToCar.normalized, transform.forward);

                if (angle > 0)
                {
                    float distanceToCar = directionToCar.magnitude;
                    if (distanceToCar <= 20)
                    {
                        shouldStopForNPC = true;
                        break; 
                    }
                }
            }
        }
    }

    void CheckAndToggleNavMeshAgent()
    {
        if (player == null) return;

        float distanceToPlayer = Vector3.Distance(transform.position, player.transform.position);

        // Activate or deactivate NavMeshAgent based on distance
        agent.enabled = distanceToPlayer <= activationDistance;
    }

    void StopCar()
    {
        if(agent == null || !agent.enabled || !agent.isOnNavMesh) return;
        if (!agent.isStopped)
        { 
            storedDestination = agent.destination;
            agent.isStopped = true;
        }
    }

    void ResumeCarMovement()
    {
        if(agent == null || !agent.enabled || !agent.isOnNavMesh) return;

        if (agent.isStopped)
        {
            agent.isStopped = false;
            if (storedDestination.HasValue)
            {
                agent.SetDestination(storedDestination.Value);
                storedDestination = null;
            }
        }
    }
    void roam()
    {
        if(agent == null || !agent.enabled || !agent.isOnNavMesh) return;
        if(index >= 0 && index < pathPoints.Length)
        {
            if(Vector3.Distance(transform.position, pathPoints[index].position) < minimumDistance)
            {
                if(!isSplineClosed && index == pathPoints.Length - 1)
                {
                    index = spawnIndex; 
                    RespawnAtSpawnIndex(); 
                }
                else
                {
                    index = (index + 1) % pathPoints.Length;
                }
            }
            agent.SetDestination(pathPoints[index].position);
        }
        else
        {
            index = 0; 
        }
    }

    void CheckTrafficLightState()
    {
        shouldStop = (currentTrafficLight != null && !currentTrafficLight.IsCarGreen()) ||
                    (currentTrafficLight2 != null && !currentTrafficLight2.IsCarGreen()) ||
                    (currentTrafficLight3 != null && !currentTrafficLight3.IsCarGreen());
    }

    void RespawnAtSpawnIndex()
    {
        var agent = GetComponent<NavMeshAgent>();
        var rb = GetComponent<Rigidbody>();

        if (agent != null)
        {
            agent.enabled = false;
            transform.position = pathPoints[spawnIndex].position;
            // Explicitly set the rotation to match the spawn point's rotation or a predefined orientation
            transform.rotation = Quaternion.LookRotation(pathPoints[spawnIndex].forward);
            agent.enabled = true;
            agent.ResetPath(); 
            agent.Warp(pathPoints[spawnIndex].position); 
        }
        else
        {
            transform.position = pathPoints[spawnIndex].position;
            // Optionally, set rotation here as well if needed
        }

        // If there's a Rigidbody, reset its forces
        if (rb != null)
        {
            rb.velocity = Vector3.zero; // Reset linear velocity
            rb.angularVelocity = Vector3.zero; // Reset angular velocity
        }
    }

    private void OnTriggerEnter(Collider other)
    {
        if (other.CompareTag("Car") && !nearbyCars.Contains(other.gameObject))
        {
            nearbyCars.Add(other.gameObject); 
        }

        if (other.CompareTag("30") || other.CompareTag("50") || other.CompareTag("100"))
        {
            int speedLimit = int.Parse(other.tag);
            float speedInMetersPerSecond = speedLimit / 3.6f;
            // storedSpeedBeforeStopping = speedInMetersPerSecond;
            agent.speed = speedInMetersPerSecond;
        }
        // Handle Traffic Light logic
        if (other.gameObject.CompareTag("TrafficLight") || other.gameObject.CompareTag("TrafficLight2") || other.gameObject.CompareTag("TrafficLight3"))
        {
            AssignTrafficLight(other);
            // Handle Car Stop areas specifically for traffic lights
            if (other.gameObject.CompareTag("Car Stop"))
            {
                shouldStop = true;
            }
        }
        // Handle Smart Intersection logic
        if (other.gameObject.CompareTag("SmartIntersection"))
        {

            currentSmartIntersection = other.GetComponent<SmartIntersectionController>();
            if (currentSmartIntersection != null)
            {

                string direction = DetermineDirection(); // Placeholder method to determine the car's direction

                currentSmartIntersection.EnqueueCar(this, direction); // Add this car to the queue with direction

                shouldStop = true; // Stop at the currentSmartIntersection until it's this car's turn
            }
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

    private void OnTriggerExit(Collider other)
    {
        if (other.gameObject.CompareTag("Car Stop"))
        {
            shouldStop = false;
        }

        if (other.CompareTag("Car"))
        {
            nearbyCars.Remove(other.gameObject); 
        }

        if (other.gameObject.CompareTag("SmartIntersection"))
        {
            shouldStop = false;
            currentSmartIntersection.NotifyCarExitedIntersection();
        }

        else if (other.gameObject.CompareTag("TrafficLight"))
        {
            TrafficLight trafficLight = other.GetComponent<TrafficLight>();
            if (trafficLight != null && currentTrafficLight == trafficLight.trafficLightController)
            {
                currentTrafficLight = null;
            }
        }
        else if (other.gameObject.CompareTag("TrafficLight2"))
        {
            TrafficLight2 trafficLight2 = other.GetComponent<TrafficLight2>();
            if (trafficLight2 != null && currentTrafficLight2 == trafficLight2.trafficLightController2)
            {
                currentTrafficLight2 = null;
            }
        }
        else if (other.gameObject.CompareTag("TrafficLight3"))
        {
            TrafficLight3 trafficLight3 = other.GetComponent<TrafficLight3>();
            if (trafficLight3 != null && currentTrafficLight3 == trafficLight3.trafficLightController3)
            {
                currentTrafficLight3 = null;
            }
        }
    }

    public string DetermineDirection()
    {
        
        Vector3 directionVector = transform.forward; 
        float angle = Vector3.SignedAngle(directionVector, Vector3.forward, Vector3.up);

        if (angle < 45 && angle > -45)
        {
            return "North";
        }
        else if (angle >= 45 && angle <= 135)
        {
            return "East";
        }
        else if (angle <= -45 && angle >= -135)
        {
            return "West";
        }
        else
        {
            return "South";
        }
    }
}