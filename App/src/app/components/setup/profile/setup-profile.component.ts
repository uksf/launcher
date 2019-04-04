import { Component, NgZone, Output, EventEmitter, OnInit } from '@angular/core';
import { ElectronService } from '../../../services/electron.service';
import { FormBuilder, Validators, FormGroup } from '@angular/forms';
import * as log from 'electron-log';
import * as settings from 'electron-settings';
import { ModsLocationService } from '../../../services/modslocation.service';
import * as path from 'path';

const instructionFound = 'We have selected the Arma 3 profile we think you use for UKSF.'
    + '\n\nIf this is incorrect, select the profile you wish to use from the list below.'
    + '\n\nAlternatively, select a profile from the list below and press \'Copy\' to create a new profile and copy your game settings from the selected profile.'
    + '\n\nOtherwise, press \'Next\'';

const instructionNotFound = 'We can\'t find an Arma 3 profile we think you use for UKSF.'
    + '\n\nSelect a profile from the list below and press \'Copy\' to create a new profile and copy your game settings from the selected profile.'
    + '\n\nAlternatively, select a profile you wish to use from the list below. (Not recommended)';

@Component({
    selector: 'app-setup-profile',
    templateUrl: './setup-profile.component.html',
    styleUrls: ['../../../pages/setup/setup.component.scss', './setup-profile.component.scss']
})
export class SetupProfileComponent implements OnInit {
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
            profile: ['', Validators.required]
        });
    }

    ngOnInit() {
        if (settings.has('profile')) {
            const profile = settings.get('profile').toString();
            // this.form.controls['profile'].setValue(profile);

        } else {
            if (settings.has('gameLocation')) {
                const location = path.dirname(settings.get('gameLocation').toString());
                // .form.controls['location'].setValue(location);

            } else {
                log.info('Could not find Arma 3 location');
                this.instruction = instructionNotFound;
                this.blocking.emit(this.block);
            }
        }
    }
}
