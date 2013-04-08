/// <summary>
/// Joe Adams
/// </summary>
using UnityEngine;
using System.Collections;
using System.IO;
using System.Collections.Generic;
using System.Xml;
using System.Xml.XPath;

public delegate List<HexTile> NeighborFunction(HexTile tile);

/// <summary>
/// The Map Manager
/// </summary>
public class Map : MonoBehaviour {
	
	#region Fields 
	
	/// <summary>
	/// The map file path.
	/// </summary>
	public string MapFile = Path.Combine(Path.Combine(Path.Combine("Assets","Map"),"Maps"), "basicmap10x10.xml");
	
	/// <summary>
	/// The start position for making the map.
	/// </summary>
	public Vector2 StartPosition;
	
	/// <summary>
	/// Gets or sets a value indicating whether this <see cref="Map"/> fog of war.
	/// </summary>
	/// <value>
	/// <c>true</c> if fog of war; otherwise, <c>false</c>.
	/// </value>
	public bool FogOfWar;
	
	/// <summary>
	/// The camera position.
	/// </summary>
	public Vector3 cameraPosition = new Vector3(4f, 9f, -2);
	
	/// <summary>
	/// The camera angle.
	/// </summary>
	public Vector3 cameraAngle = new Vector3(70, 0, 0);
	
	/// <summary>
	/// The distance the mouse is from the edge of the screen when the scroll starts.
	/// </summary>
	public float ScrollDistance = 10F;
	
	/// <summary>
	/// The scroll speed for the camera.
	/// </summary>
	public float ScrollSpeed = 5F;
	
	/// <summary>
	/// Gets or sets the hovered tile.
	/// </summary>
	/// <value>
	/// The hovered tile.
	/// </value>
	public HexTile HoveredTile;
	
	/// <summary>
	/// The hex prefab.
	/// </summary>
	public GameObject HexPrefab;
	
	/// <summary>
	/// The main camera.
	/// </summary>
	public Camera mCamera;
	
	/// <summary>
	/// The mini map camera.
	/// </summary>
	public Camera MiniMapCamera;
	
	/// <summary>
	/// The _map.
	/// </summary>
	protected HexTile[,] _map;
	
	/// <summary>
	/// Refreshes the map
	/// </summary>
	protected bool _refresh;
	
	protected SearchNodeFactory _fact = SearchNodeFactory.Fact;
	
	public string MapMaterialsPath = "Map/";
	
	
	#endregion
	
	#region Methods
	
	#region Standard Methods
	
	// Use this for initialization
	void Awake (){
		generateMap();
	}
	
	// Set the Camera
	void Start(){
		setCamera();
	}
	
	// Update is called once per frame
	void Update () {
		Vector3 mousePos = Input.mousePosition;
		mousePosition(mousePos);
		camPosition(mousePos);
		if(_refresh && FogOfWar){
			fogOfWar();
		}
	}
	
	#endregion
	
	#region Map Generation
	
	/// <summary>
	/// Generates the map.
	/// </summary>
	protected virtual void generateMap(){
		// Get the size and extent
		Vector2 hexExtent = getHexExtent();
		Vector2 hexSize = getHexSize();
		
		// Get Row itr and stuff
		XmlDocument doc = new XmlDocument();
		doc.Load(MapFile);
		XPathNavigator rowNav = doc.CreateNavigator();
		XPathExpression rowExpr;
		rowExpr = rowNav.Compile("Map/Row");
		XPathNodeIterator rowItr = rowNav.Select(rowExpr);
		
		Vector2 mapSize = new Vector2(0, rowItr.Count);
		// For each row, or each y coordinate
		for(int i = 0; rowItr.MoveNext(); i++){
			float zpos = StartPosition.y + i*hexExtent.y*1.5F;
			float xOffset = hexExtent.x *i;
			
			// Get tile itr and stuff
			XPathNavigator tileNav = rowItr.Current;
			XPathExpression tileExpr = tileNav.Compile("Tile");
			XPathNodeIterator tileItr = tileNav.Select(tileExpr);
			
			// Set the map size and initialize
			if(mapSize.x == 0){
				mapSize.x = tileItr.Count;
			}
			if(_map == null){
				_map = new HexTile[(int)mapSize.x, (int)mapSize.y];
			}
			
			// For each column or x coordinate
			for(int j = 0; tileItr.MoveNext(); j++){
				float xPos = j*hexSize.x+xOffset-(i/2)*hexSize.x;
				Vector3 pos = new Vector3(xPos, 0, zpos);
				GameObject obj = (GameObject)Instantiate(HexPrefab);
				HexTile tile = obj.GetComponent<HexTile>();
				_map[j,i] = tile;
				tile.gameObject.transform.position = pos;
				tile.SetLocation(toMapPosition(j, i));
				tile.Map = this;
				this.setTile(tile, tileItr.Current.Clone());
								
				/*pos.z = pos.z-.01F;
				map[j,i].Shadow = (GameObject)Instantiate(ShadowPrefab);
				map[j,i].Shadow.transform.position =  pos;
				map[j,i].CanBeSeen = false;*/
			}
		}
	}
	
