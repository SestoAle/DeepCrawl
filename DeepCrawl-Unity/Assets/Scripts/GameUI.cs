using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;
using UnityEngine.SceneManagement;

// This class manage the UI of the game and the interactions with its elements
public class GameUI : MonoBehaviour
{
  // Text in the summary window
  public Text windowText;
  public ScrollRect scrollRect;
  public RectTransform scrollPanel;
  public RectTransform altarPanel;
  public RectTransform actionPanel;
  public RectTransform gameOverPanel;

  public RectTransform[] uiElements;
  // Max number of lines displayed in the summaru window
  public int maxLine;
  // List of the line displayed in the summary window
  public List<string> textLines = new List<string>();
  public bool hasToUptade = false;

  // Progress Button
  public Button increaseAtk;
  public Button increaseDes;
  public Button increaseDef;
  public Button increaseHp;
  public Button decreaseAtk;
  public Button decreaseDef;
  public Button decreaseDes;
  public Button decreaseHp;
  public Button finishProgress;
  public Button resetProgress;
  public Text altarTitle;
  public Text atkText;
  public Text desText;
  public Text defText;
  public Text hpProgressText;

  // Texts in the inventory window
  public Text itemText;
  public Text hpText;
  public Text meleeText;
  public Text rangeText;
  public Text potionText;

  public GameObject infoIcon;
  public Image infoImage;
  public float infoOffset = 50.0f;

  // Action buttons
  public Button potionButton;
  public Button rangeButton;
  public Button compassButton;
  public Sprite healthPotionImage;
  public Sprite buffPotionImage;
  public Sprite nullPotionImage;

  // Level text
  public Text levelText;

  // Button images
  public Image potionImage;
  public Image rangeImage;
  public Image compassImage;

  public bool isRangeMode = false;

  // Popup elements to display the damages dealt by the characters
  public GameObject popupContainer;
  public Canvas canvas;
  public Canvas popupCanvas;
  public float popupOffset = 2.0f;

  // GameOver elements

  public GameObject statsPanel;

  // Tile Highlight Intensity
  public float highlightIntensity = 1.5f;
  public float highlightTime = 0.5f;

  // Array of colors for all the texts (including popu text)
  public Color[] colors;

  private void Start()
  {
    GameObject gameOver = gameOverPanel.gameObject;
    // Add the button listener to reset the game
    gameOver.transform.GetChild(1).GetComponent<Button>().onClick.AddListener(() =>
    {
      BoardManagerSystem.instance.level++;
      BoardManagerSystem.instance.resetAllGame();
      gameOver.SetActive(false);
    });
    gameOver.transform.GetChild(2).GetComponent<Button>().onClick.AddListener(() =>
    {
      SceneManager.LoadScene("StartScene");
    });
  }

  // Add text in the list with color
  public void addText(string text, int color)
  {
    if (!BoardManagerSystem.instance.noAnim)
    {
      string colorHex = ColorUtility.ToHtmlStringRGB(colors[color]);

      // If the text is too long, remove a text line
      if (textLines.Count >= 100)
      {
        textLines.RemoveAt(0);
      }

      textLines.Add(" <color=#" + colorHex + ">" + text + "</color>\n");
    }
    hasToUptade = true;
  }

  // Show all the lines in textLines
  public void updateText()
  {
    hasToUptade = false;
    string mess = "";

    foreach (string s in textLines)
    {
      mess += s;
    }

    windowText.text = mess;
    StartCoroutine(scrollToBottom());
  }

  // Add white text
  public void addText(string text)
  {
    addText(text, 0);
  }

  // Reset the summary text when the game is over
  public void resetText()
  {
    textLines.Clear();
    windowText.text = "";
  }

  // These methods are usde to display the item informations above it. 
  public void moveItemText(Vector3 position)
  {
    position = new Vector3(position.x, position.y, position.z + 10);
    itemText.transform.position = position;
  }

  public void activateItemtext(bool act)
  {
    itemText.enabled = act;
  }

  public void setItemText(string text)
  {
    itemText.text = text;
  }

  // Update the HP text
  public void setHp(int hp, int maxHp, int color)
  {
    string colorHex = ColorUtility.ToHtmlStringRGB(colors[color]);
    hpText.text = "<color=#" + colorHex + ">HP: " + hp + "/" + maxHp + "</color>";
  }

  // Update the Melee weapon text
  public void setMelee(string text, string dmg)
  {
    meleeText.text = text;
    meleeText.text += ": " + dmg;
  }

  // Update the Range weapon text
  public void setRange(string text, string dmg, string rng)
  {
    rangeText.text = text;
    rangeText.text += ": " + dmg + ", range " + rng;
  }

  // Update the Potion text
  public void setPotion(string text)
  {
    potionText.text = text;
  }

