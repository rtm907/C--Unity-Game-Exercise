using UnityEngine;
using System.Collections;

public class CreatureManager : MonoBehaviour {
	
	public static bool LOSCheck(OTObject a, OTObject b)
	{
		return LOSCheck(a,b,100f);	
	}
	
	public static bool LOSCheck(OTObject a, OTObject b, float z)
	{
		RaycastHit target;
		
		//float z=100f;
		
		Physics.Raycast(new Vector3(a.rigidbody.position.x, a.rigidbody.position.y, z), 
			new Vector3(b.rigidbody.position.x, b.rigidbody.position.y, z) - 
			new Vector3(a.rigidbody.position.x, b.rigidbody.position.y, z), 
			out target, 
			Vector3.Distance(a.rigidbody.position, b.rigidbody.position));
		if(target.rigidbody == null)
		{
			return true;	
		}
		else
		{
			//print(target.rigidbody.gameObject.name);
		}
		
		return false;
	}
	
	// Finds angle between Vector and some reference axis
	public static float FindAngleWalk(Vector2 facedir)
	{	
		float angle = 0f;
			
		if(facedir.x == 0 && facedir.y > 0) {angle = Mathf.PI/2;}
		else if(facedir.x == 0 && facedir.y < 0) {angle = 3*Mathf.PI/2;}
		else
		{
			angle = Mathf.Atan(facedir.x/facedir.y);	
			angle += Mathf.PI/2;			
			if(facedir.y<0) angle += Mathf.PI;
		}
		
		return angle;
	}
	
	/*
	public static float FindAngleProjectile(Vector2 facedir)
	{
		return -(Mathf.Rad2Deg)*(CreatureManager.FindAngleWalk(facedir)) + 90;	
	}
	*/
		
	public static float FindAngleFacing(Vector2 facedir)
	{
		return -(Mathf.Rad2Deg)*(CreatureManager.FindAngleWalk(facedir)) + 90;	
	}
	        
    public static Vector2 RotateRad(Vector2 v, float angle)
    {
		Vector2 returnVal;
		
        float sin = Mathf.Sin( angle );
        float cos = Mathf.Cos( angle );
       
        float tx = v.x;
        float ty = v.y;
        returnVal.x = (cos * tx) - (sin * ty);
        returnVal.y = (cos * ty) + (sin * tx);
		
		return returnVal;
    }
 
	public static Vector2 RotateDeg(Vector2 v, float angle)
    {
		return RotateRad(v, Mathf.Deg2Rad*angle);
	}
	
	/*
    public static void RotateZ( this Vector3 v, float angle )
    {
        float sin = Mathf.Sin( angle );
        float cos = Mathf.Cos( angle );
       
        float tx = v.x;
        float ty = v.y;
        v.x = (cos * tx) - (sin * ty);
        v.y = (cos * ty) + (sin * tx);
    }
	*/
}
