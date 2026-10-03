const teamValueForm = document.getElementById("teamValueForm");

teamValueForm?.addEventListener("submit", async event => {
    event.preventDefault();
    const teamId = document.getElementById("teamDetailsRoot").dataset.teamId;
    const input = document.getElementById("teamValueInput");
    const status = document.getElementById("teamValueStatus");
    const button = teamValueForm.querySelector("button");
    button.disabled = true;
    status.textContent = "";

    try {
        const response = await fetch(`/api/teams/${teamId}/teamvalue`, {
            method: "PUT",
            headers: {
                "Content-Type": "application/json",
                "RequestVerificationToken": teamValueForm.querySelector('[name="__RequestVerificationToken"]').value
            },
            body: JSON.stringify({ teamValue: input.value === "" ? null : Number(input.value) })
        });
        if (response.ok) {
            window.location.reload();
            return;
        }
        const messages = {
            400: "The value or security token is invalid. Reload the page and try again.",
            401: "Please sign in again to update this team.",
            403: "Only the team maintainer or an admin can update this team.",
            404: "This team no longer exists."
        };
        status.textContent = messages[response.status] ?? `Update failed (${response.status}).`;
    } catch {
        status.textContent = "A network error prevented the update. Please try again.";
    } finally {
        button.disabled = false;
    }
});
