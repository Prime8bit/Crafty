import { Component, inject, OnInit } from '@angular/core';
import { UserService } from '../../services/user.service';
import { User } from '../../models/user';
import { ActivatedRoute, RouterLink } from '@angular/router';
import { DecimalPipe } from '@angular/common';
import { CarouselModule } from 'ngx-bootstrap/carousel';
import { CraftCardComponent } from '../../crafts/craft-card/craft-card.component';

@Component({
    selector: 'app-user-details',
    standalone: true,
    imports: [
        DecimalPipe, 
        CarouselModule, 
        CraftCardComponent,
        RouterLink
    ],
    templateUrl: './user-details.component.html',
    styleUrl: './user-details.component.css'
})
export class UserDetailsComponent implements OnInit {
    private userService: UserService = inject(UserService);
    private route: ActivatedRoute = inject(ActivatedRoute);
    user?: User;

    ngOnInit(): void {
        this.loadUser();
    }

    loadUser(): void {
        const userIdStr: string | null = this.route.snapshot.paramMap.get('userId');
        if (!userIdStr) {
            return;
        }

        this.userService.getUser(Number(userIdStr)).subscribe({
            next: user => {
                this.user = user;
            }
        });
    }
}
