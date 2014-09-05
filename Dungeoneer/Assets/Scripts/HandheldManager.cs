using UnityEngine;
using System.Collections;

public enum HandheldItemState
{
	Idle = 0,
	Twirl,
	Stab,
	Slash,
	Recoil,
	Block,
	Parry,
	StrikeTarget,
	LENGTH
}

public class HandheldManager : MonoBehaviour {
	
	private OTTween _tween;
	private HandheldItemState _state = HandheldItemState.Idle;
	public HandheldItemState CurrentState
	{
		get{return _state;}	
	}
	private float _timer = 0f;
	
	
	// Use this for initialization
	void Start () {
		_tween = new OTTween(GetComponent<OTSprite>(), CONSTANTS.BaseAttackAnimDuration, OTEasing.Linear);
		//_tween.onTweenFinish = OnTweenFinish;
	}
	
	// Update is called once per frame
	void Update () {
	
	}
	
	public bool Animating()
	{
		return _tween.isRunning;	
	}
	
	public void Stop()
	{
		_tween.Stop();
		_state = HandheldItemState.Idle;
	}
	
	public void Twirl(float duration)
	{
		if(!_tween.isRunning)
		{
			_timer = duration;
			_state = HandheldItemState.Twirl;
			_tween = new OTTween(GetComponent<OTSprite>(), duration, OTEasing.Linear);
			_tween.TweenAdd("rotation", 360f);
			_tween.onTweenFinish = OnTweenFinish;
		}
	}
	
	public void Stab(float duration)
	{		
		if(!_tween.isRunning)
		{
			_timer = duration;
			_state = HandheldItemState.Stab;
			_tween = new OTTween(GetComponent<OTSprite>(), duration/2f);
			_tween.TweenAdd("position", new Vector2(0,0.3f),
				OTEasing.BounceIn);
			_tween.onTweenFinish = OnTweenFinish;
		}
	}
	
	public void Slash(float duration)
	{		
		if(!_tween.isRunning)
		{
			_timer = duration;
			_state = HandheldItemState.Slash;
			_tween = new OTTween(GetComponent<OTSprite>(), duration/2f);
			_tween.TweenAdd("position", new Vector2(-0.6f,0.3f),
				OTEasing.ElasticIn);
			GetComponent<OTSprite>().rotation = 
				GetComponent<OTSprite>().transform.parent.GetComponent<OTObject>().rotation-90f;
			_tween.TweenAdd("rotation", 120f,
				OTEasing.Linear);
			_tween.onTweenFinish = OnTweenFinish;
		}
	}
		
	public void Recoil(float duration)
	{		
		if(!_tween.isRunning)
		{
			_timer = duration;
			_state = HandheldItemState.Recoil;
			_tween = new OTTween(GetComponent<OTSprite>(), duration/2f);
			_tween.TweenAdd("position", new Vector2(0,-0.2f),
				OTEasing.ExpoOut);
			_tween.onTweenFinish = OnTweenFinish;
		}
	}
	
	public void Block()
	{
		if(!_tween.isRunning)
		{
			_state = HandheldItemState.Block;
			_tween = new OTTween(GetComponent<OTSprite>(), CONSTANTS.BaseAttackAnimDuration);
			_tween.TweenAdd("position", new Vector2(0, 0.5f),
				OTEasing.ExpoOut);
			_tween.onTweenFinish = OnTweenFinish;
		}
	}
	
	public void Parry()
	{
		if(!_tween.isRunning)
		{
			_state = HandheldItemState.Parry;
			_tween = new OTTween(GetComponent<OTSprite>(), CONSTANTS.BaseAttackAnimDuration, OTEasing.Linear);
			_tween.TweenAdd("rotation", -90f);
			_tween.onTweenFinish = OnTweenFinish;
		}
	}
	
	public void StrikeTarget(OTObject target, float duration)
	{
		if(!_tween.isRunning)
		{
			_state = HandheldItemState.StrikeTarget;
			_tween = new OTTween(GetComponent<OTSprite>(), duration);
			_tween.TweenAdd("position", target.position - GetComponent<OTSprite>().position,
				OTEasing.BounceIn);
			_tween.onTweenFinish = OnTweenFinish;
		}
	}
	
	private void OnTweenFinish(OTTween tween)
	{
		switch(_state)
		{
		case HandheldItemState.Twirl:
			GetComponent<OTSprite>().rotation = 
				GetComponent<OTSprite>().transform.parent.GetComponent<OTObject>().rotation;
			break;
		case HandheldItemState.Stab:
			_tween = new OTTween(GetComponent<OTSprite>(), _timer/2f);
			_tween.TweenAdd("position", new Vector2(0,-0.3f),
				OTEasing.ExpoOut);
			break;
		case HandheldItemState.Slash:
			GetComponent<OTSprite>().rotation = 
				GetComponent<OTSprite>().transform.parent.GetComponent<OTObject>().rotation;			
			_tween = new OTTween(GetComponent<OTSprite>(), _timer/2f);
			_tween.TweenAdd("position", new Vector2(0.6f,-0.3f),
				OTEasing.ExpoOut);
			break;		
		case HandheldItemState.Recoil:
			_tween = new OTTween(GetComponent<OTSprite>(), _timer/2f);
			_tween.TweenAdd("position", new Vector2(0,0.2f),
				OTEasing.ExpoIn);
			break;
		case HandheldItemState.Block:
			_tween = new OTTween(GetComponent<OTSprite>(), CONSTANTS.BaseAttackAnimDuration);
			_tween.TweenAdd("position", new Vector2(0, -0.5f),
				OTEasing.ExpoIn);
			break;		
		case HandheldItemState.Parry:
			GetComponent<OTSprite>().rotation = 
				GetComponent<OTSprite>().transform.parent.GetComponent<OTObject>().rotation;
			break;
			
		case HandheldItemState.StrikeTarget:
			GetComponent<OTSprite>().position = new Vector2(0.3f, 0.3f);
			// do a reset at hero
			break;
		}
		_state = HandheldItemState.Idle;
	}
}
