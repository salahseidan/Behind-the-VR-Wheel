using UnityEngine;

public class TrafficLight3 : MonoBehaviour
{
    public TrafficLightController3 trafficLightController3;

    private void Awake()
    {
        trafficLightController3 = GetComponent<TrafficLightController3>();
    }
}