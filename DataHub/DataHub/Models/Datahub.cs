using Microsoft.AspNetCore.Hosting;
using Microsoft.AspNetCore.SignalR;
using System.Diagnostics;

namespace DataHub.Models
{
    public class Datahub : Hub
    {


        private static Dictionary<string, List<byte[]>> VideoDataChunks = new Dictionary<string, List<byte[]>>();
        private readonly IWebHostEnvironment _webHostEnvironment;
        private int UserCount = 0;
        public Datahub(IWebHostEnvironment webHostEnvironment)
        {
            _webHostEnvironment = webHostEnvironment;
        }
        private static List<string> ConnectedUsers = new List<string>();

        public void Connect(string userName)
        {
            if (!ConnectedUsers.Contains(userName))
            {
                ConnectedUsers.Add(userName);
            }

            // Senden Sie die gesamte Benutzerliste an alle Clients
            
        }

        public override async Task OnConnectedAsync()
        {
            string id = Context.ConnectionId;
            if (!ConnectedUsers.Contains(id))
            {
                ConnectedUsers.Add(id);
                UserCount++;
            }
            await Clients.All.SendAsync("UpdateUserList", ConnectedUsers);
        }
        public override async Task OnDisconnectedAsync(Exception exception)
        {
            await base.OnDisconnectedAsync(exception);
            string id = Context.ConnectionId;
            if (ConnectedUsers.Contains(id))
            {
                ConnectedUsers.Remove(id);
                UserCount--;
            }
            await Clients.All.SendAsync("UpdateUserList", ConnectedUsers);
        }

        
        public async Task SendVCData(string jsonData)
        {
            await Clients.All.SendAsync("ReceiveVCData", jsonData);
        }
        public async Task StartSimulation()
        {
            await Clients.All.SendAsync("StartSimulation");
        }
        public async Task PauseSimulation()
        {
            await Clients.All.SendAsync("PauseSimulation");
        }
        public async Task SkipForwardSimulation()
        {
            await Clients.All.SendAsync("SkipForwardSimulation");
        }
        public async Task ResetSimulation()
        {
            await Clients.All.SendAsync("ResetSimulation");
        }
        public async Task SimulationSpeed(double simulationspeed)
        {
            await Clients.All.SendAsync("SimulationSpeed", simulationspeed);
        }
        public async Task SendMessage(string json)
        {
            await Clients.All.SendAsync("ReceiveData", json);
        }
        public async Task StartStream()
        {
            await Clients.All.SendAsync("StartStream");
        }
        public async Task StopStream()
        {
            await Clients.All.SendAsync("StopStream");
        }
        public async Task SendStream(byte[] chunk)
        {
            if (!VideoDataChunks.ContainsKey(Context.ConnectionId))
            {
                VideoDataChunks[Context.ConnectionId] = new List<byte[]>();
            }

            VideoDataChunks[Context.ConnectionId].Add(chunk);
            
            await Clients.All.SendAsync("ReceiveStream");
        }
        public async Task FinalizeVideo()
        {
            string webpath = _webHostEnvironment.WebRootPath;
            var allChunks = VideoDataChunks[Context.ConnectionId];

            var fullVideo = allChunks.SelectMany(chunk => chunk).ToArray();

            string videoFileName = "video_temp.mp4"; // Temporärer Name der ursprünglichen Datei
            string videoPath = Path.Combine(webpath, "videos", videoFileName);

            File.WriteAllBytes(videoPath, fullVideo);

            // Hier beginnt die Konvertierung
            string convertedFileName = "video.mp4";
            string convertedVideoPath = Path.Combine(webpath, "convertedvideos", convertedFileName);

            if (ConvertVideo(videoPath, convertedVideoPath))
            {
                File.Delete(videoPath);
                videoFileName = convertedFileName; // Setze videoFileName auf den Namen der konvertierten Datei
            }
            else
            {
                // Fehlerbehandlung für den Fall, dass die Konvertierung fehlschlägt
                Console.WriteLine("Die Videokonvertierung ist fehlgeschlagen.");
            }

            await PauseSimulation();
            VideoDataChunks.Remove(Context.ConnectionId);
        }
        private bool ConvertVideo(string inputPath, string outputPath)
        {
            try
            {
                var processStartInfo = new ProcessStartInfo
                {
                    FileName = "ffmpeg",
                    Arguments = $"-i {inputPath} -c:v libx264 {outputPath}",
                    RedirectStandardOutput = true,
                    UseShellExecute = false,
                    CreateNoWindow = true
                };

                using var process = new Process { StartInfo = processStartInfo };
                process.Start();
                process.WaitForExit();

                return true;
            }
            catch
            {
                return false;
            }
        }

      





