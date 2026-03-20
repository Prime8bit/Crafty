export interface Message {
    id: number;
    content: string;
    dateRead: string;
    dateSent: string;
    senderId: number;
    senderDisplayName: string;
    senderProfileImageUrl: string;
    recipientId: number;
    recipientDisplayName: string;
}