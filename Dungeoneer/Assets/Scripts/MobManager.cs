using UnityEngine;
using System.Collections;
using System.Collections.Generic;

public class MobManager : MonoBehaviour {
	
	private List<OTSprite> _mob = new List<OTSprite>();
	
	public OTAnimatingSprite _hero;
	
	private bool _heroVisible = false;
	private Vector2? _heroLastSeen = null;
	
	/*
	// Use this for initialization
	void Start () {
	}
	*/
	
	// Update is called once per frame
	void Update () {
		// do nothing unless hero active
		if( !_hero.GetComponent<HeroManager>().Active)
		{
			return;
		}
		
		// also do a proximity check to avoid expensive
		// mass raycasting for visibility checking
		if( MobDistanceToHero() > CONSTANTS.MonsterActivationThreshold)
		{
			return;	
		}
		
		// check if mob fails to see hero
		if( !MassVisibilityCheck())
		{
			// if fails to see hero AND has not seen hero recently,
			// do nothing
			if(_heroLastSeen == null)
			{
				foreach(OTSprite goon in _mob)
				{
					OrderStayIdle(goon);	
				}
				return;	
			}
			// if fails to see hero BUT has seen him recently,
			// send goons to last known hero location
			else
			{
				foreach(OTSprite goon in _mob)
				{
					OrderMove(goon, _heroLastSeen.Value);	
					
					// cleanup once a goon reaches target location
					if((goon.position - _heroLastSeen.Value).magnitude < 10f)
					{
						_heroLastSeen = null;	
						break;
					}
				}
				
			}
		}
		
		_heroLastSeen = _hero.position;
		
		// hero visible, goons should approach & engage.
		foreach(OTSprite goon in _mob)
		{
			Vector2 distanceToHero = _hero.position - goon.position;
			if(distanceToHero.magnitude < goon.GetComponent<MonsterStatManager>().Range
				&& goon.GetComponent<MonsterManager>().SeesHero)
			// proximity check: if hero within range and visible,
			// attack
			{
				OrderAttack(goon, _hero.position);	
			}
			else
			// else, approach hero until he is within range
			// AND becomes visible
			{
				OrderMove(goon, _hero.position);	
			}
		}
		
	}
	
	private void OrderAttack(OTSprite goon, Vector2 targetLocation)
	{
		goon.GetComponent<MonsterManager>().AttackToward(targetLocation);
	}
	
	private void OrderMove(OTSprite goon, Vector2 targetLocation)
	{
		goon.GetComponent<MonsterManager>().MoveToward(targetLocation);
	}
	
	private void OrderStayIdle(OTSprite goon)
	{
		goon.GetComponent<MonsterManager>().StayIdle();	
	}
	
	public void RegisterGoon(OTSprite goon)
	{
		if(! _mob.Contains(goon))
		{
			_mob.Add(goon);	
			goon.GetComponent<MonsterManager>().SetMobManager(this);
		}	
	}
	
	public void DeregisterGoon(OTSprite goon)
	{
		if(_mob.Contains(goon))
		{
			_mob.Remove(goon);
		}	
	}
	
	// checks if at least one member of the mob sees hero
	private bool MassVisibilityCheck()
	{
		bool returnValue = false;
		foreach(OTSprite goon in _mob)
		{
			// run visibility check	
			if(goon.GetComponent<MonsterManager>().PerformHeroVisibilityCheck())
			{
				returnValue = true;	
			}
		}
		
		return returnValue;
	}
	
	// returns the distance between the Hero and the goon nearest to him
	private float MobDistanceToHero()
	{
		float returnValue = 0f;
		
		foreach(OTSprite goon in _mob)
		{
			float d = (goon.position - _hero.position).magnitude;
			
			if(returnValue == 0f || d<returnValue)
			{
				returnValue = d;	
			}
		}
		
		return returnValue;
	}
}
