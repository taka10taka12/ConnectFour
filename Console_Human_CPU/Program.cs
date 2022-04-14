using System;
using System.Collections;
using System.Collections.Generic;
using System.Linq;
using System.IO;

namespace ConnectFourMiniMax
{
    class Program
    {
        static void Main(string[] args)
        {
            var board = new int[6, 7]; //「0 = 空」,「1 = 先手」,「2 = 後手」
            DisplayBoard(board);

            for (int turn = 0; turn < 42; turn++)
            {
                // Step1. 先手後手の判別
                var playerNum = 1;
                if (turn % 2 != 0) playerNum = 2;
                Console.WriteLine("Turn : {0}", turn);
                Console.WriteLine("Player{0}はコインを投下する列を入力して下さい", playerNum);

                // Step2. コインを投下する列を受け取る
                if (playerNum == 1)
                {
                    var inputColumn = int.Parse(Console.ReadLine());
                    if (board[5, inputColumn] != 0)
                    {
                        while (board[5, inputColumn] != 0)
                        {
                            Console.WriteLine("その列には投下できません。別の列を選択して下さい");
                            inputColumn = int.Parse(Console.ReadLine());
                        }
                    }

                    // Step3. 入力された列にコインを投下する
                    DropCoin(board, inputColumn, playerNum);
                }
                else // CPU(MiniMax)
                {
                    var state = new ConnectFourState(board);
                    var action = MiniMaxAction(state, 7);
                    // Step3. 入力された列にコインを投下する
                    DropCoin(board, (action / 6), playerNum);
                }

                // Step4. 現在の盤面を表示する
                DisplayBoard(board);

                // Step5. 勝敗判定をする
                if (PlayerIsWin(board, playerNum))
                {
                    Console.WriteLine("Player{0}の勝利！！", playerNum);
                    break;
                }

            }
        }

        static void DropCoin(int[,] board, int column, int playerNum)
        {
            for (int row_i = 0; row_i < 6; row_i++)
            {
                if (board[row_i, column] == 0)
                {
                    board[row_i, column] = playerNum;
                    break;
                }
            }
        }

        static void DisplayBoard(int[,] board)
        {
            for (int row_i = 6; row_i >= 0; row_i--)
            {
                var str = "";
                for (int column_i = 0; column_i < 7; column_i++)
                {
                    if (row_i == 6)
                    {
                        str += "  " + column_i.ToString() + " ";
                    }
                    else
                    {
                        str += "| " + board[row_i, column_i].ToString() + " ";
                    }
                }
                Console.WriteLine(str);
            }
        }

        static bool PlayerIsWin(int[,] board, int playerNum)
        {
            var isWin = false;
            for (int row_i = 0; row_i < 6; row_i++)
            {
                for (int column_i = 0; column_i < 4; column_i++)
                {
                    var judge = false;
                    // 右横４つの判別
                    for (int i = 0; i < 4; i++)
                    {
                        if (board[row_i, column_i + i] == playerNum)
                        {
                            judge = true;
                        }
                        else
                        {
                            judge = false;
                            break;
                        }
                    }
                    if (judge) isWin = true;

                    if (row_i < 3)
                    {
                        // 右斜め上４つの判別
                        for (int i = 0; i < 4; i++)
                        {
                            if (board[row_i + i, column_i + i] == playerNum)
                            {
                                judge = true;
                            }
                            else
                            {
                                judge = false;
                                break;
                            }
                        }
                    }
                    if (judge) isWin = true;
                }
            }

            for (int column_i = 0; column_i < 7; column_i++)
            {
                for (int row_i = 0; row_i < 3; row_i++)
                {
                    var judge = false;
                    // 真上４つの判別
                    for (int i = 0; i < 4; i++)
                    {
                        if (board[row_i + i, column_i] == playerNum)
                        {
                            judge = true;
                        }
                        else
                        {
                            judge = false;
                            break;
                        }
                    }
                    if (judge) isWin = true;

                    if (column_i < 4)
                    {
                        // 左斜め上４つの判別
                        for (int i = 0; i < 4; i++)
                        {
                            if (board[row_i + i, column_i + (3 - i)] == playerNum)
                            {
                                judge = true;
                            }
                            else
                            {
                                judge = false;
                                break;
                            }
                        }
                    }
                    if (judge) isWin = true;
                }
            }
            return isWin;
        }

