using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public enum Dir {
	Up,
	Right,
	Down,
	Left
}

public struct ExitConstraint {
    public bool upExitRequired;
    public bool rightExitRequired;
    public bool downExitRequired;
    public bool leftExitRequired;

    public static ExitConstraint None = new ExitConstraint();

    public ExitConstraint(bool upRequired = false, bool rightRequired = false, bool downRequired = false, bool leftRequired = false) {
        upExitRequired = upRequired;
        rightExitRequired = rightRequired;
        downExitRequired = downRequired;
        leftExitRequired = leftRequired;
    }

    public void addDirConstraint(Dir dir) {
        if (dir == Dir.Up) {
            upExitRequired = true;
        }
        else if (dir == Dir.Right) {
            rightExitRequired = true;
        }
        else if (dir == Dir.Down) {
            downExitRequired = true;
        }
        else if (dir == Dir.Left) {
            leftExitRequired = true;
        }
    }


    public IEnumerable<Vector2Int> requiredExitLocations() {
        if (upExitRequired) {
            yield return new Vector2Int(LevelGenerator.ROOM_WIDTH / 2, LevelGenerator.ROOM_HEIGHT - 1);
        }
        if (rightExitRequired) {
            yield return new Vector2Int(LevelGenerator.ROOM_WIDTH - 1, LevelGenerator.ROOM_HEIGHT / 2);
        }
        if (downExitRequired) {
            yield return new Vector2Int(LevelGenerator.ROOM_WIDTH / 2, 0);
        }
        if (leftExitRequired) {
            yield return new Vector2Int(0, LevelGenerator.ROOM_HEIGHT / 2);
        }
    }
}
