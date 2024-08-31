using UnityEngine;
using System.IO;
using UnityEngine.UI;
using UnityEngine.SceneManagement;
using System.Collections.Generic;

public class MainMenu : MonoBehaviour
{
    public InputManagerNormal inputManager;
    public GameObject[] cameraWaypoints;
    public GameObject[] carPrefabs;
    public GameObject[] modeNames; 
    public GameObject[] carsNames; 
    public GameObject[] timeNames;
    public GameObject[] ViewNames;
    public GameObject[] AssistanceNames;    
    public Button[] mainMenuButtons;
    public Button[] timemenuButtons;
    public Button[] carSelectButtons;
    public Button[] modeSelectButtons;
    public Button[] ViewButtons;
    public Button[] AssistanceButtons;
    public Material daySkybox;
    public Material nightSkybox;
    public Color dayLightColor;
    public Color nightLightColor;
    public GameObject mainMenu;
    public GameObject carSelectMenu;
    public GameObject modeSelectMenu;
    public GameObject timeSelectMenu;
    public GameObject ViewMenu;
    public GameObject AssistanceMenu;
    private int selectedMainMenuIndex = 0;
    private int selectedCarSelectIndex = 0;
    private int selectedModeSelectIndex = 0;
    private int selectedTimeMenuIndex = 0;
    private int selectedViewMenuIndex = 0;
    private int selectedAssistanceMenuIndex = 0;
    public Color selectedColor;
    public Color normalColor;
    private int selectedCarIndex = 0;
    private int selectedModeIndex = 0;
    private int selectedTimeIndex = 0;
    private int selectedViewIndex = 0;
    private int selectedAssistanceIndex = 0;
    public float lerpSpeed = 0.1f;
    private float lastInputTime = 0f;
    private float inputCooldown = 0.5f;
    private bool carSelected = false;
    private bool modeSelected = false;
    private bool timeSelected = false; 
    private bool viewSelected = false;
    private bool assistanceSelected = false;
    private bool isDayTime = true;
    public static bool IsVRMode = true;
    private bool hasReachedWaypointOnce = false;
    private DrivingAssistanceSystemPB drivingAssistanceSystemPB;
    private DrivingAssistanceSystemHighway drivingAssistanceSystemHighway;
    private DrivingAssistanceSystem drivingAssistanceSystemTraining;
    [SerializeField]private string selectedCarTag;
    [SerializeField]private string selectedMode;
    private string logFilePath;
    public GameObject backBackground;

    void Start()
    {
        string persistentPath = Application.persistentDataPath;
        logFilePath = Path.Combine(persistentPath, "game_log.txt");
        File.WriteAllText(logFilePath, "Log Start\n"); 
        LogToFile($"Persistent Data Path: {persistentPath}");

        transform.position = cameraWaypoints[0].transform.position;
        HighlightSelectedButton();
        mainMenu.SetActive(false);
        backBackground.SetActive(false);
        carSelectMenu.SetActive(false);
        modeSelectMenu.SetActive(false);
        timeSelectMenu.SetActive(false);
        ViewMenu.SetActive(false);
        AssistanceMenu.SetActive(false);

        for (int i = 0; i < carPrefabs.Length; i++)
        {
            carPrefabs[i].gameObject.SetActive(false);
            carsNames[i].gameObject.SetActive(false);
        }
    }

    private void LogToFile(string message)
    {
        File.AppendAllText(logFilePath, message + "\n");
    }

    void FixedUpdate()
    {
        transform.position = Vector3.Lerp(transform.position, cameraWaypoints[1].transform.position, lerpSpeed);

        if (!hasReachedWaypointOnce && Vector3.Distance(transform.position, cameraWaypoints[1].transform.position) < 0.1f)
        {
            mainMenu.SetActive(true);
            backBackground.SetActive(true);
            mainMenuButtons[5].gameObject.SetActive(false);
            hasReachedWaypointOnce = true; 
        }
    }

