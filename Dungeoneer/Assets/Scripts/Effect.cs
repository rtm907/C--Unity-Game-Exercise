using UnityEngine;
using System.Collections;
using System.Collections.Generic;

public enum EffectType 
{
	Stun = 0,
	Burn,
	Poison,
	Slow,
	LENGTH
}

/*
public enum EffectDelivery
{
	Passive = 0,
	InstantOnSelf,
	InstantOnTargetUnderCursor,
	Projectile, // aimed at cursor by default
	ProjectileAoEonImpact,
	AoEonSelf,
	AoEonCursor,
	LENGTH
}
*/

public class Effect : ScriptableObject
{
	private EffectType _effectType;
	private float _effectValue;
	//private EffectDelivery _deliveryType;
	private float _effectDuration;
	
	public EffectType EfType
	{
		get
		{
			return _effectType;	
		}
	}
	
	public float EfValue
	{
		get
		{
			return _effectValue;	
		}
	}
	
	/*
	public EffectDelivery EfDelivery
	{
		get
		{
			return _deliveryType;	
		}
	}
	*/
	
	public float EfDuration
	{
		get
		{
			return _effectDuration;	
		}
	}
	
	public void DecrementDuration(float decrement)
	{
		_effectDuration -= decrement;	
	}
	
	public void SetValues(EffectType effectType, float effectValue, float duration)
	{
		_effectType = effectType;
		_effectValue = effectValue;
		//_deliveryType = delivery;
		_effectDuration = duration;
	}
	
	/*
	public override bool Equals(Object obj) 
    {
		return obj is Effect && this == (Effect)obj;
    }
	*/
	
	public static bool operator ==(Effect a, Effect b) 
    {
		return (a.EfType == b.EfType);
	}
	
	public static bool operator !=(Effect a, Effect b) 
    {
		return (a.EfType != b.EfType);
	}
}

/*
public class Skill : ScriptableObject {
	
	private string _name;
	private List<Effect> _skillEffects = new List<Effect>();
	
	
	// Use this for initialization
	void Start () {
		//_skillEffects = new List<Effect>();
	}
	
	public void SetName(string newName)
	{
		_name = newName;	
	}
	
	public string GetName()
	{
		return _name;
	}
	
	public void AddEffect(Effect newEffect)
	{
		_skillEffects.Add(newEffect);
	}
	
	public List<Effect> GetEffects()
	{
		return _skillEffects;	
	}
	

}
*/