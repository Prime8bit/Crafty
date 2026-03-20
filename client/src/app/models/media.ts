export enum MediaType {
    Image = 1,
    Video = 2,
    Model3d = 3
}

export interface CloudMedia {
    id: number,
    url: string,
    cloudId: string,
    type: MediaType
}

export interface CraftMedia extends CloudMedia {
    craftId: number,
    craftName: string
}

export interface UserMedia extends CloudMedia {
    userId: number,
    userUserName: string
}