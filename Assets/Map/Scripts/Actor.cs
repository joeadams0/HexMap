using UnityEngine;
using System.Collections;
using System.Collections.Generic;

public class Actor : MonoBehaviour {
	
	public bool ShowView;
	public int SightRange;
	public float Speed;
	
	public HexTile Tile;
	
	protected List<HexTile> _path;
	protected float lastMoved = -5;
	
	// Use this for initialization
	void Start () {
		_path = new List<HexTile>();
	}
	
	// Update is called once per frame
	void FixedUpdate () {
		if(_path.Count > 0 && Time.time - lastMoved >  1/Speed){
			performMove();
		}
	}
	
	/// <summary>
	/// Move to the specified tile.
	/// </summary>
	/// <param name='tile'>
	/// Tile.
	/// </param>
	public void move(HexTile tile){
		if(tile != null){
			Map map = Tile.Map;
			_path = map.AStarSearch(Tile, tile);
		}
	}
	
	/// <summary>
	/// Performs the move.
	/// </summary>
	protected void performMove(){
		Tile.Map.moveActor(this.gameObject, _path[0].Location);
		setPosition(Tile.transform.position);
		_path.RemoveAt(0);
		lastMoved = Time.time;
	}
	
	protected void setPosition(Vector3 pos){
		transform.position = new Vector3(pos.x, transform.position.y, pos.z);
	}
}
