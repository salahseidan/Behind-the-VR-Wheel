using UnityEngine;

public class TrafficLight : MonoBehaviour
{
    public TrafficLightController trafficLightController;

    private void Awake()
    {
        trafficLightController = GetComponent<TrafficLightController>();
    }
}