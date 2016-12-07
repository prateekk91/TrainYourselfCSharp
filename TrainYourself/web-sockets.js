window.onload = function () {
    var status = document.getElementById("status");
    var msg = document.getElementById("message");
    var canvas = document.getElementById("canvas");
    var buttonColor = document.getElementById("color");
    var buttonDepth = document.getElementById("depth");
    var context = canvas.getContext("2d");
    canvas.width = window.innerWidth;
    canvas.height = window.innerHeight;
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

    function colorDeviatedJoint(jsonObject) {
        var deviatedJointNumber = jsonObject.deviatedJointNumber;
        for (var j = 0; j < jsonObject.trackedBodyJoints.length; j++) {
            var jointNumber = (jsonObject.trackedBodyJoints[j].JointType);
            //            console.log(jointNumber);
            //            console.log("back" + deviatedJointNumber);
            if (jointNumber == deviatedJointNumber) {
                var joint = jsonObject.trackedBodyJoints[j].Position;
                // Draw TrackedBody!!!
                context.fillStyle = "#FF0000";
                context.beginPath();
                context.arc(joint.X, joint.Y, 10, 0, Math.PI * 2, true);
                context.closePath();
                context.fill();
            }
        }
    }

    function multiplyMatrixAndPoint(matrix, point) {

        //Give a simple variable name to each part of the matrix, a column and row number
        var c0r0 = matrix[0], c1r0 = matrix[1], c2r0 = matrix[2], c3r0 = matrix[3];
        var c0r1 = matrix[4], c1r1 = matrix[5], c2r1 = matrix[6], c3r1 = matrix[7];
        var c0r2 = matrix[8], c1r2 = matrix[9], c2r2 = matrix[10], c3r2 = matrix[11];
        var c0r3 = matrix[12], c1r3 = matrix[13], c2r3 = matrix[14], c3r3 = matrix[15];

        //Now set some simple names for the point
        var x = point[0];
        var y = point[1];
        var z = point[2];
        var w = point[3];

        //Multiply the point against each part of the 1st column, then add together
        var resultX = (x * c0r0) + (y * c0r1) + (z * c0r2) + (w * c0r3);

        //Multiply the point against each part of the 2nd column, then add together
        var resultY = (x * c1r0) + (y * c1r1) + (z * c1r2) + (w * c1r3);

        //Multiply the point against each part of the 3rd column, then add together
        var resultZ = (x * c2r0) + (y * c2r1) + (z * c2r2) + (w * c2r3);

        //Multiply the point against each part of the 4th column, then add together
        var resultW = (x * c3r0) + (y * c3r1) + (z * c3r2) + (w * c3r3);

        return [resultX, resultY, resultZ, resultW]
    }

    // Rotate for side view
    function rotateSkeleton(bodyJoints) {
        xAngle = 0.78;
        yAngle = 0.6;
        pivotIndex = 16 // Joint index
        var xRotationMatrix = [
                              1, 0, 0, 0,
                              0, Math.cos(xAngle), Math.sin(xAngle), 0,
                              0, -Math.sin(xAngle), Math.cos(xAngle), 0,
                              0, 0, 0, 1
        ];

        var yRotationMatrix = [
                              Math.cos(yAngle), 0, -Math.sin(yAngle), 0,
                              0, 1, 0, 0,
                              Math.sin(yAngle), 0, Math.cos(yAngle), 0,
                              0, 0, 0, 1
        ];

        var rotatedJoints = bodyJoints;
        for (var i = 0; i < bodyJoints.length; i++) {
            var point = [bodyJoints[i].Position.X - bodyJoints[pivotIndex].Position.X,
                        bodyJoints[i].Position.Y - bodyJoints[pivotIndex].Position.Y,
                        bodyJoints[i].Position.Z - bodyJoints[pivotIndex].Position.Z,
                        1];
            rotatedPoint = multiplyMatrixAndPoint(xRotationMatrix, point);
            rotatedPoint = multiplyMatrixAndPoint(yRotationMatrix, rotatedPoint);
            rotatedJoints[i].Position.X = rotatedPoint[0];
            rotatedJoints[i].Position.Y = rotatedPoint[1];
            rotatedJoints[i].Position.Z = rotatedPoint[2];
        }

        for (var i = 0; i < rotatedJoints.length; i++) {
            rotatedJoints[i].Position.X += rotatedJoints[pivotIndex].Position.X;
            rotatedJoints[i].Position.Y += rotatedJoints[pivotIndex].Position.Y;
            rotatedJoints[i].Position.Z += rotatedJoints[pivotIndex].Position.Z;
        }

        return rotatedJoints;
    }

    // Receive data FROM the server!
    socket.onmessage = function (event) {
        if (typeof event.data === "string") {
            //msg.innerHTML = event.data;
            // SKELETON DATA

            //Ideal and Tracked body parsing and display
            // Get the data in JSON format.
            try {
                var jsonObject = JSON.parse(event.data);
                if ('deviatedJointName' in jsonObject) {
                    msg.innerHTML = jsonObject.deviatedJointName + " not in position";
                }
                else {
                    msg.innerHTML = "Hold the posture";
                }
            }
            catch (e) {
                msg.innerHTML = event.data;
                context.clearRect(0, 0, canvas.width, canvas.height);
                return;
            }

            /*
            {joints: idealBodyJoints:[[{JointType:0, Position:{X,Y,Z}, TrackingState:0}.....15 joints]] trackedBodyJoints:[[{JointType:0, Position:{X,Y,Z}, TrackingState:0}.....15 joints]]}

            */
            // Display the skeleton joints.

            if ('idealBodyJoints' in jsonObject) {
                refreshIdealBody = true;
            } else {
                refreshIdealBody = false;
            }

            if (refreshIdealBody) {
                context.clearRect(0, 0, canvas.width, canvas.height);
            } else {
                context.clearRect(canvas.width / 2, 0, canvas.width / 2, canvas.height);
            }
            console.log(jsonObject);

            var scaleRatio = jsonObject.scaleRatio;
            //            rotateSkeleton(jsonObject.idealBodyJoints);
            //            rotateSkeleton(jsonObject.trackedBodyJoints);
            console.log(jsonObject);


            if (refreshIdealBody) {
                for (var i = 0; i < jsonObject.idealBodyJoints.length; i++) {
                    jsonObject.idealBodyJoints[i].Position.X = jsonObject.idealBodyJoints[i].Position.X + 200;
                    jsonObject.idealBodyJoints[i].Position.Y = jsonObject.idealBodyJoints[i].Position.Y + 200;
                }
            }

            var translateFootX = 0;
            var translateFootY = 0;

            if (jsonObject.trackedBodyJoints[13].Position.Y > jsonObject.trackedBodyJoints[14].Position.Y) {
                var footPositionX = jsonObject.trackedBodyJoints[13].Position.X;
                var footPositionY = jsonObject.trackedBodyJoints[13].Position.Y;
                var newFootPositionX = jsonObject.trackedBodyJoints[13].Position.X * scaleRatio;
                var newFootPositionY = jsonObject.trackedBodyJoints[13].Position.Y * scaleRatio;
            } else {
                var footPositionX = jsonObject.trackedBodyJoints[14].Position.X;
                var footPositionY = jsonObject.trackedBodyJoints[14].Position.Y;
                var newFootPositionX = jsonObject.trackedBodyJoints[14].Position.X * scaleRatio;
                var newFootPositionY = jsonObject.trackedBodyJoints[14].Position.Y * scaleRatio;
            }

            translateFootX = footPositionX - newFootPositionX;
            translateFootY = footPositionY - newFootPositionY;
            
            for (var i = 0; i < jsonObject.trackedBodyJoints.length; i++) {
                jsonObject.trackedBodyJoints[i].Position.X = jsonObject.trackedBodyJoints[i].Position.X * scaleRatio + translateFootX + 200 + 700;
                jsonObject.trackedBodyJoints[i].Position.Y = jsonObject.trackedBodyJoints[i].Position.Y * scaleRatio + translateFootY + 200;
            }

            if (refreshIdealBody) {
                for (var i = 0; i < jsonObject.idealBodyJoints.length; i++) {
                    var joint = jsonObject.idealBodyJoints[i].Position;

                    // Draw IdealBody!!!
                    context.fillStyle = "#00FF00";
                    context.beginPath();
                    context.arc(joint.X, joint.Y, 10, 0, Math.PI * 2, true);
                    context.closePath();
                    context.fill();
                }
            }

            for (var j = 0; j < jsonObject.trackedBodyJoints.length; j++) {
                var joint = jsonObject.trackedBodyJoints[j].Position;
                // Draw TrackedBody!!!
                context.fillStyle = "#00FF00";
                context.beginPath();
                context.arc(joint.X, joint.Y, 10, 0, Math.PI * 2, true);
                context.closePath();
                context.fill();
            }

            if ('deviatedJointNumber' in jsonObject) {
                colorDeviatedJoint(jsonObject);
            }

            if (refreshIdealBody) {
                drawLines(jsonObject.idealBodyJoints);
            }
            drawLines(jsonObject.trackedBodyJoints);
        }
    };

    //    buttonColor.onclick = function () {
    //        console.log('from onclick');
    //        socket.send("exercise1");
    //    }
    //
    //    buttonDepth.onclick = function () {
    //        socket.send("Depth");
    //    }
};