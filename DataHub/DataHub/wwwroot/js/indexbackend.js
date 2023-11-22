
    const usersConnected = document.getElementById("users-online");
    //var player = videojs('my-video');

    // Verbindung zum SignalR-Hub

    const connection = new signalR.HubConnectionBuilder()
        .withUrl("/datahub") // Pfad zum Hub-Endpunkt
        .build();

    let editor = ace.edit("jsonInput");
    editor.setTheme("ace/theme/monokai");
    editor.session.setMode("ace/mode/json");
    let isUpdateFromServer = false;

    
    editor.session.on("change", () => {

        if (!isUpdateFromServer) {
            const content = editor.getValue();
            connection.invoke("LiveEditorContent", content)
                .catch(err => console.error(err.toString()));
        }

    });

    connection.on("ReceiveChanges", (json) => {

        editor.setValue(json, -1);

    });

    connection.on("ReceiveLiveEditorContent", (content) => {
        const currentPosition = editor.getCursorPosition();

        isUpdateFromServer = true;
        editor.session.setValue(content);

        editor.moveCursorToPosition(currentPosition);
        editor.clearSelection();

        isUpdateFromServer = false;
    });

    connection.on("ReceiveVCData", (json) => {
        /*

        const jsonElement = document.getElementById("JSON-INPUT");
        const chatArea = document.getElementById("chatArea");
        const messageElement = document.createElement("div");

        messageElement.textContent = `${json}`;
        chatArea.appendChild(messageElement);
    
        */

        var json2 = JSON.stringify(json, null, 2);

        if (json2 != oldjson) {

            editor.setValue(json2);
            formatJsonInEditor();
            const oldjson = json2;
        }



    });
    /*
    connection.on("ReceiveData", (json) => {
        const jsonElement = document.getElementById("JSON-INPUT");
        const chatArea = document.getElementById("chatArea");
        const messageElement = document.createElement("div");
        messageElement.classList.add("message");
        messageElement.textContent = `${json}`;
        chatArea.appendChild(messageElement);
    });
    */
    connection.on("UpdateUserList", (users) => {

        mylist = users;

        const usersListDiv = document.getElementById("users-online");
        const currentPosition = editor.getCursorPosition();

        usersListDiv.innerHTML = "";

        for (let i = 0; i < mylist.length; i++) {
            const userElement = document.createElement("div");
            userElement.classList.add("message");
            userElement.textContent = `User ${i + 1}: ${mylist[i]}`;
            usersListDiv.appendChild(userElement);
        }


        const content = editor.getValue();

        //Schreibt nur Werte vom Befüllten Editor rein
        if (content != "") {

            connection.invoke("LiveEditorContent", content)
                .catch(err => console.error(err.toString()));
            editor.moveCursorToPosition(currentPosition);
            editor.clearSelection();

        }




    });

    connection.on("GetChunksAddQuery", (chunk) => {
        appendChunk(chunk);
    });
    /*
    var video = document.querySelector('video');
    var mediaSource = new MediaSource();
    video.src = URL.createObjectURL(mediaSource);

    // Globaler SourceBuffer
    var sourceBuffer;

    mediaSource.addEventListener('sourceopen', function () {
        // Erstellen Sie hier den SourceBuffer mit dem korrekten MIME-Type
        sourceBuffer = mediaSource.addSourceBuffer('video/mp4; codecs="avc1.42E01E"');

        // Listener für das Hinzufügen weiterer Chunks
    
    });

    function appendChunk(chunk) {
        if (mediaSource.readyState === 'open') {
            sourceBuffer.appendBuffer(chunk);
        } 
    }
    */





    let i = 0;
    var mylist = [];
    var arraynumber = 0;


    // Verbindung zum Hub starten
    connection.start()
        .then(() => {
            console.log("SignalR connection started.");
        })
        .catch(err => {
            console.error("Error starting SignalR connection:", err);
        });








    const jsonInput = document.getElementById("JSON-INPUT");

    document.getElementById("startSimulation").addEventListener("click", function () {
        connection.invoke("StartSimulation").catch(err => console.error(err));
    });
    document.getElementById("pauseSimulation").addEventListener("click", function () {
        connection.invoke("PauseSimulation").catch(err => console.error(err));
    });
    document.getElementById("skipforwardsimulation").addEventListener("click", function () {
        connection.invoke("SkipForwardSimulation").catch(err => console.error(err));
    });
    document.getElementById("resetsimulation").addEventListener("click", function () {
        connection.invoke("ResetSimulation").catch(err => console.error(err));
    });
    document.getElementById("finalizevideo").addEventListener("click", function () {
        connection.invoke("FinalizeVideo").catch(err => console.error(err));
    });


    let currentspeedvalue = 1;
    let currentcommand = "None";
    const commandMenu = document.getElementById("command-Input");
    commandMenu.addEventListener("change", function () {
        currentcommand = commandMenu.value;
    });
    const commandButton = document.getElementById("command-Button");
    commandButton.addEventListener("click", function () {
        if (currentcommand != "None") {
            connection.invoke(currentcommand).catch(err => console.error(err));
        }


    });
    const speedMenu = document.getElementById("simulationspeed");
    speedMenu.addEventListener("change", function () {
        currentspeedvalue = parseFloat(speedMenu.value);
    });

    const speedButton = document.getElementById("simulationspeedbutton");
    speedButton.addEventListener("click", function () {
        connection.invoke("SimulationSpeed", currentspeedvalue).catch(err => console.error(err));
    });

    const submitButton = document.getElementById("submitButton1");
    submitButton.addEventListener("click", () => {
        const json = editor.getValue();

        connection.invoke("SubmitChanges", json)
            .catch(err => console.error("Error sending message:", err));

    });


