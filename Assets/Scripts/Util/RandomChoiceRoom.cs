using System.Collections;
using System.Collections.Generic;
using UnityEngine;

// The Random Choice room is a simple room that just 
// Chooses from a list of other rooms when it's created. 
// Think of it as the "pick a card from this hand of rooms" option
public class RandomChoiceRoom : Room {

	public GameObject[] roomChoices;

	public override Room createRoom(ExitConstraint requiredExits) {
		GameObject roomPrefab = GlobalFuncs.randElem(roomChoices);
		return roomPrefab.GetComponent<Room>().createRoom(requiredExits);
	}
}
