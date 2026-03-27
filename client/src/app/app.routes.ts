import { Routes } from '@angular/router';
import { HomeComponent } from './home/home.component';
import { CraftListComponent } from './crafts/craft-list/craft-list.component';
import { CraftDetailsComponent } from './crafts/craft-details/craft-details.component';
import { AuthGuard } from './guards/auth.guard';
import { NotFoundComponent } from './errors/not-found/not-found.component';
import { ServerErrorComponent } from './errors/server-error/server-error.component';
import { UserUpdateComponent } from './users/user-update/user-update.component';
import { UserDetailsComponent } from './users/user-details/user-details.component';
import { CraftUpdateComponent } from './crafts/craft-update/craft-update.component';
import { PreventUnsavedChangesGuard } from './guards/prevent-unsaved-changes.guard';
import { OrderComponent } from './orders/order/order.component';
import { UserUpdateResolver } from './resolvers/user-update.resolver';
import { InappropriateCraftListComponent } from './crafts/inappropriate-craft-list/inappropriate-craft-list.component';
import { AdminGuard } from './guards/admin.guard';
import { ChatComponent } from './chat/chat.component';
import { Model3dViewerComponent } from './model3d-viewer/model3d-viewer.component';
import { CheckoutComponent } from './orders/checkout/checkout.component';

export const routes: Routes = [
    {path:'', component: HomeComponent},    
    {
        path: '',
        runGuardsAndResolvers: 'always',
        canActivate: [AuthGuard],   
        children: [  
            {path:'user/update', component: UserUpdateComponent, resolve: {user: UserUpdateResolver}},
            {path:'craft/newCraft', component: CraftUpdateComponent, canDeactivate: [PreventUnsavedChangesGuard]},
            {path:'craft/:craftId/update', component: CraftUpdateComponent, canDeactivate: [PreventUnsavedChangesGuard]}, 
            {path:'order/:orderId', component: OrderComponent},
            {path:'inappropriate-craft-list', component: InappropriateCraftListComponent, canActivate: [AdminGuard]},
            {path:'chat', component: ChatComponent},
            {path:'cart', component: OrderComponent},
            {path:'checkout', component: CheckoutComponent}
        ]
    },  
    {path:'user/:userId', component: UserDetailsComponent},
    {path:'craft', component: CraftListComponent},
    {path:'craft/:craftId', component: CraftDetailsComponent},
    {path:'not-found', component: NotFoundComponent},
    {path:'server-error', component: ServerErrorComponent},
    {path:'model3d', component: Model3dViewerComponent},
    {path:'**', component: HomeComponent, pathMatch: 'full'},
];
