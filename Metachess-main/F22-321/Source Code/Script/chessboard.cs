using ovrAvatar.Arbiter;
using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class chessboard : MonoBehaviour

{
    [Header("Art stuff")]
    [SerializeField] private Material tileMaterial;
    
    [SerializeField] private float tileSize=1.0f;
    [SerializeField] private float yOffset=0.2f;
    [SerializeField] private Vector3 boardCenter = Vector3.zero;
    [Header("prefabs & Materials")]
    [SerializeField] private GameObject[] prefabs;
    [SerializeField] private Material[] teamMaterials;
    [SerializeField] private float deathSize = 0.5f;
    [SerializeField] private float deathSpacing = 3f;
    [SerializeField] private float dragOffset = 1f;
    //LOGIC
    private Chesspiece[,] chessPieces;
    private Chesspiece currentlyDragging;
    private List<Vector2Int> AvailableMoves = new List<Vector2Int>();
    private List<Chesspiece> deadWhites = new List<Chesspiece>();
    private List<Chesspiece> deadBlacks = new List<Chesspiece>();
    private const int TILE_COUNT_X = 8;
    private const int TILE_COUNT_Y = 8;
    private GameObject[,] tiles;
    private Camera currentCamera;
    private Vector2Int currentHover;
    private Vector3 bounds;

    [SerializeField] private Color _baseColor, _offsetColor;
    [SerializeField] private SpriteRenderer _renderer;
    [SerializeField] private GameObject _highlight;
    private void Awake()
    {
        GenrateAllTiles(tileSize, TILE_COUNT_X, TILE_COUNT_Y);
        
        spawnAllPieces();
        PositionAllPiece();
      
    }

    private void Update()
    {
        if (!currentCamera)
        {
            currentCamera = Camera.main;
            return;
        }

        RaycastHit info;
        Ray ray = currentCamera.ScreenPointToRay(Input.mousePosition);
        if (Physics.Raycast(ray, out info, 100, LayerMask.GetMask("Tile","Hover","Highlight")))
        {
            // Get the indexes of the tile i've hit
            Vector2Int hitPosition = LookupTileIndex(info.transform.gameObject);

            // If we're hovering a tile after not hovering any tiles
            if (currentHover == -Vector2Int.one)
            {
                currentHover = hitPosition;
                tiles[hitPosition.x, hitPosition.y].layer = LayerMask.NameToLayer("Hover");

            }

            // If we were already hovering a tile, change the previous one
            if (currentHover != hitPosition)
            {
                tiles[currentHover.x, currentHover.y].layer = LayerMask.NameToLayer("Tile");
                //(ContainsValidMoves(ref AvailableMoves, currentHover)) ? LayerMask.NameToLayer("Highlight") : LayerMask.NameToLayer("Tile");
                //LayerMask.NameToLayer("Tile");
                //(ContainsValidMoves(ref AvailableMoves, currentHover)) ? LayerMask.NameToLayer("Highlight") : LayerMask.NameToLayer("Tile");

                currentHover = hitPosition;
                tiles[hitPosition.x, hitPosition.y].layer = LayerMask.NameToLayer("Hover");

            }


            if (Input.GetMouseButtonDown(0))
            {
                if (chessPieces[hitPosition.x, hitPosition.y] != null)
                {
                    //Is it our turn?
                    if (true)
                    {
                        currentlyDragging = chessPieces[hitPosition.x, hitPosition.y];
                        //Get the list of where i can go, highlight tiles
                        AvailableMoves = currentlyDragging.GetAvailableMoves(ref chessPieces, TILE_COUNT_X, TILE_COUNT_Y);
                        highlighttiles();
                    }
                }
            }
            //if we are releasing the mouse button
            if (currentlyDragging != null && Input.GetMouseButtonUp(0))
            {
                Vector2Int previousPosition = new Vector2Int(currentlyDragging.currentX, currentlyDragging.currentY);
               
                bool validMove = MoveTo(currentlyDragging, hitPosition.x, hitPosition.y);
                if (!validMove)
                {
                    currentlyDragging.SetPosition(GetTileCenter(previousPosition.x, previousPosition.y));
                    //.transform.position = GetTileCenter(previousPosition.x, previousPosition.y);
                    //SetPosition(GetTileCenter(previousPosition.x, previousPosition.y));
                    currentlyDragging = null;
                   // RemoveHighlightTiles();
                }
                else
                {
                    currentlyDragging = null;   
                }
                
            }
        }
        else
        {
            if (currentHover != -Vector2Int.one)
            {
                tiles[currentHover.x, currentHover.y].layer = LayerMask.NameToLayer("Tile");
                //(ContainsValidMoves(ref AvailableMoves, currentHover)) ? LayerMask.NameToLayer("Highlight") : LayerMask.NameToLayer("Tile");
                    //LayerMask.NameToLayer("Tile");
                //(ContainsValidMoves(ref AvailableMoves,currentHover))? LayerMask.NameToLayer("Highlight") : LayerMask.NameToLayer("Tile");

                currentHover = -Vector2Int.one;
            }
            if (currentlyDragging && Input.GetMouseButtonDown(0))
            {
                currentlyDragging.SetPosition(GetTileCenter(currentlyDragging.currentX, currentlyDragging.currentY));
                // SetPosition(GetTileCenter(currentlyDragging.currentX, currentlyDragging.currentY));
                currentlyDragging = null;
               // RemoveHighlightTiles();
            }
        }
        if (currentlyDragging)
        {
            Plane horizontalPlane = new Plane(Vector3.up, Vector3.up * yOffset);
            float distance = 0.0f;
            if(horizontalPlane.Raycast(ray, out distance))
            {
               currentlyDragging.SetPosition( ray.GetPoint(distance)+Vector3.up*dragOffset);
            }
        }


    }
    
    //Generate the board
    private void GenrateAllTiles(float tileSizze, int tilecountx,int tilecounty)
    {
        yOffset += transform.position.y;
        bounds = new Vector3((tilecountx / 2) * tileSize, 0,(tilecountx / 2) * tileSize) + boardCenter;
        tiles = new GameObject[tilecountx, tilecounty];
        for (int x = 0; x < tilecountx; x++)
            for (int y = 0; y < tilecounty; y++)
                tiles[x,y] = GenerateSingleTile(tileSizze, x, y);


    }

    private GameObject GenerateSingleTile(float  tileSize,int x,int y)
    {

        GameObject tileObject = new GameObject(string.Format("X:{0},Y{1}", x, y));

        tileObject.transform.parent = transform;


        Mesh mesh = new Mesh(); 
        tileObject.AddComponent<MeshFilter>().mesh = mesh;
        tileObject.AddComponent<MeshRenderer>().material=tileMaterial;
        



        Vector3[] vertices = new Vector3[4];
        vertices[0] = new Vector3(x*tileSize, yOffset, y*tileSize)-bounds;
        vertices[1] = new Vector3(x * tileSize, yOffset, (y+1) * tileSize)-bounds;
        vertices[2] = new Vector3((x + 1) * tileSize, yOffset, (y) * tileSize) - bounds;
        vertices[3] = new Vector3((x+1) * tileSize, yOffset, (y+1) * tileSize) - bounds;

        int[] tris = new int[] {0,1,2,1,3,2};
        mesh.vertices = vertices;
        mesh.triangles = tris;
        mesh.RecalculateNormals();


        tileObject.layer = LayerMask.NameToLayer("Tile");
        //tileObject.layer = LayerMask.NameToLayer("Hover");
        tileObject.AddComponent<BoxCollider>();


        return tileObject;
    
    }
    
    //Spaming of pieces
    private void spawnAllPieces()
    {

        chessPieces = new Chesspiece[TILE_COUNT_X,TILE_COUNT_Y];
        int whiteTeam = 0;
        int blackTeam = 1;

        //whiteteam
        chessPieces[0, 0] = SpawnSinglePiece(ChessPieceType.Rook, whiteTeam);
        chessPieces[1, 0] = SpawnSinglePiece(ChessPieceType.Knight, whiteTeam);
        chessPieces[2, 0] = SpawnSinglePiece(ChessPieceType.Bishop, whiteTeam);
        chessPieces[3, 0] = SpawnSinglePiece(ChessPieceType.Queen, whiteTeam);
        chessPieces[4, 0] = SpawnSinglePiece(ChessPieceType.King, whiteTeam);
        chessPieces[5, 0] = SpawnSinglePiece(ChessPieceType.Bishop, whiteTeam);
        chessPieces[6, 0] = SpawnSinglePiece(ChessPieceType.Knight, whiteTeam);
        chessPieces[7, 0] = SpawnSinglePiece(ChessPieceType.Rook, whiteTeam);
        // chessPieces[0, 1] = SpawnSinglePiece(ChessPieceType.Pawn, whiteTeam);
        for (int i = 0; i < TILE_COUNT_X; i++)
            chessPieces[i, 1] = SpawnSinglePiece(ChessPieceType.Pawn, whiteTeam);

        //Blackteam
        chessPieces[0, 7] = SpawnSinglePiece(ChessPieceType.Rook, blackTeam);
        chessPieces[1, 7] = SpawnSinglePiece(ChessPieceType.Knight, blackTeam);
        chessPieces[2, 7] = SpawnSinglePiece(ChessPieceType.Bishop, blackTeam);
        chessPieces[3, 7] = SpawnSinglePiece(ChessPieceType.Queen, blackTeam);
        chessPieces[4, 7] = SpawnSinglePiece(ChessPieceType.King, blackTeam);
        chessPieces[5, 7] = SpawnSinglePiece(ChessPieceType.Bishop, blackTeam);
        chessPieces[6, 7] = SpawnSinglePiece(ChessPieceType.Knight, blackTeam);
        chessPieces[7, 7] = SpawnSinglePiece(ChessPieceType.Rook, blackTeam);
        for (int i = 0; i < TILE_COUNT_X; i++)
            chessPieces[i, 6] = SpawnSinglePiece(ChessPieceType.Pawn, blackTeam);





    }
    private Chesspiece SpawnSinglePiece(ChessPieceType type, int team)
    {
       
        Chesspiece cp = Instantiate(prefabs[(int)type - 1], transform).GetComponent<Chesspiece>();
        cp.type = type;
        cp.team = team;
        
        cp.GetComponent<MeshRenderer>().material = teamMaterials[team];
        return cp;
    }

    //positioning
    private void PositionAllPiece()
    {
        for (int x = 0; x < TILE_COUNT_X; x++)
        {
            for(int y = 0; y < TILE_COUNT_Y; y++)
            {
                if (chessPieces[x, y] != null)
                    PositionSinglePieces(x, y,true);
            }
        }
    }

    private void PositionSinglePieces(int x, int y, bool force = false)
    {
        chessPieces[x, y].currentX = x;
        chessPieces[x, y].currentY=y;
        chessPieces[x, y].SetPosition(GetTileCenter(x, y),force);
       // transform.position = GetTileCenter(x, y);
            //SetPosition(GetTileCenter(x,y));
    }

    private Vector3 GetTileCenter(int x, int y)
    {
        /* if (x == 0 || y == 1)
         {
              return new Vector3(x*tileSize,yOffset,y*tileSize) - bounds + new Vector3(tileSize/2,1,tileSize/2);

         }*/
        return new Vector3(x * tileSize, yOffset, y * tileSize) - bounds + new Vector3(tileSize / 2, 0, tileSize / 2);
        //new Vector3(x*tileSize,yOffset,y*tileSize) - bounds + new Vector3(tileSize/2,0,tileSize/2);
    }

    //Hightlight tiles
    private void highlighttiles()
    {
        for (int i =0; i<AvailableMoves.Count;i++)
        {
            tiles[AvailableMoves[i].x, AvailableMoves[i].y].layer = LayerMask.NameToLayer("Highlight");
        }
    }


    private void RemoveHighlightTiles()
    {
        for (int i = 0; i < AvailableMoves.Count; i++)
        {
            tiles[AvailableMoves[i].x, AvailableMoves[i].y].layer = LayerMask.NameToLayer("Tile");

            AvailableMoves.Clear();
        }
    }
    //operation
    private bool ContainsValidMoves(ref List<Vector2Int> moves, Vector2 pos)
    {
        for(int i = 0; i < moves.Count; i++)
        {
            if (moves[i].x == pos.x && moves[i].y==pos.y)
                return true;   
            
             
        }
        return false;
    }
    private bool MoveTo(Chesspiece cp, int x, int y)

    {
        if(!ContainsValidMoves(ref AvailableMoves, new Vector2(x,y))) 
           return false;

        Vector2Int previousPosition = new Vector2Int(cp.currentX,cp.currentY);

        if (chessPieces[x, y] != null)
        {
            Chesspiece ocp = chessPieces[x, y]; 

            if(cp.team==ocp.team)
            {
                return false;
            }
            //if its enemy team
            if(ocp.team==0)
            {
               /* if(cp.type==ChessPieceType.Pawn&& ocp.type==ChessPieceType.Pawn)
                {
                    deadWhites.Add(ocp);
                    ocp.SetScale(Vector3.one * deathSize);
                    ocp.SetPosition(new Vector3(8 * tileSize, yOffset, -1 * tileSize) - bounds + new Vector3(tileSize / (51 / 2), 1, tileSize / (51 / 2)) + (Vector3.forward * deathSpacing) * deadWhites.Count);
                }*/
                deadWhites.Add(ocp);
               ocp.SetScale(Vector3.one * deathSize);
                ocp.SetPosition(new Vector3 (8*tileSize,yOffset,-1*tileSize)-bounds + new Vector3(tileSize/ (51 / 2), 0,tileSize/ (51 / 2)) +(Vector3.forward*deathSpacing)* deadWhites.Count);

            }
            else
            {
                deadBlacks.Add(ocp);
                ocp.SetScale(Vector3.one * deathSize);
                ocp.SetPosition(new Vector3(-1 * tileSize, yOffset, 8 * tileSize) - bounds + new Vector3(tileSize /(19/10), 0, tileSize / (19/10)) + (Vector3.back * deathSpacing) * deadBlacks.Count);
            }
        }

        chessPieces[x, y] = cp;
        chessPieces[previousPosition.x, previousPosition.y] = null;
        PositionSinglePieces(x, y); return true;
    }
    private Vector2Int LookupTileIndex(GameObject hitinfo)
    {
        for(int x = 0;x < TILE_COUNT_X; x++) 
            for(int y = 0;y < TILE_COUNT_Y;y++) 
                if (tiles[x,y] == hitinfo) 
                    return new Vector2Int(x,y);
        return -Vector2Int.one;//invalid
    
    
    }
    
 

}
