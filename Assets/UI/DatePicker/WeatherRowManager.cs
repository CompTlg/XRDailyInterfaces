using System;
using UnityEngine;
using UnityEngine.UI;
using TMPro;

namespace UI.Dates
{
    /// <summary>
    /// Adds a single row of temperature labels directly below the week row.
    /// One label per day, no buttons, no icons.
    /// </summary>
    [RequireComponent(typeof(WeekLockedDatePicker))]
    public class WeatherRowManager : MonoBehaviour
    {
        [Header("Data")]
        public WeatherDayData WeekData;

        [Header("Row Style")]
        public float RowHeight          = 32f;
        public Color RowBackgroundColor = new Color(0.15f, 0.15f, 0.2f, 1f);
        public Color TempTextColor      = Color.white;
        public float TempFontSize       = 14f;

        private WeekLockedDatePicker _weekLock;
        private DatePicker           _datePicker;
        private GameObject           _weatherRow;
        private bool                 _needsBuild = false;

        void Awake()
        {
            _weekLock   = GetComponent<WeekLockedDatePicker>();
            _datePicker = GetComponent<DatePicker>();
        }

        void OnEnable()  { _needsBuild = true; }

        void LateUpdate()
        {
            if (_needsBuild)
            {
                _needsBuild = false;
                BuildWeatherRow();
            }
        }

        public void Refresh() { _needsBuild = true; }

        private void BuildWeatherRow()
        {
            if (_datePicker == null || _datePicker.Ref_DayTable == null) return;

            if (_weatherRow != null) Destroy(_weatherRow);

            DateTime  weekStart   = _weekLock.WeekStart;
            Transform tableParent = _datePicker.Ref_DayTable.transform.parent;

            _weatherRow = new GameObject("WeatherTempRow", typeof(RectTransform));
            _weatherRow.transform.SetParent(tableParent, false);
            _weatherRow.transform.SetSiblingIndex(
                _datePicker.Ref_DayTable.transform.GetSiblingIndex() + 1);

            var rowRT       = _weatherRow.GetComponent<RectTransform>();
            rowRT.anchorMin = new Vector2(0, 0);
            rowRT.anchorMax = new Vector2(1, 0);
            rowRT.pivot     = new Vector2(0.5f, 1f);
            rowRT.sizeDelta = new Vector2(0, RowHeight);

            var bg   = _weatherRow.AddComponent<Image>();
            bg.color = RowBackgroundColor;

            var hlg = _weatherRow.AddComponent<HorizontalLayoutGroup>();
            hlg.childControlWidth      = true;
            hlg.childControlHeight     = true;
            hlg.childForceExpandWidth  = true;
            hlg.childForceExpandHeight = true;
            hlg.spacing = 0;
            hlg.padding = new RectOffset(2, 2, 2, 2);

            for (int i = 0; i < 7; i++)
            {
                DateTime day   = weekStart.AddDays(i);
                var      entry = WeekData != null ? WeekData.GetEntry(day) : null;

                var cell  = new GameObject("Temp_" + day.ToString("ddd"), typeof(RectTransform));
                cell.transform.SetParent(_weatherRow.transform, false);

                var tmp       = cell.AddComponent<TextMeshProUGUI>();
                tmp.text      = entry?.TemperatureLabel ?? "—";
                tmp.fontSize  = TempFontSize;
                tmp.color     = TempTextColor;
                tmp.alignment = TextAlignmentOptions.Center;
            }

            LayoutRebuilder.ForceRebuildLayoutImmediate(tableParent as RectTransform);
        }
    }
}
