using UnityEngine;
using UnityEngine.UI;

public class FPSCounter : MonoBehaviour
{
    public int lastFrame;
    public int DisplayEachXFrame;
    TMPro.TMP_Text textContainter;
    private void Start()
    {
        textContainter= GetComponent<TMPro.TMP_Text>();
    }
    public void Update()
    {
        if (FrameLimiter.FramesInPlay % DisplayEachXFrame == 0)
        {
            lastFrame = FrameLimiter.FramesInPlay;
        }
        textContainter.text = FrameLimiter.FramesInPlay.ToString() + " On Frame";

    }
}