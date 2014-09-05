using UnityEngine;
using System.Collections;
using System.Collections.Generic;

public class testBotManager : MonoBehaviour {
	
	private Vector2? globalTarget = null;
	
	private Vector2? currentTarget = null;
	
	private Stack<Vector2> route = new Stack<Vector2>();
	
	private int[,] _dungeon;
	
	private OTSprite _self;
	
	private OTSprite _targetEffect;
	
	// Use this for initialization
	void Start () {
		_self = GetComponent<OTSprite>();
	}
	
	public void SetDungeon(int[,] dungeon)
	{
		_dungeon = dungeon;		
	}
	
	private void SpawnEffect(Vector2 position)
	{
		GameObject temp = OT.CreateObject("visualEffect");
		OTSprite sprite = temp.GetComponent<OTSprite>();
		sprite.tintColor = Color.blue;
		sprite.position = position;
		_targetEffect = sprite;
	}
	
	// Update is called once per frame
	void Update () {
	
		if(_dungeon == null)
		{
			return;	
		}
		
		if( route.Count == 0)
		{
			// obtain new globalTarget
			int x = Random.Range(1, _dungeon.GetLength(0)-1);
			int y = Random.Range(1, _dungeon.GetLength(1)-1);
			while(_dungeon[x,y] == (int) TileContent.wall)
			{
				x = Random.Range(1, _dungeon.GetLength(0)-1);
				y = Random.Range(1, _dungeon.GetLength(1)-1);	
			}
			
			globalTarget = new Vector2(x,y);
			
			// obtain route
			route = Pathfinder.FindPath(_dungeon, (int) Mathf.Round(_self.position.x/CONSTANTS.TileSize),
				(int) Mathf.Round(_self.position.y/CONSTANTS.TileSize), x,y);
			
			if(route == null)
			{
				//print ("ERROR??!?");
				return;
			}
			
			//print(route.Count);
			if(_targetEffect != null)
			{
				OT.DestroyObject(_targetEffect);
			}
			SpawnEffect(globalTarget.Value*CONSTANTS.TileSize);
			
			currentTarget = route.Pop();
		}
		else
		{
			// check if close enough to target
			Vector2 diff = new Vector2((currentTarget.Value.x*CONSTANTS.TileSize-_self.position.x),
				(currentTarget.Value.y*CONSTANTS.TileSize-_self.position.y));
			if(diff.magnitude < 5)
			{
				currentTarget = route.Pop();	
			}
			else
			{
				// if too far, advance...
				diff.Normalize();
				_self.position += 500f* diff*Time.deltaTime;
			}
			
		}
		
	}
}
