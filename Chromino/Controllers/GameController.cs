﻿using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.Rendering;
using Microsoft.EntityFrameworkCore;
using Data;
using Data.Models;
using Data.Core;
using Data.DAL;
using Data.Enumeration;
using Data.ViewModel;
using System.Globalization;

namespace Controllers
{
    public class GameController : CommonController
    {
        public GameController(DefaultContext context) : base(context)
        {
        }

        [HttpGet]
        public IActionResult StartNew()
        {
            GetPlayerInfosFromSession();

            return View(null);
        }


        [HttpPost]
        public IActionResult StartNew(string[] pseudos)
        {
            GetPlayerInfosFromSession();

            if (pseudos == null || pseudos.Length == 0)
            {
                return View();
            }

            string error = null;
            Player playerBot = PlayerDal.Bot();

            List<Player> players = new List<Player>(8);
            if (pseudos[0] != null && pseudos[0].ToUpperInvariant() != "BOT")
            {
                Player player = PlayerDal.Details(pseudos[0]);
                if (player != null)
                    players.Add(player);
                else
                    error = "pseudo 1 doesn't exist";
            }
            else
            {
                players.Add(playerBot);
            }
            for (int i = 1; i < pseudos.Length; i++)
            {
                if (pseudos[i] != null)
                {
                    Player player = PlayerDal.Details(pseudos[i]);
                    if (player != null)
                        players.Add(player);
                    else
                        error = $"pseudo {i} doesn't exist";
                }
            }

            if (error != null)
            {
                ViewBag.error = error;
                return View(pseudos);
            }

            ChrominoDal.CreateChrominos();
            int gameId = GameDal.AddGame().Id;
            GamePlayerDal.Add(gameId, players);
            GameCore gamecore = new GameCore(Ctx, gameId);
            gamecore.BeginGame();
            return RedirectToAction("Show", "Game", new { id = gameId });
        }

        public IActionResult ContinueGame(int id)
        {
            GetPlayerInfosFromSession();

            List<Player> players = GamePlayerDal.Players(id);
            if (players.Count == 1 && players[0].Pseudo == "bot")
            {
                return RedirectToAction("PlayBot", "Game", new { id });
            }
            else
            {
                // TODO jeux "normal"
                return View();
            }
        }

        public IActionResult PlayBot(int id)
        {
            GetPlayerInfosFromSession();

            Player bot = PlayerDal.Bot();
            GameCore gamecore = new GameCore(Ctx, id);
            gamecore.PlayBot();

            return RedirectToAction("Show", "Game", new { id });
        }

        [HttpPost]
        public IActionResult Play(int playerId, int gameId, int chrominoId, int x, int y, Orientation orientation)
        {
            GetPlayerInfosFromSession();

            GameCore gameCore = new GameCore(Ctx, gameId);
            ChrominoInGame chrominoInGame = GameChrominoDal.Details(gameId, chrominoId);
            chrominoInGame.XPosition = x;
            chrominoInGame.YPosition = y;
            chrominoInGame.Orientation = orientation;
            bool move = gameCore.Play(chrominoInGame, playerId);
            if (!move)
            {
                // todo : position pas bonne. avertir joueur
            }
            else
            {
                NextPlayerPlayIfBot(gameId, gameCore);
            }

            return RedirectToAction("Show", "Game", new { id = gameId });
        }


        [HttpPost]
        public IActionResult DrawChromino(int playerId, int gameId)
        {
            GetPlayerInfosFromSession();

            GameCore gameCore = new GameCore(Ctx, gameId);
            int playersNumber = GamePlayerDal.PlayersNumber(gameId);
            GamePlayer gamePlayer = GamePlayerDal.Details(gameId, playerId);
            if (playerId == PlayerId && (!gamePlayer.PreviouslyDraw || playersNumber == 1))
            {
                gameCore.DrawChromino(playerId);
            }
            return RedirectToAction("Show", "Game", new { id = gameId });
        }

        [HttpPost]
        public IActionResult PassTurn(int playerId, int gameId)
        {
            GetPlayerInfosFromSession();
            if (playerId == PlayerId)
            {
                GameCore gameCore = new GameCore(Ctx, gameId);
                gameCore.PassTurn(playerId);
                NextPlayerPlayIfBot(gameId, gameCore);


            }
            return RedirectToAction("Show", "Game", new { id = gameId });
        }


        // todo : [HttpPost]
        public IActionResult Show(int id)
        {
            GetPlayerInfosFromSession();

            List<Player> players = GamePlayerDal.Players(id);
            int playersNumber = players.Count;

            if (players.Where(x => x.Id == PlayerId).FirstOrDefault() != null || players.Count == 1 && players[0].Id == BotId) // identified player in the game or only bot play
            {
                int chrominosInGame = GameChrominoDal.InGame(id);
                int chrominosInStack = GameChrominoDal.InStack(id);

                Dictionary<string, int> pseudos_chrominos = new Dictionary<string, int>();
                foreach (Player player in players)
                {
                    pseudos_chrominos.Add(player.Pseudo, GameChrominoDal.PlayerNumberChrominos(id, player.Id));
                }

                List<Chromino> identifiedPlayerChrominos = new List<Chromino>();
                if (players.Count == 1) // si un seul joueur, soit bot seul et on affiche ses chrominos, soit le joueur identifié est nécessairement le joueur courant et on affiche aussi ses chrominos
                {
                    identifiedPlayerChrominos = ChrominoDal.PlayerChrominos(id, players[0].Id);
                }
                else
                {
                    identifiedPlayerChrominos = ChrominoDal.PlayerChrominos(id, PlayerId);
                }
                Game game = GameDal.Details(id);
                GameStatus gameStatus = game.Status;
                bool autoPlay = game.AutoPlay;
                Player playerTurn = GamePlayerDal.PlayerTurn(id);
                GamePlayer gamePlayerTurn = GamePlayerDal.Details(id, playerTurn.Id);
                List<Square> squares = SquareDal.List(id);
                GameVM gameViewModel = new GameVM(id, squares, autoPlay, gameStatus, chrominosInGame, chrominosInStack, pseudos_chrominos, identifiedPlayerChrominos, playerTurn, gamePlayerTurn, playersNumber);
                return View(gameViewModel);
            }
            else
            {
                return RedirectToAction("NotFound");
            }
        }

        [HttpPost]
        public IActionResult AutoPlay(int gameId, bool autoPlay)
        {
            GetPlayerInfosFromSession();

            GameDal.SetAutoPlay(gameId, autoPlay);
            return RedirectToAction("PlayBot", "Game", new { id = gameId });
        }

        public IActionResult NotFound()
        {
            return View();
        }

        private void NextPlayerPlayIfBot(int gameId, GameCore gameCore)
        {
            if (PlayerDal.IsBot(GamePlayerDal.PlayerTurn(gameId).Id))
            {
                bool play;
                while (!(play = gameCore.PlayBot())) ;
            }
        }
    }
}
