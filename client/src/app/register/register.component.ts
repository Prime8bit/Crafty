import { Component, inject, OnInit, output } from '@angular/core';
import { AbstractControl, FormBuilder, FormGroup, ReactiveFormsModule, ValidatorFn, Validators } from '@angular/forms';
import { AccountService } from '../services/account.service';
import { NgIf } from '@angular/common';
import { Router } from '@angular/router';
import { HttpErrorResponse } from '@angular/common/http';

@Component({
  selector: 'app-register',
  standalone: true,
  imports: [ReactiveFormsModule, NgIf],
  templateUrl: './register.component.html',
  styleUrl: './register.component.css'
})
export class RegisterComponent implements OnInit {
    private accountService = inject(AccountService);
    private formBuilder = inject(FormBuilder);
    private router = inject(Router);

    cancelRegister = output<boolean>();
    registerForm: FormGroup = new FormGroup({});
    validationErrors?: string[];
    
    ngOnInit(): void {
        this.initializeForm();
    }

    initializeForm(): void {
        this.registerForm = this.formBuilder.group({ 
            userName: ['', Validators.required],
            firstName: ['', Validators.required],
            lastName: ['', Validators.required],
            email: ['', [Validators.required, Validators.email]],
            password: ['', [Validators.required, Validators.minLength(4), Validators.maxLength(20)]],
            confirmPassword: ['', [Validators.required, this.matchPasswords('password')]],
        });

        // subscribe to changes from the first password field and update the validity of the confirm password field
        this.registerForm.controls['password'].valueChanges.subscribe({
            next: () => this.registerForm.controls['confirmPassword'].updateValueAndValidity()
        })
    }

    matchPasswords(otherPassword: string): ValidatorFn {
        return (control: AbstractControl) => {
            return control.value === control.parent?.get(otherPassword)?.value ? null : { isMatching: true }
        }
    }

    register(): void {
        this.accountService.register(this.registerForm.value).subscribe({
            next: _ => this.router.navigateByUrl('/user/update'),
            error: (error: HttpErrorResponse) => this.validationErrors = error.error
        });
    }

    cancel(): void {
        this.cancelRegister.emit(false);
    }
}
