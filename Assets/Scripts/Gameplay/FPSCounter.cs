using UnityEngine;
using UnityEngine.UI;

public class FPSCounter : MonoBehaviour
{
    public int avgFrameRate;
    TMPro.TMP_Text textContainter;
    private void Start()
    {
        textContainter= GetComponent<TMPro.TMP_Text>();
    }
    public void Update()
    {
        float current = 0;
        current = (int)(1f / Time.unscaledDeltaTime);
        avgFrameRate = (int)current;
        textContainter.text = avgFrameRate.ToString() + " FPS";
    }
}