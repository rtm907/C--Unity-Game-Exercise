using UnityEngine;
using System.Collections;
using System.Collections.Generic;

public enum LevelType
{
	Corridors = 0,
	Cave,
	Lava,
	Catacombs,
	LENGTH
}

public class GameManager : MonoBehaviour {
	
	public OTAnimatingSprite Hero;
	
	public GameObject TileContainer;
	public GameObject MobContainer;
	
	private OTSprite[,] _tiles;
	
	public OTSprite[,] Tiles
	{
		get
		{
			return _tiles;	
		}
	}
	
	private LevelType _levelType;
	
	// Use this for initialization
	void Start () {
		_tiles = new OTSprite[CONSTANTS.BaseTilesX, CONSTANTS.BaseTilesY];
		
		
		OT.view.alwaysPixelPerfect = true;
		OT.view.zoom = CONSTANTS.BaseZoom;
		
		//_levelType = (LevelType) Random.Range(0,2);
		_levelType = LevelType.Catacombs;
		
		int[,] dungeon = null;
		
		if(_levelType == LevelType.Corridors)
		{
			dungeon = DungeonGenerator.
				GenerateDungeonRoomsV2(CONSTANTS.BaseTilesX, CONSTANTS.BaseTilesY, 
					CONSTANTS.BaseRoomX, CONSTANTS.BaseRoomY);
		
		}
		else if(_levelType == LevelType.Cave)
		{
			dungeon = DungeonGenerator.GenerateDungeonCave(CONSTANTS.BaseTilesX, CONSTANTS.BaseTilesY);
		}
		else if(_levelType == LevelType.Lava)
		{
			dungeon = DungeonGenerator.GenerateDungeonLavaIslands
				(CONSTANTS.BaseTilesX,	CONSTANTS.BaseTilesY);
		}
		else if(_levelType == LevelType.Catacombs)
		{
			dungeon = DungeonGenerator.GenerateCatacombsV2(CONSTANTS.BaseTilesX, CONSTANTS.BaseTilesY);	
		}
			
		DungeonGenerator.PrintDungeonToFile(dungeon, "Dungeon.txt");
		
		GenerateTerrain(dungeon);
		PopulateMapWithMobs(dungeon, 7, 4);
	}
	
	private bool _placedMapDetails = false;
	
	// Update is called once per frame
	void Update () {
		 
		if(!_placedMapDetails && OT.ContainersReady())
		{
			PlaceMapDetails(CONSTANTS.MapDetailFrequency);
			
			PlaceDestructibles(0.001f);
			
			if(_levelType == LevelType.Corridors)
			{
				PadWallsWithBorders();
			}
			else if(_levelType == LevelType.Cave)
			{
				PadWallsWithOutcrop();	
			}
			else if(_levelType == LevelType.Lava)
			{
				PadWallsWithOutcrop();	
			}
			else if(_levelType == LevelType.Catacombs)
			{
				PadWallsWithBorders();	
			}
			
			_placedMapDetails = true;
		}
		
		// problem with initialization order :(
		
		
		// ZOOM controls
		if (Input.GetAxis("Mouse ScrollWheel") < 0)
		{
			OT.view.zoom = Mathf.Max(OT.view.zoom - 1f, -2f);
		}
		else if (Input.GetAxis("Mouse ScrollWheel") > 0)
		{
			OT.view.zoom = Mathf.Min(OT.view.zoom + 1f, 2f);
		}
		
	}
	
	//private List<OTSprite> _monsterRoster = new List<OTSprite>();
	
