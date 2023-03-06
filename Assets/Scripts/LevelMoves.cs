using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class LevelMoves : Level
{

    public int numMoves;
    public int targetScore;
    
    private int movesUsed = 0;
    // Start is called before the first frame update
    void Start()
    {

        hud.SetLevelType();
        hud.SetScore(currentScore);
        hud.SetTarget(targetScore);
        hud.SetRemaining(numMoves);
    }

    // Update is called once per frame
    void Update()
    {
        
    }

    public override void OnMove()
    {
        movesUsed++;

        hud.SetRemaining(numMoves - movesUsed);

        if(numMoves == movesUsed)
        {
            if(currentScore >= targetScore)
            {
                GameWin();
            }
            else
            {
                GameLose();
            }
        }
       
    }
}
