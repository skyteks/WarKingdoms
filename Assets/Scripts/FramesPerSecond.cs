using UnityEngine;
using UnityEngine.UI;

public class FramesPerSecond : MonoBehaviour
{
    public float updateInterval = 0.1f;

    private float accum = 0.0f; // FPS accumulated over the interval
    private int frames = 0; // Frames drawn over the interval
    private float timeleft; // Left time for current interval

    public Text textDisplay;

    void Start()
    {
        timeleft = updateInterval;
    }

    void Update()
    {
        timeleft -= Time.deltaTime;
        accum += Time.timeScale / Time.deltaTime;
        frames++;

        // Interval ended - update GUI text and start new interval
        if (timeleft <= 0.0f)
        {
            // display two fractional digits (f2 format)
            textDisplay.text = string.Concat("FPS: ", (accum / frames).ToString("###"));
            timeleft = updateInterval;
            accum = 0.0f;
            frames = 0;
        }
    }

    void OnValidate()
    {
        textDisplay.text = "FPS: N/A";
    }
}
