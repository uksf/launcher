import { Injectable, NgZone } from '@angular/core';
import { ElectronService } from '../electron.service';

@Injectable()
export class LibraryService {
    constructor(private electronService: ElectronService, private zone: NgZone) { }

    createLibraryFunction(typeName: string, methodName: string) {
        return this.electronService.edge.func({
            assemblyFile: this.electronService.extraResourcesPath + 'Library.dll',
            typeName: `Library.${typeName}`,
            methodName: methodName
        });
    }

    createZonedLibraryFunction(typeName: string, methodName: string) {
        const func = this.createLibraryFunction(typeName, methodName);
        return (payload: {}, callback: (error: Error, result: any) => void) => {
            func(payload, (error, result) => {
                this.zone.run(() => {
                    callback(error, result);
                });
            });
        };
    }
}
