using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using ChessEngine;
using ChessEngine.Pieces;
using ChessEngine.Moves;

namespace ChessEngine
{
    public class Board
    {
        Piece[] board;
        public Coordinate enPassantSquare { get; private set; }
        public Board()  
        {
            board = new Piece[64];

        }
        ///// <summary>
        ///// One-dimensional 64 sized array
        ///// </summary>
        ///// <param name="index">square index on board which is 1&lt;=index&lt;=64</param>
        ///// <returns></returns>
        //public Piece this[int index]
        //{
        //    get
        //    {
        //        int file = index % 9;
        //        int rank = index / 9;
        //        return board[rank * 16 + file];
        //    }
        //    set
        //    {
        //        int file = index % 9 + 1;
        //        int rank = index / 9 + 1;
        //        board[rank * 16 + file] = value;

        //    }
        //}
        /// <summary>
        /// returns piece on the cordinate 
        /// </summary>
        /// <param name="file">none-zero based</param>
        /// <param name="rank">none-zero based</param>
        /// <returns></returns>
        public Piece this[int rank, int file]
        {
            get
            {

                return board[(rank - 1) * 7 + file];
            }
            set
            {
                board[(rank - 1) * 7 + file] = value;
            }
        }
        public Piece this[Coordinate square]
        {
            get { return board[(square.rank - 1) * 15 + square.file]; }
            set { board[(square.rank - 1) - 15 + square.file] = value; }
        }

    }
}
