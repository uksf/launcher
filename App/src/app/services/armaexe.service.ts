import { Injectable } from '@angular/core';
import { ElectronService } from './electron.service';
import { FilesService } from './files.service';

@Injectable()
export class ArmaExeService {
    constructor(private electronService: ElectronService, private filesService: FilesService) { }

    async checkExe(path: string): Promise<ArmaExeState> {
        if (path === '') {
            return new ArmaExeState('Please select an Arma 3 exe');
        }

        if (await this.filesService.exists(path)) {
            if (!path.includes('.exe')) {
                return new ArmaExeState('File is not an exe');
            }
            if (!path.includes('arma')) {
                return new ArmaExeState('File is not an Arma 3 exe');
            }
            if (path.includes('battleye')) {
                return new ArmaExeState('File cannot be a battleye exe');
            }
            if (path.includes('launcher')) {
                return new ArmaExeState('File cannot be a launcher exe');
            }
            if (path.includes('server')) {
                return new ArmaExeState('File cannot be a server exe');
            }
            const is64Bit = process.env.PROCESSOR_ARCHITECTURE === 'AMD64';
            if (is64Bit && !path.includes('x64')) {
                return new ArmaExeState('We recommend using the \'arma3_x64\' exe', false);
            }
        } else {
            return new ArmaExeState('File does not exist');
        }

        return new ArmaExeState('', false);
    }
}

export class ArmaExeState {
    constructor(public message: string, public block: boolean = true) { }
}
