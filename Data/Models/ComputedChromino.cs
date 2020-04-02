﻿using Data.Enumeration;
using System.ComponentModel.DataAnnotations.Schema;

namespace Data.Models
{
    [Table("ComputedChromino")]
    public class ComputedChromino
    {
        public int Id { get; set; }
        public int ChrominoId { get; set; }
        public int GameId { get; set; }
        public int PlayerId { get; set; }
        public Orientation Orientation { get; set; }
        public bool Flip { get; set; }
        public int X { get; set; }
        public int Y { get; set; }
        public int? ParentId { get; set; }

        public Chromino Chromino { get; set; }
        public Game Game { get; set; }
        public Player Bot { get; set; }
        public ComputedChromino Parent { get; set; }

        public override bool Equals(object c)
        {
            if (c == null || !(c is ComputedChromino))
                return false;
            else
                return X == ((ComputedChromino)c).X && Y == ((ComputedChromino)c).Y && Orientation == ((ComputedChromino)c).Orientation;
        }

        public override int GetHashCode()
        {
            return (X * 10000 + Y * 10 + Orientation).GetHashCode();
        }
    }
}