import { MessageConnection } from "./message-connection";

export interface MessageGroup {
    name: string;
    connections: MessageConnection[];
}