	/// <summary>
	/// Sets the tile with the configurations from the xml file.
	/// </summary>
	/// <param name='tile'>
	/// Tile.
	/// </param>
	/// <param name='nav'>
	/// Nav.
	/// </param>
	protected void setTile(HexTile tile, XPathNavigator nav){
		nav.MoveToFirstChild();
		tile.CanMove = nav.ValueAsBoolean;
		nav.MoveToNext();
		if(!nav.Value.Equals("none")){
			// Set Material
			Material mat = (Material)Resources.Load(MapMaterialsPath + nav.Value, typeof(Material));
			tile.setMaterial(mat);
		}
	}
	
	#endregion
	
	#region Actor Placement 
	
	/// <summary>
	/// Puts the unit on the map.
	/// </summary>
	/// <param name='obj'>
	/// Object.
	/// </param>
	/// <param name='pos'>
	/// Position.
	/// </param>
	public void putActor(GameObject obj, Vector2 pos){
		pos = toArrayPosition((int)pos.x, (int)pos.y);
		HexTile tile = _map[(int)pos.x, (int)pos.y];
		obj.transform.position = new Vector3(tile.gameObject.transform.position.x, obj.gameObject.transform.position.y, tile.transform.position.z);
		tile.setActor(obj);
		_refresh = tile.getActor().ShowView;
	}
	
	/// <summary>
	/// Removes the unit from the map.
	/// </summary>
	/// <param name='tile'>
	/// Tile.
	/// </param>
	public void removeActor(HexTile tile){
		_refresh = tile.getActor().ShowView;
		tile.setActor(null);
	}
	
	/// <summary>
	/// Removes the unit from the map.
	/// </summary>
	/// <param name='pos'>
	/// Position.
	/// </param>
	public void removeActor(Vector2 pos){
		pos = toArrayPosition((int)pos.x, (int)pos.y);
		removeActor(_map[(int)pos.x, (int)pos.y]);
	}
	
	/// <summary>
	/// Moves the unit.
	/// </summary>
	/// <returns>
	/// The unit.
	/// </returns>
	/// <param name='pos1'>
	/// If set to <c>true</c> pos1.
	/// </param>
	/// <param name='pos2'>
	/// If set to <c>true</c> pos2.
	/// </param>
	public bool moveActor(Vector2 pos1, Vector2 pos2){
		pos1 = toArrayPosition((int)pos1.x, (int)pos1.y);
		pos2 = toArrayPosition((int)pos2.x, (int)pos2.y);
		HexTile tile1 = _map[(int)pos1.x, (int)pos1.y];
		HexTile tile2 = _map[(int)pos2.x, (int)pos2.y];
		return moveActor(tile1, tile2);
		
	}
	
	/// <summary>
	/// Moves the unit.
	/// </summary>
	/// <returns>
	/// The unit.
	/// </returns>
	/// <param name='unit'>
	/// If set to <c>true</c> unit.
	/// </param>
	/// <param name='loc'>
	/// If set to <c>true</c> location.
	/// </param>
	public bool moveActor(GameObject unit, Vector2 loc){
		return moveActor(getHexTile(unit), getHexTile(loc));
	}
	
	/// <summary>
	/// Moves the unit.
	/// </summary>
	/// <returns>
	/// The unit.
	/// </returns>
	/// <param name='tile1'>
	/// If set to <c>true</c> tile1.
	/// </param>
	/// <param name='tile2'>
	/// If set to <c>true</c> tile2.
	/// </param>
	public bool moveActor(HexTile tile1, HexTile tile2){
		if(tile1.Actor != null && tile1 != tile2 && tile2.Actor == null && tile2.CanMove){
			tile2.setActor(tile1.Actor);
			tile1.setActor(null);
			_refresh = tile2.getActor().ShowView;
			return true;
		}
		return false;
	}
	
	#endregion
	
	#region Hex Tile Accessors
	
