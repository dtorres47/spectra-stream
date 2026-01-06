document.addEventListener('DOMContentLoaded', async () => {
    const status = document.getElementById('status');
    let catalog = { Abilities: [], Quests: [] };
    let selectedQuest = null;
    let selectedAbility = null;
    let selectedVoice = { voice: 'default', price: 200 };

    // Load catalog
    try {
        const res = await fetch('/api/catalog');
        catalog = await res.json();
        renderQuests();
        renderAbilities();
    } catch (err) {
        console.error('Failed to load catalog:', err);
    }

    // Tab switching
    document.querySelectorAll('.tab').forEach(tab => {
        tab.addEventListener('click', () => {
            document.querySelectorAll('.tab').forEach(t => t.classList.remove('active'));
            document.querySelectorAll('.tab-content').forEach(c => c.classList.remove('active'));
            tab.classList.add('active');
            document.getElementById(`tab-${tab.dataset.tab}`).classList.add('active');
            hideStatus();
        });
    });

    // TTS voice selection
    document.querySelectorAll('#tts-voices .option-item').forEach(item => {
        item.addEventListener('click', () => {
            document.querySelectorAll('#tts-voices .option-item').forEach(i => i.classList.remove('selected'));
            item.classList.add('selected');
            selectedVoice = {
                voice: item.dataset.voice,
                price: parseInt(item.dataset.price)
            };
            document.getElementById('tts-total').textContent = formatPrice(selectedVoice.price);
        });
    });

    // Donate submit
    document.getElementById('donate-submit').addEventListener('click', async () => {
        const donor = getDonor();
        const amount = parseFloat(document.getElementById('donate-amount').value);
        const message = document.getElementById('donate-message').value.trim();

        if (!amount || amount <= 0) {
            showStatus('Please enter a valid amount', 'error');
            return;
        }

        await submitDonation(donor, amount, message);
    });

    // TTS submit
    document.getElementById('tts-submit').addEventListener('click', async () => {
        const donor = getDonor();
        const text = document.getElementById('tts-text').value.trim();

        if (!text) {
            showStatus('Please enter a message for TTS', 'error');
            return;
        }

        const btn = document.getElementById('tts-submit');
        btn.disabled = true;
        btn.textContent = 'Submitting...';

        try {
            const params = new URLSearchParams({
                text: text,
                voice: selectedVoice.voice,
                donor: donor,
                amount_cents: selectedVoice.price
            });

            const res = await fetch(`/api/tts/submit?${params}`);
            if (res.ok) {
                showStatus('TTS submitted for approval!', 'success');
                document.getElementById('tts-text').value = '';
            } else {
                showStatus('Failed to submit TTS', 'error');
            }
        } catch (err) {
            showStatus('Could not connect', 'error');
        } finally {
            btn.disabled = false;
            btn.textContent = 'Submit TTS';
        }
    });

    // Quest submit
    document.getElementById('quest-submit').addEventListener('click', async () => {
        if (!selectedQuest) return;

        const btn = document.getElementById('quest-submit');
        btn.disabled = true;
        btn.textContent = 'Submitting...';

        try {
            // Record donation first
            const donor = getDonor();
            await submitDonation(donor, selectedQuest.price_cents / 100, `Quest: ${selectedQuest.name}`, true);

            // Add quest
            const res = await fetch(`/api/quest/add?id=${selectedQuest.id}`);
            if (res.ok) {
                showStatus(`Quest "${selectedQuest.name}" activated!`, 'success');
                clearQuestSelection();
            } else {
                showStatus('Failed to activate quest', 'error');
            }
        } catch (err) {
            showStatus('Could not connect', 'error');
        } finally {
            btn.disabled = false;
            btn.textContent = 'Activate Quest';
        }
    });

    // Ability submit
    document.getElementById('ability-submit').addEventListener('click', async () => {
        if (!selectedAbility) return;

        const btn = document.getElementById('ability-submit');
        btn.disabled = true;
        btn.textContent = 'Submitting...';

        try {
            // Record donation
            const donor = getDonor();
            await submitDonation(donor, selectedAbility.price_cents / 100, `Ability: ${selectedAbility.name}`, true);

            // TODO: Trigger ability via SignalR or API
            showStatus(`Ability "${selectedAbility.name}" triggered!`, 'success');
            clearAbilitySelection();
        } catch (err) {
            showStatus('Could not connect', 'error');
        } finally {
            btn.disabled = false;
            btn.textContent = 'Trigger Ability';
        }
    });

    // Request submit
    document.getElementById('request-submit').addEventListener('click', async () => {
        const board = document.getElementById('request-board').value.trim();
        const phone = document.getElementById('request-phone').value.trim();
        const note = document.getElementById('request-note').value.trim();
        const amount = parseFloat(document.getElementById('request-amount').value);

        if (!board && !phone) {
            showStatus('Please enter a soundboard or phone number', 'error');
            return;
        }

        const btn = document.getElementById('request-submit');
        btn.disabled = true;
        btn.textContent = 'Submitting...';

        try {
            // Record donation
            const donor = getDonor();
            if (amount > 0) {
                await submitDonation(donor, amount, `Request: ${board || 'Custom'}`, true);
            }

            // Submit request
            const params = new URLSearchParams();
            if (board) params.append('board', board);
            if (phone) params.append('phone', phone);
            if (note) params.append('note', note);

            const res = await fetch(`/api/request/submit?${params}`);
            if (res.ok) {
                showStatus('Request submitted for approval!', 'success');
                document.getElementById('request-board').value = '';
                document.getElementById('request-phone').value = '';
                document.getElementById('request-note').value = '';
            } else {
                showStatus('Failed to submit request', 'error');
            }
        } catch (err) {
            showStatus('Could not connect', 'error');
        } finally {
            btn.disabled = false;
            btn.textContent = 'Submit Request';
        }
    });

    // Helper functions
    function getDonor() {
        return document.getElementById('donor').value.trim() || 'Anonymous';
    }

    async function submitDonation(donor, amount, message, silent = false) {
        try {
            const res = await fetch('/api/donation', {
                method: 'POST',
                headers: { 'Content-Type': 'application/json' },
                body: JSON.stringify({
                    time: new Date().toISOString(),
                    donor: donor,
                    amount: amount,
                    message: message || ''
                })
            });

            if (res.ok && !silent) {
                showStatus('Thank you for your donation!', 'success');
                document.getElementById('donate-message').value = '';
            } else if (!res.ok) {
                throw new Error('Failed');
            }
        } catch (err) {
            if (!silent) showStatus('Could not send donation', 'error');
            throw err;
        }
    }

    function renderQuests() {
        const grid = document.getElementById('quests-grid');
        if (catalog.Quests.length === 0) {
            grid.innerHTML = '<div class="loading">No quests available</div>';
            return;
        }

        grid.innerHTML = catalog.Quests.map(q => `
            <div class="option-item" data-id="${q.id}">
                <div class="option-icon">${q.icon_url ? `<img src="${q.icon_url}" alt="">` : '🎯'}</div>
                <span class="option-name">${q.name}</span>
                <span class="option-price">${formatPrice(q.price_cents)}</span>
            </div>
        `).join('');

        grid.querySelectorAll('.option-item').forEach(item => {
            item.addEventListener('click', () => {
                grid.querySelectorAll('.option-item').forEach(i => i.classList.remove('selected'));
                item.classList.add('selected');
                selectedQuest = catalog.Quests.find(q => q.id === item.dataset.id);
                document.getElementById('quest-total').textContent = formatPrice(selectedQuest.price_cents);
                document.getElementById('quest-price').classList.remove('hidden');
                document.getElementById('quest-submit').disabled = false;
                document.getElementById('quest-submit').textContent = 'Activate Quest';
            });
        });
    }

    function renderAbilities() {
        const grid = document.getElementById('abilities-grid');
        if (catalog.Abilities.length === 0) {
            grid.innerHTML = '<div class="loading">No abilities available</div>';
            return;
        }

        grid.innerHTML = catalog.Abilities.map(a => `
            <div class="option-item" data-id="${a.id}">
                <div class="option-icon">${a.icon_url ? `<img src="${a.icon_url}" alt="">` : '⚡'}</div>
                <span class="option-name">${a.name}</span>
                <span class="option-price">${formatPrice(a.price_cents)}</span>
            </div>
        `).join('');

        grid.querySelectorAll('.option-item').forEach(item => {
            item.addEventListener('click', () => {
                grid.querySelectorAll('.option-item').forEach(i => i.classList.remove('selected'));
                item.classList.add('selected');
                selectedAbility = catalog.Abilities.find(a => a.id === item.dataset.id);
                document.getElementById('ability-total').textContent = formatPrice(selectedAbility.price_cents);
                document.getElementById('ability-price').classList.remove('hidden');
                document.getElementById('ability-submit').disabled = false;
                document.getElementById('ability-submit').textContent = 'Trigger Ability';
            });
        });
    }

    function clearQuestSelection() {
        selectedQuest = null;
        document.querySelectorAll('#quests-grid .option-item').forEach(i => i.classList.remove('selected'));
        document.getElementById('quest-price').classList.add('hidden');
        document.getElementById('quest-submit').disabled = true;
        document.getElementById('quest-submit').textContent = 'Select a Quest';
    }

    function clearAbilitySelection() {
        selectedAbility = null;
        document.querySelectorAll('#abilities-grid .option-item').forEach(i => i.classList.remove('selected'));
        document.getElementById('ability-price').classList.add('hidden');
        document.getElementById('ability-submit').disabled = true;
        document.getElementById('ability-submit').textContent = 'Select an Ability';
    }

    function formatPrice(cents) {
        return '$' + (cents / 100).toFixed(2);
    }

    function showStatus(msg, type) {
        status.textContent = msg;
        status.className = `status ${type}`;
    }

    function hideStatus() {
        status.className = 'status hidden';
    }
});
