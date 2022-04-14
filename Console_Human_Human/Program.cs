using System;
using System.Collections;
using System.Collections.Generic;
using System.Linq;
using System.IO;

namespace EasyConnectFour
{
    class Program
    {
        static void Main()
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
    }
}
