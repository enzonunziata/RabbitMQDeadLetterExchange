"use strict";

var connection = new signalR.HubConnectionBuilder().withUrl("/rabbitmqhub").build();

document.getElementById("sendButton").disabled = true;
document.getElementById("sendWithException").disabled = true;

connection.on("QueueReceived", function (context, messageType, body) {
    var li = document.createElement("li");
    document.getElementById("messagesList").prepend(li);

    var title = '';
    switch (context) {
        case 'main-success': title = `<i class="bi bi-envelope-check-fill" style="color:green;"></i> <strong>${messageType}</strong> successfully delivered`; break;
        case 'main-failure': title = `<i class="bi bi-envelope-check-fill" style="color:red;"></i> <strong>${messageType}</strong> was rejected`; break;
        case 'dead-letter': title = `<i class="bi bi-check2-circle"></i> <strong>${messageType}</strong> from dead letter exchange`; break;
    }

    li.innerHTML = `[<small>${new Date().toUTCString()}</small>] &nbsp; ${title}<br/>${body}`;
});

connection.start().then(function () {
    document.getElementById("sendButton").disabled = false;
    document.getElementById("sendWithException").disabled = false;
}).catch(function (err) {
    return console.error(err.toString());
});

var handler = (withException) => {
    return async (event) => {
        var messageType = document.getElementById("messageType").value;
        var comment = document.getElementById("messageComment").value;
        await fetch("/home/sendmessage", {
            method: 'POST',
            headers: {
                'Content-Type': 'application/json'
            },
            body: JSON.stringify({ messageType, comment, withException })
        });
        document.getElementById("messageComment").value = '';
        event.preventDefault();
    };
};

document.getElementById("sendButton").addEventListener("click", handler(false));
document.getElementById("sendWithException").addEventListener("click", handler(true));

document.getElementById("clearList").addEventListener("click", (event) => {
    document.getElementById("messagesList").innerHTML = '';
});