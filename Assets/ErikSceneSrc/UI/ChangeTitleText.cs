using TMPro;
using UnityEngine;

public class ChangeTitleText : MonoBehaviour
{
    public TextMeshProUGUI titleText;

    public void ChangeText()
    {
        titleText.text = "Neuer Titel!";
    }
}