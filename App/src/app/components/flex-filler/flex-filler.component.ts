import { Component } from '@angular/core';

@Component({
    selector: 'app-flex-filler',
    template: '',
    // tslint:disable-next-line:use-host-property-decorator
    host: {
        '[style.flex]': '1'
    }
})
export class FlexFillerComponent {
    constructor() { }
}
