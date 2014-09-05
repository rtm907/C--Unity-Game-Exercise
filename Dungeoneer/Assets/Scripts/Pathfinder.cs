using UnityEngine;
using System.Collections;
using System.Collections.Generic;

public class Pathfinder : MonoBehaviour {
	
	// Floodfills given dungeon at given level:
	// 0 = wall and 1 = basic floor.
	// 2+ = floodfill at some other level.
	// Floodfill starts at given tile and expands, filling accessible floor tiles with
	// the value "level".
	public static Stack<Vector2> FindPath(int[,] dungeon, int startX, int startY, int goalX, int goalY)
	{
		// quit if already at goal
		if(startX == goalX && startY == goalY)
		{
			return null;	
		}
		
		// quit if given start location is not empty floor
		if(dungeon[startX, startY] == (int) TileContent.wall)
		{
			print ("Pathfinder impassable start point passed.");
			return null;
		}
		
		Vector2?[,] dirmap = new Vector2?[dungeon.GetLength (0), dungeon.GetLength(1)];
		
		List<Vector2> currentWave = new List<Vector2>();
		currentWave.Add(new Vector2(startX, startY));
		
		while(currentWave.Count > 0)
		{
			List<Vector2> nextWave = new List<Vector2>();
			
			foreach(Vector2 v in currentWave)
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
						
						if(dirmap[nextX, nextY] == null && dungeon[nextX, nextY] != (int) TileContent.wall)
						{
							Vector2 newEntry = new Vector2(nextX, nextY);
							if(!nextWave.Contains(newEntry))
							{
								dirmap[nextX, nextY] = new Vector2(i,j);
								nextWave.Add(newEntry);
								
								// quit if target hit
								if(goalX == nextX && goalY == nextY)
								{
									break;	
								}
							}
						}
					}
				}
				
			}
			
			currentWave = nextWave;
		}
		
		Stack<Vector2> returnVal = new Stack<Vector2>();
		// recover route
		int currentX = goalX;
		int currentY = goalY;
		Vector2 currentDir = dirmap[currentX, currentY].Value;
		//Vector2 nextDir;
		
		int count = 0;
		
		while(!(currentX == startX && currentY == startY))
		{
			++count;
			returnVal.Push(new Vector2(currentX, currentY));
			currentX = currentX - (int) currentDir.x;
			currentY = currentY - (int) currentDir.y;
			currentDir = dirmap[currentX, currentY].Value;
			if(count > 100)
			{
				print(count);
				break;	
			}
		}
		
		return returnVal;
	}
	
}
