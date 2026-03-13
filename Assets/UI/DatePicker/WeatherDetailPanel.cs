using UnityEngine;
using UnityEngine.UI;
using TMPro;

namespace UI.Dates
{
    public class WeatherDetailPanel : MonoBehaviour
    {
        [Header("Labels")]
        public TextMeshProUGUI DateLabel;
        public TextMeshProUGUI DescriptionLabel;
        public TextMeshProUGUI HighTempLabel;
        public TextMeshProUGUI LowTempLabel;
        public TextMeshProUGUI HumidityLabel;
        public TextMeshProUGUI WindLabel;
        public TextMeshProUGUI PrecipLabel;

        [Header("Close")]
        public Button CloseButton;

        void Awake()
        {
            if (CloseButton != null)
                CloseButton.onClick.AddListener(() => gameObject.SetActive(false));
            gameObject.SetActive(false);
        }

        public void Populate(WeatherDayEntry entry, string formattedDate)
        {
            if (DateLabel)        DateLabel.text        = formattedDate;
            if (DescriptionLabel) DescriptionLabel.text = entry?.WeatherDescription ?? "—";
            if (HighTempLabel)    HighTempLabel.text    = "High: " + (entry?.HighTemp    ?? "—");
            if (LowTempLabel)     LowTempLabel.text     = "Low: "  + (entry?.LowTemp     ?? "—");
            if (HumidityLabel)    HumidityLabel.text    = "Humidity: " + (entry?.Humidity ?? "—");
            if (WindLabel)        WindLabel.text        = "Wind: "  + (entry?.WindSpeed   ?? "—");
            if (PrecipLabel)      PrecipLabel.text      = "Precip: " + (entry?.Precipitation ?? "—");
            gameObject.SetActive(true);
        }
    }
}
