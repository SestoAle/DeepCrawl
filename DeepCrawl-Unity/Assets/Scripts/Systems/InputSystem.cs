using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using Unity.Entities;
using MLAgents;
using UnityEngine.EventSystems;

[UpdateAfter(typeof(TurnSystem))]
public class InputSystem : ComponentSystem
{

    public struct Data
    {
        public readonly int Length;
        public EntityArray Entity;
        public GameObjectArray GameObject;
        public ComponentDataArray<Turn> Turns;
        public ComponentDataArray<Stats> Stats;

        public SubtractiveComponent<Movement> Movements;
        public SubtractiveComponent<Attack> Attacks;
        public SubtractiveComponent<UserInput> UserInput;
        public SubtractiveComponent<EndTurn> EndTurns;
        public SubtractiveComponent<Damage> Damage;
    }

    [Inject] private Data data;

    // Highlights the range tile
    void highlightRange(int range, Tile startTile, GameObject character)
    {
        foreach (DIRECTION dir in System.Enum.GetValues(typeof(DIRECTION)))
        {
            Vector2 offset = GameManager.instance.directionToTile((int)dir);
            Tile newTile = startTile;
            Tile prevTile = startTile;
            for (int i = 0; i < range; i++)
            {
                newTile = BoardManagerSystem.instance.getTile((int)(prevTile.x + offset.x), (int)(prevTile.y + offset.y));

                if (newTile == null)
                    break;

                if (newTile.parent != startTile.parent)
                    break;

                if (!prevTile.isNeighbour(newTile))
                    break;

                if (newTile.canMove())
                    newTile.rangeHighlight();

                if (newTile.hasCharacter() && !newTile.isAround(character))
                    newTile.highlight();

                prevTile = newTile;

                if (!newTile.canMove() || newTile.hasCharacter())
                {
                    break;
                }
            }
        }
    }

    public float timeHold;
    public bool longTouch;
    public bool shortTouch;
    public bool isMoving;
    Vector3 prevMousePosition;


