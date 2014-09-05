using UnityEngine;
using System.Collections;

public class LoadingScreenManager : MonoBehaviour {
	
	private float _loadingProgress = 0f;
	private string _displayText;
	
	// Use this for initialization
	void Start () {
		Application.LoadLevelAsync("Main Scene");
	}
	
	// Update is called once per frame
	void Update () {
		print("active...");
		
		if (Application.GetStreamProgressForLevel("Main Scene") == 1)
		{
            _displayText = "LOADED!";
		}
        else {
            _loadingProgress = Application.GetStreamProgressForLevel(1) * 100;
            _displayText = "LOADING... " + _loadingProgress.ToString();
        }
	}
	

	private void OnGUI() {
		
		GUI.Label(new Rect(Screen.width/2 - 100, Screen.height, 200, 30), _displayText);
	}
}
