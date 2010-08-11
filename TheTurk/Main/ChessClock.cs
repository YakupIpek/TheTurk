using System;
using System.Diagnostics;

namespace ChessEngine.Main
{
    public class ChessClock
    {
        public long timeForPerMove { get; private set; }
        public ChessClock(int moveNumber, int minute, int incrementPerMove)
        {
            if (moveNumber == 0) moveNumber = 40;
            timeForPerMove = MinToMilliSecond(minute) / moveNumber + SecToMilliSecond(incrementPerMove);
        }
        public ChessClock(string time)
        {
            var splitedTime = time.Split(' ');
            int moveNumber = int.Parse(splitedTime[0]);
            long baseSec = MinToMilliSecond(int.Parse(splitedTime[1]));
            long incSec = SecToMilliSecond(int.Parse(splitedTime[2]));
            if (moveNumber == 0) moveNumber = 40;
            timeForPerMove = baseSec / moveNumber + incSec;
        }

        public static long MinToMilliSecond(int minute)
        {
            return minute * 60 * 1000;
        }
        public static long SecToMilliSecond(int sec)
        {
            return sec * 1000;
        }
    }
}
