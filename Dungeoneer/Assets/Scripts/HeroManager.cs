using UnityEngine;
using System.Collections;
using System.Collections.Generic;

public class HeroManager : MonoBehaviour {
	
	#region Fields
	
	// Pointer hooks
	public Texture IconFootie;
	public Texture IconArcher;
	public Texture IconMage;
	
	public OTSprite Handheld1;
	public OTSprite Handheld2;
	
	public OTSprite Effects;
	
	public GUITexture MenuBoxBackground;
	public GUITexture InventorySlot;
	public GUITexture ItemGUITexture;
	public GUITexture InventoryBody;
	
	public GUITexture SkillBoxBackground;
	public GUITexture SkillButton;
	
	public GameManager _gameManager;
	
	public GameObject FogContainer;
	
	// feet
	private OTTween _leftfootTween;
	private OTTween _rightfootTween;
	
	// Private helper pointers
	private OTAnimatingSprite self;
	private GUIStyle _boxStyle;
	
	// Base stats
	private int _heroClass;
	private string _heroName;
	
	private int _strength;
	private int _strengthActive;
	private int _agility;
	private int _agilityActive;
	private int _magic;
	private int _magicActive;
	private int _level;
	private int _xp;
	
	// Derivative stats
	private int _currentHP;
	private int _maxHP;
	private int _maxHPActive;
	private float _movespeed;
	private float _movespeedActive;
	private float _attack;
	private float _armor;
	
	private float _blockChance = 0.0f;
	private float _dodgeChange = 0.0f;
	
	// Flags
	private bool _gameOver = false;
	private bool _onExit = false;
	private bool _moving = false;
	private bool _active = false;
	public bool Active
	{
		get
		{
			return _active;
		}
	}
	private bool _usingSkill = false;
	
	// Counters	
	private float _LOSUpdateTimer = 0f;
	
	// Inventory-related	
	private GUITexture[] _inventorySlots = new GUITexture[CONSTANTS.BaseInventorySize];
	private GUITexture[] _items = new GUITexture[CONSTANTS.BaseInventorySize];
	
	private GUITexture[] _equippedSlots = new GUITexture[CONSTANTS.BaseEquipmentSize];
	private GUITexture[] _equippedItems = new GUITexture[CONSTANTS.BaseEquipmentSize];
	
	private OTSprite _itemUnderMouse;
	private GUITexture _itemPickedUp;
	
	// Skills related
	private GUITexture[] _skillButtons = new GUITexture[CONSTANTS.BaseNumberOfSkills];
	private bool[] _skillValues = new bool[CONSTANTS.BaseNumberOfSkills];
	private string[] _skillNames = new string[CONSTANTS.BaseNumberOfSkills];
	private int[] _quickSkills = new int[5];
	private float[] _quickSkillsCooldowns = new float[5];
	private float[] _quickSkillsTimers = new float[5];
	
	private bool[] _activeSkills = new bool[CONSTANTS.BaseNumberOfSkills];
	
	private OTSprite _monsterUnderMouse;
	
	private Texture _greyBox;
	
	//private Skill[] _skills = new Skill[CONSTANTS.BaseNumberOfSkills];
	
	//private List<Effect> _activeEffects = new List<Effect>();
	#endregion
	
	#region Initializers
	
	private void InitializeEquipmentSlots()
	{
		InventoryBody.pixelInset = new Rect(Screen.width/2, 2*Screen.height/7, Screen.width/2, 5*Screen.height/7);
		
		int SizeRowsRatio = CONSTANTS.BaseInventorySize/CONSTANTS.BaseInventoryRows;
		
		int width = Screen.width/(2*SizeRowsRatio);
		int height = Screen.height/(SizeRowsRatio);
		
		// helm
		InitializeEquipmentSlot(0, new Vector2(3*Screen.width/4, Screen.height - Screen.height/(SizeRowsRatio)),
			width, height);
		
		// cloak
		InitializeEquipmentSlot(1, new Vector2(5*Screen.width/6, Screen.height - 2*Screen.height/(SizeRowsRatio)),
			width, height);
		
		// gloves
		InitializeEquipmentSlot(2, new Vector2(4*Screen.width/6, Screen.height - 3*Screen.height/(SizeRowsRatio)),
			width, height);
		
		// body armor
		InitializeEquipmentSlot(3, new Vector2(3*Screen.width/4, Screen.height - 4*Screen.height/(SizeRowsRatio)),
			width, height);	
		
		// right hand
		InitializeEquipmentSlot(4, new Vector2(4*Screen.width/6, Screen.height - 5*Screen.height/(SizeRowsRatio)),
			width, height);		
		
		// left hand
		InitializeEquipmentSlot(5, new Vector2(5*Screen.width/6, Screen.height - 5*Screen.height/(SizeRowsRatio)),
			width, height);
		
		// boots
		InitializeEquipmentSlot(6, new Vector2(3*Screen.width/4, Screen.height - 6*Screen.height/(SizeRowsRatio)),
			width, height);
		
	}
	
	private void InitializeEquipmentSlot(int index, Vector2 topleft, int width, int height)
	{		
		_equippedSlots[index] = Instantiate(InventorySlot) as GUITexture;
		_equippedSlots[index].transform.parent = MenuBoxBackground.transform;
		_equippedSlots[index].pixelInset = 
			new Rect(topleft.x, topleft.y, width, height);
		_equippedSlots[index].transform.localPosition = new Vector3(0,0,1);
	}
	
	private void InitializeSkills()
	{
		for(int i=0; i<_skillButtons.Length; ++i)
		{
			if(_skillValues[i])
			{
				_skillButtons[i].color = Color.green;	
			}
		}
		
		UpdateSkillBaseCooldowns();
	}
	
	// Sets basic cooldown levels for quickSkills
	private void UpdateSkillBaseCooldowns()
	{
		for(int i=0; i<_quickSkillsCooldowns.Length; ++i)
		{
			_quickSkillsCooldowns[i] = CONSTANTS.SkillCooldowns[_quickSkills[i]];	
		}
	}
	
	// Use this for initialization
	void Start () {
		// BoxStyle init
		_boxStyle = new GUIStyle();
		_boxStyle.alignment = TextAnchor.UpperLeft;
		_boxStyle.fontSize = 20;
		
		// Inventory init
		ItemGUITexture.gameObject.SetActive(false);
		
		InitializeWindow(MenuBoxBackground, InventorySlot, _inventorySlots, CONSTANTS.BaseInventorySize,
			CONSTANTS.BaseInventoryRows, Screen.width/2);
		
		InitializeEquipmentSlots();
		
		// Skill Window Init
		InitializeWindow(SkillBoxBackground, SkillButton, _skillButtons, CONSTANTS.BaseNumberOfSkills,
			CONSTANTS.BaseNumberOfSkillRows, 0);
		
		for(int i=0; i< _skillButtons.Length; ++i)
		{
			//newItem.texture = Resources.Load(data.itemType.ToString()+" Icon") as Texture;
			
			_skillButtons[i].texture = Resources.Load("IconSkill"+CONSTANTS.SkillNames[i]) as Texture;
			_skillNames[i] = CONSTANTS.SkillNames[i];
		}
		
		_greyBox = Resources.Load("GreyBox") as Texture;
		
		// Hero Init
		LoadHero();
		
		InitializeSkills();
		
		self = OT.ObjectByName("Hero") as OTAnimatingSprite;	
		self.onCollision = OnCollision;
		
		//OT.view.movementTarget = self.gameObject;
		
		// feet
		transform.FindChild("footleft").GetComponent<footManager>().SetLeadingStep(true);
		transform.FindChild("footright").GetComponent<footManager>().SetLeadingStep(false);
		
		// handheld items
		HandheldItemsReset();
		
		// LOS init
		_tiles = _gameManager.Tiles;
		_LOSMap = new bool[_tiles.GetLength(0), _tiles.GetLength(1)];
		_fogOfWar = new OTSprite[_tiles.GetLength(0), _tiles.GetLength(1)];
		
		if(CONSTANTS.DEBUGEnableVisibility)
		{
			GameObject tempobj;
			OTSprite newsprite;
			for (int i=0; i < _tiles.GetLength(0); ++i)
			{
				for (int j=0; j < _tiles.GetLength(1); ++j)
				{
					tempobj = OT.CreateObject("fog");
					newsprite = tempobj.GetComponent<OTSprite>();
					newsprite.transform.parent = FogContainer.transform;
					newsprite.position = new Vector2(i*CONSTANTS.TileSize, j*CONSTANTS.TileSize);
					_fogOfWar[i,j]=newsprite;
				}
			}
		}
		
		Effects.visible = false;
		
		if(CONSTANTS.DEBUGEnableVisibility)
		{
			UpdateLOS();
		}
	}
	
