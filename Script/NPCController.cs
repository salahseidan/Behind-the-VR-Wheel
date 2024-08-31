using UnityEngine;
using UnityEngine.AI;

public class NPCController : MonoBehaviour
{
    public NavMeshAgent agent;
    public Animator animator;
    public int avoidancePriority;


    public GameObject Path; 
    public GameObject player;
    private float activationDistance = 500;
    [SerializeField]private Transform[] pathPoints; 

    public int index; 

    private float minimumDistance = 5f;

    [SerializeField]private bool shouldStop = false;

    private Vector3? storedDestination = null;

    [SerializeField]private TrafficLightController currentTrafficLight;
    [SerializeField]private TrafficLightController2 currentTrafficLight2; 
    [SerializeField]private TrafficLightController3 currentTrafficLight3;  

    void Start()
    {
        agent = GetComponent<NavMeshAgent>();
        animator = GetComponent<Animator>();
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

        // Initialize pathPoints with the waypoints of WaypointsParent
        pathPoints = new Transform[Path.transform.childCount];
        for (int i = 0; i < Path.transform.childCount; i++)
        {
            pathPoints[i] = Path.transform.GetChild(i);
        }
    }

    void Update()
    {
        CheckAndToggleNavMeshAgent();

        bool isRedLight = false;

        // Check the state of each traffic light controller if it's not null
        if (currentTrafficLight != null && !currentTrafficLight.IsPedestrianGreen())
        {
            isRedLight = true;
        }
        else if (currentTrafficLight2 != null && !currentTrafficLight2.IsPedestrianGreen())
        {
            isRedLight = true;
        }
        else if (currentTrafficLight3 != null && !currentTrafficLight3.IsPedestrianGreen())
        {
            isRedLight = true;
        }

        // React based on the isRedLight flag
        if (shouldStop && isRedLight)
        {
            if(agent == null || !agent.enabled || !agent.isOnNavMesh) return;
            if (!agent.isStopped)
            {
                storedDestination = agent.destination;
                agent.isStopped = true;
            }
            animator.SetFloat("vertical", 0);
        }
        else
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
            roam();
        }
    }

    void CheckAndToggleNavMeshAgent()
    {
        if (player == null) return;

        float distanceToPlayer = Vector3.Distance(transform.position, player.transform.position);

        // Activate or deactivate NavMeshAgent based on distance
        agent.enabled = distanceToPlayer <= activationDistance;
    }

    void roam()
    {
        if(agent == null || !agent.enabled || !agent.isOnNavMesh) return;
        if(index >= 0 && index < pathPoints.Length){
            if(Vector3.Distance(transform.position, pathPoints[index].position) < minimumDistance){
                // Move to the next waypoint in the current direction
                index = (index + 1) % pathPoints.Length;
            }
            agent.SetDestination(pathPoints[index].position);
            animator.SetFloat("vertical", !agent.isStopped ? 1 : 0);
        }
        else
        {
            index = 0;
        }
    }

    private void OnTriggerEnter(Collider other)
    {
        if (other.gameObject.CompareTag("StopLine"))
        {
            // Check if the character is facing the same direction as the stop line
            float angle = transform.position.x - other.transform.position.x;
            if (angle < 90) // Check if the angle is close to 0
            {
                // Character is on the X side and facing the same direction as the stop line
                shouldStop = true;
            }
        }
        else if (other.gameObject.CompareTag("OppositeStopLine"))
        {
            // Check if the character is facing the same direction as the stop line
            float angle = transform.position.x - other.transform.position.x;
            if (angle > 100) // Check if the angle is close to 0
            {
                // Character is on the -X side and facing the same direction as the stop line
                shouldStop = true;
            }
        }
        else if (other.gameObject.CompareTag("TrafficLight") || other.gameObject.CompareTag("TrafficLight2") || other.gameObject.CompareTag("TrafficLight3"))
        {
            AssignTrafficLight(other);
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
        if (other.gameObject.CompareTag("StopLine") || other.gameObject.CompareTag("OppositeStopLine"))
        {
            shouldStop = false;
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
}