    void Update()
    {
        float currentTime = Time.time;

        selectedCarTag = GetCarTagByIndex(selectedCarIndex);
        selectedMode = GetSceneNameByIndex(selectedModeIndex);

        if (currentTime - lastInputTime > inputCooldown)
        {
            if (inputManager.left)
            {
                if (mainMenu.activeSelf)
                {
                    selectedMainMenuIndex--;
                    if (selectedMainMenuIndex < 0) selectedMainMenuIndex = mainMenuButtons.Length - 1;
                    HighlightSelectedButton();
                }
                else if (carSelectMenu.activeSelf)
                {
                    selectedCarSelectIndex--;
                    if (selectedCarSelectIndex < 0) selectedCarSelectIndex = carSelectButtons.Length - 1;
                    HighlightSelectedButton();
                }
                else if (modeSelectMenu.activeSelf)
                {
                    selectedModeSelectIndex--;
                    if (selectedModeSelectIndex < 0) selectedModeSelectIndex = modeSelectButtons.Length - 1;
                    HighlightSelectedButton();
                }
                else if (timeSelectMenu.activeSelf)
                {
                    selectedTimeMenuIndex--;
                    if (selectedTimeMenuIndex < 0) selectedTimeMenuIndex = timemenuButtons.Length - 1;
                    HighlightSelectedButton();
                }
                else if (ViewMenu.activeSelf)
                {
                    selectedViewMenuIndex--;
                    if (selectedViewMenuIndex < 0) selectedViewMenuIndex = ViewButtons.Length - 1;
                    HighlightSelectedButton();
                }
                else if (AssistanceMenu.activeSelf)
                {
                    selectedAssistanceMenuIndex--;
                    if (selectedAssistanceMenuIndex < 0) selectedAssistanceMenuIndex = AssistanceButtons.Length - 1;
                    HighlightSelectedButton();
                }
                
                lastInputTime = currentTime;
            }

            if (inputManager.right)
            {
                
                if (mainMenu.activeSelf)
                {
                    selectedMainMenuIndex++;
                    if (selectedMainMenuIndex >= mainMenuButtons.Length) selectedMainMenuIndex = 0;
                    HighlightSelectedButton();
                }
                else if (carSelectMenu.activeSelf)
                {
                    selectedCarSelectIndex++;
                    if (selectedCarSelectIndex >= carSelectButtons.Length) selectedCarSelectIndex = 0;
                    HighlightSelectedButton();
                }
                else if (modeSelectMenu.activeSelf)
                {
                    selectedModeSelectIndex++;
                    if (selectedModeSelectIndex >= modeSelectButtons.Length) selectedModeSelectIndex = 0;
                    HighlightSelectedButton();
                }
                else if (timeSelectMenu.activeSelf)
                {
                    selectedTimeMenuIndex++;
                    if (selectedTimeMenuIndex >= timemenuButtons.Length) selectedTimeMenuIndex = 0;
                    HighlightSelectedButton();
                }
                else if (ViewMenu.activeSelf)
                {
                    selectedViewMenuIndex++;
                    if (selectedViewMenuIndex >= ViewButtons.Length) selectedViewMenuIndex = 0;
                    HighlightSelectedButton();
                }
                else if (AssistanceMenu.activeSelf)
                {
                    selectedAssistanceMenuIndex++;
                    if (selectedAssistanceMenuIndex >= AssistanceButtons.Length) selectedAssistanceMenuIndex = 0;
                    HighlightSelectedButton();
                }

                lastInputTime = currentTime;
            }

            if (inputManager.X) 
            {
                if (mainMenu.activeSelf)
                {
                    mainMenuButtons[selectedMainMenuIndex].onClick.Invoke();
                }
                else if (carSelectMenu.activeSelf)
                {
                    carSelectButtons[selectedCarSelectIndex].onClick.Invoke();
                }
                else if (modeSelectMenu.activeSelf)
                {
                    modeSelectButtons[selectedModeSelectIndex].onClick.Invoke();
                }
                else if (timeSelectMenu.activeSelf)
                {
                    timemenuButtons[selectedTimeMenuIndex].onClick.Invoke();
                }
                else if (ViewMenu.activeSelf)
                {
                    ViewButtons[selectedViewMenuIndex].onClick.Invoke();
                }
                else if (AssistanceMenu.activeSelf)
                {
                    AssistanceButtons[selectedAssistanceMenuIndex].onClick.Invoke();
                }
            }

            // set all others in list to false 
            for (int i = 0; i < carsNames.Length; i++)
            {
                if (i != selectedCarIndex)
                {
                    carsNames[selectedCarIndex].gameObject.SetActive(true);
                    carPrefabs[selectedCarIndex].gameObject.SetActive(true);
                    carsNames[i].gameObject.SetActive(false);
                    carPrefabs[i].gameObject.SetActive(false);
                }
            }

            for (int i = 0; i < modeNames.Length; i++)
            {
                if (i != selectedModeIndex)
                {
                    modeNames[selectedModeIndex].gameObject.SetActive(true);
                    modeNames[i].gameObject.SetActive(false);
                }
            }

            for (int i = 0; i < timeNames.Length; i++)
            {
                if (i != selectedTimeIndex)
                {
                    timeNames[selectedTimeIndex].gameObject.SetActive(true);
                    timeNames[i].gameObject.SetActive(false);
                }
            }

            for (int i = 0; i < ViewNames.Length; i++)
            {
                if (i != selectedViewIndex)
                {
                    ViewNames[selectedViewIndex].gameObject.SetActive(true);
                    ViewNames[i].gameObject.SetActive(false);
                }
            }

            for (int i = 0; i < AssistanceNames.Length; i++)
            {
                if (i != selectedAssistanceIndex)
                {
                    AssistanceNames[selectedAssistanceIndex].gameObject.SetActive(true);
                    AssistanceNames[i].gameObject.SetActive(false);
                }
            }
        }
    }

