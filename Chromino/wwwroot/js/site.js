﻿$(document).ready(function () {

    $(document).click(function () {
        StopDraggable();
        StartDraggable();
    });

    $(".handPlayerChromino").dblclick(function () {
        PutChromino(this);
    });

    // Action StartNew events
    $('#addPlayer').click(function () {
        AddPlayer();
    });
    $('#removePlayer').click(function () {
        RemovePlayer();
    });

    ResizeGameArea();

    StartDraggable();
});


//***************************************************//
//** gestion déplacements / rotation des chrominos **//
//***************************************************//

jQuery.fn.rotate = function (degrees) {
    $(this).css({ 'transform': 'rotate(' + degrees + 'deg)' });
};

let TimeoutRotate = null;
let ToRotate = true;
let IsPut;

function ScheduleRotate() {
    ToRotate = true;
    clearTimeout(TimeoutRotate);
    TimeoutRotate = setTimeout(function () {
        ToRotate = false;
    }, 120);
}

function StartDraggable() {
    $(".handPlayerChromino")
        .draggableTouch()
        .bind("dragstart", function (event, pos) {
            ScheduleRotate();
            var id = this.id;
            var x = pos.left - GameAreaOffsetX;
            var y = pos.top - GameAreaOffsetY;
            $("#chrominoPosition").html("position " + id + " left : " + x + "top : " + y);
        })
        .bind("dragend", function (event, pos) {
            if (ToRotate) {
                ToRotate = false;
                clearTimeout(TimeoutRotate);
                var chromino = this;
                clearTimeout(TimeoutPut);
                IsPut = false;
                var TimeoutPut = setTimeout(function () {
                    Rotation(chromino);
                }, 300);

            }
            else {
                var id = this.id;
                var x = pos.left - GameAreaOffsetX;
                var y = pos.top - GameAreaOffsetY;
                $("#chrominoPosition").html("position " + id + " left : " + x + "top : " + y);
                clearTimeout(TimeoutRotate);
                ToRotate = false;

            }
        });
}

function Rotation(chromino) {
    if (!IsPut) {
        var transform = $(chromino).css("transform");
        switch (transform) {
            case "none":
            case "matrix(1, 0, 0, 1, 0, 0)": // 0° => 90°
                $(chromino).rotate(90);
                break;
            case "matrix(0, 1, -1, 0, 0, 0)": // 90° => 180°
                $(chromino).rotate(180);
                break;
            case "matrix(-1, 0, 0, -1, 0, 0)": //180° => 270°
                $(chromino).rotate(270);
                break;
            case "matrix(0, -1, 1, 0, 0, 0)": // 270° => 0°
                $(chromino).rotate(0);
                break;
            default:
                break;
        }
    }
}

function StopDraggable() {
    $(this).unbind("mouseup");
    $(this).unbind("mousedown");
    $(document).unbind("mousemove");
    $(".handPlayerChromino").draggableTouch("disable");
}

function PutChromino(chromino) {
    IsPut = true;
    var id = chromino.id;
    var position = $(chromino).position();
    var x = position.left - GameAreaOffsetX;
    var y = position.top - GameAreaOffsetY;
    var xIndex = Math.round(x / SquareSize);
    var yIndex = Math.round(y / SquareSize);

    // pour debug
    $("#chrominoPosition").html(id + "put at position left : " + x + "top : " + y);
    $("#chrominoOnAreaGame").html(id + "put at position left : " + xIndex + "top : " + yIndex);

    $("#FormX").val(xIndex);
    $("#FormY").val(yIndex);
    var transform = $(chromino).css("transform");
    switch (transform) {
        case "none":
        case "matrix(1, 0, 0, 1, 0, 0)": // 0°
            $("#FormOrientation").val(1); // todo : valeurs scalaire à changer en // enum c#
            break;
        case "matrix(0, 1, -1, 0, 0, 0)": // 90°
            $("#FormOrientation").val(4);
            break;
        case "matrix(-1, 0, 0, -1, 0, 0)": //180°
            $("#FormOrientation").val(3);
            break;
        case "matrix(0, -1, 1, 0, 0, 0)": // 270°
            $("#FormOrientation").val(2);
            break;
        default:
            break;
    }
    $("#FormChrominoId").value(); // TODO mettre valeur 

    $("#FormSendMove").submit();
}


//***************************************//
//********* fonctions StartNew  *********//
//***************************************//

function AddPlayer() {
    if ($('#groupPlayer2').is(':hidden'))
        $('#groupPlayer2').show(600);
    else if ($('#groupPlayer3').is(':hidden'))
        $('#groupPlayer3').show(600);
    else if ($('#groupPlayer4').is(':hidden'))
        $('#groupPlayer4').show(600);
    else if ($('#groupPlayer5').is(':hidden'))
        $('#groupPlayer5').show(600);
    else if ($('#groupPlayer6').is(':hidden'))
        $('#groupPlayer6').show(600);
    else if ($('#groupPlayer7').is(':hidden'))
        $('#groupPlayer7').show(600);
    else if ($('#groupPlayer8').is(':hidden'))
        $('#groupPlayer8').show(600);
}
function RemovePlayer() {
    if (!$('#groupPlayer8').is(':hidden')) {
        $('#player8').val('');
        $('#groupPlayer8').hide(600);
    }
    else if (!$('#groupPlayer7').is(':hidden')) {
        $('#player7').val('');
        $('#groupPlayer7').hide(600);
    }
    else if (!$('#groupPlayer6').is(':hidden')) {
        $('#player6').val('');
        $('#groupPlayer6').hide(600);
    }
    else if (!$('#groupPlayer5').is(':hidden')) {
        $('#player5').val('');
        $('#groupPlayer5').hide(600);
    }
    else if (!$('#groupPlayer4').is(':hidden')) {
        $('#player4').val('');
        $('#groupPlayer4').hide(600);
    }
    else if (!$('#groupPlayer3').is(':hidden')) {
        $('#player3').val('');
        $('#groupPlayer3').hide(600);
    }
    else if (!$('#groupPlayer2').is(':hidden')) {
        $('#player2').val('');
        $('#groupPlayer2').hide(600);
    }
}

