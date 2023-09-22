import { Component } from '@angular/core';
import { PassengerService } from './../api/services/passenger.service';
import { FormBuilder, Validators } from '@angular/forms';
import { AuthService } from '../auth/auth.service';
import { Router } from '@angular/router';
//import { error } from 'console';

@Component({
  selector: 'app-register-passenger',
  templateUrl: './register-passenger.component.html',
  styleUrls: ['./register-passenger.component.css']
})
export class RegisterPassengerComponent {

  constructor(private passengerService: PassengerService,
    private fb: FormBuilder, private authService: AuthService,
    private router: Router) { }

  form = this.fb.group({
    email: ['', Validators.compose([Validators.required, Validators.minLength(1), Validators.maxLength(100)])],
    firstName: ['', Validators.compose([Validators.required, Validators.minLength(1), Validators.maxLength(35)])],
    lastName: ['', Validators.compose([Validators.required, Validators.minLength(1), Validators.maxLength(35)])],
    isFemale: [true, Validators.required]
  })

  checkPassenger(): void {
    const email = this.form.get('email')?.value;  // Extract email from the form

    // Check if email is available
    if (email) {
      const params = { email: email };  // Correct way to create params object
      this.passengerService.findPassenger(params)
        .subscribe(() => this.login(email),
          error => { if (error.status != 404) console.error(error) });
      //error => {console.error(error) });

    }
  }

  register() {
    if (this.form.invalid)
      return;

    console.log("Form Values:", this.form.value);
    const email = this.form.get('email')?.value;  // Extract email from the form

  // Check if email is available
    if (email) {
      this.passengerService.registerPassenger({ body: this.form.value })
        .subscribe(() => this.login(email),
        error => console.error(error));  // Correct way to pass email
    }
  }

  private login(email: string) {
    this.authService.loginUser({ email: email });

    this.router.navigate(['/search-flights']);
  }

}