	private void GenerateTerrain(int[,] dungeon)
	{
		for(int j=0; j < CONSTANTS.BaseTilesY; ++j)
		{
			for(int i=0; i < CONSTANTS.BaseTilesX; ++i)	
			{
				GameObject tempobj = null;
				OTSprite newsprite = null;
				
				int currentTile = dungeon[i,j];
				
				if(currentTile <= 5)
				{
					// Place Hero
					if(currentTile == 2)
					{
						Hero.position = new Vector2(i*CONSTANTS.TileSize, j*CONSTANTS.TileSize);
						
						tempobj = OT.CreateObject("StairsUp");
						newsprite = tempobj.GetComponent<OTSprite>();				
						newsprite.position = new Vector2(i*CONSTANTS.TileSize, j*CONSTANTS.TileSize);
						if(_tiles[i,j] != null)
						{
							OT.Destroy(_tiles[i,j]);	
						}
						_tiles[i,j]=newsprite;
						continue;
					}
					
					// Place goons
					if(currentTile == 3)
					{
						/*
						GameObject newgruntie = OT.CreateObject("monster");
						newgruntie.GetComponent<OTSprite>().position 
							= new Vector2(i*CONSTANTS.TileSize, j*CONSTANTS.TileSize);
						newgruntie.GetComponent<MonsterManager>().
							SetMonsterType((MonsterType) Random.Range(0, (int) 2));
						//_monsterRoster.Add(newgruntie.GetComponent<OTSprite>());
						//newgruntie.GetComponent<MonsterManager>().SetCrosshairToInvisible();
						*/
						currentTile = 1;
					}
					
					// Place loot
					if(currentTile == 4)
					{	
						GameObject newchest = OT.CreateObject("chest");
						newchest.GetComponent<OTAnimatingSprite>().position 
							= new Vector2(i*CONSTANTS.TileSize, j*CONSTANTS.TileSize);	
						currentTile = 1;
					}
					// Place exits
					if(currentTile == 5)
					{	
						tempobj = OT.CreateObject("StairsDown");
						newsprite = tempobj.GetComponent<OTSprite>();				
						newsprite.position = new Vector2(i*CONSTANTS.TileSize, j*CONSTANTS.TileSize);
						if(_tiles[i,j] != null)
						{
							OT.Destroy(_tiles[i,j]);	
						}
						_tiles[i,j]=newsprite;
						continue;
					}
					
					// Place tiles
					if(currentTile == (int) TileContent.floor)
					{
						tempobj = OT.CreateObject("dirt");
						newsprite = tempobj.GetComponent<OTSprite>();
						
						if(_levelType == LevelType.Corridors)
						{
							newsprite.frameName = "FloorRooms";
						}
					
						else if(_levelType == LevelType.Cave)
						{
							newsprite.frameName = 
								"dirt"+"0"+(Random.Range(1,5)).ToString();
						}
							
						else if(_levelType == LevelType.Lava)
						{
							newsprite.frameName = "ash";
						}
						else if(_levelType == LevelType.Catacombs)
						{
							newsprite.frameName = "ash";	
						}
					}
					else
					{				
						if(_levelType == LevelType.Corridors)
						{
							tempobj = OT.CreateObject("wall");
							newsprite = tempobj.GetComponent<OTSprite>();
							newsprite.frameName = 
								"wall"+"0"+(Random.Range(1,5)).ToString();
							//newsprite.frameName = "FloorRooms";
						}
					
						else if(_levelType == LevelType.Cave)
						{
							tempobj = OT.CreateObject("wall");
							newsprite = tempobj.GetComponent<OTSprite>();
							newsprite.frameName = 
								"wall"+"0"+(Random.Range(1,5)).ToString();
						}
							
						else if(_levelType == LevelType.Lava)
						{
							tempobj = OT.CreateObject("lava");
							newsprite = tempobj.GetComponent<OTSprite>();
						}
						else if(_levelType == LevelType.Catacombs)
						{
							tempobj = OT.CreateObject("wall");
							newsprite = tempobj.GetComponent<OTSprite>();
							newsprite.frameName = 
								"wall"+"0"+(Random.Range(1,5)).ToString();
						}
					}
					
					newsprite.transform.parent = TileContainer.transform;
					newsprite.position = new Vector2(i*CONSTANTS.TileSize, j*CONSTANTS.TileSize);
					if(_tiles[i,j] != null)
					{
						OT.Destroy(_tiles[i,j]);	
					}
					_tiles[i,j]=newsprite;
					
				}
			}	
		}
		
		//AddTestMob(dungeon);
		//AddTestBot(dungeon);
		//CalibrateMonsters();
		//PlaceMapDetails();
	}
	
