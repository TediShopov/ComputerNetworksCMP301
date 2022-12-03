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
        if (FrameLimiter.Instance.FramesInPlay % DisplayEachXFrame == 0)
        {
            lastFrame = FrameLimiter.Instance.FramesInPlay;
        }
        textContainter.text = FrameLimiter.Instance.FramesInPlay.ToString() + " On Frame";

    }
}