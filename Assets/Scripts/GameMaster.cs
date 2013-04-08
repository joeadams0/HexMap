using UnityEngine;
using System.Collections;
using System.Collections.Generic;

public class GameMaster : MonoBehaviour {
	
	public GameObject Unit;
	public Map map;
	public Actor actor;
	
	private GameObject unit;
	List<HexTile> path;
	
	void Awake(){
		unit = (GameObject)Instantiate(Unit);
		actor = unit.GetComponent<Actor>();
	}
	
	// Use this for initialization
	void Start () {
		map.putActor(unit, new Vector2(0,0));
	}
	
	// Update is called once per frame
	void Update () {
		if(Input.GetMouseButtonUp(1)){
			actor.move(map.HoveredTile);
		}
	}
}
