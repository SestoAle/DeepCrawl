using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.SceneManagement;
using UnityEngine.UI;


public class StartManagerSystem : MonoBehaviour
{

  [Header("---Difficulty Points---")]
  public int hard = 3;
  public int medium = 4;
  public int easy = 5;
  [Space(10)]

  [Header("---UI Elements---")]
  public Button easyButton;
  public Button mediumButton;
  public Button hardButton;
  public Animator[] animators;

  // Set the listener of the mode buttons
  void Start()
  {
    easyButton.onClick.AddListener(() => {
      BoardManagerSystem.difficulty = easy;
      disableAllButtons();
      startAnimation(easyButton);
      StartCoroutine(StartGame());
    });

    mediumButton.onClick.AddListener(() => {
      BoardManagerSystem.difficulty = medium;
      disableAllButtons();
      startAnimation(mediumButton);
      StartCoroutine(StartGame());
    });

    hardButton.onClick.AddListener(() => {
      BoardManagerSystem.difficulty = hard;
      disableAllButtons();
      startAnimation(hardButton);
      StartCoroutine(StartGame());
    });
  }

  // Start the fade animation of the texts
  public void startAnimation(Button button)
  {
    foreach (Animator a in animators)
    {
      if(a.gameObject != button.gameObject)
      {
        a.Play("Fade");
      }
        
    }
  }

  // Disable all the buttons
  public void disableAllButtons()
  {
    easyButton.enabled = false;
    mediumButton.enabled = false;
    hardButton.enabled = false;
  }

  // Start the main scene
  public IEnumerator StartGame()
  {
    bool isPlaying = false;

    // Wait until fade animation is finished
    while (!isPlaying)
    {
      isPlaying = false;
      foreach (Animator a in animators)
      {
        if (a.GetCurrentAnimatorStateInfo(0).IsName("Fade"))
        {
          isPlaying = true;
          break;
        }
      }
      yield return null;
    }

    while (isPlaying)
    {
      isPlaying = false;
      foreach (Animator a in animators)
      {
        if (a.GetCurrentAnimatorStateInfo(0).IsName("Fade"))
        {
          isPlaying = true;
        }
      }
      yield return null;
    }

    // Start the MainScene
    SceneManager.LoadScene("MainScene");
  }

}