        static int MiniMaxScore(ConnectFourState state, int depth)
        {
            if (state.isDone() || depth == 0)
            {
                return state.getScore();
            }

            var legal_actions = state.legalActions();
            if (legal_actions.Count() == 0)
            {
                return state.getScore();
            }

            var bestScore = -1000;
            foreach (int action in legal_actions)
            {
                state.advance(action);
                var score = -1 * MiniMaxScore(state, depth - 1);
                state.reverse(action);
                if (score > bestScore)
                {
                    bestScore = score;
                }
            }
            return bestScore;
        }

        static int MiniMaxAction(ConnectFourState state, int depth)
        {
            var bestScore = -1000;
            var bestAction = -1;

            Console.WriteLine("-----CPU Calculation Start!!-----");
            foreach (int action in state.legalActions())
            {
                state.advance(action);
                var score = -1 * MiniMaxScore(state, depth);
                Console.WriteLine("action : {0}", action);
                Console.WriteLine("score : {0}", score);
                DisplayBoard(state._board);
                state.reverse(action);
                if (bestScore < score)
                {
                    bestScore = score;
                    bestAction = action;
                }
            }
            Console.WriteLine("-----CPU Calculation Finish!!-----");
            return bestAction;
        }
    }

    class ConnectFourState
    {
        public int[,] _board { get; set; }

        private int coinCount(int[,] board)
        {
            var count = 0;
            for (int row_i = 0; row_i < 6; row_i++)
            {
                for (int column_i = 0; column_i < 7; column_i++)
                {
                    if (board[row_i, column_i] != 0) count++;
                }
            }
            return count;
        }

        private bool isConnectFour(int playerNum)
        {
            var isConnectFour = false;
            for (int row_i = 0; row_i < 6; row_i++)
            {
                for (int column_i = 0; column_i < 4; column_i++)
                {
                    var judge = false;
                    // 右横４つの判別
                    for (int i = 0; i < 4; i++)
                    {
                        if (_board[row_i, column_i + i] == playerNum)
                        {
                            judge = true;
                        }
                        else
                        {
                            judge = false;
                            break;
                        }
                    }
                    if (judge) isConnectFour = true;

                    if (row_i < 3)
                    {
                        // 右斜め上４つの判別
                        for (int i = 0; i < 4; i++)
                        {
                            if (_board[row_i + i, column_i + i] == playerNum)
                            {
                                judge = true;
                            }
                            else
                            {
                                judge = false;
                                break;
                            }
                        }
                    }
                    if (judge) isConnectFour = true;
                }
            }

            for (int column_i = 0; column_i < 7; column_i++)
            {
                for (int row_i = 0; row_i < 3; row_i++)
                {
                    var judge = false;
                    // 真上４つの判別
                    for (int i = 0; i < 4; i++)
                    {
                        if (_board[row_i + i, column_i] == playerNum)
                        {
                            judge = true;
                        }
                        else
                        {
                            judge = false;
                            break;
                        }
                    }
                    if (judge) isConnectFour = true;

                    if (column_i < 4)
                    {
                        // 左斜め上４つの判別
                        for (int i = 0; i < 4; i++)
                        {
                            if (_board[row_i + i, column_i + (3 - i)] == playerNum)
                            {
                                judge = true;
                            }
                            else
                            {
                                judge = false;
                                break;
                            }
                        }
                    }
                    if (judge) isConnectFour = true;
                }
            }
            return isConnectFour;
        }

        private bool isFirstPlayer()
        {
            return (coinCount(_board) % 2 == 0);
        }

        public ConnectFourState(int[,] board)
        {
            _board = (int[,])board.Clone();
        }

        public int getScore()
        {
            if (isLose()) return -1;
            else if (isDraw()) return 0;
            else return 0;
        }

        public bool isLose()
        {
            var playerNum = 1;
            if (isFirstPlayer()) playerNum = 2;

            return isConnectFour(playerNum);
        }

        public bool isDraw()
        {
            return coinCount(_board) == 42;
        }

        public bool isDone()
        {
            return isLose() || isDraw();
        }

        public void advance(int action)
        {
            var playerNum = 2;
            if (coinCount(_board) % 2 == 0) playerNum = 1;

            _board[action % 6, action / 6] = playerNum;
        }

        public void reverse(int action)
        {
            _board[action % 6, action / 6] = 0;
        }

        public List<int> legalActions()
        {
            var actions = new List<int>();
            for (int column_i = 0; column_i < 7; column_i++)
            {
                for (int row_i = 0; row_i < 6; row_i++)
                {
                    if (_board[row_i, column_i] == 0)
                    {
                        actions.Add(row_i + column_i * 6);
                        break;
                    }
                }
            }
            return actions;
        }
    }

}
