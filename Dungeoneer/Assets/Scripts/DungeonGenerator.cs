using UnityEngine;
using System.Collections;
using System.Collections.Generic;

public enum TileContent
{
	wall = 0,
	floor,
	hero,
	monster,
	chest,
	stairsDown,
	LENGTH
}

public enum Dir
{
	Up = 0,
	Right,
	Down,
	Left,
	LENGTH
}

public class DungeonGenerator : MonoBehaviour {
	
	// FIX LATER WITH A STRUCT OR SOMETHING
	// 0 := wall
	// 1 := floor
	// 2 := hero spawn
	// 3 := monster
	// 4 := chest
	// 5 := stairs down(stairs up by default at hero spawn)
	
	#region Generators

	public static int[,] GenerateDungeonRoomsV2(int sizeX, int sizeY, int baseRoomX, int baseRoomY)
	{
		int[,] dungeon = new int[sizeX, sizeY];
		
		// Generate possible rooms
		Rect[] potentialRooms = new Rect[(int) (CONSTANTS.RoomSearchIntensity*(sizeX*sizeY)/(baseRoomX*baseRoomY))];
		
		for (int i=0; i < potentialRooms.Length; ++i)
		{
			potentialRooms[i] = GenerateRandomRoom(sizeX, sizeY, baseRoomX, baseRoomY);
		}
		
		// Pick as many as you can non-overlaping rooms in the simplest way possible
		List<Rect> chosenRooms = new List<Rect>();
		chosenRooms.Add(potentialRooms[0]);
		
		for(int i=1; i<potentialRooms.Length; ++i)
		{
			// if current potential room overlaps with none of the chosen room,
			// add it to the chosen pool
			if(! RoomsOverlapCheck(chosenRooms, potentialRooms[i]))
			{
				chosenRooms.Add(potentialRooms[i]);
			}
		}
		
		// Dig the rooms
		foreach(Rect room in chosenRooms)
		{
			DigRoom(dungeon, room);	
		}
		
		Rect[] rooms = chosenRooms.ToArray();
		
		
		// Connect the rooms with corridors. Maintain connectivity flags.
		// Each iteration must connect one additional room to the existing connected complex.
		// Make sure that at least some rooms connect to at least 2-3 others. Bias
		// toward smallest distance when connecting.
		bool[,] roomConnectivity = new bool[chosenRooms.Count,chosenRooms.Count];
		for(int i=0; i<chosenRooms.Count; ++i)
		{
			roomConnectivity[i,i] = true;	
		}
		
		bool[] globalConnectivity = new bool[chosenRooms.Count];
		//int lastConnectedRoomIndex = 0;
		globalConnectivity[0] = true;
		
		// Connect each room to its closest unconnected neighbor
		for (int i=0; i < rooms.Length; ++i)
		{
			if(ConnectionsToRoom(i,roomConnectivity) > 0)
			{
				continue;	
			}
			
			int currentRoom = i;
			int nextToConnect = NearestUnconnectedRoomIndex(rooms, currentRoom, roomConnectivity);
			
			ConnectRooms(dungeon, rooms[currentRoom], rooms[nextToConnect], CONSTANTS.BaseCorridorWidth);	
			
			roomConnectivity[currentRoom, nextToConnect] = true;
			roomConnectivity[nextToConnect, currentRoom] = true;
			if(globalConnectivity[nextToConnect])
			{
				globalConnectivity[currentRoom] = true;
				for(int j=0; j<roomConnectivity.GetLength(1); ++j)
				{
					if(roomConnectivity[currentRoom,j])
					{
						globalConnectivity[j] = true;	
					}
				}
			}
			if(globalConnectivity[currentRoom])
			{
				globalConnectivity[nextToConnect] = true;
				for(int j=0; j<roomConnectivity.GetLength(1); ++j)
				{
					if(roomConnectivity[nextToConnect,j])
					{
						globalConnectivity[j] = true;	
					}
				}
			}
		}
		
		
		// ensure global connectivity
		for (int i=0; i < rooms.Length; ++i)
		{
			if(globalConnectivity[i])
			{
				continue;	
			}
			
			int currentRoom = i;
			int nextToConnect = NearestGlobalConnectedRoomIndex(rooms, currentRoom, globalConnectivity);
			
			ConnectRooms(dungeon, rooms[currentRoom], rooms[nextToConnect], CONSTANTS.BaseCorridorWidth);	
			roomConnectivity[currentRoom, nextToConnect] = true;
			roomConnectivity[nextToConnect, currentRoom] = true;
			if(globalConnectivity[nextToConnect])
			{
				globalConnectivity[currentRoom] = true;
				for(int j=0; j<roomConnectivity.GetLength(1); ++j)
				{
					if(roomConnectivity[currentRoom,j])
					{
						globalConnectivity[j] = true;	
					}
				}
			}
		}
		
				
		// make the middle of the first room the starting position
		int heroStartX = (int) (rooms[0].center.x);
		int heroStartY = (int) (rooms[0].center.y);
		dungeon[heroStartX, heroStartY] = (int) TileContent.hero;
		
		
		// Floodfill level
		int[,] floodFill = new int[sizeX, sizeY];
		for(int i=0; i<sizeX; ++i)
		{
			for(int j=0; j<sizeY; ++j)
			{
				if(dungeon[i,j] != (int) TileContent.wall)
				{
					floodFill[i,j] = 1;
				}
			}
		}
		
		FloodFill(floodFill, 2, heroStartX, heroStartY);
		
		// Excavate non-flooded regions
		ExcavateNonFloodedRegions(floodFill);
		
		// Create chambers by spawning wall
		CutExcavationIntoChambers(floodFill);
		
		// Fill tiny spaces
		// run multiple iterations...
		for(int i=0; i<1; ++i)
		{
			FillTinyChambers(floodFill);
		}
		
		// Update global dungeon with new excavation
		for(int i=0; i<sizeX; ++i)
		{
			for(int j=0; j<sizeY; ++j)
			{
				if(floodFill[i,j] == 1)
				{
					dungeon[i,j] = (int) TileContent.floor;	
				}
			}
		}
		

		
		// FloodFill isolated chambers
		int chambersCount = FloodFillIsolatedChambers(floodFill);
		
		// Obtain isolated chambers tile coordinates
		List<Vector2>[] chambersTiles = new List<Vector2>[chambersCount];
		for(int l=0; l<chambersCount; ++l)
		{
			chambersTiles[l] = new List<Vector2>();
			for(int i=0; i<sizeX; ++i)
			{
				for(int j=0; j<sizeY; ++j)
				{
					if(floodFill[i,j] == l+3)
					{
						chambersTiles[l].Add(new Vector2(i,j));
					}
				}	
			}
		}
		
		// get bounding boxes
		Rect[] chamberBoxes = new Rect[chambersCount];
		for(int l=0; l<chambersCount; ++l)
		{
			chamberBoxes[l] = ChamberBoundingBox(chambersTiles[l]);	
		}
		
		// connect chambers to nearest rooms
		for(int l=0; l<chambersCount; ++l)
		{
			int nearestRoomIndex = 
				NearestGlobalConnectedRoomIndex(rooms, chamberBoxes[l]);
			ConnectRooms(dungeon, chamberBoxes[l], rooms[nearestRoomIndex], 1);
		}

		// populate rooms with monsters and loot
		for (int i=1; i<rooms.Length; ++i)
		{
			PopulateRoomWithMonsters(dungeon, rooms[i], 0.1f);
			PopulateRoomWithChests(dungeon, rooms[i], Random.Range(1,3));
		}
		
		// place exit
		PlaceExitInRoom(dungeon, rooms[Random.Range(1,rooms.Length)]);
		
		return dungeon;
	}
	
