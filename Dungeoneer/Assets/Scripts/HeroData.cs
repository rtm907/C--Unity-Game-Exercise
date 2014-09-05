using UnityEngine;
using System.Collections;

public class HeroData : MonoBehaviour {
	
	private string _characterName;
	private int _characterClass;
	

	/*
	// Use this for initialization
	void Start () {
	
	}
	
	// Update is called once per frame
	void Update () {
	
	}
	*/
	
	public int CharacterClass
	{
		get
		{
			return _characterClass;
		}
		set
		{
			_characterClass = value;	
		}
	}
	
	public string CharacterName
	{
		get
		{
			return _characterName;
		}
		set
		{
			_characterName = value;	
		}
	}
}
