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

            //Ideal and Tracked body parsing and display
            // Get the data in JSON format.
            var jsonObject = JSON.parse(event.data);
            /*
            {joints: idealBodyJoints:[[{JointType:0, Position:{X,Y,Z}, TrackingState:0}.....15 joints]] trackedBodyJoints:[[{JointType:0, Position:{X,Y,Z}, TrackingState:0}.....15 joints]]}
            
            */
            // Display the skeleton joints.
            context.clearRect(0, 0, canvas.width, canvas.height);

            for (var i = 0; i < jsonObject.idealBodyJoints.length; i++) {
                var joint = jsonObject.idealBodyJoints[i].Position;

                // Draw IdealBody!!!
                context.fillStyle = "#FF0000";
                context.beginPath();
                context.arc(joint.X * 300 + 200, canvas.height - joint.Y * 300 - 200, 10, 0, Math.PI * 2, true);
                context.closePath();
                context.fill();
            }

            for (var j = 0; j < jsonObject.trackedBodyJoints.length; j++) {
                var joint = jsonObject.trackedBodyJoints[j].Position;

                // Draw TrackedBody!!!
                context.fillStyle = "#FF0000";
                context.beginPath();
                context.arc(joint.X * 300 + 200 + 700, canvas.height - joint.Y * 300 - 200, 10, 0, Math.PI * 2, true);
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