	private static int FloodFillIsolatedChambers(int[,] dungeon)
	{
		int level = 3;
		
		Vector2? entryPoint = FloodFillFindEntryPoint(dungeon);
		
		while(entryPoint != null)
		{
			FloodFill(dungeon, level, (int) entryPoint.Value.x, (int) entryPoint.Value.y);
			++level;
			entryPoint = FloodFillFindEntryPoint(dungeon);
		}
		
		return level-3;
	}
	
	// returns first floor tile (value 1) found
	// if none found, returns null
	private static Vector2? FloodFillFindEntryPoint(int[,] dungeon)
	{
		for(int i=0; i<dungeon.GetLength(0); ++i)
		{
			for(int j=0; j<dungeon.GetLength(1); ++j)
			{
				if(dungeon[i,j] == (int) TileContent.floor)
				{
					return new Vector2(i,j);	
				}
			}	
		}
		
		return null;
	}
	
	// Cellular automata ALGO
	// see http://roguebasin.roguelikedevelopment.org/
	// index.php?title=Cellular_Automata_Method_for_Generating_Random_Cave-Like_Levels
	public static int[,] GenerateDungeonCave(int sizeX, int sizeY)
	{
		int[,] dungeon = new int[sizeX, sizeY];
		
		for (int i=0; i<sizeX; ++i)
		{
			for (int j=0; j<sizeY; ++j)
			{
				float randomNumber = Random.Range(0f, 1f);
				
				// fill cave
				if(randomNumber < CONSTANTS.CaveBaseDensity)
				{
					dungeon[i,j]= (int) TileContent.floor;	
				}
			}
		}
		
		for (int i=0; i<CONSTANTS.CaveCAIterations; ++i)
		{
			dungeon = CaveCAInterate(dungeon);
		}
		
		return BuildBridgesOrCorridorsCavesAndLava(dungeon);
	}
	
	private static int[,] BuildBridgesOrCorridorsCavesAndLava(int[,] dungeon)
	{
		int sizeX = dungeon.GetLength(0);
		int sizeY = dungeon.GetLength(1);
		
		// clear spawn point
		int xrand = Random.Range(2, sizeX-2);
		int yrand = Random.Range(2, sizeY-2);
		
		for (int i=-1; i<=1; ++i)
		{
			for (int j=-1; j<=1; ++j)
			{	
				dungeon[xrand+i, yrand+j] = (int) TileContent.floor;
			}
		}
		dungeon[xrand, yrand] = (int) TileContent.hero;
		//dungeon[xrand+1, yrand] = (int) TileContent.stairsDown;
		
		// Floodfill level
		int[,] floodFill = new int[sizeX, sizeY];
		for(int i=0; i<sizeX; ++i)
		{
			for(int j=0; j<sizeY; ++j)
			{
				if(dungeon[i,j] != (int) TileContent.wall)
				{
					floodFill[i,j] = 1;
				}
			}
		}
		
		FloodFill(floodFill, 2, xrand, yrand);
		
		// DEBUG
		//PrintDungeonToFile(floodFill, "Floodfill1.txt");
		
		// FloodFill isolated chambers
		int chambersCount = FloodFillIsolatedChambers(floodFill);
		
		// Obtain all chambers tile coordinates
		List<Vector2>[] chambersTiles = new List<Vector2>[chambersCount+1];
		for(int l=0; l<chambersCount+1; ++l)
		{
			chambersTiles[l] = new List<Vector2>();
			for(int i=0; i<sizeX; ++i)
			{
				for(int j=0; j<sizeY; ++j)
				{
					if(floodFill[i,j] == l+2)
					{
						chambersTiles[l].Add(new Vector2(i,j));
					}
				}	
			}
		}
		
		// DEBUG
		//PrintDungeonToFile(floodFill, "Floodfill2.txt");
		
		// obtain index of largest chamber
		int largestChamberIndex = LargestChamberIndex(chambersTiles);
		
		// connect smaller chambers to largest chamber
		for(int i=0; i<chambersTiles.Length; ++i)
		{
			if(i == largestChamberIndex)
			{
				continue;	
			}
			
			Vector2 tile1 = chambersTiles[i][Random.Range(0,chambersTiles[i].Count)];
			Vector2 tile2 = chambersTiles[largestChamberIndex]
			[Random.Range(0,chambersTiles[largestChamberIndex].Count)];
			
			//print("CONNECTING "+ i+" AND "+largestChamberIndex + ":");
			//print("FROM " + tile1.ToString() + " AND "+ tile2.ToString());
			ConnectTiles(dungeon, tile1, tile2, Random.Range(1,3));
		}
		
		// nevermind...
		dungeon[xrand, yrand] = (int) TileContent.hero;
		
		CaveFillWithGoons(dungeon, CONSTANTS.CaveGoonDensity);
		CaveFillWithChests(dungeon, CONSTANTS.CaveChestDensity);

		// place exit somewhere
		Vector2 exitLocation = chambersTiles[0][0];
		dungeon[(int) exitLocation.x, (int) exitLocation.y] = (int) TileContent.stairsDown;	
		
		return dungeon;
	}
	
	private static int[,] CaveCAInterate(int[,] cave)
	{
		int[,] dungeon = new int[cave.GetLength(0), cave.GetLength(1)];
		
		for (int i=0; i<cave.GetLength(0); ++i)
		{
			for (int j=0; j<cave.GetLength(1); ++j)
			{	
				if (CaveTileTurnToWall(cave, i,j))
				{
					dungeon[i,j] = (int) TileContent.wall;	
				}
				else
				{
					dungeon[i,j] = (int) TileContent.floor;	
				}
			}
		}
		
		return dungeon;
	}
	
