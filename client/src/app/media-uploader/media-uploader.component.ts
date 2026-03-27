import { CommonModule, NgClass, NgFor, NgIf, NgStyle } from '@angular/common';
import { Component, inject, input, InputSignal, output, OutputEmitterRef, SimpleChanges } from '@angular/core';
import { FileItem, FileLikeObject, FileUploader, FileUploaderOptions, FileUploadModule, ParsedResponseHeaders } from 'ng2-file-upload';
import { AccountService } from '../services/account.service';
import { CloudMedia, MediaType, CraftMedia } from '../models/media';
import { environment } from '../../environments/environment';
import { MediaService } from '../services/media.service';
import { ToastrService } from 'ngx-toastr';
import { Craft } from '../models/craft';
import { Model3dViewerComponent } from '../model3d-viewer/model3d-viewer.component';

@Component({
  selector: 'app-media-uploader',
  standalone: true,
  imports: [
    NgIf,
    NgFor,
    NgStyle,
    NgClass,
    CommonModule,
    FileUploadModule,
    Model3dViewerComponent
],
  templateUrl: './media-uploader.component.html',
  styleUrl: './media-uploader.component.css'
})

export class MediaUploaderComponent {
    private accountService: AccountService = inject(AccountService);
    private cloudMediaService: MediaService = inject(MediaService);
    private toastr: ToastrService = inject(ToastrService);
    // I need to expose my enum to the html template.
    MediaType = MediaType;
    
    parentCraft: InputSignal<Craft> = input.required<Craft>();    
    parentCraftChanged: OutputEmitterRef<Craft> = output<Craft>();

    baseUrl = environment.apiUrl;

    // This is used for <input type="file"> tags to determine which file types are supported
    // There is no standard mime type for model3d files so just use the file extension instead.
    acceptTypes = 'image/jpg, image/jpeg, image/png, image/webp, image/gif, video/mp4, video/webm, video/ogg, .glb';
    // This is used for the filter function for the FileUploader
    acceptExtensions = ['jpg', 'jpeg', 'png', 'webp', 'gif', 'mp4', 'webm', 'ogg', 'glb'];
    uploader = new FileUploader({
            url: `${this.baseUrl}medias`,
            authToken: 'Bearer ' + this.accountService.currentUser()?.token,
            isHTML5: true,
            removeAfterUpload: true,
            autoUpload: false,
            maxFileSize: 100 * 1024 * 1024 // Cloudinary has an upper limit of 10MB
        });
    hasBaseDropZoneOver = false;
    hasAnotherDropZoneOver = false;
    response = '';

    // I am making this an input signal so parent components can check if all media has been
    // uploaded before allowing the user to continue.
    tempMedias: CloudMedia[] = [];
    deletedIds: number[] = [];

    constructor() {
        // Setup video uploads
        this.uploader.options.filters?.push({
            name: 'mediaExtensionFilter',
            fn: (item: FileLikeObject, options: FileUploaderOptions) => {
                const extension = item.name?.split('.').pop()?.toLowerCase();
                return extension ? this.acceptExtensions.includes(extension) : false;
            }
        });

        this.uploader.response.subscribe(res => this.response = res);
        
        // Disable cookie-based authentication because I use JWT tokens instead
        this.uploader.onAfterAddingFile = this.onAfterAddingTempMedia.bind(this);
        this.uploader.onSuccessItem = this.onUploadSuccess.bind(this);
        this.uploader.onCancelItem = this.onUploadCancel.bind(this);
    }

    // File Uploader methods
    fileOverBase(event:any): void {
        this.hasBaseDropZoneOver = event;
    }    

    deleteTempMedia(file: FileItem): void {
        const index = this.tempMedias.findIndex(media => media.cloudId === file._file.name);
        if (index > -1) {
            this.tempMedias.splice(index, 1);
        }
        file.remove();
    }

    deleteAllTempMedias(): void {
        this.tempMedias = [];
        this.uploader.clearQueue();
    }

    deleteUploadedImage(cloudId: string): void {
        const index = this.parentCraft().medias.findIndex(media => media.cloudId === cloudId);
        if (index > -1) {
            this.cloudMediaService.deleteCloudImage(cloudId).subscribe({
                next: () => this.deleteMediaFromCraft(index),
                error: error => {
                    this.toastr.error(error.message);
                }
            });
        }
    }
    
