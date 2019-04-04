import { Injectable } from '@angular/core';
import { FilesService } from './files.service';
import { ErrorState } from '../types';

@Injectable()
export class ArmaExeService {
    constructor(private filesService: FilesService) { }

    async checkExe(path: string): Promise<ErrorState> {
        if (path === '') {
            return new ErrorState('Please select an Arma 3 exe');
        }

        if (await this.filesService.exists(path)) {
            if (!path.includes('.exe')) {
                return new ErrorState('File is not an exe');
            }
            if (!path.includes('arma')) {
                return new ErrorState('File is not an Arma 3 exe');
            }
            if (path.includes('battleye')) {
                return new ErrorState('File cannot be a battleye exe');
            }
            if (path.includes('launcher')) {
                return new ErrorState('File cannot be a launcher exe');
            }
            if (path.includes('server')) {
                return new ErrorState('File cannot be a server exe');
            }
            const is64Bit = process.env.PROCESSOR_ARCHITECTURE === 'AMD64';
            if (is64Bit && !path.includes('x64')) {
                return new ErrorState('We recommend using the \'arma3_x64\' exe', false);
            }
        } else {
            return new ErrorState('File does not exist');
        }

        return new ErrorState('', false);
    }
}
