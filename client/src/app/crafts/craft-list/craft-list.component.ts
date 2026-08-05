import { Component, inject, OnInit } from '@angular/core';
import { CraftService } from '../../services/craft.service';
import { CraftCardComponent } from '../craft-card/craft-card.component';
import { PaginationModule } from 'ngx-bootstrap/pagination';
import { FormsModule } from '@angular/forms';
import { KeyValuePipe } from '@angular/common';
import { BsDropdownModule } from 'ngx-bootstrap/dropdown';

@Component({
  selector: 'app-craft-list',
  standalone: true,
  imports: [
    CraftCardComponent, 
    PaginationModule,
    FormsModule,
    BsDropdownModule,
    KeyValuePipe
],
  templateUrl: './craft-list.component.html',
  styleUrl: './craft-list.component.css'
})
export class CraftListComponent implements OnInit{
    craftService = inject(CraftService);
    // This dictionary is used to map the dropdown values to the property names for the backend.
    craftSortDict: Record<string, string> = {"Name":"name", "Price":"price", "Date":"createdAt"};
    selectedSortOption = "Date";

    ngOnInit(): void {
        if (this.craftService.paginatedResult().totalCount === 0) {
            this.loadCrafts();
        }
    }

    loadCrafts(): void {
        this.craftService.getCrafts();
    }

    resetFilters(): void {
        this.craftService.resetCraftListParams();
        this.sort(this.selectedSortOption);
    }

    setSortOrder(newOrder: boolean): void {
        this.craftService.craftListParams().isOrderDescending = newOrder;
        this.loadCrafts();
    }

    sort(sortOption: string): void {
        if (sortOption in this.craftSortDict) {
            this.selectedSortOption = sortOption;
            this.craftService.craftListParams().orderBy = this.craftSortDict[sortOption];
            this.loadCrafts();
        }
    }

    pageChanged(event: any): void {
        if (this.craftService.craftListParams().pageNumber !== event.page) {
            this.craftService.craftListParams().pageNumber = event.page;
            this.loadCrafts();
        }
    }
}
