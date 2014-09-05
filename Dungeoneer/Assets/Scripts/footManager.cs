using UnityEngine;
using System.Collections;

public class footManager : MonoBehaviour {

	private OTTween _tween;
	private bool _footForward;
	private bool _moving;
	
	
	// Use this for initialization
	void Start () {
		_tween = new OTTween(GetComponent<OTSprite>(), CONSTANTS.BaseFootStepAplitude, OTEasing.Linear);
		_tween.Tween("position", new Vector2(0, CONSTANTS.BaseFootStep));
		_tween.onTweenFinish = OnTweenFinish;
	}
	
	/*
	// Update is called once per frame
	void Update () {
	
	}
	*/
	
	public void SetLeadingStep(bool forward)
	{
		_footForward = forward;	
	}
	
	public void SetMovingFlag(bool moving)
	{
		_moving = moving;
		
		if(_moving)
		{
			OnTweenFinish(_tween);	
		}
		else
		{
			_tween.onTweenFinish = null;
			_tween.Stop();	
		}
	}
	
	private void OnTweenFinish(OTTween tween)
	{
		OTSprite tmp = gameObject.transform.parent.gameObject.GetComponent<OTSprite>();
		if(tmp != null)
		{
			if(tmp.baseName == "monster")
			{
				if(tmp.GetComponent<MonsterStatManager>().HP< 0)
				{
					// DO NOTHING!
					return;
				}
			}
		}
		
		if(_moving)
		{
			float mp = (_footForward)? 1f : -1f;
			_tween = new OTTween(GetComponent<OTSprite>(), CONSTANTS.BaseFootStepAplitude, OTEasing.Linear);
			_tween.Tween("position", new Vector2(0, mp*CONSTANTS.BaseFootStep));
			_tween.onTweenFinish = OnTweenFinish;
			_footForward = !_footForward;
		}
	}
}
