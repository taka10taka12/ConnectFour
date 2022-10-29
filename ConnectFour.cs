using System;
using System.Text;
using System.Collections;
using System.Collections.Generic;
using System.Linq;

namespace ConnectFour
{
    internal class Program
    {
        static void Main(string[] args)
        {
            // Player1の名前を入力
            Console.WriteLine("Player1の名前を入力して下さい");
            var player1 = Console.ReadLine().Trim();

            // Player2は人間orCOM
            Console.WriteLine("Player2を選択して下さい（人 => 1 , COM => 2）");
            var player2 = Console.ReadLine().Trim();
            if (player2 == "1")
            {
                Console.WriteLine("Player2の名前を入力して下さい");
                player2 = Console.ReadLine().Trim();
            }
            else
            {
                player2 = "COM";
            }

            // 先攻後攻決め
            Console.WriteLine($"Player1:{player1}は手番を選択して下さい（先攻 => 1, 後攻 => 2, ランダム => 3）");
            var isFirst = int.Parse(Console.ReadLine().Trim()); // ToDo isFirstの変数名変える
            if (isFirst == 3)
            {
                var rand = new Random();
                if (rand.Next(100) % 2 != 0)
                {
                    isFirst = 1;
                }
                else
                {
                    isFirst = 2;
                }
            }

            var firstPlayer = player1;
            var secondPlayer = player2;
            if (isFirst == 2) (firstPlayer, secondPlayer) = (secondPlayer, firstPlayer);

            // ゲーム開始
            Console.WriteLine($"先攻 : {firstPlayer}, 後攻 : {secondPlayer}");
            Console.WriteLine("ゲームを開始します");
            var game = new ConnectFour(firstPlayer, secondPlayer);
            game.Run();
        }
    }

    internal class ConnectFour
    {
        private string _firstPlayer { get; set; }
        private string _secondPlayer { get; set; }

        public ConnectFour(string firstPlayer, string secondPlayer)
        {
            this._firstPlayer = firstPlayer;
            this._secondPlayer = secondPlayer;
        }

        public void Run()
        {
            var turn = 1;
            var gameBoard = new int[6, 7];
            while (turn <= 43)
            {
                // ターン表示
                Console.WriteLine($"現在のターン : {turn}");

                // 盤面表示
                ConnectFourController.Display(gameBoard);

                // コインを投下するPlayerを表示
                var player = (turn % 2 == 1) ? this._firstPlayer : this._secondPlayer;
                Console.WriteLine($"{player}はコインを投下する列を入力して下さい");

                // PlayerがCOMの場合、MiniMax法使用
                // Playreが人の場合、投下する列を入力
                if (player == "COM")
                {
                    var method = new MiniMax(gameBoard);
                    var inputColumn = method.MiniMaxAction(8);
                    Console.WriteLine(inputColumn);
                    ConnectFourController.Advance(gameBoard, inputColumn);
                }
                else
                {
                    var inputColumn = -1;
                    for (; ; )
                    {
                        inputColumn = int.Parse(Console.ReadLine().Trim());
                        if (gameBoard[5, inputColumn] == 0) break;
                        else
                        {
                            Console.WriteLine("その列には投下できません。別の列を入力して下さい");
                        }
                    }
                    ConnectFourController.Advance(gameBoard, inputColumn);
                }

                // 勝敗判定
                if (ConnectFourController.isConnectFour(gameBoard))
                {
                    Console.WriteLine($"{player}の勝利");
                    ConnectFourController.Display(gameBoard);
                    break;
                }

                // 次のターンへ
                turn += 1;
            }

            // 決着がつかなかった場合
            if (ConnectFourController.isDraw(gameBoard))
            {
                Console.WriteLine("ゲーム終了！！引き分けです！！");
            }
        }
    }

    internal class MiniMax
    {
        private int[,] _gameBoard { get; set; }
        private int _depthMax { get; set; }

        public MiniMax(int[,] gameBoard)
        {
            this._gameBoard = (int[,])gameBoard.Clone();
        }

        // 引数で指定した深さまで探索し、最良の行動（コインを投下する列）を返す
        public int MiniMaxAction(int depthMax = 8)
        {
            // 探索する深さを指定
            this._depthMax = depthMax;

            // 初期値設定
            var depth = 0;
            var bestAction = 0;
            var bestScore = -10;

            // 評価値が0となる行動をまとめるリスト
            var list = new List<int>();

            // 現在の盤面からコイン投下可能な列を取得
            var actions = this.legalActions(this._gameBoard);

            // 投下可能な列にそれぞれコインを投下し、評価値が最大となる列を求める
            foreach (var column in actions)
            {
                // 一手進む
                ConnectFourController.Advance(this._gameBoard, column);

                // 評価値を求め、最大評価値を更新
                var score = -1 * this.MiniMaxScore(depth + 1);
                if (score > bestScore)
                {
                    bestAction = column;
                    bestScore = score;
                }

                // 評価値が0となる行動を記録
                if (score == 0) list.Add(column);

                // 一手戻る
                ConnectFourController.Reverse(this._gameBoard, column);
            }

            // 最大評価値が0の場合、評価値が0となる行動からランダムで選ぶ
            if (bestAction == 0)
            {
                var rand = new Random();
                bestAction = list[rand.Next(list.Count())];
            }
            Console.WriteLine($"bestScore : {bestScore} , bestAction : {bestAction}");

            return bestAction;
        }

