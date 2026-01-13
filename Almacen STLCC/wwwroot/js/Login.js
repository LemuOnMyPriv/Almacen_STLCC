const passwordInput = document.getElementById("Password");
const toggle = document.getElementById("togglePassword");

toggle.addEventListener("mousedown", () => {
    passwordInput.type = "text";
    toggle.classList.remove("fa-eye");
    toggle.classList.add("fa-eye-slash");
});

toggle.addEventListener("mouseup", () => {
    passwordInput.type = "password";
    toggle.classList.remove("fa-eye-slash");
    toggle.classList.add("fa-eye");
});

toggle.addEventListener("mouseleave", () => {
    passwordInput.type = "password";
    toggle.classList.remove("fa-eye-slash");
    toggle.classList.add("fa-eye");
});

toggle.addEventListener("touchstart", () => {
    passwordInput.type = "text";
    toggle.classList.remove("fa-eye");
    toggle.classList.add("fa-eye-slash");
});

toggle.addEventListener("touchend", () => {
    passwordInput.type = "password";
    toggle.classList.remove("fa-eye-slash");
    toggle.classList.add("fa-eye");
});
