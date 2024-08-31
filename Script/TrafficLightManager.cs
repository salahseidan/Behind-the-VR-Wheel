using UnityEngine;
using System.Collections;

public class TrafficLightManager : MonoBehaviour
{
    public TrafficLightController northTrafficLight;
    public TrafficLightController eastTrafficLight;
    public TrafficLightController southTrafficLight;
    public TrafficLightController westTrafficLight;

    private void Start()
    {
        StartCoroutine(ChangeLights());
    }

    private IEnumerator ChangeLights()
    {
        while (true)
        {
        northTrafficLight?.SetGreenLight();
        southTrafficLight?.SetGreenLight();
        eastTrafficLight?.SetRedLight();
        westTrafficLight?.SetRedLight();
        yield return new WaitForSeconds(10);

        northTrafficLight?.SetRedLight();
        southTrafficLight?.SetRedLight();
        eastTrafficLight?.SetGreenLight();
        westTrafficLight?.SetGreenLight();
        yield return new WaitForSeconds(10);
        }
    }
}