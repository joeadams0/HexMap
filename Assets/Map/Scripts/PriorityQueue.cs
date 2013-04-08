using UnityEngine;
using System.Collections;
using System;
using System.Collections.Generic;

public class PriorityQueue{
	
	public int Count {
		get{
			return _queue.Count;
		}
		private set{
		}
	}
	List<SearchNode> _queue;
	
	public PriorityQueue(){
		_queue = new List<SearchNode>();
	}
	
	/// <summary>
	/// Enqueue the specified node using binary search
	/// </summary>
	/// <param name='node'>
	/// Node.
	/// </param>
	public void enqueue(SearchNode node){
		if(Count == 0){
			_queue.Add(node);
			return;
		}
		int start = 0;
		int end = _queue.Count -1;
		int middle;
		while(end >= start){
			middle = (end-start)/2 + start;
			SearchNode mid = _queue[middle];
			int comp = node.CompareTo(mid);
			// Found the spot
			if(end-start == 0){
				if(comp > 0){
					_queue.Insert(middle+1, node);
				}
				else{
					_queue.Insert(middle, node);
				}
				break;
			}
			if(comp < 0){
				end = middle -1;
			}
			else if(comp > 0){
				start = middle + 1;
			}
			else{
				_queue.Insert(middle, node);
				break;
			}
		}
	}
	
	/// <summary>
	/// Dequeue first node.
	/// </summary>
	public SearchNode dequeue(){
		if(_queue.Count >0){
			SearchNode node = _queue[0];
			_queue.RemoveAt(0);
			return node;
		}
		else{
			return null;
		}
	}
	
	/// <summary>
	/// Contains the specified node.
	/// </summary>
	/// <param name='node'>
	/// If set to <c>true</c> node.
	/// </param>
	public bool contains(SearchNode node){
		foreach(SearchNode n in _queue){
			if(n.Equals(node)){
				return true;
			}
		}
		return false;
	}
	
	/// <summary>
	/// Get the specified node.
	/// </summary>
	/// <param name='node'>
	/// Node.
	/// </param>
	public SearchNode get(SearchNode node){
		foreach(SearchNode n in _queue){
			if(n.Equals(node)){
				return n;
			}
		}
		return null;
	}
	
	public void printQueue(){
		foreach(SearchNode node in _queue){
			Debug.Log(node.Tile.Location);
		}
	}
}
