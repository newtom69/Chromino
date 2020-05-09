﻿function CallbackGameInfos(data) {
    OpponentsAllBots = data.opponentsAllBots;
    Orientation = { horizontal: data.horizontal, vertical: data.vertical };
    Players = data.playersInfos;
    XMin = data.gameVM.xMin;
    YMin = data.gameVM.yMin;
    Guid = data.gameVM.game.guid;
    GameAreaLinesNumber = data.gameVM.linesNumber;
    GameAreaColumnsNumber = data.gameVM.columnsNumber;
    InStack = data.gameVM.chrominosInStack;
    PlayerId = data.gameVM.player.id;
    PlayerTurn = { id: data.gameVM.playerTurn.id, name: data.gameVM.playerTurn.userName, isBot: data.gameVM.playerTurn.bot };
    PlayersNumber = data.gameVM.pseudos.length;
    HelpNumber = data.gameVM.player.help;
    IsGameFinish = data.gameVM.isGameFinish;
    HaveDrew = data.gameVM.haveDrew;
    MemosNumber = data.gameVM.memosNumber;
    data.gameVM.playErrors.forEach(pe => PlayErrors.push({ name: pe.name, description: pe.description, illustrationPictureClass: pe.illustrationPictureClass, illustrationPictureCaption: pe.illustrationPictureCaption }));
    data.gameVM.tips.forEach(tip => Tips.push({ id: tip.id, elementId: tip.domElementId, headPictureClass: tip.headPictureClass, description: tip.description, illustrationPictureClass: tip.illustrationPictureClass }));
    let move = data.gameVM.game.move - 1;
    for (let icp = 0; icp < data.gameVM.chrominosPlayedVM.length; icp++) {
        let currentPseudo = data.gameVM.pseudos[(move - 1) % data.gameVM.pseudos.length];
        if (data.gameVM.chrominosPlayedVM[icp].chrominoId == null) {
            let playerPass = currentPseudo == "Vous" ? "Vous avez passé" : `${currentPseudo} a passé`;
            HistoryChrominos.push({ infoPlayerPlay: playerPass, square0: "Na", square1: "Na", square2: "Na" });
        }
        else {
            let playerPlay = currentPseudo == "Vous" ? "Vous avez posé" : `${currentPseudo} a posé`;
            let squares = new Array;
            for (let i = 0; i < 3; i++)
                squares.push("Square_" + (data.gameVM.chrominosPlayedVM[icp].indexesX[i] + data.gameVM.chrominosPlayedVM[icp].indexesY[i] * data.gameVM.columnsNumber));
            HistoryChrominos.push({ infoPlayerPlay: playerPlay, square0: squares[0], square1: squares[1], square2: squares[2] });
        }
        move--;
        if (move == 0)
            break;
    }
}

function CallbackChatGetMessages(data, newMessages) {
    if (!newMessages)
        $('#ChatPopupContent').val($('#ChatPopupContent').val() + data.chat);
    if (data.newMessagesNumber > 0) {
        $('#NotifChat').text(data.newMessagesNumber);
        $('#NotifChat').show();
    }
    else {
        $('#NotifChat').hide();
    }
}

function CallbackChatReadMessages(data) {
    $('#ChatPopupContent').val($('#ChatPopupContent').val() + data.chat);
}

function CallbackAddMessage(data) {
    $('#ChatPopupContent').val($('#ChatPopupContent').val() + data.newMessage);
    $('#ChatInput').val("");
    SendMessageSent(Guid);
}

function CallbackTipClosePopup(dontShowAllTips) {
    if (dontShowAllTips)
        Tips = new Array;
    else
        Tips.splice(Tips.findIndex(x => x.id == Tip.id), 1);
}

function CallbackAddMemo(data) {
    ClosePopup("#PopupMemo");
    if (data.memosNumber != 0) {
        $('#NotifMemo').text(data.memosNumber);
        $('#NotifMemo').show();
    }
    else {
        $('#NotifMemo').text(0);
        $('#NotifMemo').hide();
    }
}

function CallbackHelp(data) {
    if (data.indexes.length > 0) {
        HelpIndexes = data.indexes;
        HelpNumber--;
    }
    else {
        $('#InfoGame').html("Pas d'emplacements possibles").fadeIn().delay(1000).fadeOut();
    }
    RefreshHelp();
}

