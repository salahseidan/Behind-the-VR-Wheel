using UnityEngine;

public class CameraSwitch : MonoBehaviour

{
    public GameObject VRCamera;
    public GameObject NonVRCamera;
    public GameObject wideAngleCamera;
    public GameObject XRCanvas;
    public GameObject NonVRCanvas;
    public CarControl carControl;
    public ScoreManager scoreManager;
    public DrivingAssistanceSystemHighway drivingAssistanceSystemHighway;
    public DrivingAssistanceSystemPB drivingAssistanceSystemPB;
    public DrivingAssistanceSystem drivingAssistanceSystem;

    public inputManager input_Manager;
    public Transform NONVRTransform;
    Quaternion originalRotation;

    void Start()
    {
        // Set the first person camera active and the wide angle camera inactive
        if (MainMenu.IsVRMode)
        {
            VRCamera.SetActive(true);
            XRCanvas.SetActive(true);
            Canvas canvas = XRCanvas.GetComponent<Canvas>();
            canvas.worldCamera = VRCamera.GetComponent<Camera>();
            carControl.AssignTextComponents(XRCanvas);
            scoreManager.AssignTextComponents(XRCanvas);

            if (drivingAssistanceSystemHighway != null)
            {
                drivingAssistanceSystemHighway.AssignTextComponents(XRCanvas);
            }
            if (drivingAssistanceSystemPB != null)
            {
                drivingAssistanceSystemPB.AssignTextComponents(XRCanvas);
            }
            if (drivingAssistanceSystem != null)
            {
                drivingAssistanceSystem.AssignTextComponents(XRCanvas);
            }
        
            NonVRCamera.SetActive(false);
            wideAngleCamera.SetActive(false);
            NonVRCanvas.SetActive(false);
        }
        
        else
        {
            VRCamera.SetActive(false);
            XRCanvas.SetActive(false);
            NonVRCamera.SetActive(true);
            wideAngleCamera.SetActive(false);
            NonVRCanvas.SetActive(true);
            carControl.AssignTextComponents(NonVRCanvas);
            scoreManager.AssignTextComponents(NonVRCanvas);
            
            if (drivingAssistanceSystemHighway != null)
            {
                drivingAssistanceSystemHighway.AssignTextComponents(NonVRCanvas);
            }
            if (drivingAssistanceSystemPB != null)
            {
                drivingAssistanceSystemPB.AssignTextComponents(NonVRCanvas);
            }
            if (drivingAssistanceSystem != null)
            {
                drivingAssistanceSystem.AssignTextComponents(NonVRCanvas);
            }
        }
    }   

    void Update()
    {
        if (input_Manager.cameraChange)
        {
            if (VRCamera.activeSelf || NonVRCamera.activeSelf)
            {
                wideAngleCamera.SetActive(true);
                VRCamera.SetActive(false);
                NonVRCamera.SetActive(false);
            }
            else
            {
                wideAngleCamera.SetActive(false);
                if (MainMenu.IsVRMode)
                {
                    VRCamera.SetActive(true);
                }
                else
                {
                    NonVRCamera.SetActive(true);
                }
            }
        }
        if (NonVRCamera.activeSelf)
        {
            HandleNonVRCameraMovement();
        }
    }

    void HandleNonVRCameraMovement()
    {
        var rotationSpeed = 200f;

        // Check D-Pad state for rotation
        if (input_Manager.right) {
            NONVRTransform.transform.Rotate(Vector3.up, rotationSpeed * Time.deltaTime);
        }
        else if (input_Manager.left) {
            NONVRTransform.transform.Rotate(Vector3.up, -rotationSpeed * Time.deltaTime);
        }
        else if (input_Manager.down) {
            // Rotate to look behind when down is pressed
            NONVRTransform.transform.rotation = Quaternion.Euler(0, 180, 0);
        }
        else if (input_Manager.up) {
            NONVRTransform.transform.rotation = originalRotation;
        }
    }
}