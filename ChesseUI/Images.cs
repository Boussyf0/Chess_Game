﻿using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows.Media;
using System.Windows.Media.Imaging;
using Chesselogique;

namespace ChesseUI
{
    public static class Images
    {
        private static readonly Dictionary<PieceType, ImageSource> whitesources = new()
        {
            {PieceType.Pawn, LoadImage("Assets/PawnW.png") },
            {PieceType.Bishop, LoadImage("Assets/BishopW.png") },
            {PieceType.Knight, LoadImage("Assets/KnightW.png") },
            {PieceType.Rook, LoadImage("Assets/RookW.png") },
            {PieceType.Queen, LoadImage("Assets/QueenW.png") },
            {PieceType.King, LoadImage("Assets/KingW.png") },
        };

        private static readonly Dictionary<PieceType, ImageSource> blacksources = new()
        {
            {PieceType.Pawn, LoadImage("Assets/PawnB.png") },
            {PieceType.Bishop, LoadImage("Assets/BishopB.png") },
            {PieceType.Knight, LoadImage("Assets/KnightB.png") },
            {PieceType.Rook, LoadImage("Assets/RookB.png") },
            {PieceType.Queen, LoadImage("Assets/QueenB.png") },
            {PieceType.King, LoadImage("Assets/KingB.png") },
        };

        private static ImageSource LoadImage(string filepath)
        {
            return new BitmapImage(new Uri(filepath, UriKind.Relative));
        }

        public static ImageSource GetImage(Player color, PieceType type)
        {
            return color switch
            {
                Player.White => whitesources[type],
                Player.Black => blacksources[type],
                _ => null
            };

        }

        public static ImageSource GetImage(Piece piece)
        {
            if (piece == null)
            {
                return null;    
            }
            return GetImage(piece.color, piece.Type);
        }
    }
}