	private static bool CaveTileTurnToWall(int[,] cave, int x, int y)
	{
		if(x==0 || x==cave.GetLength(0)-1 || y==0 || y==cave.GetLength(1)-1)
		{
			return true;	
		}
		
		int counter = 0;
		for (int i=-1; i<=1; ++i)
		{
			for (int j=-1; j<=1; ++j)
			{	
				if(cave[x+i,y+j] == (int) TileContent.wall)
				{
					++counter;	
				}
			}
		}
		
		return (counter >= 5);
	}
	
	private static Rect GenerateRandomRoom(int mapSizeX, int mapSizeY, int baseRoomX, int baseRoomY)
	{
		int leftX = Random.Range(1, mapSizeX-baseRoomX);
		int topY = Random.Range(1, mapSizeY-baseRoomY);
		
		int width = Random.Range(Mathf.Max(5,baseRoomX)-2, Mathf.Max(5,baseRoomX)+2);
		int height = Random.Range(Mathf.Max(5,baseRoomY)-2, Mathf.Max(5,baseRoomY)+2);
		
		return new Rect(leftX, topY, width, height);
	}
	
	public static int[,] GenerateDungeonLavaIslands(int sizeX, int sizeY)
	{
		int numberOfSeeds = Mathf.Max(sizeX*sizeY/100, 1);
		
		// spawn random seeds for islands...
		Vector2[] spawnPoints = new Vector2[numberOfSeeds];
		for(int i=0; i<numberOfSeeds; ++i)
		{
			spawnPoints[i] = new Vector2(Random.Range(1, sizeX-1), Random.Range(1, sizeY-1));
		}
		
		int[,] dungeon = new int[sizeX, sizeY];
		
		for(int i=0; i<numberOfSeeds; ++i)
		{
			LavaIslandsPlantSeed(dungeon, spawnPoints[i], Random.Range(2, 5));
		}
		
		// run iterations
		for(int i=0; i<2; ++i)
		{
			dungeon = IterateLavaIslands(dungeon);	
		}
		
		return BuildBridgesOrCorridorsCavesAndLava(dungeon);
	}
	
	private static void LavaIslandsPlantSeed(int[,] dungeon, Vector2 center, int radius)
	{
		for(int i=-radius; i<=radius;++i)
		{
			for(int j=-radius; j<=radius; ++j)
			{
				if(!BoundsCheckSoft((int)center.x+i, (int)center.y+j, 
					dungeon.GetLength(0), dungeon.GetLength(1)))
				{
					continue;	
				}
				
				float diceroll = Random.Range(0f,1f);
				if(diceroll < 0.8f)
				{
					dungeon[(int)center.x+i, (int)center.y+j] = (int) TileContent.floor;
				}
			}
		}
	}
	
	private static int[,] IterateLavaIslands(int[,] dungeon)
	{
		int[,] iteration = new int[dungeon.GetLength(0), dungeon.GetLength(1)];
		
		for(int i=0; i<dungeon.GetLength(0); ++i)
		{
			for(int j=0; j<dungeon.GetLength(1); ++j)
			{
				if(dungeon[i,j] == (int) TileContent.floor || 
					LavaTurnToFloor(dungeon, i,j))
				{
					float diceroll = Random.Range(0f, 1f);
					
					if(diceroll<0.8f)
					{
						iteration[i,j] = (int) TileContent.floor;	
					}
				}
			}
		}
		
		return iteration;
	}
	
	private static bool LavaTurnToFloor(int[,] dungeon, int x, int y)
	{
		int count = 0;
		for(int i=-1; i<=1; ++i)
		{
			for(int j=-1; j<=1; ++j)
			{
				if((Mathf.Abs(i)+Mathf.Abs(j) == 0) || 
					(! BoundsCheckSoft(x+i, y+j, dungeon.GetLength(0), dungeon.GetLength(1))))
				{
					continue;	
				}
				
				if(dungeon[x+i, y+j] == (int) TileContent.floor)
				{
					++count;	
				}
			}
		}
		
		return count > 3 && BoundsCheckSoft(x,y,dungeon.GetLength(0), dungeon.GetLength(1));
	}
	
	public static int[,] GenerateCatacombs(int mapSizeX, int mapSizeY)
	{
		int[,] dungeon = new int[mapSizeX, mapSizeY];
		
		// start a digger and let it run
		int startX = 1;
		int startY = Random.Range(1, mapSizeY-1);
		
		CorridorDiggerHorizontal(dungeon, startX, startY, true, 3,//(int) Mathf.Log(mapSizeX,2),
			Random.Range(2*mapSizeX/3, mapSizeX-4), 0.2f);

		dungeon[startX, startY] = (int) TileContent.hero;
		// fix later
		dungeon[startX+1, startY] = (int) TileContent.stairsDown;
		
		return dungeon;
	}
	
	public static int[,] GenerateCatacombsV2(int mapSizeX, int mapSizeY)
	{
		int corridorWidth = 4;
		
		int[,] dungeon = new int[mapSizeX, mapSizeY];
		
		List<Rect> roomsAndCorridors = new List<Rect>();
		
		int startX = 1;
		int startY = Random.Range(1,Random.Range(1, mapSizeY-1));
		
		Rect newCorridor = GenerateCorridor(dungeon,startX, startY, Dir.Right, corridorWidth);
		roomsAndCorridors.Add(newCorridor);
		DigRoom(dungeon, newCorridor, true);
		
		//float tmp = 0f;
		for(int i=0; i<100; ++i)
		//while(DungeonWallDensity(dungeon) > 0.5f)
		{
			//tmp = DungeonWallDensity(dungeon);
			//print(DungeonWallDensity(dungeon));
			
			Rect randomRect = roomsAndCorridors[Random.Range(0,roomsAndCorridors.Count)];
			Vector3 startPoint = RandomPointFromRectangle(randomRect);
			
			
			//float diceroll1 = Random.Range(-1f,1f);
			//float diceroll2 = Random.Range(-1f,1f);
			
			Rect newRect = GenerateCorridor(dungeon, (int) startPoint.x, (int) startPoint.y,
				(Dir) (int) startPoint.z, corridorWidth);
			
			if( ! RectanglesSoftOverlapCheck(roomsAndCorridors, newRect))
			{
				roomsAndCorridors.Add(newRect);
				DigRoom(dungeon, newRect, true);
				
				print( ((Dir) (int) startPoint.z).ToString() );
				
				//PrintDungeonToFile(dungeon, 
				//	"CatacombsCor"+roomsAndCorridors.Count.ToString()+".txt");
			}
		}
		
		dungeon[startX, startY] = (int) TileContent.hero;
		dungeon[startX+1, startY] = (int) TileContent.stairsDown;
		
		return dungeon;
	}
	
