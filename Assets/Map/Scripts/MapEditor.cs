using UnityEngine;
using System.Collections;
using System.Collections.Generic;
using System.Xml;
using System.IO;

public class MapEditor : Map {
	
	public Vector2 MapSize = new Vector2(10F, 10F);
	List<Material> mats = new List<Material>();
	string[] names;
	string xField;
	string yField;
	public string SaveFile = "NewMap.xml";
	public string Directory = "Assets\\Map\\Maps\\";
	
	int selected = 0;
	
	void Start(){
		setCamera();
		Object[] materials = Resources.LoadAll(MapMaterialsPath, typeof(Material));
		names = new string[materials.Length];
		int i = 0;
		foreach(Object mat in materials){
			mats.Add((Material)mat);
			names[i] = ((Material)mat).name;
			i++;
		}
	}
	
	void FixedUpdate(){
		if(Input.GetMouseButtonUp(0)){
			if(HoveredTile != null){
				HoveredTile.renderer.material = mats[selected];
			}
		}
		if(Input.GetMouseButtonUp(1)){
			if(HoveredTile != null){
				HoveredTile.CanMove = !HoveredTile.CanMove;
			}
		}
	}
	
	void OnGUI(){
		GUI.Window(0, new Rect(10, 175, 150, 200), textures, "Textures");
		GUI.Window(1, new Rect(10, 95, 200, 70), tile, "Tile");
		GUI.Window(2, new Rect(10, 10, 200, 80), mapSize, "Map Size");
		GUI.Label(new Rect(Screen.width - 330, 10, 50, 20), "File: ");
		SaveFile = GUI.TextArea(new Rect(Screen.width - 300, 10, 290, 20), SaveFile);
		if(GUI.Button(new Rect(10, 385, 100, 30), "Save")){
			save();
		}
		
	}
	
	void textures(int windowId){
		selected = GUI.SelectionGrid(new Rect(10, 20, 130, 30*names.Length), selected, names, 1);
	}
	
	void tile(int windowId){
		HexTile tile = HoveredTile;
		if(tile != null){
			bool canMove = tile.CanMove;
			canMove = GUI.Toggle(new Rect(10, 20, 100, 20), canMove, "Can Move");
			GUI.Label(new Rect(10, 35, 100, 20), "Material: " + tile.renderer.material.name);
			GUI.Label(new Rect(100, 20, 100, 20), "Loc: (" + tile.Location.x + ", " + tile.Location.y + ")");
			tile.CanMove = canMove;
		}
	}
	
	void mapSize(int windowId){
		if(xField == null){
			xField = "" + MapSize.x;
		}
		if(yField == null){
			yField = "" + MapSize.y;
		}
		GUI.Label(new Rect(10,15, 100, 20), "Map Size:"); 
		GUI.Label(new Rect(10, 30, 30, 20), "X: ");
		GUI.Label(new Rect(10, 50, 30, 20), "Y: ");
		xField = GUI.TextArea(new Rect(30, 32, 30, 20), "" + xField);
		yField = GUI.TextArea(new Rect(30, 50, 30, 20), "" + yField);
		if(GUI.Button(new Rect(80, 40, 100, 30), "Change Size")){
			int x = 0,y = 0;
			int.TryParse(xField, out x);
			int.TryParse(yField, out y);
			
			if(x != MapSize.x || y != MapSize.y){
				sizeChange(x,y);
			}
		}
	}
	
	protected override void generateMap(){
		// Get the size and extent
		Vector2 hexExtent = getHexExtent();
		Vector2 hexSize = getHexSize();
		_map = new HexTile[(int)MapSize.x, (int)MapSize.y];
		
		// For each row, or each y coordinate
		for(int i = 0; i<MapSize.y; i++){
			float zpos = StartPosition.y + i*hexExtent.y*1.5F;
			float xOffset = hexExtent.x *i;
			
			
			// For each column or x coordinate
			for(int j = 0; j<MapSize.x; j++){
				float xPos = j*hexSize.x+xOffset-(i/2)*hexSize.x;
				Vector3 pos = new Vector3(xPos, 0, zpos);
				GameObject obj = (GameObject)Instantiate(HexPrefab);
				HexTile tile = obj.GetComponent<HexTile>();
				_map[j,i] = tile;
				tile.gameObject.transform.position = pos;
				tile.SetLocation(toMapPosition(j, i));
				tile.Map = this;
			}
		}
	}
	
	protected void sizeChange(int x, int y){
		HexTile[,] map = new HexTile[x, y];
		// Get the size and extent
		Vector2 hexExtent = getHexExtent();
		Vector2 hexSize = getHexSize();
		
		// For each row, or each y coordinate
		for(int i = 0; i<map.GetLength(1); i++){
			float zpos = StartPosition.y + i*hexExtent.y*1.5F;
			float xOffset = hexExtent.x *i;
			
			
			// For each column or x coordinate
			for(int j = 0; j<map.GetLength(0); j++){
				float xPos = j*hexSize.x+xOffset-(i/2)*hexSize.x;
				Vector3 pos = new Vector3(xPos, 0, zpos);
				if(j<_map.GetLength(0) && i<_map.GetLength(1)){
					map[j, i] = _map[j, i];
				}
				else{
					GameObject obj = (GameObject)Instantiate(HexPrefab);
					HexTile tile = obj.GetComponent<HexTile>();
					map[j,i] = tile;
					tile.gameObject.transform.position = pos;
					tile.SetLocation(toMapPosition(j, i));
					tile.Map = this;
				}
			}
		}
		
		if(MapSize.x -x >0 || MapSize.y- y >0){
			for(int i = 0; i < _map.GetLength(1); i++){
				for(int j = 0; j< _map.GetLength(0); j ++){
					if(i>=y || j>= x){
						Destroy(_map[j,i].gameObject);
					}
				}
			}
		}
		_map = map;
		MapSize = new Vector2(_map.GetLength(0), _map.GetLength(1));
	}
	
	protected void save(){
		using (XmlWriter writer = XmlWriter.Create(Path.Combine(Directory, SaveFile)))
		{
		    writer.WriteStartDocument();
		    writer.WriteStartElement("Map");
	
		    for(int i = 0; i<_map.GetLength(0); i++)
		    {
				writer.WriteStartElement("Row");
				for(int j = 0; j<_map.GetLength(1); j++){
					HexTile tile = _map[j,i];
					writer.WriteStartElement("Tile");
					writer.WriteElementString("CanMove", tile.CanMove.ToString().ToLower());
					writer.WriteElementString("Material", tile.renderer.material.name.Replace(" (Instance)", ""));
					writer.WriteEndElement();
				}
		
				writer.WriteEndElement();
		    }
	
		    writer.WriteEndElement();
		    writer.WriteEndDocument();
		}
	}
}
