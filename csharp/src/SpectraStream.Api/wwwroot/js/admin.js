// Client count
const elClients = document.getElementById('clients');
async function refreshClients() {
    const t = await fetch('/api/debug/clients').then(r => r.text()).catch(() => '0');
    elClients.textContent = (t.match(/\d+/) || ['0'])[0];
}
document.getElementById('refreshClients').onclick = refreshClients;

// Control actions
document.getElementById('reload').onclick = async () => {
    await fetch('/api/catalog/reload',{ method:'POST' });
    await loadCatalog();
};
document.getElementById('saveState').onclick = () => fetch('/api/state/save',{ method:'POST' });
document.getElementById('syncOverlay').onclick = () => fetch('/api/state/rehydrate',{ method:'POST' });

// Donations
document.getElementById('sendDonation').onclick = async () => {
    const donor = document.getElementById('donor').value || 'Viewer';
    const amountD = parseFloat(document.getElementById('amount').value || '0');
    const cents = Math.max(0, Math.round(amountD * 100));
    const msg = encodeURIComponent(document.getElementById('msg').value || '');
    await fetch(`/api/donation?donor=${encodeURIComponent(donor)}&amount_cents=${cents}&msg=${msg}`);
    refreshClients();
};

// Direct TTS
document.getElementById('sendTTS').onclick = async () => {
    const text = encodeURIComponent(document.getElementById('ttsText').value || '');
    if (!text) return;
    const voice = encodeURIComponent(document.getElementById('ttsVoice').value || '');
    await fetch(`/api/test/tts?text=${text}&voice=${voice}`);
    refreshClients();
};

// Catalog loading
async function loadCatalog() {
    const data = await fetch('/api/catalog').then(r => r.json());
    const abil = document.getElementById('abilities');
    const ques = document.getElementById('quests');
    abil.innerHTML = ''; ques.innerHTML = '';

    data.abilities.forEach(a => {
        const d = document.createElement('div');
        d.className = 'item';
        d.innerHTML = `<div><strong>${a.name}</strong>
      <small class="mono">($${((a.price_cents||0)/100).toFixed(2)}, cd ${a.cooldown_ms||3000}ms)</small>
      <br/><small class="mono">${a.id}</small></div>`;
        const b = document.createElement('button');
        b.textContent = 'Fire';
        b.onclick = () => fetch(`/api/ability/fire?id=${encodeURIComponent(a.id)}`);
        d.appendChild(b);
        abil.appendChild(d);
    });

    data.quests.forEach(q => {
        const d = document.createElement('div');
        d.className = 'item';
        const tgt = q.target || 1;
        d.innerHTML = `<div><strong>${q.name}</strong>
      <small class="mono">($${((q.price_cents||0)/100).toFixed(2)}, target ${tgt})</small>
      <br/><small class="mono">${q.id}</small></div>`;
        const b = document.createElement('button');
        b.textContent = 'Start';
        b.onclick = async () => { await fetch(`/api/quest/add?id=${encodeURIComponent(q.id)}`); loadActiveQuests(); };
        d.appendChild(b);
        ques.appendChild(d);
    });

    loadActiveQuests();
}

// Active quests
async function loadActiveQuests() {
    const data = await fetch('/api/quest/active').then(r => r.json());
    const list = document.getElementById('active');
    list.innerHTML = data.length ? '' : '<div class="item"><em>None yet</em></div>';
    data.forEach(qs => {
        const d = document.createElement('div'); d.className = 'item';
        d.innerHTML = `<div><strong>${qs.name}</strong> <small class="mono">${qs.progress}/${qs.target}</small>
      <br/><small class="mono">${qs.id}</small></div>`;
        const btns = document.createElement('div'); btns.className='btns';
        ['+1','Reset','Remove'].forEach((txt,i) => {
            const btn = document.createElement('button');
            btn.textContent = txt;
            if(i>0) btn.className='secondary';
            btn.onclick = async () => {
                const m = i===0?'inc':(i===1?'reset':'remove');
                await fetch(`/api/quest/${m}?id=${encodeURIComponent(qs.id)}`, { method: 'POST' });
                loadActiveQuests();
            };
            btns.appendChild(btn);
        });
        d.appendChild(btns);
        list.appendChild(d);
    });
    refreshClients();
}