	#endregion
	
	#region CollisionCheckers
	
	private static bool RectanglesSoftOverlapCheck(List<Rect> rects, Rect r)
	{
		//bool returnValue = false;
		
		for(int i=0; i<rects.Count; ++i)
		{
			if(	RectanglesSoftOverlapCheck(rects[i], r))
			{
				return true;	
			}
		}
		
		return false;
	}
	
	// SOFT overlap check; allows intersections, forbids partial overlap
	// think parallel / perpendicular streets vs overlapping streets
	// runnin in the same direction
	private static bool RectanglesSoftOverlapCheck(Rect r1, Rect r2)
	{
		return (
			(r1.xMin < r2.xMin && r1.xMax-2 > r2.xMin && r1.xMax < r2.xMax) ||
			(r1.xMin > r2.xMin && r1.xMin+2 < r2.xMax && r1.xMax > r2.xMax) ||
			(r1.yMin < r2.yMin && r1.yMax-2 > r2.yMin && r1.yMax < r2.yMax) ||
			(r1.yMin > r2.yMin && r1.yMin+2 < r2.yMax && r1.yMax > r2.yMax));
	}
	
	private static void CaveFillWithGoons(int[,] cave, float density)
	{
		for (int i=0; i<cave.GetLength(0); ++i)
		{
			for (int j=0; j<cave.GetLength(1); ++j)
			{
				if(cave[i,j] == 1 && density > Random.Range(0f, 1f))
				{
					cave[i,j] = (int) TileContent.monster;
				}
			}	
		}
	}
		
	private static void CaveFillWithChests(int[,] cave, float density)
	{
		for (int i=0; i<cave.GetLength(0); ++i)
		{
			for (int j=0; j<cave.GetLength(1); ++j)
			{
				if(cave[i,j] == 1 && density > Random.Range(0f, 1f))
				{
					cave[i,j] = (int) TileContent.chest;
				}
			}	
		}
	}
	
	private static bool RoomsOverlapCheck(List<Rect> rooms, Rect room2)
	{
		foreach(Rect room in rooms)
		{
			if(	RoomsOverlapCheck(room, room2))
			{
				return true;	
			}
		}
		
		return false;
	}
	
	// HARD overlap check, returns TRUE if rooms have any common points at all
	private static bool RoomsOverlapCheck(Rect room1, Rect room2)
	{
		return(
			PointInRect(new Vector2(room1.xMin, room1.yMin), room2) ||
			PointInRect(new Vector2(room1.xMin, room1.yMax), room2) ||
			PointInRect(new Vector2(room1.xMax, room1.yMin), room2) ||
			PointInRect(new Vector2(room1.xMax, room1.yMax), room2) ||
			PointInRect(new Vector2(room2.xMin, room2.yMin), room1) ||
			PointInRect(new Vector2(room2.xMin, room2.yMax), room1) ||
			PointInRect(new Vector2(room2.xMax, room2.yMin), room1) ||
			PointInRect(new Vector2(room2.xMax, room2.yMax), room1)
			);
	}
	
	private static bool PointInRect(Vector2 point, Rect rectangle)
	{
		return ((int)rectangle.xMin <= (int)point.x && (int)point.x <= (int)rectangle.xMax 
			&& (int)rectangle.yMin <= (int)point.y && (int)point.y <= (int)rectangle.yMax);
	}
	
	#endregion
	
	#region Diggers
	
	//(x,y) = startpoint
	private static Rect GenerateCorridor
		(int[,] dungeon, int x, int y, Dir direction, int width)
	{
		float desiredMinLength = 0.6f;
		float desiredMaxLength = 0.95f;
		int maxLength = 0;
		
		int length = 0;
		
		int xmin = 0;
		int ymin = 0;
		int rectWidth = 0;
		int rectHeight = 0;
		
		switch(direction)
		{
			case Dir.Right:
				maxLength = dungeon.GetLength (1)-1-x;
				length = 
					(int) (maxLength * Random.Range(desiredMinLength, desiredMaxLength));
				xmin = x;
				ymin = y-width/2;
				rectWidth = length;
				rectHeight = width;
				break;
			case Dir.Left:
				maxLength = x-1;
				length = 
					(int) (maxLength * Random.Range(desiredMinLength, desiredMaxLength));
				xmin = x-length;
				ymin = y-width/2;
				rectWidth = length;
				rectHeight = width;
				break;

			case Dir.Up:
				maxLength = dungeon.GetLength (0)-1-y;
				length = 
					(int) (maxLength * Random.Range(desiredMinLength, desiredMaxLength));
				xmin = x-width/2;
				ymin = y;
				rectWidth = width;
				rectHeight = length;
				break;
			case Dir.Down:
				maxLength = y-1;
				length = 
					(int) (maxLength * Random.Range(desiredMinLength, desiredMaxLength));
				xmin = x-width/2;
				ymin = y-length;
				rectWidth = width;
				rectHeight = length;
				break;
		}
		
		return new Rect(xmin, ymin, rectWidth, rectHeight);
	}
	
	private static Rect GenerateRoom
		(int[,] dungeon, int x, int y, int width, int height)
	{
		int xmin = x-width/2;
		int ymin = y-height/2;
		
		int rectWidth = Mathf.Min(dungeon.GetLength(0)-1-xmin, width);
		int rectHeight = Mathf.Min(dungeon.GetLength (1)-1-ymin, height);
		
		return new Rect(xmin, ymin, rectWidth, rectHeight);
	}
	
	private static void CorridorDiggerHorizontal
		(int[,] dungeon, int startX, int startY, bool movingToTheRight, int width,
			int lifetime, float offshootProbability)
	{
		if(width <= 0)
		{
			return;	
		}
		
		int currentX = startX;
		int currentY = startY;
		
		while(lifetime > 0)
		{
			// dig
			for(int i=0; i<width; ++i)
			{
				if(BoundsCheckSoft(currentX, currentY+i, dungeon.GetLength(0), dungeon.GetLength(1)))
				{
					dungeon[currentX, currentY+i] = (int) TileContent.floor;
				}
			}
			
			// try to branch
			float diceroll = Random.Range(0f, 1f);
			if(diceroll < offshootProbability)
			{
				// offshoot...
				float upOrDown = Random.Range(-1f, 1f);
				bool branchUp = false;
				if(upOrDown < 0f)
				{
					branchUp = true;
				}
				
				int branchLifetimeMax = branchUp ? (dungeon.GetLength(1)-currentY) : currentY;
				
				CorridorDiggerVertical(dungeon, currentX, currentY, branchUp, width-1,
					(int) (branchLifetimeMax*Random.Range(0.25f,0.75f)), offshootProbability);
			}
			
			if(movingToTheRight)
			{
				++currentX;
			}
			else
			{
				--currentX;	
			}
				
			--lifetime;	
		}
		
		CatacombsTryToSpawnRoom(dungeon, currentX, currentY, true, width, offshootProbability);
	}
	
