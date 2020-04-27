﻿using ChrominoBI;
using Data;
using Data.Core;
using Data.DAL;
using Data.Enumeration;
using Data.Models;
using Data.ViewModel;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Hosting;
using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Mvc;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading;
using System.Threading.Tasks;
using Tool;

namespace Controllers
{
    [Authorize]
    public partial class GameController : CommonController
    {
        public GameController(Context context, UserManager<Player> userManager, IWebHostEnvironment env) : base(context, userManager, env)
        {
        }

        /// <summary>
        /// affiche à l'écran la prochaine partie à jouer
        /// </summary>
        /// <returns></returns>
        public IActionResult ShowNextToPlay()
        {
            TempData["ByShowNextToPlay"] = true;
            int gameId = GamePlayerDal.FirstIdMultiGameToPlay(PlayerId);
            return gameId == 0 ? RedirectToAction("Index", "Home") : RedirectToAction("Show", new { id = gameId });
        }

        /// <summary>
        /// page de création de partie
        /// </summary>
        /// <returns></returns>
        [HttpGet]
        public IActionResult New()
        {
            return View(null);
        }

        /// <summary>
        /// retour du formulaire de création de partie
        /// </summary>
        /// <param name="pseudos">pseudos des joueurs de la partie à créer</param>
        /// <returns></returns>
        [HttpPost]
        public IActionResult New(string[] pseudos, int botsNumber)
        {
            const int MaxPlayers = 8;
            if (pseudos == null || pseudos.Length == 0)
                return View();

            List<string> listPseudos = pseudos.ToList();
            listPseudos.Add(PlayerPseudo);
            listPseudos.Reverse();
            string[] pseudosNotNull = listPseudos.Where(c => c != null).ToArray();

            List<string> errors = new List<String>();
            List<Player> players = new List<Player>(8);
            for (int i = 0; i < pseudosNotNull.Length; i++)
            {
                Player player = PlayerDal.Details(pseudosNotNull[i]);
                if (player == null)
                    errors.Add($"Le joueur {pseudosNotNull[i]} n'existe pas");
                else if (pseudosNotNull[i].Contains("bot", new StringComparison()))
                    errors.Add($"Le joueur {pseudosNotNull[i]} est un bot et ne peut pas être ajouté ici");
                else if (players.Contains(player))
                    errors.Add($"Le joueur {pseudosNotNull[i]} est ajouté plusieurs fois dans la partie");
                else
                    players.Add(player);
            }

            if (botsNumber + players.Count > MaxPlayers)
                errors.Add($"Le nombre de joueurs est supérieur à {MaxPlayers}. Il ne faut pas chercher à dépasser les bornes des limites ! ;-)");
            else if (botsNumber < 0)
                errors.Add($"Le nombre de bots est inférieur à 0. Bien tenté mais ça ne passe pas ici ! 😉");

            if (errors.Count != 0)
            {
                TempData["Errors"] = errors;
                TempData["PlayersNumber"] = pseudosNotNull.Length;
                TempData["Players"] = pseudos;
                return RedirectToAction("AgainstFriends", "Games", "newgame");
            }
            for (int i = 1; i < botsNumber + 1; i++)
                players.Add(PlayerDal.Details("bot" + i));
            CreateGame(players, out int gameId);
            return RedirectToAction("Show", new { id = gameId });
        }

        /// <summary>
        /// retour du formulaire de création de partie contre bots
        /// </summary>
        /// <param name="botsNumber">nombre d'adversaires bots</param>
        /// <returns></returns>
        [HttpPost]
        public IActionResult NewAgainstBots(int botsNumber)
        {
            List<Player> players = new List<Player>(botsNumber + 1) { PlayerDal.Details(PlayerId) };
            for (int iBot = 1; iBot <= botsNumber; iBot++)
                players.Add(PlayerDal.Details("bot" + iBot));
            CreateGame(players, out int gameId);
            return RedirectToAction("Show", new { id = gameId });
        }

