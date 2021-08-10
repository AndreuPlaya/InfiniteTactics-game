using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;

public class GameUI : MonoBehaviour
{
    public Image fadePlane;
    public GameObject gameOverUI;
    private void Start()
    {
        StartCoroutine(LateStart(2));
    }
    IEnumerator LateStart(float waitTime)
    {
        yield return new WaitForSeconds(waitTime);
        GameObject.FindGameObjectWithTag("Player").GetComponent<Player>().OnDeath += OnGameOver;
    }
    void OnGameOver()
    {
        Debug.Log("Rolling game over screen");
        StartCoroutine(Fade(Color.clear, Color.black, 1));
        gameOverUI.SetActive(true);
    }

    IEnumerator Fade(Color from, Color to, float time)
    {
        float speed = 1 / time;
        float percent = 0;

        while (percent < 1)
        {
            percent += Time.deltaTime * speed;
            fadePlane.color = Color.Lerp(from, to, percent);
            yield return null;
        }
    }

    //UI Imput

    public void StartNewGame()
    {
        Application.LoadLevel("Game");
    }
}