    private void HighlightSelectedButton()
    {
            if(mainMenu.activeSelf)
            {
                for (int i = 0; i < mainMenuButtons.Length; i++)
                {
                    mainMenuButtons[i].GetComponent<Image>().color = i == selectedMainMenuIndex ? selectedColor : normalColor;
                }
            }
            else if (carSelectMenu.activeSelf)
            {
                for (int i = 0; i < carSelectButtons.Length; i++)
                {
                    carSelectButtons[i].GetComponent<Image>().color = i == selectedCarSelectIndex ? selectedColor : normalColor;
                }
            }
            else if (modeSelectMenu.activeSelf)
            {
                for (int i = 0; i < modeSelectButtons.Length; i++)
                {
                    modeSelectButtons[i].GetComponent<Image>().color = i == selectedModeSelectIndex ? selectedColor : normalColor;
                }
            }
            else if (timeSelectMenu.activeSelf)
            {
                for (int i = 0; i < timemenuButtons.Length; i++)
                {
                    timemenuButtons[i].GetComponent<Image>().color = i == selectedTimeMenuIndex ? selectedColor : normalColor;
                }
            }
            else if (ViewMenu.activeSelf)
            {
                for (int i = 0; i < ViewButtons.Length; i++)
                {
                    ViewButtons[i].GetComponent<Image>().color = i == selectedViewMenuIndex ? selectedColor : normalColor;
                }
            }
            else if (AssistanceMenu.activeSelf)
            {
                for (int i = 0; i < AssistanceButtons.Length; i++)
                {
                    AssistanceButtons[i].GetComponent<Image>().color = i == selectedAssistanceMenuIndex ? selectedColor : normalColor;
                }
            }
    }

    public void CarTextLeft()
    {
        selectedCarIndex = (selectedCarIndex - 1) % carPrefabs.Length;
        if (selectedCarIndex < 0) selectedCarIndex += carPrefabs.Length;
    }

    public void CarTextRight()
    {
        selectedCarIndex = (selectedCarIndex + 1) % carPrefabs.Length;
        if (selectedCarIndex >= carPrefabs.Length) selectedCarIndex = 0;
    }

    public void ModeTextLeft()
    {
        selectedModeIndex = (selectedModeIndex - 1) % modeNames.Length;
        if (selectedModeIndex < 0) selectedModeIndex += modeNames.Length;
    }

    public void ModeTextRight()
    {
        selectedModeIndex = (selectedModeIndex + 1) % modeNames.Length;
        if (selectedModeIndex >= modeNames.Length) selectedModeIndex = 0;
    }

    public void TimeTextLeft()
    {
        selectedTimeIndex = (selectedTimeIndex - 1) % timeNames.Length;
        if (selectedTimeIndex < 0) selectedTimeIndex += timeNames.Length;
    }

