using UnityEngine;
using System.Collections.Generic;

public class SmartIntersectionController : MonoBehaviour
{
    [SerializeField]
    private List<string> directionOrder; 

    private bool isIntersectionOccupied = false;

    private Dictionary<string, Queue<CarNPCController>> queues = new Dictionary<string, Queue<CarNPCController>>();

    private void Start()
    {
        foreach (var direction in directionOrder)
        {
            queues.Add(direction, new Queue<CarNPCController>());
        }
    }

    public void EnqueueCar(CarNPCController car, string direction)
    {
        if (queues.ContainsKey(direction))
        {
            queues[direction].Enqueue(car);
        }
    }

    private void Update()
    {
        // If the intersection is occupied, do not allow any car to proceed.
        if (isIntersectionOccupied) return;

        foreach (var direction in directionOrder)
        {
            if (direction != null && queues.ContainsKey(direction) && queues[direction].Count > 0)
            {
                CarNPCController carToProceed = queues[direction].Peek();

                if (carToProceed != null && CanProceed(carToProceed.gameObject))
                {
                    carToProceed.shouldStop = false;
                    isIntersectionOccupied = true; // Mark the intersection as occupied
                    queues[direction].Dequeue();
                    break; // Ensure only one car proceeds per update cycle
                }
            }
        }
    }

    // Method to be called by CarNPCController when it exits the intersection
    public void NotifyCarExitedIntersection()
    {
        isIntersectionOccupied = false;
    }

    public bool CanProceed(GameObject car)
    {
        // If the intersection is occupied, no car can proceed.
        if (isIntersectionOccupied) return false;

        string direction = car.GetComponent<CarNPCController>().DetermineDirection();
        CarNPCController carController = car.GetComponent<CarNPCController>();
        if (queues.ContainsKey(direction) && queues[direction].Count > 0 && queues[direction].Peek() == carController)
        {
            return true;
        }
        else
        {
            return false;
        }
    }
}