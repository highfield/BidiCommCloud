//vedi: https://docs.microsoft.com/en-us/aspnet/core/tutorials/signalr?view=aspnetcore-5.0&tabs=visual-studio
"use strict";

//handler bottone "Esegui"
const handleExec = async (conn) => {
    const mittente = $("#mitt").val();
    const destinatario = $("#dest").val();
    const nomeServizio = $("#serv").val();

    const a = parseFloat($("#numa").val());
    const b = parseFloat($("#numb").val());
    //const result = a + b;

    const payload = JSON.stringify({ a, b });
    const msg = {
        mittente,
        destinatario,
        nomeServizio,
        payload,
    }
    const resp = await conn.invoke("RequestReply", msg);
    const {
        result,
    } = JSON.parse(resp);

    $("#result").text(result);
}


//handler testo di log in arrivo dal server
const handleLog = (rawText) => {
    const encodedText = rawText
        .replace(/&/g, "&amp;")
        .replace(/</g, "&lt;")
        .replace(/>/g, "&gt;");

    $("<li>").text(encodedText).appendTo("#log");
}


//init app
async function init() {
    const connection = new signalR.HubConnectionBuilder().withUrl("/myhub").build();
    await connection.start();

    connection.on("ReceiveLog", handleLog);

    $("#btnExec").on("click", () => handleExec(connection));
}


$(function () {
    init();
});
