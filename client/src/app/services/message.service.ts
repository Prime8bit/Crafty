import { inject, Injectable, signal } from '@angular/core';
import { environment } from '../../environments/environment';
import { HttpClient, HttpParams, HttpResponse } from '@angular/common/http';
import { PaginationParams } from '../models/pagination-params';
import { Message } from '../models/message';
import { CreateMessage } from '../models/create-message';
import { HubConnection, HubConnectionBuilder, HubConnectionState } from '@microsoft/signalr';
import { UserToken } from '../models/user-token';
import { Contact } from '../models/contact';
import { MessageGroup } from '../models/message-group';

enum MessageHubMessages
{
    SendMessage = 'SendMessage',
    ReceiveMessageThread = 'ReceiveMessageThread',
    UpdatedGroup = 'UpdatedGroup'
}

@Injectable({
  providedIn: 'root'
})
export class MessageService {
    private http = inject(HttpClient);
    private hubConnection?: HubConnection;
    baseUrl = environment.apiUrl;
    hubUrl = environment.hubsUrl;
    paginationParams = signal<PaginationParams>(new PaginationParams());
    messageThread = signal<Message[]>([]);
    contacts = signal<Contact[]>([]);
    
    resetOrderListParams() {
        this.paginationParams.set(new PaginationParams());
    }

    getContacts() {
        return this.http.get<Contact[]>(`${this.baseUrl}messages/contacts`).subscribe({
            next: contacts => this.contacts.set(contacts)
        })
    }

    getMessageThread(otherUserId: number, pageNumber: number, pageSize: number) {
        this.paginationParams().pageNumber = pageNumber;
        this.paginationParams().pageSize = pageSize;

        let params = new HttpParams();
        if (this.paginationParams().pageNumber && this.paginationParams().pageSize) {
            params = params.append("pageNumber", this.paginationParams().pageNumber);
            params = params.append("pageSize", this.paginationParams().pageSize);
        }

        return this.http.get<Message[]>(`${this.baseUrl}messages/thread/${otherUserId}`, {observe: 'response', params}).subscribe({
            next: response => this.setPaginatedResponseReversed(response)
        });
    }

    async createMessageAsync(newMessage: CreateMessage) {
        return this.hubConnection?.invoke(MessageHubMessages.SendMessage, newMessage);
    }

    deleteMessage(messageId: number) {
        return this.http.delete(`${this.baseUrl}messages/${messageId}`);
    }

    createHubConnection(userToken: UserToken, otherUserId: string) {
        this.hubConnection = new HubConnectionBuilder()
            .withUrl(`${this.hubUrl}message?user=${otherUserId}`, {
                accessTokenFactory: () => userToken.token
            })
            .withAutomaticReconnect()
            .build();

        this.hubConnection.start().catch(error => console.log(error));
        this.setupHubConnectionMessages(otherUserId)
    }

    stopHubConnection() {
        if (this.hubConnection?.state === HubConnectionState.Connected) {
            this.hubConnection?.stop().catch(error => console.log(error));
        }
    }

    private setupHubConnectionMessages(otherUserIdStr: string) {
        if (this.hubConnection === null) {
            return;
        }
        const otherUserId = Number(otherUserIdStr);

        this.hubConnection!.on(MessageHubMessages.ReceiveMessageThread, messages => {
            this.messageThread.set(messages.reverse());
            this.contacts.update(contacts => contacts.map(contact =>
                contact.id === otherUserId
                ? { ...contact, wasLastMessageRead: true}
                : contact
            ))
        })

        this.hubConnection!.on(MessageHubMessages.SendMessage, message => {
            this.messageThread.update(messages => [...messages, message]);
        })

        this.hubConnection!.on(MessageHubMessages.UpdatedGroup, (group: MessageGroup) => {
            if (group.connections.some(connection => connection.userId === otherUserId)) {
                this.messageThread.update(messages => {
                    messages.forEach(message => {
                        if (!message.dateRead) {
                            message.dateRead = `${new Date(Date.now())}`;
                        }
                    })

                    return messages;
                })
            }
        })
    }

    private setPaginatedResponseReversed(response: HttpResponse<Message[]> ) {
        const responseBody = response.body as Message[];
        responseBody.reverse();
        
        // this.paginatedResult.set({
        //     items: responseBody, 
        //     pagination: JSON.parse(response.headers.get('Pagination')!)
        // })
    }
}
