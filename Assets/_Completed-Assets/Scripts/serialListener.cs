using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;
using System.IO.Ports;

public class serialListener : MonoBehaviour {
    public Text text;
    private SerialPort sp = new SerialPort("COM3", 9600);

	// Use this for initialization
	void Start () {
        sp.Open();
        sp.ReadTimeout = 1;
        text.text = "START";
	}
	
	// Update is called once per frame
	void Update () {
        if (sp.IsOpen)
        {
            try
            {
                text.text = sp.ReadByte().ToString();
            }
            catch (System.Exception)
            {

            }
        }
        
	}
}
