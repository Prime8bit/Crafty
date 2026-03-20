import { Component, HostListener, inject, OnInit, ViewChild } from '@angular/core';
import { UserService } from '../../services/user.service';
import { AccountService } from '../../services/account.service';
import { User } from '../../models/user';
import { ToastrService } from 'ngx-toastr';
import { CraftService } from '../../services/craft.service';
import { ActivatedRoute, Router, RouterLink } from '@angular/router';
import { Craft } from '../../models/craft';
import { FormsModule, NgForm } from '@angular/forms';
import { MediaUploaderComponent } from '../../media-uploader/media-uploader.component';

@Component({
    selector: 'app-craft-update',
    standalone: true,
    imports: [
        FormsModule, 
        RouterLink,
        MediaUploaderComponent
    ],
    templateUrl: './craft-update.component.html',
    styleUrl: './craft-update.component.css'
})

export class CraftUpdateComponent implements OnInit {
    @ViewChild('newCraftForm') newCraftForm?: NgForm;
    @ViewChild('mediaUploader') mediaUploader?: MediaUploaderComponent;
    // Display a warning if the user tries to navigate away from the page with unsaved changes
    @HostListener('window:beforeunload', ['$event']) notify($event: any) {
        if (this.newCraftForm?.dirty) {
            $event.returnValue = true;
        }
    }

    private accountService: AccountService = inject(AccountService);
    private userService: UserService = inject(UserService);
    private craftService: CraftService = inject(CraftService);
    private toastr: ToastrService = inject(ToastrService);
    private router: Router = inject(Router);
    private route: ActivatedRoute = inject(ActivatedRoute);

    user?: User;
    
    craftId: string | null = null;
    craft: Craft = {
        id: 0,
        name: '',
        description: '',
        price: 0,
        stock: 0,
        createdAt: '',
        sellerDisplayName: '',
        sellerUserName: '',
        searchImageId: null,
        searchImage: null,
        medias: [],
        isArchived: false
    };
    // To prevent browser errors, tempImage and tempVideo are set to null
    // Input's of type file are not allowed to have their values set in code.
    tempDataModel: any = {};    

    ngOnInit(): void {
        this.loadUser();
        this.loadCraft();
    }    

    updateCraft() {
        if (this.user === undefined) {
            return;
        }

        if (this.mediaUploader?.tempMedias === undefined || this.mediaUploader.tempMedias.length > 0) {
            this.toastr.error('You must upload all media before creating a new craft.');
            return;
        }

        this.craft.sellerDisplayName = this.user.displayName;
        this.craft.sellerUserName = this.user.userName;
        
        if (this.craftId !== null) {
            this.craftService.updateCraft(this.craft.id.toString(), this.craft).subscribe({
                next: (response: Craft) => {
                    this.userService.markUserAsChanged(this.user!);
                    // I need to mark the form as pristine so the prevent-unsaved-changes guard doesn't trigger
                    this.newCraftForm?.form.markAsPristine();
                    this.router.navigateByUrl('/user/update');
                    this.toastr.success(`Updated craft ${response.name} successfully.`)
                },
                error: error => this.toastr.error(error)
            });
        }
        else
        {
            this.craftService.newCraft(this.craft).subscribe({
                next: (response: Craft) => {
                    this.userService.markUserAsChanged(this.user!);
                    // I need to mark the form as pristine so the prevent-unsaved-changes guard doesn't trigger
                    this.newCraftForm?.form.markAsPristine();
                    this.router.navigateByUrl('/user/update');
                    this.toastr.success(`Added ${response.name} to your products.`)
                },
                error: error => this.toastr.error(error)
            });
        }
    }

    onParentCraftChanged(event: Craft) {   
        this.craft = event;
    }
    
    private loadUser() {
        const user = this.accountService.currentUser();
        if (!user) {
            return;
        }

        this.userService.getUser(user.userName).subscribe({
            next: user => this.user = user
        });
    }

    private loadCraft() {
        this.craftId = this.route.snapshot.paramMap.get('craftId');

        if (this.craftId === null) {
            this.craft = {
                id: 0,
                name: '',
                description: '',
                price: 0,
                stock: 0,
                createdAt: '',
                sellerDisplayName: '',
                sellerUserName: '',
                searchImageId: null,
                searchImage: null,
                medias: [],
                isArchived: false
            };
        }
        else {
            this.craftService.getCraft(this.craftId).subscribe({
                next: craft => this.craft = craft
            });
        }
    }
}
