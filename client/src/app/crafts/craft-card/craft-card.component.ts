import { Component, computed, inject, input, OnInit } from '@angular/core';
import { Craft } from '../../models/craft';
import { DecimalPipe } from '@angular/common';
import { RouterLink } from '@angular/router';
import { WishlistService } from '../../services/wishlist.service';
import { ToastrService } from 'ngx-toastr';
import { AccountService } from '../../services/account.service';
import { CraftService } from '../../services/craft.service';
import { Model3dViewerComponent } from '../../model3d-viewer/model3d-viewer.component';

@Component({
    selector: 'app-craft-card',
    standalone: true,
    imports: [
        DecimalPipe,
        RouterLink,
        Model3dViewerComponent
    ],
    templateUrl: './craft-card.component.html',
    styleUrl: './craft-card.component.css'
})
export class CraftCardComponent {
    private wishlistService = inject(WishlistService);
    private toastr = inject(ToastrService);
    private craftService = inject(CraftService);
    accountService = inject(AccountService);

    craft = input.required<Craft>();

    isWishlisted = computed(() => this.wishlistService.wishlistIds().includes(this.craft().id));

    toggleWishlist() {
        this.wishlistService.toggleWishlist(this.craft().id).subscribe({
            next: () => {
                if (this.isWishlisted()) {
                    this.wishlistService.wishlistIds.update(ids => ids.filter(id => id !== this.craft().id));
                }
                else {
                    this.wishlistService.wishlistIds.update(ids => [...ids, this.craft().id]);
                }
            },
            error: (error) => {
                this.toastr.error(error.error);
            }
        })
    }

    archiveCraft() {
        if (this.craft().isArchived) {
            return;
        }

        if (this.accountService.currentUser()?.userName !== this.craft().sellerUserName) {
            this.toastr.error("You cannot archive a craft that you don't produce.");
        }

        this.craft().isArchived = true;
        this.craftService.archiveCraft(this.craft().id).subscribe({
            next: craft => this.toastr.info(`Successfully archived ${craft.name}`),
            error: error => {
                this.toastr.error(error.error)
                this.craft().isArchived = false;
            }
        });
    }
}
