import { Component, NgZone, Output, EventEmitter, OnInit } from '@angular/core';
import { ElectronService } from '../../../services/electron.service';
import { FormBuilder, Validators, FormGroup } from '@angular/forms';
import * as log from 'electron-log';
import * as settings from 'electron-settings';
import { ModsLocationService } from '../../../services/modslocation.service';
import * as path from 'path';

const instructionFound = 'We have selected your Arma 3 install location as your mod download location.'
    + '\n\nIf you wish to change this, select the folder you wish to use below.'
    + '\n\nOtherwise, press \'Next\'';

const instructionNotFound = 'We can\'t find your Arma 3 installation.'
    + '\n\nThis is unusual, so you should check the game is installed in Steam.'
    + '\n\nYou can continue by selecting the mods download location you wish to use manually. (Not recommended)';

@Component({
    selector: 'app-setup-mods',
    templateUrl: './setup-mods.component.html',
    styleUrls: ['../../../pages/setup/setup.component.scss', './setup-mods.component.scss']
})
export class SetupModsComponent implements OnInit {
    @Output() blocking = new EventEmitter<boolean>();
    private gameLocation = '';
    instruction = instructionFound;
    form: FormGroup;
    errorMessage = '';
    block = true;

    constructor(
        private electronService: ElectronService,
        private modsLocationService: ModsLocationService,
        private zone: NgZone,
        formBuilder: FormBuilder
    ) {
        this.form = formBuilder.group({
            location: ['', Validators.required]
        });
    }

    ngOnInit() {
        if (settings.has('modsLocation')) {
            const location = settings.get('modsLocation').toString();
            this.form.controls['location'].setValue(location);
            this.checkSize(location);
        } else {
            if (settings.has('gameLocation')) {
                const location = path.dirname(settings.get('gameLocation').toString());
                this.form.controls['location'].setValue(location);
                this.checkSize(location);
            } else {
                log.info('Could not find Arma 3 location');
                this.instruction = instructionNotFound;
                this.blocking.emit(this.block);
            }
        }
    }

    browse() {
        this.electronService.dialog.showOpenDialog({
            title: 'Arma 3 Exe',
            defaultPath: this.gameLocation,
            properties: ['openDirectory']
        }, (files) => {
            if (files.length === 0) { return; }
            const location = files[0];
            this.form.controls['location'].setValue(location);
            this.checkSize(location);
        });
    }

    async checkSize(location: string) {
        const state = await this.modsLocationService.checkSize(location);
        if (!state.block) {
            settings.set('modsLocation', location);
        }
        this.zone.run(() => {
            this.errorMessage = state.message;
            this.block = state.block;
            this.blocking.emit(this.block);
        });
    }
}
