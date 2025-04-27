function validateRegisterForm() {
    const fullName = document.getElementById("FullName")?.value.trim();
    const phone = document.getElementById("Phone")?.value.trim();
    const email = document.getElementById("Email")?.value.trim();
    const password = document.getElementById("Password")?.value.trim();
    const confirmPassword = document.getElementById("ConfirmPassword")?.value.trim();
    const region = document.getElementById("Region")?.value.trim();

    if (!fullName) {
        Swal.fire({ icon: 'warning', title: 'Full Name Required', text: 'Please enter your full name.' });
        return false;
    }

    if (!phone) {
        Swal.fire({ icon: 'warning', title: 'Phone Required', text: 'Please enter your phone number.' });
        return false;
    }

    if (!/^\d{8,15}$/.test(phone)) {
        Swal.fire({ icon: 'error', title: 'Invalid Phone', text: 'Phone number must be between 8 and 15 digits.' });
        return false;
    }

    if (!email) {
        Swal.fire({ icon: 'warning', title: 'Email Required', text: 'Please enter your email address.' });
        return false;
    }

    const emailPattern = /^[^\s@]+@[^\s@]+\.[^\s@]+$/;
    if (!emailPattern.test(email)) {
        Swal.fire({ icon: 'error', title: 'Invalid Email', text: 'Please enter a valid email address.' });
        return false;
    }

    if (!password) {
        Swal.fire({ icon: 'warning', title: 'Password Required', text: 'Please enter a password.' });
        return false;
    }

    const strongPasswordPattern = /^(?=.*[a-z])(?=.*[A-Z])(?=.*\d)(?=.*[\W_]).{8,}$/;
    if (!strongPasswordPattern.test(password)) {
        Swal.fire({
            icon: 'error',
            title: 'Weak Password',
            html: 'Password must be at least 8 characters and include:<br>- Uppercase letter<br>- Lowercase letter<br>- Number<br>- Special character (!@#$%^&*)'
        });
        return false;
    }

    if (!confirmPassword) {
        Swal.fire({ icon: 'warning', title: 'Confirm Password Required', text: 'Please confirm your password.' });
        return false;
    }

    if (password !== confirmPassword) {
        Swal.fire({ icon: 'error', title: 'Password Mismatch', text: 'Password and Confirm Password do not match.' });
        return false;
    }

    if (!region) {
        Swal.fire({ icon: 'warning', title: 'Region Required', text: 'Please select your region.' });
        return false;
    }

    document.querySelector('form').submit();
}

// 🟢 دالة حساب قوة الباسورد وعرضها
document.addEventListener("DOMContentLoaded", function () {
    const passwordInput = document.getElementById("Password");
    const strengthBar = document.getElementById("password-strength-bar");
    const strengthText = document.getElementById("password-strength-text");

    passwordInput.addEventListener("input", function () {
        const value = passwordInput.value;
        let strength = 0;

        if (value.length >= 8) strength++;
        if (/[a-z]/.test(value)) strength++;
        if (/[A-Z]/.test(value)) strength++;
        if (/\d/.test(value)) strength++;
        if (/[\W_]/.test(value)) strength++;

        switch (strength) {
            case 0:
            case 1:
            case 2:
                strengthBar.style.width = "30%";
                strengthBar.className = "progress-bar bg-danger";
                strengthText.innerText = "Weak";
                break;
            case 3:
            case 4:
                strengthBar.style.width = "70%";
                strengthBar.className = "progress-bar bg-warning";
                strengthText.innerText = "Medium";
                break;
            case 5:
                strengthBar.style.width = "100%";
                strengthBar.className = "progress-bar bg-success";
                strengthText.innerText = "Strong";
                break;
        }
    });
});
