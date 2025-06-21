const audioElement = document.getElementById("audio");
const eqWrap = document.getElementById("eq-wrap");
const presetSelect = document.getElementById("preset");

document.addEventListener("DOMContentLoaded", e => {
   document.getElementById("equalizer-btn").addEventListener("click", ev => {
       document.getElementById("equalizerModal").style.display = "block";
       console.log("equalizer clicked");
   });
});

const presets = {
    flat:     [0, 0, 0, 0, 0, 0, 0, 0, 0, 0],
    bass:     [6, 4, 2, 0, -2, -4, -6, -8, -10, -12],
    classical:[-6, -4, -2, 0, 2, 4, 2, 0, -2, -4],
    pop:      [3, 2, 0, -1, 0, 1, 2, 3, 2, 1]
};

const audioCtx = new (window.AudioContext || window.webkitAudioContext)();
const sourceNode = audioCtx.createMediaElementSource(audioElement);

const frequencies = [60, 170, 310, 600, 1000, 3000, 6000, 12000, 14000, 16000];
const filters = frequencies.map(freq => {
    const filter = audioCtx.createBiquadFilter();
    filter.type = "peaking";
    filter.frequency.value = freq;
    filter.Q.value = 1;
    filter.gain.value = 0;
    return filter;
});

sourceNode.connect(filters[0]);
for (let i = 0; i < filters.length - 1; i++) {
    filters[i].connect(filters[i + 1]);
}
filters[filters.length - 1].connect(audioCtx.destination);

const sliders = [];

filters.forEach((filter, i) => {
    const band = document.createElement("div");
    band.className = "eq-band";

    const label = document.createElement("label");
    label.textContent = `${frequencies[i]}Hz`;

    const slider = document.createElement("input");
    slider.type = "range";
    slider.min = -12;
    slider.max = 12;
    slider.value = 0;
    slider.step = 1;

    slider.addEventListener("input", () => {
        filter.gain.value = slider.value;
    });

    band.appendChild(label);
    band.appendChild(slider);
    eqWrap.appendChild(band);

    sliders.push(slider);
});

presetSelect.addEventListener("change", (e) => {
    const preset = presets[e.target.value];
    if (!preset) return;

    preset.forEach((gain, i) => {
        filters[i].gain.value = gain;
        sliders[i].value = gain;
    });
});

audioElement.addEventListener('play', () => {
    if (audioCtx.state === 'suspended') {
        audioCtx.resume();
    }
});

function closeEqualizerModal() {
    document.getElementById("equalizerModal").style.display = "none";
}