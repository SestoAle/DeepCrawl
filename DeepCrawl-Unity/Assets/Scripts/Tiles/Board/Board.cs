using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class Board
{

  public int[,] roomsMap;
  public Tile[,] map;
  public List<Room> rooms;
  public GameObject[,] doors;
  int roomsSize;

  List<int[]> directions = new List<int[]>{
      new int[] {0,1},
      new int[] {1,0},
      new int[] {0,-1},
      new int[] {-1,0},
    };

  public Board(int numRooms, int roomsSize)
  {
    this.roomsSize = roomsSize;
    int roomsMapSize = Mathf.Max(3, numRooms - 3);
    int totalSize = roomsSize * roomsMapSize;
    // Create the tile map
    map = new Tile[totalSize, totalSize];
    // Create the doors map
    doors = new GameObject[totalSize, totalSize];
    // Create the list of rooms
    rooms = new List<Room>();
    // Generate the room map
    roomsMap = generateRoomsMap(numRooms, roomsMapSize);

    // For each room
    foreach (Room room in rooms)
    {
      if (room == null)
      {
        continue;
      }
      // Generate tiles of the room
      room.generateTiles();
      // Generate the number of the enemies of the room
      room.computeNumEnemies();
      // Link the tiles of the room
      room.GenerateNeighbours();
    }

    foreach (Room room in rooms)
    {
      if (room == null)
      {
        continue;
      }
      // Link the room neighbours
      room.generateRoomLink();
    }
  }

  // Generate the rooms
  public int[,] generateRoomsMap(int roomNumber, int size)
  {
    int[,] roomsMap = new int[size, size];
    int[] currentRoom = { (int)size / 2, (int)size / 2 };
    List<Room> createdRoom = new List<Room>();
    roomsMap[currentRoom[0], currentRoom[1]] = 1;
    int currentCount = 0;
    Room room = new Room(currentCount, this, roomsSize);
    room.setCoords(new int[] { currentRoom[0] * roomsSize, currentRoom[1] * roomsSize });
    room.setRoomMapCords(currentRoom);
    room.setSize(roomsSize);
    rooms.Add(room);
    createdRoom.Add(room);

    while (currentCount < roomNumber - 1)
    {
      // Choose a random room already created
      Room parentRoom = createdRoom[Random.Range(0, createdRoom.Count)];
      // Get the room coordinates in the roomMap
      currentRoom = parentRoom.getRoomMapCoords();
      // Get a random direction
      int directionIndex;
      if(currentCount == 0)
      {
        directionIndex = 0;
      }
      else
      {
        directionIndex = Random.Range(0, directions.Count);
      }

      // Get the new coordinates
      int[] nextRoom =
      {
        currentRoom[0] + directions[directionIndex][0],
        currentRoom[1] + directions[directionIndex][1]
      };
      int tries = 1;

      while ((nextRoom[0] < 0 || nextRoom[1] < 0 || nextRoom[0] >= size || nextRoom[1] >= size) ||
             (roomsMap[nextRoom[0], nextRoom[1]] == 1 && tries < directions.Count))
      {
        // Get the next directions
        directionIndex = directionIndex % (directions.Count - 1) + 1;
        nextRoom = new int[]
        {
          currentRoom[0] + directions[directionIndex][0],
          currentRoom[1] + directions[directionIndex][1]
        };

        tries++;
      }

      if (tries >= 4)
      {
        continue;
      }

      // Increment room count
      currentCount++;
      // Create new room
      Room newRoom = new Room(currentCount, this, roomsSize);
      // Add the new room to the list of created rooms
      createdRoom.Add(newRoom);
      // Set real coords of the new room
      newRoom.setCoords(new int[] { nextRoom[0] * roomsSize, nextRoom[1] * roomsSize });
      // Set the room map coords of the new room
      newRoom.setRoomMapCords(nextRoom);
      // Update roomMap
      roomsMap[nextRoom[0], nextRoom[1]] = 1;
      rooms.Add(newRoom);
      // Add the children room to the parent
      parentRoom.addRoomNeigh(directions[directionIndex], newRoom);
      // Add the parent room to the children
      int[] prevDirections = { directions[directionIndex][0] * -1, directions[directionIndex][1] * -1 };
      newRoom.addRoomNeigh(prevDirections, parentRoom);
    }

    return roomsMap;
  }

  // Add the tile to the general map
  public void addTile(Tile tile, int x, int y)
  {
    map[x, y] = tile;
  }

  public void addTile(Tile tile, int x, int y, Room room)
  {
    x = x + room.getCoords()[0];
    y = y + room.getCoords()[1];
    map[x, y] = tile;
  }

  public Tile getTile(int x, int y)
  {
    if (x < 0 || y < 0 || x >= map.GetLength(0) || y >= map.GetLength(1))
    {
      return null;
    }

    return map[x, y];
  }

  public GameObject getDoor(int x, int y)
  {
    if (x < 0 || y < 0 || x >= doors.GetLength(0) || y >= doors.GetLength(1))
    {
      return null;
    }

    return doors[x, y];
  }

  // Get the start room of the level
  public Room getStartRoom()
  {
    foreach (Room r in rooms)
    {
      if (r != null && r.getId() == 0)
      {
        return r;
      }
    }
    return null;
  }

  // Get a random tile from the general map
  public Tile getRandomTile()
  {
    Tile tile = null;
    while (tile == null)
    {
      int x = Random.Range(0, map.GetLength(0));
      int y = Random.Range(0, map.GetLength(1));
      tile = map[x, y];
    }

    return tile;
  }

  public Tile[,] getTiles()
  {
    return map;
  }
}