	private static void CorridorDiggerVertical
		(int[,] dungeon, int startX, int startY, bool movingUp, int width,
			int lifetime, float offshootProbability)
	{
		if(width <= 0)
		{
			return;	
		}
		
		int currentX = startX;
		int currentY = startY;
		
		while(lifetime > 0)
		{
			// dig
			for(int i=0; i<width; ++i)
			{
				if(BoundsCheckSoft(currentX+i, currentY, dungeon.GetLength(0), dungeon.GetLength(1)))
				{
					dungeon[currentX+i, currentY] = (int) TileContent.floor;
				}
			}
			
			// try to branch
			float diceroll = Random.Range(0f, 1f);
			if(diceroll < offshootProbability)
			{
				// offshoot...
				float rightOrLeft = Random.Range(-1f, 1f);
				bool branchRight = false;
				if(rightOrLeft < 0f)
				{
					branchRight = true;
				}
				
				int branchLifetimeMax = branchRight ? (dungeon.GetLength(0)-currentX) : currentX;
				
				CorridorDiggerVertical(dungeon, currentX, currentY, branchRight, width-1,
					(int) (branchLifetimeMax*Random.Range(0.25f,0.1f)), offshootProbability);	
			}
			
			if(movingUp)
			{
				++currentY;
			}
			else
			{
				--currentY;	
			}
			
			--lifetime;	
		}
		
		CatacombsTryToSpawnRoom(dungeon, currentX, currentY, false, width, offshootProbability);
	}
	
	private static void CatacombsTryToSpawnRoom
		(int[,] dungeon, int x, int y, bool continueUpNotRight, int exitWidth, 
			float offshootProbability)
	{
		int roomMinX = Mathf.Max(x-Random.Range(1,5), 1);
		int roomMinY = Mathf.Max(y-Random.Range(1,5), 1);
		int roomWidth = Mathf.Min(Random.Range(5,9), dungeon.GetLength(0)-1-x);
		int roomHeight = Mathf.Min(Random.Range(5,9), dungeon.GetLength(1)-1-y);
		
		Rect room = new Rect(roomMinX, roomMinY, roomWidth, roomHeight);
		
		DigRoom(dungeon, room);
		
		// do a density check on the dungeon; if too high, stop expanding
		int size = dungeon.GetLength(0)*dungeon.GetLength(1);
		int walls = WallCount(dungeon);
		
		float density = (float)walls/(float)size;
		//print(density+ "; " + CONSTANTS.CatacombsLowDensityThreshold);
		if(density < CONSTANTS.CatacombsLowDensityThreshold)
		{
			// QUIT!
			//print ("?");
			return;
		}
		
		// density OK; continue to expand
		if(continueUpNotRight)
		{
			// vertical offshoot...
			float upOrDown = Random.Range(-1f, 1f);
			bool branchUp = false;
			if(upOrDown < 0f)
			{
				branchUp = true;
			}
			
			int branchLifetimeMax = branchUp ? (dungeon.GetLength(1)-y) : y;
			
			CorridorDiggerVertical(dungeon, x, y, branchUp, exitWidth,
				(int) (branchLifetimeMax*Random.Range(0.25f,0.75f)), offshootProbability);	
		}
		else
		{
			// horizontal offshoot...
			float rightOrLeft = Random.Range(-1f, 1f);
			bool branchRight = false;
			if(rightOrLeft < 0f)
			{
				branchRight = true;
			}
			
			int branchLifetimeMax = branchRight ? (dungeon.GetLength(0)-x) : x;
			
			CorridorDiggerHorizontal(dungeon, x, y, branchRight, exitWidth,
				(int) (branchLifetimeMax*Random.Range(0.25f,0.1f)), offshootProbability);	
		}
	}
	
	private static void DigRoom(int[,] dungeon, Rect room)
	{
		DigRoom(dungeon, room, true);	
	}
	
	private static void DigRoom(int[,] dungeon, Rect room, bool wallsOnBorders)
	{
		for(int i=0; i<room.width; ++i)
		{			
			for(int j=0; j<room.height; ++j)
			{
				int x = (int)(room.xMin+i);
				int y = (int) (room.yMin+j);
				
				// don't dig borders if given flag
				if(wallsOnBorders && 
					((i==0 || i==room.width-1) || (j==0 || j==room.height-1)))
				{
					continue;	
				}
				
				// don't dig along edge of dungeon
				if(! BoundsCheckSoft(x,y, dungeon.GetLength (0), dungeon.GetLength (1)))
				{
					continue;	
				}
				
				// if everything's OK, dig ahead.
				dungeon[x,y] = (int) TileContent.floor;	
			}
		}
	}
	
	// L-shaped connection
	private static void ConnectRooms(int[,] dungeon, Rect room1, Rect room2, int corridorWidth)
	{
		int Room1x = (int) room1.center.x;
		int Room1y = (int) room1.center.y;
		int Room2x = (int) room2.center.x;
		int Room2y = (int) room2.center.y;
		
		int currentX = Room1x;
		int currentY = Room1y;
		
		// dig in X
		while(currentX != Room2x)
		{
			
			for(int i=-corridorWidth/2; i< -corridorWidth/2+corridorWidth; ++i)
			{
				if(!(currentY + i < 0 || currentY + i >= dungeon.GetLength(1)))
				{
					dungeon[currentX, currentY + i] = (int) TileContent.floor;
				}
			}
			
			int delta = (Room2x > currentX) ? 1 : -1;
			currentX += delta;
		}
		
		// round corners
		for(int i=-corridorWidth/2; i< -corridorWidth/2+corridorWidth; ++i)
		{
			if(!(currentY + i < 0 || currentY + i >= dungeon.GetLength(1)))
			{
				dungeon[currentX, currentY + i] = (int) TileContent.floor;
			}
		}
		
		// dig in Y
		while(currentY != Room2y)
		{
			for(int i=-corridorWidth/2; i< -corridorWidth/2+corridorWidth; ++i)
			{
				if(!(currentX + i < 0 || currentX + i >= dungeon.GetLength(0)))
				{
					dungeon[currentX + i, currentY] = (int) TileContent.floor;
				}
			}
				
			int delta = (Room2y > currentY) ? 1 : -1;
			currentY += delta;
		}
	}
	