        // 盤面の評価値を返す
        private int MiniMaxScore(int depth)
        {
            // 『現在の深さが規定の深さに達した場合』または『現在の盤面で決着がついている場合』に評価値を返す
            if (depth == this._depthMax || ConnectFourController.isDone(this._gameBoard))
            {
                return this.getScore(this._gameBoard);
            }

            // 初期値設定
            var bestScore = -10;

            // 現在の盤面からコイン投下可能な列を取得
            var actions = this.legalActions(this._gameBoard);

            // 投下可能な列にそれぞれコインを投下し、評価値が最大となる列を求める
            foreach (var column in actions)
            {
                // 一手進む
                ConnectFourController.Advance(this._gameBoard, column);

                // 評価値を求め、最大評価値を更新
                var score = -1 * this.MiniMaxScore(depth + 1);
                bestScore = Math.Max(bestScore, score);

                // 一手戻る
                ConnectFourController.Reverse(this._gameBoard, column);
            }

            return bestScore;
        }

        // 現在の盤面からコイン投下可能な列をリストで返す
        private List<int> legalActions(int[,] gameBoard)
        {
            var list = new List<int>();

            for (int ci = 0; ci < 7; ci++)
            {
                if (gameBoard[5, ci] == 0) list.Add(ci);
            }

            return list;
        }

        // 盤面の評価値を返す
        private int getScore(int[,] gameBoard)
        {
            // 現在の盤面がConnectFourの時のみ『-1』,それ以外は『0』を返す
            if (ConnectFourController.isConnectFour(gameBoard)) return -1;
            else return 0;
        }
    }

    internal static class ConnectFourController
    {
        // 引数の盤面を表示する
        internal static void Display(int[,] gameBoard)
        {
            Console.WriteLine("  0   1   2   3   4   5   6  ");
            for (int hi = 5; hi >= 0; hi--)
            {
                var sb = new StringBuilder();
                for (int wi = 0; wi < 7; wi++)
                {
                    sb.Append("| ");
                    sb.Append(gameBoard[hi, wi]);
                    sb.Append(" ");
                }
                sb.Append("|");
                Console.WriteLine(sb.ToString());
            }
        }

        // 引数に指定した盤面において、引数指定の列にコインを投下する
        internal static void Advance(int[,] gameBoard, int column)
        {
            var dropCoin = (Count(gameBoard) % 2 == 0) ? 1 : 2;
            for (int hi = 0; hi < 6; hi++)
            {
                if (gameBoard[hi, column] == 0)
                {
                    gameBoard[hi, column] = dropCoin;
                    break;
                }
            }
        }

        // 引数に指定した盤面において、引数指定の列の一番上のコインを回収する
        internal static void Reverse(int[,] gameBoard, int column)
        {
            for (int hi = 5; hi >= 0; hi--)
            {
                if (gameBoard[hi, column] != 0)
                {
                    gameBoard[hi, column] = 0;
                    break;
                }
            }
        }

        // 引数に指定した盤面に含まれるコインの枚数を数える
        internal static int Count(int[,] gameBoard)
        {
            var count = 0;
            for (int hi = 0; hi < 6; hi++)
            {
                for (int wi = 0; wi < 7; wi++)
                {
                    if (gameBoard[hi, wi] != 0) count++;
                }
            }
            return count;
        }

        // 引数に指定した盤面が『ConnectFour』かを判別する
        internal static bool isConnectFour(int[,] gameBoard)
        {
            // 返り値用の変数
            var judge = false;

            #region ConnectFourの判別処理
            // 縦４の判別
            for (int hi = 0; hi <= 2; hi++)
            {
                for (int wi = 0; wi < 7; wi++)
                {
                    for (int num = 1; num <= 2; num++)
                    {
                        if (
                                gameBoard[hi, wi] == num &&
                                gameBoard[hi + 1, wi] == num &&
                                gameBoard[hi + 2, wi] == num &&
                                gameBoard[hi + 3, wi] == num
                            )
                        {
                            judge = true;
                        }
                    }
                }
            }

            // 横４の判別
            for (int hi = 0; hi < 6; hi++)
            {
                for (int wi = 0; wi <= 3; wi++)
                {
                    for (int num = 1; num <= 2; num++)
                    {
                        if (
                                gameBoard[hi, wi] == num &&
                                gameBoard[hi, wi + 1] == num &&
                                gameBoard[hi, wi + 2] == num &&
                                gameBoard[hi, wi + 3] == num
                            )
                        {
                            judge = true;
                        }
                    }
                }
            }

            // 右上４の判別
            for (int hi = 0; hi <= 2; hi++)
            {
                for (int wi = 0; wi <= 3; wi++)
                {
                    for (int num = 1; num <= 2; num++)
                    {
                        if (
                                gameBoard[hi, wi] == num &&
                                gameBoard[hi + 1, wi + 1] == num &&
                                gameBoard[hi + 2, wi + 2] == num &&
                                gameBoard[hi + 3, wi + 3] == num
                            )
                        {
                            judge = true;
                        }
                    }
                }
            }

            // 右下４の判別
            for (int hi = 3; hi < 6; hi++)
            {
                for (int wi = 0; wi <= 3; wi++)
                {
                    for (int num = 1; num <= 2; num++)
                    {
                        if (
                                gameBoard[hi, wi] == num &&
                                gameBoard[hi - 1, wi + 1] == num &&
                                gameBoard[hi - 2, wi + 2] == num &&
                                gameBoard[hi - 3, wi + 3] == num
                            )
                        {
                            judge = true;
                        }
                    }
                }
            }
            #endregion

            return judge;
        }

        // 引数指定の盤面が『コインで全て埋まっているかどうか』を判別する
        internal static bool isDraw(int[,] gameBoard)
        {
            return Count(gameBoard) == 42;
        }

        // 引数指定の盤面が『すでに決着がついているかどうか』を判別する
        internal static bool isDone(int[,] gameBoard)
        {
            return isDraw(gameBoard) || isConnectFour(gameBoard);
        }

    }
}
