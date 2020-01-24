﻿using Data.Core;
using Data.Enumeration;
using Data.Models;
using System;
using System.Collections.Generic;
using System.Text;
using System.Linq;

namespace Data.ViewModel
{
    public class GameViewModel
    {
        public int GameId { get; set; }
        public bool AutoPlay { get; set; }
        public List<Square> Squares { get; set; }
        public int XMin { get; set; }
        public int XMax { get; set; }
        public int YMin { get; set; }
        public int YMax { get; set; }
        public int LinesNumber { get; set; }
        public int ColumnsNumber { get; set; }
        public int SquaresNumber { get; set; }
        public SquareViewModel[] SquaresViewModel { get; set; }
        public int ChrominosInGame { get; set; }
        public int ChrominosInStack { get; set; }
        public List<int> PlayerNumberChrominos { get; set; }
        public GameStatus GameStatus { get; set; }
        public List<ChrominoViewModel> IdentifiedPlayerChrominosViewModel { get; set; }

        public GameViewModel(int gameId, List<Square> squares, bool autoPlay, GameStatus gameStatus, int chrominosInGame, int chrominosInStack, List<int> playerNumberChrominos, List<Chromino> identifiedPlayerChrominos)
        {
            AutoPlay = autoPlay;
            GameId = gameId;
            ChrominosInGame = chrominosInGame;
            ChrominosInStack = chrominosInStack;
            PlayerNumberChrominos = playerNumberChrominos;
            Squares = squares;
            GameStatus = gameStatus;
            XMin = squares.Select(g => g.X).Min() - 1; // +- 1 pour marge permettant de poser un chromino sur un bord
            XMax = squares.Select(g => g.X).Max() + 1;
            YMin = squares.Select(g => g.Y).Min() - 1;
            YMax = squares.Select(g => g.Y).Max() + 1;

            ColumnsNumber = XMax - XMin + 1;
            LinesNumber = YMax - YMin + 1;

            SquaresNumber = ColumnsNumber * LinesNumber;

            SquaresViewModel = new SquareViewModel[SquaresNumber];
            for (int i = 0; i < SquaresViewModel.Length; i++)
            {
                SquaresViewModel[i] = new SquareViewModel
                {
                    State = SquareViewModelState.Free,
                    Edge = OpenEdge.All,
                };
            }

            foreach (Square square in Squares)
            {
                int index = IndexGridState(square.X, square.Y);
                SquaresViewModel[index] = square.SquareViewModel;
            }

            IdentifiedPlayerChrominosViewModel = new List<ChrominoViewModel>();
            foreach (Chromino chromino in identifiedPlayerChrominos)
            {
                SquareViewModel square1 = new SquareViewModel { State = (SquareViewModelState)chromino.FirstColor, Edge = OpenEdge.Right };
                SquareViewModel square2 = new SquareViewModel { State = (SquareViewModelState)chromino.SecondColor, Edge = OpenEdge.RightLeft };
                SquareViewModel square3 = new SquareViewModel { State = (SquareViewModelState)chromino.ThirdColor, Edge = OpenEdge.Left };
                ChrominoViewModel chrominoViewModel = new ChrominoViewModel();
                chrominoViewModel.SquaresViewModel[0] = square1;
                chrominoViewModel.SquaresViewModel[1] = square2;
                chrominoViewModel.SquaresViewModel[2] = square3;
                chrominoViewModel.ChrominoId = chromino.Id;
                //dernier modif à répercuter
                IdentifiedPlayerChrominosViewModel.Add(chrominoViewModel);
                //TODO faire edge
            }
        }


        public int IndexGridState(int x, int y)
        {
            if (x < XMin || x > XMax || y < YMin || y > YMax)
                throw new IndexOutOfRangeException();

            return y * ColumnsNumber + x - (YMin * ColumnsNumber + XMin);
        }



    }


}
