using UnityEngine;
using System.Collections;
using System.Collections.Generic;

public class MonsterStatManager : MonoBehaviour {
	
	public GameObject ParticlesContainer;
	
	private float _speed;
	private float _speedActive;
	private int _currentHP;
	private int _maxHP;
	private float _attack;
	private float _attackRange;
	
	private List<Effect> _effects = new List<Effect>();
	
	// Use this for initialization
	void Start () {
		//_speed = CONSTANTS.MonsterBaseSpeed;
		//_maxHP = CONSTANTS.MonsterBaseMaxHP;
		//_currentHP = _maxHP;
		//_attack = CONSTANTS.MonsterBaseAttack;
		//_attackRange = CONSTANTS.MonsterAttackRangeMelee;
	}
	
	// update active effects
	void Update()
	{
		List<Effect> toRemove = new List<Effect>();
		
		foreach(Effect ef in _effects)
		{
			ef.DecrementDuration(Time.deltaTime);
			if(ef.EfDuration < 0f)
			{
				toRemove.Add(ef);	
			}
		}
		
		foreach(Effect ef in toRemove)
		{
			//print("REMOVING!");
			MopUpEffect(ef);
			_effects.Remove(ef);
		}
		
		ApplyEffects();
	}
	
	private void MopUpEffect(Effect ef)
	{
		switch(ef.EfType)
		{
		case EffectType.Stun:
			_stunned = false;
			break;			
		}
	}
	
	private bool _stunned = false;
	public bool Stunned
	{
		get{return _stunned;}	
	}
	
	
	private void ApplyEffects()
	{
		foreach(Effect ef in _effects)
		{
			ApplyEffect(ef);	
		}
	}
	
	private void ApplyEffect(Effect ef)
	{
		switch(ef.EfType)
		{
		case EffectType.Stun:
			_stunned = true;
			break;			
		}
	}
	
	public void UpdateStats(MonsterType monsterType)
	{
		switch(monsterType)
		{
		case MonsterType.OrcGrunt:	
			_speed = CONSTANTS.MonsterGruntSpeed;
			_speedActive = _speed;
			_maxHP = CONSTANTS.MonsterGruntMaxHP;
			_currentHP = _maxHP;
			_attack = CONSTANTS.MonsterGruntAttack;
			_attackRange = CONSTANTS.MonsterGruntRange;
			break;
			
		case MonsterType.OrcSpear:
			_speed = CONSTANTS.MonsterAxemanSpeed;
			_speedActive = _speed;
			_maxHP = CONSTANTS.MonsterAxemanMaxHP;
			_currentHP = _maxHP;
			_attack = CONSTANTS.MonsterAxemanAttack;
			_attackRange = CONSTANTS.MonsterAxemanRange;
			break;
			
		case MonsterType.Spider1:
			_speed = CONSTANTS.MonsterSpider1Speed;
			_speedActive = _speed;
			_maxHP = CONSTANTS.MonsterSpider1MaxHP;
			_currentHP = _maxHP;
			_attack = CONSTANTS.MonsterSpider1Attack;
			_attackRange = CONSTANTS.MonsterSpider1Range;
			break;
			
		case MonsterType.Ogre:
			_speed = CONSTANTS.MonsterOgreSpeed;
			_speedActive = _speed;
			_maxHP = CONSTANTS.MonsterOgreMaxHP;
			_currentHP = _maxHP;
			_attack = CONSTANTS.MonsterOgreAttack;
			_attackRange = CONSTANTS.MonsterOgreRange;
			break;
			
		case MonsterType.Furbeast:
			_speed = CONSTANTS.MonsterFurbeastSpeed;
			_speedActive = _speed;
			_maxHP = CONSTANTS.MonsterFurbeastMaxHP;
			_currentHP = _maxHP;
			_attack = CONSTANTS.MonsterFurbeastAttack;
			_attackRange = CONSTANTS.MonsterFurbeastRange;
			break;

		case MonsterType.Scorpion:
			_speed = CONSTANTS.MonsterScorpionSpeed;
			_speedActive = _speed;
			_maxHP = CONSTANTS.MonsterScorpionMaxHP;
			_currentHP = _maxHP;
			_attack = CONSTANTS.MonsterScorpionAttack;
			_attackRange = CONSTANTS.MonsterScorpionRange;
			break;
		}
		
		
	}
	
	private void RecalcStats()
	{
		foreach(Effect ef in _effects)
		{
			switch(ef.EfType)
			{
			case EffectType.Slow:
				_speedActive = ef.EfValue*_speed;
				break;
			}
		}
	}
	
	public void AddEffects(List<Effect> effects)
	{
		foreach(Effect ef in effects)
		{
			// prevent effects stack?
			if(! _effects.Contains(ef))
			{
				_effects.Add(ef);
				
				// visual effects?
				switch(ef.EfType)
				{
				case EffectType.Stun:
					GameObject newVisualEffect = OT.CreateObject("visualEffect");
					newVisualEffect.GetComponent<OTSprite>().frameName = "stars";
					newVisualEffect.transform.parent = GetComponent<OTSprite>().transform;
					newVisualEffect.GetComponent<OTSprite>().depth = -3;
					newVisualEffect.GetComponent<OTSprite>().position = new Vector2(0f,0f);
					OTTween tween = new OTTween(newVisualEffect.GetComponent<OTSprite>(),
						ef.EfDuration, OTEasing.Linear);
					tween.TweenAdd("rotation", 360f);
					OT.Destroy(newVisualEffect, ef.EfDuration);
					break;
					
				}
			}
		}
	}
	
	public float Speed
	{
		get {return _speedActive;}	
	}
	
	public int HP
	{
		get {return _currentHP;}	
	}
	
	public float Attack
	{
		get{return _attack;}	
	}
	
	public float Range
	{
		get{return _attackRange;}	
	}
	
	public void RegisterDamage(float damage)
	{
		_currentHP -= (int) damage;	
		
		EmitBloodParticles(Random.Range(3,7));
		
		if(_currentHP <= 0)
		{
			GetComponent<MonsterManager>().InformFeetOfMotionUpdate(false);
			//Fall();	
		}
	}
	
	private void EmitBloodParticles(int count)
	{
		GameObject temp;
		OTSprite particle;
			
		Vector2 dir = GetComponent<OTSprite>().position -
			GetComponent<MonsterManager>().Hero.position ;
		dir.Normalize();
		dir *=70f;
		
		for(int i=0; i< count; ++i)
		{
			temp = OT.CreateObject("particle");
			particle = temp.GetComponent<OTSprite>();
			particle.frameName = "BloodParticle";
			
			particle.transform.parent = ParticlesContainer.transform;
			
			particle.collidable = false;
			
			particle.position = GetComponent<OTSprite>().position;
			Vector2 dirTemp = CreatureManager.RotateDeg(dir, Random.Range(-30f, 30f));
			particle.rigidbody.AddForce(dirTemp.x, dirTemp.y, 0, ForceMode.Impulse);
			
			OTTween tween = new OTTween(particle, 1f, OTEasing.Linear);
			tween.TweenAdd("rotation", Random.Range(100f, 1000f));
	
			OT.Destroy(particle, 1f);	
		}
	}
	
	/*
	// clean up on monster fall
	private void Fall()
	{
		//GetComponent<MonsterManager>().Fall();
	}
	*/
}