	/// <summary>
	/// Gets the hex tile.
	/// </summary>
	/// <returns>
	/// The hex tile.
	/// </returns>
	/// <param name='location'>
	/// Location.
	/// </param>
	public HexTile getHexTile(Vector2 location){
		location = toArrayPosition((int)location.x, (int)location.y);
		if(location.x>=0 && location.y>=0 && location.x<_map.GetLength(0) && location.y< _map.GetLength(1))
			return _map[(int)location.x, (int)location.y];
		else
			return null;
	}
	
	/// <summary>
	/// Gets the hex tile.
	/// </summary>
	/// <returns>
	/// The hex tile.
	/// </returns>
	/// <param name='obj'>
	/// Object.
	/// </param>
	public HexTile getHexTile(GameObject obj){
		foreach(HexTile tile in _map){
			if(tile.Actor == obj){
				return tile;
			}
		}
		return null;
	}
	
	/// <summary>
	/// Gets the neighboring hex on the map.
	/// </summary>
	/// <returns>
	/// The neighbors.
	/// </returns>
	/// <param name='tile'>
	/// Tile.
	/// </param>
	public List<HexTile> getNeighbors(HexTile tile){
		List<HexTile> neighbors = new List<HexTile>();
		Vector2 pos = tile.Location;
		HexTile neighbor = getHexTile(new Vector2(pos.x+1, pos.y));
		if(neighbor != null){
			neighbors.Add (neighbor);
		}
		neighbor = getHexTile(new Vector2(pos.x, pos.y+1));
		if(neighbor != null){
			neighbors.Add (neighbor);
		}
		neighbor = getHexTile(new Vector2(pos.x-1, pos.y+1));
		if(neighbor != null){
			neighbors.Add (neighbor);
		}
		neighbor = getHexTile(new Vector2(pos.x+1, pos.y-1));
		if(neighbor != null){
			neighbors.Add (neighbor);
		}
		neighbor = getHexTile(new Vector2(pos.x, pos.y-1));
		if(neighbor != null){
			neighbors.Add (neighbor);
		}
		neighbor = getHexTile(new Vector2(pos.x-1, pos.y));
		if(neighbor != null){
			neighbors.Add (neighbor);
		}
		return neighbors;
	}
	
	#endregion
	
	#region Fog Of War
	
	/// <summary>
	/// Sets all of the fog of war.
	/// </summary>
	protected void fogOfWar(){
		List<Actor> _friendlyActors= new List<Actor>();
		foreach(HexTile tile in _map){
			if(tile.getActor() != null){
				if(tile.getActor().ShowView){
					_friendlyActors.Add(tile.getActor());
				}
			}
			tile.CanBeSeen = false;
		}
		// For each active unit, set it and the map around it to visible
		foreach(Actor actor in _friendlyActors){
			revealMap(getHexTile(actor.gameObject), actor.SightRange);
		}
	}
	
	/// <summary>
	/// Sets the inital tile and all tiles within dist map area around it to visible.
	/// </summary>
	/// <param name='initialTile'>
	/// Initial tile.
	/// </param>
	/// <param name='dist'>
	/// Dist.
	/// </param>
	public void revealMap(HexTile initialTile, int dist){
		ArrayList openList = new ArrayList();
		ArrayList closedList = new ArrayList();
		openList.Add(initialTile);
		while(openList.Count>0){
			HexTile head = (HexTile)(openList[0]);
			List<HexTile> path = AStarSearch(initialTile, head);
			if(path.Count-1 == (int)distance(initialTile, head)){
				head.CanBeSeen = true;
			}
			closedList.Add (head);
			openList.RemoveAt(0);
			if(head.CanMove){
				List<HexTile> neighbors = getNeighbors(head);
				foreach(HexTile tile in neighbors){
					if(distance(initialTile, tile) <= dist){
						if(!(openList.Contains(tile) || closedList.Contains(tile))){
							openList.Add(tile);
						}
					}
				}
			}
		}
	}
	
	/// <summary>
	/// Shows the whole map.
	/// </summary>
	protected void showWholeMap(){
		foreach(HexTile tile in _map){
			tile.CanBeSeen = true;
		}
	}
	
	#endregion
	
	#region Helpers
	
	/// <summary>
	/// Converts the array position to map position
	/// </summary>
	/// <returns>
	/// The map position.
	/// </returns>
	/// <param name='x'>
	/// X.
	/// </param>
	/// <param name='y'>
	/// Y.
	/// </param>
	protected Vector2 toMapPosition(int x, int y){
		return new Vector2(x - y/2,y);
	}
	
	/// <summary>
	/// Converts the map position to the array position
	/// </summary>
	/// <returns>
	/// The array position.
	/// </returns>
	/// <param name='x'>
	/// X.
	/// </param>
	/// <param name='y'>
	/// Y.
	/// </param>
	protected Vector2 toArrayPosition(int x, int y){
		return new Vector2(x + y/2, y);
	}
	
