using UnityEngine;
using TMPro;

public class UI : MonoBehaviour
{
    public TextMeshProUGUI jumpCount;
    public TextMeshProUGUI infoText;

    public static UI instance;

    private void Awake() { instance = this; }

    public void UpdateJumpCount(int count)
    {
        jumpCount.text = "Jump Count: " + count;
    }

    public void SetInfoText(string text)
    {
        infoText.text = text;
    }
}
