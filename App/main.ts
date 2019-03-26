import { app, BrowserWindow } from 'electron';
import * as path from 'path';
import * as url from 'url';
import * as settings from 'electron-settings';
import * as log from 'electron-log';

let win: BrowserWindow;
let serve;
const args = process.argv.slice(1);
serve = args.some(val => val === '--serve');

const defaults = {
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
    },
    login: {
        email: undefined,
        password: undefined
    }
};

function createWindow() {
    win = new BrowserWindow({
        width: 500,
        height: 500,
        webPreferences: {
            nodeIntegration: true,
        },
        frame: false,
        center: true,
        fullscreenable: false,
        maximizable: false,
        resizable: false,
        show: false,
        icon: path.join(__dirname, 'src/icon.ico')
    });

    log.info(`Reading settings from '${settings.file()}'`);
    if (Object.keys(settings.getAll()).length === 0) {
        log.info(`No settings found, setting defaults`);
        settings.setAll(defaults);
    }

    const settingsAll = settings.getAll();
    Object.keys(settings.getAll()).forEach((key) => {
        settings.watch(key, newValue => {
            log.info(`Setting '${key}' updated to '${JSON.stringify(newValue)}'`);
        });
    });
    Object.keys(defaults).forEach((key) => {
        if (!settingsAll.hasOwnProperty(key)) {
            settings.set(key, defaults[key]);
        }
    });
    log.info(`Settings read`);

    if (serve) {
        require('electron-reload')(__dirname, {
            electron: require(`${__dirname}/node_modules/electron`)
        });
        win.loadURL('http://localhost:4300');
    } else {
        win.loadURL(url.format({
            pathname: path.join(__dirname, 'dist/index.html'),
            protocol: 'file:',
            slashes: true
        }));
    }

    if (serve) {
        win.webContents.openDevTools();
    }

    win.once('ready-to-show', () => {
        log.info(`Showing window`);
        win.show();
    });

    win.on('closed', () => {
        win = null;
        log.info('Stopping launcher');
    });
}

try {
    if (!app.requestSingleInstanceLock()) {
        app.quit();
    } else {
        log.transports.file.init();
        log.transports.file.clear();
        log.transports.file.level = 'debug';
        log.catchErrors();
        log.info('Starting launcher');

        app.commandLine.appendSwitch('js-flags', '--max-old-space-size=8192');

        app.on('ready', createWindow);

        app.on('second-instance', () => {
            win.focus();
        });

        app.on('window-all-closed', () => {
            if (process.platform !== 'darwin') {
                app.quit();
            }
        });

        app.on('activate', () => {
            if (win === null) {
                createWindow();
            }
        });
    }
} catch (e) {
    log.error(e);
}
