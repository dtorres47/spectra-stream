document.addEventListener('DOMContentLoaded', () => {
    const questsList = document.getElementById('questsList');
    const activeList = document.getElementById('activeList');
    const completedList = document.getElementById('completedList');
    const refreshBtn = document.getElementById('refresh');

    let catalog = { Quests: [] };
    let activeQuests = [];
    let completedQuests = [];

    // Initial load
    loadAll();

    // Refresh button
    refreshBtn.addEventListener('click', loadAll);

    async function loadAll() {
        await Promise.all([loadCatalog(), loadActive()]);
        render();
    }

    async function loadCatalog() {
        try {
            const res = await fetch('/api/catalog');
            catalog = await res.json();
        } catch (err) {
            console.error('Failed to load catalog:', err);
        }
    }

    async function loadActive() {
        try {
            const res = await fetch('/api/quest/active');
            const data = await res.json();
            
            // Separate active and completed
            activeQuests = data.filter(q => q.current < q.target);
            completedQuests = data.filter(q => q.current >= q.target);
        } catch (err) {
            console.error('Failed to load active quests:', err);
        }
    }

    function render() {
        renderCatalog();
        renderActive();
        renderCompleted();
    }

    function renderCatalog() {
        if (catalog.Quests.length === 0) {
            questsList.innerHTML = '<div class="item"><em>No quests in catalog</em></div>';
            return;
        }

        questsList.innerHTML = catalog.Quests.map(q => `
            <div class="item">
                <div class="info">
                    <div class="name">${escapeHtml(q.name)}</div>
                    <div class="meta">
                        <span class="price">$${(q.price_cents / 100).toFixed(2)}</span>
                        · Target: ${q.target}
                    </div>
                </div>
                <div class="btns">
                    <button class="secondary" data-add="${q.id}">Add to Active</button>
                </div>
            </div>
        `).join('');

        // Add event listeners
        questsList.querySelectorAll('[data-add]').forEach(btn => {
            btn.addEventListener('click', () => addQuest(btn.dataset.add));
        });
    }

    function renderActive() {
        if (activeQuests.length === 0) {
            activeList.innerHTML = '<div class="item"><em>No active quests</em></div>';
            return;
        }

        activeList.innerHTML = activeQuests.map(q => `
            <div class="item">
                <div class="info">
                    <div class="name">${escapeHtml(q.name)}</div>
                    <div class="meta">Progress: ${q.current} / ${q.target}</div>
                </div>
                <span class="progress">${Math.round((q.current / q.target) * 100)}%</span>
                <div class="btns">
                    <button class="success" data-inc="${q.id}">+1</button>
                    <button class="secondary" data-reset="${q.id}">Reset</button>
                    <button class="danger" data-remove="${q.id}">Remove</button>
                </div>
            </div>
        `).join('');

        // Add event listeners
        activeList.querySelectorAll('[data-inc]').forEach(btn => {
            btn.addEventListener('click', () => incrementQuest(btn.dataset.inc));
        });
        activeList.querySelectorAll('[data-reset]').forEach(btn => {
            btn.addEventListener('click', () => resetQuest(btn.dataset.reset));
        });
        activeList.querySelectorAll('[data-remove]').forEach(btn => {
            btn.addEventListener('click', () => removeQuest(btn.dataset.remove));
        });
    }

    function renderCompleted() {
        if (completedQuests.length === 0) {
            completedList.innerHTML = '<div class="item"><em>No completed quests</em></div>';
            return;
        }

        completedList.innerHTML = completedQuests.map(q => `
            <div class="item completed">
                <div class="info">
                    <div class="name">${escapeHtml(q.name)}</div>
                    <div class="meta">Completed: ${q.current} / ${q.target}</div>
                </div>
                <div class="btns">
                    <button class="secondary" data-reset="${q.id}">Reset</button>
                    <button class="danger" data-remove="${q.id}">Remove</button>
                </div>
            </div>
        `).join('');

        // Add event listeners
        completedList.querySelectorAll('[data-reset]').forEach(btn => {
            btn.addEventListener('click', () => resetQuest(btn.dataset.reset));
        });
        completedList.querySelectorAll('[data-remove]').forEach(btn => {
            btn.addEventListener('click', () => removeQuest(btn.dataset.remove));
        });
    }

    async function addQuest(id) {
        try {
            await fetch(`/api/quest/add?id=${id}`);
            await loadActive();
            render();
        } catch (err) {
            console.error('Failed to add quest:', err);
        }
    }

    async function incrementQuest(id) {
        try {
            await fetch(`/api/quest/inc?id=${id}`, { method: 'POST' });
            await loadActive();
            render();
        } catch (err) {
            console.error('Failed to increment quest:', err);
        }
    }

    async function resetQuest(id) {
        try {
            await fetch(`/api/quest/reset?id=${id}`, { method: 'POST' });
            await loadActive();
            render();
        } catch (err) {
            console.error('Failed to reset quest:', err);
        }
    }

    async function removeQuest(id) {
        try {
            await fetch(`/api/quest/remove?id=${id}`, { method: 'POST' });
            await loadActive();
            render();
        } catch (err) {
            console.error('Failed to remove quest:', err);
        }
    }

    function escapeHtml(text) {
        const div = document.createElement('div');
        div.textContent = text;
        return div.innerHTML;
    }
});