	private void PlaceMapDetails(float frequency)
	{
		for (int i=0; i<_tiles.GetLength(0); ++i)
		{
			for(int j=0; j<_tiles.GetLength(1); ++j)
			{
				if(_tiles[i,j].baseName == "dirt")
				{
					float diceRoll = Random.Range(0f, 1f);	
					if(diceRoll < frequency)
					{
						GameObject tempobj = OT.CreateObject("mapDetail");
						OTSprite newsprite = tempobj.GetComponent<OTSprite>();
						newsprite.transform.parent = TileContainer.transform;
						newsprite.position = _tiles[i,j].position + 
							new Vector2(Random.Range(0,24), Random.Range(0,24));
						newsprite.rotation = Random.Range(0,360);
						//print(newsprite.spriteContainer.isReady);
						newsprite.frameIndex = Random.Range(0,5);
						//newsprite.frameName = 
						//	newsprite.spriteContainer.GetFrame(Random.Range(0,4)).name;
						newsprite.depth = -1;
					}
				}
			}
		}
		
	}
	
	private void PlaceDestructibles(float frequency)
	{
		for (int i=0; i<_tiles.GetLength(0); ++i)
		{
			for(int j=0; j<_tiles.GetLength(1); ++j)
			{
				if(_tiles[i,j].baseName == "dirt")
				{
					float diceRoll = Random.Range(0f, 1f);	
					if(diceRoll < frequency)
					{
						GameObject tempobj = OT.CreateObject("destructible");
						OTSprite newsprite = tempobj.GetComponent<OTSprite>();
						newsprite.transform.parent = TileContainer.transform;
						newsprite.position = _tiles[i,j].position + 
							new Vector2(Random.Range(0,24), Random.Range(0,24));
						
						newsprite.depth = -1;
					}
				}
			}
		}
	}
	
	private void PadWallsWithBorders()
	{
		int lengthX = _tiles.GetLength(0);
		int lengthY = _tiles.GetLength(1);
		
		for (int i=0; i< lengthX; ++i)
		{
			for(int j=0; j< lengthY; ++j)
			{
				GameObject tempobj;
				OTSprite newsprite;
				
				if(_tiles[i,j].baseName == "wall")
				{
					if(0<i && 0<j && (_tiles[i-1,j-1].baseName != "wall" ||
						_tiles[i,j-1].baseName != "wall" ||
						_tiles[i-1,j].baseName != "wall"))
					{
						tempobj = OT.CreateObject("wallBorder");
						newsprite = tempobj.GetComponent<OTSprite>();
						newsprite.frameName = "BorderCorner";
						newsprite.position = _tiles[i,j].position;
						//newsprite.flipVertical = true;
						newsprite.flipHorizontal = true;
						
						newsprite.transform.parent = TileContainer.transform;
					}
					
					if(0<i && j< lengthY-1 && (_tiles[i-1,j+1].baseName != "wall" ||
						_tiles[i,j+1].baseName != "wall" ||
						_tiles[i-1,j].baseName != "wall"))
					{
						tempobj = OT.CreateObject("wallBorder");
						newsprite = tempobj.GetComponent<OTSprite>();
						newsprite.frameName = "BorderCorner";
						newsprite.position = _tiles[i,j].position;
						newsprite.flipVertical = true;
						newsprite.flipHorizontal = true;
						
						newsprite.transform.parent = TileContainer.transform;
					}
					
					if(i < lengthX-1 && 0<j && (_tiles[i+1,j-1].baseName != "wall" || 
						_tiles[i,j-1].baseName != "wall" ||
						_tiles[i+1,j].baseName != "wall"))
					{
						tempobj = OT.CreateObject("wallBorder");
						newsprite = tempobj.GetComponent<OTSprite>();
						newsprite.frameName = "BorderCorner";
						newsprite.position = _tiles[i,j].position;
						//newsprite.flipVertical = true;
						//newsprite.flipHorizontal = true;
						
						newsprite.transform.parent = TileContainer.transform;
					}
					
					if(i<lengthX-1 && j<lengthY-1 && (_tiles[i+1,j+1].baseName != "wall" ||
						_tiles[i,j+1].baseName != "wall" ||
						_tiles[i+1,j].baseName != "wall"))
					{
						tempobj = OT.CreateObject("wallBorder");
						newsprite = tempobj.GetComponent<OTSprite>();
						newsprite.frameName = "BorderCorner";
						newsprite.position = _tiles[i,j].position;
						newsprite.flipVertical = true;
						//newsprite.flipHorizontal = true;
						
						newsprite.transform.parent = TileContainer.transform;
					}
					
					if(0<j && _tiles[i,j-1].baseName != "wall")
					{
						tempobj = OT.CreateObject("wallBorder");
						newsprite = tempobj.GetComponent<OTSprite>();
						newsprite.frameName = "BorderSide";
						newsprite.position = _tiles[i,j].position;
						//newsprite.rotation = 270;
						//newsprite.flipVertical = true;
						//newsprite.flipHorizontal = true;
						
						newsprite.transform.parent = TileContainer.transform;
					}
				
					if(0<i && _tiles[i-1,j].baseName != "wall")
					{
						tempobj = OT.CreateObject("wallBorder");
						newsprite = tempobj.GetComponent<OTSprite>();
						newsprite.frameName = "BorderSide";
						newsprite.position = _tiles[i,j].position;
						//newsprite.flipVertical = true;
						//newsprite.flipHorizontal = true;
						newsprite.rotation = 270;
						
						newsprite.transform.parent = TileContainer.transform;
					}
								
					if(j<lengthY-1 && _tiles[i,j+1].baseName != "wall")
					{
						tempobj = OT.CreateObject("wallBorder");
						newsprite = tempobj.GetComponent<OTSprite>();
						newsprite.frameName = "BorderSide";
						newsprite.position = _tiles[i,j].position;
						newsprite.rotation = 180;
						//newsprite.flipVertical = true;
						//newsprite.flipHorizontal = true;
						
						newsprite.transform.parent = TileContainer.transform;
					}
							
					if(i<lengthX-1 && _tiles[i+1,j].baseName != "wall")
					{
						tempobj = OT.CreateObject("wallBorder");
						newsprite = tempobj.GetComponent<OTSprite>();
						newsprite.frameName = "BorderSide";
						newsprite.position = _tiles[i,j].position;
						newsprite.rotation = 90;
						//newsprite.flipVertical = true;
						//newsprite.flipHorizontal = true;
						
						newsprite.transform.parent = TileContainer.transform;
					}
					
				}
			}
		}
	}
		