	// connects tiles with a straight corridor
	private static void ConnectTiles(int[,] dungeon, Vector2 tile1, Vector2 tile2, int corridorWidth)
	{
		int currentX = (int) tile1.x;
		int currentY = (int) tile1.y;
		
		float deltax = tile2.x-tile1.x;
		float deltay = tile2.y-tile1.y;
		
		float signX = Mathf.Sign(deltax);
		float signY = Mathf.Sign(deltay);
		
		float xyratio = Mathf.Abs(deltax)/(Mathf.Abs(deltay) + Mathf.Abs(deltax));
		
		bool lastdirx = true;
		int temp = 1;
		
		//int count = 0;
		
		while(currentX != (int) tile2.x || currentY != (int) tile2.y)
		{
			dungeon[currentX, currentY] = (int) TileContent.floor;
			//++count;
			
			temp = 1;
			// dig "above"...
			for(int i=0; i<corridorWidth/2; ++i)
			{
				if(!lastdirx)
				{
					if(BoundsCheckSoft
						(currentX+temp, currentY, dungeon.GetLength(0), dungeon.GetLength(1)))
					{
						dungeon[currentX+temp, currentY] = (int) TileContent.floor;
					}
				}
				else
				{
					if(BoundsCheckSoft
						(currentX, currentY+temp, dungeon.GetLength(0), dungeon.GetLength(1)))
					{
						dungeon[currentX, currentY+temp] = (int) TileContent.floor;
					}
				}
				++temp;
			}
			
			temp = -1;
			// dig "below"...
			for(int i=0; i<(corridorWidth-1)/2; ++i)
			{
				if(!lastdirx)
				{
					if(BoundsCheckSoft
						(currentX+temp, currentY, dungeon.GetLength(0), dungeon.GetLength(1)))
					{
						dungeon[currentX+temp, currentY] = (int) TileContent.floor;
					}
				}
				else
				{
					if(BoundsCheckSoft
						(currentX, currentY+temp, dungeon.GetLength(0), dungeon.GetLength(1)))
					{
						dungeon[currentX, currentY+temp] = (int) TileContent.floor;
					}
				}
				--temp;
			}
			
			// reached goal in x, advance in y
			if(currentX == (int) tile2.x)
			{
				currentY += (int) signY;
				lastdirx = false;
				continue;
			}
			
			// reached goal in y, advance in x
			if(currentY == (int) tile2.y)
			{
				currentX += (int) signX;
				lastdirx = true;
				continue;
			}
			
			// else, pick either x and y and advance...
			float diceroll = Random.Range(0f,1f);
			// advance in x...
			if(diceroll < xyratio)
			{
				currentX += (int) signX;
				lastdirx = true;
				continue;
			}
			// or in y.
			else
			{
				currentY += (int) signY;
				lastdirx = false;
				continue;
			}
		}
		
		//print("BRIDGE LENGTH = " + count);
	}
	
	#endregion
	
	#region Spawners
	
	// Spawns monsters. Density denotes the expected value of the size of the group of monsters
	// in relation to the size of the room (with one monsters being roughly as large as one tie).
	// Assumes square room.
	private static void PopulateRoomWithMonsters(int[,] dungeon, Rect room, float density)
	{
		int roomsize = (int) ((room.width-2)*(room.height-2));
		int threshold = (int) (density*roomsize);
		
		for(int i=1; i<room.width-1; ++i)
		{
			for(int j=1; j<room.height-1; ++j)
			{
				if(Random.Range(0,roomsize) < threshold)
				{
					dungeon[(int)(room.xMin+i),(int) (room.yMin+j)] = (int) TileContent.monster;	
				}
			}
		}
	}
	
	// Spawns chests in the room.
	// Assumes square room.
	private static void PopulateRoomWithChests(int[,] dungeon, Rect room, int quantity)
	{
		int roomsize = (int) ((room.width-2)*(room.height-2));
		
		for(int i=0; i<quantity; ++i)
		{
			int chestLocation = Random.Range(0,roomsize);
			dungeon[((int) room.xMin) + 1 + (chestLocation)/((int) room.height-2), 
				((int) room.yMin) + 1 + (chestLocation)%((int) room.height-2)] = 
				(int) TileContent.chest;
		}
	}
	
	// places exit in the middle of the room
	private static void PlaceExitInRoom(int[,] dungeon, Rect room)
	{
		dungeon[(int) room.center.x, (int) room.center.y] = (int) TileContent.stairsDown;
	}
	
	#endregion
	
	#region UtilityFunctions
	
	// the first two coordinates of the Vector3 are the point
	// the third coordinate is the desired Dir.
	private static Vector3 RandomPointFromRectangle(Rect r)
	{
		int rectX = (int) (r.xMin + Random.Range(2f,r.width-2f));
		int rectY = (int) (r.yMin + Random.Range(2f,r.height-2f));
		
		float diceroll = Random.Range(-1f, 1f);
		
		int direction = 0;
		
		// we want a vertical corridor...
		if(RectIsHorizontal(r))
		{
			rectY = diceroll < 0f ? (int) (r.yMin+2) : (int) (r.yMax-2);
			direction = diceroll < 0f ? (int) Dir.Down : (int) Dir.Up;
		}
		// or a horizontal one...
		else
		{
			rectX = diceroll < 0f ? (int) (r.xMin+2) : (int) (r.xMax-1);
			direction = diceroll < 0f ? (int) Dir.Left : (int) Dir.Right;
		}
		
		return new Vector3(rectX, rectY, direction);
	}
	
	private static bool RectIsHorizontal(Rect r)
	{
		return r.width > r.height;	
	}
	
	private static float DungeonWallDensity(int[,] dungeon)
	{
		return ((float) WallCount(dungeon))/
			((float) dungeon.GetLength(0)*dungeon.GetLength(1));	
	}
	
	private static int WallCount(int[,] dungeon)
	{
		int count = 0;
		
		for(int i=0; i<dungeon.GetLength(0); ++i)
		{
			for(int j=0; j<dungeon.GetLength(1); ++j)
			{
				if(dungeon[i,j] == (int) TileContent.wall)
				{
					++count;	
				}
			}
		}
		
		return count;
	}
	
	private static int LargestChamberIndex(List<Vector2>[] chambers)
	{
		int returnValue = 0;
		int currentMaxSize = chambers[0].Count;
		
		for(int i=1; i<chambers.Length; ++i)
		{
			if(currentMaxSize < chambers[i].Count)
			{
				returnValue = i;
				currentMaxSize = chambers[i].Count;
			}
		}
		
		return returnValue;
	}
	