        /// <summary>
        /// page de création de partie solo
        /// </summary>
        /// <returns></returns>
        [HttpGet]
        public IActionResult NewSingle()
        {
            List<Player> players = new List<Player> { PlayerDal.Details(PlayerId) };
            CreateGame(players, out int gameId);
            return RedirectToAction("Show", new { id = gameId });
        }

        /// <summary>
        /// création d'une partie "revanche"
        /// </summary>
        /// <returns></returns>
        [HttpPost]
        public IActionResult Rematch(string[] playersName)
        {
            List<Player> players = new List<Player>();
            foreach (string name in playersName)
                players.Add(PlayerDal.Details(name));
            CreateGame(players, out int gameId);
            return RedirectToAction("Show", new { id = gameId });
        }

        /// <summary>
        /// affiche une partie 
        /// </summary>
        /// <param name="id">Id de la partie</param>
        /// <returns></returns>
        public async Task<IActionResult> ShowAsync(int id)
        {
            if (id == 0)
                return RedirectToAction("GameNotFound");

            Player player = PlayerDal.Details(PlayerId);
            bool isAdmin = await UserManager.IsInRoleAsync(player, "Admin");

            bool showPossiblesPositions = false;

            GameVM gameVM = new GameBI(Ctx, Env, id).GameVM(PlayerId, isAdmin);
            if (gameVM != null)
            {
                if (GamePlayerDal.PlayerTurn(id).Bot)
                    ViewData["ShowBotPlayingInfoPopup"] = true;
                return View(gameVM);
            }
            else
            {
                return RedirectToAction("GameNotFound");
            }
        }

        /// <summary>
        /// tente de jouer le chromino à l'emplacement choisit par le joueur
        /// </summary>
        /// <param name="gameId">id du jeu</param>
        /// <param name="chrominoId">id du chromino</param>
        /// <param name="x">abscisse (0 étant le Caméléon du premier chromino du jeu)</param>
        /// <param name="y">ordonnée (0 étant le Caméléon du premier chromino du jeu)</param>
        /// <param name="orientation">vertical, horizontal</param>
        /// <param name="flip">true si le chromino est tourné de 180°</param>
        /// <returns></returns>
        [HttpPost]
        public JsonResult Play(int gameId, int chrominoId, int x, int y, Orientation orientation, bool flip)
        {
            if (GamePlayerDal.PlayerTurn(gameId).Id != PlayerId)
                return new JsonResult(new { errorReturn = PlayReturn.NotPlayerTurn.ToString() });
            if (GameDal.IsFinished(gameId))
                return new JsonResult(new { errorReturn = PlayReturn.ErrorGameFinish.ToString() });
            if (ChrominoInHandDal.ChrominosNumber(gameId, PlayerId) == 1 && ChrominoDal.IsCameleon(chrominoId))
                return new JsonResult(new { errorReturn = PlayReturn.LastChrominoIsCameleon.ToString() });

            ChrominoInGame chrominoInGame = new ChrominoInGame() { GameId = gameId, PlayerId = PlayerId, ChrominoId = chrominoId, XPosition = x, YPosition = y, Orientation = orientation, Flip = flip };
            PlayReturn playReturn = new PlayerBI(Ctx, Env, gameId, PlayerId).Play(chrominoInGame);
            if (playReturn.IsError())
                return new JsonResult(new { errorReturn = playReturn.ToString() });
            else
            {
                List<string> lastChrominoColors = ColorsLastChromino(gameId, PlayerId);
                List<string> playedChrominocolors = ColorsPlayedChromino(chrominoId);
                int nextPlayerId = GamePlayerDal.PlayerTurn(gameId).Id;
                bool finish = GameDal.IsFinished(gameId);
                return new JsonResult(new { nextPlayerId = nextPlayerId, isBot = PlayerDal.IsBot(nextPlayerId), colors = playedChrominocolors, lastChrominoColors = lastChrominoColors, finish = finish });
            }
        }

