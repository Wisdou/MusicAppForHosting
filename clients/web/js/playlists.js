﻿document.addEventListener("DOMContentLoaded",  async () => {
    const user = await isAuthenticated();

    document.getElementById('addPlaylistForm').addEventListener('submit', async function (event) {
        event.preventDefault();

        const formData = new FormData(this);

        try {
            const response = await fetch(`${apiBaseUrl}/playlists`, {
                body: JSON.stringify({
                    title: formData.get('title'),
                }),
                headers: { 'content-type': 'application/json' },
                method: "POST",
                credentials: "include",
            });

            if (response.status === 200) {
                window.location.reload();
            } else {
                document.getElementById('errorMessage').textContent = await response.text();
            }
        }
        catch (e) {
            console.error(e);
        }

    });

    await loadPlaylists();
});

async function loadPlaylists() {
    try {
        const response = await fetch(`${apiBaseUrl}/playlists`, {
            method: "GET",
            credentials: "include",
        });

        if (response.status === 200) {
            const playlists = await response.json();

            const playlistsList = document.getElementById("playlists-list");
            playlistsList.innerHTML = "";

            playlists.forEach(playlist => {
                const li = document.createElement("li");
                const coverUrl = "https://cdn-icons-png.flaticon.com/512/595/595067.png";

                li.innerHTML = `
            <div class="track-item-right">
                    <img src="${ coverUrl }" alt="cover" class="track-cover">
                    <div class="track-info">
                        <span><b>${ playlist.title }</b></span>
                        <span>Количество треков: ${ playlist.tracksCount }</span>
                    </div>
             </div>
             <div class="track-item-left">
             <span class="delete-icon" onclick="onDeletePlaylist('${playlist.id}')">
             <svg xmlns="http://www.w3.org/2000/svg" width="20" height="20" viewBox="0 0 128 128">
                  <path d="M 49 1 C 47.34 1 46 2.34 46 4 C 46 5.66 47.34 7 49 7 L 79 7 C 80.66 7 82 5.66 82 4 C 82 2.34 80.66 1 79 1 L 49 1 z M 24 15 C 16.83 15 11 20.83 11 28 C 11 35.17 16.83 41 24 41 L 101 41 L 101 104 C 101 113.37 93.37 121 84 121 L 44 121 C 34.63 121 27 113.37 27 104 L 27 52 C 27 50.34 25.66 49 24 49 C 22.34 49 21 50.34 21 52 L 21 104 C 21 116.68 31.32 127 44 127 L 84 127 C 96.68 127 107 116.68 107 104 L 107 40.640625 C 112.72 39.280625 117 34.14 117 28 C 117 20.83 111.17 15 104 15 L 24 15 z M 24 21 L 104 21 C 107.86 21 111 24.14 111 28 C 111 31.86 107.86 35 104 35 L 24 35 C 20.14 35 17 31.86 17 28 C 17 24.14 20.14 21 24 21 z M 50 55 C 48.34 55 47 56.34 47 58 L 47 104 C 47 105.66 48.34 107 50 107 C 51.66 107 53 105.66 53 104 L 53 58 C 53 56.34 51.66 55 50 55 z M 78 55 C 76.34 55 75 56.34 75 58 L 75 104 C 75 105.66 76.34 107 78 107 C 79.66 107 81 105.66 81 104 L 81 58 C 81 56.34 79.66 55 78 55 z"></path>
               </svg>
            </span>
             </div>
             `;

                li.style.cursor = "pointer";
                li.ondblclick = function () {
                  window.location = `playlist-details.html?playlistId=${playlist.id}`;
                };

                playlistsList.appendChild(li);
            });

        }
        else {
            alert(await response.text());
        }
    }
    catch (error) {
        console.error(error);
    }
}

async function onDeletePlaylist(playlistId) {
    try {
        const response = await fetch(`${apiBaseUrl}/playlists/${playlistId}`, {
            method: "DELETE",
            credentials: "include",
        });

        if (response.status === 200) {
            window.location.reload();
        }
        else {
            alert(await response.text());
        }
    }
    catch (error) {
        console.error(error);
    }
}

function onClosePlaylistModal() {
    document.getElementById("addPlaylistModal").style.display = "none";
}

function onAddPlaylistModalWindow() {
    document.getElementById("addPlaylistModal").style.display = "block";
}