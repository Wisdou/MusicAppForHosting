let addToPlaylistTrackId;

document.addEventListener("DOMContentLoaded", async () => {
    const user = await isAuthenticated();

    await loadTracks(1, defaultPageSize);

    if (user && user.role === "Admin") {
        document.querySelectorAll(".delete-icon").forEach(e => e.style.display = "block");
    }

    document.getElementById("addToPlaylistForm").addEventListener("submit", async (ev) => {
       ev.preventDefault();

        const selectElement = document.getElementById("playlists-select");
        const selectedId = selectElement.options[selectElement.selectedIndex].value;

        console.log("selectedId", selectedId);

        try {
            const response = await fetch(`${apiBaseUrl}/playlists/${selectedId}/tracks/${addToPlaylistTrackId}`, {
                method: "POST",
                credentials: "include",
                headers: { "Content-Type": "application/json" },
            });

            if (response.status === 200) {
                onCloseAddToPlaylistModalWindow();
            }
            else {
                alert(await response.text());
            }
        }
        catch(e) {
            console.error(e);
        }
    });

    document.getElementById('addTrackForm').addEventListener('submit', function (event) {
        event.preventDefault();

        const formData = new FormData(this);

        const jsonPart = JSON.stringify({
            title: formData.get('title'),
            performer: formData.get('performer'),
            genre: formData.get('genre')
        });

        formData.append('JsonPart', jsonPart);

        fetch(`${ apiBaseUrl }/tracks`, {
            method: 'POST',
            credentials: "include",
            body: formData
        })
            .then(response => {
                if (response.ok) {
                    alert('Трек успешно добавлен');
                    window.location.reload();
                } else {
                    return response.text().then(text => {
                        throw new Error(text);
                    });
                }
            })
            .catch(error => {
                document.getElementById('errorMessage').textContent = error.message;
            });
    });


    document.body.addEventListener('dragover', (e) => {
        e.preventDefault();
    });

    document.body.addEventListener('drop', (e) => {
        e.preventDefault();
        console.log("files", e.dataTransfer.files);
        const file = e.dataTransfer.files[0];
        if (file && file.type === 'audio/mpeg') {
            handleMp3Drop(file);
        }
    });
});

function handleMp3Drop(file) {
    jsmediatags.read(file, {
        onSuccess: function(tag) {
            const title = tag.tags.title || file.name.replace(/\.[^/.]+$/, "");
            const artist = tag.tags.artist || 'Неизвестный исполнитель';

            document.getElementById('trackTitleInput').value = title;
            document.getElementById('performerInput').value = artist;
        },
        onError: function(error) {
            console.log('Ошибка чтения тегов: ', error);
            document.getElementById('trackTitleInput').value = file.name.replace(/\.[^/.]+$/, "");
            document.getElementById('performerInput').value = '';
        }
    });

    const audioInput = document.getElementById('audioInput');
    const dataTransfer = new DataTransfer();
    dataTransfer.items.add(file);
    audioInput.files = dataTransfer.files;

    document.getElementById('addTrackModal').style.display = 'block';
}

