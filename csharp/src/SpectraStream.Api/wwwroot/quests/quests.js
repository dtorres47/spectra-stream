let selectedQuest = null;

fetch("/api/catalog")
  .then(res => res.json())
  .then(catalog => {
    const grid = document.getElementById("questGrid");
    catalog.forEach(quest => {
      const div = document.createElement("div");
      div.className = "quest-box";
      div.innerHTML = `
        <h3>${quest.name}</h3>
        <p>${quest.description}</p>
        <p><strong>$${quest.amount}</strong></p>
        <button onclick="selectQuest('${quest.id}')">Select</button>
        <button onclick="copyCode('${quest.id}')">Copy Code</button>
        <a href="https://streamlabs.com/YOUR_CHANNEL/donate" target="_blank">
          <button>Donate</button>
        </a>
      `;
      grid.appendChild(div);
    });
  })
  .catch(err => console.error("Error loading catalog:", err));

function selectQuest(id) {
  selectedQuest = id;
  alert(`Selected quest: ${id}`);
}

function copyCode(id) {
  const code = `QUEST:${id}-${Date.now()}`;
  navigator.clipboard.writeText(code)
    .then(() => alert(`Copied code: ${code}`))
    .catch(err => console.error("Copy failed", err));
}
