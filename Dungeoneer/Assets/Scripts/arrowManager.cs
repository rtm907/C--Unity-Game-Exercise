using UnityEngine;
using System.Collections;
using System.Collections.Generic;

public class arrowManager : MonoBehaviour {
	
	public GameObject ArrowContainer;
	
	private OTObject _shooter;
	private OTSprite arrow;
	private Vector2 _velocity;
	private float _range;
	private float _distanceTravelled = 0;
	private float _damage;
	private OTObject _target;
	//private OTAnimatingSprite owner;
	
	private bool _AOEonImpact = false;
	
	private List<Effect> _effects;
	
	public void SetParameters(OTObject shooter, Vector2 velocity, float range, float damage)
	{
		if (arrow == null) 
		{
			arrow = GetComponent<OTSprite>();
			arrow.onCollision = OnCollision;
		}
		_shooter = shooter;
		arrow.position = _shooter.position;
		_velocity = velocity;
		_range = range;
		_damage = damage;
		
		arrow.transform.parent = ArrowContainer.transform;
	}
	
	public void SetParameters(OTObject shooter, Vector2 velocity, float range, 
		float damage, bool AOEonImpact)
	{
		_AOEonImpact = AOEonImpact;
		SetParameters(shooter, velocity, range, damage);
	}
	
	public void SetParameters(OTObject shooter, Vector2 velocity, float range, 
		float damage, OTObject target, List<Effect> effects)
	{
		_target = target;
		_effects = effects;
		SetParameters(shooter, velocity, range, damage);
	}
	
	// Use this for initialization
	void Start () {
		if (arrow == null)
		{
			arrow = GetComponent<OTSprite>();
			arrow.onCollision = OnCollision;
		}
	}
	
	// Update is called once per frame
	void Update () {
		
		// homing projectile
		if(_target != null)
		{
			Vector2 dir = _target.position - arrow.position;
			dir.Normalize();
			arrow.rotation = CreatureManager.FindAngleFacing(dir);
			arrow.position += dir*_velocity.magnitude;
		}
		// fly straight
		else{
			arrow.position += _velocity;
			_distanceTravelled += _velocity.magnitude;
		}
			
		if(_distanceTravelled > _range)
		{
			OTObject.Destroy(arrow);	
		}
	}
	
	private void OnCollision(OTObject owner)
	{
		OTObject target = owner.collisionObject;
		
		// deliver AOE check
		if(_AOEonImpact && (target.baseName == "wall" || 
			target.baseName == "monster"))
		{
			GameObject AOE = OT.CreateObject("AOE");
			AOE.GetComponent<AOEManager>().SetParemeters(_shooter, _damage);
			AOE.GetComponent<OTAnimatingSprite>().position = arrow.position;
			OTObject.Destroy(arrow);
		}
		
		// homing arrow check
		if(_target != null)
		{
			// hit
			if (target == _target)
			{
				target.GetComponent<MonsterStatManager>().RegisterDamage(_damage);
			
				if(_effects != null)
				{
					target.GetComponent<MonsterStatManager>().AddEffects(_effects);	
				}
				
				// relegate XP duties to MonsterManager
				/*
				if(target.GetComponent<MonsterStatManager>().HP <0)
				{
					_shooter.GetComponent<HeroManager>().GainXP(200);		
				}*/
				
				OT.Destroy(arrow);
			}
			// ignore collisions; continue toward target
			else
			{
				return;
			}
		}
		
		// hero hits monster
		if(target is OTSprite && 
			target.baseName == "monster"
			&& target != _shooter
			&& _shooter.baseName == "Hero")
		{			
			target.GetComponent<MonsterStatManager>().RegisterDamage(_damage);
			
			// move XP award duties
			/*
			if(target.GetComponent<MonsterStatManager>().HP <0)
			{
				//CreatureManager.CreatureFallAnim(target as OTAnimatingSprite);
				_shooter.GetComponent<HeroManager>().GainXP(200);		
			}
			*/
		}
		// monster hits hero
		else if(target.baseName == "Hero"
			&& target != _shooter
			&& _shooter.baseName == "monster")
		{
			target.GetComponent<HeroManager>().RegisterDamage(_damage);
		}
		// projectile hits wall
		else if(target.baseName == "wall")
		{
			OTObject.Destroy(arrow);
		}
		else if(target.baseName == "destructible")
		{
			target.GetComponent<destructibleManager>().Smash(_velocity);
			OTObject.Destroy(arrow);	
		}
		
		/*
		else
		{
			OTObject.Destroy(arrow);	
		}
		*/
		
	}
}