  // Create the popup text for the damages. This text will appear above the 
  // character who have the damage component, and it will follow its owner until
  // it disappear. The movements are managed by the ECS. The method returns
  // the istance of the PopupText (to pass it as a component).
  public GameObject createPopupText(string text, int color)
  {
    // Instantiate a PopupText prefab and get the references
    GameObject instance = Instantiate(popupContainer);
    GameObject popupText = instance.transform.GetChild(0).gameObject;
    Animator popupAnimator = popupText.GetComponent<Animator>();

    // Set the color of the text
    popupText.GetComponent<Text>().color = colors[color];
    // Set the tect
    popupText.GetComponent<Text>().text = text;

    // Set the initial position of the text away from the UI, to avoid graphic
    // glitches
    instance.transform.SetParent(popupCanvas.transform, false);
    instance.transform.position = new Vector3(-1000, -1000, -1000);
    // Destroy the gameo object when its animation is finished
    AnimatorClipInfo[] clipInfo = popupAnimator.GetCurrentAnimatorClipInfo(0);
    Destroy(instance, clipInfo[0].clip.length - 0.1f);

    // return the instance of the PopupText
    return instance;
  }

  // Show the GameOver panel
  public void showGameOver(bool lost)
  {
    // Get tje gameOverPanel and set it active
    GameObject gameOver = gameOverPanel.gameObject;
    gameOver.SetActive(true);
    gameOver.transform.SetParent(canvas.transform, false);

    // Change the text depending on the win/loose condition
    if (lost)
    {
      gameOver.transform.GetChild(2).gameObject.SetActive(true);
      gameOver.transform.GetChild(0).GetComponent<Text>().text = "You died...";
      gameOver.transform.GetChild(1).gameObject.SetActive(false);
    }
    else
    {
      gameOver.transform.GetChild(1).gameObject.SetActive(true);
      gameOver.transform.GetChild(0).GetComponent<Text>().text = "You cleared the level!";
      gameOver.transform.GetChild(2).gameObject.SetActive(false);
    }
  }

  // Show the EndGame panel
  public void showGameOver()
  {
    // Instantiate the prefab and set its parent
    GameObject gameOver = gameOverPanel.gameObject;
    gameOver.SetActive(true);
    gameOver.transform.SetParent(canvas.transform, false);
    gameOver.transform.GetChild(2).gameObject.SetActive(true);
    gameOver.transform.GetChild(0).GetComponent<Text>().text = "You won the game!";
    gameOver.transform.GetChild(1).gameObject.SetActive(false);
  }

  // Change the alpha to 0.5 of the image
  public void changeImageAlpha(bool highlights, Image image)
  {
    if (highlights)
    {
      image.color = new Color(1.0f, 1.0f, 1.0f, 1.0f);
    }
    else
    {
      image.color = new Color(1.0f, 1.0f, 1.0f, 0.58f);
    }
  }

  // Change the alpha to 0.5 of background of the image
  public void changeImageBackground(Image image, int color)
  {
    image.color = colors[color] + new Color(0f, 0f, 0f, 0.58f);
  }

  // Filling animation of info icon
  public void startFillingIcon()
  {
    infoIcon.transform.GetChild(0).GetComponent<Image>().fillAmount += 0.03f;
    infoIcon.transform.GetChild(1).GetComponent<Image>().enabled = true;
  }

  // Fill info icon completely
  public void endFillingIcon()
  {
    infoIcon.transform.GetChild(0).GetComponent<Image>().fillAmount = 0;
    infoIcon.transform.GetChild(1).GetComponent<Image>().enabled = false;
  }

  // Scroll the scroll rect to the bottom
  IEnumerator scrollToBottom()
  {
    //LayoutRebuilder.ForceRebuildLayoutImmediate(scrollPanel);
    yield return new WaitForEndOfFrame();
    yield return new WaitForEndOfFrame();
    scrollRect.verticalNormalizedPosition = 0;
  }

  // Move camera at position x and y
  public void moveCameraAtPosition(float x, float y)
  {
    Vector3 cameraOffset = GameManager.instance.cameraOffset;
    Vector3 cameraRotation = GameManager.instance.cameraRotation;
    Vector3 newCameraPos;
    newCameraPos.x = x - cameraOffset.x;
    newCameraPos.z = y - cameraOffset.z;
    newCameraPos.y = cameraOffset.y;
    Camera.main.transform.position = newCameraPos;
    Camera.main.transform.rotation = Quaternion.Euler(cameraRotation);
  }

  // Display altar dialog
  public void showAltarDialog()
  {
    StartCoroutine(altarRoutine());
  }

  // Change altar dialogue text
  public void setAltarText(Altar altar, Stats stats)
  {
    altarTitle.text = "The altar gives you power. You have " + altar.actualPoints + " points, choose wisely";

    hpProgressText.text = "HP " + stats.maxHp;
    atkText.text = "ATK " + stats.atk;
    desText.text = "DEX " + stats.des;
    defText.text = "DEF " + stats.def;
  }

  public IEnumerator altarRoutine()
  {
    yield return new WaitForEndOfFrame();
    altarPanel.gameObject.SetActive(!altarPanel.gameObject.activeInHierarchy);
    actionPanel.gameObject.SetActive(!altarPanel.gameObject.activeInHierarchy);
    scrollPanel.gameObject.SetActive(!altarPanel.gameObject.activeInHierarchy);
  }

  // Set the level text
  public void changeLevelText(int level)
  {
    levelText.text = "Level " + level;
  }

  // Change the image of the potion button depending on if it's a buff or health
  public void changeImagePotion(bool isBuff)
  {
    if (isBuff)
    {
      potionImage.sprite = buffPotionImage;
    }
    else
    {
      potionImage.sprite = healthPotionImage;
    }
  }

  // Change the image of the potion to NonePotion
  public void setNullImagePotion()
  {
    potionImage.sprite = nullPotionImage;
  }
}
