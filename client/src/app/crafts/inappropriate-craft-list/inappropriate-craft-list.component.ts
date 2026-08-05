import { Component, inject } from '@angular/core';
import { CraftService } from '../../services/craft.service';
import { PaginationParams } from '../../models/pagination-params';
import { Craft } from '../../models/craft';
import { PagedList } from '../../models/paged-list';
import { PaginationModule } from 'ngx-bootstrap/pagination';
import { CraftCardComponent } from '../craft-card/craft-card.component';
import { FormsModule } from '@angular/forms';

@Component({
    selector: 'app-inappropriate-craft-list',
    standalone: true,
    imports: [
        FormsModule,
        PaginationModule,
        CraftCardComponent
        ],
    providers: [CraftService],
    templateUrl: './inappropriate-craft-list.component.html',
    styleUrl: './inappropriate-craft-list.component.css'
})
export class InappropriateCraftListComponent {
    craftService = inject(CraftService);
    // This dictionary is used to map the dropdown values to the property names for the backend.
    craftSortDict: Record<string, string> = {"Name":"name", "Price":"price", "Date":"createdAt"};
    selectedSortOption = "Date";
    paginatedCrafts: PagedList<Craft> = new PagedList<Craft>();
    paginationParams: PaginationParams = { pageNumber: 1, pageSize: 5, orderBy : "createdAt", isOrderDescending: false };

    ngOnInit(): void {
        if (this.paginatedCrafts.totalCount === 0) {
            this.loadCrafts();
        }
    }

    loadCrafts(): void {
        this.craftService.getInappropriateCrafts(this.paginationParams).subscribe({
            next: paginatedCrafts => this.paginatedCrafts = paginatedCrafts
        });
    }

    pageChanged(event: any): void {
        if (this.paginationParams.pageNumber !== event.page) {
            this.paginationParams.pageNumber = event.page;
            this.loadCrafts();
        }
    }
}
