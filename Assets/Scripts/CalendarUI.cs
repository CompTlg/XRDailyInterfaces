using UnityEngine;
using TMPro;

public class CalendarUI : MonoBehaviour
{
    public TextMeshProUGUI eventText;

    public void SelectDay(string day)
    {
        eventText.text = "Events for " + day + ":\n\nNo events yet.";
    }
}