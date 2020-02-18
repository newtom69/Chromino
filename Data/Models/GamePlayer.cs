﻿using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace Data.Models
{
    [Table("GamePlayer")]
    public class GamePlayer
    {
        [Key]
        public int Id { get; set; }
        public int GameId { get; set; }
        public int PlayerId { get; set; }
        public bool Turn { get; set; }
        public bool Win { get; set; }
        public bool PreviouslyDraw { get; set; }
        public bool PreviouslyPass { get; set; }
        public int Points { get; set; }

        public Game Game { get; set; }
        public Player Player { get; set; }
    }
}
