export interface Message {
    id: number;
    content: string;
    dateRead: string;
    dateSent: string;
    senderDeleted: boolean;
    recipientDeleted: boolean;
    senderId: number;
    senderDisplayName: string;
    senderProfileImageUrl: string;
    recipientId: number;
    recipientDisplayName: string;
}