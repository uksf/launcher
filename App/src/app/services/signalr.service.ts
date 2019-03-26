import { Injectable, EventEmitter } from '@angular/core';
import { HubConnectionBuilder, HubConnectionState, HttpTransportType, LogLevel, HubConnection } from '@aspnet/signalr';
import { SessionService } from './session.service';
import { AppConfig } from '../../environments/environment';

@Injectable()
export class SignalRService {
    constructor(private sessionService: SessionService) { }

    connect(endpoint: String): ConnectionContainer {
        const reconnectEvent = new EventEmitter();
        const connection = new HubConnectionBuilder().withUrl(`${AppConfig.apiUrl}/hub/${endpoint}`, {
            accessTokenFactory: () => {
                return this.sessionService.getToken();
            }, transport: HttpTransportType.ServerSentEvents, logger: LogLevel.Warning
        }).build();
        this.waitForConnection(connection).then(() => {
            connection.onclose(error => {
                if (error) {
                    const reconnect = setInterval(() => {
                        if (connection.state === HubConnectionState.Connected) {
                            clearInterval(reconnect);
                            reconnectEvent.emit();
                            return;
                        }
                        connection.start().then(() => {
                            clearInterval(reconnect);
                            reconnectEvent.emit();
                        }).catch(() => {
                            console.log(`Failed to re-connect to SignalR hub: ${endpoint}`);
                        });
                    }, 5000);
                }
            });
        });
        return new ConnectionContainer(connection, reconnectEvent);
    }

    private waitForConnection(connection: HubConnection): Promise<void> {
        return new Promise<void>((resolve) => {
            let retries = 0;
            const connectFunction = function () {
                connection.start().then(() => {
                    clearTimeout(connectTimeout);
                    resolve();
                }).catch(() => {
                    const newDelay = Math.min(retries++, 60);
                    console.log(`Failed to connect to SignalR hub, retrying in ${newDelay} seconds`);
                    connectTimeout = setTimeout(connectFunction, newDelay * 1000);
                });
            };
            let connectTimeout = setTimeout(connectFunction, 0);
        });
    }
}

export class ConnectionContainer {
    connection: HubConnection;
    reconnectEvent: EventEmitter<any>;

    constructor(connection: HubConnection, reconnectEvent: EventEmitter<any>) {
        this.connection = connection;
        this.reconnectEvent = reconnectEvent;
    }
}