	/// <summary>
	/// Gets the hex extent of the hex tile. Extent is half the size.
	/// </summary>
	/// <returns>
	/// The hex extent.
	/// </returns>
	protected Vector2 getHexExtent(){
		GameObject inst = (GameObject)Instantiate(HexPrefab);
		Vector2 extents =  new Vector2(inst.collider.bounds.extents.x, inst.collider.bounds.extents.z);
		Destroy(inst);
		return extents;
	}
	
	/// <summary>
	/// Gets the size of the hex.
	/// </summary>
	/// <returns>
	/// The hex size.
	/// </returns>
	protected Vector2 getHexSize(){
		GameObject inst = (GameObject) Instantiate(HexPrefab);
		Vector2 size = new Vector2(inst.collider.bounds.size.x, inst.collider.bounds.size.z);
		Destroy(inst);
		return size;
	}
	#endregion
	
	#region Mouse Input & Camera Control
	
	/// <summary>
	/// Sets the hovered tile.
	/// </summary>
	/// <param name='mousePos'>
	/// Mouse position.
	/// </param>
	protected void mousePosition(Vector3 mousePos){
		RaycastHit hit;
		Ray ray = mCamera.ScreenPointToRay(mousePos);
		int layerMask = 1<<8;
		if(Physics.Raycast(ray, out hit, Mathf.Infinity, layerMask)){
			HoveredTile = ((HexTile)(hit.collider.gameObject.GetComponent(typeof(HexTile))));
		}
		else if(HoveredTile != null){
			HoveredTile = null;
		}
	}
	
	/// <summary>
	/// Scrolls the camera.
	/// </summary>
	/// <param name='mousePos'>
	/// Mouse position.
	/// </param>
	protected void camPosition(Vector3 mousePos){
		float mPosX = mousePos.x;
		float mPosY = mousePos.y;
		
		if (mPosX < ScrollDistance) {
			mCamera.transform.Translate(Vector3.right * -ScrollSpeed * Time.deltaTime);
		}
		if (mPosX >= Screen.width - ScrollDistance) {
			mCamera.transform.Translate(Vector3.right * ScrollSpeed * Time.deltaTime);
		}
		if (mPosY < ScrollDistance) {
			mCamera.transform.Translate(( Vector3.forward) * - ScrollSpeed * Time.deltaTime, Space.World );
		}
		if (mPosY >= Screen.height - ScrollDistance) {
			mCamera.transform.Translate((Vector3.forward) * ScrollSpeed * Time.deltaTime, Space.World);
		}

	}
	
	/// <summary>
	/// Sets the camera position;
	/// </summary>
	protected void setCamera(){
		mCamera.transform.position = cameraPosition;
		mCamera.transform.localEulerAngles = cameraAngle;
	}
	
	#endregion
	
	#region Distance
	
	/// <summary>
	/// Gets the distance between two tiles.
	/// </summary>
	/// <param name='tile1'>
	/// Tile1.
	/// </param>
	/// <param name='tile2'>
	/// Tile2.
	/// </param>
	public float distance(HexTile tile1, HexTile tile2){
		if(tile1 == null || tile2 == null){
			return 0;
		}
		Vector3 pos1 = tile1.getCoordinates();
		Vector3 pos2 = tile2.getCoordinates();
		float dist = Mathf.Abs(pos1.x - pos2.x);
		dist = Mathf.Max(Mathf.Abs(pos1.y-pos2.y), dist);
		dist = Mathf.Max(Mathf.Abs(pos1.z-pos2.z), dist);
		return dist;
	}
	
	/// <summary>
	/// Gets the distance between two positions.
	/// </summary>
	/// <param name='pos1'>
	/// Pos1.
	/// </param>
	/// <param name='pos2'>
	/// Pos2.
	/// </param>
	public float distance(Vector2 pos1, Vector2 pos2){
		return distance(getHexTile(pos1), getHexTile(pos2));
	}
	
	/// <summary>
	/// Getst the distance between a unit and a position.
	/// </summary>
	/// <param name='unit'>
	/// Actor.
	/// </param>
	/// <param name='pos2'>
	/// Pos2.
	/// </param>
	public float distance(GameObject unit, Vector2 pos2){
		return distance(getHexTile(unit), getHexTile(pos2));
	}
	
	#endregion
	
	#region A* Search
	
