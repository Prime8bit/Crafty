import { Component, inject, OnInit} from '@angular/core';
import { UserService } from '../../services/user.service';
import { UserMedia } from '../../models/media';
import { User } from '../../models/user';
import { AccountService } from '../../services/account.service';
import { ToastrService } from 'ngx-toastr';

@Component({
    selector: 'app-profile-image-uploader',
    standalone: true,
    templateUrl: './profile-image-uploader.component.html',
    styleUrl: './profile-image-uploader.component.css'
})
export class ProfileImageUploaderComponent implements OnInit{  
    private accountService = inject(AccountService);
    private userService = inject(UserService);  
    private toastr = inject(ToastrService);
    
    currentImageUrl?: string;
    selectedFile?: File;
    
    user?: User;

    ngOnInit(): void {
        this.loadUser();
    }

    loadUser(): void {
        const user = this.accountService.currentUser();
        if (!user) {
            return;
        }

        this.userService.getUser(user.userId).subscribe({
            next: (user) => {
                this.user = user;
                this.currentImageUrl = this.user?.profileImage?.url;
            } 
        });
    }

    onFileSelected(event: Event): void {
        const input = event.target as HTMLInputElement;
        if (!input.files || input.files.length === 0) return;

        this.selectedFile = input.files[0];

        this.currentImageUrl = URL.createObjectURL(this.selectedFile);
    }

    save(): void {
        if (!(this.selectedFile && this.user)) return;

        const imageFormData = new FormData();
        imageFormData.append('file', this.selectedFile);

        this.userService.updateUserProfileImage(this.user, imageFormData).subscribe({
            next: (updatedUserMediaItem: UserMedia) => {
                if (this.currentImageUrl && this.currentImageUrl != this.user?.profileImage?.url) {
                    URL.revokeObjectURL(this.currentImageUrl);
                }
                this.currentImageUrl = updatedUserMediaItem.url;
                this.toastr.success("Profile image updated successfully.");
            }
        });
    }

    cancel(): void {
        if (this.currentImageUrl && this.currentImageUrl != this.user?.profileImage?.url) {
            URL.revokeObjectURL(this.currentImageUrl);
        }
        this.currentImageUrl = this.user?.profileImage?.url;
    }
}
