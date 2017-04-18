using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.Networking;
using UnityEngine.UI;

public class textHiUsername : MonoBehaviour {
    public Text hiUsername;
    private UserUnity user;
    private bool isLoaded = false;
    private string uname = "";
	// Use this for initialization
	void Start () {
        uname = "Wait a moment...";
        StartCoroutine(GetText());
    }
	
	// Update is called once per frame
	void Update () {
        if (!isLoaded)
        {
            hiUsername.text = "Hi, " + uname + "!";
        }
    }

    IEnumerator GetText()
    {
        // ini dapetin jsonnya, jadi kalo peluru tinggal ..../user/peluru.json
        // kalo bensin .../user/bensin.json
        UnityWebRequest www = UnityWebRequest.Get("https://fir-auth-af652.firebaseio.com/" + PlayerPrefs.GetString("UID") + ".json");
        //setiap url connection harus pake yiel return ini
        yield return www.Send();

        if (www.isError)
        {
            Debug.Log("ERROR");
        }
        else
        {
            // www.downloadHandler.text -> hasil json nya
            Debug.Log(www.downloadHandler.text);
            bool isLoaded = true;

            // Kalo ini, kalo dapetin json nya 1 user, dia otomatis buat class User yg atributnya nyesuai in sm isi json nya
            user = JsonUtility.FromJson<UserUnity>(www.downloadHandler.text);
            uname = user.name;
        }
    }
}
