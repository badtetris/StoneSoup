using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;

public class PlayerItemText : MonoBehaviour {

	protected Text _text;

	// Use this for initialization
	void Start () {
		_text = GetComponent<Text>();
	}
		
	// Update is called once per frame
	void Update () {
		if (Player.instance != null) {
			if (Player.instance.tileWereHolding != null) {
				_text.text = string.Format("Item: {0}", Player.instance.tileWereHolding.tileName);
			}
			else {
				_text.text = "Item: None";
			}
		}
	}
}
