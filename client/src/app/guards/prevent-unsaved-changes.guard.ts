import { CanDeactivateFn } from '@angular/router';
import { CraftUpdateComponent } from '../crafts/craft-update/craft-update.component';

export const PreventUnsavedChangesGuard: CanDeactivateFn<CraftUpdateComponent> = (component) => {
    if (component.newCraftForm?.dirty) {
        return confirm('Are you sure you want to leave this page? Any unsaved changes will be lost.');
    }
    return true;
};
