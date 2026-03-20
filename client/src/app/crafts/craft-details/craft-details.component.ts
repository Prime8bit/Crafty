import { Component, computed, inject, OnInit } from '@angular/core';
import { CraftService } from '../../services/craft.service';
import { Craft } from '../../models/craft';
import { MediaType } from '../../models/media';
import { ActivatedRoute, RouterLink } from '@angular/router';
import { DecimalPipe } from '@angular/common';
import { CarouselModule } from 'ngx-bootstrap/carousel';
import { WishlistService } from '../../services/wishlist.service';
import { ToastrService } from 'ngx-toastr';
import { HasRoleDirective } from '../../directives/has-role.directive';
import { Model3dViewerComponent } from '../../model3d-viewer/model3d-viewer.component';

@Component({
    selector: 'app-craft-details',
    standalone: true,
    imports: [
        DecimalPipe,
        CarouselModule,
        RouterLink,
        HasRoleDirective,
        Model3dViewerComponent
    ],
    templateUrl: './craft-details.component.html',
    styleUrl: './craft-details.component.css'
})
export class CraftDetailsComponent implements OnInit {
    private craftService = inject(CraftService);
    private route = inject(ActivatedRoute);
    private wishlistService = inject(WishlistService);
    private toastr = inject(ToastrService);
    // Expose the enum to the template
    MediaType = MediaType;
    craft?: Craft;

    ngOnInit(): void {
        this.loadCraft();
    }

    loadCraft() {
        const craftIdStr = this.route.snapshot.paramMap.get('craftId');
        if (!craftIdStr) {
            return;
        }

        this.craftService.getCraft(craftIdStr).subscribe({
            next: craft => this.craft = craft
        });
    }

    toggleWishlist() {
        if (!this.craft)
            return;

        this.wishlistService.toggleWishlist(this.craft.id).subscribe({
            next: _ => {
                if (this.craft) {
                    if (this.wishlistService.wishlistIds().includes(this.craft.id)) {
                        this.wishlistService.wishlistIds.update(ids =>  ids.filter(id => id !== this.craft!.id));
                    }
                    else
                    {
                        this.wishlistService.wishlistIds.update(ids => [...ids, this.craft!.id]);
                    }
                }
            },
            error: (error) => {
                this.toastr.error(error.error);
            }
        })
    }

    markAsInappropriate() {
        if (!this.craft)
            return;

        this.craftService.markCraftAsInappropriate(this.craft?.id).subscribe({
            next: _ => this.toastr.info(`${this.craft?.name} successfully marked as inappropriate.`),
            error: error => this.toastr.error(error.error)
        });
    }

    markAsAppropriate() {
        if (!this.craft)
            return;

        this.craftService.markCraftAsAppropriate(this.craft?.id).subscribe({
            next: _ => this.toastr.info(`${this.craft?.name} successfully marked as appropriate.`),
            error: error => this.toastr.error(error.error)
        });
    }

    archiveCraft() {
        if (!this.craft)
            return;

        this.craftService.archiveCraft(this.craft?.id).subscribe({
            next: _  => this.toastr.info(`Craft ${this.craft?.name} successfully archived.`),
            error: error  => this.toastr.error(error.error)
        });
    }
}
