using UnityEngine;
using System.Collections;

public class TrafficLightManager3 : MonoBehaviour
{
    public TrafficLightController3 northTrafficLight;
    public TrafficLightController3 eastTrafficLight;
    public TrafficLightController3 southTrafficLight;
    public TrafficLightController3 westTrafficLight;

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