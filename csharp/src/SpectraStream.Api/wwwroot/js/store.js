document.addEventListener('DOMContentLoaded', async () => {
    const status = document.getElementById('status');
    const submitBtn = document.getElementById('submit-btn');
    const selectionName = document.getElementById('selection-name');
    const selectionPrice = document.getElementById('selection-price');

    let catalog = { Abilities: [], Quests: [] };
    let selection = null; // { type: 'ability'|'quest', item: {...} }

    // Load catalog
    try {
        const res = await fetch('/api/catalog');
        catalog = await res.json();
        renderAbilities();
        renderQuests();
    } catch (err) {
        console.error('Failed to load catalog:', err);
    }

    // Submit handler
    submitBtn.addEventListener('click', async () => {
        if (!selection) return;

        const donor = document.getElementById('donor').value.trim() || 'Anonymous';
        const amount = parseFloat(document.getElementById('amount').value);
        const message = document.getElementById('message').value.trim();

        if (!amount || amount <= 0) {
            showStatus('Please enter a valid amount', 'error');
            return;
        }

        submitBtn.disabled = true;
        submitBtn.textContent = 'Submitting...';

        try {
            if (selection.type === 'ability') {
                // Submit to TTS queue (goes to moderation)
                const params = new URLSearchParams({
                    text: message || `${donor} triggered ${selection.item.name}`,
                    voice: '',
                    donor: donor,
                    msg: message,
                    amount_cents: Math.round(amount * 100)
                });
                
                const res = await fetch(`/api/tts/submit?${params}`);
                if (res.ok) {
                    showStatus(`Ability "${selection.item.name}" submitted for approval!`, 'success');
                    clearSelection();
                    document.getElementById('message').value = '';
                } else {
                    showStatus('Failed to submit', 'error');
                }
            } else if (selection.type === 'quest') {
                // Record donation
                await fetch('/api/donation', {
                    method: 'POST',
                    headers: { 'Content-Type': 'application/json' },
                    body: JSON.stringify({
                        time: new Date().toISOString(),
                        donor: donor,
                        amount: amount,
                        message: `Quest: ${selection.item.name}`
                    })
                });

                // Add quest to active quests
                const res = await fetch(`/api/quest/add?id=${selection.item.id}`);
                if (res.ok) {
                    showStatus(`Quest "${selection.item.name}" activated!`, 'success');
                    clearSelection();
                } else {
                    showStatus('Failed to activate quest', 'error');
                }
            }
        } catch (err) {
            showStatus('Could not connect', 'error');
        } finally {
            submitBtn.disabled = false;
            updateSubmitButton();
        }
    });

    // Render abilities
    function renderAbilities() {
        const grid = document.getElementById('abilities-grid');
        if (catalog.Abilities.length === 0) {
            grid.innerHTML = '<div class="loading">No abilities available</div>';
            return;
        }

        grid.innerHTML = catalog.Abilities.map(a => `
            <div class="option-item" data-type="ability" data-id="${a.id}">
                <div class="option-icon">⚡</div>
                <span class="option-name">${escapeHtml(a.name)}</span>
                <span class="option-price">${formatPrice(a.price_cents)}</span>
            </div>
        `).join('');

        grid.querySelectorAll('.option-item').forEach(item => {
            item.addEventListener('click', () => selectItem('ability', item.dataset.id));
        });
    }

    // Render quests
    function renderQuests() {
        const grid = document.getElementById('quests-grid');
        if (catalog.Quests.length === 0) {
            grid.innerHTML = '<div class="loading">No quests available</div>';
            return;
        }

        grid.innerHTML = catalog.Quests.map(q => `
            <div class="option-item" data-type="quest" data-id="${q.id}">
                <div class="option-icon">🎯</div>
                <span class="option-name">${escapeHtml(q.name)}</span>
                <span class="option-price">${formatPrice(q.price_cents)}</span>
            </div>
        `).join('');

        grid.querySelectorAll('.option-item').forEach(item => {
            item.addEventListener('click', () => selectItem('quest', item.dataset.id));
        });
    }

    // Select an item
    function selectItem(type, id) {
        // Clear all selections
        document.querySelectorAll('.option-item').forEach(el => el.classList.remove('selected'));

        // Find the item
        let item;
        if (type === 'ability') {
            item = catalog.Abilities.find(a => a.id === id);
        } else {
            item = catalog.Quests.find(q => q.id === id);
        }

        if (!item) return;

        // Select it
        const el = document.querySelector(`.option-item[data-type="${type}"][data-id="${id}"]`);
        if (el) el.classList.add('selected');

        selection = { type, item };

        // Update summary
        selectionName.textContent = `${type === 'ability' ? '⚡' : '🎯'} ${item.name}`;
        selectionPrice.textContent = formatPrice(item.price_cents);

        // Update amount field to match minimum
        const amountInput = document.getElementById('amount');
        const minAmount = item.price_cents / 100;
        if (parseFloat(amountInput.value) < minAmount) {
            amountInput.value = minAmount.toFixed(2);
        }

        updateSubmitButton();
    }

    // Clear selection
    function clearSelection() {
        document.querySelectorAll('.option-item').forEach(el => el.classList.remove('selected'));
        selection = null;
        selectionName.textContent = 'None';
        selectionPrice.textContent = '';
        updateSubmitButton();
    }

    // Update submit button text
    function updateSubmitButton() {
        if (!selection) {
            submitBtn.disabled = true;
            submitBtn.textContent = 'Select an ability or quest';
        } else {
            submitBtn.disabled = false;
            submitBtn.textContent = selection.type === 'ability' 
                ? `Submit Ability (${formatPrice(selection.item.price_cents)}+)`
                : `Activate Quest (${formatPrice(selection.item.price_cents)}+)`;
        }
    }

    // Helpers
    function formatPrice(cents) {
        return '$' + (cents / 100).toFixed(2);
    }

    function showStatus(msg, type) {
        status.textContent = msg;
        status.className = `status ${type}`;
    }

    function escapeHtml(text) {
        const div = document.createElement('div');
        div.textContent = text;
        return div.innerHTML;
    }
});
