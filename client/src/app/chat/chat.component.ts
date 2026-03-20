import { AfterViewChecked, Component, ElementRef, inject, ViewChild } from '@angular/core';
import { FormsModule, NgForm } from '@angular/forms';
import { ToastrService } from 'ngx-toastr';
import { AccountService } from '../services/account.service';
import { MessageService } from '../services/message.service';
import { DatePipe } from '@angular/common';

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

    messageService = inject(MessageService);
    currentUserId: number = 0;
    otherUserId: number = 2;
    messageContent = "";

    ngOnInit(): void {
        this.messageService.getContacts();        
    }

    ngOnDestroy(): void {
        this.messageService.stopHubConnection();
    }

    loadMessages() {
        const userToken = this.accountService.currentUser();
        if (!userToken) {
            return;
        }

        this.messageService.createHubConnection(userToken, this.otherUserId.toString());
    }

    deleteMessage(messageId: number) {
        this.messageService.deleteMessage(messageId).subscribe({
            next: _ => this.toastr.info("Message successfully deleted."),
            error: error => this.toastr.error(error.error)
        });
    }

    sendMessage() {
        this.messageService.createMessageAsync({
            recipientId: this.otherUserId,
            content: this.messageContent
        }).then(() => {
            this.messageForm?.reset();
            this.scrollMessageThreadToBottom();
        });
    }

    selectUser(userId: number) {
        this.otherUserId = userId;
        this.messageService.stopHubConnection();
        this.loadMessages();
    }

    ngAfterViewChecked(): void {
        this.scrollMessageThreadToBottom();
    }

    private scrollMessageThreadToBottom() {
        if (this.scrollContainer) {
            this.scrollContainer.nativeElement.scrollTop = this.scrollContainer.nativeElement.scrollHeight;
        }
    }
}
