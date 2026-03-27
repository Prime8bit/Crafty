export interface UserLoginRequest {
    userName: string;
    password: string;
}

export interface UserToken {
    userId: number;
    userDisplayName: string;
    token: string;
}