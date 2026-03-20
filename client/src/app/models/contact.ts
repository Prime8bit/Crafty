import { UserMedia } from "./media";

export interface Contact {
    id: number;
    displayName: string;
    profileImageUrl: string;
    lastMessage: string;
    lastMessageDate: string;
    wasLastMessageRead: boolean;
}