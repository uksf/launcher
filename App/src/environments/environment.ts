export const AppConfig = {
    production: false,
    environment: 'LOCAL',
    apiUrl: 'http://localhost:5000',
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