	private void PadWallsWithOutcrop()
	{
		int lengthX = _tiles.GetLength(0);
		int lengthY = _tiles.GetLength(1);
		
		for (int i=0; i< lengthX; ++i)
		{
			for(int j=0; j< lengthY; ++j)
			{
				GameObject tempobj;
				OTSprite newsprite;
				
				if(_tiles[i,j].baseName == "dirt")
				{
					if(0<i && 0<j && (_tiles[i-1,j-1].baseName == "wall" &&
						_tiles[i,j-1].baseName == "wall" &&
						_tiles[i-1,j].baseName == "wall"))
					{
						tempobj = OT.CreateObject("wallOutcrop");
						newsprite = tempobj.GetComponent<OTSprite>();
						newsprite.frameName = "wallOutcropSW";
						newsprite.position = _tiles[i,j].position;
						
						newsprite.transform.parent = TileContainer.transform;
					}
					else if(0<i && 0<j && (_tiles[i-1,j-1].baseName == "lava" &&
						_tiles[i,j-1].baseName == "lava" &&
						_tiles[i-1,j].baseName == "lava"))
					{
						tempobj = OT.CreateObject("lavaOutcrop");
						newsprite = tempobj.GetComponent<OTSprite>();
						newsprite.frameName = "lavaOutcropSW";
						newsprite.position = _tiles[i,j].position;
						
						newsprite.transform.parent = TileContainer.transform;
					}
					
					if(0<i && j< lengthY-1 && (_tiles[i-1,j+1].baseName == "wall" &&
						_tiles[i,j+1].baseName == "wall" &&
						_tiles[i-1,j].baseName == "wall"))
					{
						tempobj = OT.CreateObject("wallOutcrop");
						newsprite = tempobj.GetComponent<OTSprite>();
						newsprite.frameName = "wallOutcropNW";
						newsprite.position = _tiles[i,j].position;
						
						newsprite.transform.parent = TileContainer.transform;
					}
					else if(0<i && j< lengthY-1 && (_tiles[i-1,j+1].baseName == "lava" &&
						_tiles[i,j+1].baseName == "lava" &&
						_tiles[i-1,j].baseName == "lava"))
					{
						tempobj = OT.CreateObject("lavaOutcrop");
						newsprite = tempobj.GetComponent<OTSprite>();
						newsprite.frameName = "lavaOutcropNW";
						newsprite.position = _tiles[i,j].position;
						
						newsprite.transform.parent = TileContainer.transform;
					}
					
					if(i < lengthX-1 && 0<j && (_tiles[i+1,j-1].baseName == "wall" && 
						_tiles[i,j-1].baseName == "wall" &&
						_tiles[i+1,j].baseName == "wall"))
					{
						tempobj = OT.CreateObject("wallOutcrop");
						newsprite = tempobj.GetComponent<OTSprite>();
						newsprite.frameName = "wallOutcropSE";
						newsprite.position = _tiles[i,j].position;
						
						newsprite.transform.parent = TileContainer.transform;
					}
					else if(i < lengthX-1 && 0<j && (_tiles[i+1,j-1].baseName == "lava" && 
						_tiles[i,j-1].baseName == "lava" &&
						_tiles[i+1,j].baseName == "lava"))
					{
						tempobj = OT.CreateObject("lavaOutcrop");
						newsprite = tempobj.GetComponent<OTSprite>();
						newsprite.frameName = "lavaOutcropSE";
						newsprite.position = _tiles[i,j].position;
						
						newsprite.transform.parent = TileContainer.transform;
					}
					
					if(i<lengthX-1 && j<lengthY-1 && (_tiles[i+1,j+1].baseName == "wall" &&
						_tiles[i,j+1].baseName == "wall" &&
						_tiles[i+1,j].baseName == "wall"))
					{
						tempobj = OT.CreateObject("wallOutcrop");
						newsprite = tempobj.GetComponent<OTSprite>();
						newsprite.frameName = "wallOutcropNE";
						newsprite.position = _tiles[i,j].position;
						
						newsprite.transform.parent = TileContainer.transform;
					}
					else if(i<lengthX-1 && j<lengthY-1 && (_tiles[i+1,j+1].baseName == "lava" &&
						_tiles[i,j+1].baseName == "lava" &&
						_tiles[i+1,j].baseName == "lava"))
					{
						tempobj = OT.CreateObject("lavaOutcrop");
						newsprite = tempobj.GetComponent<OTSprite>();
						newsprite.frameName = "lavaOutcropNE";
						newsprite.position = _tiles[i,j].position;
						
						newsprite.transform.parent = TileContainer.transform;
					}
					
					if(0<j && _tiles[i,j-1].baseName == "wall")
					{
						tempobj = OT.CreateObject("wallOutcrop");
						newsprite = tempobj.GetComponent<OTSprite>();
						newsprite.frameName = "wallOutcropS";
						newsprite.position = _tiles[i,j].position;
						
						newsprite.transform.parent = TileContainer.transform;
					}
					else if(0<j && _tiles[i,j-1].baseName == "lava")
					{
						tempobj = OT.CreateObject("lavaOutcrop");
						newsprite = tempobj.GetComponent<OTSprite>();
						newsprite.frameName = "lavaOutcropS";
						newsprite.position = _tiles[i,j].position;
						
						newsprite.transform.parent = TileContainer.transform;
					}
				
					if(0<i && _tiles[i-1,j].baseName == "wall")
					{
						tempobj = OT.CreateObject("wallOutcrop");
						newsprite = tempobj.GetComponent<OTSprite>();
						newsprite.frameName = "wallOutcropW";
						newsprite.position = _tiles[i,j].position;
						
						newsprite.transform.parent = TileContainer.transform;
					}
					else if(0<i && _tiles[i-1,j].baseName == "lava")
					{
						tempobj = OT.CreateObject("lavaOutcrop");
						newsprite = tempobj.GetComponent<OTSprite>();
						newsprite.frameName = "lavaOutcropW";
						newsprite.position = _tiles[i,j].position;
						
						newsprite.transform.parent = TileContainer.transform;
					}
								
					if(j<lengthY-1 && _tiles[i,j+1].baseName == "wall")
					{
						tempobj = OT.CreateObject("wallOutcrop");
						newsprite = tempobj.GetComponent<OTSprite>();
						newsprite.frameName = "wallOutcropN";
						newsprite.position = _tiles[i,j].position;
						
						newsprite.transform.parent = TileContainer.transform;
					}
					else if(j<lengthY-1 && _tiles[i,j+1].baseName == "lava")
					{
						tempobj = OT.CreateObject("lavaOutcrop");
						newsprite = tempobj.GetComponent<OTSprite>();
						newsprite.frameName = "lavaOutcropN";
						newsprite.position = _tiles[i,j].position;
						
						newsprite.transform.parent = TileContainer.transform;
					}
							
					if(i<lengthX-1 && _tiles[i+1,j].baseName == "wall")
					{
						tempobj = OT.CreateObject("wallOutcrop");
						newsprite = tempobj.GetComponent<OTSprite>();
						newsprite.frameName = "wallOutcropE";
						newsprite.position = _tiles[i,j].position;
						
						newsprite.transform.parent = TileContainer.transform;
					}
					else if(i<lengthX-1 && _tiles[i+1,j].baseName == "lava")
					{
						tempobj = OT.CreateObject("lavaOutcrop");
						newsprite = tempobj.GetComponent<OTSprite>();
						newsprite.frameName = "lavaOutcropE";
						newsprite.position = _tiles[i,j].position;
						
						newsprite.transform.parent = TileContainer.transform;
					}
					
				}
			}
		}
	}
	