    public void TimeTextRight()
    {
        selectedTimeIndex = (selectedTimeIndex + 1) % timeNames.Length;
        if (selectedTimeIndex >= timeNames.Length) selectedTimeIndex = 0;
    }

    public void ViewTextLeft()
    {
        selectedViewIndex = (selectedViewIndex - 1) % ViewNames.Length;
        if (selectedViewIndex < 0) selectedViewIndex += ViewNames.Length;
    }

    public void ViewTextRight()
    {
        selectedViewIndex = (selectedViewIndex + 1) % ViewNames.Length;
        if (selectedViewIndex >= ViewNames.Length) selectedViewIndex = 0;
    }

    public void AssistanceTextLeft()
    {
        selectedAssistanceIndex = (selectedAssistanceIndex - 1) % AssistanceNames.Length;
        if (selectedAssistanceIndex < 0) selectedAssistanceIndex += AssistanceNames.Length;
    }

    public void AssistanceTextRight()
    {
        selectedAssistanceIndex = (selectedAssistanceIndex + 1) % AssistanceNames.Length;
        if (selectedAssistanceIndex >= AssistanceNames.Length) selectedAssistanceIndex = 0;
    }

    public void OnCarSelectButtonPressed()
    {
        carSelected = true;
        UpdatePlayButtonState();
    }

    public void OnModeSelectButtonPressed()
    {
        modeSelected = true;
        UpdatePlayButtonState();
    }

    public void OnTimeSelectButtonPressed()
    {
        if (selectedTimeIndex == 0)
        {
            isDayTime = true;

        }
        else
        {
            isDayTime = false;
        }   
        OnTimeSelectButtonPressed(isDayTime);
    }

    public void OnViewSelectButtonPressed()
    {
        if (selectedViewIndex == 0)
        {
            // IsVRMode = true;
            IsVRMode = true;
        }
        else
        {
            IsVRMode = false;
        }
        OnViewSelectButtonPressed(IsVRMode);
    }

    public void OnAssistanceSelectButtonPressed()
    {
        assistanceSelected = true;
        UpdatePlayButtonState();

        if (selectedAssistanceIndex == 1)
        {
            DeactivateNonSelectedAssistance();
        }
    }

    public void OnViewSelectButtonPressed(bool isVR)
    {
        // Set the VR mode based on the button pressed
        IsVRMode = isVR;
        viewSelected = true;
        UpdatePlayButtonState();
    }

    public void UpdatePlayButtonState()
    {
        // Play button is active if either day or night is selected
        mainMenuButtons[5].gameObject.SetActive(carSelected && modeSelected && timeSelected && viewSelected && assistanceSelected);
    }

    public void OnTimeSelectButtonPressed(bool isDay)
    {
        isDayTime = isDay; // Update based on the button pressed. You might need to adjust how you call this method based on your UI setup.
        timeSelected = true;
        UpdatePlayButtonState();
        ApplyTimeOfDaySettings(); 
    }

    public void OnPlayButtonPressed()
    {
        // Load the selected scene based on selectedModeIndex
        string sceneToLoad = GetSceneNameByIndex(selectedModeIndex);
        SceneManager.LoadScene(sceneToLoad, LoadSceneMode.Single); 
        SceneManager.sceneLoaded += OnSceneLoaded;
    }

    private string GetSceneNameByIndex(int index)
    {
        // Map selectedModeIndex to scene names. Adjust the indices and names as per your setup.
        switch (index)
        {
            case 0: return "Paderborn";
            case 1: return "Training";
            case 2: return "Highway";
            default: return "MainMenu"; // Default case to handle unexpected index
        }
    }

    private void ApplyTimeOfDaySettings()
    {
        if (isDayTime)
        {
            RenderSettings.skybox = daySkybox;
            GameObject lightGameObject = GameObject.FindWithTag("Light");
            if (lightGameObject != null) 
            {
                Light directionalLight = lightGameObject.GetComponent<Light>();
                if (directionalLight != null) {
                    directionalLight.color = dayLightColor;
                } 
            }
        }
        else
        {
            RenderSettings.skybox = nightSkybox;
            GameObject lightGameObject = GameObject.FindWithTag("Light");
            if (lightGameObject != null) 
            {
                Light directionalLight = lightGameObject.GetComponent<Light>();
                if (directionalLight != null) {
                    directionalLight.color = nightLightColor;
                } 
            }
        }

        // Update the environment immediately
        DynamicGI.UpdateEnvironment();
    }

