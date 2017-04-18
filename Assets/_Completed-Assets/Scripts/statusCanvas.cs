using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;

public class statusCanvas : MonoBehaviour {
    public Text jumlahPeluru;
    public Text jumlahBensin;

    // Use this for initialization
    void Start () {
        jumlahBensin.text = "0";
        jumlahPeluru.text = "0";
    }

    // Update is called once per frame
    void Update () {
        int jBensin = (int)PlayerPrefs.GetFloat("cBensin");
        if (jBensin > 0)
        {
            jumlahBensin.text = jBensin.ToString();
        }
        else
        {
            jumlahBensin.text = "0";
        }

        int jPeluru = PlayerPrefs.GetInt("cPeluru");
        if (jPeluru > 0)
        {
            jumlahPeluru.text = jPeluru.ToString();
        }
        else
        {
            jumlahPeluru.text = "0";
        }
    }
}