	private void InitializeWindow(GUITexture background, GUITexture basicButton, GUITexture[] buttonsContainer,
		int numberOfButtons, int numberOfRows, int windowLeftBound)
	{		
		background.pixelInset = 
			new Rect(windowLeftBound,0, Screen.width/2, Screen.height);
		background.gameObject.SetActive(false);
		basicButton.pixelInset = 
			new Rect(windowLeftBound,0, Screen.width/(2*numberOfButtons/numberOfRows), 
				Screen.height/(numberOfButtons/numberOfRows));
		buttonsContainer[0] = basicButton;
		for(int i=1; i<numberOfButtons; ++i)
		{
			buttonsContainer[i] = Instantiate(basicButton) as GUITexture;	
			buttonsContainer[i].transform.parent = background.transform;
			buttonsContainer[i].pixelInset = 
				new Rect(windowLeftBound + 
					(i%(numberOfButtons/numberOfRows))
					*Screen.width/(2*numberOfButtons/numberOfRows), 
					(Screen.height)*(((float)(i/(numberOfButtons/numberOfRows)))
					/(numberOfButtons/numberOfRows)), 
					Screen.width/(2*numberOfButtons/numberOfRows), 
					Screen.height/(numberOfButtons/numberOfRows));
		}
	}
	
	private void HandheldItemsReset()
	{
		Handheld1.position = new Vector2(0.3f, 0.3f);
		Handheld2.position = new Vector2(-0.3f, 0.3f);
		UpdateHandheldItemSprites();	
	}
	
	#endregion 
	
	#region StatUpdaters
	
	public void GainXP(int XP)
	{
		_xp += XP;
		
		while( _xp > CONSTANTS.BaseXPPerLevel*_level)
		{
			LevelUP();
		}
	}
	
	OTSprite _levelUpEffect;
	
	private void LevelUpTweenOnFinish(OTTween tween)
	{
		OT.Destroy(_levelUpEffect, 0.5f);
	}
	
	private void LevelUP()
	{
		// effects
		GameObject temp = OT.CreateObject("visualEffect");
		_levelUpEffect = temp.GetComponent<OTSprite>();
		_levelUpEffect.frameName = "circle";
		_levelUpEffect.size *= 100;
		_levelUpEffect.transform.parent = self.transform;
		_levelUpEffect.depth = -10;
		_levelUpEffect.tintColor = Color.yellow;
		_levelUpEffect.alpha = 0f;
		OTTween tween = new OTTween(_levelUpEffect, 2f, OTEasing.Linear);	
		tween.TweenAdd("alpha", 0.5f);
		tween.onTweenFinish = LevelUpTweenOnFinish;
		
		// stats
		_xp -= CONSTANTS.BaseXPPerLevel*_level;
		++_level;	
				
		switch(_heroClass)
		{
		case 0:
			++_strength;
			break;
		case 1:
			++_agility;
			break;
		case 2:
			++_magic;
			break;
		}
		RecalcStats();
		_currentHP = _maxHP;
	}
	
	// Recalculates the character' derivative stats; call
	// after changing base stats
	// USE DELEGATES FROM CONSTANTS.- INSTEAD
	private void RecalcStats()
	{
		_strengthActive = _strength;
		_agilityActive = _agility;
		_magicActive = _magic;
		
		_maxHP = 2*_strength + 1*_agility;
		_maxHPActive = _maxHP;
		if(_currentHP == 0)
		{
			_currentHP = _maxHPActive;
		}
		
		_movespeed = 100f + 100f*(_agility / (10f+_agility));
		_movespeedActive = _movespeed;
		
		AddEquipmentBonuses();
		ApplySkillBonuses();
		//ApplyEffects();
		
		_attack = 2*Mathf.Max(Mathf.Max(_strengthActive, _agilityActive),_magicActive);
		_armor = _strengthActive + _agilityActive;
	}
	
	private void AddEquipmentBonuses()
	{
		for(int i=0; i<_equippedItems.Length; ++i)
		{
			GUITexture currentItem = _equippedItems[i];
			if(currentItem != null)
			{
				ItemData data = currentItem.GetComponent<ItemData>();
				for (int j=0; j<data.ItemBonuses.Length; ++j)
				{
					ApplyItemBonus(data.ItemBonuses[j], data.ItemBonusValues[j]);	
				}
			}
		}
	}
	
	private void ApplyItemBonus(ItemBonusType bonusType, float bonusAmount)
	{
		switch(bonusType)
		{			
		case ItemBonusType.PlusAgility:
			_agilityActive += (int) bonusAmount;
			break;
		case ItemBonusType.PlusMagic:
			_magicActive += (int) bonusAmount;
			break;
		case ItemBonusType.PlusMaxHP:
			_maxHPActive += (int) bonusAmount;
			break;
		case ItemBonusType.PlusSpeed:
			_movespeedActive += (bonusAmount);
			break;
		case ItemBonusType.PlusStrength:
			_strengthActive += (int) bonusAmount;
			break;
		}
	}
	
	/*
	private void ApplyEffects()
	{
		foreach(Effect ef in _activeEffects)
		{
			switch(ef.EfType)
			{
			case EffectType.ModifyStrengthRaw:
				_strengthActive += (int) ef.EfValue;
				break;
			}
		}
	}
	*/
		
	private void ApplySkillBonuses()
	{
		for(int i=0; i<CONSTANTS.BaseNumberOfSkills; ++i)
		{
			if(_activeSkills[i])
			{
				switch(_skillNames[i])
				{
				case "Shield":
					_blockChance += CONSTANTS.SkillShieldBlockBonus;
					_movespeedActive *= CONSTANTS.SkillShieldMovespeedModifier;
					break;
				case "Dodge":
					_dodgeChange += CONSTANTS.SkillDodgeDodgeBonus;
					_movespeedActive *= CONSTANTS.SkillDodgeMovespeedModifier;
					break;
				}
				
			}
		}
	}
	
	private bool TryToBlock()
	{
		float diceroll = Random.Range(0f, 1f);
		
		//float blockChance = _blockChance;
			
		return (diceroll < _blockChance);
	}
	
	private void PlayBlockAnim()
	{
		if(_equippedItems[5] == null)
		{
			return;	
		}
		
		HandheldManager offhand = Handheld2.GetComponent<HandheldManager>();
		
		offhand.Stop();
		HandheldItemsReset();
		offhand.Block();
	}
	
	public void RegisterDamage(float damage)
	{
		float residueDamage = damage / Mathf.Pow(1.05f, _armor);
		
		// block check; for now absorbs full damage.
		if(TryToBlock())
		{
			PlayBlockAnim();
			return;	
		}
		
		_currentHP -= (int) Mathf.Max(residueDamage,1f);
		
		if(_currentHP < 1)
		{
			GameOver();	
		}
	}
	
	#endregion
	
	#region Updates Logic
	
	private void UpdateSkillCooldowns()
	{
		for (int i=0; i<_quickSkillsTimers.Length; ++i)
		{
			_quickSkillsTimers[i] = 
				Mathf.Max(_quickSkillsTimers[i] - Time.deltaTime, 0f);
		}
	}
	
	// Update is called once per frame
	void Update () {
		OT.view.position = new Vector2(Mathf.Round(self.position.x), Mathf.Round(self.position.y));
		
		// LOS
		if(CONSTANTS.DEBUGEnableVisibility)
		{
			_LOSUpdateTimer += Time.deltaTime;
			if(_LOSUpdateTimer > CONSTANTS.LOSUpdateInterval)
			{
				_LOSUpdateTimer = 0f;
				UpdateLOS();
			}
		}
			
		UpdateSkillCooldowns();
		
		// busy; do nothing
		if(_usingSkill)
		{
			return;
		}
		
		// LMB and RMB checks
		if((Input.GetMouseButton(0) || Input.GetMouseButton(1)) 
			&& (!MenuBoxBackground.gameObject.activeSelf || Input.mousePosition.x < Screen.width/2)
			&& (!SkillBoxBackground.gameObject.activeSelf || Input.mousePosition.x > Screen.width/2))
		{
			// quick fix to an infinite loop with the footManager tween... Fix later.
			if(!_active)
			{
				_active = true;	
			}
			
			Vector2 facedir = OT.view.mouseWorldPosition - self.position;
			self.rotation = CreatureManager.FindAngleFacing(facedir);
			
			// LMB -> move or pickup
			if(Input.GetMouseButton(0))
			{
				LMBHandlerWorld(facedir);
			}
			// RMB -> attack
			else if(Input.GetMouseButton(1) && ! Handheld1.GetComponent<HandheldManager>().Animating())
			{
				RMBHandlerWorld(facedir);
			}
		}
		// skill menu
		else if((Input.GetMouseButtonUp(0) || Input.GetMouseButtonUp(1)) 
			&& (SkillBoxBackground.gameObject.activeSelf && Input.mousePosition.x < Screen.width/2))
		{

			MouseHandlerSkillMenu();
		}
		// nothing
		else
		{
			if(_moving)
			{
				_moving = false;
				InformFeetOfMotionUpdate(false);
			}
		}
		
		KeyboardHandler();
	}
	