// TTS Queue
async function loadQueue() {
    const qList = document.getElementById('qList');
    const items = await fetch('/api/tts/queue').then(r => r.json());
    qList.innerHTML = items.length ? '' : '<div class="item"><em>None pending</em></div>';
    items.forEach(it => {
        const d = document.createElement('div'); d.className='item';
        const dollars = (Number(it.amount_cents||0)/100).toFixed(2);
        d.innerHTML = `<div><strong>${it.text}</strong><br/>
      <small class="mono">${it.voice||'default'}</small> |
      <small class="mono">${it.donor||'Anonymous'}</small> |
      <small class="mono">$${dollars}</small></div>`;
        const btns = document.createElement('div'); btns.className='btns';
        ['Approve','Reject'].forEach((txt,i) => {
            const btn = document.createElement('button');
            btn.textContent = txt;
            if(i===1) btn.className='secondary';
            btn.onclick = async () => {
                const action = txt.toLowerCase();
                await fetch(`/api/tts/${action}?id=${it.id}`, { method:'POST' });
                loadQueue();
            };
            btns.appendChild(btn);
        });
        d.appendChild(btns);
        qList.appendChild(d);
    });
}
document.getElementById('qRefresh').onclick = loadQueue;
document.getElementById('qSubmit').onclick = async () => {
    const text = document.getElementById('qText').value||''; if(!text) return;
    const voice = document.getElementById('qVoice').value||'';
    const donor = document.getElementById('qDonor').value||'';
    const cents = Math.max(0,Math.round(parseFloat(document.getElementById('qAmount').value||'0')*100));
    await fetch(`/api/tts/submit?text=${encodeURIComponent(text)}&voice=${encodeURIComponent(voice)}&donor=${encodeURIComponent(donor)}&amount_cents=${cents}`);
    document.getElementById('qText').value='';
    loadQueue(); refreshClients();
};

// Requests
async function loadRequestQueue() {
    const rqList = document.getElementById('rqList');
    const items = await fetch('/api/request/queue').then(r => r.json());
    rqList.innerHTML = items.length ? '' : '<div class="item"><em>None pending</em></div>';
    items.forEach(it => {
        const d = document.createElement('div'); d.className='item';
        d.innerHTML = `<div><strong>${it.board||( '(no board)')} ${it.note? '—'+it.note : ''}</strong><br/>
      <small class="mono">Phone: ${it.phone||'(none)'}</small> |
      <small class="mono">Masked: ${it.masked_phone||'(none)'}</small></div>`;
        const btns = document.createElement('div'); btns.className='btns';
        ['approve','reject'].forEach((act,i) => {
            const btn = document.createElement('button');
            btn.textContent = act.charAt(0).toUpperCase()+act.slice(1);
            if(act==='reject') btn.className='secondary';
            btn.onclick = async () => {
                await fetch(`/api/request/${act}?id=${it.id}`,{ method:'POST' });
                loadRequestQueue(); loadActiveRequests();
            };
            btns.appendChild(btn);
        });
        d.appendChild(btns);
        rqList.appendChild(d);
    });
}
async function loadActiveRequests() {
    const rqActive = document.getElementById('rqActive');
    const items = await fetch('/api/request/active').then(r => r.json());
    rqActive.innerHTML = items.length ? '' : '<div class="item"><em>None</em></div>';
    items.forEach(it => {
        const d = document.createElement('div'); d.className='item';
        d.innerHTML = `<div><strong>${it.board||( '(no board)')} ${it.note? '—'+it.note : ''}</strong><br/>
      <small class="mono">Masked: ${it.masked_phone||'(none)'}</small></div>`;
        const btn = document.createElement('button');
        btn.textContent='Complete';
        btn.onclick = async () => { await fetch(`/api/request/complete?id=${it.id}`,{ method:'POST'}); loadActiveRequests(); };
        d.appendChild(btn);
        rqActive.appendChild(d);
    });
}
document.getElementById('rqRefresh').onclick = loadRequestQueue;
document.getElementById('rqSubmit').onclick = async () => {
    const board = document.getElementById('rqBoard').value||'';
    const phone = document.getElementById('rqPhone').value||'';
    const note = document.getElementById('rqNote').value||'';
    await fetch(`/api/request/submit?board=${encodeURIComponent(board)}&phone=${encodeURIComponent(phone)}&note=${encodeURIComponent(note)}`);
    loadRequestQueue(); loadActiveRequests();
};

// Init
refreshClients();
loadCatalog();
loadQueue();
loadRequestQueue();
loadActiveRequests();
