window.onload = function () {
    var status = document.getElementById("status");
    var msg = document.getElementById("message");
    var canvas = document.getElementById("canvas");
    var buttonColor = document.getElementById("color");
    var buttonDepth = document.getElementById("depth");
    var context = canvas.getContext("2d");

    var camera = new Image();

    camera.onload = function () {
        context.drawImage(camera, 0, 0);
    }

    if (!window.WebSocket) {
        status.innerHTML = "Your browser does not support web sockets!";
        return;
    }

    status.innerHTML = "Connecting to server...";

    // Initialize a new web socket.
    var socket = new WebSocket("ws://localhost:8181");

    // Connection established.
    socket.onopen = function () {
        status.innerHTML = "Connection successful.";
    };

    // Connection closed.
    socket.onclose = function () {
        status.innerHTML = "Connection closed.";
    }

    // Receive data FROM the server!
    socket.onmessage = function (event) {
        if (typeof event.data === "string") {
            msg.innerHTML = event.data;
            // SKELETON DATA
            
            // Get the data in JSON format.
            var jsonObject = JSON.parse(event.data);
            /*
            {joints: [{x,y,z},{x,y,z}.....14 joints] }
            
            */
            // Display the skeleton joints.
            for (var j = 0; j < jsonObject.joints.length; j++) {
                var joint = jsonObject.joints[j];

                // Draw!!!
                context.fillStyle = "#FF0000";
                context.beginPath();
                context.arc(joint.x*1000, joint.y*1000, 2, 0, Math.PI * 2, true);
                context.closePath();
                context.fill();
            }
        }
        else if (event.data instanceof Blob) {
            // RGB FRAME DATA
            // 1. Get the raw data.
            var blob = event.data;

            // 2. Create a new URL for the blob object.
            window.URL = window.URL || window.webkitURL;

            var source = window.URL.createObjectURL(blob);

            // 3. Update the image source.
            camera.src = source;

            // 4. Release the allocated memory.
            window.URL.revokeObjectURL(source);
        }
    };

    buttonColor.onclick = function () {
        socket.send("Color");
    }

    buttonDepth.onclick = function () {
        socket.send("Depth");
    }
};
