// using UnityEngine;

// public class GameLoader : MonoBehaviour
// {
//     public static GameLoader Instance { get; private set; }

//     public string SelectedCarTag { get; set; }
//     public int SelectedModeIndex { get; set; }
//     public bool IsVRMode { get; set; }
//     public bool IsDayTime { get; set; }
//     public bool AssistanceSelected { get; set; }

//     private void Awake()
//     {
//         if (Instance == null)
//         {
//             Instance = this;
//             DontDestroyOnLoad(gameObject);
//             Debug.Log("GameLoader instance created.");
//         }
//         else
//         {
//             Destroy(gameObject);
//             Debug.LogWarning("Duplicate GameLoader instance destroyed.");
//         }
//     }
// }