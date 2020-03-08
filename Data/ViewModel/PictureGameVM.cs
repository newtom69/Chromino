﻿using System;
using System.Collections.Generic;
using System.Text;

namespace Data.ViewModel
{
    public class PictureGameVM
    {
        public int GameId { get; private set; }
        public string FullName { get; private set; }
        public Dictionary<string, int> Pseudos_chrominos { get; private set; }
        public string PlayerPseudoTurn { get; private set; }
        public DateTime PlayedDate { get; private set; }


        public PictureGameVM(int gameId, string fullName, Dictionary<string, int> pseudos_chrominos, string playerPseudoTurn, DateTime playedDate)
        {
            GameId = gameId;
            FullName = fullName;
            Pseudos_chrominos = pseudos_chrominos;
            PlayerPseudoTurn = playerPseudoTurn;
            PlayedDate = playedDate;
        }
    }
}