import { Component, inject } from '@angular/core';
import { User } from '../../models/user';
import { UserService } from '../../services/user.service';

@Component({
    selector: 'app-user-list',
    standalone: true,
    imports: [],
    templateUrl: './user-list.component.html',
    styleUrl: './user-list.component.css'
})
export class UserListComponent {
    userService: UserService = inject(UserService);

    ngOnInit(): void {
        if (this.userService.users().length === 0) {
            this.loadUsers();
        }
    }

    loadUsers(): void {
        this.userService.getUsers();
    }
}
