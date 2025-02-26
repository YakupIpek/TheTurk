﻿using System;
using System.Collections.Generic;
using System.Diagnostics;
using TheTurk.Moves;
using System.Linq;
using TheTurk.Pieces;
using System.Threading.Tasks.Sources;
using System.Runtime.CompilerServices;
using System.Text.RegularExpressions;

namespace TheTurk.Engine
{
    public class TableEntry
    {
        public int Score { get; set; }
        public int Depth { get; set; }
        public long HashKey { get; set; }
    }

    public static class ResultExtensions
    {
        public static (int score, Move[] line) Negate(this (int score, Move[] line) result) => (-result.score, result.line);
    }
    public partial class ChessEngine
    {
        private KillerMoves killerMoves;
        private HistoryMoves historyMoves;
        private int iterationPly;
        private long timeLimit;
        private Stopwatch elapsedTime;
        int node;
        public bool ExitRequested { get; set; }
        public readonly Board Board;

        Dictionary<long, TableEntry> transpositions;
        private Move[] bestLine;

        /// <summary>
        /// 
        /// </summary>
        /// <param name="board"></param>
        /// <param name="protocol">Protocol will be used to write output of bestline</param>
        public ChessEngine(Board board)
        {
            ExitRequested = false;
            Board = board;
            elapsedTime = new Stopwatch();
            historyMoves = new HistoryMoves();
            killerMoves = new KillerMoves();
        }
        /// <param name="timeLimit">Time limit in millisecond</param>
        /// <returns></returns>
        public IEnumerable<EngineResult> Search(long timeLimit)
        {
            this.timeLimit = timeLimit;

            elapsedTime.Restart();
            ExitRequested = false;
            historyMoves = new HistoryMoves();
            killerMoves = new KillerMoves();
            bestLine = [];

            int infinity = int.MaxValue;

            int alpha = -infinity,
                beta = infinity,
                depth = 0;

            //var eval = Evaluation.Evaluate(Board);
            //alpha = eval - Pawn.Piecevalue;
            //beta = beta + Pawn.Piecevalue;

            iterationPly = 1;

            while (HaveTime() && !ExitRequested)
            {
                //transpositions = new(50000);
                node = 0;
                var (score, line) = AlphaBeta(alpha, beta, iterationPly, depth, false);

                if (score <= alpha || score >= beta)
                {
                    // Make full window search again
                    alpha = -infinity;
                    beta = infinity;

                    continue;
                }

                alpha = score - Pawn.Piecevalue / 4; //Narrow Aspiration window
                beta = score + Pawn.Piecevalue / 4;

                bestLine = line;

                yield return new EngineResult(iterationPly, score, elapsedTime.ElapsedMilliseconds, node, line.Reverse().ToArray());

                if (Math.Abs(score) >= Board.CheckMateValue || ExitRequested)
                    break;

                iterationPly++;
            }

            ExitRequested = false;
        }


