// The file contents for the current environment will overwrite these during build.
// The build system defaults to the dev environment which uses `index.ts`, but if you do
// `ng build --env=prod` then `index.prod.ts` will be used instead.
// The list of which env maps to which file can be found in `.angular-cli.json`.

export const AppConfig = {
    production: false,
    environment: 'DEV',
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