	private int _mobCounter = 0;
	
	private void PopulateMapWithMobs(int[,] dungeon, int threshold, int radius)
	{
		float[,] spatialMap = CreateSpatialMap(dungeon, threshold);
		
		Vector2 currentMax = FindMaxValueInSpatialMap(spatialMap);
		while(spatialMap[(int) currentMax.x, (int) currentMax.y] > 0f)
		{
			int x = (int) currentMax.x;
			int y = (int) currentMax.y;
			
			SpawnMob(x,y, 5, dungeon);
			ClearAreaInSpatialMap(spatialMap, x,y, 5);
			currentMax = FindMaxValueInSpatialMap(spatialMap);
		}
	}	
	
	private float[,] CreateSpatialMap(int[,] dungeon, int threshold)
	{
		float[,] spatialMap = new float[dungeon.GetLength(0), dungeon.GetLength (1)];
		
		float delta = (float) 1/(float) threshold;
		
		for( int i=0; i<dungeon.GetLength(0); ++i)
		{
			for(int j=0; j<dungeon.GetLength(1); ++j)
			{
				if(dungeon[i,j] != (int) TileContent.wall)
				{
					for( int k=0; k<threshold; ++k)
					{
						if(BoundsCheck(dungeon, i+k, j) && 
							dungeon[i+k,j] != (int) TileContent.wall)
						{
							spatialMap[i+k,j] = 1f-0f*delta;	
						}
						else
						{
							//print(i);
							break;	
						}				
					}
					
					for( int k=0; k<threshold; ++k)
					{
						if(BoundsCheck(dungeon, i-k, j) && 
							dungeon[i-k,j] != (int) TileContent.wall)
						{
							spatialMap[i-k,j] = 1f-0f*delta;	
						}
						else
						{
							break;	
						}
					}
					
					for( int k=0; k<threshold; ++k)
					{
						if(BoundsCheck(dungeon, i, j+k) && 
							dungeon[i,j+k] != (int) TileContent.wall)
						{
							spatialMap[i,j+k] = 1f-0f*delta;	
						}
						else
						{
							break;	
						}
					}
					
					for( int k=0; k<threshold; ++k)
					{
						if(BoundsCheck(dungeon, i, j-k) && 
							dungeon[i,j-k] != (int) TileContent.wall)
						{
							spatialMap[i,j-k] = 1f-0f*delta;	
						}
						else
						{
							break;	
						}
					}
				}
			}
		}
		
		return spatialMap;
	}
	
