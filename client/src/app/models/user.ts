import { Craft } from './craft'
import { CloudMedia } from './media'

export interface User {
    id: number
    userName: string
    email: string
    fullName: string
    displayName: string
    created: Date
    lastActive: Date
    address: string
    profileImage: CloudMedia
    products: Craft[]
}