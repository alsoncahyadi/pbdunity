using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.SceneManagement;
using UnityEngine.UI;

public class playGame : MonoBehaviour {
    public Button btnPlayGame;
    public Dropdown dropBGM;

	// Use this for initialization
	void Start () {
        Button btn = btnPlayGame.GetComponent<Button>();
        btn.onClick.AddListener(delegate () { TaskOnClick(); });
        dropBGM.value = PlayerPrefs.GetInt(PlayerPrefs.GetString("username") + "Bgm");
    }

    void TaskOnClick()
    {
        PlayerPrefs.SetInt(PlayerPrefs.GetString("username") + "Bgm", dropBGM.value);
        PlayerPrefs.Save();
        SceneManager.LoadScene("CompleteMainScene");
    }
}
