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
    public Button enterButton;

    public float alphaDifficulty = 0.6f;

    public Image diffSelected;
    public Image diffUnselected;

    public Animator startPanelAnimator;

    public Animator[] animators;


    private bool isPlayButton = false;

    // Set the listener of the mode buttons
    void Start()
    {
        easyButton.onClick.AddListener(() =>
        {
            BoardManagerSystem.difficulty = easy;
            unselectAllButtons();
            selectButton(easyButton.image);
        });

        mediumButton.onClick.AddListener(() =>
        {
            BoardManagerSystem.difficulty = medium;
            unselectAllButtons();
            selectButton(mediumButton.image);
        });

        hardButton.onClick.AddListener(() =>
        {
            BoardManagerSystem.difficulty = hard;
            unselectAllButtons();
            selectButton(hardButton.image);
        });


        enterButton.onClick.AddListener(() =>
        {
            if (!isPlayButton)
            {
                startPanelAnimator.SetTrigger("enterIsPressed");
                isPlayButton = true;
            }
            else
            {
                StartCoroutine(LoadNewScene());
            }
        });

        easyButton.onClick.Invoke();
    }

    // Start the fade animation of the texts
    public void startAnimation(Button button)
    {
        foreach (Animator a in animators)
        {
            if (a.gameObject != button.gameObject)
            {
                a.Play("Fade");
            }

        }
    }

    // Unselect all button
    public void unselectAllButtons()
    {
        var temp = easyButton.image.color;
        temp.a = 0;
        easyButton.image.color = temp;
        mediumButton.image.color = temp;
        hardButton.image.color = temp;
    }

    // Disable all the buttons
    public void disableAllButtons()
    {
        easyButton.enabled = false;
        mediumButton.enabled = false;
        hardButton.enabled = false;
    }

    public void selectButton(Image button)
    {
        var temp = button.color;
        temp.a = alphaDifficulty;
        button.color = temp;
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

    // The coroutine runs on its own at the same time as Update() and takes an integer indicating which scene to load.
    IEnumerator LoadNewScene()
    {

        // Start an asynchronous operation to load the scene that was passed to the LoadNewScene coroutine.
        AsyncOperation async = SceneManager.LoadSceneAsync("MainScene");

        // While the asynchronous operation to load the new scene is not yet complete, continue waiting until it's done.
        while (!async.isDone)
        {
            yield return null;
        }

    }

}
