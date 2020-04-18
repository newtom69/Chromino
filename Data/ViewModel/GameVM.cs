﻿using Data.Enumeration;
using Data.Models;
using System;
using System.Collections.Generic;
using System.Linq;

namespace Data.ViewModel
{
    public class GameVM
    {
        public Player Player { get; set; }
        public int XMin { get; set; }
        public int XMax { get; set; }
        public int YMin { get; set; }
        public int YMax { get; set; }
        public int LinesNumber { get; set; }
        public int ColumnsNumber { get; set; }
        public int SquaresNumber { get; set; }
        public SquareVM[] SquaresVM { get; set; }
        public int ChrominosInStack { get; set; }
        public Dictionary<string, int> PseudosChrominos { get; set; }
        public Dictionary<string, ChrominoVM> Pseudos_LastChrominoVM { get; set; }
        public List<ChrominoVM> PlayerChrominosVM { get; set; }
        public Player PlayerTurn { get; set; }
        public Game Game { get; set; }
        public GamePlayer GamePlayerTurn { get; set; }
        public GamePlayer GamePlayer { get; set; }
        public List<int> BotsId { get; set; }
        public List<ChrominoPlayedVM> ChrominosPlayedVM { get; set; }
        public List<string> Pseudos { get; set; }
        public bool OpponenentsAreBots { get; set; }
        public List<PossiblesChrominoVM> PossiblesChrominosVM { get; set; }
        public bool ShowPossiblesPositions { get; set; }
        public List<string> TipsDomElementIdOn { get; set; }
        public List<Tip> Tips { get; set; }

        public GameVM(Game game, Player player, List<Square> squares, int chrominosInStackNumber, Dictionary<string, int> pseudosChrominos, List<Chromino> playerChrominos, Player playerTurn, GamePlayer gamePlayerTurn, GamePlayer gamePlayer, List<int> botsId, Dictionary<string, Chromino> pseudos_lastChrominos, List<ChrominoInGame> chrominosInGamePlayed, List<string> pseudos, bool opponenentsAreBots, List<GoodPosition> goodPositions, bool showPossiblesPositions, List<string> tipsDomElementIdOn, List<Tip> tipsOn)
        {
            Player = player;
            OpponenentsAreBots = opponenentsAreBots;
            Game = game;
            PlayerTurn = playerTurn;
            GamePlayerTurn = gamePlayerTurn;
            GamePlayer = gamePlayer;
            ChrominosInStack = chrominosInStackNumber;
            BotsId = botsId;
            XMin = squares.Select(g => g.X).Min() - 2; // +- 2 pour marge permettant de poser un chromino sur un bord
            XMax = squares.Select(g => g.X).Max() + 2;
            YMin = squares.Select(g => g.Y).Min() - 2;
            YMax = squares.Select(g => g.Y).Max() + 2;
            ColumnsNumber = XMax - XMin + 1;
            LinesNumber = YMax - YMin + 1;
            SquaresNumber = ColumnsNumber * LinesNumber;
            SquaresVM = new SquareVM[SquaresNumber];
            for (int i = 0; i < SquaresVM.Length; i++)
                SquaresVM[i] = new SquareVM(ColorCh.None);
            foreach (Square square in squares)
                SquaresVM[IndexGridState(square.X, square.Y)] = square.SquareViewModel;

            PlayerChrominosVM = new List<ChrominoVM>();
            foreach (Chromino chromino in playerChrominos)
                PlayerChrominosVM.Add(new ChrominoVM() { ChrominoId = chromino.Id, SquaresVM = new SquareVM[3] { new SquareVM(chromino.FirstColor), new SquareVM(chromino.SecondColor), new SquareVM(chromino.ThirdColor) } });

            Pseudos_LastChrominoVM = new Dictionary<string, ChrominoVM>();
            foreach (var pseudo_chromino in pseudos_lastChrominos)
                Pseudos_LastChrominoVM.Add(pseudo_chromino.Key != Player.UserName ? pseudo_chromino.Key : "Vous", new ChrominoVM() { ChrominoId = pseudo_chromino.Value.Id, SquaresVM = new SquareVM[3] { new SquareVM(pseudo_chromino.Value.FirstColor, true, false, false, false), new SquareVM(pseudo_chromino.Value.SecondColor, true, false, true, false), new SquareVM(pseudo_chromino.Value.ThirdColor, true, false, true, false) } });

            ChrominosPlayedVM = new List<ChrominoPlayedVM>();
            foreach (ChrominoInGame chrominoInGame in chrominosInGamePlayed)
                ChrominosPlayedVM.Add(new ChrominoPlayedVM(chrominoInGame, XMin, YMin));

            Pseudos = pseudos;
            PseudosChrominos = pseudosChrominos;
            int indexPlayerPseudo = Pseudos.IndexOf(Player.UserName);
            if (indexPlayerPseudo != -1)
            {
                Pseudos[indexPlayerPseudo] = "Vous";
                int value = PseudosChrominos[Player.UserName];
                PseudosChrominos.Remove(Player.UserName);
                PseudosChrominos["Vous"] = value;
            }

            ShowPossiblesPositions = showPossiblesPositions;
            PossiblesChrominosVM = new List<PossiblesChrominoVM>();
            if (showPossiblesPositions)
                foreach (GoodPosition goodPosition in goodPositions)
                    PossiblesChrominosVM.Add(new PossiblesChrominoVM(goodPosition, XMin, YMin));

            TipsDomElementIdOn = tipsDomElementIdOn;
            Tips = tipsOn;
        }

        private int IndexGridState(int x, int y)
        {
            if (x < XMin || x > XMax || y < YMin || y > YMax)
                throw new IndexOutOfRangeException();

            return y * ColumnsNumber + x - (YMin * ColumnsNumber + XMin);
        }
    }
}