    private void OnSceneLoaded(Scene scene, LoadSceneMode mode)
    {
        // Now that the scene is loaded, activate the selected car prefab
        DeactivateNonSelectedCars();
        ApplyTimeOfDaySettings();

        DeactivateNonSelectedAssistance();

        // Important: Unsubscribe to ensure this method doesn't get called on every scene load
        SceneManager.sceneLoaded -= OnSceneLoaded;
    }

    private void DeactivateNonSelectedCars()
    {
        // List of all possible car tags
        List<string> carTags = new List<string> { "BMW", "FIAT", "VW" };

        // Log the selected car tag

        // Remove the selected car tag from the list so we don't deactivate the selected car
        carTags.Remove(selectedCarTag);

        // Get the root objects in the current scene
        Scene currentScene = SceneManager.GetActiveScene();
        GameObject[] rootObjects = currentScene.GetRootGameObjects();

        // Deactivate cars with tags that are not selected
        foreach (string tag in carTags)
        {
            foreach (GameObject rootObject in rootObjects)
            {
                List<GameObject> carsToDeactivate = FindChildrenWithTag(rootObject, tag);
                foreach (GameObject carToDeactivate in carsToDeactivate)
                {
                    if (carToDeactivate != null)
                    {
                        carToDeactivate.SetActive(false);
                    }
                }
            }
        }

        // Log the active status of all cars
        foreach (string tag in new List<string> { "BMW", "FIAT", "VW" })
        {
            foreach (GameObject rootObject in rootObjects)
            {
                List<GameObject> cars = FindChildrenWithTag(rootObject, tag);
            }
        }
    }

    private List<GameObject> FindChildrenWithTag(GameObject parent, string tag)
    {
        List<GameObject> taggedObjects = new List<GameObject>();

        if (parent.CompareTag(tag))
        {
            taggedObjects.Add(parent);
        }

        foreach (Transform child in parent.transform)
        {
            taggedObjects.AddRange(FindChildrenWithTag(child.gameObject, tag));
        }

        return taggedObjects;
    }

    private void DeactivateNonSelectedAssistance()
    {
        if (selectedAssistanceIndex == 0)
        {
            return;
        }
        
        List<string> carTags = new List<string> { "BMW", "FIAT", "VW" };

        // Get the root objects in the current scene
        Scene currentScene = SceneManager.GetActiveScene();
        GameObject[] rootObjects = currentScene.GetRootGameObjects();

        foreach (string tag in carTags)
        {
            foreach (GameObject rootObject in rootObjects)
            {
                List<GameObject> carsToDeactivate = FindChildrenWithTag(rootObject, tag);
                foreach (GameObject car in carsToDeactivate)
                {
                    DeactivateComponents(car);
                }
            }
        }
    }

    private void DeactivateComponents(GameObject car)
    {
        // Deactivate DrivingAssistanceSystem component
        var drivingAssistanceSystem = car.GetComponent<DrivingAssistanceSystem>();
        if (drivingAssistanceSystem != null)
        {
            drivingAssistanceSystem.enabled = false;
        }

        // Deactivate DrivingAssistanceSystemPB component
        var drivingAssistanceSystemPB = car.GetComponent<DrivingAssistanceSystemPB>();
        if (drivingAssistanceSystemPB != null)
        {
            drivingAssistanceSystemPB.enabled = false;
        }

        // Deactivate DrivingAssistanceSystemHighway component
        var drivingAssistanceSystemHighway = car.GetComponent<DrivingAssistanceSystemHighway>();
        if (drivingAssistanceSystemHighway != null)
        {
            drivingAssistanceSystemHighway.enabled = false;
        }
    }

    private string GetCarTagByIndex(int index)
    {
        switch (index)
        {
            case 0: return "BMW";
            case 1: return "FIAT";
            case 2: return "VW";
            default: return "BMW"; 
        }
    }
}