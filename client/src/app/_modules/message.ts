    export interface Message {
        id: number;
        senderID: number;
        senderUserName: string;
        senderPhotoUrl: string;
        recipientID: number;
        recipientUserName: string;
        recipientPhotoUrl: string;
        content: string;
        dateRead?: Date;
        messageSent: Date;
    }