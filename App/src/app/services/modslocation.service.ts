import { Injectable } from '@angular/core';
import { HttpClient } from '@angular/common/http';
import { AppConfig } from '../../environments/environment';
import { FileHelper } from './library/file-helper.service';
import { ErrorState } from '../types';
import * as log from 'electron-log';

@Injectable()
export class ModsLocationService {
    private fallbackSize = '37580963840'; // Default size ~35GB as fallback

    constructor(private httpClient: HttpClient, private fileHelper: FileHelper) { }

    async getModpackSize(): Promise<string> {
        return new Promise((resolve) => {
            this.httpClient.get(`${AppConfig.apiUrl}/mods/size`).subscribe((response: string) => {
                resolve(response);
            }, _ => {
                log.info(`Failed to get modpack space required. Using fallback: '${this.fallbackSize}'`);
                resolve(this.fallbackSize);
            });
        });
    }

    async checkSize(path: string): Promise<ErrorState> {
        if (path === '') {
            return new ErrorState('Please select a folder');
        }

        return new Promise(async (resolve) => {
            const requiredSpace = await this.getModpackSize();
            this.fileHelper.getDriveSpace(path, (_, result: string) => {
                if (result <= requiredSpace) {
                    resolve(new ErrorState('Not enough drive space'));
                } else {
                    resolve(new ErrorState('', false));
                }
            });
        });
    }
}
