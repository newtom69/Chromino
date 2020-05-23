﻿using ChrominoBI;
using Core;
using Data.DAL;
using Data.Enumeration;
using Data.Models;
using Data.ViewModel;
using Microsoft.AspNetCore.Hosting;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using Tool;

namespace Data.Core
{
    public partial class GameBI
    {
        private readonly IWebHostEnvironment Env;
        /// <summary>
        /// DbContext du jeu
        /// </summary>
        private Context Ctx { get; }
        private int GameId { get; set; }
        public List<Player> Players { get; set; }

        /// <summary>
        /// List des GamePlayer du jeu (jointure entre Game et Player)
        /// </summary>
        public List<GamePlayer> GamePlayers { get; set; }
        private GoodPositionBI GoodPositionBI { get; set; }
        private ChrominoInGameDal ChrominoInGameDal { get; }
        private ChrominoInHandDal ChrominoInHandDal { get; }
        private ChrominoDal ChrominoDal { get; }
        private SquareDal SquareDal { get; }
        private PlayerDal PlayerDal { get; }
        private GamePlayerDal GamePlayerDal { get; }
        private GameDal GameDal { get; }
        private GoodPositionDal GoodPositionDal { get; }
        private TipDal TipDal { get; }
        private PlayErrorDal PlayErrorDal { get; }

        public GameBI(Context ctx, IWebHostEnvironment env, int gameId)
        {
            Env = env;
            Ctx = ctx;
            GameId = gameId;
            GameDal = new GameDal(ctx);
            ChrominoInGameDal = new ChrominoInGameDal(ctx);
            ChrominoInHandDal = new ChrominoInHandDal(ctx);
            ChrominoDal = new ChrominoDal(ctx);
            SquareDal = new SquareDal(ctx);
            PlayerDal = new PlayerDal(ctx);
            GamePlayerDal = new GamePlayerDal(ctx);
            GoodPositionDal = new GoodPositionDal(ctx);
            TipDal = new TipDal(ctx);
            PlayErrorDal = new PlayErrorDal(ctx);
            GoodPositionBI = new GoodPositionBI(ctx, GameId);
            Players = GamePlayerDal.Players(gameId);
            GamePlayers = new List<GamePlayer>();
            foreach (Player player in Players)
                GamePlayers.Add(GamePlayerDal.Details(gameId, player.Id));
        }

        /// <summary>
        /// Commence une partie
        /// </summary>
        /// <param name="playerNumber"></param>
        public void BeginGame(int playerNumber)
        {
            if (playerNumber == 1)
                GameDal.SetStatus(GameId, GameStatus.SingleInProgress);
            else
                GameDal.SetStatus(GameId, GameStatus.InProgress);
            ChrominoInGame chrominoInGame = ChrominoInGameDal.FirstToGame(GameId);
            PlayerBI playerBI = new PlayerBI(Ctx, Env, GameId, 0);
            playerBI.Play(chrominoInGame);

            foreach (GamePlayer currentGamePlayer in GamePlayers)
                FillHand(currentGamePlayer);
            GoodPositionBI.UpdateAllPlayersWholeGame();

            new PictureFactoryTool(GameId, Path.Combine(Env.WebRootPath, "image/game"), Ctx).MakeThumbnail();
            playerBI.ChangePlayerTurn();
        }

        public GameVM GameVM(int playerId, bool isAdmin)
        {
            Player playerTurn = GamePlayerDal.PlayerTurn(GameId);
            List<Player> players = GamePlayerDal.Players(GameId);
            if (isAdmin || players.Where(x => x.Id == playerId).FirstOrDefault() != null || GamePlayerDal.IsAllBots(GameId))
            {
                // je joueur est Admin, ou est dans la partie ou c'est une partie entre bots
                Player player = PlayerDal.Details(playerId);
                int chrominosInStackNumber = ChrominoInGameDal.InStack(GameId);
                Dictionary<string, int> pseudosChrominos = new Dictionary<string, int>();
                List<string> pseudos = new List<string>();
                foreach (Player currentPlayer in players)
                {
                    pseudosChrominos.Add(currentPlayer.UserName, ChrominoInHandDal.ChrominosNumber(GameId, currentPlayer.Id));
                    pseudos.Add(currentPlayer.UserName);
                }
                Dictionary<string, int> pseudosIds = new Dictionary<string, int>();

                //todo : pas très propre. revoir construtrion liste PlayerVM {ids, name, chrominosNumber, lastChromino} ?
                foreach (var pseudoChromino in pseudosChrominos)
                    pseudosIds.Add(pseudoChromino.Key, PlayerDal.Details(pseudoChromino.Key).Id);

                List<Chromino> playerChrominos;
                if (!GamePlayerDal.IsPlayerIn(GameId, playerId)) // si le joueur n'est pas dans la partie, il regarde la main du joueur dont c'est le tour de jouer
                    playerChrominos = ChrominoDal.PlayerChrominos(GameId, playerTurn.Id);
                else
                    playerChrominos = ChrominoDal.PlayerChrominos(GameId, playerId);

                if (GameDal.IsFinished(GameId) && !GamePlayerDal.IsViewFinished(GameId, playerId))
                {
                    PlayerBI playerBI = new PlayerBI(Ctx, Env, GameId, playerId);
                    if (GamePlayerDal.GetWon(GameId, playerId) != true)
                        playerBI.LooseGame(playerId);
                    else if (GamePlayerDal.WinnersId(GameId).Count > 1) // draw game
                        playerBI.WinGame(playerId, true);
                    else // only 1 winner
                        playerBI.WinGame(playerId);
                }
                GamePlayer gamePlayer = GamePlayerDal.Details(GameId, playerId);
                List<Square> squares = SquareDal.List(GameId);
                List<ChrominoInGame> chrominosInGamePlayed = ChrominoInGameDal.List(GameId);
                Game game = GameDal.Details(GameId);
                List<Tip> tips = TipDal.ListOn(playerId);
                List<PlayError> playErrors = PlayErrorDal.List();
                GameVM gameVM = new GameVM(game, player, squares, chrominosInStackNumber, pseudosChrominos, pseudosIds, playerChrominos, playerTurn, gamePlayer, chrominosInGamePlayed, pseudos, tips, playErrors);
                return gameVM;
            }
            else return null;
        }

        /// <summary>
        /// marque la partie terminée
        /// </summary>
        public void SetGameFinished()
        {
            if (GamePlayerDal.PlayersNumber(GameId) == 1)
                GameDal.SetStatus(GameId, GameStatus.SingleFinished);
            else
                GameDal.SetStatus(GameId, GameStatus.Finished);
        }

        /// <summary>
        /// rempli la main du ou de tous les joueurs, de chrominos
        /// </summary>
        /// <param name="gamePlayer">le joueur concerné</param>
        private void FillHand(GamePlayer gamePlayer)
        {
            for (int i = 0; i < HandBI.StartChrominosNumber; i++)
                ChrominoInHandDal.FromStack(GameId, gamePlayer.PlayerId);
        }
    }
}