	/// <summary>
	/// A*Search wrapper
	/// </summary>
	/// <returns>
	/// The star search.
	/// </returns>
	/// <param name='start'>
	/// Start.
	/// </param>
	/// <param name='end'>
	/// End.
	/// </param>
	public List<HexTile> AStarSearch (HexTile start, HexTile end){
		SearchNode startNode = generateSearchNode(start, null, 0, null);
		SearchNode endNode = generateSearchNode(end, null, 0, null);
		return AStarSearch(startNode, endNode);
	}
	
	/// <summary>
	/// Performs A* search to find optimal path between two tiles.
	/// </summary>
	/// <returns>
	/// The star search.
	/// </returns>
	/// <param name='start'>
	/// Start.
	/// </param>
	/// <param name='finish'>
	/// Finish.
	/// </param>
	protected List<HexTile> AStarSearch(SearchNode start, SearchNode finish){
		// Openlist
		PriorityQueue openList = new PriorityQueue();
		// Closed List
		List<SearchNode> closedList = new List<SearchNode>();
		// Add initial node
		openList.enqueue(start);
		
		List<HexTile> path = new List<HexTile>();
		// Go till there is nothing on openlist
		while(openList.Count>0){
			//Debug.Log(openList);
			//openList.printQueue();
			SearchNode head = openList.dequeue();
			//Debug.Log(head.Tile.Location);
			if(head.Tile == finish.Tile){
				path = generatePath(head);
				break;
			}
			else if(head.Tile.CanMove){
				List<HexTile> neighbors = getNeighbors(head.Tile);
				foreach(HexTile t in neighbors){
					SearchNode s = generateSearchNode(t, finish.Tile, head.Cost + 1, head);
					if(openList.contains(s)){
						SearchNode oldNode = openList.get(s);
						updateNode(s, oldNode);
					}
					else if(containsNode(closedList, s) == null){
						openList.enqueue(s);
					}
				}
			}
			closedList.Add(head);
		}
		//_fact.returnNodes(openList);
		//_fact.returnNodes(closedList);
		return path;
	}
	
	/// <summary>
	/// Creates a search node from a hextile.
	/// </summary>
	/// <returns>
	/// The search node.
	/// </returns>
	/// <param name='tile'>
	/// Tile.
	/// </param>
	/// <param name='finish'>
	/// Finish.
	/// </param>
	/// <param name='cost'>
	/// Cost.
	/// </param>
	/// <param name='parent'>
	/// Parent.
	/// </param>
	protected SearchNode generateSearchNode(HexTile tile, HexTile finish, int cost, SearchNode parent){
		SearchNode node = _fact.getNode();
		node.Tile = tile;
		node.Cost = cost;
		node.DistToGoal = (int)distance(tile, finish);
		node.Parent = parent;
		return new SearchNode(tile, cost, (int)distance(tile, finish), parent);
	}
	
	/// <summary>
	/// Sees if hex is movable.
	/// </summary>
	/// <returns>
	/// The move.
	/// </returns>
	/// <param name='pos'>
	/// If set to <c>true</c> position.
	/// </param>
	public bool canMove(Vector2 pos){
		return getHexTile(pos).CanMove && getHexTile(pos).Actor == null;
	}
	
	/// <summary>
	/// Updates the search node.
	/// </summary>
	/// <param name='s'>
	/// S.
	/// </param>
	/// <param name='oldNode'>
	/// Old node.
	/// </param>
	protected void updateNode(SearchNode s, SearchNode oldNode){
		if(oldNode.Cost>s.Cost){
			oldNode.Cost = s.Cost;
			oldNode.Parent = s.Parent;
		}
	}
	
	/// <summary>
	/// Generates the path. Reverse transverses the tree to create path.
	/// </summary>
	/// <returns>
	/// The path.
	/// </returns>
	/// <param name='node'>
	/// Node.
	/// </param>
	protected List<HexTile> generatePath(SearchNode node){
		List<HexTile> path = new List<HexTile>();
		while(node != null){
			path.Insert(0, node.Tile);
			node = node.Parent;
		}
		// Get rid of initial node
		if(path.Count >0){
			path.RemoveAt(0);
		}
		return path;
	}
	
	/// <summary>
	/// Checks if the closed list contains the node.
	/// </summary>
	/// <returns>
	/// The node.
	/// </returns>
	/// <param name='closedList'>
	/// Closed list.
	/// </param>
	/// <param name='node'>
	/// Node.
	/// </param>
	protected SearchNode containsNode(List<SearchNode> closedList, SearchNode node){
		foreach(SearchNode n in closedList){
			if(n.Equals(node)){
				return n;
			}
		}
		return null;
	}
	#endregion
	
	#endregion

}