function CallbackDrawChromino(data) {
    if (data.id === undefined) {
        ErrorReturn(data.errorReturn);
    }
    else {
        HaveDrew = true;
        ShowInfoPopup = false;
        AddChrominoInHand(data);
        DecreaseInStack();
        UpdateInHandNumber(PlayerId, 1);
        StopDraggable();
        StartDraggable();
        if (!OpponentsAllBots && Players.length > 1)
            SendChrominoDrawn();
        RefreshDom();
    }
}

function CallbackSkipTurn(data) {
    if (data.id === undefined) {
        ErrorReturn(data.errorReturn);
    }
    else {
        RefreshButtonNextGame();
        AddHistorySkipTurn("Vous avez passé");
        if (!OpponentsAllBots && Players.length > 1)
            SendTurnSkipped();
        RefreshVar(data);
        RefreshDom();
    }
}

function CallbackPlayChromino(data, chrominoId, xIndex, yIndex, orientation, flip) {
    IsPlayingBackEnd = false;
    ShowWorkIsFinish();
    if (data.nextPlayerId === undefined) {
        ErrorReturn(data.errorReturn);
    }
    else {
        HideButtonPlayChromino();
        AddChrominoInGame({ xIndex: xIndex, yIndex: yIndex, orientation: orientation, flip: flip, colors: data.colors }, "Vous avez posé");
        RemoveChrominoInHand(chrominoId);
        UpdateInHandNumber(PlayerId, -1, data.lastChrominoColors);
        if (!OpponentsAllBots && Players.length > 1)
            SendChrominoPlayed({ xIndex: xIndex, yIndex: yIndex, orientation: orientation, flip: flip, colors: data.colors });
        RefreshVar(data);
        RefreshDom();
    }
}

function CallbackBotPlayed(data, botId) {
    $('#InfoGame').fadeOut();
    let infoBotPlay = Players.find(p => p.id == botId).name;
    if (data.draw) {
        DecreaseInStack();
        UpdateInHandNumber(botId, 1);
        infoBotPlay += " a pioché et"
    }
    if (data.skip) {
        AddHistorySkipTurn(infoBotPlay + " a passé");
        if (!OpponentsAllBots && Players.length > 1)
            SendBotTurnSkipped(data.draw);
    }
    else {
        let xIndex = data.x - XMin;
        let yIndex = data.y - YMin;
        AddChrominoInGame({ xIndex: xIndex, yIndex: yIndex, orientation: data.orientation, flip: data.flip, colors: data.colors }, infoBotPlay + " a posé");
        UpdateInHandNumber(botId, -1, data.lastChrominoColors);
        if (!OpponentsAllBots && Players.length > 1)
            SendBotChrominoPlayed({ xIndex: xIndex, yIndex: yIndex, orientation: data.orientation, flip: data.flip, colors: data.colors }, data.draw);
    }
    ShowWorkIsFinish();
    RefreshVar(data);
    RefreshDom(true);
}

function CallbackEnd(data) {
    let toAdd = '';
    Players.forEach(e => toAdd += `<input name="playersName" value="${e.name}" />`);
    $(toAdd).appendTo('#FormRematch');
    if (OpponentsAllBots) {
        $("#Askrematch-text").html("Rejouer ?");
        $("#Askrematch").show();
    }
    else if (data.askRematch) {
        $("#Askrematch-text").html("Prendre votre revanche ?");
        $("#Askrematch").show();
    }
}

function DecreaseInStack() {
    InStack--;
    RefreshInStack();
}

function UpdateInHandNumber(playerId, value, lastChrominoColors = undefined) {
    let player = Players.find(p => p.id == playerId);
    player.chrominosNumber += value;
    if (player.chrominosNumber <= 1) {
        player.lastChrominoColors = lastChrominoColors;
        if (player.id != PlayerId)
            ShowInfoPopup = true;
    }
    UpdateInHandNumberDom(player);
}

function RefreshVar(data) {
    if (data !== undefined) {
        if (data.finish !== undefined) IsGameFinish = data.finish;
        ChangePlayerTurn();
    }
    if (PlayerTurn.id != PlayerId)
        HaveDrew = false;
    if (IsGameFinish) {
        ShowInfoPopup = true;
        End();
    }
}

function ChangePlayerTurn() {
    let index = Players.findIndex(p => p.id == PlayerTurn.id);
    let newIndex = index + 1 < Players.length ? index + 1 : 0;
    PlayerTurn.id = Players[newIndex].id;
    PlayerTurn.isBot = Players[newIndex].isBot;
    PlayerTurn.name = Players[newIndex].name;
}