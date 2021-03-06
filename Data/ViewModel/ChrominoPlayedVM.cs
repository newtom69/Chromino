﻿using Data.Enumeration;
using Data.Models;

namespace Data.ViewModel
{
    public class ChrominoPlayedVM
    {
        public short[] IndexesX { get; set; } = new short[3];
        public short[] IndexesY { get; set; } = new short[3];
        public int PlayerId { get; set; }
        public string PlayerPseudo { get; set; }
        public byte Move { get; set; }
        public int? ChrominoId { get; set; }

        public ChrominoPlayedVM(ChrominoInGame chrominoInGame, int xMin, int yMin)
        {
            ChrominoId = chrominoInGame.ChrominoId;
            if (ChrominoId != null)
            {
                int indexX = chrominoInGame.XPosition - xMin;
                int indexY = chrominoInGame.YPosition - yMin;
                IndexesX[0] = (short)indexX;
                IndexesY[0] = (short)indexY;
                switch (chrominoInGame.Orientation)
                {
                    case Orientation.Horizontal:
                        IndexesX[1] = (short)(indexX + 1);
                        IndexesY[1] = (short)indexY;
                        IndexesX[2] = (short)(indexX + 2);
                        IndexesY[2] = (short)indexY;
                        break;
                    case Orientation.Vertical:
                        IndexesX[1] = (short)indexX;
                        IndexesX[2] = (short)indexX;
                        IndexesY[1] = (short)(indexY + 1);
                        IndexesY[2] = (short)(indexY + 2);
                        break;
                }
            }
            PlayerId = chrominoInGame.PlayerId ?? 0;
            PlayerPseudo = chrominoInGame.Player == null ? "First chromino" : chrominoInGame.Player.UserName;
            Move = chrominoInGame.Move;
        }
    }
}
