using UnityEngine;
using System.Collections;

public class TrafficLightManager2 : MonoBehaviour
{
    public TrafficLightController2 northTrafficLight;
    public TrafficLightController2 eastTrafficLight;
    public TrafficLightController2 southTrafficLight;
    public TrafficLightController2 westTrafficLight;

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