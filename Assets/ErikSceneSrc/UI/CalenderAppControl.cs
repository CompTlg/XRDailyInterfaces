using System;
using System.Collections;
using TMPro;
using UnityEngine;

public class CalenderAppControl : MonoBehaviour
{
    [SerializeField] private TextMeshProUGUI DateText;
    private float updateIntervalSeconds = 60f;

    // Start is called once before the first execution of Update after the MonoBehaviour is created
    void Start()
    {
        DateText.text = DateTime.Now.ToString("yyyy-MM-dd HH:mm:ss");
    }

    // Update is called once per frame
    void Update()
    {
        DateText.text = DateTime.Now.ToString("yyyy-MM-dd HH:mm:ss");
    }
}
