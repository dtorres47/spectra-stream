document.addEventListener('DOMContentLoaded', async () => {
    const grid = document.getElementById('quests-grid');
    const detail = document.getElementById('quest-detail');
    const detailTitle = document.getElementById('detail-title');
    const detailDesc = document.getElementById('detail-desc');
    const detailObjectives = document.getElementById('detail-objectives');
    const detailToken = document.getElementById('detail-token');
    const copyBtn = document.getElementById('copy-btn');
    const kofiLink = document.getElementById('kofi-link');
    const status = document.getElementById('status');

    let quests = [];
    let selected = null;
    let kofiUrl = '#';

    // Load brand config (Ko-fi URL) and storefront
    try {
        const brand = await fetch('/config/brand.json').then(r => r.json());
        if (brand.kofiUrl) {
            kofiUrl = brand.kofiUrl;
            kofiLink.href = kofiUrl;
        }
    } catch (e) {
        console.warn('brand.json load failed:', e);
    }

    try {
        quests = await fetch('/api/storefront').then(r => r.json());
        renderGrid();
    } catch (e) {
        grid.innerHTML = '<div class="loading">Could not load quests</div>';
        console.error('Storefront load failed:', e);
    }

    function renderGrid() {
        if (!quests.length) {
            grid.innerHTML = '<div class="loading">No quests available</div>';
            return;
        }
        grid.innerHTML = quests.map(q => `
            <div class="option-item" data-id="${q.id}">
                <div class="option-icon">🎯</div>
                <span class="option-name">${escapeHtml(q.title)}</span>
                <span class="option-price">${formatPrice(q.priceCents)}</span>
            </div>
        `).join('');

        grid.querySelectorAll('.option-item').forEach(item => {
            item.addEventListener('click', () => select(item.dataset.id));
        });
    }

    function select(id) {
        selected = quests.find(q => q.id === id);
        if (!selected) return;

        grid.querySelectorAll('.option-item').forEach(el =>
            el.classList.toggle('selected', el.dataset.id === id));

        detailTitle.textContent = selected.title;
        detailDesc.textContent = selected.description || '';
        detailObjectives.innerHTML = (selected.objectives || [])
            .map(o => `<li><strong>${escapeHtml(o.title)}</strong>${o.description ? ' — ' + escapeHtml(o.description) : ''}</li>`)
            .join('');
        detailToken.textContent = selected.token;
        detail.classList.remove('hidden');
    }

    copyBtn.addEventListener('click', async () => {
        if (!selected) return;
        try {
            await navigator.clipboard.writeText(selected.token);
            showStatus(`Copied ${selected.token} — paste it into your Ko-fi message!`, 'success');
        } catch {
            // Clipboard API can fail (permissions/http); fall back to selecting the text
            const range = document.createRange();
            range.selectNodeContents(detailToken);
            const sel = window.getSelection();
            sel.removeAllRanges();
            sel.addRange(range);
            showStatus('Press Ctrl+C to copy the highlighted token', 'success');
        }
    });

    function formatPrice(cents) {
        return '$' + ((cents || 0) / 100).toFixed(2);
    }

    function showStatus(msg, type) {
        status.textContent = msg;
        status.className = `status ${type}`;
    }

    function escapeHtml(text) {
        const div = document.createElement('div');
        div.textContent = text ?? '';
        return div.innerHTML;
    }
});