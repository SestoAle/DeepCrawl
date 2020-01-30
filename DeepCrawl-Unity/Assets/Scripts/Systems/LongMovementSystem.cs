using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using Unity.Entities;

public class LongMovementSystem : ComponentSystem
{

    public struct Data
    {
        public readonly int Length;
        public EntityArray Entity;
        public GameObjectArray GameObjects;
        public BufferArray<MovementElementBuffer> movementBuffers;
        public ComponentDataArray<Position> Position;

        public SubtractiveComponent<Movement> movement;
    }

    [Inject] private Data data;

    // Get the roation direction of the movement, computing the offset from 
    // start tile to the end tile
    public DIRECTION offsetToRotation(Vector3 startPosition, Vector3 endPosition)
    {
        Vector3 offset = endPosition - startPosition;
        if (offset == new Vector3(0, 0, 1))
        {
            return DIRECTION.North;
        }
        if (offset == new Vector3(1, 0, 0))
        {
            return DIRECTION.East;
        }
        if (offset == new Vector3(0, 0, -1))
        {
            return DIRECTION.South;
        }
        if (offset == new Vector3(-1.0f, 0.0f, 0.0f))
        {
            return DIRECTION.West;
        }
        if (offset == new Vector3(1, 0, 1))
        {
            return DIRECTION.NorthEast;
        }
        if (offset == new Vector3(1, 0, -1))
        {
            return DIRECTION.SouthEast;
        }
        if (offset == new Vector3(-1, 0, -1))
        {
            return DIRECTION.SouthWest;
        }
        if (offset == new Vector3(-1.0f, 0.0f, 1.0f))
        {
            return DIRECTION.NorthWest;
        }

        return DIRECTION.North;
    }

    protected override void OnUpdate()
    {
        if (BoardManagerSystem.instance.isTraning)
            return;

        for (int i = 0; i < data.Length; i++)
        {


            // Get the movement components buffer
            DynamicBuffer<MovementElementBuffer> mb = data.movementBuffers[i];
            if (mb.Length > 0)
            {
                // Change camera mode to automatic
                GameManager.instance.cameraManual = false;

                // Get the current tile
                Tile tile = BoardManagerSystem.instance.getTileFromObject(data.GameObjects[i]);

                if (!BoardManagerSystem.instance.noAnim && data.GameObjects[i].tag == "Player")
                    BoardManagerSystem.instance.deHighlightAll(tile.parent);

                // If the player open a room with an enemy within it,
                // end the long movement action
                if (tile.parent.hasEnemy())
                {
                    Movement lastMovement = new Movement { x = tile.x, y = tile.y };
                    PostUpdateCommands.AddComponent(data.Entity[i], lastMovement);
                    mb.Clear();
                    return;
                }

                // Get the player position
                Vector3 position = new Vector3(data.Position[i].x, 0, data.Position[i].y);
                // Get the first element of the buffer
                MovementElementBuffer movementElement = mb[0];
                // Get the new tile from the movement component
                Tile newTile = BoardManagerSystem.instance.getTile(movementElement.x, movementElement.y);
                // If the player can not move in the next tile
                if (!newTile.canMove())
                {
                    // Execute the last movement and then end the action
                    Movement lastMovement = new Movement { x = tile.x, y = tile.y };
                    PostUpdateCommands.AddComponent(data.Entity[i], lastMovement);
                    // If the tile has an interactable, interact with it and end the movement
                    GameObject interactableObject = newTile.getInteractable();
                    Entity interactableEntity = interactableObject.GetComponent<GameObjectEntity>().Entity;
                    if (!EntityManager.HasComponent<Interact>(interactableEntity))
                        PostUpdateCommands.AddComponent(interactableEntity, new Interact { });
                    mb.Clear();
                    return;
                }
                // Set the character of the current tile to null
                tile.setCharacter(null);
                // Set the character of the next tile
                newTile.setCharacter(data.GameObjects[i]);
                // Remove the element from the buffer
                mb.RemoveAt(0);
                // Add a new movement component
                Movement movement = new Movement { x = movementElement.x, y = movementElement.y };
                // Add a rotation component
                int rotationY = (int)offsetToRotation(position, new Vector3(movementElement.x, 0, movementElement.y));
                Rotation rotation = new Rotation { rotationY = rotationY };

                PostUpdateCommands.AddComponent(data.Entity[i], rotation);
                PostUpdateCommands.AddComponent(data.Entity[i], movement);
            }
        }
    }
}
