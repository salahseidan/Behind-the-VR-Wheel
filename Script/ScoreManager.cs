using UnityEngine;
using UnityEngine.SceneManagement;
using System.Collections;
using System.Collections.Generic;
using TMPro;

public class ScoreManager : MonoBehaviour
{
    public int score = 20000;
    public TextMeshProUGUI scoreText;

    void Update()
    {
        if (scoreText != null)
        {
            scoreText.text = "Score : " + score.ToString();
        }
    }

    public void AssignTextComponents(GameObject activeCanvas)
    {
        if (activeCanvas.activeInHierarchy)
        {
            scoreText = FindChildWithTag(activeCanvas.transform, "ScoreText").GetComponent<TextMeshProUGUI>();
        }
    }

    private Transform FindChildWithTag(Transform parent, string tag)
    {
        foreach (Transform child in parent)
        {
            if (child.CompareTag(tag))
            {
                return child;
            }
            Transform result = FindChildWithTag(child, tag);
            if (result != null)
            {
                return result;
            }
        }
        return null;
    }

    // Method to decrease score based on the type of fault
    public void DecreaseScore(string faultType)
    {
        switch (faultType)
        {
            case "redLight":
                score -= 200; 
                break;
            case "collision with pedestrians":
                score -= 500; 
                break;
            case "collision with cars":
                score -= 350; 
                break;
            case "collision with cones":
                score -= 500; 
                break;
            case "noBlinker":
                score -= 50; 
                break;
            case "speeding":
                score -= 100; 
                break;
            case "stalling":
                score -= 300; 
                break;
            default:
                break;
        }
    }
}