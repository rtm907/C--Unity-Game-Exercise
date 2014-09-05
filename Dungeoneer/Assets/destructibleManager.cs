using UnityEngine;
using System.Collections;

public class destructibleManager : MonoBehaviour {
	
	public GameObject ParticlesContainer;
	
	/*
	// Use this for initialization
	void Start () {
	
	}
	*/
	
	/*
	// Update is called once per frame
	void Update () {
	
	}
	*/
	
	public void Smash(Vector2 dir)
	{
		dir.Normalize();
		dir *= 100f;
		
		for(int i=0; i<5; ++i)
		{
			GameObject temp = OT.CreateObject("particle");
			OTSprite particle = temp.GetComponent<OTSprite>();
			
			particle.transform.parent = ParticlesContainer.transform;
			
			particle.position = GetComponent<OTSprite>().position;
			Vector2 dirTemp = CreatureManager.RotateDeg(dir, Random.Range(-30f, 30f));
			particle.rigidbody.AddForce(dirTemp.x, dirTemp.y, 0, ForceMode.Impulse);
			
			OTTween tween = new OTTween(particle, 2f, OTEasing.Linear);
			tween.TweenAdd("rotation", Random.Range(100f, 1000f));
	
			OT.Destroy(particle, 2f);
		}
		
		OTSprite destructible = GetComponent<OTSprite>();	
		OTTween tween2 = new OTTween(destructible, 0.5f, OTEasing.Linear);
		tween2.TweenAdd("alpha", -1f);
		
		destructible.collidable = false;
		
		OT.Destroy(destructible, 0.5f);		
	}
}
