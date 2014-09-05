using UnityEngine;
using System.Collections;
using System.Collections.Generic;

public enum MonsterType
{
	OrcGrunt = 0,
	OrcSpear,
	Spider1,
	Ogre,
	Furbeast,
	Scorpion,
	LENGTH,
}

public class MonsterManager : MonoBehaviour {
	
	#region field
	
	public GameObject MapDetritusContainer;
	
	public OTAnimatingSprite Hero;
	
	public OTSprite LeftFoot;
	public OTSprite RightFoot;
	public OTSprite Head;
	
	public OTSprite Weapon;
	
	public OTSprite Crosshair;
	
	public ParticleSystem Blood;
	
	private OTSprite _monster;
	
	private MonsterType _monsterType;
	
	private Vector2? _lastKnownHeroLocation;
	
	private MobManager _mobManager;
	public void SetMobManager(MobManager mobManager)
	{
		_mobManager = mobManager;
	}
	
	#endregion
	
	#region flags
		
	private bool _moving = false;
	private bool _dead = false;
	private bool _seesHero = false;
	public bool SeesHero
	{
		get{return _seesHero;}	
	}
	
	#endregion
	
	// weird redundancy; there is some problem with the init
	public void SetMonsterType(MonsterType monsterType)
	{
		_monsterType = monsterType;
	}
	
	// Use this for initialization
	void Start () {
		_monster = GetComponent<OTSprite>();
		
		LeftFoot.GetComponent<footManager>().SetLeadingStep(true);
		RightFoot.GetComponent<footManager>().SetLeadingStep(false);
		
		Weapon.position += new Vector2(0.3f, 0.5f);
		SetSkin();
		
		//Crosshair.visible = false;	
	}
	
	public void InformFeetOfMotionUpdate(bool moving)
	{
		if(LeftFoot.GetComponent<footManager>() != null)
		{
			LeftFoot.GetComponent<footManager>().SetMovingFlag(moving);
		}
		if(RightFoot.GetComponent<footManager>() != null)
		{
			RightFoot.GetComponent<footManager>().SetMovingFlag(moving);
		}
	}
	
	private void SetSkin()
	{		
		_monster.frameName = "body"+_monsterType.ToString();
		Head.frameName = "head"+_monsterType.ToString();
		LeftFoot.frameName = "footleft"+_monsterType.ToString();
		RightFoot.frameName = "footright"+_monsterType.ToString();
		
		GetComponent<MonsterStatManager>().UpdateStats(_monsterType);
	}
	
	// Update is called once per frame
	void Update () {		
		// fix later; can't initialize this any other way
		if(!_underMouse)
		{
			Crosshair.visible = false;	
		}
		
		// dead, do nothing
		if(GetComponent<MonsterStatManager>().HP < 0)
		{	
			if(!_dead)
			{
				Fall ();
			}	
			
			return;	
		}
		
		// remove undesired physics
		_monster.rigidbody.velocity = Vector3.zero;
		_monster.rigidbody.angularVelocity = Vector3.zero;
	}
	
	// returns true if goon incapacitated
	private bool IncapacitationCheck()
	{
		// dead, do nothing
		if(GetComponent<MonsterStatManager>().HP < 0)
		{	
			return true;	
		}
		
		// stunned...
		if(GetComponent<MonsterStatManager>().Stunned)
		{
			return true;	
		}
		
		return false;
	}
	
	private Vector2? _globalTarget = null;
	private Vector2? _currentTarget = null;
	private Stack<Vector2> _route = new Stack<Vector2>();
	private int[,] _dungeon;
	public int[,] Dungeon
	{
		set{_dungeon = value;}	
	}
	