	private void LMBHandlerWorld(Vector2 facedir)
	{
		if(_itemUnderMouse != null && 
			Vector2.Distance(_itemUnderMouse.position, self.position) < CONSTANTS.PickUpRadius)
		{
			// Pick up
			// inventory closed, try to insert item in inventory
			if(!MenuBoxBackground.gameObject.activeSelf)
			{
				if(TryToPlacePickupInInventory(_itemUnderMouse))
				{
					Destroy(_itemUnderMouse.gameObject);
				}
			}
			
			// inventory open, pick up item
			else
			{
				SpawnItem(_itemUnderMouse.GetComponent<ItemData>());
				Destroy(_itemUnderMouse.gameObject);
			}
		}
		else
		{
			// Move
			if(! _moving)
			{
				_moving = true;
				InformFeetOfMotionUpdate(true);
			}
			
			// rotation/ animation
			if(!self.isPlaying)
			{
				self.PlayOnce("CloakWave");	
			}
			
			facedir.Normalize();
			self.position += Time.deltaTime * (_movespeedActive*facedir);
			
		}
	}
	
	private void WeaponAnimHandler(float duration)
	{
		if(_equippedItems[4] == null)
		{
			Handheld1.GetComponent<HandheldManager>().Slash(duration);
		}
		else
		{
			switch(_equippedItems[4].GetComponent<ItemData>().itemType)
			{
			case ItemType.swordShort:
			case ItemType.swordLong:
				Handheld1.GetComponent<HandheldManager>().Stab(duration);
				break;
			case ItemType.axeHatchet:
			case ItemType.axeBattle:		
			case ItemType.axeTwoHanded:
			case ItemType.swordTwoHanded:
				Handheld1.GetComponent<HandheldManager>().Slash(duration);
				break;							
			case ItemType.bow:				
				Handheld1.GetComponent<HandheldManager>().Recoil(duration);
				break;		
			case ItemType.staff:				
				Handheld1.GetComponent<HandheldManager>().Twirl(duration);
				break;
			}
		}
	}
	
	// returns true if skill requires melee and weapon is melee
	// returns true if skill does not require melee and weapon is ranged
	// returns false otherwise
	private bool ItemSkillSyncCheck(bool skillRequiresMelee)
	{
		// fists...
		if(_equippedItems[4] == null)
		{
			return (skillRequiresMelee);
		}
		
		ItemClass weapon = _equippedItems[4].GetComponent<ItemData>().itemClass;
		
		// assumes one can wield only melee or ranged weapons
		return (weapon == ItemClass.Melee1H || weapon == ItemClass.Melee2H) == skillRequiresMelee;
	}
	
	private void RMBHandlerWorld(Vector2 facedir)
	{
		facedir.Normalize();
		
		SkillAttackHandler(facedir);
		
		/*
		if(_equippedItems[4] == null)
		{
			Handheld1.GetComponent<HandheldManager>().Slash();
		}
		else
		{
			switch(_equippedItems[4].GetComponent<ItemData>().itemType)
			{
			case ItemType.swordShort:
			case ItemType.swordLong:
				Handheld1.GetComponent<HandheldManager>().Stab();
				break;
			case ItemType.axeHatchet:
			case ItemType.axeBattle:		
			case ItemType.axeTwoHanded:
			case ItemType.swordTwoHanded:
				Handheld1.GetComponent<HandheldManager>().Slash();
				break;							
			case ItemType.bow:				
				Handheld1.GetComponent<HandheldManager>().Recoil();
				break;		
			case ItemType.staff:				
				Handheld1.GetComponent<HandheldManager>().Twirl();
				break;
			}
		}
		*/
		
		//GameObject newarrow = OT.CreateObject("arrow");
		
		/*
		switch(_heroClass)
		{
		case 0:
			newarrow.GetComponent<arrowManager>().
				SetParameters(self, CONSTANTS.BaseMeleeProjectileSpeed*facedir, 
					CONSTANTS.BaseMeleeRange, _attack);
			newarrow.GetComponent<OTSprite>().visible = false;
			break;
				
		case 1:
			newarrow.GetComponent<arrowManager>().SetParameters(self, 
				CONSTANTS.BaseRangedProjectileSpeed*facedir, 
				CONSTANTS.BaseRangedRange, _attack);
			break;
							
		case 2:
			newarrow.GetComponent<OTSprite>().frameName = "spark";
			newarrow.GetComponent<arrowManager>().SetParameters(self, 
				CONSTANTS.BaseMagicProjectileSpeed*facedir, 
				CONSTANTS.BaseMagicRange, _attack);
			break;
		}
		*/
		
		// newarrow.GetComponent<OTSprite>().rotation = CreatureManager.FindAngleFacing(facedir);	
	}
	
	private void MouseHandlerSkillMenu()
	{		
		int slotUnderMouse = SkillSlotUnderPoint(Input.mousePosition);
		if(slotUnderMouse != -1)
		{
			if(Input.GetMouseButtonUp(0))
			{
				// reqs check
				if(!RunSkillReqsCheck(slotUnderMouse))
				{
					// fail, do nothing
					return;
				}
				
				if(_skillValues[slotUnderMouse] == true)
				{
					int index = slotUnderMouse % 5;
					_quickSkills[index ] = slotUnderMouse;
					
					UpdateSkillBaseCooldowns();
					
					// skill appears on cooldown to prevent skill-swap-spam
					// actually this should be changed so skill-swap
					// not allowed during cooldown...
					_quickSkillsTimers[index] = _quickSkillsCooldowns[index];
				}
				
				_skillValues[slotUnderMouse] = true;
				_skillButtons[slotUnderMouse].color = Color.green;
				
				/*
				if(_skills[slotUnderMouse] != null)
				{
					foreach(Effect ef in _skills[slotUnderMouse].GetEffects())
					{
						_activeEffects.Add(ef);
					}
					RecalcStats();
				}
				*/
			}
			if(Input.GetMouseButtonUp(1))
			{
				_skillValues[slotUnderMouse] = false;	
				_skillButtons[slotUnderMouse].color = Color.gray;
			}
		}
	}
	
	private bool RunSkillReqsCheck(int skillIndex)
	{
		return (_strength > CONSTANTS.SkillReqs[skillIndex].x) &&
			(_agility > CONSTANTS.SkillReqs[skillIndex].y) &&
				(_magic > CONSTANTS.SkillReqs[skillIndex].z);
	}
	
	private void KeyboardHandler()
	{
		// Inventory window switcher
		if(Input.GetKeyUp(KeyCode.I))
		{
			if(MenuBoxBackground.gameObject.activeSelf)
			{
				// Drop item upon closing inv window
				if(_itemPickedUp != null)
				{
					_itemPickedUp.GetComponent<ItemTextureControls>().PickedUp = false;
					AttemptItemPutDown(_itemPickedUp);
				}
			}
			MenuBoxBackground.gameObject.SetActive(!MenuBoxBackground.gameObject.activeSelf);
		}
		
		// skills window
		if(Input.GetKeyUp(KeyCode.S))
		{
			SkillBoxBackground.gameObject.SetActive(!SkillBoxBackground.gameObject.activeSelf);
		}
		
		// quick skill 1 (defense)
		if(Input.GetKeyUp(KeyCode.Q))
		{
			if(SkillCooldownCheck(1))
			{
				SkillDefenseHandler();
			}
		}
		
		// quick skill 2 (stun/nuke)
		if(Input.GetKeyUp(KeyCode.W))
		{
			if(SkillCooldownCheck(2))
			{
				SkillStunHandler();
			}
		}
		
		// quick skill 3 (teleport)
		if(Input.GetKeyUp(KeyCode.E))
		{
			if(SkillCooldownCheck(3))
			{
				SkillTeleportHandler();
			}
		}
		
		// quick skill 4 (AoE)
		if(Input.GetKeyUp(KeyCode.R))
		{
			if(SkillCooldownCheck(4))
			{
				SkillAOEHandler();
			}
		}
	}
	
	#region SkillHandlers
	
	private bool SkillCooldownCheck(int quickSkillIndex)
	{
		return (_quickSkillsTimers[quickSkillIndex] == 0f);
	}
	
