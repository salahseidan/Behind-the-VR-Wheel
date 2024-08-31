using UnityEngine;

public class TrafficLight2 : MonoBehaviour
{
    public TrafficLightController2 trafficLightController2;

    private void Awake()
    {
        trafficLightController2 = GetComponent<TrafficLightController2>();
    }
}