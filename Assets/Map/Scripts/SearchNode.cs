/// <summary>
/// Joe Adams
/// </summary>
using System;
using UnityEngine;

/// <summary>
/// Search node. Used for A* Search.
/// </summary>
public class SearchNode : IComparable
{
	public HexTile Tile;
	public int Cost;
	public int DistToGoal;
	public SearchNode Child;
	public SearchNode Parent;
	
	public SearchNode (HexTile tile, int cost, int distToGoal, SearchNode parent)
	{
		Tile = tile;
		Cost = cost;
		Tile = tile;
		Parent = parent;
	}
	
	public SearchNode(){
	}
	
	public int heuristic(){
		return Cost + DistToGoal;
	}
	
	/// <summary>
	/// Compares the heuristics.
	/// </summary>
	/// <returns>
	/// The to.
	/// </returns>
	/// <param name='obj'>
	/// Object.
	/// </param>
	public int CompareTo(object obj){
		SearchNode node = (SearchNode) obj;
		int val = 0;
		if(this.heuristic() < node.heuristic()){
			val = -1;
		}
		else if(this.heuristic() > node.heuristic()){
			val = 1;
		}
		
		return val;
	}
	
	/// <summary>
	/// Determines whether the specified <see cref="System.Object"/> is equal to the current <see cref="SearchNode"/>.
	/// </summary>
	/// <param name='obj'>
	/// The <see cref="System.Object"/> to compare with the current <see cref="SearchNode"/>.
	/// </param>
	/// <returns>
	/// <c>true</c> if the specified <see cref="System.Object"/> is equal to the current <see cref="SearchNode"/>;
	/// otherwise, <c>false</c>.
	/// </returns>
	public override bool Equals(object obj){
		if(!(obj.GetType().Equals(this.GetType()))){
			return false;
		}
		else{
			SearchNode node = (SearchNode) obj;
			return Tile.Equals(node.Tile);
		}
	}
	
	/// <summary>
	/// Serves as a hash function for a <see cref="SearchNode"/> object.
	/// </summary>
	/// <returns>
	/// A hash code for this instance that is suitable for use in hashing algorithms and data structures such as a hash table.
	/// </returns>
	public override int GetHashCode(){
		return base.GetHashCode();
	}
}

