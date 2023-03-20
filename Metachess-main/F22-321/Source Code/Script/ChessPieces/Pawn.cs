using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class Pawn : Chesspiece
{
    public override List<Vector2Int> GetAvailableMoves(ref Chesspiece[,] board, int tileCountX, int tileCountY)
    {
        List<Vector2Int> r = new List<Vector2Int>();

        int direction = (team == 0) ? 1 : -1;
        //one in front
        if (board[currentX, currentY + direction] == null)
        {
            r.Add(new Vector2Int(currentX, currentY + direction));

        }
        // two in front
        if (board[currentX, currentY + direction] == null)
        {//white team
            if (team == 0 && currentY == 1 && board[currentX, currentY + (direction * 2)] == null)
                r.Add(new Vector2Int(currentX, currentY + (direction * 2)));
            //black team
            if (team == 1 && currentY == 6 && board[currentX, currentY + (direction * 2)] == null)
                r.Add(new Vector2Int(currentX, currentY + (direction * 2)));
        }
        //right side
        if(currentX!=tileCountX-1)
            if (board[currentX+1, currentY+direction] != null && board[currentX + 1, currentY + direction].team !=team)
                r.Add(new Vector2Int(currentX+1, currentY + direction ));
        //left side
        if (currentX != 0)
            if (board[currentX - 1, currentY + direction] != null && board[currentX - 1, currentY + direction].team != team)
                r.Add(new Vector2Int(currentX - 1, currentY + direction));
        return r;
    }
}
