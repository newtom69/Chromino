﻿using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Threading.Tasks;
using Controllers;
using Data;
using Data.Core;
using Data.Models;
using Data.ViewModel;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Hosting;
using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Mvc;

namespace ChrominoApp.Controllers
{
    [Authorize]
    public class GamesController : CommonController
    {
        public GamesController(Context context, UserManager<Player> userManager, IWebHostEnvironment env) : base(context, userManager, env)
        {
        }

        /// <summary>
        /// Page des parties à jouer
        /// </summary>
        /// <returns></returns>
        public IActionResult ToPlay()
        {
            GetPlayerInfos();
            return View(MakePicturesGameVM(GamePlayerDal.MultiGamesAgainstAtLeast1HumanToPlay(PlayerId), true));
        }

        /// <summary>
        /// Page des parties à jouer contre uniquement des bots 
        /// </summary>
        /// <returns></returns>
        public IActionResult AgainstBots()
        {
            GetPlayerInfos();
            return View(MakePicturesGameVM(GamePlayerDal.GamesAgainstBotsOnly(PlayerId), true));
        }

        /// <summary>
        /// Page des parties solo en cours
        /// </summary>
        /// <returns></returns>
        public IActionResult Single()
        {
            GetPlayerInfos();
            return View(MakePicturesGameVM(GamePlayerDal.SingleGamesInProgress(PlayerId)));
        }

        /// <summary>
        /// Page des parties en cours (tour d'un adversaire)
        /// </summary>
        /// <returns></returns>
        public IActionResult InProgress()
        {
            GetPlayerInfos();
            return View(MakePicturesGameVM(GamePlayerDal.GamesWaitTurn(PlayerId), true));
        }

        /// <summary>
        /// Page des parties solo terminées
        /// </summary>
        /// <returns></returns>
        public IActionResult SingleFinished()
        {
            GetPlayerInfos();
            return View(MakePicturesGameVM(GamePlayerDal.SingleGamesFinished(PlayerId)));
        }

        /// <summary>
        /// Page des parties gagnées
        /// </summary>
        /// <returns></returns>
        public IActionResult Won()
        {
            GetPlayerInfos();
            return View(MakePicturesGameVM(GamePlayerDal.GamesWon(PlayerId)));
        }

        /// <summary>
        /// Page des parties perdues
        /// </summary>
        /// <returns></returns>
        public IActionResult Lost()
        {
            GetPlayerInfos();
            return View(MakePicturesGameVM(GamePlayerDal.GamesLost(PlayerId)));
        }

        /// <summary>
        /// fabrique la liste de PictureGameVM pour la vue
        /// </summary>
        /// <param name="games">liste des jeux</param>
        /// <returns></returns>
        private List<PictureGameVM> MakePicturesGameVM(List<Game> games, bool keepSuspens = false)
        {
            List<PictureGameVM> listPictureGameVM = new List<PictureGameVM>();
            foreach (Game game in games)
            {
                List<Player> players = GamePlayerDal.Players(game.Id);
                string playerPseudoTurn = GamePlayerDal.PlayerTurn(game.Id).UserName;
                Dictionary<string, int> pseudos_chrominos = new Dictionary<string, int>();
                foreach (Player player in players)
                {
                    int chrominosNumber = GameChrominoDal.PlayerNumberChrominos(game.Id, player.Id);
                    if (keepSuspens && chrominosNumber == 0)
                        chrominosNumber = 1;
                    pseudos_chrominos.Add(player.UserName, chrominosNumber);
                }
                string pictureName = $"{GameDal.Details(game.Id).Guid}.png";
                if (!System.IO.File.Exists(Path.Combine(Env.WebRootPath, @"image/game", pictureName)))
                    new GameCore(Ctx, Env, game.Id).MakeThumbnail();
                listPictureGameVM.Add(new PictureGameVM(game.Id, pictureName, pseudos_chrominos, playerPseudoTurn, game.PlayedDate));
            }
            return listPictureGameVM;
        }
    }
}