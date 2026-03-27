import { HttpClient } from '@angular/common/http';
import { inject, Injectable } from '@angular/core';
import { environment } from '../../environments/environment';
import { Observable } from 'rxjs';

@Injectable({
    providedIn: 'root'
})

export class MediaService {
    private http = inject(HttpClient);
    baseUrl = environment.apiUrl;
    
    deleteCloudImage(cloudId: string): Observable<Object> {
        const deleteUri = `${this.baseUrl}medias/images/${encodeURIComponent(cloudId)}`;
        const result = this.http.delete(deleteUri);
        return result;
    }

    deleteCloudVideo(cloudId: string): Observable<Object> {
        const deleteUri = `${this.baseUrl}medias/videos/${encodeURIComponent(cloudId)}`;
        return this.http.delete(deleteUri);
    }

    deleteCloudModel3d(cloudId: string): Observable<Object> {
        const deleteUri = `${this.baseUrl}medias/model3d/${encodeURIComponent(cloudId)}`;
        return this.http.delete(deleteUri);
    }
}