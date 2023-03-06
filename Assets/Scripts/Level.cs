using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class Level : MonoBehaviour
{

    public Grid grid;

    public Hud hud;

    public int score1Star;
    public int score2Star;
    public int score3Star;

    protected int currentScore;

    protected bool didWin;
    // Start is called before the first frame update
    void Start()
    {
        hud.SetScore(currentScore);
    }

    // Update is called once per frame
    void Update()
    {

    }

    public virtual void GameWin()
    {
        grid.GameOver();
        didWin = true;
        StartCoroutine(WaitForGridFill());
    }

    public virtual void GameLose()
    {
        grid.GameOver();
        didWin = false;
        StartCoroutine(WaitForGridFill());

    }

    public virtual void OnMove()
    {
        Debug.Log("You Moved!");

    }

    public virtual void OnPieceCleared(GamePiece piece)
    {
        if (piece.ColorComponent.Color == ColorPiece.ColorType.RED)
        {
            currentScore += 100;
        }
        else if (piece.ColorComponent.Color == ColorPiece.ColorType.GREEN)
        {
            currentScore += 150;
        }
        else if (piece.ColorComponent.Color == ColorPiece.ColorType.BLUE)
        {
            currentScore += 200;
        }
        else if (piece.ColorComponent.Color == ColorPiece.ColorType.YELLOW)
        {
            currentScore += 250;
        }

        hud.SetScore(currentScore);

    }

    protected virtual IEnumerator WaitForGridFill()
    {
        while (grid.IsFilling)
        {
            yield return 0;
        }

        if (didWin)
        {
            hud.OnGameWin(currentScore);
        }
        else
        {
            hud.OnGameLose();
        }
    }
}
