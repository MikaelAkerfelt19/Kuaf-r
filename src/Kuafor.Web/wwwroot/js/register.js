document.addEventListener("DOMContentLoaded", function () {
    const form = document.getElementById("register-form");
    if (!form) return;

    // Telefon: sadece rakam ve 10 hane
    const phone = form.querySelector("#Phone");
    if (phone) {
        phone.setAttribute("maxlength", "10");
        phone.setAttribute("inputmode", "numeric");
        phone.addEventListener("input", function (e) {
            e.target.value = e.target.value.replace(/\D/g, "").slice(0, 10);
        });
    }

    // jQuery validation kurallarını blur ile tetiklemek
    if (window.jQuery && $.validator) {
        $.validator.setDefaults({
            onkeyup: false,
            onclick: false,
            onfocusout: function (element) {
                this.element(element);
            }
        });

        // Formu yeniden parse et
        $(form).removeData("validator").removeData("unobtrusiveValidation");
        $.validator.unobtrusive.parse(form);

        // Tüm input, select, textarea blur olduğunda kontrol et
        $(form)
            .on("blur", "input, textarea, select", function () {
                $(form).validate().element(this);
            })
            .on("change", "input[type=checkbox], select", function () {
                $(form).validate().element(this);
            });

        // Submit öncesi formu kontrol et
        $(form).on("submit", function (e) {
            if (!$(this).valid()) e.preventDefault();
        });
    }

    // ---- Ekstra özel kontroller (örnek: Email & Şifre Gücü) ----

    // E-posta kontrolü (regex ile)
    const emailInput = form.querySelector("#Email");
    const emailError = document.querySelector("[data-valmsg-for='Email']");
    if (emailInput && emailError) {
        emailInput.addEventListener("blur", function () {
            const email = emailInput.value.trim();
            const pattern = /^[^ ]+@[^ ]+\.[a-z]{2,}$/i;
            if (email && !pattern.test(email)) {
                emailError.textContent = "Geçerli bir e-posta adresi giriniz.";
            } else {
                emailError.textContent = "";
            }
        });
    }

    // Şifre: 6+ karakter, en az 1 harf ve 1 rakam
    const passInput = form.querySelector("#Password");
    const passError = document.querySelector("[data-valmsg-for='Password']");
    if (passInput && passError) {
        passInput.addEventListener("blur", function () {
            const pass = passInput.value;
            const pattern = /^(?=.*[A-Z])(?=.*\d)(?=.*[!@#$%^&*])[A-Za-z\d!@#$%^&*]{6,}$/;
            if (pass && !pattern.test(pass)) {
                passError.textContent = "Şifre en az 6 karakter olmalı, en az 1 büyük harf, 1 sayı içermelidir ve 1 özel harf içermelidir.";
            } else {
                passError.textContent = "";
            }
        });
    }

    // Şifre tekrar kontrolü
    const confirmPass = form.querySelector("#ConfirmPassword");
    const confirmError = document.querySelector("[data-valmsg-for='ConfirmPassword']");
    if (confirmPass && confirmError) {
        confirmPass.addEventListener("blur", function () {
            if (confirmPass.value !== passInput.value) {
                confirmError.textContent = "Şifreler eşleşmiyor.";
            } else {
                confirmError.textContent = "";
            }
        });
    }

    pass.addEventListener("keyup", function (e) {
        const caps = e.getModifierState && e.getModifierState("CapsLock");
        pass.title = caps ? "Caps Lock açık görünüyor." : "";
    });
});