	private void SkillAttackHandler(Vector2 facedir)
	{
		// melee
		if(_skillNames[_quickSkills[0]] == "Melee")
		{	
			// weapon sync check
			if(! ItemSkillSyncCheck(true))
			{
				return;	
			}
			
			GameObject newarrow = OT.CreateObject("arrow");
			newarrow.GetComponent<arrowManager>().
				SetParameters(self, CONSTANTS.BaseMeleeProjectileSpeed*facedir, 
					CONSTANTS.BaseMeleeRange, _attack);
			newarrow.GetComponent<OTSprite>().visible = false;
			newarrow.GetComponent<OTSprite>().rotation = CreatureManager.FindAngleFacing(facedir);
			//duration = CONSTANTS.BaseAttackAnimDuration;
		}
		// ranged
		else if(_skillNames[_quickSkills[0]] == "Ranged")
		{	
			// weapon sync check
			if(! ItemSkillSyncCheck(false))
			{
				return;	
			}
			
			GameObject newarrow = OT.CreateObject("arrow");
			newarrow.GetComponent<arrowManager>().SetParameters(self, 
				CONSTANTS.BaseRangedProjectileSpeed*facedir, 
				CONSTANTS.BaseRangedRange, _attack);
			newarrow.GetComponent<OTSprite>().rotation = CreatureManager.FindAngleFacing(facedir);
			
		}
		// magic
		else if(_skillNames[_quickSkills[0]] == "Magic")
		{				
			GameObject newarrow = OT.CreateObject("arrow");
			newarrow.GetComponent<OTSprite>().frameName = "spark";
			newarrow.GetComponent<arrowManager>().SetParameters(self, 
				CONSTANTS.BaseMagicProjectileSpeed*facedir, 
				CONSTANTS.BaseMagicRange, _attack);
			newarrow.GetComponent<OTSprite>().rotation = CreatureManager.FindAngleFacing(facedir);
			//duration = CONSTANTS.BaseAttackAnimDuration;
		}
		
		float duration = CONSTANTS.SkillCooldowns[_quickSkills[0]];
		WeaponAnimHandler(duration);
	}
	
	private void SkillDefenseHandler()
	{
		int skillIndex = _quickSkills[1];
		_activeSkills[skillIndex] = !_activeSkills[skillIndex];
		
		// take care of animations etc
		if(_activeSkills[skillIndex])
		{	
			if(_skillNames[skillIndex] == "Shield")
			{
				Handheld2.size *= 2f;
			}
		
			else if(_skillNames[skillIndex] == "Dodge")
			{
				self.alpha = 0.5f;		
			}
			
			else if(_skillNames[skillIndex] == "Wall")
			{
				Effects.frameName = "ring";		
				Effects.visible = true;
				Effects.tintColor = Color.blue;
			}
		}
		else
		{			
			if(_skillNames[skillIndex] == "Shield")
			{
				Handheld2.size *= 0.5f;
			}
			
			else if(_skillNames[skillIndex] == "Dodge")
			{
				self.alpha = 1.0f;
			}
			
			else if(_skillNames[skillIndex] == "Wall")
			{
				Effects.visible = false;
				Effects.tintColor = Color.white;
			}
		}
		
		RecalcStats();
		
		_quickSkillsTimers[1] = _quickSkillsCooldowns[1];
	}
	
	private void SkillStunHandler()
	{
		// skill needs target to execute
		if(_monsterUnderMouse == null)
		{
			return;	
		}
		
		float distance = (_monsterUnderMouse.position - self.position).magnitude;
		
		// effects
		// crush
		if(_skillNames[_quickSkills[2]] == "Crush")
		{			
			// range check
			if(distance > CONSTANTS.SkillCrushRange)
			{
				return;	
			}
			
			// weapon sync check
			if(! ItemSkillSyncCheck(true))
			{
				return;	
			}
			
			GameObject newarrow = OT.CreateObject("arrow");
			newarrow.GetComponent<OTSprite>().frameName = Handheld1.frameName.ToString();
			
			Vector2 facedir = _monsterUnderMouse.position - self.position;
			facedir.Normalize();
			
			List<Effect> effects = new List<Effect>();
			Effect ef = ScriptableObject.CreateInstance("Effect") as Effect;
			ef.SetValues(EffectType.Stun, 0f, CONSTANTS.SkillCrushStunDuration);
			effects.Add(ef);
			
			// shoot weapon from above
			newarrow.GetComponent<arrowManager>().SetParameters(self, 
				2*CONSTANTS.BaseRangedProjectileSpeed*facedir, 
				300f, _attack*CONSTANTS.SkillCrushDamageMP, 
				_monsterUnderMouse, effects);
			newarrow.GetComponent<OTSprite>().position = 
				new Vector2(_monsterUnderMouse.position.x, _monsterUnderMouse.position.y + 100);
		}
		// snipe
		else if(_skillNames[_quickSkills[2]] == "Snipe")
		{
			// range check
			if(distance > CONSTANTS.SkillSnipeRange)
			{
				return;	
			}
			
			// weapon sync check
			if(! ItemSkillSyncCheck(false))
			{
				return;	
			}
			
			Handheld1.GetComponent<HandheldManager>().Recoil(CONSTANTS.SkillCooldowns[_quickSkills[0]]);
			
			GameObject newarrow = OT.CreateObject("arrow");
			
			Vector2 facedir = _monsterUnderMouse.position - self.position;
			facedir.Normalize();
			
			List<Effect> effects = new List<Effect>();			
			Effect ef = ScriptableObject.CreateInstance("Effect") as Effect;
			ef.SetValues(EffectType.Slow, CONSTANTS.SkillSnipeSlowMagnitude,
				CONSTANTS.SkillSnipeSlowDuration);
			effects.Add(ef);
			
			// shoot homing arrow 
			newarrow.GetComponent<arrowManager>().SetParameters(self, 
			2*CONSTANTS.BaseRangedProjectileSpeed*facedir, 
			CONSTANTS.BaseRangedRange, _attack*CONSTANTS.SkillSnipeDamageMP, 
				_monsterUnderMouse, effects);
		}		
		// burn
		else if(_skillNames[_quickSkills[2]] == "Burn")
		{
			// range check
			if(distance > CONSTANTS.SkillBurnRange)
			{
				return;	
			}
			
			Handheld1.GetComponent<HandheldManager>().Twirl(CONSTANTS.SkillCooldowns[_quickSkills[0]]);
			
			GameObject newarrow = OT.CreateObject("arrow");
			newarrow.GetComponent<OTSprite>().frameName = "fireballProj";
			
			Vector2 facedir = _monsterUnderMouse.position - self.position;
			facedir.Normalize();
			
			// shoot fireball from above
			newarrow.GetComponent<arrowManager>().SetParameters(self, 
				4*CONSTANTS.BaseRangedProjectileSpeed*facedir, 
				CONSTANTS.BaseRangedRange, _attack*CONSTANTS.SkillBurnDamageMP, 
				_monsterUnderMouse, null);
			newarrow.GetComponent<OTSprite>().position = 
				new Vector2(_monsterUnderMouse.position.x, _monsterUnderMouse.position.y + 400);
		}
		
		_quickSkillsTimers[2] = _quickSkillsCooldowns[2];
	}

	private void SkillTeleportHandler()
	{
		// Shoot ray in dir of mouse
		RaycastHit losRay;
		
		Vector3 startPoint = new Vector3(self.transform.position.x, 
			self.transform.position.y, 40);
				
		Vector3 mousePos = Input.mousePosition;
		mousePos.z = 1;
		Vector3 targetxy = Camera.main.ScreenToWorldPoint(mousePos);
		
		Vector3 endPoint = new Vector3(targetxy.x, targetxy.y, 40);
		Vector3 dir = endPoint - startPoint;
		float distance = Vector3.Distance(startPoint, endPoint);
		
		float range = 0f;
		
		switch(_skillNames[_quickSkills[3]])
		{
		case "Teleport":
			range = CONSTANTS.SkillTeleportRange;
			break;
		case "Leap":
			range = CONSTANTS.SkillLeapRange;
			break;
		case "Slink":
			range = CONSTANTS.SkillSlinkRange;
			break;
		}
		
		distance = Mathf.Min(distance, range);
		dir.Normalize();
		dir *= distance;
		
		Physics.Raycast(startPoint, dir, out losRay, distance);
		
		Vector3 targetSpot;
		
		// no obstructions, can teleport to mouse
		if(losRay.rigidbody == null)
		{
			targetSpot = self.transform.position + new Vector3(dir.x,
				dir.y, 0);
		}
		// obstruction, teleport in front of it
		else
		{
			Vector3 dir2 = dir;
			dir2.Normalize();
			
			Vector2 selfxy = new Vector2(self.collider.bounds.extents.x,
				self.collider.bounds.extents.y);

			Vector3 target = losRay.point - dir2*selfxy.magnitude;
			
			targetSpot = 
				new Vector3(target.x, target.y, self.transform.position.z);
		}

		
		// effects
		// Leap
		if(_skillNames[_quickSkills[3]] == "Leap")
		{	
			OTTween leap = new OTTween(self.transform, 0.5f, OTEasing.Linear);
			leap.TweenAdd("position", targetSpot-self.transform.position, OTEasing.ExpoInOut);
			leap.onTweenFinish = TweenLeapOnFinish;
			
			_usingSkill = true;
		}
		// Slink
		else if(_skillNames[_quickSkills[3]] == "Slink")
		{	
			OTTween slink = new OTTween(self.transform, 0.5f, OTEasing.Linear);
			slink.TweenAdd("position", targetSpot-self.transform.position, OTEasing.Linear);
			slink.onTweenFinish = TweenSlinkOnFinish;
			
			self.alpha = 0.5f;
			
			_usingSkill = true;
		}
		// Teleport
		else if(_skillNames[_quickSkills[3]] == "Teleport")
		{		
			self.transform.position = targetSpot;
			
			Effects.frameName = "circle";
			Effects.visible = true;
			OTTween teleport = new OTTween(Effects, 0.5f, OTEasing.Linear);
			teleport.TweenAdd("alpha", -1f, OTEasing.Linear);
			teleport.onTweenFinish = TweenTeleport1OnFinish;
			
			_usingSkill = true;
		}
		
		_quickSkillsTimers[3] = _quickSkillsCooldowns[3];
	}
	