	private Vector2 FindMaxValueInSpatialMap(float[,] spatialMap)
	{
		float currentMax = 0;
		Vector2 currentMaxCoords = new Vector2(0,0);
		
		for( int i=0; i< spatialMap.GetLength(0); ++i)
		{
			for( int j=0; j< spatialMap.GetLength(1); ++j)
			{
				if(spatialMap[i,j] >currentMax)
				{
					currentMax = spatialMap[i,j];
					currentMaxCoords = new Vector2(i,j);
				}
			}
		}
		
		return currentMaxCoords;
	}
	
	private void ClearAreaInSpatialMap(float[,] spatialMap, int x, int y, int radius)
	{
		for( int i=-radius; i<= radius; ++i)
		{
			for( int j=-radius; j<= radius; ++j)
			{
				if(BoundsCheck(spatialMap, x+i, y+j))
				{
					spatialMap[x+i, y+j] = 0f;
				}
			}
		}
	}
	
	private bool BoundsCheck(int[,] dungeon, int x, int y)
	{
		return ( 0<=x && 0<=y && x<dungeon.GetLength(0) && y<dungeon.GetLength(1) );	
	}
	
	private bool BoundsCheck(float[,] dungeon, int x, int y)
	{
		return ( 0<=x && 0<=y && x<dungeon.GetLength(0) && y<dungeon.GetLength(1) );	
	}
	
