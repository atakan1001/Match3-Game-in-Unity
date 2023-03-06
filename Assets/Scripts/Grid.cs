using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class Grid : MonoBehaviour
{
    public enum PieceType
    {
        EMPTY,
        NORMAL,
        COUNT,
    };

    [System.Serializable]
    public struct PiecePrefab
    {
        public PieceType type;
        public GameObject prefab;
    };





    public int xDim;
    public int yDim;
    public float fillTime;

    public PiecePrefab[] piecePrefabs;
    public GameObject backgroundPrefab;
    public Level level;

    private Dictionary<PieceType, GameObject> piecePrefabDict;
    private GamePiece[,] pieces;

    private GamePiece pressedPiece;
    private GamePiece enteredPiece;

    public bool gameOver = false;

    private bool isFilling = false;

    public bool IsFilling
    {
        get { return isFilling; }
    }
    // Start is called before the first frame update
    void Awake()
    {
        piecePrefabDict = new Dictionary<PieceType, GameObject>();
        for(int i = 0; i < piecePrefabs.Length; i++)
        {
            if (!piecePrefabDict.ContainsKey(piecePrefabs[i].type))
            {
                piecePrefabDict.Add(piecePrefabs[i].type, piecePrefabs[i].prefab);
            }
        }

        for (int x = 0; x < xDim; x++)
        {
            for(int y = 0; y < yDim; y++)
            {
                GameObject background = (GameObject)Instantiate(backgroundPrefab, GetWorldPosition(x, y), Quaternion.identity);
                background.transform.parent = transform;
            }
        }

        pieces = new GamePiece[xDim, yDim];

       
        for (int x = 0; x < xDim; x++)
        {
            for (int y = 0; y < yDim; y++)
            {
                if (pieces[x, y] == null)
                {
                    SpawnNewPiece(x, y, PieceType.EMPTY);

                }
            }
        }
        StartCoroutine(Fill());

        
    }

    // Update is called once per frame
    void Update()
    {
        
    }

    public IEnumerator Fill()
    {
        bool needsRefill = true;
        isFilling = true;
       // int _y = yDim - 1;
        while (needsRefill)
        {
            yield return new WaitForSeconds(fillTime);
            while (FillStep())
            {
              
                yield return new WaitForSeconds(fillTime);
               // _y--;
            }
            needsRefill = ClearAllValidMatches();
        }

        isFilling = false;
    }

    public bool FillStep()
    {
     
        
        
        bool movedPiece = false;
      
        
        // We're looking for pieces we can move down and the bottom row can not be moved down
         for(int y = yDim - 2; y >= 0; y--)
             // Going from bottom to top (ignoring the bottom row)
         {
             for(int x = 0; x < xDim; x++)
             {
                 GamePiece piece = pieces[x, y];

                 if (piece.IsMovable())
                 {
                     GamePiece pieceBelow = pieces[x, y + 1];

                     if(pieceBelow.Type == PieceType.EMPTY)
                     {
                         Destroy(pieceBelow.gameObject);
                         piece.MovableComponent.Move(x, y + 1, fillTime);
                         pieces[x, y + 1] = piece;
                         SpawnNewPiece(x, y, PieceType.EMPTY);
                         movedPiece = true;
                     }
                 }
             }
         }
        
        for (int x = 0; x < xDim; x++)
        {

            GamePiece pieceBelow = pieces[x, 0];

            if(pieceBelow.Type == PieceType.EMPTY)
            {
                Destroy(pieceBelow.gameObject);
                GameObject newPiece = (GameObject)Instantiate(piecePrefabDict[PieceType.NORMAL], GetWorldPosition(x, -1), Quaternion.identity);
                newPiece.transform.parent = transform;

                /*
                if(x == 0 && _y == 0) {
                    pieces[x, 0] = newPiece.GetComponent<GamePiece>();
                    pieces[x, 0].Init(x, -1, this, PieceType.NORMAL);
                    pieces[x, 0].MovableComponent.Move(x, 0, fillTime);
                    pieces[x, 0].ColorComponent.SetColor((ColorPiece.ColorType)Random.Range(0, 1));
                    movedPiece = true;
                }
                
                 

                
                else
                {
                   
                }*/

                pieces[x, 0] = newPiece.GetComponent<GamePiece>();
                pieces[x, 0].Init(x, -1, this, PieceType.NORMAL);
                pieces[x, 0].MovableComponent.Move(x, 0, fillTime);
                pieces[x, 0].ColorComponent.SetColor((ColorPiece.ColorType)Random.Range(0, 4));
                movedPiece = true;

            }
        }
       
        
        return movedPiece; 
    }
    public Vector2 GetWorldPosition(int x, int y)
    {
        return new Vector2(transform.position.x - xDim / 2.0f + x,
            transform.position.y + yDim / 2.0f - y);
    }

    public GamePiece SpawnNewPiece(int x, int y, PieceType type)
    {
        GameObject newPiece = (GameObject)Instantiate(piecePrefabDict[type], GetWorldPosition(x, y), Quaternion.identity);
        newPiece.transform.parent = transform;

        pieces[x, y] = newPiece.GetComponent<GamePiece>();
        pieces[x, y].Init(x, y, this, type);

        return pieces[x, y];
    }

    public bool IsAdjacent(GamePiece piece1, GamePiece piece2)
    {
        return (piece1.X == piece2.X && (int)Mathf.Abs(piece1.Y - piece2.Y) == 1)
            || (piece1.Y == piece2.Y && (int)Mathf.Abs(piece1.X - piece2.X) == 1);
    }

    public void SwapPieces(GamePiece piece1, GamePiece piece2)
    {
        if ( gameOver)
        {
            return; 
        }

        if(piece1.IsMovable() && piece2.IsMovable())
        {
            pieces[piece1.X, piece1.Y] = piece2;
            pieces[piece2.X, piece2.Y] = piece1;

            if(GetMatch(piece1, piece2.X, piece2.Y) != null || GetMatch(piece2, piece1.X, piece1.Y) != null)
            {
                int piece1X = piece1.X;
                int piece1Y = piece1.Y;

                piece1.MovableComponent.Move(piece2.X, piece2.Y, fillTime);
                piece2.MovableComponent.Move(piece1X, piece1Y, fillTime);

                ClearAllValidMatches();
                pressedPiece = null;
                enteredPiece = null;
                StartCoroutine(Fill());
                level.OnMove();

            }
            else
            {
                pieces[piece1.X, piece1.Y] = piece1;
                pieces[piece2.X, piece2.Y] = piece2;
            }
           

        }
    }

    public void PressPiece(GamePiece piece)
    {
        pressedPiece = piece;
    }

    public void EnterPiece(GamePiece piece)
    {
        enteredPiece = piece;
    }

    public void ReleasePiece()
    {
        if(IsAdjacent(pressedPiece, enteredPiece))
        {
            SwapPieces(pressedPiece, enteredPiece);
        }
    }

    public List<GamePiece> GetMatch(GamePiece piece, int newX, int newY)
    {
        if (piece.IsColored())
        {
            ColorPiece.ColorType color = piece.ColorComponent.Color;
            List<GamePiece> horizontalPieces = new List<GamePiece>();
            List<GamePiece> verticalPieces = new List<GamePiece>();
            List<GamePiece> matchingPieces = new List<GamePiece>();

            // Checking horizontally
            horizontalPieces.Add(piece);

            for(int dir = 0; dir <= 1; dir++)
            {
                for(int xOffset = 1; xOffset < xDim; xOffset++)
                {
                    int x;

                    if(dir == 0)
                    { //Left
                        x = newX - xOffset;
                    }
                    else
                    { //Right
                        x = newX + xOffset;
                    }

                    if(x < 0 || x >= xDim)
                    {
                        break;
                    }

                    if (pieces[x, newY].IsColored() && pieces[x, newY].ColorComponent.Color == color)
                    {
                        horizontalPieces.Add(pieces[x, newY]);
                    }
                    else
                    {
                        break;
                    }
                }
            }
            if(horizontalPieces.Count >= 3)
            {
                for(int i = 0; i < horizontalPieces.Count; i++)
                {
                    matchingPieces.Add(horizontalPieces[i]);
                }
            }

            if(matchingPieces.Count >= 3)
            {
                return matchingPieces;
            }



            //Checking vertically
            verticalPieces.Add(piece);
            for (int dir = 0; dir <= 1; dir++)
            {
                for (int yOffset = 1; yOffset < yDim; yOffset++)
                {
                    int y;

                    if (dir == 0)
                    { //Up
                        y = newY - yOffset;
                    }
                    else
                    { //Down
                        y = newY + yOffset;
                    }

                    if (y < 0 || y >= yDim)
                    {
                        break;
                    }

                    if (pieces[newX, y].IsColored() && pieces[newX, y].ColorComponent.Color == color)
                    {
                        verticalPieces.Add(pieces[newX, y]);
                    }
                    else
                    {
                        break;
                    }
                }
            }
            if (verticalPieces.Count >= 3)
            {
                for (int i = 0; i < verticalPieces.Count; i++)
                {
                    matchingPieces.Add(verticalPieces[i]);
                }
            }

            if (matchingPieces.Count >= 3)
            {
                return matchingPieces;
            }


        }

        return null;   

    }

    public bool ClearAllValidMatches()
    {
        bool needsRefill = false;

        for(int y = 0; y < yDim; y++)
        {
            for(int x = 0; x < xDim; x++)
            {
                if (pieces[x, y].IsClearable())
                {
                    List<GamePiece> match = GetMatch(pieces[x, y], x, y);
                    if(match != null)
                    {
                        for(int i = 0; i < match.Count; i++)
                        {
                            if (ClearPiece(match[i].X, match[i].Y))
                            {
                                needsRefill = true;
                            }
                        }
                    }
                }
            }
        }
        return needsRefill;
    }

    public bool ClearPiece(int x, int y)
    {
        if (pieces[x, y].IsClearable() && !pieces[x, y].ClearableComponent.IsCleared)
        {
            pieces[x, y].ClearableComponent.Clear();
            SpawnNewPiece(x, y, PieceType.EMPTY);

            return true;
        }
        return false;
    }

    public void GameOver()
    {
        gameOver = true;
    }
}