	private void SkillAOEHandler()
	{
		Vector2 facedir = OT.view.mouseWorldPosition - self.position;
		facedir.Normalize();
		
		// Flurry
		if(_skillNames[_quickSkills[4]] == "Flurry")
		{
			// weapon sync check
			if(! ItemSkillSyncCheck(true))
			{
				return;	
			}
			
			OTTween flurry = new OTTween(self, 0.5f, OTEasing.Linear);
			flurry.TweenAdd("rotation", 360f, OTEasing.Linear);
			flurry.onTweenFinish = TweenFlurryOnFinish;
			
			_usingSkill = true;
			Handheld1.position += new Vector2(0, 0.2f);
			
			GameObject AOE = OT.CreateObject("AOE");
			AOE.GetComponent<OTAnimatingSprite>().position = self.position;
			AOE.GetComponent<OTAnimatingSprite>().visible = false;
			AOE.GetComponent<AOEManager>().
				SetParemeters(self, _attack*CONSTANTS.SkillFlurryDamageMP);
		}
		// Multishot
		else if(_skillNames[_quickSkills[4]] == "Multishot")
		{
			// weapon sync check
			if(! ItemSkillSyncCheck(false))
			{
				return;	
			}
			
			facedir = CreatureManager.RotateDeg(facedir, -CONSTANTS.SkillMultishotHalfarcDeg);
			
			float angle = 2*CONSTANTS.SkillMultishotHalfarcDeg/(CONSTANTS.SkillMultishotArrows-1);
			
			for(int i=0; i<CONSTANTS.SkillMultishotArrows; ++i)
			{
				GameObject newarrow = OT.CreateObject("arrow");
				newarrow.GetComponent<arrowManager>().SetParameters(self, 
					CONSTANTS.BaseRangedProjectileSpeed*facedir, 
					CONSTANTS.SkillMultishotRange, _attack*CONSTANTS.SkillMultishotDamageMP);
				newarrow.GetComponent<OTSprite>().rotation = 
					CreatureManager.FindAngleFacing(facedir);
				
				facedir = CreatureManager.RotateDeg(facedir, angle);
			}
		}
		// Fireball
		else if(_skillNames[_quickSkills[4]] == "Fireball")
		{
				
			GameObject newarrow = OT.CreateObject("arrow");
			newarrow.GetComponent<OTSprite>().frameName = "fireballProj";
			newarrow.GetComponent<OTSprite>().rotation 
				= CreatureManager.FindAngleFacing(facedir);
			newarrow.GetComponent<arrowManager>().SetParameters(self, 
				CONSTANTS.SkillFireballSpeedMP*CONSTANTS.BaseMagicProjectileSpeed*facedir, 
				CONSTANTS.SkillFireballRange, _attack*CONSTANTS.SkillFireballDamageMP, true);
		}
		
		_quickSkillsTimers[4] = _quickSkillsCooldowns[4];
	}
	
	#endregion
	
	#region SkillTweens
	
	private void TweenCrushOnFinish(OTTween tween)
	{
		_usingSkill = false;
		HandheldItemsReset();
	}
	
	private void TweenSlinkOnFinish(OTTween tween)
	{
		self.alpha = 1.0f;
		_usingSkill = false;	
	}
	
	private void TweenLeapOnFinish(OTTween tween)
	{
		_usingSkill = false;	
	}
	
	private void TweenTeleport1OnFinish(OTTween tween)
	{
		Effects.visible = false;
		Effects.alpha = 1f;
		_usingSkill = false;	
	}
	
	private void TweenFlurryOnFinish(OTTween tween)
	{
		_usingSkill = false;
		//Handheld1.position += new Vector2(0, -0.2f);
		HandheldItemsReset();
	}
	
	#endregion
	
	public void SetMonsterUnderMouse(OTSprite monster)
	{
		_monsterUnderMouse = monster;	
	}
	
	// OnCollision updates
	private void OnCollision(OTObject owner)
	{
		// heavy redundancy, fix...
		_onExit = false;
		
		OTObject target = owner.collisionObject;
		if (target is OTAnimatingSprite)
		{	
			// CHEST handler
			if(target.baseName == "chest")
			{
				target.GetComponent<OTAnimatingSprite>().PlayOnce("Open");
				target.GetComponent<OTAnimatingSprite>().collidable = false;
				
				OTSprite newpickup = OT.CreateObject("pickup").GetComponent<OTSprite>();
				
				newpickup.position = 
					target.position + new Vector2(Random.Range(-20,20), Random.Range(-20,20));
				newpickup.gameObject.AddComponent<ItemData>();
				newpickup.GetComponent<ItemData>().SpawnRandom();
				
				newpickup.frameName = newpickup.GetComponent<ItemData>().itemType.ToString();
				
				OT.DestroyObject(target, 5);
			}
		}
		// EXIT handler
		else if(target.baseName == "StairsDown")
		{
			_onExit = true;
		}
}
	
	private void InformFeetOfMotionUpdate(bool moving)	
	{
		transform.FindChild("footleft").GetComponent<footManager>().SetMovingFlag(moving);
		transform.FindChild("footright").GetComponent<footManager>().SetMovingFlag(moving);
	}
	
	// GameOver mop-up function
	private void GameOver()
	{
		Time.timeScale = 0;	
		_gameOver = true;
	}
	
	#endregion
	
	#region BasicGUI
	
	private void OnGUI() {
				
		// GameOver! label
		if(_gameOver)
		{
			GUI.Box (new Rect(Screen.width/2-50,Screen.height/2-15, 100, 30), "GAME OVER!");
		}
		
		// status bar
		GUI.Box (new Rect(0,0, Screen.width, 30), 
			"NAME: " + _heroName + 
			"; STRENGTH: " + _strengthActive + "; AGILITY: " + _agilityActive + 
			"; MAGIC: " + _magicActive + 
			"; LEVEL: " + _level + "; XP: " + _xp +
			"; ATTACK: " + _attack + " ARMOR: " + _armor +
			"; HP: " + _currentHP + "/" + _maxHPActive
			);
		
		// Next Level button; activates when the character is over an exit
		if(_onExit)
		{
			if(GUI.Button(new Rect(Screen.width/2 - 80, 30, 160, 50), "DESCEND DEEPER..."))
			{
				_moving = false;
				InformFeetOfMotionUpdate(false);
				SaveHero();
				OT.DestroyAll();
				Application.LoadLevel("Main Scene");
			}
		}
		
		// QuickSkills WIndow
		GUI.Window(2, new Rect(Screen.width*11/12, Screen.height/8, Screen.width/12, Screen.height*5/8), 
			WindowQuickSkills,"");
		
		// Character Window
		if(Input.GetKey(KeyCode.C))
		{
			GUI.Window(1, new Rect(0,0,Screen.width/2,Screen.height),WindowChar, "CHARACTER");
		}

		// Inventory interface handled elsewhere
		
		// Skill tooltip
		if (SkillBoxBackground.gameObject.activeSelf && Input.mousePosition.x < Screen.width/2)
		{
			int i = SkillSlotUnderPoint(Input.mousePosition);
			if(i != -1)
			{
				GUITexture skillSlot = _skillButtons[i];
				GUI.Box(new Rect(skillSlot.pixelInset.xMin, Screen.height - skillSlot.pixelInset.yMax, 200, 20), 
					_skillNames[i]);
				GUI.Box(new Rect(skillSlot.pixelInset.xMin, Screen.height 
					- skillSlot.pixelInset.yMax+20, 200, 40), 
					"REQS: " + " strength > " + CONSTANTS.SkillReqs[i].x 
					+ "\n agility > " + CONSTANTS.SkillReqs[i].y
					+ "; magic > " + CONSTANTS.SkillReqs[i].z);
			}
		}
	}
	
