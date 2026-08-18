const groupId = document.getElementById("groupMeta").dataset.groupId;
const tableBody = document.querySelector("#groupTeamsTable tbody");
const teamSelect = document.getElementById("teamSelect");
const pointsInput = document.getElementById("pointsInput");
const errorBox = document.getElementById("errorBox");
const addButton = document.getElementById("addBtn");

async function loadGroupTeams() {
    const res = await fetch(`/api/groups/${groupId}/teams`);
    tableBody.innerHTML = "";

    if (!res.ok) {
        tableBody.innerHTML = `<tr><td colspan="3">Error: ${res.status}</td></tr>`;
        return;
    }

    const data = await res.json();
    for (const x of data) {
        const tr = document.createElement("tr");
        tr.innerHTML = `<td>${x.name}</td><td>${x.country}</td><td>${x.points}</td>`;
        tableBody.appendChild(tr);
    }
    return data;
}

async function loadTeamOptions(existingTeamIds) {
    const res = await fetch("/api/teams");
    teamSelect.innerHTML = "";
    if (!res.ok) {
        addButton.disabled = true;
        return;
    }

    const teams = await res.json();
    const filtered = teams.filter(t => !existingTeamIds.has(t.id));

    for (const t of filtered) {
        const opt = document.createElement("option");
        opt.value = t.id;
        opt.textContent = `${t.name} (${t.country})`;
        teamSelect.appendChild(opt);
    }

    addButton.disabled = filtered.length === 0;
}

async function refreshAll() {
    errorBox.textContent = "";
    const groupTeams = await loadGroupTeams();
    if (!groupTeams) {
        teamSelect.innerHTML = "";
        addButton.disabled = true;
        return;
    }

    const existing = new Set(groupTeams.map(x => x.teamId));
    await loadTeamOptions(existing);
}

async function addTeamToGroup() {
    errorBox.textContent = "";

    if (!teamSelect.value) return;

    const teamId = Number(teamSelect.value);
    const points = Number(pointsInput.value);

    const res = await fetch(`/api/groups/${groupId}/teams`, {
        method: "POST",
        headers: { "Content-Type": "application/json" },
        body: JSON.stringify({ teamId, points })
    });

    if (!res.ok) {
        const errorMessage = await res.json();
        errorBox.textContent = `Error ${res.status}: ${errorMessage}`;
        return;
    }

    await refreshAll();
}

document.addEventListener("DOMContentLoaded", () => {
    refreshAll();
    addButton.addEventListener("click", (e) => {
        e.preventDefault();
        addTeamToGroup();
    });
});
