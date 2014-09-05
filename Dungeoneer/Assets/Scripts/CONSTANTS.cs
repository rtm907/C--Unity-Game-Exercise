using UnityEngine;
using System.Collections;

public class CONSTANTS : MonoBehaviour {

	// MonsterManager
	public static float MonsterActivationThreshold = 400f;
	public static float MonsterAttackRangeMelee = 60f;
	public static float MonsterAttackRangeAxethrow = 400f;
	
		// OrcGrunt
	public static float MonsterGruntSpeed = 150f;
	public static int MonsterGruntMaxHP = 50;
	public static float MonsterGruntAttack =  10f;
	public static float MonsterGruntRange = 50f;
		// Axeman
	public static float MonsterAxemanSpeed = 175f;
	public static int MonsterAxemanMaxHP = 20;
	public static float MonsterAxemanAttack =  5f;
	public static float MonsterAxemanRange = 200f;
		// Spider1
	public static float MonsterSpider1Speed = 225f;
	public static int MonsterSpider1MaxHP = 40;
	public static float MonsterSpider1Attack =  15f;
	public static float MonsterSpider1Range = 65f;
		// Ogre
	public static float MonsterOgreSpeed = 120f;
	public static int MonsterOgreMaxHP = 100;
	public static float MonsterOgreAttack =  30f;
	public static float MonsterOgreRange = 50f;
		// Furbeast
	public static float MonsterFurbeastSpeed = 80f;
	public static int MonsterFurbeastMaxHP = 150;
	public static float MonsterFurbeastAttack =  40f;
	public static float MonsterFurbeastRange = 50f;
		// Scorpion
	public static float MonsterScorpionSpeed = 150f;
	public static int MonsterScorpionMaxHP = 50;
	public static float MonsterScorpionAttack =  20f;
	public static float MonsterScorpionRange = 50f;
	
	
	// MonsterStatManager
	public static float MonsterBaseSpeed = 1.5f;
	public static int MonsterBaseMaxHP = 30;
	public static float MonsterBaseAttack = 5f;
	
	// DungeonGenerator
		// rooms
	public static float RoomSearchIntensity = 10f;
	public static int BaseCorridorWidth = 2;
	
		// cave
	public static float CaveBaseDensity = 0.55f;
	public static int CaveCAIterations = 5;
	public static float CaveGoonDensity = 0.01f;
	public static float CaveChestDensity = 0.01f;
	
		// catacombs
	public static float CatacombsLowDensityThreshold = 0.6f;
	
	// GameManager
	public static int BaseTilesX = 30;
	public static int BaseTilesY = 30;
	public static int BaseRoomX = 10;
	public static int BaseRoomY = 10;	
	public static float BaseZoom = 2f;
	public static int TileSize = 64;
	
	public static string[] TileNames = new string[]{"wall", "dirt"};
	
	public static float MapDetailFrequency = 0.04f;
	
	// HeroManager
	
		//inventory
	public static int BaseInventorySize = 16;
	public static int BaseInventoryRows = 2;
	
	public static int ItemTextVerticalOffset = 20;
	public static float ItemTextBaseWidth = 180f;
	
	public static int BaseEquipmentSize = 7;
	
	// skill window
	public static int BaseNumberOfSkills = 15;
	public static int BaseNumberOfSkillRows = 3;
	
	public static string[] SkillNames = 
	{"Melee", "Shield", "Crush", "Leap", "Flurry",    // WARRIOR
	"Ranged", "Dodge", "Snipe", "Slink", "Multishot", // ARCHER
	"Magic", "Wall", "Burn", "Teleport", "Fireball"}; // MAGE
	
	public static Vector3[] SkillReqs = 
	{new Vector3(1,1,1), new Vector3(1,1,1), new Vector3(1,1,1), 
		new Vector3(1,1,1), new Vector3(1,1,1), // WARRIOR
		new Vector3(1,1,1), new Vector3(1,1,1), new Vector3(1,1,1),
		new Vector3(1,1,1), new Vector3(1,1,1), // ARCHER
		new Vector3(1,1,1), new Vector3(1,1,1), new Vector3(1,1,1),
		new Vector3(1,1,1), new Vector3(1,1,1) // MAGE
	};
	
	public static float[] SkillCooldowns = 
	{
		0.5f, 0.1f, 1.0f, 2.0f, 5.0f, // WARRIOR
		0.4f, 0.1f, 1.0f, 3.0f, 5.0f, // ARCHER
		0.45f, 0.1f, 1.0f, 2.0f, 5.0f // MAGE		
	};
	
	// skills
	
	//defense
	public static float SkillShieldBlockBonus = 0.4f;
	public static float SkillShieldMovespeedModifier = 0.6f;
	
	public static float SkillDodgeDodgeBonus = 0.3f;
	public static float SkillDodgeMovespeedModifier = 0.8f;
	
	// stun/nuke
	public static float SkillCrushRange = 80f;
	public static float SkillCrushDamageMP = 0.01f;
	public static float SkillCrushStunDuration = 4f;
	
	public static float SkillSnipeRange = 300f;
	public static float SkillSnipeDamageMP = 0.01f;
	public static float SkillSnipeSlowDuration = 5f;
	public static float SkillSnipeSlowMagnitude = 0.1f;
	
	public static float SkillBurnRange = 200f;
	public static float SkillBurnDamageMP = 3f;
	
	// teleport
	public static float SkillTeleportRange = 400f;
	public static float SkillLeapRange = 400f;
	public static float SkillSlinkRange = 400f;
	
	// AoE
	public static float SkillFireballSpeedMP = 2f;
	public static float SkillFireballRange = 800f;
	public static float SkillFireballDamageMP = 2f;
	
	public static float SkillFlurryDamageMP = 2f;
	
	public static float SkillMultishotRange = 500f;
	public static float SkillMultishotDamageMP = 2f;
	public static float SkillMultishotArrows = 7;
	public static float SkillMultishotHalfarcDeg = 15f;
	
	public static int BaseXPPerLevel = 100;
	
		// range & projectile speed
	public static int BaseMeleeProjectileSpeed = 2;
	public static int BaseMeleeRange = 30;
	public static int BaseRangedProjectileSpeed = 5;
	public static int BaseRangedRange = 200;
	public static int BaseMagicProjectileSpeed = 3;
	public static int BaseMagicRange = 300;
	
		// los
	public static bool DEBUGEnableVisibility = false;
	
	public static float ExploredFogAlpha = 0.6f;
	public static int BaseSightRange = 10;
	public static float PickUpRadius = 50f;
	
	public static float BaseClearVisibilityRange = 5;
	
	public static float LOSUpdateInterval = 0.5f;
	
	public static float BaseAttackAnimDuration = 0.5f;
	
	public static float BaseFootStep = 0.15f;
	public static float BaseFootStepAplitude = 0.2f;
}