	private static Rect ChamberBoundingBox(List<Vector2> chamber)
	{
		Vector2 someTile = chamber[0];
		int xMin = (int) someTile.x;
		int xMax = (int) someTile.x;
		int yMin = (int) someTile.y;
		int yMax = (int) someTile.y;
		
		foreach(Vector2 v in chamber)
		{
			int vx = (int) v.x;
			int vy = (int) v.y;
			
			if(vx<xMin)
			{
				xMin = vx;	
			}
			if(vx>xMax)
			{
				xMax = vx;	
			}
			if(vy<yMin)
			{
				yMin = vy;	
			}
			if(vy>yMax)
			{
				yMax = vy;	
			}
		}
		
		return new Rect(xMin, yMin, xMax-xMin, yMax-yMin);
	}
	
	private static void FillTinyChambers(int[,] dungeon)
	{
		int minDimension = 4;//Random.Range(6,10);
		
		// vertical lines run
		for(int i=0; i<dungeon.GetLength(0); ++i)
		{
			int count = 0;
			
			for(int j=0; j<dungeon.GetLength(1); ++j)
			{
				if(dungeon[i,j] == 1)
				{
					++count;
				}
				else if(count < minDimension)
				{
					count = 0;
					DrawVerticalFill(dungeon, i, j-1);
				}
				else
				{
					count = 0;	
				}
				
				/*
				if(count > maxDimension)
				{
					DrawHorizontalWall(dungeon, i,j);
					count = 0;
				}
				*/
			}
		}
		
		// horizontal lines run
		for(int i=0; i<dungeon.GetLength(0); ++i)
		{
			int count = 0;
			
			for(int j=0; j<dungeon.GetLength(1); ++j)
			{
				if(dungeon[j,i] == 1)
				{
					++count;
				}
				else if(count < minDimension)
				{
					DrawHorizontalFill(dungeon, j-1, i);
					count = 0;	
				}
				else
				{
					count = 0;	
				}
				
				/*
				if(count > maxDimension)
				{
					DrawVerticalWall(dungeon, j,i);
					count = 0;
				}
				*/
			}
		}
	}
	
	private static void CutExcavationIntoChambers(int[,] dungeon)
	{
		int maxDimension = 6;//Random.Range(6,10);
		
		// vertical lines run
		for(int i=0; i<dungeon.GetLength(0); ++i)
		{
			int count = 0;
			
			for(int j=0; j<dungeon.GetLength(1); ++j)
			{
				if(dungeon[i,j] == 1)
				{
					++count;
				}
				else
				{
					count = 0;	
				}
				
				if(count > maxDimension)
				{
					DrawHorizontalWall(dungeon, i,j);
					count = 0;
				}
			}
		}
		
		// horizontal lines run
		for(int i=0; i<dungeon.GetLength(0); ++i)
		{
			int count = 0;
			
			for(int j=0; j<dungeon.GetLength(1); ++j)
			{
				if(dungeon[j,i] == 1)
				{
					++count;
				}
				else
				{
					count = 0;	
				}
				
				if(count > maxDimension)
				{
					DrawVerticalWall(dungeon, j,i);
					count = 0;
				}
			}
		}
		
	}
	
	// conducts a vertical fill, going down (y-negative)
	private static void DrawVerticalFill(int[,] dungeon, int startX, int startY)
	{
		int currentX = startX;
		int currentY = startY;
		
		while(BoundsCheckSoft(currentX, currentY, dungeon.GetLength(0), dungeon.GetLength(1))
			&& dungeon[currentX, currentY] == 1)
		{
			dungeon[currentX, currentY] = 0;
			--currentY;
		}
	}
	
	// conducts a horizontal fill, going down (x-negative)
	private static void DrawHorizontalFill(int[,] dungeon, int startX, int startY)
	{
		int currentX = startX;
		int currentY = startY;
		
		while(BoundsCheckSoft(currentX, currentY, dungeon.GetLength(0), dungeon.GetLength(1))
			&& dungeon[currentX, currentY] == 1)
		{
			dungeon[currentX, currentY] = 0;
			--currentX;
		}
	}
	
	// draws vertical wall from startpoint, going up (y-positive)
	private static void DrawVerticalWall(int[,] dungeon, int startX, int startY)
	{
		int currentX = startX;
		int currentY = startY;
		
		while(BoundsCheckSoft(currentX, currentY, dungeon.GetLength(0), dungeon.GetLength(1))
			&& dungeon[currentX, currentY] == 1)
		{
			dungeon[currentX, currentY] = 0;
			++currentY;
		}
	}
	
	// draws horizontal wall from startpoint, going right (x-pos)
	private static void DrawHorizontalWall(int[,] dungeon, int startX, int startY)
	{
		int currentX = startX;
		int currentY = startY;
		
		while(BoundsCheckSoft(currentX, currentY, dungeon.GetLength(0), dungeon.GetLength(1))
			&& dungeon[currentX, currentY] == 1)
		{
			dungeon[currentX, currentY] = 0;
			++currentX;
		}
	}
	
	// Floodfills given dungeon at given level:
	// 0 = wall and 1 = basic floor.
	// 2+ = floodfill at some other level.
	// Floodfill starts at given tile and expands, filling accessible floor tiles with
	// the value "level".
	private static void FloodFill(int[,] dungeon, int level, int startX, int startY)
	{
		// quit if given start location is not empty floor
		if(dungeon[startX, startY] != 1)
		{
			print ("FLOODFILL FAIL EXCEPTION!");
			return;
		}
		
		List<Vector2> currentWave = new List<Vector2>();
		currentWave.Add(new Vector2(startX, startY));
		
		while(currentWave.Count > 0)
		{
			List<Vector2> nextWave = new List<Vector2>();
			
			foreach(Vector2 v in currentWave)
			{
				if(TryToFloodFillTile(dungeon, level, (int) v.x, (int) v.y))
				{
					for(int i=-1; i<=1; ++i)
					{
						for(int j=-1; j<=1; ++j)
						{
							if(Mathf.Abs(i) + Mathf.Abs(j) != 1)
							{
								continue;	
							}
							
							int nextX = ((int)v.x)+i;
							int nextY = ((int)v.y)+j;
							
							if(CheckIfTileShouldBeFloodedAtNextWave
								(dungeon, level, nextX, nextY))
							{
								Vector2 newEntry = new Vector2(nextX, nextY);
								if(!nextWave.Contains(newEntry))
								{
									nextWave.Add(newEntry);
								}
							}
						}
					}
				}
			}
			
			currentWave = nextWave;
		}
	}
	
