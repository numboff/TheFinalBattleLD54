using UnityEngine;
using TMPro;
using UnityEditor;

public class TimeManager : MonoBehaviour
{
    public float totalDuration;  // Total duration of the game in seconds
    public TextMeshProUGUI timeDisplay;          // Reference to the Text component for displaying remaining time

    private float remainingTime;       // Current remaining time

    private float shakingRange;     // The range within which rocks will shake
    private float previousTick = 0f;        // The speed at which the stone falls down

    private GameObject[] rocks;

    private void Start()
    {
        remainingTime = totalDuration;
        rocks = GameObject.FindGameObjectsWithTag("Floor");

        ActivateShaking();
        shakingRange = Mathf.Round(totalDuration / rocks.Length);
        Debug.Log(shakingRange);
    }

    private void Update()
    {
        if (remainingTime > 0)
        {
            remainingTime -= Time.deltaTime;
            UpdateTimeDisplay();
            if (((Mathf.Round(remainingTime) % shakingRange) == 0) && (Mathf.Round(remainingTime) != previousTick))
            {
                previousTick = Mathf.Round(remainingTime);

                ActivateShaking();
            }
        }
        else
        {
            Debug.Log("Game Over");
        }
    }

    private void UpdateTimeDisplay()
    {
        int minutes = Mathf.FloorToInt(remainingTime / 60);
        int seconds = Mathf.FloorToInt(remainingTime % 60);
        string timeString = string.Format("{0:00}", seconds);
        timeDisplay.text = timeString;
    }

    private void ActivateShaking()
    {
        rocks = GameObject.FindGameObjectsWithTag("Floor");

        if(rocks.Length > 0)
        {
            ShakingStone tmp = rocks[0].GetComponent<ShakingStone>();
            if (!tmp.isShacking) tmp.StartShaking();

            tmp = rocks[rocks.Length - 1].GetComponent<ShakingStone>();
            if (!tmp.isShacking) tmp.StartShaking();
        }
        else
        {
            Debug.Log("No stones left.");
        }
        
    }
}