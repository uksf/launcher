export const AppConfig = {
    production: true,
    environment: 'PROD',
    apiUrl: 'https://api.uk-sf.co.uk',
    defaults: {
        theme: 'dark',
        setupDone: false,
        autoUpdate: true,
        gameLocation: undefined,
        modsLocation: undefined,
        profile: undefined,
        startup: {
            noSplash: true,
            scriptErrors: false,
            hugePages: false,
            malloc: 'System Default',
            filePatching: false
        }
    }
};
