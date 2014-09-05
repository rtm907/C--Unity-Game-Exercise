using UnityEngine;
using System.Collections;

public class CharacterMenu : MonoBehaviour {
	
	public Texture Footie;
	public Texture Archer;
	public Texture Mage;
	//public HeroData heroData;
	
	private int _charCode=0;
	private string _textAreaString = "CHARACTER NAME";
	
	private void OnGUI() {
		Buttons();
	}
	
	private void Buttons()
	{
		GUI.Label(new Rect(0,0, Screen.width, 20), "CHOOSE YOUR CLASS!");
		
		if(GUI.Button(new Rect(0,20,300,450), Footie))
		{
			_charCode = 0;
			//heroData.CharacterClass = 0;
		}
		if(GUI.Button(new Rect(Screen.width/2 -150,20,300,450), Archer))
		{
			_charCode = 1;
			//heroData.CharacterClass = 1;
		}
		if(GUI.Button(new Rect(Screen.width-300,20,300,450), Mage))
		{
			_charCode = 2;
			//heroData.CharacterClass = 2;
		}
		
		_textAreaString = GUI.TextArea (new Rect (Screen.width/2-100, Screen.height-20, 200, 20), _textAreaString);
		//heroData.CharacterName = _textAreaString;
		
		if(GUI.Button(new Rect(Screen.width-100,Screen.height-20,100,20), "BEGIN GAME"))
		{
			System.IO.StreamWriter file = new System.IO.StreamWriter("HeroData.txt");
			file.WriteLine(_charCode.ToString());
			file.WriteLine(_textAreaString);
			file.Close();
			
			Application.LoadLevel("Main Scene");
		}
		
		GUI.Label(new Rect(0,Screen.height-20, 400, 20), "CLASS: " + _charCode + "; NAME: " + _textAreaString);
	}
}
