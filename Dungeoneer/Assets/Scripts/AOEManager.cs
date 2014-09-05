using UnityEngine;
using System.Collections;

public class AOEManager : MonoBehaviour {

	public GameObject AOEContainer;
	
	private OTObject _shooter;
	private OTAnimatingSprite _AOE;
	private float _damage;
	
	private float _duration = 1f;
	
	public void SetParemeters(OTObject shooter, float damage)
	{
		_AOE = GetComponent<OTAnimatingSprite>();
		_AOE.onCollision = OnCollision;
		_shooter = shooter;
		_damage = damage;
		
		_AOE.transform.parent = AOEContainer.transform;
	}
	
	private bool _playing;
	private bool _played;
	
	// Update is called once per frame
	void Update () {
		_duration -= Time.deltaTime;
		
		if(_duration < 0)
		{
			OT.Destroy(GetComponent<OTAnimatingSprite>());	
		}
		
		if(_playing)
		{
			_played = true;	
		}
	}
	
	private void OnCollision(OTObject owner)
	{
		// already administer damage; return
		if(_played)
		{
			return;	
		}
		
		OTObject target = owner.collisionObject;
		
		if(target is OTSprite && 
			target.baseName == "monster")
		{
			_playing = true;
			target.GetComponent<MonsterStatManager>().RegisterDamage(_damage);	
		}
	}
	
}
