using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class SortSpriteWithParent : MonoBehaviour {

	protected SpriteRenderer _mySprite;
	protected SpriteRenderer _parentSprite;

	public int offset = 0;

	public int parentDepth = 1;

	// Use this for initialization
	void Start () {
		_mySprite = GetComponent<SpriteRenderer>();
		Transform parent = transform;
		for (int i = 0; i < parentDepth; i++) {
			parent = parent.parent;
		}
		if (parent != null) {
			_parentSprite = parent.GetComponent<SpriteRenderer>();
		}

	}
	
	// Update is called once per frame
	void Update () {
		if (_mySprite != null && _parentSprite != null) {
			_mySprite.sortingOrder = _parentSprite.sortingOrder + offset;
		}
	}
}
