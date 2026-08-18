async function loadChampionships() {
    const res = await fetch("/api/championships", {
        headers: { "Accept": "application/json" }
    });

    const tbody = document.querySelector("#championshipsTable tbody");
    tbody.innerHTML = "";

    if (!res.ok) {
        tbody.innerHTML = `<tr><td colspan="4">Error: ${res.status}</td></tr>`;
        return;
    }

    const data = await res.json();

    for (const c of data) {
        const editionYear = new Date(c.championshipEdition).getFullYear();
        const tr = document.createElement("tr");
        tr.innerHTML = `
      <td>${editionYear}</td>
      <td>${c.hostCountry}</td>
      <td>${c.type}</td>
      <td>${c.groupCount}</td>
    `;
        tbody.appendChild(tr);
    }
}

document.addEventListener("DOMContentLoaded", () => {
    loadChampionships();
    document.getElementById("reloadBtn").addEventListener("click", loadChampionships);
});