        /*
        public async Task FinalizeVideo()
        {
            string webpath = _webHostEnvironment.WebRootPath;
            var allChunks = VideoDataChunks[Context.ConnectionId];

            var fullVideo = allChunks.SelectMany(chunk => chunk).ToArray();

            string videoFileName = "video.mp4"; // Der Name deiner Video-Datei
            string videoPath = Path.Combine(webpath, "videos", videoFileName);

            File.WriteAllBytes(videoPath, fullVideo);

            await PauseSimulation();
            VideoDataChunks.Remove(Context.ConnectionId);
        }
        */
        /*
        private bool ConvertVideo(string inputPath, string outputPath)
        {
            try
            {
                var processStartInfo = new ProcessStartInfo
                {
                    FileName = "ffmpeg",
                    Arguments = $"-i {inputPath} -c:v libx264 {outputPath}",
                    RedirectStandardOutput = true,
                    UseShellExecute = false,
                    CreateNoWindow = true
                };

                using var process = new Process { StartInfo = processStartInfo };
                process.Start();
                process.WaitForExit();

                return true;
            }
            catch
            {
                return false;
            }
        }
        public async Task ReceiveVideo(string fileName, byte[] videoData)
        {
            var tempInputPath = Path.Combine(Path.GetTempPath(), fileName);
            var tempOutputPath = Path.Combine(Path.GetTempPath(), Path.GetFileNameWithoutExtension(fileName) + "_converted.mp4");

            await System.IO.File.WriteAllBytesAsync(tempInputPath, videoData);

            // Konvertiere das Video
            bool success = ConvertVideo(tempInputPath, tempOutputPath);

            if (success)
            {
                await Clients.Caller.SendAsync("VideoConverted", tempOutputPath);
            }
            else
            {
                await Clients.Caller.SendAsync("VideoConversionFailed");
            }

            // Lösche die temporäre Eingabedatei
            System.IO.File.Delete(tempInputPath);
        }
        */
    }
}

/*
const ffmpeg = require('fluent-ffmpeg');
const usersConnected = document.getElementById("users-online");

const mediaSource = new MediaSource();
mediaSource.addEventListener('sourceopen', sourceOpen);

function convertToH264(inputPath, outputPath) {
    ffmpeg(inputPath)
        .toFormat('mp4')
        .videoCodec('libx264')
        .on('end', () => {
            console.log('Konvertierung abgeschlossen.');
        })
        .on('error', (err) => {
            console.error('Fehler:', err);
        })
        .save(outputPath);
}

// Verwendung:
convertToH264('path/to/your/h263video.avi', 'path/to/your/output/h264video.mp4');
function sourceOpen() {
    const sourceBuffer = mediaSource.addSourceBuffer('video/mp4; codecs="avc1.42E01E, mp4a.40.2"');
    sourceBuffer.appendBuffer(chunk);
}

// Verbindung zum SignalR-Hub
const connection = new signalR.HubConnectionBuilder()
    .withUrl("/datahub") // Pfad zum Hub-Endpunkt
    .build();

connection.on("ReceiveVCData", (json) => {
    const jsonElement = document.getElementById("JSON-INPUT");
    const chatArea = document.getElementById("chatArea");
    const messageElement = document.createElement("div");

    messageElement.textContent = `${json}`;
    chatArea.appendChild(messageElement);
});
connection.on("ReceiveData", (json) => {
    const jsonElement = document.getElementById("JSON-INPUT");
    const chatArea = document.getElementById("chatArea");
    const messageElement = document.createElement("div");
    messageElement.classList.add("message");
    messageElement.textContent = `${json}`;
    chatArea.appendChild(messageElement);
});
connection.on("UpdateUserList", (users) => {

    mylist = users; // Aktualisiere die lokale Liste mit der vom Server gesendeten Liste

    const usersListDiv = document.getElementById("users-online");
    usersListDiv.innerHTML = "";

    for (let i = 0; i < mylist.length; i++) {
        const userElement = document.createElement("div");
        userElement.classList.add("message");
        userElement.textContent = `User ${i + 1}: ${mylist[i]}`;
        usersListDiv.appendChild(userElement);
    }

});


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




const showChatButton = document.getElementById("vision");
const chatArea = document.getElementById("chatArea");
showChatButton.addEventListener("click", () => {
    chatArea.style.display = "block";
});


const cleanChatButton = document.getElementById("clear");
cleanChatButton.addEventListener("click", () => {
    chatArea.innerHTML = "";

    chatArea.style.display = "none";
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

// Absende-Button-Event-Handler
const sendButton = document.getElementById("Submit");
sendButton.addEventListener("click", () => {
    const json = document.getElementById("JSON-INPUT").value;
    connection.invoke("SendMessage", json)
        .catch(err => console.error("Error sending message:", err));

});
*/