	private void SpawnMob(int x, int y, int mobSize, int[,] dungeon)
	{
		int currentX = x;
		int currentY = y;
		
		int goonsToPlace = mobSize;
		
		GameObject newMob = new GameObject("Mob"+_mobCounter.ToString());
		newMob.AddComponent("MobManager");
		newMob.transform.position = Vector3.zero;
		newMob.GetComponent<MobManager>()._hero = Hero;
		newMob.transform.parent = MobContainer.transform;
		++_mobCounter;
		
		// this may possibly split mobs
		// fix later
		while(goonsToPlace > 0)
		{
			if(_tiles[currentX, currentY].baseName == "dirt")
			{
				// place goon
				GameObject newgruntie = OT.CreateObject("monster");
				newgruntie.GetComponent<OTSprite>().position 
					= new Vector2(currentX*CONSTANTS.TileSize, currentY*CONSTANTS.TileSize);
				newgruntie.GetComponent<MonsterManager>().
					SetMonsterType((MonsterType) Random.Range(2, (int) MonsterType.LENGTH));
				newgruntie.transform.parent = newMob.transform;
				newMob.GetComponent<MobManager>().RegisterGoon(newgruntie.GetComponent<OTSprite>());
				newgruntie.GetComponent<MonsterManager>().Dungeon = dungeon;
				--goonsToPlace;
			}
			
			bool updatedCoords = false;
			
			while(!updatedCoords)
			{
				float diceRoll = Random.Range(0f, 1f);
				if(diceRoll < 0.25f)
				{
					if(currentX > 1)
					{
						--currentX;	
						updatedCoords = true;
					}
				}
				else if(diceRoll < 0.5f)
				{
					if(currentX < dungeon.GetLength(0)-1)
					{
						++currentX;	
						updatedCoords = true;
					}
				}
				else if(diceRoll < 0.75f)
				{
					if(currentY > 1)
					{
						--currentY;	
						updatedCoords = true;
					}
				}
				else if(diceRoll < 1f)
				{
					if(currentY < dungeon.GetLength(1)-1)
					{
						++currentY;	
						updatedCoords = true;
					}
				}
			}
		}
	}
	
	// MobManager DEBUGGER
	private void AddTestMob(int[,] dungeon)
	{
		int x = Random.Range(1, dungeon.GetLength(0)-1);
		int y = Random.Range(1, dungeon.GetLength(1)-1);
		while(dungeon[x,y] == (int) TileContent.wall)
		{
			x = Random.Range(1, dungeon.GetLength(0)-1);
			y = Random.Range(1, dungeon.GetLength(1)-1);	
		}
		
		SpawnMob(x,y, 5, dungeon);
	}
	
	// PATHFINDING DEBUGGER
	private void AddTestBot(int[,] dungeon)
	{
		int x = Random.Range(1, dungeon.GetLength(0)-1);
		int y = Random.Range(1, dungeon.GetLength(1)-1);
		while(dungeon[x,y] == (int) TileContent.wall)
		{
			x = Random.Range(1, dungeon.GetLength(0)-1);
			y = Random.Range(1, dungeon.GetLength(1)-1);	
		}
		
		GameObject tmp = OT.CreateObject("testBot");
		OTSprite testBot = tmp.GetComponent<OTSprite>();
		testBot.position = new Vector2(x*CONSTANTS.TileSize, y*CONSTANTS.TileSize);
		testBot.GetComponent<testBotManager>().SetDungeon(dungeon);
	}
	
	/*
	private void CalibrateMonsters()
	{
		foreach(OTSprite monster in _monsterRoster)
		{
			monster.GetComponent<MonsterManager>().SetCrosshairsToInvisible();	
		}
	}
	*/
}
