using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class apt283SearchRoomChoice : RandomChoiceRoom {

	protected static List<apt283SearchRoom> _validRooms = new List<apt283SearchRoom>();

	public override Room createRoom(ExitConstraint requiredExits) {
		// We're going to choose a room based on which ones appear to fit the requirements. 
		_validRooms.Clear();

		foreach (GameObject roomPrefab in roomChoices) {
			apt283SearchRoom searchRoom = roomPrefab.GetComponent<apt283SearchRoom>();

            if (requiredExits.upExitRequired && !searchRoom.doesEntranceExist(Dir.Up)) {
                continue;
            }
            if (requiredExits.rightExitRequired && !searchRoom.doesEntranceExist(Dir.Right)) {
                continue;
            }
            if (requiredExits.downExitRequired && !searchRoom.doesEntranceExist(Dir.Down)) {
                continue;
            }
            if (requiredExits.leftExitRequired && !searchRoom.doesEntranceExist(Dir.Left)) {
                continue;
            }

            if (requiredExits.upExitRequired
                && requiredExits.rightExitRequired
                && !searchRoom.doesPathExist(Dir.Up, Dir.Right)) {
                continue;
            }
            if (requiredExits.upExitRequired
                && requiredExits.downExitRequired
                && !searchRoom.doesPathExist(Dir.Up, Dir.Down)) {
                continue;
            }
            if (requiredExits.upExitRequired
                && requiredExits.leftExitRequired
                && !searchRoom.doesPathExist(Dir.Up, Dir.Left)) {
                continue;
            }
            if (requiredExits.rightExitRequired
                && requiredExits.downExitRequired
                && !searchRoom.doesPathExist(Dir.Right, Dir.Down)) {
                continue;
            }
            if (requiredExits.rightExitRequired
                && requiredExits.leftExitRequired
                && !searchRoom.doesPathExist(Dir.Right, Dir.Down)) {
                continue;
            }
            if (requiredExits.downExitRequired
                && requiredExits.leftExitRequired
                && !searchRoom.doesPathExist(Dir.Down, Dir.Left)) {
                continue;
            }

            _validRooms.Add(searchRoom);
			
		}

		Room chosenRoom = GlobalFuncs.randElem(_validRooms);
		return chosenRoom.createRoom(requiredExits);
	}
}
