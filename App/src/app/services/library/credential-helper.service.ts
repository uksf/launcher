import { Injectable } from '@angular/core';
import { LibraryService } from './library.service';

@Injectable()
export class CredentialHelper {
    constructor(private libraryService: LibraryService) { }

    encrypt = this.libraryService.createZonedLibraryFunction('CredentialHelper', 'Encrypt');
    decrypt = this.libraryService.createZonedLibraryFunction('CredentialHelper', 'Decrypt');
}