async function loadTracks(pageIndex, pageSize) {
    try {
        const response = await fetch(`${ apiBaseUrl }/tracks?pageIndex=${ pageIndex }&pageSize=${ pageSize }`, {
            method: "GET",
            credentials: "include",
        });

        if (response.status === 200) {
            const tracks = await response.json();
            loadedTracks = tracks;
            const tracksList = document.getElementById("tracks-list");

            tracksList.innerHTML = "";

            tracks.forEach(track => {
                const li = document.createElement("li");
                const coverUrl = `${ apiBaseUrl }/tracks/${ track.id }/cover`;

                li.innerHTML = `
            <div class="track-item-right">
                    <img src="${ coverUrl }" alt="cover" class="track-cover">
                    <div class="track-info">
                        <span><b>${ track.title }</b></span>
                        <span>${ track.performer }</span>
                    </div>
             </div>
             <div class="track-item-left">
             <span class="add-to-favorites-icon" style="display: ${track.isInFavorites ? 'none' : 'block'}" onclick="addToFavorites('${track.id}')">
               <svg xmlns="http://www.w3.org/2000/svg" width="20" height="20" viewBox="0 0 50 50">
                   <path d="M 13 2 C 12.447 2 12 2.448 12 3 L 12 47 C 12 47.358 12.190953 47.689187 12.501953 47.867188 C 12.812953 48.045187 13.194906 48.043281 13.503906 47.863281 L 25 41.158203 L 36.496094 47.863281 C 36.651094 47.954281 36.826 48 37 48 C 37.172 48 37.344047 47.956187 37.498047 47.867188 C 37.809047 47.689188 38 47.358 38 47 L 38 33.167969 L 36 34.447266 L 36 45.259766 L 25.503906 39.136719 C 25.348906 39.045719 25.174 39 25 39 C 24.826 39 24.651094 39.045719 24.496094 39.136719 L 14 45.259766 L 14 4 L 36 4 L 36 13.205078 L 36.345703 12.298828 L 36.648438 11.996094 C 36.868437 11.776094 37.348953 11.365187 38.001953 11.117188 L 38.001953 3 C 38.001953 2.448 37.554953 2 37.001953 2 L 13 2 z M 39.0625 12.910156 C 38.6625 12.910156 38.2625 13.210156 38.0625 13.410156 L 35.662109 19.710938 L 28.962891 20.111328 C 28.562891 20.111328 28.1625 20.410547 28.0625 20.810547 C 27.9625 21.210547 28.062891 21.710156 28.462891 21.910156 L 33.5625 26.111328 L 31.861328 32.611328 C 31.761328 33.011328 31.961719 33.510937 32.261719 33.710938 C 32.461719 33.810938 32.661328 33.910156 32.861328 33.910156 C 33.062328 33.910156 33.263891 33.810938 33.462891 33.710938 L 39.0625 30.111328 L 44.662109 33.810547 C 44.962109 34.110547 45.461719 34.010547 45.761719 33.810547 C 46.161719 33.510547 46.262109 33.110937 46.162109 32.710938 L 44.462891 26.210938 L 49.662109 22.011719 C 49.962109 21.711719 50.062891 21.310156 49.962891 20.910156 C 49.862891 20.510156 49.4625 20.210938 49.0625 20.210938 L 42.361328 19.810547 L 39.962891 13.511719 C 39.862891 13.211719 39.4625 12.910156 39.0625 12.910156 z M 39.0625 16.710938 L 40.662109 21.210938 C 40.862109 21.510937 41.1625 21.810547 41.5625 21.810547 L 46.363281 22.111328 L 42.662109 25.111328 C 42.362109 25.311328 42.263281 25.711328 42.363281 26.111328 L 43.5625 30.710938 L 39.5625 28.111328 C 39.4625 28.011328 39.2625 27.910156 39.0625 27.910156 C 38.8615 27.910156 38.662891 28.011719 38.462891 28.011719 L 34.462891 30.611328 L 35.662109 26.011719 C 35.762109 25.711719 35.663281 25.211719 35.363281 25.011719 L 31.662109 22.011719 L 36.462891 21.810547 C 36.862891 21.710547 37.163281 21.511328 37.363281 21.111328 L 39.0625 16.710938 z"></path>
            </svg>
            </span>
            <span class="add-to-favorites-icon" onclick="addToPlaylist('${track.id}')">
                <svg xmlns="http://www.w3.org/2000/svg" width="20" height="20" viewBox="0 0 24 24" fill="currentColor">
    <path d="M3 6h14v2H3V6zm0 4h14v2H3v-2zm0 4h8v2H3v-2zm13 0v3h-3v2h3v3h2v-3h3v-2h-3v-3h-2z"/>
  </svg>
            </span>
             <span class="delete-icon">
             <svg xmlns="http://www.w3.org/2000/svg" width="20" height="20" viewBox="0 0 128 128">
                  <path d="M 49 1 C 47.34 1 46 2.34 46 4 C 46 5.66 47.34 7 49 7 L 79 7 C 80.66 7 82 5.66 82 4 C 82 2.34 80.66 1 79 1 L 49 1 z M 24 15 C 16.83 15 11 20.83 11 28 C 11 35.17 16.83 41 24 41 L 101 41 L 101 104 C 101 113.37 93.37 121 84 121 L 44 121 C 34.63 121 27 113.37 27 104 L 27 52 C 27 50.34 25.66 49 24 49 C 22.34 49 21 50.34 21 52 L 21 104 C 21 116.68 31.32 127 44 127 L 84 127 C 96.68 127 107 116.68 107 104 L 107 40.640625 C 112.72 39.280625 117 34.14 117 28 C 117 20.83 111.17 15 104 15 L 24 15 z M 24 21 L 104 21 C 107.86 21 111 24.14 111 28 C 111 31.86 107.86 35 104 35 L 24 35 C 20.14 35 17 31.86 17 28 C 17 24.14 20.14 21 24 21 z M 50 55 C 48.34 55 47 56.34 47 58 L 47 104 C 47 105.66 48.34 107 50 107 C 51.66 107 53 105.66 53 104 L 53 58 C 53 56.34 51.66 55 50 55 z M 78 55 C 76.34 55 75 56.34 75 58 L 75 104 C 75 105.66 76.34 107 78 107 C 79.66 107 81 105.66 81 104 L 81 58 C 81 56.34 79.66 55 78 55 z"></path>
               </svg>
            </span>

                <span id="${track.id}" class="play-pause-icon" onclick="onPlayPauseButtonClick('${track.id}', this)">▶</span>
                <span>${ track.duration }</span>
             </div>
             `;

                li.querySelector(".delete-icon").addEventListener("click", async () => {
                    await deleteTrack(track.id);
                });

                tracksList.appendChild(li);
            });
        } else if (response.status === 401) {
            window.location = "sign-in.html";
        }

    } catch (err) {
        console.error(err);
    }
}

