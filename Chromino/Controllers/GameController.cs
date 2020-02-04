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
            string[] pseudosNotNull = pseudos.Where(c => c != null).ToArray();

            string error = null;
            List<Player> players = new List<Player>(8);
            for (int i = 0; i < pseudosNotNull.Length; i++)
            {
                Player player = PlayerDal.Details(pseudosNotNull[i]);
                if (player != null && !players.Contains(player))
                    players.Add(player);
                else
                    error = $"pseudo {pseudosNotNull[i]} doesn't exist or already in game";
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
            if (players.Count == 1 && PlayerDal.IsBot(players[0].Id))
            {
                return RedirectToAction("PlayBot", "Game", new { id });
            }
            else
            {
                // TODO jeux "normal"
                return View();
            }
        }

        [HttpPost]
        public IActionResult Play(int playerId, int gameId, int chrominoId, int x, int y, Orientation orientation)
        {
            GetPlayerInfosFromSession();

            GameCore gameCore = new GameCore(Ctx, gameId);
            ChrominoInHand chrominoInHand = GameChrominoDal.Details(gameId, chrominoId);
            ChrominoInGame chrominoInGame = new ChrominoInGame()
            {
                GameId = gameId,
                ChrominoId = chrominoId,
                XPosition = x,
                YPosition = y,
                Orientation = orientation,
            };
            bool move = gameCore.Play(chrominoInGame, playerId);
            if (!move)
            {
                // todo : position pas bonne. avertir joueur
            }
            else
            {
                //Ctx.ChrominosInHand.Remove(chrominoInHand);
                //Ctx.SaveChanges();
                NextPlayerPlayIfBot(gameId, gameCore);
            }

            return RedirectToAction("Show", "Game", new
            {
                id = gameId
            });
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
                List<int> botsId = PlayerDal.BotsId();

                GameVM gameViewModel = new GameVM(id, squares, autoPlay, gameStatus, chrominosInGame, chrominosInStack, pseudos_chrominos, identifiedPlayerChrominos, playerTurn, gamePlayerTurn, botsId);
                return View(gameViewModel);
            }
            else
            {
                return RedirectToAction("NotFound");
            }
        }

        public IActionResult PlayBot(int id, int botId)
        {
            GetPlayerInfosFromSession();

            GameCore gamecore = new GameCore(Ctx, id);
            gamecore.PlayBot(botId);
            return RedirectToAction("Show", "Game", new { id });
        }

        [HttpPost]
        public IActionResult AutoPlay(int gameId, int botId, bool autoPlay)
        {
            GetPlayerInfosFromSession();

            GameDal.SetAutoPlay(gameId, autoPlay);
            return RedirectToAction("PlayBot", "Game", new { id = gameId, botId = botId });
        }

        public IActionResult NotFound()
        {
            return View();
        }

        private void NextPlayerPlayIfBot(int gameId, GameCore gameCore)
        {
            int playerId = GamePlayerDal.PlayerTurn(gameId).Id;
            if (PlayerDal.IsBot(playerId))
            {
                bool play;
                while (!(play = gameCore.PlayBot(playerId))) ;
            }
        }
    }
}
