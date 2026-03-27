import { AfterViewInit, Component, ElementRef, inject, input, NgZone, OnDestroy, ViewChild } from '@angular/core';
import * as THREE from 'three';
import { GLTFLoader } from 'three/examples/jsm/loaders/GLTFLoader.js';
import { OrbitControls } from 'three/examples/jsm/controls/OrbitControls.js';
import { ToastrService } from 'ngx-toastr';

@Component({
  selector: 'app-model3d-viewer',
  standalone: true,
  imports: [],
  templateUrl: './model3d-viewer.component.html',
  styleUrl: './model3d-viewer.component.css'
})
export class Model3dViewerComponent implements AfterViewInit, OnDestroy{
    @ViewChild('canvas') canvas?: ElementRef<HTMLCanvasElement>;
    // TODO (Nate): Change this to required and remove the default after debugging.
    modelUrl = input<string>("https://res.cloudinary.com/db3ulheek/image/upload/v1773881023/RobotArm_adiync.glb")

    private toastr = inject(ToastrService);
    private scene = new THREE.Scene();
    private camera?: THREE.PerspectiveCamera;
    private renderer?: THREE.WebGLRenderer;
    private controls?: OrbitControls;
    private frameId = 0;
    private resizeObserver?: ResizeObserver;

    // I need to grab the NgZone, so I can run the three.js animate loop outside the angular refresh loop
    // The angular refresh loop updates once EVERY 60ms which is a waste
    constructor(private ngZone: NgZone) {}

    ngAfterViewInit(): void {
        this.initThreeJs();
        this.startResizeObservation();

        this.ngZone.runOutsideAngular(() => this.animate());
    }
    
    ngOnDestroy(): void {
        this.resizeObserver?.disconnect();

        if (this.frameId) {
            cancelAnimationFrame(this.frameId);
        }
        this.renderer?.dispose();
        this.scene.clear();
    }

    initThreeJs(): void {
        const canvasElement = this.canvas?.nativeElement;
        if (!canvasElement)
        {
            this.toastr.error("No canvas found");
            return;
        }

        // Temporary, will be replaced by ResizeObserver
        const width = canvasElement.clientWidth || 300;
        const height = canvasElement.clientHeight || 300;

        this.scene.background = new THREE.Color(0x888888)

        this.camera = new THREE.PerspectiveCamera(75, width / height, 0.1, 1000);

        this.renderer = new THREE.WebGLRenderer({canvas: canvasElement, antialias: true });
        // This is needed to prevent shading from looking like flat white where the light hits it.
        this.renderer.toneMapping = THREE.ACESFilmicToneMapping;
        this.renderer.setSize(width, height);

        this.controls = new OrbitControls(this.camera, this.renderer.domElement);
        this.controls.enableDamping = true;

        const loader = new GLTFLoader();
        loader.load(this.modelUrl(), (gltf) => {
            this.scene.add(gltf.scene);
            const box = new THREE.Box3().setFromObject(gltf.scene);
            const size = box.getSize(new THREE.Vector3());
            const cameraDistance = Math.max(size.x, size.y, size.z) / 2 + 2;
            const center = box.getCenter(new THREE.Vector3());
            this.camera?.position?.set(0, cameraDistance, cameraDistance);
            this.camera?.lookAt(center);
        });

        const ambientLight = new THREE.AmbientLight(0xffffff, Math.PI);
        this.scene.add(ambientLight);

        // Unless specified otherwise, directional lights always point at 0,0,0
        const directionalLight = new THREE.DirectionalLight(0xffffff, Math.PI * 2);
        directionalLight.position.set(0,1,1);
        this.scene.add(directionalLight);
    }

    // To provide animate as a function object to requestAnimationFrame, I can't just declare this as a function
    // I need to make it an arrow function object.
    private animate = (): void => {
        if (!this.renderer || !this.controls || !this.camera) {
            return;
        }

        this.frameId = requestAnimationFrame(this.animate);
        
        this.controls.update();
        this.renderer.render(this.scene, this.camera);
    }

    private startResizeObservation(): void {
        const parent = this.canvas?.nativeElement?.parentElement;
        if (!parent)
        {
            this.toastr.error("Unable to find canvas parent.");
            return;
        }

        this.resizeObserver = new ResizeObserver((entries) => {
            for (const entry of entries) {
                this.onResize(entry.contentRect.width, entry.contentRect.height);
            }
        });

        this.resizeObserver.observe(parent);
    }

    private onResize(width: number, height: number): void {
        if (!this.renderer || !this.camera) {
            return;
        }

        this.camera.aspect = width / height;
        this.camera.updateProjectionMatrix();

        this.renderer.setSize(width, height);
    }
}
