using System.Collections;
using UnityEngine;

public class TrafficLightController2 : MonoBehaviour
{
    [SerializeField] private Material redLight;
    [SerializeField] private Material greenLight;
    [SerializeField] private Material yellowLight;
    [SerializeField] private Material pedestrianRedLight;
    [SerializeField] private Material pedestrianGreenLight;
    [SerializeField] private Material leftRedLight;
    [SerializeField] private Material leftYellowLight;
    [SerializeField] private Material leftGreenLight;
    [SerializeField] private Material bikeRedLight; 
    [SerializeField] private Material bikeYellowLight; 
    [SerializeField] private Material bikeGreenLight; 

    private Material[] materials;

    [SerializeField] private MeshRenderer trafficLightRenderer;

    private void Awake()
    {
        trafficLightRenderer = GetComponent<MeshRenderer>();
        materials = trafficLightRenderer.materials;

        leftYellowLight = materials[1];
        leftGreenLight = materials[2];
        greenLight = materials[3];
        leftRedLight = materials[4];
        redLight = materials[5];
        yellowLight = materials[6];
        pedestrianGreenLight = materials[8];
        pedestrianRedLight = materials[9];
        bikeGreenLight = materials[10];
        bikeYellowLight = materials[11];
        bikeRedLight = materials[12];
    }

    public float transitionTime = 15f; // Time for transitions between lights

    public void SetRedLight()
    {
        StartCoroutine(TransitionToRed());
    }

    private IEnumerator TransitionToRed()
    {
        SetYellowLight();
        yield return new WaitForSeconds(transitionTime);
        SetLights(true, false, false, false, true, true, false, false, false, false, true); 
    }

    public void SetYellowLight()
    {
        SetLights(false, true, false, true, false, false, true, false, true, true, false); 
    }

    public void SetGreenLight()
    {
        StartCoroutine(TransitionToGreen());
    }

    private IEnumerator TransitionToGreen()
    {
        SetLights(true, true, false, false, true, true, true, false, false, true, false); 
        yield return new WaitForSeconds(transitionTime);
        SetLights(false, false, true, true, false, false, false, true, true, false, false); 
    }

    private void SetLights(bool red, bool yellow, bool green, bool pedestrianRed, bool pedestrianGreen, bool leftRed, bool leftYellow, bool leftGreen, bool bikeRed, bool bikeYellow, bool bikeGreen)
    {
        if (red) SetEmission(redLight, true); else SetEmission(redLight, false);
        if (green) SetEmission(greenLight, true); else SetEmission(greenLight, false);
        if (yellow) SetEmission(yellowLight, true); else SetEmission(yellowLight, false);
        
        pedestrianRedLight.color = pedestrianRed ? Color.white : Color.black;
        pedestrianGreenLight.color = pedestrianGreen ? Color.white : Color.black;
    
        if (leftRed) SetEmission(leftRedLight, true); else SetEmission(leftRedLight, false);
        if (leftYellow) SetEmission(leftYellowLight, true); else SetEmission(leftYellowLight, false);
        if (leftGreen) SetEmission(leftGreenLight, true); else SetEmission(leftGreenLight, false);
        if (bikeRed) SetEmission(bikeRedLight, true); else SetEmission(bikeRedLight, false);
        if (bikeYellow) SetEmission(bikeYellowLight, true); else SetEmission(bikeYellowLight, false);
        if (bikeGreen) SetEmission(bikeGreenLight, true); else SetEmission(bikeGreenLight, false);
    }

    private void SetEmission(Material material, bool isOn)
    {
        if (isOn)
        {
            material.EnableKeyword("_EMISSION");
        }
        else
        {
            material.DisableKeyword("_EMISSION");
        }
    }

    public bool IsPedestrianGreen()
    {
        return pedestrianGreenLight.color == Color.white;
    }

    public bool IsCarGreen()
    {
        // Check if the green light material's emission is enabled, indicating the car's green light is on
        return greenLight.IsKeywordEnabled("_EMISSION");
    }
}