const queueEl = document.getElementById('queue');

async function loadQueue() {
    try {
        const queue = await fetch('/api/queue').then(r => r.json());
        render(queue);
    } catch (e) {
        queueEl.innerHTML = '<div class="item"><em>Failed to load queue</em></div>';
        console.error('Queue load failed:', e);
    }
}

function render(queue) {
    if (!queue.length) {
        queueEl.innerHTML = '<div class="item"><em>No active quests</em></div>';
        return;
    }

    queueEl.innerHTML = '';
    queue.forEach(q => {
        const d = document.createElement('div');
        d.className = 'item';

        const objectives = (q.objectives || [])
            .map(o => escapeHtml(o.title))
            .join(', ');

        const when = q.createdAt ? new Date(q.createdAt).toLocaleTimeString() : '';

        d.innerHTML = `<div>
            <strong>${escapeHtml(q.title)}</strong>
            <small class="mono">from ${escapeHtml(q.supporter || 'Anonymous')}${when ? ' · ' + when : ''}</small>
            ${objectives ? `<br/><small class="mono">${objectives}</small>` : ''}
        </div>`;

        const btn = document.createElement('button');
        btn.textContent = 'Complete';
        btn.onclick = async () => {
            btn.disabled = true;
            const res = await fetch(
                `/api/queue/remove?instanceId=${encodeURIComponent(q.instanceId)}`,
                { method: 'POST' }
            ).catch(() => null);
            if (!res || !res.ok) {
                btn.disabled = false;
                console.warn('Remove failed');
                return;
            }
            loadQueue();
        };
        d.appendChild(btn);
        queueEl.appendChild(d);
    });
}

function escapeHtml(text) {
    const div = document.createElement('div');
    div.textContent = text ?? '';
    return div.innerHTML;
}

// Live updates: re-render whenever a quest is added or removed
const connection = new signalR.HubConnectionBuilder()
    .withUrl('/ws')
    .withAutomaticReconnect()
    .build();

connection.on('ReceiveEvent', msg => {
    if (msg && (msg.type === 'QUEST_UPSERT' || msg.type === 'QUEST_REMOVE')) {
        loadQueue();
    }
});
connection.onreconnected(loadQueue);
connection.start().catch(() => {/* polling via Refresh still works */ });

document.getElementById('refresh').onclick = loadQueue;
loadQueue();