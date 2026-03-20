import { inject } from '@angular/core';
import { CanActivateFn } from '@angular/router';
import { AccountService } from '../services/account.service';
import { ToastrService } from 'ngx-toastr';

export const AuthGuard: CanActivateFn = (route, state) => {
    // Because this is not a class, we must inject the AccountService
    // using an alternate method
    const accountService = inject(AccountService);
    const toastr = inject(ToastrService);

    if (!accountService.currentUser()) {
        toastr.error('Unauthorized users cannot proceed.');
        return false;
    }

    return true;
};
