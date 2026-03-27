import { HttpClient } from '@angular/common/http';
import { computed, inject, Injectable, signal } from '@angular/core';
import { UserLoginRequest, UserToken } from '../models/user-login';
import { map, Observable } from 'rxjs';
import { environment } from '../../environments/environment';
import { WishlistService } from './wishlist.service';
import { OrderService } from './order.service';

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

    login(model: UserLoginRequest): Observable<UserToken> {
        return this.http.post<UserToken>(`${this.baseUrl}accounts/login`, model).pipe(
            map((user: UserToken) => {
                if (user) {
                    this.setCurrentUser(user);
                }
                return user;
            }
        ));
    }

    register(model: any): Observable<UserToken> {
        return this.http.post<UserToken>(`${this.baseUrl}accounts/register`, model).pipe(
            map((user: UserToken) => {
                if (user) {
                    this.setCurrentUser(user);
                }
                return user;
            }
        ));
    }

    logout(): void {        
        localStorage.removeItem('user');
        this.currentUser.set(null);
    }

    setCurrentUser(userLoginResponse: UserToken): void {
        localStorage.setItem('user', JSON.stringify(userLoginResponse));
        this.currentUser.set(userLoginResponse);
        // Cache the users wishlist.
        this.wishlistService.getWishlistIds();
    }

    loadUserFromStorage(): void {
        const userString = localStorage.getItem('user');
        if (!userString) {
            return;
        }

        const user = JSON.parse(userString);
        this.setCurrentUser(user);
    }
}
