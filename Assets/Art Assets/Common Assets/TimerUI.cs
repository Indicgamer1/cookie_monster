using System;
using UnityEngine;
using TMPro;
using System.Collections;
using UnityEngine.UI;
public class TimerUI : MonoBehaviour
{
     
    [SerializeField] private TextMeshProUGUI timerText;
    [SerializeField] private Image clockImage;
    
    private int startTime=59;

    private int currentTime;

    private void Start()
    {
        currentTime = startTime;
        UpdateTimerText();
        StartCoroutine(StartCountDown());
    }

    private IEnumerator StartCountDown()
    {
        while (currentTime > 0)
        {
            yield return new WaitForSeconds(1f);
            currentTime--;
            UpdateTimerText();
        }
        
        TimerTimeEnd();
    }

    private void UpdateTimerText()
    {
        timerText.text = currentTime.ToString("00");
        
        //clockImage.fillAmount = (float)currentTime / (float)startTime;
    }

    private void TimerTimeEnd()
    {
        //
    }
}
