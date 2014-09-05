using UnityEngine;
using System.Collections;

public class PickupManager : MonoBehaviour {
	
	public OTAnimatingSprite Hero;
	public Texture background;
	
	private bool _hovering = false;
	
	void Start()
	{
		//GetComponent<OTSprite>().onMouseEnterOT = OnMouseEnterOT;	
		//GetComponent<OTSprite>().onMouseExitOT = OnMouseExitOT;
	}
	
	void OnMouseEnter()	
	{
		Hero.GetComponent<HeroManager>().SetItemUnderMouse(GetComponent<OTSprite>());
		GetComponent<OTSprite>().tintColor = Color.red;
		_hovering = true;
	}
	
	void OnMouseExit()
	{
		Hero.GetComponent<HeroManager>().SetItemUnderMouse(null);
		GetComponent<OTSprite>().tintColor = Color.white;
		_hovering = false;
	}
	
	private int _verticalOffset = CONSTANTS.ItemTextVerticalOffset;
	private float _baseWidth = CONSTANTS.ItemTextBaseWidth;
	
	void OnGUI()
	{
		if(_hovering && Input.GetKey(KeyCode.Z))
		{
			ItemData itemData = GetComponent<ItemData>();	
			if(itemData == null)
			{
				return;	
			}
			
			//int lines = 3 + itemData.ItemBonuses.Length;

			float boxXval = Input.mousePosition.x;
			float boxYval = Screen.height - Input.mousePosition.y;
			
			if((boxYval + 9*_verticalOffset) > Screen.height)
			{
				boxYval -= (_verticalOffset*9);
			}
			
			if((boxXval+_baseWidth) > Screen.width)
			{
				boxXval	-= (_baseWidth);
			}		
	
			GUI.Label (new Rect(boxXval, boxYval, _baseWidth, _verticalOffset*9),
				background);
			
			GUI.contentColor = Color.black;
			GUI.Label (new Rect(boxXval, boxYval, _baseWidth, _verticalOffset),
				itemData.ItemName.ToString());
			boxYval+=_verticalOffset;
			
			GUI.contentColor = Color.red;
			GUI.Label (new Rect(boxXval, boxYval, _baseWidth, _verticalOffset), 
				itemData.itemType.ToString());
			boxYval+=_verticalOffset;
			
			GUI.contentColor = Color.green;
			GUI.Label (new Rect(boxXval, boxYval, _baseWidth, _verticalOffset), 
				itemData.itemClass.ToString());
			boxYval+=_verticalOffset;
			
			ItemBonusType[] itemBonuses = itemData.ItemBonuses;
			float[] itemBonusValues = itemData.ItemBonusValues;
			
			GUI.contentColor = Color.blue;
			for(int i=0; i< itemBonuses.Length; ++i)
			{
				GUI.Label (new Rect(boxXval, boxYval, _baseWidth, _verticalOffset), 
					itemBonuses[i].ToString() + ": " + itemBonusValues[i].ToString());
				boxYval+=_verticalOffset;
			}
			
		}
	}
}
