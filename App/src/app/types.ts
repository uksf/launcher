export class ErrorState {
    constructor(public message: string, public block: boolean = true) { }
}

export class Profile {
    public name: string;
    public path: string;
    public displayName: string;

    constructor(name: string) { }
}