//***************************************//
//********* fonctions GameArea  *********//
//***************************************//
let GameAreaOffsetX;
let GameAreaOffsetY;
let SquareSize;

function ResizeGameArea() {
    var documentWidth = $(document).width();
    var documentHeight = $(document).height();
    var width = documentWidth;
    var height = documentHeight;
    if (width < height) {
        height = height - 200; //-200 : taille bandeaux
    }
    else {
        width = width - 200;
    }
    SquareSize = Math.min(Math.trunc(Math.min(height / gameAreaLinesNumber, width / gameAreaColumnsNumber)), 30);

    var gameAreaHeight = SquareSize * gameAreaLinesNumber;
    var gameAreaWidth = SquareSize * gameAreaColumnsNumber;
    $('#gameArea').height(gameAreaHeight);
    $('#gameArea').width(gameAreaWidth);

    GameAreaOffsetX = (documentWidth - gameAreaWidth) / 2 - 10; //todo documentWidth pas le bon ; +10 offest inconnu
    GameAreaOffsetY = (documentHeight - gameAreaHeight) / 2 - 10;

    $('.gameLineArea').outerHeight("auto");
    $('.squareOpenRight').outerWidth(SquareSize);
    $('.squareOpenBottom').outerWidth(SquareSize);
    $('.squareOpenLeft').outerWidth(SquareSize);
    $('.squareOpenTop').outerWidth(SquareSize);
    $('.squareOpenTopBotom').outerWidth(SquareSize);
    $('.squareOpenLeftRight').outerWidth(SquareSize);
    $('.squareFreeCloseNone').outerWidth(SquareSize);
    $('.squareFreeCloseTop').outerWidth(SquareSize);
    $('.squareFreeCloseRightTop').outerWidth(SquareSize);
    $('.squareFreeCloseTopBotom').outerWidth(SquareSize);
    $('.squareFreeCloseRightBottomTop').outerWidth(SquareSize);
    $('.squareFreeCloseLeftTop').outerWidth(SquareSize);
    $('.squareFreeCloseLeftRight').outerWidth(SquareSize);
    $('.squareFreeCloseBottomLeftTop').outerWidth(SquareSize);
    $('.squareFreeCloseAll').outerWidth(SquareSize);
    $('.squareFreeCloseRightLeftTop').outerWidth(SquareSize);
    $('.squareFreeCloseRight').outerWidth(SquareSize);
    $('.squareFreeCloseBottom').outerWidth(SquareSize);
    $('.squareFreeCloseRightBottom').outerWidth(SquareSize);
    $('.squareFreeCloseLeft').outerWidth(SquareSize);
    $('.squareFreeCloseBottomLeft').outerWidth(SquareSize);
    $('.squareFreeCloseRightBottomLeft').outerWidth(SquareSize);
    $('.squareFreeCloseRightLeft').outerWidth(SquareSize);
    $('.squareOpenRight').outerHeight(SquareSize);
    $('.squareOpenBottom').outerHeight(SquareSize);
    $('.squareOpenLeft').outerHeight(SquareSize);
    $('.squareOpenTop').outerHeight(SquareSize);
    $('.squareOpenTopBotom').outerHeight(SquareSize);
    $('.squareOpenLeftRight').outerHeight(SquareSize);
    $('.squareFreeCloseNone').outerHeight(SquareSize);
    $('.squareFreeCloseTop').outerHeight(SquareSize);
    $('.squareFreeCloseRightTop').outerHeight(SquareSize);
    $('.squareFreeCloseTopBotom').outerHeight(SquareSize);
    $('.squareFreeCloseRightBottomTop').outerHeight(SquareSize);
    $('.squareFreeCloseLeftTop').outerHeight(SquareSize);
    $('.squareFreeCloseLeftRight').outerHeight(SquareSize);
    $('.squareFreeCloseBottomLeftTop').outerHeight(SquareSize);
    $('.squareFreeCloseAll').outerHeight(SquareSize);
    $('.squareFreeCloseRightLeftTop').outerHeight(SquareSize);
    $('.squareFreeCloseRight').outerHeight(SquareSize);
    $('.squareFreeCloseBottom').outerHeight(SquareSize);
    $('.squareFreeCloseRightBottom').outerHeight(SquareSize);
    $('.squareFreeCloseLeft').outerHeight(SquareSize);
    $('.squareFreeCloseBottomLeft').outerHeight(SquareSize);
    $('.squareFreeCloseRightBottomLeft').outerHeight(SquareSize);
    $('.squareFreeCloseRightLeft').outerHeight(SquareSize);
    $('#gameArea').show();
    $('.gameLineArea').css('display', 'flex');
}
