window.onload = function () {
    var status = document.getElementById("status");
    var msg = document.getElementById("message");
    var canvas = document.getElementById("canvas");
    var buttonColor = document.getElementById("color");
    var buttonDepth = document.getElementById("depth");
    var context = canvas.getContext("2d");
    var QueryString = function () {
        // This function is anonymous, is executed immediately and
        // the return value is assigned to QueryString!
        var query_string = {};
        var query = window.location.search.substring(1);
        var vars = query.split("&");
        for (var i = 0; i < vars.length; i++) {
            var pair = vars[i].split("=");
            // If first entry with this name
            if (typeof query_string[pair[0]] === "undefined") {
                query_string[pair[0]] = decodeURIComponent(pair[1]);
                // If second entry with this name
            } else if (typeof query_string[pair[0]] === "string") {
                var arr = [query_string[pair[0]], decodeURIComponent(pair[1])];
                query_string[pair[0]] = arr;
                // If third or later entry with this name
            } else {
                query_string[pair[0]].push(decodeURIComponent(pair[1]));
            }
        }
        return query_string;
    }();

    var exerciseName = QueryString.exerciseName;
    console.log(exerciseName);

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
    var socket = new WebSocket("ws://192.168.0.22:8181");

    // Connection established.
    socket.onopen = function () {
        status.innerHTML = "Connection successful.";
        console.log('sending exercise name');
        socket.send(exerciseName);
    };

    // Connection closed.
    socket.onclose = function () {
        status.innerHTML = "Connection closed.";
    }

    function drawLines(bodyJoints) {
        context.fillStyle = "#0000FF";
        context.lineWidth = 10;
        context.beginPath();
        context.moveTo(bodyJoints[0].Position.X, bodyJoints[0].Position.Y);
        context.lineTo(bodyJoints[1].Position.X, bodyJoints[1].Position.Y);
        context.stroke();
        context.moveTo(bodyJoints[1].Position.X, bodyJoints[1].Position.Y);
        context.lineTo(bodyJoints[2].Position.X, bodyJoints[2].Position.Y);
        context.stroke();
        context.moveTo(bodyJoints[1].Position.X, bodyJoints[1].Position.Y);
        context.lineTo(bodyJoints[3].Position.X, bodyJoints[3].Position.Y);
        context.stroke();
        context.moveTo(bodyJoints[1].Position.X, bodyJoints[1].Position.Y);
        context.lineTo(bodyJoints[4].Position.X, bodyJoints[4].Position.Y);
        context.stroke();
        context.moveTo(bodyJoints[2].Position.X, bodyJoints[2].Position.Y);
        context.lineTo(bodyJoints[6].Position.X, bodyJoints[6].Position.Y);
        context.stroke();
        context.moveTo(bodyJoints[3].Position.X, bodyJoints[3].Position.Y);
        context.lineTo(bodyJoints[5].Position.X, bodyJoints[5].Position.Y);
        context.stroke();
        context.moveTo(bodyJoints[5].Position.X, bodyJoints[5].Position.Y);
        context.lineTo(bodyJoints[7].Position.X, bodyJoints[7].Position.Y);
        context.stroke();
        context.moveTo(bodyJoints[6].Position.X, bodyJoints[6].Position.Y);
        context.lineTo(bodyJoints[8].Position.X, bodyJoints[8].Position.Y);
        context.stroke();
        context.moveTo(bodyJoints[11].Position.X, bodyJoints[11].Position.Y);
        context.lineTo(bodyJoints[13].Position.X, bodyJoints[13].Position.Y);
        context.stroke();
        context.moveTo(bodyJoints[12].Position.X, bodyJoints[12].Position.Y);
        context.lineTo(bodyJoints[14].Position.X, bodyJoints[14].Position.Y);
        context.stroke();
        context.moveTo(bodyJoints[9].Position.X, bodyJoints[9].Position.Y);
        context.lineTo(bodyJoints[11].Position.X, bodyJoints[11].Position.Y);
        context.stroke();
        context.moveTo(bodyJoints[10].Position.X, bodyJoints[10].Position.Y);
        context.lineTo(bodyJoints[12].Position.X, bodyJoints[12].Position.Y);
        context.stroke();

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
                jsonObject.idealBodyJoints[i].Position.X = jsonObject.idealBodyJoints[i].Position.X * 300 + 200;
                jsonObject.idealBodyJoints[i].Position.Y = canvas.height - jsonObject.idealBodyJoints[i].Position.Y * 300 - 200;
            }

            for (var i = 0; i < jsonObject.trackedBodyJoints.length; i++) {
                jsonObject.trackedBodyJoints[i].Position.X = jsonObject.trackedBodyJoints[i].Position.X * 300 + 200 + 700;
                jsonObject.trackedBodyJoints[i].Position.Y = canvas.height - jsonObject.trackedBodyJoints[i].Position.Y * 300 - 200;
            }

            for (var i = 0; i < jsonObject.idealBodyJoints.length; i++) {
                var joint = jsonObject.idealBodyJoints[i].Position;

                // Draw IdealBody!!!
                context.fillStyle = "#FF0000";
                context.beginPath();
                context.arc(joint.X, joint.Y, 10, 0, Math.PI * 2, true);
                context.closePath();
                context.fill();
            }

            for (var j = 0; j < jsonObject.trackedBodyJoints.length; j++) {
                var joint = jsonObject.trackedBodyJoints[j].Position;

                // Draw TrackedBody!!!
                context.fillStyle = "#FF0000";
                context.beginPath();
                context.arc(joint.X, joint.Y, 10, 0, Math.PI * 2, true);
                context.closePath();
                context.fill();
            }
            drawLines(jsonObject.idealBodyJoints);
            drawLines(jsonObject.trackedBodyJoints);
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
        console.log('from onclick');
        socket.send("exercise1");
    }

    buttonDepth.onclick = function () {
        socket.send("Depth");
    }
};