        /// <summary>
        /// fait jouer un bot
        /// </summary>
        /// <param name="id">Id de la partie</param>
        /// <param name="botId">Id du bot</param>
        /// <returns></returns>
        public IActionResult PlayBot(int id, int botId)
        {
            ChrominoInGame chrominoInGame;
            BotBI botBI = new BotBI(Ctx, Env, id, botId);
            PlayReturn playreturn;
            bool botDraw = false;
            do
            {
                playreturn = botBI.PlayBot(out chrominoInGame);
                if (playreturn == PlayReturn.DrawChromino)
                    botDraw = true;
            }
            while (playreturn.IsError() || playreturn == PlayReturn.DrawChromino);

            List<string> lastChrominoColors = ColorsLastChromino(id, botId);
            int nextPlayerId = GamePlayerDal.PlayerTurn(id).Id;
            bool finish = GameDal.GetStatus(id).IsFinished();
            if (chrominoInGame != null)
            {
                List<string> playedChrominocolors = ColorsPlayedChromino(chrominoInGame.ChrominoId);
                return new JsonResult(new { botName = PlayerDal.Name(botId), botSkip = false, botDraw = botDraw, nextPlayerId = nextPlayerId, isBot = PlayerDal.IsBot(nextPlayerId), x = chrominoInGame.XPosition, y = chrominoInGame.YPosition, orientation = chrominoInGame.Orientation, flip = chrominoInGame.Flip, colors = playedChrominocolors, lastChrominoColors = lastChrominoColors, finish = finish });
            }
            else
            {
                return new JsonResult(new { botName = PlayerDal.Name(botId), botSkip = true, botDraw = botDraw, nextPlayerId = nextPlayerId, isBot = PlayerDal.IsBot(nextPlayerId), finish = finish });
            }
        }

        /// <summary>
        /// Pioche un chromino
        /// </summary>
        /// <param name="gameId">id du jeu</param>
        /// <returns></returns>
        [HttpPost]
        public JsonResult DrawChromino(int gameId)
        {
            if (GamePlayerDal.PlayerTurn(gameId).Id != PlayerId)
                return new JsonResult(new { errorReturn = PlayReturn.NotPlayerTurn.ToString() });
            if (GamePlayerDal.Details(gameId, PlayerId).PreviouslyDraw && GamePlayerDal.PlayersNumber(gameId) != 1)
                return new JsonResult(new { errorReturn = PlayReturn.CantDraw2TimesInARow.ToString() });
            if (ChrominoInGameDal.InStack(gameId) == 0)
                return new JsonResult(new { errorReturn = PlayReturn.NoMoreChrominosInStack.ToString() });

            int chrominoId = new PlayerBI(Ctx, Env, gameId, PlayerId).TryDrawChromino(out _);
            List<string> colors = ColorsPlayedChromino(chrominoId);
            return new JsonResult(new { id = chrominoId, colors = colors });
        }

        /// <summary>
        /// Passe le tour du joueur
        /// </summary>
        /// <param name="gameId">Id du jeu</param>
        /// <returns></returns>
        [HttpPost]
        public JsonResult SkipTurn(int gameId)
        {
            if (GamePlayerDal.PlayerTurn(gameId).Id != PlayerId)
                return new JsonResult(new { errorReturn = PlayReturn.NotPlayerTurn.ToString() });

            int nextPlayerId = new PlayerBI(Ctx, Env, gameId, PlayerId).SkipTurn();
            bool finish = GameDal.GetStatus(gameId).IsFinished();
            return new JsonResult(new { id = nextPlayerId, isBot = PlayerDal.IsBot(nextPlayerId), finish = finish });
        }

        /// <summary>
        /// Affiche les positions possible pour les chrominos de la main du joueur
        /// </summary>
        /// <param name="gameId">id du jeu</param>
        /// <returns></returns>
        [HttpPost]
        public IActionResult Help(int gameId)
        {
            bool status = PlayerDal.DecreaseHelp(PlayerId);
            HashSet<int> possibleSquareIndexes = new HashSet<int>();
            if (status)
            {
                int xMin = SquareDal.XMin(gameId) - 2; // -2 : marges
                int xMax = SquareDal.XMax(gameId) + 2;
                int yMin = SquareDal.YMin(gameId) - 2;
                int columnsNumber = xMax - xMin + 1;
                List<GoodPosition> goodPositions = GoodPositionDal.RootListByPriority(gameId, PlayerId);
                var possiblesChrominosVM = new List<PossiblesChrominoVM>();
                foreach (GoodPosition goodPosition in goodPositions)
                    possiblesChrominosVM.Add(new PossiblesChrominoVM(goodPosition, xMin, yMin));
                for (int iChromino = 0; iChromino < possiblesChrominosVM.Count; iChromino++)
                    for (int i = 0; i < 3; i++)
                        possibleSquareIndexes.Add(possiblesChrominosVM[iChromino].IndexesX[i] + possiblesChrominosVM[iChromino].IndexesY[i] * columnsNumber);
            }
            return new JsonResult(new { indexes = possibleSquareIndexes });
        }

