import { Injectable, EventEmitter } from '@angular/core';
import * as log from 'electron-log';
import * as settings from 'electron-settings';

@Injectable()
export class ThemeService {
    themeUpdatedEvent = new EventEmitter<null>();
    theme = 'dark';
    otherTheme = 'light';

    constructor() {
        const storedTheme = settings.get('theme').toString();
        this.updateTheme(storedTheme);
        // HeaderBarComponent.themeUpdateEvent = new EventEmitter();
        // HeaderBarComponent.themeUpdateEvent.subscribe(() => {
        //     this.updateTheme(this.otherTheme);
        // });
    }

    updateTheme(newTheme = 'dark') {
        this.theme = newTheme;
        if (this.theme === this.otherTheme) {
            settings.set('theme', this.theme);
        }
        log.info(`Loading '${this.theme}' theme`);
        this.updateOtherTheme();
        this.updateThemeSubscribers();
    }

    updateThemeSubscribers() {
        // HeaderBarComponent.otherTheme = this.otherTheme;
        this.themeUpdatedEvent.emit();
    }

    updateOtherTheme() {
        this.otherTheme = this.theme === 'light' ? 'dark' : 'light';
    }
}
