document.getElementById("sign-up-form").addEventListener("submit", async function (e) {

    e.preventDefault();
    document.getElementById("signInErrorMessage").style.display = "none";

    await signUp();
});

async function signUp() {
    try {
        const email = document.getElementById("emailInput").value;
        const password = document.getElementById("passwordInput").value;
        const username = document.getElementById("usernameInput").value;

        const response = await fetch(`${apiBaseUrl}/auth/sign-up`, {
            method: "POST",
            headers: {
                "Content-Type": "application/json",
            },
            body: JSON.stringify({ username, password, email }),
        });

        if (response.status === 200) {
            window.location = "sign-in.html";
        }
        else if (response.status === 400) {
            const responseData = await response.text();

            const signInErrorMessageElement = document.getElementById("signInErrorMessage");
            signInErrorMessageElement.textContent = responseData;
            signInErrorMessageElement.style.display = "block";
        }
        else {
            const data = await response.json();
            console.log(data);
        }
    }
    catch (e) {
        console.error(e);
    }
}