import { AfterViewChecked, Component, ElementRef, inject, ViewChild } from '@angular/core';
import { FormsModule, NgForm } from '@angular/forms';
import { ToastrService } from 'ngx-toastr';
import { AccountService } from '../services/account.service';
import { MessageService } from '../services/message.service';
import { DatePipe } from '@angular/common';
import { ActivatedRoute, Router } from '@angular/router';
import { UserService } from '../services/user.service';

@Component({
  selector: 'app-chat',
  standalone: true,
  imports: [FormsModule, DatePipe],
  templateUrl: './chat.component.html',
  styleUrl: './chat.component.css'
})
export class ChatComponent implements AfterViewChecked{
    @ViewChild('messageForm') messageForm?: NgForm;
    @ViewChild('scrollContainer') scrollContainer?: ElementRef<HTMLUListElement>;
    private toastr = inject(ToastrService);
    private accountService = inject(AccountService);
    private router = inject(Router);
    private route = inject(ActivatedRoute);
    private userService = inject(UserService);
    private isConnectedToMessageHub = false;

    messageService = inject(MessageService);
    otherUserId?: number;
    otherUserDisplayName?: string;
    messageContent = "";

    ngOnInit(): void {
        this.otherUserId = 0;
        this.messageService.getContacts();        
        this.route.queryParams.subscribe(params => {
            const userIdStr = params['selectedUser'];
            if (userIdStr) {
                this.otherUserId = Number(userIdStr);
                this.loadMessages();
            }
        })
    }

    ngOnDestroy(): void {
        this.messageService.stopHubConnection();
    }

    async loadMessages(): Promise<void>{
        const userToken = this.accountService.currentUser();
        if (!userToken || !this.otherUserId) {
            return;
        }
        
        // Disconnect from any existing chatrooms first. It is okay to call this even if not connected.
        await this.messageService.stopHubConnection();
        // Then connect to the new one.
        const otherUserId = this.otherUserId;
        this.userService.getUser(this.otherUserId).subscribe({
            next: async (user) => {
                this.otherUserDisplayName = user.displayName;
                await this.messageService.createHubConnection(userToken, otherUserId.toString());
            },
            error: error => {
                this.toastr.error(error.error);
            }
        });
    }

    async deleteMessage(messageId: number): Promise<void>{
        await this.messageService.deleteMessage(messageId);
        this.toastr.info("Message successfully deleted.");
    }

    async sendMessage(): Promise<void>{
        if (!this.otherUserId) {
            return;
        }

        await this.messageService.createMessageAsync({
            recipientId: this.otherUserId,
            content: this.messageContent
        });
        this.messageForm?.reset();
        this.scrollMessageThreadToBottom();
    }

    selectUser(userId: number): void{
        this.otherUserId = userId;
        
        // This will cause ngInit to be called again, which will load the messages for the new otherUser
        this.router.navigate([], {
            relativeTo: this.route,
            queryParams: {
                selectedUser: this.otherUserId
            },
            queryParamsHandling: 'merge',
            replaceUrl: true
        });
    }

    ngAfterViewChecked(): void {
        this.scrollMessageThreadToBottom();
    }

    private scrollMessageThreadToBottom(): void {
        if (this.scrollContainer) {
            this.scrollContainer.nativeElement.scrollTop = this.scrollContainer.nativeElement.scrollHeight;
        }
    }
}