    protected override void OnUpdate()
    {
        for (var i = 0; i < data.Length; i++)
        {
            if (data.Turns[i].hasTurn == 1 && data.Turns[i].hasEndedTurn != 1)
            {
                // Brained agent
                if (data.GameObject[i].tag == "BrainedAgent" || BoardManagerSystem.instance.doubleAgent)
                {
                    if (data.GameObject[i].GetComponent<BaseAgent>().brain.brainType == BrainType.Player)
                    {
                        if (Input.anyKeyDown)
                        {
                            BaseAgent agent = data.GameObject[i].GetComponent<BaseAgent>();
                            // Give the reward and check if the enemy or the agent are dead
                            agent.giveHpReward();
                            agent.giveDamageReward();

                            agent.RequestDecision();
                            //PostUpdateCommands.AddComponent(data.Entity[i], new Wait { });
                        }
                    }
                    else
                    {
                        BaseAgent agent = data.GameObject[i].GetComponent<BaseAgent>();
                        // Give the reward and check if the enemy or the agent are dead
                        agent.giveHpReward();
                        agent.giveDamageReward();
                        // Request decision from the net
                        agent.RequestDecision();
                    }

                }
                else if (data.GameObject[i].tag == "Random")
                {
                    // Make a random actions
                    int numAction = (int)BoardManagerSystem.instance.academy.resetParameters["numActions"];
                    PostUpdateCommands.AddComponent(data.Entity[i], new UserInput { action = Random.Range(0, numAction) });
                }
                else
                // Player inputs management
                {
                    // Get the start tile of the character
                    Tile startTile = BoardManagerSystem.instance.getTileFromObject(data.GameObject[i]);

                    // If touch count is 2, reset all the variables
                    if (Input.touchCount == 2)
                    {
                        GameManager.instance.gameUI.endFillingIcon();
                        GameManager.instance.resetMousePos();
                        prevMousePosition = new Vector3();
                        isMoving = false;
                        timeHold = 0f;
                        longTouch = false;
                        shortTouch = false;
                        return;
                    }

                    if (GameManager.instance.gameUI.isRangeMode)
                    {
                        // Highlights the range tiles
                        highlightRange(data.Stats[i].actualRange, startTile, data.GameObject[i]);
                    }
                    else
                    {
                        // Highlights the neighbours
                        if (data.GameObject[i].tag == "Player")
                        {
                            if (!BoardManagerSystem.instance.isTraning)
                            {
                                startTile.highlightNeighbours();
                            }
                        }

                    }

                    // Activate the range mode
                    if (GameManager.instance.gameUI.rangeButton.GetComponent<ButtonManager>().GetButtonDown())
                    {
                        BoardManagerSystem.instance.deHighlightAll(startTile.parent);
                        if (!GameManager.instance.gameUI.isRangeMode)
                        {
                            GameManager.instance.gameUI.isRangeMode = true;
                            GameManager.instance.gameUI.addText("Enter range mode", 4);
                            GameManager.instance.gameUI.addText("Choose a shoot direction", 4);
                        }
                        else
                        {
                            GameManager.instance.gameUI.isRangeMode = false;
                            GameManager.instance.gameUI.addText("Exit range mode", 4);
                        }
                        timeHold = 0;
                        return;
                    }

                    // If the compass button is pressed, change the camera mode to automatic
                    if (GameManager.instance.gameUI.compassButton.GetComponent<ButtonManager>().GetButtonDown())
                    {
                        BoardManagerSystem.instance.deSelectAll();
                        GameManager.instance.cameraManual = false;
                        GameManager.instance.resetMousePos();
                        prevMousePosition = new Vector3();
                        timeHold = 0;
                        return;
                    }

                    // If potion button is pressed, execute the potion action (8)
                    if (GameManager.instance.gameUI.potionButton.GetComponent<ButtonManager>().GetButtonDown())
                    {
                        BoardManagerSystem.instance.deSelectAll();
                        PostUpdateCommands.AddComponent(data.Entity[i], new UserInput { action = 8 });
                        shortTouch = false;
                        timeHold = 0;
                        return;
                    }

                    // If a UI element is pressed, don't propagate the pressed event
                    foreach (RectTransform ui in GameManager.instance.gameUI.uiElements)
                    {
                        if (ui.GetComponent<ButtonManager>().GetPressedDown())
                        {
                            shortTouch = false;
                            longTouch = false;
                            timeHold = 0;
                            return;
                        }
                    }

                    // If the touch time is less then 0.4 seconds, is a shortTouch
                    if (Input.GetMouseButtonUp(0) && timeHold > 0f && Time.time - timeHold < 0.4f && !isMoving)
                    {
                        shortTouch = true;
                        timeHold = 0f;
                    }

                    // When the touch is finished, reset all the variables
                    if (Input.GetMouseButtonUp(0))
                    {
                        GameManager.instance.gameUI.endFillingIcon();
                        GameManager.instance.resetMousePos();
                        prevMousePosition = new Vector3();
                        isMoving = false;
                        timeHold = 0f;
                        longTouch = false;
                    }

                    Tile clickedTile = null;

                    if (Input.GetMouseButtonDown(0))
                    {
                        timeHold = Time.time;
                        prevMousePosition = Input.mousePosition;
                    }

                    // If is a swipe, change the camera mode to manual
                    if (timeHold > 0f && (Vector3.Distance(Input.mousePosition, prevMousePosition) > 30 || isMoving))
                    {
                        isMoving = true;
                        GameManager.instance.gameUI.compassImage.GetComponent<Animator>().SetTrigger("magnify");
                        GameManager.instance.moveManualCamera(Input.mousePosition);
                        BoardManagerSystem.instance.deSelectAll();
                        return;
                    }

                    // If the touch is longer then 0.4 seconds, is a long touch
                    if (timeHold > 0f && Time.time - timeHold > 0.4f)
                    {
                        longTouch = true;
                        timeHold = 0f;
                    }

                    // If is a short touch, do the action
                    if (shortTouch && (!EventSystem.current.IsPointerOverGameObject()) && !isMoving)
                    {
                        // Get the touched tile
                        RaycastHit hit;
                        if (Physics.Raycast(Camera.main.ScreenPointToRay(Input.mousePosition), out hit))
                        {
                            clickedTile = hit.collider.gameObject.GetComponent<Tile>();
                            if(!clickedTile.GetComponent<Renderer>().enabled)
                            {
                                clickedTile = null;
                            }
                        }

                        shortTouch = false;
                        Vector3 offset = new Vector3();

                        // If the tile pressed is a near tile or a range tile, then execute the action
                        if (!GameManager.instance.gameUI.isRangeMode && clickedTile != null)
                        {

                            offset = clickedTile.getPosition() - startTile.getPosition();

                            // If there is no enemy in the room, execute a long movement
                            if (clickedTile != null && (clickedTile.canMove() || clickedTile.hasInteractable()) && !startTile.parent.hasEnemy()
                                && !BoardManagerSystem.instance.isTraning)
                            {
                                if (EntityManager.HasComponent<MovementElementBuffer>(data.Entity[i]))
                                {
                                    // Create the movement buffer
                                    DynamicBuffer<MovementElementBuffer> movementBuffer = EntityManager.GetBuffer<MovementElementBuffer>(data.Entity[i]);
                                    // Get the best path of tiles
                                    List<Tile> path = BoardManagerSystem.instance.findPath(startTile, clickedTile);
                                    for (int t = path.Count - 1; t >= 0; t--)
                                    {
                                        // Add the movement components to the buffer
                                        Tile tile = path[t];
                                        movementBuffer.Add(new MovementElementBuffer { x = tile.x, y = tile.y });
                                    }
                                    return;
                                }
                            }

                            if (!clickedTile.isSelected)
                            {
                                BoardManagerSystem.instance.deSelectAll();
                                clickedTile.selectTile();
                            }

                            // Movement actions (0-7)
                            if ((int)offset.x == 0 && (int)offset.z == 1)
                            {
                                PostUpdateCommands.AddComponent(data.Entity[i], new UserInput { action = 0 });
                            }
                            if ((int)offset.x == 1 && (int)offset.z == 0)
                            {
                                PostUpdateCommands.AddComponent(data.Entity[i], new UserInput { action = 1 });
                            }
                            if ((int)offset.x == 0 && (int)offset.z == -1)
                            {
                                PostUpdateCommands.AddComponent(data.Entity[i], new UserInput { action = 2 });
                            }
                            if ((int)offset.x == -1 && (int)offset.z == 0)
                            {
                                PostUpdateCommands.AddComponent(data.Entity[i], new UserInput { action = 3 });
                            }
                            if ((int)offset.x == 1 && (int)offset.z == 1)
                            {
                                PostUpdateCommands.AddComponent(data.Entity[i], new UserInput { action = 4 });
                            }
                            if ((int)offset.x == 1 && (int)offset.z == -1)
                            {
                                PostUpdateCommands.AddComponent(data.Entity[i], new UserInput { action = 5 });
                            }
                            if ((int)offset.x == -1 && (int)offset.z == -1)
                            {
                                PostUpdateCommands.AddComponent(data.Entity[i], new UserInput { action = 6 });
                            }
                            if ((int)offset.x == -1 && (int)offset.z == 1)
                            {
                                PostUpdateCommands.AddComponent(data.Entity[i], new UserInput { action = 7 });
                            }
                        }
                        else
                        {
                            if (clickedTile == null)
                                continue;
                            offset = clickedTile.getPosition() - startTile.getPosition();
                            // Ranged actions (9-16)
                            if ((int)offset.x == 0 && (int)offset.z >= 1)
                            {
                                PostUpdateCommands.AddComponent(data.Entity[i], new UserInput { action = 9 });
                            }
                            if (offset.x >= 1 && (int)offset.z == 0)
                            {
                                PostUpdateCommands.AddComponent(data.Entity[i], new UserInput { action = 10 });
                            }
                            if ((int)offset.x == 0 && (int)offset.z <= -1)
                            {
                                PostUpdateCommands.AddComponent(data.Entity[i], new UserInput { action = 11 });
                            }
                            if ((int)offset.x <= -1 && (int)offset.z == 0)
                            {
                                PostUpdateCommands.AddComponent(data.Entity[i], new UserInput { action = 12 });
                            }
                            if ((int)offset.x >= 1 && (int)offset.z >= 1)
                            {
                                PostUpdateCommands.AddComponent(data.Entity[i], new UserInput { action = 13 });
                            }
                            if ((int)offset.x >= 1 && (int)offset.z <= -1)
                            {
                                PostUpdateCommands.AddComponent(data.Entity[i], new UserInput { action = 14 });
                            }
                            if ((int)offset.x <= -1 && (int)offset.z <= -1)
                            {
                                PostUpdateCommands.AddComponent(data.Entity[i], new UserInput { action = 15 });
                            }
                            if ((int)offset.x <= -1 && (int)offset.z >= 1)
                            {
                                PostUpdateCommands.AddComponent(data.Entity[i], new UserInput { action = 16 });
                            }
                        }
                    }

                    // If is a long touch, log the characteristics of the 
                    // pressed item
                    if (longTouch && (!EventSystem.current.IsPointerOverGameObject()))
                    {
                        // Get the pressed tile
                        RaycastHit hit;
                        if (Physics.Raycast(Camera.main.ScreenPointToRay(Input.mousePosition), out hit))
                        {
                            clickedTile = hit.collider.gameObject.GetComponent<Tile>();
                            if (!clickedTile.GetComponent<Renderer>().enabled)
                            {
                                continue;
                            }
                            if (!clickedTile.isSelected)
                            {
                                BoardManagerSystem.instance.deSelectAll();
                                clickedTile.selectTile();
                            }
                        }
                        if (clickedTile == null)
                            continue;

                        GameUI gameUI = GameManager.instance.gameUI;

                        // If the ray hit a tile with an item,
                        // get the hitem hit
                        Item item = clickedTile.getItem();
                        GameObject character = (GameObject)clickedTile.getCharacter();

                        if (item != null || character != null)
                        {
                            // Display the info icon and start filling it
                            gameUI.startFillingIcon();
                            gameUI.infoIcon.transform.position = Input.mousePosition + Camera.main.ViewportToScreenPoint(new Vector3(0, gameUI.infoOffset, 0));

                            if (gameUI.infoImage.fillAmount < 1.0f)
                                return;
                        }

                        // if the info icon is filled
                        if (item != null)
                        {
                            // Log the item characteristics on the UI
                            if (item.GetType() == typeof(MeleeWeapon))
                            {
                                gameUI.addText(item.itemName + ",", 0);
                                gameUI.addText("damage: " + ((MeleeWeapon)item).damageString, 0);
                            }

                            if (item.GetType() == typeof(RangeWeapon))
                            {
                                gameUI.addText(item.itemName + ",", 0);
                                gameUI.addText("damage = " + ((RangeWeapon)item).damageString + ", range = " + ((RangeWeapon)item).range, 0);
                            }

                            if (item.GetType() == typeof(BuffPotion))
                            {
                                gameUI.addText(item.itemName + ",", 0);
                                gameUI.addText("def + " + ((BuffPotion)item).def + ", atk + " + ((BuffPotion)item).atk + ", turns: " + ((BuffPotion)item).duration, 0);
                            }

                            if (item.GetType() == typeof(HealthPotion))
                            {
                                gameUI.addText(item.itemName + ",", 0);
                                gameUI.addText("hp + " + ((HealthPotion)item).hp, 0);
                            }
                        }

                        // Log the character statistics on the UI
                        if (character != null)
                        {
                            Inventory pressedInventory = character.GetComponent<Inventory>();
                            EntityManager entityManager = World.Active.GetExistingManager<EntityManager>();
                            Stats pressedStats = entityManager.GetComponentData<Stats>(character.GetComponent<Character>().Entity);
                            gameUI.addText(character.name, 3);
                            gameUI.addText("Enemy Hp: " + pressedStats.hp, 3);
                            gameUI.addText("Melee: " + pressedInventory.meeleWeapon.itemName + ", damage:" + pressedInventory.meeleWeapon.damageString, 3);
                            gameUI.addText("Range: " + pressedInventory.rangeWeapon.itemName + ", damage:" + pressedInventory.rangeWeapon.damageString, 3);
                            if (pressedInventory.potion == null)
                            {
                                gameUI.addText("Potion: None", 3);
                            }
                            else
                            {
                                gameUI.addText("Potion: " + pressedInventory.potion.itemName, 3);
                            }
                        }
                        longTouch = false;
                        gameUI.endFillingIcon();
                    }
                }
            }
        }
    }
}
