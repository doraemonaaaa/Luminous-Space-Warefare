using TMPro;
using UnityEngine;
using System.Collections;
using System.Collections.Generic;

public class LapUI : MonoBehaviour
{
    public RectTransform lapCanvas;
    public TextMeshProUGUI lapText;

    [SerializeField] protected float showTime = 3f;

    private void Start()
    {
        HideLap();
    }

    public void ShowLap(string lap_text)
    {
        this.gameObject.SetActive(true);
        if(this.gameObject.activeInHierarchy == true)
            StartCoroutine(ShowLapCoroutine(lap_text));
    }

    public void HideLap()
    {
        lapCanvas.gameObject.SetActive(false);
    }

    private IEnumerator ShowLapCoroutine(string lap_text)
    {
        lapCanvas.gameObject.SetActive(true);
        lapText.text = lap_text;
        yield return new WaitForSeconds(showTime);
        HideLap();
    }

}
