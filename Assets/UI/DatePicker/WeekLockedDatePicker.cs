using System;
using System.Collections.Generic;
using UnityEngine;
using UI.Tables;

namespace UI.Dates
{
    /// <summary>
    /// Locks the DatePicker to display ONLY the current week as a single row.
    /// Builds the full month then culls all rows except the target week in LateUpdate.
    /// </summary>
    [RequireComponent(typeof(DatePicker))]
    [ExecuteInEditMode]
    public class WeekLockedDatePicker : MonoBehaviour
    {
        [Tooltip("Leave 0 to auto-use today's week. Set Year/Month/Day to hardcode a week.")]
        public int AnchorYear  = 0;
        public int AnchorMonth = 0;
        public int AnchorDay   = 0;

        public DayOfWeek FirstDayOfWeek = DayOfWeek.Monday;

        private DatePicker _datePicker;
        private bool _needsRowCull = false;

        void Awake()
        {
            _datePicker = GetComponent<DatePicker>();
        }

        void OnEnable()
        {
            _datePicker = GetComponent<DatePicker>();
            ApplyWeekLock();
        }

#if UNITY_EDITOR
        void OnValidate()
        {
            _datePicker = GetComponent<DatePicker>();
            ApplyWeekLock();
        }
#endif

        void LateUpdate()
        {
            if (_needsRowCull)
            {
                _needsRowCull = false;
                CullToSingleWeekRow();
            }
        }

        public void ApplyWeekLock()
        {
            if (_datePicker == null) return;

            DateTime anchor    = GetAnchor();
            int daysBack       = ((int)anchor.DayOfWeek - (int)FirstDayOfWeek + 7) % 7;
            DateTime weekStart = anchor.AddDays(-daysBack).Date;
            DateTime weekEnd   = weekStart.AddDays(6).Date;

            _datePicker.Config.DateRange.RestrictFromDate = true;
            _datePicker.Config.DateRange.FromDate         = new SerializableDate(weekStart);
            _datePicker.Config.DateRange.RestrictToDate   = true;
            _datePicker.Config.DateRange.ToDate           = new SerializableDate(weekEnd);

            _datePicker.Config.Misc.ShowDatesInOtherMonths            = false;
            _datePicker.Config.Header.ShowHeader                      = false;
            _datePicker.Config.Header.ShowNextAndPreviousMonthButtons = false;
            _datePicker.Config.Header.ShowNextAndPreviousYearButtons  = false;

            _datePicker.VisibleDate = new SerializableDate(weekStart);
            _datePicker.UpdateDisplay();

            _needsRowCull = true;
        }

        private void CullToSingleWeekRow()
        {
            if (_datePicker == null || _datePicker.Ref_DayTable == null) return;

            DateTime weekStart = WeekStart;
            DateTime weekEnd   = weekStart.AddDays(6);

            var rows = _datePicker.Ref_DayTable.Rows;

            for (int i = 1; i < rows.Count; i++)
            {
                var buttons = rows[i].GetComponentsInChildren<DatePicker_DayButton>(true);

                bool isTargetRow = false;
                foreach (var btn in buttons)
                {
                    if (btn.Date.Date >= weekStart && btn.Date.Date <= weekEnd)
                    {
                        isTargetRow = true;
                        break;
                    }
                }

                rows[i].gameObject.SetActive(isTargetRow);
            }
        }

        public void SetWeek(DateTime anyDateInTargetWeek)
        {
            AnchorYear  = anyDateInTargetWeek.Year;
            AnchorMonth = anyDateInTargetWeek.Month;
            AnchorDay   = anyDateInTargetWeek.Day;
            ApplyWeekLock();
        }

        private DateTime GetAnchor()
        {
            try
            {
                if (AnchorYear > 0 && AnchorMonth > 0 && AnchorDay > 0)
                    return new DateTime(AnchorYear, AnchorMonth, AnchorDay);
            }
            catch { }
            return DateTime.Today;
        }

        public DateTime WeekStart
        {
            get
            {
                DateTime a = GetAnchor();
                int d = ((int)a.DayOfWeek - (int)FirstDayOfWeek + 7) % 7;
                return a.AddDays(-d).Date;
            }
        }

        public DateTime WeekEnd => WeekStart.AddDays(6);
    }
}
