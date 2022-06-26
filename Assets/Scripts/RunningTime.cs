using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;

public class RunningTime : MonoBehaviour
{
    public float _Sec;
    public int _Min;
    public Text _TimerText;

    // Start is called before the first frame update
    void Start()
    {
    }

    // Update is called once per frame
    void Update()
    {
        Timer();
    }

    void Timer() {
        _Sec += Time.deltaTime;

        _TimerText.text = string.Format("시간 : " + "{0:D2}:{1:D2}", _Min, (int)_Sec);

        if ((int)_Sec > 59) {
            _Sec = 0;
            _Min++;
        }
    }
}

