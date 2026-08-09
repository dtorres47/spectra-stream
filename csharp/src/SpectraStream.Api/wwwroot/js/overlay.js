// ---- Elements ----
const questList = document.getElementById("quests");
const wsStatus = document.getElementById("wsStatus");
const statActive = document.getElementById("stat-active");
const video = document.getElementById("animation-video");

// ---- State: instanceId -> element ----
const questElems = new Map();

// ---- Background video autoplay ----
if (video) {
    const tryPlay = () => video.play().catch(() => { });
    video.readyState >= 2
        ? tryPlay()
        : video.addEventListener("canplay", tryPlay, { once: true });
}

// ---- Rendering ----
function renderQuest(q) {
    if (!q || !q.instanceId) return;
    let el = questElems.get(q.instanceId);
    if (!el) {
        el = document.createElement("div");
        el.className = "quest";
        el.dataset.id = q.instanceId;
        questList.appendChild(el);
        questElems.set(q.instanceId, el);
    }

    const objectives = (q.objectives || [])
        .map(o => `<li>${escapeHtml(o.title)}</li>`)
        .join("");

    el.innerHTML =
        `<span class="quest-title">${escapeHtml(q.title || "Quest")}</span>` +
        `<span class="quest-supporter">from ${escapeHtml(q.supporter || "Anonymous")}</span>` +
        (objectives ? `<ul class="quest-objectives">${objectives}</ul>` : "");

    updateStats();
}

function removeQuest(instanceId) {
    const el = questElems.get(instanceId);
    if (el) { el.remove(); questElems.delete(instanceId); }
    updateStats();
}

function updateStats() {
    if (statActive) statActive.textContent = questElems.size;
}

function escapeHtml(text) {
    const div = document.createElement("div");
    div.textContent = text ?? "";
    return div.innerHTML;
}

// ---- Initial sync: load current queue on page load ----
async function syncQueue() {
    try {
        const queue = await fetch("/api/queue").then(r => r.json());
        // Clear and re-render (handles overlay refresh mid-stream)
        questElems.forEach(el => el.remove());
        questElems.clear();
        queue.forEach(renderQuest);
    } catch (e) {
        console.warn("Queue sync failed:", e);
    }
}

// ---- SignalR connection (the /ws endpoint is a SignalR hub) ----
const connection = new signalR.HubConnectionBuilder()
    .withUrl("/ws")
    .withAutomaticReconnect()
    .build();

connection.on("ReceiveEvent", msg => {
    if (!msg || !msg.type) return;
    switch (msg.type) {
        case "QUEST_UPSERT":
            renderQuest(msg.data);
            break;
        case "QUEST_REMOVE":
            if (msg.data && msg.data.instanceId) removeQuest(msg.data.instanceId);
            break;
    }
});

connection.onreconnecting(() => { wsStatus.textContent = "WS: reconnecting"; });
connection.onreconnected(() => {
    wsStatus.textContent = "WS: connected";
    syncQueue(); // catch anything missed while disconnected
});
connection.onclose(() => { wsStatus.textContent = "WS: disconnected"; });

async function start() {
    try {
        await connection.start();
        wsStatus.textContent = "WS: connected";
        await syncQueue();
    } catch {
        wsStatus.textContent = "WS: retrying";
        setTimeout(start, 3000);
    }
}
start();