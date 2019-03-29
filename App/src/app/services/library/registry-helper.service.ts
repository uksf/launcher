import { Injectable } from '@angular/core';
import { LibraryService } from './library.service';

@Injectable()
export class RegistryHelper {
    constructor(private libraryService: LibraryService) { }

    getArmaInstallLocation = this.libraryService.createZonedLibraryFunction('RegistryHelper', 'GetArmaInstallLocation');
}
