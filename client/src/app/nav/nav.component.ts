import { Component, inject } from '@angular/core';
import { FormsModule } from '@angular/forms';
import { AccountService } from '../services/account.service';
import { BsDropdownModule } from 'ngx-bootstrap/dropdown';
import { Router, RouterLink, RouterLinkActive } from '@angular/router';
import { ToastrService } from 'ngx-toastr';
import { TitleCasePipe } from '@angular/common';
import { HttpErrorResponse } from '@angular/common/http';
import { HasRoleDirective } from '../directives/has-role.directive';

@Component({
    selector: 'app-nav',
    standalone: true,
    imports: [
        FormsModule, 
        BsDropdownModule, 
        RouterLink, 
        RouterLinkActive, 
        TitleCasePipe,
        HasRoleDirective
    ],
    templateUrl: './nav.component.html',
    styleUrl: './nav.component.css'
})
export class NavComponent {
    private router = inject(Router);
    private toastr = inject(ToastrService);
    accountService: AccountService = inject(AccountService);
    model: any = {};

    login() {
        this.accountService.login(this.model).subscribe({
            next: _ => this.router.navigateByUrl('/craft'),
            error: (error: HttpErrorResponse) => this.toastr.error(error.message) 
        });
    }

    logout() {
        this.accountService.logout();
        this.router.navigateByUrl('/');
    }
}
