document.addEventListener("DOMContentLoaded", function () {
    const form = document.getElementById("login-form");
    if (!form) return;

    const idInput = document.getElementById("Identifier");
    const passInput = document.getElementById("Password");
    const btnLogin = document.getElementById("btnLogin");
    const capsHint = document.getElementById("capsHint");

    function toggleButton() {
        const ok = idInput.value.trim().length > 0 && passInput.value.length > 0;
        btnLogin.disabled = !ok;
    }

    // Caps Lock uyarısı
    function handleCaps(e) {
        const caps = e.getModifierState && e.getModifierState("CapsLock");
        if (caps) { capsHint.style.display = "inline"; }
        else { capsHint.style.display = "none"; }
    }

    // Eventler
    idInput.addEventListener("input", toggleButton);
    passInput.addEventListener("input", toggleButton);
    passInput.addEventListener("keyup", handleCaps);
    passInput.addEventListener("focus", () => { capsHint.style.display = "none"; });
    passInput.addEventListener("blur", () => { capsHint.style.display = "none"; });

    // İlk yüklemede buton durumu
    toggleButton();
});
