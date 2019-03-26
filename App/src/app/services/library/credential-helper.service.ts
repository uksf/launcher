import { Injectable } from '@angular/core';
import { ElectronService } from '../electron.service';

@Injectable()
export class CredentialHelper {
    constructor(private electronService: ElectronService) { }

    encrypt = this.electronService.edge.func({
        assemblyFile: this.electronService.extraResourcesPath + 'Library.dll',
        typeName: 'Library.CredentialHelper',
        methodName: 'Encrypt'
    });

    decrypt = this.electronService.edge.func({
        assemblyFile: this.electronService.extraResourcesPath + 'Library.dll',
        typeName: 'Library.CredentialHelper',
        methodName: 'Decrypt'
    });
}
