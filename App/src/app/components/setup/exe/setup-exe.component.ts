import { Component, NgZone, Output, EventEmitter } from '@angular/core';
import { ElectronService } from '../../../services/electron.service';
import { RegistryHelper } from '../../../services/library/registry-helper.service';
import { FormBuilder, Validators, FormGroup } from '@angular/forms';
import * as log from 'electron-log';
import { ArmaExeService } from '../../../services/armaexe.service';
import { FilesService } from '../../../services/files.service';
import * as settings from 'electron-settings';

const instructionFound = 'We found your Arma 3 installation and chose the best exe for you.'
    + '\n\nIf you are not happy with this, select the Arma 3 exe you wish to use.'
    + '\n\nOtherwise, press \'Next\'';

const instructionNotFound = 'We can\'t find your Arma 3 installation.'
    + '\n\nThis is unusual, so you should check the game is installed in Steam.'
    + '\n\nYou can continue by selecting the Arma 3 exe you wish to use manually. (Not recommended)';

@Component({
    selector: 'app-setup-exe',
    templateUrl: './setup-exe.component.html',
    styleUrls: ['../../../pages/setup/setup.component.scss', './setup-exe.component.scss']
})
export class SetupExeComponent {
    @Output() blocking = new EventEmitter<boolean>();
    private gameLocation = '';
    instruction = instructionFound;
    form: FormGroup;
    errorMessage = '';
    block = true;

    constructor(
        private electronService: ElectronService,
        private registryHelper: RegistryHelper,
        private armaExeService: ArmaExeService,
        private filesService: FilesService,
        private zone: NgZone,
        formBuilder: FormBuilder
    ) {
        this.form = formBuilder.group({
            path: ['', Validators.required]
        });
        this.registryHelper.getArmaInstallLocation({}, async (_, result: string) => {
            this.gameLocation = result;
            if (settings.has('gameLocation')) {
                const path = settings.get('gameLocation').toString();
                this.form.controls['path'].setValue(path);
                this.checkExe(path);
            } else {
                if (this.gameLocation) {
                    log.info(`Found Arma 3 location: '${this.gameLocation}'`);
                    this.findExe();
                } else {
                    log.info('Could not find Arma 3 location');
                    this.instruction = instructionNotFound;
                    this.blocking.emit(this.block);
                }
            }
        });
    }

    browse() {
        this.electronService.dialog.showOpenDialog({
            title: 'Arma 3 Exe',
            defaultPath: this.gameLocation,
            properties: ['openFile'],
            filters: [
                { name: 'Exe Files', extensions: ['exe'] }
            ]
        }, (files) => {
            if (files.length === 0) { return; }
            const path = files[0];
            this.form.controls['path'].setValue(path);
            this.checkExe(path);
        });
    }

    async checkExe(path: string) {
        const state = await this.armaExeService.checkExe(path);
        if (!state.block) {
            settings.set('gameLocation', path);
        }
        this.zone.run(() => {
            this.errorMessage = state.message;
            this.block = state.block;
            this.blocking.emit(this.block);
        });
    }

    async findExe() {
        const is64Bit = process.env.PROCESSOR_ARCHITECTURE === 'AMD64';
        const exePath = this.electronService.path.join(this.gameLocation, is64Bit ? 'arma3_x64.exe' : 'arma3.exe');
        if (await this.filesService.exists(exePath)) {
            this.form.controls['path'].setValue(exePath);
            this.instruction = instructionFound;
            this.checkExe(exePath);
        }
    }
}
