using UnityEngine;
using System.Collections;

public enum ItemType 
{
	swordShort = 0,
	swordLong,
	axeHatchet,
	axeBattle,
	
	swordTwoHanded,
	axeTwoHanded,
	staff,
	bow,
	
	Shield,
		
	Helmet,
	
	Cloak,
	
	Armor,
	Tunic,
	Robe,
	
	Gloves,
	
	Boots,

	LENGTH,
}

public enum ItemClass
{
	Melee1H = 0,
	Melee2H,
	Ranged2H,
	OffHand,
	Helmet,
	Cloak,
	Armor,
	Gloves,
	Boots,
	LENGTH
}

public enum ItemBonusType
{
	PlusStrength = 0,
	PlusAgility = 1,
	PlusMagic = 2,
	PlusMaxHP = 3,
	PlusSpeed = 4,
	LENGTH = 5,
}

public class ItemData : MonoBehaviour {
	
	private string _itemName;
	
	private ItemType _itemType;
	private ItemClass _itemClass;
	
	
	private ItemBonusType[] _itemBonuses;
	private float[] _itemBonusValues;
	
	public string ItemName
	{
		get
		{
			return _itemName;	
		}
	}
	
	public ItemType itemType
	{
		get
		{
			return _itemType;	
		}
	}
	
	public ItemClass itemClass
	{
		get
		{
			return _itemClass;	
		}
	}
	
	public ItemBonusType[] ItemBonuses
	{
		get
		{
			return _itemBonuses;	
		}
	}
	
	public float[] ItemBonusValues
	{
		get
		{
			return _itemBonusValues;	
		}
	}
	
	public void SetValues(ItemData data)
	{
		_itemName = data.ItemName;
		SetType(data.itemType);
		_itemBonuses = data.ItemBonuses;
		_itemBonusValues = data.ItemBonusValues;
	}
	
	public void SetName(string newName)
	{
		_itemName = newName;	
	}
	
	public void SetType(ItemType itemType)
	{
		_itemType = itemType;
		SetClass();
	}
	
	// assumes the item already has a type
	private void SetClass()
	{
		switch(_itemType)
		{
		case ItemType.swordShort:
		case ItemType.swordLong:
		case ItemType.axeHatchet:
		case ItemType.axeBattle:
			_itemClass = ItemClass.Melee1H;
			break;
			
		case ItemType.swordTwoHanded:
		case ItemType.axeTwoHanded:
		case ItemType.staff:
			_itemClass = ItemClass.Melee2H;
			break;
			
		case ItemType.bow:
			_itemClass = ItemClass.Ranged2H;
			break;
			
		case ItemType.Shield:
			_itemClass = ItemClass.OffHand;
			break;
			
		case ItemType.Helmet:
			_itemClass = ItemClass.Helmet;
			break;
				
		case ItemType.Cloak:
			_itemClass = ItemClass.Cloak;
			break;
			
		case ItemType.Armor:
		case ItemType.Tunic:
		case ItemType.Robe:
			_itemClass = ItemClass.Armor;
			break;
			
		case ItemType.Gloves:
			_itemClass = ItemClass.Gloves;
			break;
			
		case ItemType.Boots:
			_itemClass = ItemClass.Boots;
			break;
		}
	}
	
	public void SetBonuses(ItemBonusType[] bonuses)
	{
		_itemBonuses = bonuses;
	}
	
	public void SetBonusValues(float[] bonusValues)
	{
		_itemBonusValues = bonusValues;	
	}
	
	public void SpawnRandom()
	{
		SetType((ItemType) Random.Range(0, (int) ItemType.LENGTH));
		//SetClass();
		_itemName = _itemType.ToString();
		
		int numberOfBonuses = Random.Range(1,6);
		
		_itemBonuses = new ItemBonusType[numberOfBonuses];
		_itemBonusValues = new float[numberOfBonuses];
		for(int i=0; i<numberOfBonuses; ++i)
		{
			_itemBonuses[i] = (ItemBonusType) Random.Range(0, (int) ItemBonusType.LENGTH);
			_itemBonusValues[i] = Random.Range(1f, 10f);
		}
	}
	
	public void SaveToFile(System.IO.StreamWriter file)
	{
		file.WriteLine(_itemName);
		file.WriteLine(((int)_itemType).ToString());
		file.WriteLine(_itemBonuses.Length.ToString());
		for(int i=0; i<_itemBonuses.Length; ++i)
		{
			file.WriteLine(((int)_itemBonuses[i]).ToString());
			file.WriteLine(_itemBonusValues[i].ToString());
		}
	}
	
	public void LoadFromFile(System.IO.StreamReader file)
	{
		_itemName = file.ReadLine();
		SetType((ItemType) int.Parse(file.ReadLine()));
		
		int numberOfBonuses = int.Parse(file.ReadLine());
		
		_itemBonuses = new ItemBonusType[numberOfBonuses];
		_itemBonusValues = new float[numberOfBonuses];
		
		for(int i=0; i<numberOfBonuses; ++i)
		{
			_itemBonuses[i] = (ItemBonusType) int.Parse(file.ReadLine());
			_itemBonusValues[i] = float.Parse(file.ReadLine());
		}
	}
	
	public override string ToString ()
	{
		string returnValue = "Name: " + _itemName + "\n" + "Type: " + _itemType.ToString()
			+ "\n" + "Class: " + _itemClass.ToString();
		
		for(int i=0; i<_itemBonuses.Length; ++i)
		{
			returnValue = returnValue + "\n" + _itemBonuses[i].ToString() + 
				": +" + _itemBonusValues[i].ToString();	
		}
		
		return returnValue;
	}
}