        /// <summary>
        /// Page de partie non trouvée ou non autorisée
        /// </summary>
        /// <returns></returns>
        public IActionResult GameNotFound()
        {
            return View();
        }

        /// <summary>
        /// Cloture de la partie
        /// </summary>
        /// <param name="gameId">id du jeu</param>
        /// <returns></returns>
        [HttpPost]
        public JsonResult End(int gameId)
        {
            if (GameDal.IsFinished(gameId) && !GamePlayerDal.IsViewFinished(gameId, PlayerId))
            {
                PlayerBI playerBI = new PlayerBI(Ctx, Env, gameId, PlayerId);
                if (GamePlayerDal.GetWon(gameId, PlayerId) != true)
                {
                    playerBI.LooseGame(PlayerId);
                    if (GamePlayerDal.IsAllLoosersViewFinished(gameId))
                        return new JsonResult(new { askRematch = true });
                    return new JsonResult(new { askRematch = false });
                }
                else if (GamePlayerDal.WinnersId(gameId).Count > 1) // draw game
                    return new JsonResult(new { returnOk = playerBI.WinGame(PlayerId, true) }); // todo return null possible ?
                else // only 1 winner
                    return new JsonResult(new { returnOk = playerBI.WinGame(PlayerId) });
            }
            return new JsonResult(new { dejavu = true });
        }

        /// <summary>
        /// indique si un joueur a joué et donne les infos 
        /// </summary>
        /// <param name="gameId">id du jeu</param>
        /// <param name="playerTurnId">id du joueur dont c'est le tour à l'appel</param>
        /// <returns></returns>
        [HttpPost]
        public JsonResult WaitOpponentPlayed(int gameId, int playerTurnId)
        {
            int currentPlayerTurnId = playerTurnId;
            while (currentPlayerTurnId == playerTurnId)
            {
                Thread.Sleep(5000);
                currentPlayerTurnId = GamePlayerDal.PlayerTurn(gameId).Id;
            }
            return new JsonResult(new { id = currentPlayerTurnId });
        }


        private void CreateGame(List<Player> players, out int gameId)
        {
            List<Player> randomPlayers = players.RandomSort();
            ChrominoDal.CreateChrominos();
            gameId = GameDal.Add().Id;
            GamePlayerDal.Add(gameId, randomPlayers);
            new GameBI(Ctx, Env, gameId).BeginGame(randomPlayers.Count);
        }

        private List<string> ColorsLastChromino(int gameId, int playerId)
        {
            List<string> lastChrominoColors = new List<string>(3);
            if (ChrominoInHandDal.ChrominosNumber(gameId, playerId) == 1)
            {
                Chromino lastChromino = ChrominoInHandDal.FirstChromino(gameId, playerId);
                lastChrominoColors.Add(lastChromino.FirstColor.ToString());
                lastChrominoColors.Add(lastChromino.SecondColor.ToString());
                lastChrominoColors.Add(lastChromino.ThirdColor.ToString());
            }
            return lastChrominoColors;
        }

        private List<string> ColorsPlayedChromino(int chrominoId)
        {
            Chromino chromino = ChrominoDal.Details(chrominoId);
            if (chromino != null)
                return new List<string> { chromino.FirstColor.ToString(), chromino.SecondColor.ToString(), chromino.ThirdColor.ToString() };
            else
                return new List<string>(0);
        }
    }
}