        (int score, Move[] line) AlphaBeta(int alpha, int beta, int ply, int depth, bool nullMoveActive, bool isCapture = false)
        {
            node++;

            //if (transpositions.TryGetValue(Board.Zobrist.ZobristKey, out var entry) && entry.Depth >= ply)
            //{
            //    if (entry.Score <= alpha)
            //        return alpha;
            //    if (entry.Score >= beta)
            //        return beta;
            //
            //    return entry.Score;
            //}

            //if time out or exit requested after 1st iteration,so leave thinking.
            if ((!HaveTime() || ExitRequested) && iterationPly > 1)
                return (Board.Draw, []);

            if (Board.threeFoldRepetetion.IsThreeFoldRepetetion)
                return (Board.Draw, []);

            var moves = Board.GenerateMoves();

            if (!moves.Any())
                return (-Board.GetCheckMateOrStaleMateScore(ply), []);

            if (ply <= 0)
            {
                if (isCapture || true)
                    return (QuiescenceSearch(alpha, beta), []);
                else
                    return (Evaluation.Evaluate(Board), []);

            }

            if (nullMoveActive && !Board.IsInCheck && ply > 2 && isCapture)
            {
                int R = (ply > 6) ? 3 : 2; // Adaptive Null Move Reduction


                var state = Board.MakeNullMove();

                var (score, _) = AlphaBeta(-beta, -beta + 1, ply - R, depth + 1, false).Negate();

                Board.UndoNullMove(state);

                if (score >= beta)
                    return (beta, []);
            }

            var sortedMoves = SortMoves(moves, depth);

            var movesIndex = 0;

            var pv = new Move[0];

            foreach (var move in sortedMoves)
            {

                var state = Board.MakeMove(move);
                movesIndex++;

                var score = 0;
                var isCaptureMove = move is Ordinary c && c.CapturedPiece is not null;

                var importantMove = isCapture || (movesIndex < 2 && iterationPly > 1) || Board.IsInCheck;

                var line = Array.Empty<Move>();

                if (!importantMove)
                {
                    (score, line) = AlphaBeta(-beta, -alpha, ply - 2, depth + 1, false).Negate();

                    importantMove = score > alpha && score < beta;
                }

                if (importantMove)
                {
                    //(score, line) = AlphaBeta(-alpha - 1, -alpha, ply - 1, depth + 1, false).Negate();

                    //if (score > alpha && score < beta)
                    //{
                    var r = iterationPly > 2 && depth == iterationPly - 2 && (Board.IsInCheck || isCapture) ? 0 : 1;

                    (score, line) = AlphaBeta(-beta, -alpha, ply - r, depth + 1, false, isCaptureMove).Negate();
                    //}
                }

                Board.UndoMove(move, state);


                //if (ply > 2 && score >= alpha + Pawn.Piecevalue)
                //{
                //    // Diğer hamlelerle karşılaştırarak singular olup olmadığını kontrol et
                //    int secondBestScore = TestSingularMove(sortedMoves, move, ply, depth, alpha);
                //    if (score > secondBestScore + SingularMargin)
                //    {
                //        Board.MakeMove(move);
                //        (score, line) = AlphaBeta(-beta, -alpha, ply, depth + 1, true).Negate(); // Derinlik +1
                //        Board.UndoMove(move, boardState);
                //    }
                //}

                if (score >= beta)
                {
                    killerMoves.Add(move, depth);

                    return (beta, [.. line, move]); //beta cut-off
                }

                if (score > alpha)
                {
                    alpha = score;
                    historyMoves.AddMove(move);

                    pv = [.. line, move];
                }
            }

            //// Singular Extension: Hamle diğerlerinden çok iyiyse derinliği artır
            //if (bestScore >= (alpha + Pawn.Piecevalue))
            //{
                
            //    var state = Board.MakeMove(pv.Last());

            //    var (score, line) = AlphaBeta(-beta, -alpha, ply, depth + 1, false, true).Negate();

            //    Board.UndoMove(pv.Last(), state);

            //    return (score, [.. pv, .. line]);
            //}

            // Transposition Table'a ekle
            //transpositions[Board.Zobrist.ZobristKey] = new TableEntry
            //{
            //    Score = bestScore,
            //    Depth = ply,
            //    HashKey = Board.Zobrist.ZobristKey
            //};

            return (alpha, pv);
        }
        /// <summary>
        /// Look for capture variations for horizon effect
        /// </summary>
        /// <param name="alpha"></param>
        /// <param name="beta"></param>
        /// <param name="nodeCount"></param>
        /// <returns></returns>
        int QuiescenceSearch(int alpha, int beta)
        {
            node++;

            var eval = Evaluation.Evaluate(Board);

            if (eval >= beta)
                return beta;

            if (eval > alpha)
            {
                alpha = eval;
            }

            var moves = MVVLVASorting(Board.GenerateMoves());

            foreach (var capture in moves)
            {
                var state = Board.MakeMove(capture);

                var score = -QuiescenceSearch(-beta, -alpha);

                Board.UndoMove(capture, state);

                if (score >= beta) // The move is too good
                    return beta;

                if (score > alpha)// Best move so far
                    alpha = score;
            }

            return alpha;
        }
        /// <summary>
        /// Filter uncapture moves and sort captured moves
        /// </summary>
        /// <param name="moves"></param>
        /// <returns></returns>
        IEnumerable<Move> MVVLVASorting(IEnumerable<Move> moves)
        {
            return moves.OfType<Ordinary>().Where(move => move.CapturedPiece != null).
                OrderByDescending(move => move.MovePriority());
        }
        /// <summary>
        /// Sort moves best to worst
        /// </summary>
        /// 
        /// <returns></returns>
        IEnumerable<Move> SortMoves(IEnumerable<Move> moves, int depth)
        {
            var previousBestMove = bestLine.ElementAtOrDefault(depth);
            var killer = killerMoves.BestMoves[depth];
            var bestHistoryMove = Board.Side == Color.White ? historyMoves.WhiteBestMove : historyMoves.BlackBestMove;

            // List to hold sorting criteria, starting with MovePriority at the end
            var sortCriteria = moves.OrderBy(m => false);

            // Add the sorting criteria only if they are not null
            if (previousBestMove != null)
            {
                sortCriteria = moves.OrderBy(move => move.Equals(previousBestMove));
            }

            if (bestHistoryMove != null)
            {
                sortCriteria = sortCriteria.ThenByDescending(move => move.Equals(bestHistoryMove));
            }

            if (killer != null)
            {
                sortCriteria = sortCriteria.ThenByDescending(move => move.Equals(killer));
            }

            // Finally, add MovePriority sorting as the last criteria
            sortCriteria = sortCriteria.ThenByDescending(move => move.MovePriority());

            return sortCriteria;
        }


        bool HaveTime()
        {
            return timeLimit > elapsedTime.ElapsedMilliseconds;
        }
    }
}
