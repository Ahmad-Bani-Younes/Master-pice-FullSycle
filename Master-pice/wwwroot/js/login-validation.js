// login-validation.js

document.addEventListener("DOMContentLoaded", function () {
    setTimeout(function () {
        var success = document.getElementById('successMessage');
        var error = document.getElementById('errorMessage');
        if (success) {
            success.classList.remove('show');
            success.classList.add('hide');
        }
        if (error) {
            error.classList.remove('show');
            error.classList.add('hide');
        }
    }, 3000);
});

function validateLoginForm() {
    const email = document.getElementById("Email")?.value.trim();
    const password = document.getElementById("Password")?.value.trim();

    if (!email) {
        Swal.fire({
            icon: 'warning',
            title: 'Email Required',
            text: 'Please enter your email address.'
        });
        return false;
    }

    const emailPattern = /^[^\s@]+@[^\s@]+\.[^\s@]+$/;
    if (!emailPattern.test(email)) {
        Swal.fire({
            icon: 'error',
            title: 'Invalid Email',
            text: 'Please enter a valid email address.'
        });
        return false;
    }

    if (!password) {
        Swal.fire({
            icon: 'warning',
            title: 'Password Required',
            text: 'Please enter your password.'
        });
        return false;
    }

    // ✅ كل شي تمام → أرسل الفورم
    document.getElementById("loginForm").submit();
}
