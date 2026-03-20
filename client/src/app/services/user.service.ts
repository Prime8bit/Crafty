import { HttpClient } from '@angular/common/http';
import { inject, Injectable, signal } from '@angular/core';
import { environment } from '../../environments/environment';
import { User } from '../models/user';
import { of, tap } from 'rxjs';
import { UserMedia } from '../models/media';

@Injectable({
    providedIn: 'root'
})

export class UserService {
    private http = inject(HttpClient);
    baseUrl = environment.apiUrl;
    users = signal<User[]>([]);
    
    getUsers() {
        return this.http.get<User[]>(`${this.baseUrl}users`).subscribe({
            next: users => this.users.set(users)
        });
    }

    getUser(userName: string) {
        const user = this.users().find(u => u.userName === userName);
        if (user !== undefined) {
            return of(user);
        }
        
        return this.http.get<User>(`${this.baseUrl}users/${userName}`).pipe(
            tap(fetchedUser => {
                // Add the fetched user to the users collection
                this.users.update(users => [...users, fetchedUser]);
            })
        );
    }

    updateUser(user: User) {
        return this.http.put(`${this.baseUrl}users`, user).pipe(
            tap(() => {
                this.users.update(users => 
                    users.map<User>(u => u.userName === user.userName ? user : u)
                )
            })
        );
    }

    updateUserProfileImage(user: User, imageFormData: FormData) {
        return this.http.post<UserMedia>(`${this.baseUrl}users/set-profile-image`, imageFormData).pipe(
            tap((userMediaItem) => {
                user.profileImage = userMediaItem;
                this.users.update(users => 
                    users.map<User>(u => u.userName === user.userName ? user : u)
                )
            })
        );
    }

    markUserAsChanged(user: User)
    {        
        // This will force the user to be pulled from the backend the next time getUser is called on that user
        this.users.update(users => users.filter(u => u.userName !== user.userName));
    }
}