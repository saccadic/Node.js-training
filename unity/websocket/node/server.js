var ws = require('websocket.io');
var server = ws.listen(8000,function () {
    console.log("Websocket Server start");
  }
);

server.on("connection",function(socket) {
    socket.on("message",function(data) {
        console.log("message " + data);
        server.clients.forEach(
          function(client) {
            client.send(data);
          }
        );
      }
    );
  }
);