    deleteUploadedVideo(cloudId: string): void {
        const index = this.parentCraft().medias.findIndex(media => media.cloudId === cloudId);
        if (index > -1) {
            this.cloudMediaService.deleteCloudVideo(cloudId).subscribe({
                next: () => this.deleteMediaFromCraft(index),  
                error: error => this.toastr.error(error.message)
            });            
        }
    }

    deleteUploadedModel3d(cloudId: string): void {
        const index = this.parentCraft().medias.findIndex(media => media.cloudId === cloudId);
        if (index > -1) {
            this.cloudMediaService.deleteCloudModel3d(cloudId).subscribe({
                next: () => this.deleteMediaFromCraft(index),  
                error: error => this.toastr.error(error.message)
            });            
        }
    }

    setSearchImage(cloudId: string) {
        for (const media of this.parentCraft().medias) {
            if (media.cloudId === cloudId) {
                this.parentCraft().searchImageId = media.id;
                this.parentCraft().searchImage = media;
                this.parentCraftChanged.emit(this.parentCraft());
                break;
            }
        }
    }

    onAfterAddingTempMedia(tempMedia: FileItem) {
        // I use header authentication rather than cookie-based authentication, so I need to set this to false
        tempMedia.withCredentials = false;

        // I assign negative numbers to ids for temp media items so they don't conflict with real media items that have positive ids. 
        // I also use the file name as the cloudId for temp media items so I can find and delete them if the upload is cancelled.
        if (tempMedia._file.name.endsWith('.mp4') || tempMedia._file.name.endsWith('.webm') || tempMedia._file.name.endsWith('.ogg')) {
            this.tempMedias.push({
                id: -this.tempMedias.length - 1,
                url: URL.createObjectURL(tempMedia._file),
                cloudId: tempMedia._file.name,
                type: MediaType.Video
            });
        } else if (tempMedia._file.name.endsWith('.glb'))
        {
            this.tempMedias.push({
                id: -this.tempMedias.length - 1,
                url: URL.createObjectURL(tempMedia._file),
                cloudId: tempMedia._file.name,
                type: MediaType.Model3d
            });
        } else
        {
            this.tempMedias.push({
                id: this.tempMedias.length + 1,
                url: URL.createObjectURL(tempMedia._file),
                cloudId: tempMedia._file.name,
                type: MediaType.Image
            });
        }
    }

    onUploadSuccess(file: FileItem, response: string, status: number, headers: ParsedResponseHeaders) {
        const index = this.tempMedias.findIndex(media => media.cloudId === file._file.name);
        if (index > -1) {
            this.tempMedias.splice(index, 1);
        }
        const newCloudMedia = JSON.parse(response);
        const newCraftMedia: CraftMedia = {
            id: this.deletedIds.length > 0 ? this.deletedIds.pop()! : this.parentCraft().medias.length + 1,
            url: newCloudMedia.url,
            cloudId: newCloudMedia.cloudId,
            type: newCloudMedia.type,
            craftId: this.parentCraft().id,
            craftName: this.parentCraft().name
        };
        
        this.parentCraft().medias.push(newCraftMedia);
        if (this.parentCraft().searchImageId === null && newCraftMedia.type === MediaType.Image) {
            this.parentCraft().searchImageId = newCraftMedia.id;
            this.parentCraft().searchImage = newCraftMedia;
        }
        this.parentCraftChanged.emit(this.parentCraft());
    }
    
    onUploadCancel(file: FileItem, response: string, status: number, headers: ParsedResponseHeaders) {        
        const index = this.tempMedias.findIndex(media => media.cloudId === file._file.name);
        if (index > -1) {
            this.tempMedias.splice(index, 1);
        }
    }

    private deleteMediaFromCraft(index: number) {
        const oldId = this.parentCraft().medias[index].id;
        this.parentCraft().medias.splice(index, 1);   

        if (this.parentCraft().searchImageId === oldId)
        {            
            const firstImage = this.parentCraft().medias.find(media => media.type === MediaType.Image);
            this.parentCraft().searchImageId = firstImage ? firstImage.id : null;
            this.parentCraft().searchImage = firstImage ? firstImage : null;
        }
        this.deletedIds.push(oldId);
        this.parentCraftChanged.emit(this.parentCraft());   
    }
}