async function addToPlaylist(trackId) {
    addToPlaylistTrackId = trackId;

    try {
        const response = await fetch(`${apiBaseUrl}/playlists`, {
            method: "GET",
            credentials: "include",
        });

        if (response.status === 200) {
            const playlists = await response.json();

            const playlistsSelect = document.getElementById("playlists-select");
            playlistsSelect.innerHTML = "";

            playlists.forEach(p => {
                const option = document.createElement("option");
                option.value = p.id;
                option.text = p.title;

                playlistsSelect.appendChild(option);
            });

        }
        else {
            alert(await response.text());
        }
    }
    catch (error) {
        console.error(error);
    }

    document.getElementById("addToPlaylistModal").style.display = "block";
}

async function addToFavorites(trackId) {
    try {
        const response = await fetch(`${apiBaseUrl}/tracks/${trackId}/favorites`, {
            method: "POST",
            headers: {
                "Content-Type": "application/json",
            },
            credentials: "include",
        });

        if (response.status === 200) {
            alert("Трек успешно добавлен в избранное.");
            window.location.reload();
        }
    }
    catch (error) {
        console.error(error);
    }
}

async function deleteTrack(trackId) {
    try {
        const isConfirmed = confirm("Желаете удалить данный трек?");
        if (!isConfirmed) {
            return;
        }

        const response = await fetch(`${ apiBaseUrl }/tracks/${ trackId }`, {
            method: "DELETE",
            credentials: "include",
        });

        if (response.status === 200) {
            alert("Трек успешно удален!");
            window.location.reload();
        }
    } catch (e) {
        console.error(e);
    }
}

function onCloseAddingModalWindow() {
    document.getElementById("addTrackModal").style.display = "none";
}

function OnAddTrackButtonClick() {
    document.getElementById("addTrackModal").style.display = "block";
}

function onCloseAddToPlaylistModalWindow() {
    document.getElementById("addToPlaylistModal").style.display = "none";
}