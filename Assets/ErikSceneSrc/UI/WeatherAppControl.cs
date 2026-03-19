using System;
using System.Collections;
using TMPro;
using UnityEngine;

public class WeatherAppControl : MonoBehaviour
{
    [SerializeField] private TextMeshProUGUI conditionText;
    [SerializeField] private TextMeshProUGUI tempText;
    [SerializeField] private TextMeshProUGUI updatedText;
    [SerializeField] private float updateIntervalSeconds = 60f;

    private Coroutine _updateRoutine;

    private void Start()
    {
        if (conditionText != null)
        {
            conditionText.gameObject.SetActive(false);
        }

        if (tempText != null)
        {
            tempText.text = "--";
        }

        if (updatedText != null)
        {
            updatedText.text = "--";
        }

        _updateRoutine = StartCoroutine(UpdateWeatherLoop());
    }

    private IEnumerator UpdateWeatherLoop()
    {
        // Initial delay so UI starts in the "no data" state
        yield return new WaitForSeconds(updateIntervalSeconds);

        while (true)
        {
            UpdateWeatherOnce();
            yield return new WaitForSeconds(updateIntervalSeconds);
        }
    }

    private void UpdateWeatherOnce()
    {
        var info = GetCurrentWeatherFromTime();

        if (conditionText != null)
        {
            conditionText.gameObject.SetActive(true);
            conditionText.text = $"{info.Condition}, {info.Temperature:0.#}°C";
        }

        if (tempText != null)
        {
            tempText.text = $"{info.Temperature:0.#}°C";
        }

        if (updatedText != null)
        {
            updatedText.text = DateTime.Now.ToString("yyyy-MM-dd HH:mm:ss");
        }
    }

    private WeatherInfo GetCurrentWeatherFromTime()
    {
        var now = DateTime.Now;

        // Simple deterministic pseudo "API" based on current time.
        int seed = now.Year * 1000 + now.DayOfYear * 10 + now.Hour;
        string condition;
        switch (seed % 4)
        {
            case 0:
                condition = "Sunny";
                break;
            case 1:
                condition = "Rainy";
                break;
            case 2:
                condition = "Snowy";
                break;
            default:
                condition = "Cloudy";
                break;
        }

        // Rough seasonal temperature curve based on month
        float seasonalBase = 10f + 10f * Mathf.Sin((now.Month - 1) / 12f * 2f * Mathf.PI);
        float variation = (seed % 7) - 3; // small deterministic jitter
        float temperature = seasonalBase + variation;

        return new WeatherInfo(condition, temperature);
    }

    private readonly struct WeatherInfo
    {
        public readonly string Condition;
        public readonly float Temperature;

        public WeatherInfo(string condition, float temperature)
        {
            Condition = condition;
            Temperature = temperature;
        }
    }
}