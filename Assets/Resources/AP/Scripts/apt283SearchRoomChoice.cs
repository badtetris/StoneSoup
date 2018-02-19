using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class apt283SearchRoomChoice : RandomChoiceRoom {

	protected static List<apt283SearchRoom> _validRooms = new List<apt283SearchRoom>();

	public override Room createRoom(params Dir[] requiredExits) {
		// We're going to choose a room based on which ones appear to fit the requirements. 
		_validRooms.Clear();

		foreach (GameObject roomPrefab in roomChoices) {
			apt283SearchRoom searchRoom = roomPrefab.GetComponent<apt283SearchRoom>();

			if (requiredExits.Length == 0) {
				_validRooms.Add(searchRoom);
			}
			// If we only have one required exit, just check if the entrance exists.
			else if (requiredExits.Length == 1) {
				if (searchRoom.doesEntranceExist(requiredExits[0])) {
					_validRooms.Add(searchRoom);
				}
			}
			else {
				if (searchRoom.doesPathExist(requiredExits[0], requiredExits[1])) {
					_validRooms.Add(searchRoom);
				}
			}
		}

		Room chosenRoom = GlobalFuncs.randElem(_validRooms);
		return chosenRoom.createRoom(requiredExits);
	}
}
