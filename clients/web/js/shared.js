const apiBaseUrl = "http://217.199.252.13/api";
const defaultPageSize = 1000;

let currentTrackId;
let isCurrentTrackPlaying = false;
let currentTrackPlayPauseButton;

let loadedTracks = [];

let currentTrackIndex;
let repeatTrack = false;
let repeatPlaylist = false;
let shuffleEnabled = false;

document.getElementById("shuffle-playlist-btn").addEventListener("click", () => {
    shuffleEnabled = !shuffleEnabled;
    document.getElementById("shuffle-playlist-btn").classList.toggle("active", shuffleEnabled);
});

document.getElementById("repeat-track-btn").addEventListener("click", () => {
    repeatTrack = !repeatTrack;
    document.getElementById("repeat-track-btn").classList.toggle("active", repeatTrack);
});

document.getElementById("repeat-playlist-btn").addEventListener("click", () => {
    repeatPlaylist = !repeatPlaylist;
    document.getElementById("repeat-playlist-btn").classList.toggle("active", repeatPlaylist);
});

document.addEventListener("DOMContentLoaded", async () => {
   document.getElementById("sign-out-button").addEventListener("click", signOut);

    const audio = document.getElementById("audio");

    audio.addEventListener("pause", () => {
        isCurrentTrackPlaying = false;

        if (currentTrackPlayPauseButton) {
            currentTrackPlayPauseButton.textContent = "▶";
        }
    });

    audio.addEventListener("play", () => {
        isCurrentTrackPlaying = true;

        if (currentTrackPlayPauseButton) {
            currentTrackPlayPauseButton.textContent = "⏸";
        }
    });

    audio.addEventListener("ended", () => {
        if (repeatTrack) {
            audio.currentTime = 0;
            audio.play();
            return;
        }

        if (shuffleEnabled) {
            const randomTrack = loadedTracks[Math.floor(Math.random() * loadedTracks.length)];
            onPlayPauseButtonClick(randomTrack.id, document.getElementById(randomTrack.id));
            return;
        }

        playNextTrack();
    });
});

function playNextTrack() {
    if (currentTrackIndex + 1 < loadedTracks.length) {
        currentTrackIndex++;
        playTrackByIndex(currentTrackIndex);
    } else if (repeatPlaylist) {
        currentTrackIndex = 0;
        playTrackByIndex(currentTrackIndex);
    }
}

function playTrackByIndex(index) {
    const track = loadedTracks[index];

    // const audio = document.getElementById("audio");
    // audio.src = `${ apiBaseUrl }/tracks/${ track.id }/audio`;
    // currentTrackId = track.id;
    //audio.play();

    onPlayPauseButtonClick(track.id, document.getElementById(track.id));
}

function onPlayPauseButtonClick(trackId, playPauseButton) {
    const audio = document.getElementById("audio");

    // console.log(playPauseButton);

    if (trackId === currentTrackId) {
        if (isCurrentTrackPlaying) {
            audio.pause();
            isCurrentTrackPlaying = false;
            currentTrackPlayPauseButton.textContent = "▶";
        }
        else {
            audio.play();
            isCurrentTrackPlaying = true;
            currentTrackPlayPauseButton.textContent = "⏸";
        }

        return;
    }

    if (currentTrackPlayPauseButton) {
        currentTrackPlayPauseButton.textContent = "▶";
    }

    currentTrackIndex = loadedTracks.findIndex(t => t.id === trackId);
    currentTrackId = trackId;
    isCurrentTrackPlaying = true;
    currentTrackPlayPauseButton = playPauseButton;

    audio.src = `${ apiBaseUrl }/tracks/${ trackId }/audio`;
    audio.load();
    audio.play();
    currentTrackPlayPauseButton.textContent = "⏸";
}

async function isAuthenticated() {
    try {
        const response = await fetch(`${apiBaseUrl}/auth/me`, {
            method: "GET",
            credentials: "include",
        });

        if (response.status === 401) {
            window.location = "sign-in.html";
        }
        else {
            const data = await response.json();
            const userEmailElement = document.getElementById("user-email");
            userEmailElement.textContent = data.email;
            return data;
        }
    }
    catch (error) {
        console.error(error)
    }
}

async function signOut(){
    try {
        const response = await fetch(`${apiBaseUrl}/auth/sign-out`, {
            method: "POST",
            credentials: "include",
        });

        if (response.status === 200) {
            window.location = "sign-in.html";
        }
        else {
            const data = await response.json();
            console.log(data);
        }
    }
    catch (error) {
        console.error(error);
    }
}