	// character window
	private void WindowChar(int WindowID)
	{		
		switch(_heroClass)
		{
		case 0:
			GUI.Box (new Rect(0, 40, 100, 100), IconFootie);
			break;
		case 1:
			GUI.Box (new Rect(0, 40, 100, 100), IconArcher);
			break;
		case 2:
			GUI.Box (new Rect(0, 40, 100, 100), IconMage);
			break;
		}
		
		GUI.Box (new Rect(100,40, Screen.width/2,Screen.height-40), 
			"STATS: " + 
			"\n\n Name: " + _heroName + "\n Class: " + _heroClass +
			"\n\n Strength: " + _strength + "(" + _strengthActive + ")" +
			"\n Agilty: " + _agility + "(" + _agilityActive + ")" +
			"\n Magic: " + _magic + "(" + _magicActive + ")" +
			"\n\n HP: " + _currentHP + "/" + _maxHP + "(" + _maxHPActive + ")" +
			"\n\n Attack: " + _attack + "\n Armor: " + _armor +
			"\n\n Speed: " + _movespeed + "(" + _movespeedActive + ")" +
			"\n\n Level: " + _level + "\n XP: " + _xp + "/" + 100*_level, 
			_boxStyle);
	}
			
	// quick skills window
	private void WindowQuickSkills(int WindowID)
	{
		int x = 0;
		int y = 0;
		int width = Screen.width/12;
		int height = Screen.height/8;
		//int height = width;
		
		GUI.DrawTexture(new Rect(x,y,width, height), 
			_skillButtons[_quickSkills[0]].texture, ScaleMode.StretchToFill);
		float cooldownRatio = _quickSkillsTimers[0]/_quickSkillsCooldowns[0];
		if(cooldownRatio > 0f)
		{
			GUI.DrawTexture(new Rect(x,y, cooldownRatio*width, height), _greyBox, ScaleMode.StretchToFill);
		}
		y += height;
		
		GUI.DrawTexture(new Rect(x,y,width, height), 
			_skillButtons[_quickSkills[1]].texture, ScaleMode.StretchToFill);
		cooldownRatio = _quickSkillsTimers[1]/_quickSkillsCooldowns[1];
		if(cooldownRatio > 0f)
		{
			GUI.DrawTexture(new Rect(x,y, cooldownRatio*width, height), _greyBox, ScaleMode.StretchToFill);
		}
		y += height;
		
		GUI.DrawTexture(new Rect(x,y,width, height), 
			_skillButtons[_quickSkills[2]].texture, ScaleMode.StretchToFill);
		cooldownRatio = _quickSkillsTimers[2]/_quickSkillsCooldowns[2];
		if(cooldownRatio > 0f)
		{
			GUI.DrawTexture(new Rect(x,y, cooldownRatio*width, height), _greyBox, ScaleMode.StretchToFill);
		}
		y += height;
		
		GUI.DrawTexture(new Rect(x,y,width, height), 
			_skillButtons[_quickSkills[3]].texture, ScaleMode.StretchToFill);
		cooldownRatio = _quickSkillsTimers[3]/_quickSkillsCooldowns[3];
		if(cooldownRatio > 0f)
		{
			GUI.DrawTexture(new Rect(x,y, cooldownRatio*width, height), _greyBox, ScaleMode.StretchToFill);
		}
		y += height;
		
		GUI.DrawTexture(new Rect(x,y,width, height), 
			_skillButtons[_quickSkills[4]].texture, ScaleMode.StretchToFill);
		cooldownRatio = _quickSkillsTimers[4]/_quickSkillsCooldowns[4];
		if(cooldownRatio > 0f)
		{
			GUI.DrawTexture(new Rect(x,y, cooldownRatio*width, height), _greyBox, ScaleMode.StretchToFill);
		}
	}
	
	#endregion
		
	// returns index of skill slot under given point
	private int SkillSlotUnderPoint(Vector2 point)
	{
		float adjustedX = point.x;
		float adjustedY = point.y;
		
		// don't recalc every time...
		int ratio = CONSTANTS.BaseNumberOfSkills/CONSTANTS.BaseNumberOfSkillRows;
		
		float baseSlotHeight = Screen.height/(ratio) +1;
		float baseSlotWidth = Screen.width/(2*ratio) +1;
		
		if(adjustedX < 0 || adjustedX >= Screen.width/2 || adjustedY < 0 
			|| adjustedY >= CONSTANTS.BaseNumberOfSkillRows*baseSlotHeight)
		{
			return -1;
		}
		
		int returnVal = (ratio)*
			((int)(adjustedY / baseSlotHeight)) + (int)(adjustedX / baseSlotWidth);
		
		return returnVal;
	}
	
	#region Inventory
	
	private bool TwoHandedItemEquipped()
	{
		if(_equippedItems[4] == null)
		{
			return false;	
		}
		
		ItemClass mainHand = _equippedItems[4].GetComponent<ItemData>().itemClass;
		
		return (mainHand == ItemClass.Melee2H || mainHand == ItemClass.Ranged2H);
	}
	
	private void UpdateHandheldItemSprites()
	{
		if(_equippedItems[4] != null)
		{
			Handheld1.frameName = _equippedItems[4].GetComponent<ItemData>().itemType.ToString();	
		}
		else
		{
			Handheld1.frameName = "fist";
		}
	
		if(_equippedItems[5] != null)
		{
			Handheld2.visible = true;
			Handheld2.frameName = _equippedItems[5].GetComponent<ItemData>().itemType.ToString();	
		}
		else if(! TwoHandedItemEquipped())
		{
			Handheld2.visible = true;
			Handheld2.frameName = "fist";
		}
		else
		{
			Handheld2.visible = false;	
		}
	}
	
	// Updates the pointer for the item on the ground under the cursor
	public void SetItemUnderMouse(OTSprite item)
	{
		_itemUnderMouse = item;	
	}
	
	// Adds the GUITexture item described by "data" in the given slot
	private void AddItemInventory(int slot, ItemData data)
	{
		GUITexture newItem = Instantiate(ItemGUITexture) as GUITexture;
		newItem.texture = Resources.Load(data.itemType.ToString()+" Icon") as Texture;
		
		// create copying mechanic?
		newItem.GetComponent<ItemData>().SetValues(data);
		
		newItem.transform.parent = _inventorySlots[slot].transform;
		newItem.pixelInset = 
			new Rect(_inventorySlots[slot].pixelInset.xMin, _inventorySlots[slot].pixelInset.yMin, 
				_inventorySlots[slot].pixelInset.width, _inventorySlots[slot].pixelInset.height);
		newItem.gameObject.SetActive(true);
		newItem.transform.localPosition = new Vector3(0,0,2);
		_items[slot] = newItem;
		newItem.GetComponent<ItemTextureControls>().InventorySlot = slot;
		
		//RecalcStats();
	}
	
		// Adds the GUITexture item described by "data" in the given slot
	private void AddItemEquipment(int slot, ItemData data)
	{
		GUITexture newItem = Instantiate(ItemGUITexture) as GUITexture;
		newItem.texture = Resources.Load(data.itemType.ToString()+" Icon") as Texture;
		
		// create copying mechanic?
		newItem.GetComponent<ItemData>().SetValues(data);
		
		newItem.transform.parent = _equippedSlots[slot].transform;
		newItem.pixelInset = 
			new Rect(_equippedSlots[slot].pixelInset.xMin, _equippedSlots[slot].pixelInset.yMin, 
				_equippedSlots[slot].pixelInset.width, _equippedSlots[slot].pixelInset.height);
		newItem.gameObject.SetActive(true);
		newItem.transform.localPosition = new Vector3(0,0,2);
		_equippedItems[slot] = newItem;
		newItem.GetComponent<ItemTextureControls>().EquipmentSlot = slot;
		
		RecalcStats();
	}
	
	// Spawns the GUITexture item described by "data" hanging under the cursor
	private void SpawnItem(ItemData data)
	{			
		GUITexture newItem = Instantiate(ItemGUITexture) as GUITexture;
		newItem.texture = Resources.Load(data.itemType.ToString()+" Icon") as Texture;
		
		// create copying mechanic
		newItem.GetComponent<ItemData>().SetValues(data);
		
		newItem.pixelInset = 
			new Rect(Input.mousePosition.x, Input.mousePosition.y, 
				_inventorySlots[0].pixelInset.width, _inventorySlots[0].pixelInset.height);
		newItem.gameObject.SetActive(true);
		newItem.transform.localPosition = new Vector3(0,0,2);
		
		newItem.GetComponent<ItemTextureControls>().PickedUp = true;
		newItem.GetComponent<ItemTextureControls>().InventorySlot = -1;
		_itemPickedUp = newItem;
	}
	
