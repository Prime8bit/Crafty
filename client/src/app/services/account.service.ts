import { HttpClient } from '@angular/common/http';
import { computed, inject, Injectable, signal } from '@angular/core';
import { UserToken } from '../models/user-token';
import { map } from 'rxjs';
import { environment } from '../../environments/environment';
import { WishlistService } from './wishlist.service';

@Injectable({
    providedIn: 'root'
})
export class AccountService {
    private http = inject(HttpClient);
    // Make sure you don't create a circular dependency between services
    private wishlistService = inject(WishlistService);
    baseUrl = environment.apiUrl;
    currentUser = signal<UserToken | null>(null);
    roles = computed(() => {
        const user = this.currentUser();
        if (user && user.token) {
            const role = JSON.parse(atob(user.token.split('.')[1])).role
            return Array.isArray(role) ? role : [role];
        }
        return [];
    });

    login (model: any) {
        return this.http.post<UserToken>(`${this.baseUrl}accounts/login`, model).pipe(
            map((user: UserToken) => {
                if (user) {
                    this.setCurrentUser(user);
                }
            }
        ));
    }

    register (model: any) {
        return this.http.post<UserToken>(`${this.baseUrl}accounts/register`, model).pipe(
            map((user: UserToken) => {
                if (user) {
                    this.setCurrentUser(user);
                }
                return user;
            }
        ));
    }

    logout () {        
        localStorage.removeItem('user');
        this.currentUser.set(null);
    }

    setCurrentUser(userToken: UserToken) {
        localStorage.setItem('user', JSON.stringify(userToken));
        this.currentUser.set(userToken);
        // Cache the users wishlist.
        this.wishlistService.getWishlistIds();
    }
}
