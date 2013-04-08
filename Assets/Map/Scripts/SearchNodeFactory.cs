using UnityEngine;
using System.Collections;
using System.Collections.Generic;

public class SearchNodeFactory {
	
	public static SearchNodeFactory Fact = new SearchNodeFactory();
	
	private List<SearchNode> _openNodes;
		
	private SearchNodeFactory(){
		_openNodes = new List<SearchNode>();
	}
	
	public SearchNode getNode(){
		if(_openNodes.Count == 0){
			return new SearchNode();
		}
		else{
			SearchNode node = _openNodes[0];
			_openNodes.RemoveAt(0);
			return node;
		}
	}
	
	public void returnNode(SearchNode node){
		_openNodes.Add(node);
	}
	
	public void returnNodes(PriorityQueue q){
		while(q.Count != 0){
			returnNode(q.dequeue());
		}
	}
	
	public void returnNodes(List<SearchNode> list){
		foreach(SearchNode node in list){
			returnNode(node);
		}
	}
}
