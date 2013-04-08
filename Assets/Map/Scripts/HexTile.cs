/// <summary>
/// Joe Adams
/// </summary>
using UnityEngine;
using System.Collections;

/// <summary>
/// Hex tile.
/// </summary>
public class HexTile : MonoBehaviour {
	
	#region Fields
	
	/// <summary>
	/// The grid position on the map.
	/// </summary>
	public Vector3 GridPosition;
	
	/// <summary>
	/// The location on the map.
	/// </summary>
	public Vector2 Location;
	
	/// <summary>
	/// The unit at this tile.
	/// </summary>
	public GameObject Actor;
	
	/// <summary>
	/// The shadow for the fog of war.
	/// </summary>
	public GameObject Shadow;
	
	/// <summary>
	/// The color of the shadow.
	/// </summary>
	public Color ShadowColor = Color.gray;
	
	/// <summary>
	/// Gets or sets a value indicating whether this instance can be seen.
	/// </summary>
	/// <value>
	/// <c>true</c> if this instance can see; otherwise, <c>false</c>.
	/// </value>
	public bool CanBeSeen = true;
	
	/// <summary>
	/// Tells if you can move to this hex.
	/// </summary>
	public bool CanMove = true;
	
	/// <summary>
	/// The map the tile is on.
	/// </summary>
	public Map Map;

	#endregion
	
	/// <summary>
	/// Update the color of the texture.
	/// </summary>
	void Update(){
		if(CanBeSeen){
			if(Shadow != null){
				ShadowColor.a = 0f;
				Shadow.renderer.material.color = ShadowColor;
			}
			if(Actor != null){
				setActorVisible(true);
			}
		}
		else{
			if(Shadow != null){
				ShadowColor.a = .6f;
				Shadow.renderer.material.color = ShadowColor;
			}
			if(Actor != null){
				setActorVisible(false);
			}
		}
	}
	
	#region Coordinates
	
	/// <summary>
	/// Gets the coordinates.
	/// </summary>
	/// <returns>
	/// The coordinates.
	/// </returns>
	public Vector3 getCoordinates(){
		return new Vector3(Location.x, Location.y, getZ());
	}
	
	/// <summary>
	/// Sets the location.
	/// </summary>
	/// <param name='loc'>
	/// Location.
	/// </param>
	public void SetLocation(Vector2 loc){
		Location = loc;
	}
	
	/// <summary>
	/// Gets the z coordinate.
	/// </summary>
	/// <returns>
	/// The z.
	/// </returns>
	public float getZ(){
		return -(Location.x + Location.y);
	}
	
	#endregion
	
	#region Graphics 
	
	/// <summary>
	/// Sets the material.
	/// </summary>
	/// <param name='mat'>
	/// Mat.
	/// </param>
	public void setMaterial(Material mat){
		this.gameObject.renderer.material = mat;
	}
	
	/// <summary>
	/// Sets the units renderers off or on.
	/// </summary>
	/// <param name='enabled'>
	/// Enabled.
	/// </param>
	void setActorVisible(bool enabled){
		foreach(Transform child in Actor.transform){
			child.gameObject.SetActive(enabled);
		}
	}
	
	#endregion
	
	
	/// <summary>
	/// Gets the description for gui.
	/// </summary>
	/// <returns>
	/// The description.
	/// </returns>
	public string getDescription(){
		return "No description given.";
	}
	
	/// <summary>
	/// Determines whether the specified <see cref="System.Object"/> is equal to the current <see cref="HexTile"/>.
	/// </summary>
	/// <param name='obj'>
	/// The <see cref="System.Object"/> to compare with the current <see cref="HexTile"/>.
	/// </param>
	/// <returns>
	/// <c>true</c> if the specified <see cref="System.Object"/> is equal to the current <see cref="HexTile"/>; otherwise, <c>false</c>.
	/// </returns>
	public override bool Equals(object obj){
		if(!(obj.GetType().Equals(this.GetType()))){
			return false;
		}
		else{
			HexTile tile = (HexTile) obj;
			return tile.Location.x == Location.x && tile.Location.y == Location.y;
		}
	}
	
	/// <summary>
	/// Serves as a hash function for a <see cref="HexTile"/> object.
	/// </summary>
	/// <returns>
	/// A hash code for this instance that is suitable for use in hashing algorithms and data structures such as a hash table.
	/// </returns>
	public override int GetHashCode(){
		return this.gameObject.GetHashCode();
	}
	
	/// <summary>
	/// Gets the actor, if there is one.
	/// </summary>
	/// <returns>
	/// The actor.
	/// </returns>
	public Actor getActor(){
		if(Actor == null){
			return null;
		}
		return Actor.GetComponent<Actor>();
	}
	
	/// <summary>
	/// Sets the actor.
	/// </summary>
	/// <param name='actor'>
	/// Actor.
	/// </param>
	public void setActor(GameObject actor){
		Actor = actor;
		if(Actor != null){
			getActor().Tile = this;
		}
	}
}
