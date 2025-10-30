namespace Chess
{
    public class ChessProblem
    {
        private Board board;
        public ChessStatus ChessStatus;

        public ChessProblem(Board newBoard)
        {
            board = newBoard;
        }

        // Определяет мат, шах или пат белым.
        public ChessStatus CalculateChessStatus()
        {
            var isCheck = IsCheckForWhite();
            var whiteKingHasMoves = false;
            foreach (var locFrom in board.GetPieces(PieceColor.White))
            {
                foreach (var locTo in board.GetPiece(locFrom).GetMoves(locFrom, board))
                {
                    var temporaryMove = board.PerformTemporaryMove(locFrom,  locTo);
                    
                    if (!IsCheckForWhite())
                        whiteKingHasMoves = true;
                    
                    temporaryMove.Undo();
                }
            }

            if (isCheck && whiteKingHasMoves)
            {
                ChessStatus = ChessStatus.Check;
            }
            else if (isCheck && !whiteKingHasMoves)
            {
                ChessStatus = ChessStatus.Mate;
            }
            else if (!isCheck && !whiteKingHasMoves)
            {
                ChessStatus = ChessStatus.Stalemate;
            }
            else ChessStatus = ChessStatus.Ok;
            return ChessStatus;
        }

        // check — это шах
        private bool IsCheckForWhite()
        {
            var isCheck = false;
            foreach (var loc in board.GetPieces(PieceColor.Black))
            {
                var piece = board.GetPiece(loc);
                var moves = piece.GetMoves(loc, board);
                foreach (var destination in moves)
                {
                    if (Piece.Is(board.GetPiece(destination), PieceColor.White, PieceType.King))
                    {
                        isCheck = true;   
                    }
                }
            }
            return isCheck;
        }
    }
}