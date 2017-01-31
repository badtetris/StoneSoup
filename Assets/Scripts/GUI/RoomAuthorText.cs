using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;

public class RoomAuthorText : MonoBehaviour {

	protected Text _text;

	// Use this for initialization
	void Start () {
		_text = GetComponent<Text>();
	}
		
	// Update is called once per frame
	void Update () {
		_text.text = string.Format("Room by: {0}", GameManager.instance.currentRoom.roomAuthor);
	}
}
