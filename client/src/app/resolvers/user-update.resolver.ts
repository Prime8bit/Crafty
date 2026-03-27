import { ResolveFn } from '@angular/router';
import { UserService } from '../services/user.service';
import { User } from '../models/user';
import { inject } from '@angular/core';
import { AccountService } from '../services/account.service';

export const UserUpdateResolver: ResolveFn<User | null> = (route, state) => {
    const userService = inject(UserService);
    const accountService = inject(AccountService);
        
    const user = accountService.currentUser();
    if (!user?.userId) {
        return null;
    }

    return userService.getUser(user.userId);
};
