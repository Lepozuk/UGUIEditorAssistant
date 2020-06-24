using System.Collections;
using System.Collections.Generic;
using UnityEngine;
//using UnityEngine.UI;
using Titan.UI;

public class TestUI : MonoBehaviour
{
    public SliderT slider;
    // Start is called before the first frame update
    void Start()
    {
        Debug.Log(slider.name);
        slider.MyFunc();
    }

    // Update is called once per frame
    void Update()
    {
        
    }
}
