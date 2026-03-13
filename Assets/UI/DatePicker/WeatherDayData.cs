using System;
using UnityEngine;

namespace UI.Dates
{
    [Serializable]
    public class WeatherDayEntry
    {
        public string Date;               // "yyyy-MM-dd"
        public string TemperatureLabel;   // e.g. "14°C"
        public Sprite WeatherIcon;

        // Detail panel fields
        public string WeatherDescription; // e.g. "Partly cloudy"
        public string HighTemp;           // e.g. "16°C"
        public string LowTemp;            // e.g. "9°C"
        public string Humidity;           // e.g. "62%"
        public string WindSpeed;          // e.g. "18 km/h"
        public string Precipitation;      // e.g. "10%"
    }

    [CreateAssetMenu(fileName = "WeatherWeekData", menuName = "Weather/Week Data")]
    public class WeatherDayData : ScriptableObject
    {
        public WeatherDayEntry[] Days = new WeatherDayEntry[7];

        public WeatherDayEntry GetEntry(DateTime date)
        {
            string key = date.ToString("yyyy-MM-dd");
            foreach (var d in Days)
                if (d != null && d.Date == key) return d;
            return null;
        }
    }
}
