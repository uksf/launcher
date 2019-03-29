import { Injectable } from '@angular/core';
import { ipcRenderer, webFrame, remote, clipboard, dialog } from 'electron';
import * as childProcess from 'child_process';
import * as fs from 'fs';
import * as path from 'path';
import * as readline from 'readline';
import * as edge from 'electron-edge-js';

@Injectable()
export class ElectronService {
    ipcRenderer: typeof ipcRenderer;
    webFrame: typeof webFrame;
    remote: typeof remote;
    dialog: typeof dialog;
    childProcess: typeof childProcess;
    fs: typeof fs;
    path: typeof path;
    edge: typeof edge;
    clipboard: typeof clipboard;
    extraResourcesPath: string;
    readline: typeof readline;

    constructor() {
        // Conditional imports
        if (this.isElectron()) {
            this.ipcRenderer = window.require('electron').ipcRenderer;
            this.webFrame = window.require('electron').webFrame;
            this.remote = window.require('electron').remote;
            this.dialog = this.remote.dialog;
            const appPath = this.remote.app.getAppPath();
            this.extraResourcesPath = appPath.endsWith('app.asar') ? `${appPath.substr(0, appPath.length - 8)}./Library/` : './Library/';
            this.clipboard = window.require('electron').clipboard;
            this.childProcess = window.require('child_process');
            this.fs = window.require('fs');
            this.path = window.require('path');
            this.edge = window.require('electron-edge-js');
            this.readline = window.require('readline');
        }
    }

    isElectron = () => {
        return window && window.process && window.process.type;
    }
}
