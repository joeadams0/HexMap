using UnityEngine;
using System.Collections;
using System.Collections.Generic;

public class Actor : MonoBehaviour {
	
	public bool ShowView;
	public int SightRange;
	public float Speed;
	
	public HexTile Tile;
	/// <summary>
	/// The number of increments per move.
	/// </summary>
	public int NumberOfIncrements = 5;
	
	protected List<HexTile> _path;
	protected float lastIncremented = 0;
	protected int timesIncremented = -1;
	protected Vector3 velocity = Vector3.zero;
	
	// Use this for initialization
	void Start () {
		_path = new List<HexTile>();
	}
	
	// Update is called once per frame
	void FixedUpdate () {
		// Done with path, -1 implies that no path is being executed
		// _path.Count could be 0 and it could still be incrementing through moving to the last tile
		if(_path.Count == 0 && timesIncremented == NumberOfIncrements){
			timesIncremented = -1;
		}
		// Done with incrementing between two tiles and time to aim towards the next one
		if((_path.Count > 0 && (timesIncremented >= NumberOfIncrements))){
			performMove();
		}
		// Time to increment and the increment is greater than 0 and less than the total number of increments
		if(Time.time - lastIncremented >= (1/Speed)/NumberOfIncrements && timesIncremented < NumberOfIncrements && timesIncremented>=0){
			incrementMove();
		}
	}
	
	/// <summary>
	/// Move to the specified tile.
	/// </summary>
	/// <param name='tile'>
	/// Tile.
	/// </param>
	public virtual void move(HexTile tile){
		if(tile != null){
			Map map = Tile.Map;
			_path = map.AStarSearch(Tile, tile);
			performMove();
		}
	}
	
	/// <summary>
	/// Performs the move.
	/// </summary>
	protected virtual void performMove(){
		timesIncremented = 0;
		Tile.Map.moveActor(this.gameObject, _path[0].Location);
		Vector3 pos = transform.position;
		Vector3 goalPos = Tile.transform.position;
		velocity = new Vector3(goalPos.x-pos.x, 0, goalPos.z - pos.z);
		transform.LookAt(new Vector3(goalPos.x, pos.y, goalPos.z));
		velocity.Scale(new Vector3(1/(float)NumberOfIncrements, 0, 1/(float)NumberOfIncrements));
		_path.RemoveAt(0);
	}
	
	protected virtual void setPosition(Vector3 pos){
		transform.position = new Vector3(pos.x, transform.position.y, pos.z);
	}
		
	protected virtual void incrementMove(){
		transform.position = transform.position + velocity;
		timesIncremented++;
		lastIncremented = Time.time;
	}
}