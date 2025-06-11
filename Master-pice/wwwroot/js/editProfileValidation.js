document.addEventListener("DOMContentLoaded", function () {
    const form = document.getElementById('editProfileForm');
    const errorBox = document.getElementById('validationErrors');

    if (!form) return;

    form.addEventListener('submit', function (e) {
        const fullName = document.querySelector('[name="FullName"]').value.trim();
        const email = document.querySelector('[name="Email"]').value.trim();
        const phone = document.querySelector('[name="Phone"]').value.trim();
        const region = document.querySelector('[name="Region"]').value.trim();

        const phoneRegex = /^(077|078|079)\d{7}$/;
        const emailRegex = /^[^\s@]+@[^\s@]+\.[^\s@]+$/;

        let errors = [];

        if (!fullName) errors.push("Full Name is required.");
        if (!email || !emailRegex.test(email)) errors.push("Enter a valid email.");
        if (!phone || !phoneRegex.test(phone)) errors.push("Enter a valid Jordanian phone number (10 digits).");
        if (!region) errors.push("Please select your region.");

        if (errors.length > 0) {
            e.preventDefault();

            errorBox.innerHTML = errors.map(e => `<li>${e}</li>`).join('');
            errorBox.classList.remove("d-none");
            errorBox.classList.add("show");

            setTimeout(() => {
                errorBox.classList.add("d-none");
            }, 5000);
        } else {
            errorBox.classList.add("d-none");
        }
    });
});
