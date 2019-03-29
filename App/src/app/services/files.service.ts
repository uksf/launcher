import { Injectable } from '@angular/core';
import { ElectronService } from './electron.service';

@Injectable()
export class FilesService {
    constructor(private electronService: ElectronService) { }

    async exists(path: string): Promise<Boolean> {
        return new Promise((resolve, reject) => {
            this.electronService.fs.stat(path, (_, stats) => {
                if (stats && stats.isFile()) {
                    resolve(true);
                } else {
                    reject(false);
                }
            });
        });
    }
}
