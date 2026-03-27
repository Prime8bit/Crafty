import { Component, inject, OnInit } from '@angular/core';
import { WishlistService } from '../services/wishlist.service';
import { CraftCardComponent } from '../crafts/craft-card/craft-card.component';
import { PaginationModule } from 'ngx-bootstrap/pagination';
import { FormsModule } from '@angular/forms';
import { BsDropdownModule } from 'ngx-bootstrap/dropdown';
import { KeyValuePipe } from '@angular/common';

@Component({
  selector: 'app-wishlist',
  standalone: true,
  imports: [
    CraftCardComponent,
    PaginationModule,
    FormsModule,
    BsDropdownModule,
    KeyValuePipe
  ],
  templateUrl: './wishlist.component.html',
  styleUrl: './wishlist.component.css'
})
export class WishlistComponent implements OnInit {
    wishlistService = inject (WishlistService);
    pageNumber = 1;
    pageSize = 4;

    // This dictionary is used to map the dropdown values to the property names for the backend.
    craftSortDict: Record<string, string> = {"Date":"createdAt", "Name":"name", "Price":"price"};
    selectedSortOption = Object.keys(this.craftSortDict)[0];
    
    ngOnInit(): void {
        this.loadWishlist();
    }

    loadWishlist(): void {
        this.wishlistService.getWishlist(this.pageNumber, this.pageSize);
    }

    resetFilters(): void {
        this.wishlistService.resetCraftListParams();
        this.sort(this.selectedSortOption);
    }

    setSortOrder(isDescending: boolean): void {
        this.wishlistService.craftListParams().isOrderDescending = isDescending;
        this.loadWishlist();
    }

    sort(sortOption: string): void {
        if (sortOption in this.craftSortDict) {
            this.selectedSortOption = sortOption;
            this.wishlistService.craftListParams().orderBy = this.craftSortDict[sortOption];
            this.loadWishlist();
        }
    }

    pageChanged(event: any): void {
        if (this.pageNumber !== event.page) {
            this.pageNumber = event.page;
            this.loadWishlist(); 
        }
    }
}