	// Called when an item is picked up from the inventory or from the ground
	public void ItemPickedUpInventory(int inventorySlotNumber)
	{
		_items[inventorySlotNumber].transform.parent = MenuBoxBackground.transform;
		_items[inventorySlotNumber].transform.localPosition= new Vector3(0,0,3);
		_itemPickedUp = _items[inventorySlotNumber];
		_items[inventorySlotNumber] = null;
		
		RecalcStats();
	}
		
	public void ItemPickedUpEquipment(int equipmentSlotNumber)
	{
		_equippedItems[equipmentSlotNumber].transform.parent = MenuBoxBackground.transform;
		_equippedItems[equipmentSlotNumber].transform.localPosition= new Vector3(0,0,3);
		_itemPickedUp = _equippedItems[equipmentSlotNumber];
		_equippedItems[equipmentSlotNumber] = null;
		
		if(equipmentSlotNumber == 4 || equipmentSlotNumber == 5)
		{
			UpdateHandheldItemSprites();	
		}
		
		RecalcStats();
	}
	
	// returns true if item fits slot; false otherwise
	private bool EquipmentSlotCheckItemCompatability(int slotNumber, ItemClass itemClass)
	{
		//returnVal = false;
		switch(slotNumber)
		{
			// Helmet
		case 0:
			if(itemClass == ItemClass.Helmet)
			{
				return true;	
			}
			break;
			// Cloak
		case 1:		
			if(itemClass == ItemClass.Cloak)
			{
				return true;	
			}
			break;
			// Gloves			
		case 2:		
			if(itemClass == ItemClass.Gloves)
			{
				return true;	
			}
			break;
			// Armor 			
		case 3:		
			if(itemClass == ItemClass.Armor)
			{
				return true;	
			}
			break;
			// Main-hand (1h or 2h weapon)		
		case 4:		
			if(itemClass == ItemClass.Melee1H)
			{
				return true;	
			}
			else if((itemClass == ItemClass.Melee2H || itemClass == ItemClass.Ranged2H) 
				&& _equippedItems[5] == null)
			{
				return true;	
			}
			break;
			// off-hand (shielf or second weapon)		
		case 5:		
			if(itemClass == ItemClass.OffHand || itemClass == ItemClass.Melee1H)
			{
				GUITexture mainhandItem = _equippedItems[4];
				if(mainhandItem == null)
				{
					return true;	
				}
				else if(!TwoHandedItemEquipped())
				{
					return true;
				}
			}
			break;
			// boots		
		case 6:		
			if(itemClass == ItemClass.Boots)
			{
				return true;	
			}
			break;
		}
		
		return false;
	}
	
	// Called when an invetory held under the mouse cursor is dropped
	// Returns true if successful
	public bool AttemptItemPutDown(GUITexture item)
	{
		int inventorySlot = InventorySlotUnderPoint(
			new Vector2(Input.mousePosition.x, Input.mousePosition.y));
		int equipmentSlot = EquipmentSlotUnderPoint(
			new Vector2(Input.mousePosition.x, Input.mousePosition.y));
		
		// add item to inventory
		if(inventorySlot != -1)
		{
			// SWAP if necessary
			if(_items[inventorySlot] != null)
			{
				_items[inventorySlot].GetComponent<ItemTextureControls>().InventorySlot = -1;
				_items[inventorySlot].GetComponent<ItemTextureControls>().PickedUp = true;
				ItemPickedUpInventory(inventorySlot);
			}
			else
			{
				_itemPickedUp = null;
			}
			
			// Place item in inventory
			_items[inventorySlot] = item;
			item.transform.parent = _inventorySlots[inventorySlot].transform;
			item.transform.localPosition = new Vector3(0,0,2);
			item.pixelInset = 
			new Rect(_inventorySlots[inventorySlot].pixelInset.xMin, 
					_inventorySlots[inventorySlot].pixelInset.yMin, 
					_inventorySlots[inventorySlot].pixelInset.width, 
					_inventorySlots[inventorySlot].pixelInset.height);			
			item.GetComponent<ItemTextureControls>().InventorySlot = inventorySlot;
		}
		// place in equipment
		else if(equipmentSlot != -1)
		{
			// wrong equipment slot clicked; do nothing
			if(! EquipmentSlotCheckItemCompatability(equipmentSlot, 
				item.GetComponent<ItemData>().itemClass))
			{
				return false;	
			}
			
			// SWAP if necessary
			if(_equippedItems[equipmentSlot] != null)
			{
				_equippedItems[equipmentSlot].GetComponent<ItemTextureControls>().EquipmentSlot = -1;
				_equippedItems[equipmentSlot].GetComponent<ItemTextureControls>().PickedUp = true;
				ItemPickedUpEquipment(equipmentSlot);
			}
			else
			{
				_itemPickedUp = null;
			}
			
			// Place item in equipment
			_equippedItems[equipmentSlot] = item;
			item.transform.parent = _equippedSlots[equipmentSlot].transform;
			item.transform.localPosition = new Vector3(0,0,2);
			item.pixelInset = 
			new Rect(_equippedSlots[equipmentSlot].pixelInset.xMin, 
					_equippedSlots[equipmentSlot].pixelInset.yMin, 
					_equippedSlots[equipmentSlot].pixelInset.width, 
					_equippedSlots[equipmentSlot].pixelInset.height);
			item.GetComponent<ItemTextureControls>().EquipmentSlot = equipmentSlot;
			
			if(equipmentSlot == 4 || equipmentSlot == 5)
			{
				UpdateHandheldItemSprites();	
			}
		}
		// drop item
		else
		{
			OTSprite newpickup = OT.CreateObject("pickup").GetComponent<OTSprite>();
			
			newpickup.position = 
					self.position - new Vector2(10,0);
			newpickup.gameObject.AddComponent<ItemData>();
			newpickup.GetComponent<ItemData>().SetValues(item.GetComponent<ItemData>());
			newpickup.frameName = newpickup.GetComponent<ItemData>().itemType.ToString();
			
			_itemPickedUp = null;
			
			Destroy(item.gameObject);
		}
		
		RecalcStats();
		return true;
	}
	
	// Returns the number of the inventory slot under the mouse cursor
	// Returns -1 if cursor not above an inventory slot
	private int InventorySlotUnderPoint(Vector2 point)
	{
		float adjustedX = point.x - Screen.width/2;
		float adjustedY = point.y;
		
		// don't recalc every time...
		float baseSlotHeight = Screen.height/(CONSTANTS.BaseInventorySize/CONSTANTS.BaseInventoryRows);
		float baseSlotWidth = Screen.width/(2*CONSTANTS.BaseInventorySize/CONSTANTS.BaseInventoryRows);
		
		if(adjustedX < 0 || adjustedX > Screen.width/2 || adjustedY < 0 
			|| adjustedY > CONSTANTS.BaseInventoryRows*baseSlotHeight)
		{
			return -1;
		}
		
		return (CONSTANTS.BaseInventorySize/CONSTANTS.BaseInventoryRows)*
			((int)(adjustedY / baseSlotHeight)) + (int)(adjustedX / baseSlotWidth);
	}
	
	private bool PointInBox(Vector2 point, Rect box)
	{
		return (box.xMin < point.x && box.xMax > point.x 
			&& box.yMin < point.y && box.yMax > point.y);	
	}
	
	private int EquipmentSlotUnderPoint(Vector2 point)
	{
		for(int i=0; i< _equippedSlots.Length; ++i)
		{
			if(PointInBox(point, _equippedSlots[i].pixelInset))
			{
				return i;	
			}
		}
		
		return -1;
	}
	
	// Tries to place a ground item in the invetory
	// Returns false if inventory is full
	private bool TryToPlacePickupInInventory(OTSprite pickup)
	{
		for(int i=0; i<CONSTANTS.BaseInventorySize; ++i)
		{
			if(_items[i] == null)
			{
				AddItemInventory(i, pickup.GetComponent<ItemData>());
				return true;
			}
		}
		
		return false;
	}
	
	#endregion
	
	#region LOS
	
	private bool[,] _LOSMap;
	
	// integrate with other stats...
	private int _sightRange = CONSTANTS.BaseSightRange;
	
	private OTSprite[,] _tiles;

	private List<Vector2> _LOSlist = new List<Vector2>();
	
	private OTSprite[,] _fogOfWar;
	
