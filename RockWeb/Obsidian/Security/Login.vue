<template>
    <div class="login-block obsidian-block">
        <fieldset>
            <legend>Login</legend>

            <div class="alert alert-danger" v-if="errorMessage" v-html="errorMessage"></div>

            <form @submit.prevent="submitLogin">
                <rock-textbox label="Username" v-model="username"></rock-textbox>
                <rock-textbox label="Password" v-model="password" type="password"></rock-textbox>
                <rock-checkbox label="Keep me logged in" v-model="rememberMe"></rock-checkbox>
                <rock-button :is-loading="isLoading" loading-text="Logging In..." label="Log In" class="btn btn-primary" @click="submitLogin" type="submit"></rock-button>
            </form>

            <rock-button :is-loading="isLoading" label="Forgot Account" class="btn btn-link" @click="onHelpClick"></rock-button>

        </fieldset>
    </div>
</template>

<script>
    const { blockAction } = Obsidian.Blocks['Security/Login'];

    const setCookie = (cookie) => {
        let expires = '';

        if (cookie.Expires) {
            const date = new Date(cookie.Expires);

            if (date < new Date()) {
                expires = '';
            }
            else {
                expires = `; expires=${date.toGMTString()}`;
            }
        }
        else {
            expires = '';
        }

        document.cookie = `${cookie.Name}=${cookie.Value}${expires}; path=/`;
    };

    const redirectAfterLogin = () => {
        const urlParams = new URLSearchParams(window.location.search);
        const returnUrl = urlParams.get('returnurl');

        // TODO make this force relative URLs (no absolute URLs)
        window.location.href = decodeURIComponent(returnUrl);
    };

    export default {
        name: 'Security_Login',
        data() {
            return {
                username: '',
                password: '',
                rememberMe: false,
                isLoading: false,
                errorMessage: ''
            };
        },
        methods: {
            async onHelpClick() {
                this.isLoading = true;
                this.errorMessage = '';

                try {
                    const result = await blockAction('help');
                    const url = result.data;

                    if (!url) {
                        this.errorMessage = 'An unknown error occurred communicating with the server';
                    }
                    else {
                        // TODO make this force relative URLs (no absolute URLs)
                        window.location.href = url;
                    }
                }
                catch (e) {
                    this.errorMessage = `An exception occurred: ${e}`;
                }
                finally {
                    this.isLoading = false;
                }
            },
            async submitLogin() {
                this.isLoading = true;
                this.errorMessage = '';

                try {
                    const result = await blockAction('login', {
                        username: this.username,
                        password: this.password,
                        rememberMe: this.rememberMe
                    });

                    if (result.data.AuthCookie) {
                        setCookie(result.data.AuthCookie);
                        redirectAfterLogin();
                    }
                    else {
                        this.errorMessage = 'Authentication seemed to succeed, but the server did not generate a cookie';
                        this.isLoading = false;
                    }
                }
                catch (e) {
                    if (typeof e === 'string') {
                        this.errorMessage = e;
                    }
                    else {
                        this.errorMessage = `An exception occurred: ${e}`;
                    }

                    this.isLoading = false;
                }
            }
        }
    };
</script>
