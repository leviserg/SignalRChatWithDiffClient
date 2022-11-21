﻿
function scrollToBottom(elem) {
    elem.scrollTop = elem.scrollHeight;
}

let subscribed = false;

document.querySelector('#name').value = "";

let userName = prompt("Please enter your name:", "Harry Potter");

$(document).ready(function () {
    $("#name").val(userName);
});

const connection = new signalR.HubConnectionBuilder()
    .withUrl("https://localhost:7065/chat/?username=" + userName)
    .withAutomaticReconnect()
    .withHubProtocol(new signalR.protocols.msgpack.MessagePackHubProtocol()) // use with ../msgpack5.min.js + ../signalr-protocol-msgpack.min.js {see Index.cshtml}
    .build();

connection.on('SendClientMessageToChat', (message) => {
    //appendMessage(response_time_format(String(message.CreatedAt)) + " : " + String(message.Caller), String(message.Text), 'black');
    appendMessageToTextArea(response_time_format(message.CreatedAt.toString()) + " : " + message.Caller.toString(), message.Text.toString(), 'black');
});

connection.onclose(error => {
    console.log('Connection closed. ', error)
});

connection.onreconnecting(error => {
    console.log('Connection reconnecting. ', error);
});

connection.onreconnected(connectionId => {
    console.log('Connectin reconnected with id: ', connectionId);
});

function appendMessageToTextArea(sender, message, color) {
    //document.querySelector('#messages-content').insertAdjacentHTML("beforeend", `<div style="color:${color}" class="py-0,my-0">${sender} : ${message}<br></div><br>`);
    document.querySelector('#messages-content').value += sender + ' : ' + message + '\n';
    scrollToBottom(document.querySelector('#messages-content'));
}

async function connect() {
    if (connection.state === 'Disconnected') {
        try {
            await connection.start();
        }
        catch (error) {
            console.log(error);
        }
        if (connection.state === 'Connected') {
            document.querySelector('#conState').textContent = 'Connected';
            document.querySelector('#conState').style.color = 'green';
            document.querySelector('#connectButton').textContent = 'Disconnect';
        }
    } else if (connection.state === 'Connected') {
        await connection.stop();
        document.querySelector('#conState').textContent = 'Disconnected';
        document.querySelector('#conState').style.color = 'red';
        document.querySelector('#connectButton').textContent = 'Connect';
    }
};

async function sendMessage() {
    if (connection.state === 'Connected') {
        let inputText = document.querySelector('#message');
        let message = inputText.value;
        try {
            await connection.send('AddMessageToChat', message);
            let d = new Date();
            appendMessageToTextArea(time_format(d) + ' : Me', message, 'green');
        }
        catch (error) {
            console.log(error);
        }
        document.querySelector('#message').value = '';
    }
}

async function subscribe() {
    if (connection.state === 'Connected') {
        if (subscribed == true) {
            try {
                await connection.invoke("Unsubscribe");
                subscribed = false;
                document.querySelector('#subscribeButton').textContent = 'Subscribe';
            }
            catch (error) {
                console.log(error);
            }
        }
        else {
            try {
                await connection.invoke("Subscribe");
                subscribed = true;
                document.querySelector('#subscribeButton').textContent = 'Unsubscribe';
            }
            catch (error) {
                console.log(error);
            }
        }
    }
}

function time_format(d) {
    hours = format_two_digits(d.getHours());
    minutes = format_two_digits(d.getMinutes());
    seconds = format_two_digits(d.getSeconds());
    return hours + ":" + minutes + ":" + seconds;
}

function format_two_digits(n) {
    return n < 10 ? '0' + n : n;
}

function response_time_format(d) {
    return d.substr(d.indexOf(":") - 2, 8); //.inhours + ":" + minutes + ":" + seconds;
}
