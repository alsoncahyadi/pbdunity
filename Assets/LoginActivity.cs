using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;
using UnityEngine.Networking;
using UnityEngine.SceneManagement;

public class LoginActivity : MonoBehaviour {
	public GameObject inputUsername;
	public GameObject inputPassword;
    public Text textMessage;
	private string username;
	private string password;
	public User user;

	// Use this for initialization
	void Start () {
		this.inputUsername.SetActive (true);
	}

	// Update is called once per frame
	void Update () {
		// Kalo Tab bisa pindah field
		if (Input.GetKeyDown(KeyCode.Tab)) {
			if (inputUsername.GetComponent<InputField>().isFocused) { 
				inputPassword.GetComponent<InputField>().Select();
			}
		}

		//Kalo tekan Enter langsung cek firebase
		if (Input.GetKeyDown(KeyCode.Return)) {
			StartCoroutine(GetText());
		}

		// ambil value dari field
		username = inputUsername.GetComponent<InputField> ().text;
		password = inputPassword.GetComponent<InputField> ().text;
	}

	IEnumerator GetText(){
		UnityWebRequest www = UnityWebRequest.Get ("https://fir-auth-af652.firebaseio.com/loginunity/" + username + ".json");
		yield return www.Send ();

		// kalo username salah, json nya null
		if (www.downloadHandler.text == "null") {
			Debug.Log ("Username salah");
            textMessage.color = Color.red;
            textMessage.text = "Username salah";
        } else {
			// dibuat jadi user
			user = JsonUtility.FromJson<User>(www.downloadHandler.text);
			if (password 
                == user.password) {
				Debug.Log ("Login Berhasil");
                textMessage.color = Color.green;
                textMessage.text = "Login Berhasil";
                // simpan username di playerprefs
				PlayerPrefs.SetString("UID", user.UID);
                PlayerPrefs.Save();
                SceneManager.LoadScene("dashboard");
			} else {
				Debug.Log ("Login Gagal. Silakan coba lagi");
                textMessage.color = Color.red;
                textMessage.text = "Login Gagal, password tidak cocok dengan username";
            }
        }

	}
}