	public void MoveToward(Vector2 targetLocation)
	{
		// check if incapacitated
		if( IncapacitationCheck())
		{
			return;	
		}
		
		// check if target is hero && hero is visible
		if(targetLocation == Hero.position && _seesHero)
		{
			// use only global target
			_globalTarget = targetLocation;
			_route = null;
			_currentTarget = null;
			ContinueWalking();
			return;
		}
		
		// check if already en route
		if(_globalTarget != null && (_globalTarget.Value - targetLocation).magnitude < 5)
		{
			// already moving, just update
			ContinueWalking();
			return;
		}
		
		// no movement target or target changed
		SetupMovementTarget(targetLocation);
		ContinueWalking();
	}
	
	private void SetupMovementTarget(Vector2 targetLocation)
	{
		_globalTarget = targetLocation;
			
		// obtain route
		_route = Pathfinder.FindPath(_dungeon, (int) Mathf.Round(_monster.position.x/CONSTANTS.TileSize),
			(int) Mathf.Round(_monster.position.y/CONSTANTS.TileSize), 
			(int) Mathf.Round(targetLocation.x/CONSTANTS.TileSize),
			(int) Mathf.Round(targetLocation.y/CONSTANTS.TileSize));
		
		if(_route == null)
		{
			//print ("ERROR??!?");
			return;
		}
		
		_currentTarget = _route.Pop();	
	}
	
	private void ContinueWalking()
	{	
		if(_currentTarget == null)
		{
			if(_globalTarget != null)
			{
				Vector2 dir = _globalTarget.Value - _monster.position;
				TakeStep(dir);
				return;
			}
			else
			{
				return;
			}
		}
		
		// check if close enough to target
		Vector2 diff = new Vector2((_currentTarget.Value.x*CONSTANTS.TileSize-_monster.position.x),
			(_currentTarget.Value.y*CONSTANTS.TileSize-_monster.position.y));
		if(diff.magnitude < 5)
		{
			UpdateRoute();
		}
		else
		{
			// if too far, advance...
			TakeStep(diff);
		}
	}
	
	private void TakeStep(Vector2 dir)
	{
		dir.Normalize();
		RotateToFaceDirection(dir);
		_monster.position += GetComponent<MonsterStatManager>().Speed* dir*Time.deltaTime;
		
		if(!_moving)
		{
			_moving = true;
			InformFeetOfMotionUpdate(true);
		}
	}
	
	private void UpdateRoute()
	{
		if(_route != null)
		{
			_currentTarget = _route.Pop();
		}
		else
		{
			_currentTarget = null;	
		}
		
		if(_currentTarget == null)
		{
			_route = null;
			_globalTarget = null;
			_moving = false;
			InformFeetOfMotionUpdate(false);
			return;
		}
	}
	
	private void RotateToFaceDirection(Vector2 dir)
	{
		_monster.rotation = CreatureManager.FindAngleFacing(dir);
	}
	
	public void AttackToward(Vector2 targetLocation)
	{
		// incapacitation check
		if( IncapacitationCheck())
		{
			return;	
		}
		
		_moving = false;
		InformFeetOfMotionUpdate(false);
		
		Vector2 direction = targetLocation - _monster.position;
		direction.Normalize();
		
		// ATTACK
		if(! Weapon.GetComponent<HandheldManager>().Animating())
		{
			RotateToFaceDirection(direction);
			
			Weapon.GetComponent<HandheldManager>().Slash(1f); // FIX attack speed etc...
			
			GameObject newarrow = OT.CreateObject("arrow");
			newarrow.transform.parent = MapDetritusContainer.transform;
			
			switch(_monsterType)
			{
			case MonsterType.OrcGrunt:
			case MonsterType.Ogre:
			case MonsterType.Spider1:
			case MonsterType.Furbeast:
			case MonsterType.Scorpion:
				newarrow.GetComponent<arrowManager>().
					SetParameters(_monster, CONSTANTS.BaseMeleeProjectileSpeed*direction, 
						CONSTANTS.BaseMeleeRange, GetComponent<MonsterStatManager>().Attack);
				newarrow.GetComponent<OTSprite>().visible = false;
				newarrow.GetComponent<OTSprite>().rotation = CreatureManager.FindAngleFacing(direction);
				break;
									
			case MonsterType.OrcSpear:
				newarrow.GetComponent<arrowManager>().
					SetParameters(_monster, CONSTANTS.BaseRangedProjectileSpeed*direction, 
						GetComponent<MonsterStatManager>().Range, GetComponent<MonsterStatManager>().Attack);
				newarrow.GetComponent<OTSprite>().frameName = "arrow";
				newarrow.GetComponent<OTSprite>().visible = true;
				newarrow.GetComponent<OTSprite>().rotation = CreatureManager.FindAngleFacing(direction);
				break;
			}
		}
	}
	
