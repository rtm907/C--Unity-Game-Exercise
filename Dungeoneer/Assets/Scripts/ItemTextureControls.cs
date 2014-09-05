using UnityEngine;
using System.Collections;

public class ItemTextureControls : MonoBehaviour {
	
	public OTAnimatingSprite Hero;
	public Texture background;	
	
	private bool _pickedUp = false;
	
	private bool _underMouse = false;
	// -1: not in inv; 0-InventorySize: slot number
	private int _inventorySlot = -1;
	private int _equipmentSlot = -1;
	
	
	public int InventorySlot
	{
		get
		{
			return _inventorySlot;	
		}
		set
		{
			_inventorySlot = value;	
		}
	}
	
	public int EquipmentSlot
	{
		get
		{
			return _equipmentSlot;	
		}
		set
		{
			_equipmentSlot = value;	
		}
	}
	
	public bool PickedUp
	{
		get
		{
			return _pickedUp;	
		}
		set
		{
			_pickedUp = true;	
		}
	}
	
	// Update is called once per frame
	void Update () {
	
		if(_pickedUp)
		{
			GUITexture itemTexture = GetComponent<GUITexture>();
			Vector2 mouseposition = Input.mousePosition;
			itemTexture.pixelInset = new Rect(mouseposition.x-itemTexture.pixelInset.width/2, 
				mouseposition.y-itemTexture.pixelInset.height/2,
				itemTexture.pixelInset.width, itemTexture.pixelInset.height);
		}
	}
	
	void OnMouseUp()
	{
		_pickedUp = !_pickedUp;
		
		if(_pickedUp)
		{
			if(_inventorySlot != -1)
			{
				Hero.GetComponent<HeroManager>().ItemPickedUpInventory(_inventorySlot);
				_inventorySlot = -1;
			}
			else
			{
				Hero.GetComponent<HeroManager>().ItemPickedUpEquipment(_equipmentSlot);
				_equipmentSlot = -1;	
			}
		}
		else
		{
			if(
				! Hero.GetComponent<HeroManager>().AttemptItemPutDown(GetComponent<GUITexture>())
				)
			{
				// UNDO...
				_pickedUp = !_pickedUp;
			}
		}
	}
	
	void OnMouseEnter()
	{
		_underMouse = true;		
	}
	
	void OnMouseExit()
	{
		_underMouse = false;
	}
	
	private int _verticalOffset = CONSTANTS.ItemTextVerticalOffset;
	private float _baseWidth = CONSTANTS.ItemTextBaseWidth;
	
	void OnGUI()
	{
		if(_underMouse)
		{
			GUITexture item = GetComponent<GUITexture>();
			if(item == null)
			{
				return;	
			}
			
			DisplayItemInfo();
		}
	}
	
	private void DisplayItemInfo()
	{
		GUITexture item = GetComponent<GUITexture>();
		
		//int lines = 3 + GetComponent<ItemData>().ItemBonuses.Length;

		float boxXval = item.pixelInset.xMax;
		float boxYval = Screen.height - (item.pixelInset.yMin)// + item.pixelInset.height) 
			- _verticalOffset*9;
		
		if(boxYval < 0)
		{
			boxYval += (_verticalOffset*9);
		}
		
		if((boxXval+_baseWidth) > Screen.width)
		{
			boxXval	-= (item.pixelInset.width+_baseWidth);
		}
		
		GUI.Label (new Rect(boxXval, boxYval, _baseWidth, _verticalOffset*9),
			background);
		
		GUI.contentColor = Color.black;
		GUI.Label (new Rect(boxXval, boxYval, _baseWidth, _verticalOffset),
			GetComponent<ItemData>().ItemName.ToString());
		boxYval+=_verticalOffset;
		
		GUI.contentColor = Color.red;
		GUI.Label (new Rect(boxXval, boxYval, _baseWidth, _verticalOffset), 
			GetComponent<ItemData>().itemType.ToString());
		boxYval+=_verticalOffset;
		
		GUI.contentColor = Color.green;
		GUI.Label (new Rect(boxXval, boxYval, _baseWidth, _verticalOffset), 
			GetComponent<ItemData>().itemClass.ToString());
		boxYval+=_verticalOffset;
		
		ItemBonusType[] itemBonuses = GetComponent<ItemData>().ItemBonuses;
		float[] itemBonusValues = GetComponent<ItemData>().ItemBonusValues;
		
		GUI.contentColor = Color.blue;
		for(int i=0; i< itemBonuses.Length; ++i)
		{
			GUI.Label (new Rect(boxXval, boxYval, _baseWidth, _verticalOffset), 
				itemBonuses[i].ToString() + ": " + itemBonusValues[i].ToString());
			boxYval+=_verticalOffset;
		}
	}
}
