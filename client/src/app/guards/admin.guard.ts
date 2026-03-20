import { CanActivateFn } from '@angular/router';
import { AccountService } from '../services/account.service';
import { ToastrService } from 'ngx-toastr';
import { inject } from '@angular/core';

export const AdminGuard: CanActivateFn = (route, state) => {
    const accountService = inject(AccountService);
    const toastr = inject(ToastrService);

    if (accountService.roles().includes('Admin')){
        return true;
    }
    else {
        toastr.error("You do not have permission to view this page.");
        return false;
    }
};
