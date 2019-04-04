import { Injectable } from '@angular/core';
import { LibraryService } from './library.service';

@Injectable()
export class FileHelper {
    constructor(private libraryService: LibraryService) { }

    getDriveSpace = this.libraryService.createLibraryFunction('FileHelper', 'GetDriveSpace');
}
