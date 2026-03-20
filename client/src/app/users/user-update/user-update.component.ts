import { Component, inject, OnInit, ViewChild } from '@angular/core';
import { User } from '../../models/user';
import { UserService } from '../../services/user.service';
import { FormsModule, NgForm } from '@angular/forms';
import { ToastrService } from 'ngx-toastr';
import { ActivatedRoute, Router, RouterLink } from '@angular/router';
import { CraftCardComponent } from '../../crafts/craft-card/craft-card.component';
import { DatePipe } from '@angular/common';
import { ProfileImageUploaderComponent } from '../profile-image-uploader/profile-image-uploader.component';
import { TabDirective, TabsetComponent, TabsModule } from 'ngx-bootstrap/tabs';
import { OrderListComponent } from '../../order-list/order-list.component';
import { WishlistComponent } from '../../wishlist/wishlist.component';


@Component({
    selector: 'app-user-update',
    standalone: true,
    imports: [
        FormsModule, 
        TabsModule,
        RouterLink, 
        DatePipe, 
        CraftCardComponent, 
        ProfileImageUploaderComponent,
        WishlistComponent,
        OrderListComponent
    ],
    templateUrl: './user-update.component.html',
    styleUrl: './user-update.component.css'
})
export class UserUpdateComponent implements OnInit {
    private userService = inject(UserService);
    private toastr = inject(ToastrService);
    private route = inject(ActivatedRoute);
    private router = inject(Router);
    // The component is initialized before its children, so this must be optional
    @ViewChild('editForm') editForm?: NgForm;
    // As I understand it {static: true} allows angular to get the tabset before change detection runs. 
    // While i don't claim to know exactly what that means, this needs {static: true} in order to be 
    // able to select the tab during ngOnInit
    @ViewChild('tabset', {static: true}) tabset?: TabsetComponent;
    @ViewChild('orderList') orderList?: OrderListComponent;
    user: User = {} as User;
    activeTab?: TabDirective;
    filterArchived = true;

    ngOnInit(): void {
        this.route.data.subscribe({
            next: data => {
                this.user = data['user'];
            }
        });

        this.route.queryParams.subscribe({
            next: params => params['tab'] && this.selectTab(params['tab'])
        });
    }

    updateUser()
    {   
        if (this.user === undefined) {
            return;
        }
        
        this.userService.updateUser(this.user).subscribe({
            next: () => {
                this.toastr.success("Profile updated.");
                this.editForm?.reset(this.user);
            }
        });
    }

    selectTab(heading: string) {
        if (this.tabset) {
            const tab = this.tabset.tabs.find(curTab => curTab.heading === heading);
            if (tab)
            {
                tab.active = true;
            }
        }
    }

    onTabActivated(newTab: TabDirective) {
        this.activeTab = newTab;
        this.router.navigate([], {
            relativeTo: this.route,
            queryParams: { tab: this.activeTab.heading },
            queryParamsHandling: 'merge'
        });
        
        if (this.activeTab.heading === 'Orders') {
            this.orderList?.loadOrders();
        }
    }

    toggleArchivedCrafts() {
        this.filterArchived = !this.filterArchived;
    }
}
