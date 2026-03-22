const teamDetailsRoot = document.getElementById("teamDetailsRoot");

if (teamDetailsRoot) {
    const teamValueForm = document.getElementById("teamValueForm");
    const teamValueInput = document.getElementById("teamValueInput");
    const submitButton = document.getElementById("teamValueSubmitButton");
    const statusBox = document.getElementById("teamValueStatus");
    const teamValueReadout = document.getElementById("teamValueReadout");

    const teamId = Number(teamDetailsRoot.dataset.teamId);
    const originalValue = teamValueInput?.value ?? "";

    const setStatus = (message, type) => {
        if (!statusBox) return;

        statusBox.hidden = false;
        statusBox.className = `alert mt-3 ${type}`;
        statusBox.textContent = message;
    };

    const setBusy = (isBusy) => {
        if (!teamValueInput || !submitButton) return;
        teamValueInput.disabled = isBusy;
        submitButton.disabled = isBusy;
        submitButton.textContent = isBusy ? "Updating..." : "Update";
    };

    const updateReadout = (rawValue) => {
        if (!teamValueReadout) return;

        if (rawValue === "") {
            teamValueReadout.textContent = "unknown";
            return;
        }

        const parsedValue = Number(rawValue);
        if (Number.isFinite(parsedValue)) {
            teamValueReadout.textContent = new Intl.NumberFormat(undefined, {
                style: "currency",
                currency: "USD"
            }).format(parsedValue);
            return;
        }

        teamValueReadout.textContent = rawValue;
    };

    teamValueForm?.addEventListener("submit", async (event) => {
        event.preventDefault();

        if (!teamValueInput || !submitButton) {
            return;
        }

        const rawValue = teamValueInput.value.trim();
        const payload = { teamValue: rawValue === "" ? null : Number(rawValue) };

        if (rawValue !== "" && Number.isNaN(payload.teamValue)) {
            setStatus("Enter a valid TeamValue or leave the field empty.", "alert-danger");
            teamValueInput.focus();
            return;
        }

        setBusy(true);
        setStatus("Saving TeamValue...", "alert-info");

        try {
            const response = await fetch(`/api/teams/${teamId}/teamvalue`, {
                method: "PUT",
                headers: {
                    "Content-Type": "application/json",
                    "Accept": "application/json"
                },
                body: JSON.stringify(payload)
            });

            if (response.status === 204) {
                updateReadout(rawValue);
                setStatus("TeamValue updated successfully.", "alert-success");
                return;
            }

            if (response.status === 400) {
                const problem = await response.json().catch(() => null);
                const firstError = problem?.errors
                    ? Object.values(problem.errors).flat()[0]
                    : null;
                setStatus(firstError ?? "The submitted TeamValue is invalid.", "alert-danger");
                return;
            }

            if (response.status === 401) {
                setStatus("Your session has expired. Sign in again to update TeamValue.", "alert-danger");
                return;
            }

            if (response.status === 403) {
                setStatus("You are no longer allowed to update this TeamValue.", "alert-danger");
                return;
            }

            if (response.status === 404) {
                setStatus("This team could not be found anymore.", "alert-danger");
                return;
            }

            setStatus(`Update failed with status ${response.status}.`, "alert-danger");
        } catch (_error) {
            setStatus("Update failed due to a network error. Please try again.", "alert-danger");
        } finally {
            setBusy(false);
            updateReadout(teamValueInput.value.trim());
        }
    });

    teamValueInput?.addEventListener("input", () => {
        if (statusBox && statusBox.classList.contains("alert-success")) {
            statusBox.hidden = true;
            statusBox.textContent = "";
        }
    });

    if (teamValueInput && originalValue === "") {
        updateReadout("");
    }
}
