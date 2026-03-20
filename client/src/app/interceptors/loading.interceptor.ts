import { HttpInterceptorFn } from '@angular/common/http';
import { inject } from '@angular/core';
import { BusyService } from '../services/busy.service';
import { delay, finalize } from 'rxjs';

export const LoadingInterceptor: HttpInterceptorFn = (req, next) => {
    const busyService = inject(BusyService);

    busyService.busy();
    return next(req).pipe(
        // This is an artificial delay for api calls to test the loading indicator
        // delay(1000),
        finalize(() => {
            busyService.idle();
        })
    );
};