	public bool PerformHeroVisibilityCheck()
	{
		_seesHero = CreatureManager.LOSCheck(_monster, Hero);
		
		return _seesHero;
	}
	
	public void StayIdle()
	{
		// incapacitation check
		if( IncapacitationCheck())
		{
			return;	
		}
		
		_moving = false;
		InformFeetOfMotionUpdate(false);
	}
	
	private void Tint(Color color)
	{
		_monster.tintColor = color;
		Head.tintColor = color;
		LeftFoot.tintColor = color;
		RightFoot.tintColor = color;
	}
	
	private void CatapultAndDestroyMember(OTSprite member, float force)
	{
		member.transform.parent = null;
		member.depth = -1;
		
		//member.collidable = true;
		//member.physics = OTObject.Physics.NoGravity;
		
		//member.depth = -1;
		member.gameObject.AddComponent<Rigidbody>();
		member.rigidbody.useGravity = false;
		
		Vector2 dir = _monster.position - Hero.position;
		dir.Normalize();
		dir = CreatureManager.RotateDeg(dir, Random.Range(-30f,30f));
		
		member.position += 20*dir;
		
		member.rigidbody.AddForce(force * new Vector3(dir.x, dir.y, 0));	
		
		member.transform.parent = _mobManager.gameObject.transform;
		
		OT.Destroy(member, 2f);
	}
	
	private void Dismember(float force)
	{					
		//_monster.rigidbody.useGravity = false;
		Vector2 dir = _monster.position - Hero.position;
		dir.Normalize();
		//dir = CreatureManager.RotateDeg(dir, Random.Range(-30f,30f));
		_monster.rigidbody.AddForce(force * new Vector3(dir.x, dir.y, 0));
		OT.Destroy(_monster, 2f);
		
		CatapultAndDestroyMember(Head, force);
		CatapultAndDestroyMember(LeftFoot, force);
		CatapultAndDestroyMember(RightFoot, force);
	}
	
	private void SpawnBloodSpotch()
	{
		GameObject temp = OT.CreateObject("mapDetail");
		OTSprite sprite = temp.GetComponent<OTSprite>();
		sprite.frameName = "MapDetailBlood";
		sprite.position = _monster.position;
		sprite.rotation = Random.Range(0,360);
		sprite.transform.parent = MapDetritusContainer.transform;
	}
	
	private void Fall()
	{	
		_moving = false;
		InformFeetOfMotionUpdate(false);
		
		OT.DestroyImmediate(LeftFoot.GetComponent<footManager>());
		OT.DestroyImmediate(RightFoot.GetComponent<footManager>());
		
		_dead = true;
		
		Tint(Color.red);
		
		SpawnBloodSpotch();
		
		Dismember(10000f);
		
		_mobManager.DeregisterGoon(_monster);
		
		Hero.GetComponent<HeroManager>().GainXP(100);
	}
	
	// Informers
	
	private bool _underMouse = false;
	
	// NEEDS A VISIBILITY CHECK!
	// easy to do.
	void OnMouseEnter()
	{
		Hero.GetComponent<HeroManager>().SetMonsterUnderMouse(_monster);
		Crosshair.visible = true;
		
		_underMouse = true;
	}
	
	void OnMouseExit()
	{
		Hero.GetComponent<HeroManager>().SetMonsterUnderMouse(null);
		Crosshair.visible = false;
		
		_underMouse = false;
	}
}