	// Basic sub-optimal LOS algo; fix later...
	private void UpdateLOS()
	{
		int coordX = (int) (self.position.x/CONSTANTS.TileSize);
		int coordY = (int) (self.position.y/CONSTANTS.TileSize);
		
		List<Vector2> LOSnew = new List<Vector2>();
		
		for (int i= (-_sightRange); i<=_sightRange; ++i)
		{
			for (int j= (-_sightRange); j<=_sightRange; ++j)
			{
				int x = coordX+i;
				int y = coordY+j;
				
				if(x < 0 || x >= _tiles.GetLength(0) || y < 0 || y >= _tiles.GetLength(1))
				{
					// out of bounds...
					continue;
				}
				
				if(Mathf.Pow(Mathf.Pow(i,2)+Mathf.Pow(j,2), 0.5f) > _sightRange)
				{
					// out of sight range...
					continue;	
				}
				
				OTSprite target = _tiles[x, y];
				
				RaycastHit losRay;
		
				Vector3 startPoint = new Vector3(self.transform.position.x, 
					self.transform.position.y, 40);
				
				Vector2 targetxy = GetNearestCorner(target);
				
				Vector3 endPoint = new Vector3(targetxy.x, targetxy.y, 40);
				Vector3 dir = endPoint - startPoint;
				float distance = Vector3.Distance(startPoint, endPoint);
				
				Physics.Raycast(startPoint, dir, out losRay, Mathf.Max (1f,distance-30f));
		
				// OBSCURED
				if(losRay.rigidbody != null && losRay.rigidbody != target.rigidbody)
				{
					_LOSMap[x,y] = false;
				}
				// VISIBLE
				else
				{
					LOSnew.Add(new Vector2(x,y));
					if(!_LOSMap[x,y])
					{
						// run obscured->visible update
						ObscuredToVisibleUpdate(x,y);
						
						_LOSMap[x,y] = true;
					}
				}
			}
		}
		
		foreach (Vector2 cell in _LOSlist)
		{
			if(! LOSnew.Contains(cell))
			{
				// run visible -> obscured update
				VisibleToObscuredUpdate((int) cell.x, (int) cell.y);
				_LOSMap[(int) cell.x, (int) cell.y] = false;
			}
		}
		
		foreach (Vector2 cell in LOSnew)
		{	
			VisibilityUpdateAlpha((int)cell.x, (int)cell.y);
		}
		
		_LOSlist = LOSnew;
	}
	
	// returns the nearest (to the player) corner of a given tile
	private Vector2 GetNearestCorner(OTSprite target)
	{
		if(target.collider == null)
		{
			return new Vector2(target.transform.position.x, target.transform.position.y);	
		}
		
		Bounds bnds = target.collider.bounds;
		Vector3 dir = bnds.center - self.transform.position;
		float angle = CreatureManager.FindAngleFacing(new Vector2(dir.x, dir.y));
		
		//.........
		angle += 720f;
		angle = angle % 360f;
		
		//print(angle);
		
		Vector2 returnVal;
		if(0f < angle && angle <= 90f)
		{
			returnVal = new Vector2(bnds.max.x, bnds.min.y);	
		}
		else if(90f < angle && angle <= 180f)
		{
			returnVal = new Vector2(bnds.max.x, bnds.max.y);	
		}
		else if(180f < angle && angle <= 270f)
		{
			returnVal = new Vector2(bnds.min.x, bnds.max.y);	
		}
		else
		{
			returnVal = new Vector2(bnds.min.x, bnds.min.y);	
		}
		
		return returnVal;
	}
	
	private void VisibilityUpdateAlpha(int x, int y)
	{
		_fogOfWar[x,y].alpha = 1-VisibilityLevel(
			_tiles[x,y].position, self.position);
	}
	
	private void ObscuredToVisibleUpdate(int x, int y)
	{
		//_fogOfWar[x,y].alpha = 0;
		_fogOfWar[x,y].alpha = 1-VisibilityLevel(
			_tiles[x,y].position, self.position);
	}
	
	private void VisibleToObscuredUpdate(int x, int y)
	{
		//_fogOfWar[x,y].alpha = CONSTANTS.ExploredFogAlpha;
		_fogOfWar[x,y].alpha = 1;
	}
	
	private float VisibilityLevel(Vector2 tilePosition, Vector2 heroPosition)
	{
		float distance = Vector2.Distance(tilePosition, heroPosition);
		distance = distance / CONSTANTS.TileSize;
		
		
		if(distance < CONSTANTS.BaseClearVisibilityRange)
		{
			return 1f;	
		}
		
		return (1f- (distance - CONSTANTS.BaseClearVisibilityRange) / 
			(_sightRange - CONSTANTS.BaseClearVisibilityRange));
	}
	
	#endregion
	
	#region Save/Load
		
	private void SaveHero()
	{	
		System.IO.StreamWriter file = new System.IO.StreamWriter("HeroData.txt");
		
		file.WriteLine(_heroClass.ToString());
		file.WriteLine(_heroName);
		file.WriteLine(_strength.ToString());
		file.WriteLine(_agility.ToString());
		file.WriteLine(_magic.ToString());
		file.WriteLine(_level.ToString());
		file.WriteLine(_xp.ToString());
		
		SaveSkills(file);
		
		SaveInventory(file);
		
		file.Close();
	}
	
	private void LoadHero()
	{		
		System.IO.StreamReader file = new System.IO.StreamReader("HeroData.txt");
		
		_heroClass = int.Parse(file.ReadLine());
		_heroName = file.ReadLine();		
		
		if(file.Peek() != -1)
		{
			_strength = int.Parse(file.ReadLine());
			_agility = int.Parse(file.ReadLine());
			_magic = int.Parse(file.ReadLine());
			_level = int.Parse(file.ReadLine());
			_xp = int.Parse(file.ReadLine());
		}
		else
		{
			_level = 1;
			_xp = 0;
		
			switch(_heroClass)
			{
			case 0:
				_strength = 10;
				_agility = 5;
				_magic = 1;
				break;
			case 1:
				_strength = 5;
				_agility = 10;
				_magic = 3;
				break;
			case 2:
				_strength = 2;
				_agility = 2;
				_magic = 100;
				break;
			}	
		}
		
		LoadSkills(file);
		
		LoadInventory(file);
		
		file.Close();	
		
		RecalcStats();
	}
	
	private void SaveSkills(System.IO.StreamWriter file)
	{
		string skills = "";
		for (int i=0; i<_skillValues.Length; ++i)
		{
			if(_skillValues[i])
			{
				skills += "1";	
			}
			else
			{
				skills+= "0";	
			}
		}
		
		file.WriteLine(skills);
		
		file.WriteLine(_quickSkills[0]);
		file.WriteLine(_quickSkills[1]);
		file.WriteLine(_quickSkills[2]);
		file.WriteLine(_quickSkills[3]);
		file.WriteLine(_quickSkills[4]);
	}
	
	private void LoadSkills(System.IO.StreamReader file)
	{
		string skills = file.ReadLine();
				
		for (int i=0; i<_skillValues.Length; ++i)
		{
			if(skills[i] == '1')
			{
				_skillValues[i] = true;	
			}
			else
			{
				_skillValues[i] = false;
			}
		}
		
		_quickSkills[0] = int.Parse(file.ReadLine());
		_quickSkills[1] = int.Parse(file.ReadLine());
		_quickSkills[2] = int.Parse(file.ReadLine());
		_quickSkills[3] = int.Parse(file.ReadLine());
		_quickSkills[4] = int.Parse(file.ReadLine());
	}
	
	private void SaveInventory(System.IO.StreamWriter file)
	{
		// store inventory
		for (int i=0; i<_items.Length; ++i)
		{
			if(_items[i] != null)
			{
				file.WriteLine(i);
				_items[i].GetComponent<ItemData>().SaveToFile(file);
			}
		}
		
		// store equipment
		for (int i=0; i<_equippedItems.Length; ++i)
		{
			if(_equippedItems[i] != null)
			{
				file.WriteLine(_items.Length + i);
				_equippedItems[i].GetComponent<ItemData>().SaveToFile(file);
			}
		}
	}
	
	private void LoadInventory(System.IO.StreamReader file)
	{
		while(file.Peek() != -1)
		{
			int i = int.Parse(file.ReadLine());
			// item in inventory
			if(i < _items.Length)
			{
				GameObject placeholder = new GameObject();
				placeholder.AddComponent<ItemData>();
				placeholder.GetComponent<ItemData>().LoadFromFile(file);
				AddItemInventory(i, placeholder.GetComponent<ItemData>());
				Destroy(placeholder);
			}
			// item equipped
			else
			{
				i = i % _items.Length;
				GameObject placeholder = new GameObject();
				placeholder.AddComponent<ItemData>();
				placeholder.GetComponent<ItemData>().LoadFromFile(file);
				AddItemEquipment(i, placeholder.GetComponent<ItemData>());
				Destroy(placeholder);
			}
		}
	}
	
	#endregion

}