	// Tries to floodfill tile. Returns true if successful.
	private static bool TryToFloodFillTile(int[,] dungeon, int level, int tileX, int tileY)
	{
		// bounds check
		if(! BoundsCheckHard(tileX, tileY, dungeon.GetLength(0), dungeon.GetLength(1)))
		{
			return false;	
		}
		
		if(dungeon[tileX, tileY] == (int) TileContent.floor)
		{
			dungeon[tileX, tileY] = level;
			return true;
		}
		
		return false;
	}
	
	// Checks if tile should be flooded at next wave of FloodFill.
	private static bool CheckIfTileShouldBeFloodedAtNextWave
		(int[,] dungeon, int level, int tileX, int tileY)
	{
		// bounds check
		if(! BoundsCheckHard(tileX, tileY, dungeon.GetLength(0), dungeon.GetLength(1)))
		{
			return false;	
		}
		
		return dungeon[tileX, tileY] == (int) TileContent.floor;
	}
	
	private static void ExcavateNonFloodedRegions(int[,] dungeon)
	{
		for(int i=1; i<dungeon.GetLength(0)-1; ++i)
		{
			for(int j=1; j<dungeon.GetLength(1)-1; ++j)
			{
				TryToExcavateNonFloodedTile(dungeon, i,j);
			}
		}
	}
	
	private static void TryToExcavateNonFloodedTile(int[,] dungeon, int x, int y)
	{
		// bounds check
		if(! BoundsCheckHard(x,y, dungeon.GetLength(0), dungeon.GetLength(1)))
		{
			return;	
		}
		
		/*
		// check if a neighbor borders a flooded tile		
		for(int i=-1; i<=1; ++i)
		{
			for(int j=-1; j<=1; ++j)
			{
				int currentX = x+i;
				int currentY = y+j;
				if(BordersFloodedTile(dungeon, currentX, currentY))
				{
					return;
				}
			}
		}
		*/

		if(BordersFloodedTile(dungeon, x, y))
		{
			return;	
		}
	
		// clear, dig
		dungeon[x,y] = 1;
	}
	
	private static bool BordersFloodedTile(int[,] dungeon, int x, int y)
	{
		for(int i=-1; i<=1; ++i)
		{
			for(int j=-1; j<=1; ++j)
			{
				int currentX = x+i;
				int currentY = y+j;
				if(! BoundsCheckHard(currentX, currentY, dungeon.GetLength(0), 
					dungeon.GetLength(1)) || 
					dungeon[currentX, currentY] > 1)
				{
					return true;
				}
			}
		}
		return false;
	}
	
	// returns false is (x,y) is outside of the dungeon
	private static bool BoundsCheckHard(int x, int y, int dungeonX, int dungeonY)
	{
		return (x>=0 && y>=0 && x<dungeonX && y<dungeonY); 
	}
	
	// returns false is (x,y) is outside of the dungeon or on its boundary
	private static bool BoundsCheckSoft(int x, int y, int dungeonX, int dungeonY)
	{
		return (x>0 && y>0 && x<dungeonX-1 && y<dungeonY-1); 
	}
	
	private static bool AllTrue(bool[] boolarray)
	{
		for(int i=0; i<boolarray.Length; ++i)
		{
			if(!boolarray[i])
			{
				return false;	
			}
		}
		return true;
	}
	
	private static int NearestGlobalUnconnectedRoomIndex
		(Rect[] rooms, int currentRoom, bool[] globalConnectivity)
	{
		int currentNearest = currentRoom;
		float currentShortestDistance = 0f;
		for(int i=0; i<rooms.Length; ++i)
		{
			if(globalConnectivity[i])
			{
				continue;	
			}
			
			float distance = Vector2.Distance(rooms[i].center, rooms[currentRoom].center);
			if (currentShortestDistance == 0 || currentShortestDistance > distance)
			{
				currentShortestDistance = distance;
				currentNearest = i;
			}
		}
		
		return currentNearest;
	}
	
	private static int NearestGlobalConnectedRoomIndex
		(Rect[] rooms, Rect chamber)
	{
		int currentNearest = -1;
		float currentShortestDistance = 0f;
		for(int i=0; i<rooms.Length; ++i)
		{
			float distance = Vector2.Distance(rooms[i].center, chamber.center);
			if (currentShortestDistance == 0 || currentShortestDistance > distance)
			{
				currentShortestDistance = distance;
				currentNearest = i;
			}
		}
		
		return currentNearest;
	}
	
	private static int NearestGlobalConnectedRoomIndex
		(Rect[] rooms, int currentRoom, bool[] globalConnectivity)
	{
		int currentNearest = currentRoom;
		float currentShortestDistance = 0f;
		for(int i=0; i<rooms.Length; ++i)
		{
			if(!globalConnectivity[i])
			{
				continue;	
			}
			
			float distance = Vector2.Distance(rooms[i].center, rooms[currentRoom].center);
			if (currentShortestDistance == 0 || currentShortestDistance > distance)
			{
				currentShortestDistance = distance;
				currentNearest = i;
			}
		}
		
		return currentNearest;
	}
	
	private static int NearestUnconnectedRoomIndex
		(Rect[] rooms, int currentRoom, bool[,] roomConnectivity)
	{
		int currentNearest = currentRoom;
		float currentShortestDistance = 0f;
		for(int i=0; i<rooms.Length; ++i)
		{
			if(roomConnectivity[currentRoom, i])
			{
				continue;	
			}
			
			float distance = Vector2.Distance(rooms[i].center, rooms[currentRoom].center);
			if (currentShortestDistance == 0 || currentShortestDistance > distance)
			{
				currentShortestDistance = distance;
				currentNearest = i;
			}
		}
		
		return currentNearest;
	}
		
	private static int ConnectionsToRoom(int roomIndex, bool[,] roomConnectivity)
	{
		int counter = 0;
		for (int i=0; i<roomConnectivity.GetLength(0); ++i)
		{
			if(i != roomIndex && roomConnectivity[roomIndex,i])
			{
				++counter;	
			}
		}
		
		return counter;
	}
		
	public static void PrintDungeonToFile(int[,] dungeon, string filename)
	{	
		System.IO.StreamWriter file = new System.IO.StreamWriter(filename);
		
		for(int i=0; i<dungeon.GetLength(0); ++i)
		{
			string currentString = "";
			for (int j=0; j<dungeon.GetLength(1); ++j)
			{
				currentString = currentString + dungeon[i,j].ToString();	
			}
			file.WriteLine(currentString.ToString());
		}
		
		file.Close();
	}
	
	
	#endregion
}
