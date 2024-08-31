// using UnityEngine;
// using UnityEditor;

// public class ComponentCopier : EditorWindow
// {
//     GameObject sourceObject;
//     GameObject targetObject;

//     [MenuItem("Window/Component Copier")]
//     public static void ShowWindow()
//     {
//         GetWindow<ComponentCopier>("Component Copier");
//     }

//     private void OnGUI()
//     {
//         sourceObject = (GameObject)EditorGUILayout.ObjectField("Source Object", sourceObject, typeof(GameObject), true);
//         targetObject = (GameObject)EditorGUILayout.ObjectField("Target Object", targetObject, typeof(GameObject), true);

//         if (GUILayout.Button("Copy Components"))
//         {
//             if (sourceObject && targetObject)
//             {
//                 CopyComponentsRecursive(sourceObject, targetObject);
//             }
//         }
//     }

//     private void CopyComponentsRecursive(GameObject source, GameObject target)
//     {
//         // Copy components of the current GameObject
//         foreach (var sourceComponent in source.GetComponents<Component>())
//         {
//             UnityEditorInternal.ComponentUtility.CopyComponent(sourceComponent);
//             UnityEditorInternal.ComponentUtility.PasteComponentAsNew(target);
//         }

//         // Recursively copy components of all child GameObjects
//         foreach (Transform sourceChild in source.transform)
//         {
//             var targetChild = target.transform.Find(sourceChild.name);
//             if (targetChild != null)
//             {
//                 CopyComponentsRecursive(sourceChild.gameObject, targetChild.gameObject);
//             }
//         }
//     }
// }
