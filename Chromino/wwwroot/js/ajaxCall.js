function TipClosePopup(popup, checkBox) {
    if ($(checkBox).is(":checked")) {
        let dontShowAllTips = false;
        if ($('input[name=dontShowAllTips]').is(':checked'))
            dontShowAllTips = true;
        $.ajax({
            url: '/Tip/Off',
            type: 'POST',
            data: { gameId: GameId, tipId: Tip.id, dontShowAllTips: dontShowAllTips },
            success: function () { CallbackTipClosePopup(dontShowAllTips) },
        });
    }
    ClosePopup(popup);
}

function MemoAdd() {
    let memoContent = $('#MemoContent').val();
    $.ajax({
        url: '/Memo/Add',
        type: 'POST',
        data: { gameId: GameId, memo: memoContent },
        success: function (data) { CallbackMemoAdd(data) },
    });
}

function MemoGet() {
    $.ajax({
        url: '/Memo/Get',
        data: { gameId: GameId },
        success: function (data) { CallbackMemoGet(data) },
    });
}

function ChatAddMessage(recipientId) {
    let type;
    let message;
    let id;
    if (recipientId === undefined) {
        type = 'chatGame';
        id = GameId;
        message = $('#GameMessageInput').val();
    }
    else {
        type = 'private';
        id = recipientId,
            message = $('#PrivateMessageInput').val();
    }
    if (message != "") {
        $.ajax({
            url: '/Chat/PostMessage',
            type: 'POST',
            data: { type: type, id: id, message: message },
            success: function (data) { CallbackChatAddMessage(data, type) },
        });
    }
}

function GetChatMessages(onlyNewMessages, show) {
    $.ajax({
        url: '/Chat/GetMessages',
        data: { gameId: GameId, onlyNewMessages: onlyNewMessages, show: show },
        success: function (data) { CallbackGetChatMessages(data, onlyNewMessages, show); }
    });
}

function GetPrivateMessageMessages(onlyNewMessages, show, opponentId, reset) {
    $.ajax({
        url: '/Chat/GetPrivatesMessages',
        data: { opponentId: opponentId, onlyNewMessages: onlyNewMessages, show: show },
        success: function (data) { CallbackGetPrivateMessageMessages(data, opponentId, show, reset); }
    });
}

function GetNewPrivatesMessagesNumber() {
    return new Promise(function (resolve) {
        $.ajax({
            url: '/Chat/GetNewPrivatesMessagesNumber',
        }).done(function (data) {
            CallbackGetNewPrivatesMessagesNumber(data);
            resolve();
        });
    });
}

function Help() {
    if ($(".Possible").length == 0) {
        $.ajax({
            url: '/Game/Help',
            data: { gameId: GameId },
            success: function (data) { CallbackHelp(data); }
        });
    }
}

function SkipTurn() {
    HideButtonPlayChromino();
    $.ajax({
        url: '/Game/SkipTurn',
        data: { gameId: GameId },
        success: function (data) { CallbackSkipTurn(data); },
    });
}

function DrawChromino() {
    $.ajax({
        url: '/Game/DrawChromino',
        data: { gameId: GameId },
        success: function (data) { CallbackDrawChromino(data); },
    });
}

function End() {
    $.ajax({
        url: '/Game/End',
        data: { gameId: GameId },
        success: function (data) { CallbackEnd(data); },
    });
}

function GetGameInfos() {
    return new Promise(function (resolve) {
        $.ajax({
            url: '/Game/Infos',
            data: { gameId: GameId }
        }).done(function (data) {
            CallbackGameInfos(data);
            resolve();
        });
    });
}

function GetInfosAfterPlaying() {
    return new Promise(function (resolve) {
        $.ajax({
            url: '/Game/InfosAfterPlaying',
            data: { gameId: GameId, playerId: PlayerTurn.id }
        }).done(function (data) {
            resolve(data);
        });
    });

}

function PlayingBot(botId) {
    if (!IsGameFinish) {
        let infoBotPlaying = Players.find(p => p.id == botId).name + " joue";
        $('#InfoGame').html(infoBotPlaying).fadeIn();
        ShowWorkInProgress();
        $.ajax({
            url: '/Game/PlayBot',
            data: { id: GameId, botId: botId },
            success: function (data) { CallbackBotPlayed(data, botId); },
        });
    }
}

function PlayingChromino() {
    ShowInfoPopup = false;
    HideButtonPlayChromino();
    if (!IsGameFinish && LastChrominoMove != null) {
        IsPlayingBackEnd = true;
        StopDraggable(LastChrominoMove);
        ShowWorkInProgress();
        let offset = $(LastChrominoMove).offset();
        let xIndex = Math.round((offset.left - GameAreaOffsetX) / SquareSize);
        let yIndex = Math.round((offset.top - GameAreaOffsetY) / SquareSize);
        let chrominoId = LastChrominoMove.id;
        let orientation;
        let flip;
        switch (GetAngle(LastChrominoMove)) {
            case 0:
                orientation = Orientation.horizontal;
                flip = false;
                break;
            case 90:
                orientation = Orientation.vertical;
                flip = false;
                yIndex--;
                break;
            case 180:
                orientation = Orientation.horizontal;
                flip = true;
                break;
            case 270:
                orientation = Orientation.vertical;
                flip = true;
                yIndex--;
                break;
            default:
                break;
        }
        let x = xIndex + XMin;
        let y = yIndex + YMin;
        $.ajax({
            url: '/Game/Play',
            type: 'POST',
            data: { gameId: GameId, chrominoId: chrominoId, x: x, y: y, orientation: orientation, flip: flip },
            success: function (data) { CallbackPlayChromino(data, chrominoId, xIndex, yIndex, orientation, flip); },
        });
    }
}

function GetGuids() {
    $.ajax({
        url: '/Games/Guids',
        success: function (data) { CallbackGetGuids(data); },
    });
}

function AgainstFriends() {
    $.ajax({
        url: '/Games/AgainstFriendsAjax',
        success: function (data) { CallbackAgainstFriends(data); }
    });
}

//function SendersIdUnreadMessagesNumber() {
//    $.ajax({
//        url: '/Social/GetSendersIdUnreadMessagesNumber',
//        success: function (data) { CallbackSendersIdUnreadMessagesNumber(data) },
//    });
//}

function GetContactsIdNames() {
    return new Promise(function (resolve) {
        $.ajax({
            url: '/Player/ContactsIdsNames'
        }).done(function (data) {
            CallbackGetContactsIdNames(data);
            resolve();
        });
    });
}