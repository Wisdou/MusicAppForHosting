document.getElementById("sign-in-form").addEventListener("submit", async function (e) {

    e.preventDefault();
    document.getElementById("signInErrorMessage").style.display = "none";

    await signIn();
});

function togglePasswordVisibility() {
    const passwordInput = document.getElementById('passwordInput');
    const showPasswordCheckbox = document.getElementById('showPasswordCheckbox');
    passwordInput.type = showPasswordCheckbox.checked ? "text" : "password";
}

async function signIn() {
    const emailOrUsername = document.getElementById("emailOrUsernameInput").value;
    const password = document.getElementById("passwordInput").value;

    const body = JSON.stringify({ emailOrUsername, password });

    try {
        const response = await fetch(`${apiBaseUrl}/auth/sign-in?useCookies=true&rememberMe=true`, {
            method: "POST",
            headers: {
                "Content-Type": "application/json",
            },
            body: body,
            credentials: "include",
        });

        if (response.ok) {
            window.location = "index.html";
        }
        else if (response.status === 400) {
            const responseData = await response.text();

            const signInErrorMessageElement = document.getElementById("signInErrorMessage");
            signInErrorMessageElement.textContent = responseData;
            signInErrorMessageElement.style.display = "block";
        }
    }
    catch (error) {
        console.error(error);
    }
}