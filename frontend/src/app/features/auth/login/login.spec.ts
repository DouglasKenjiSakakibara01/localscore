import { provideHttpClient } from '@angular/common/http';
import { TestBed } from '@angular/core/testing';
import { provideRouter } from '@angular/router';
import { Login } from './login';

describe('Login', () => {
  beforeEach(async () => {
    await TestBed.configureTestingModule({
      imports: [Login],
      providers: [provideHttpClient(), provideRouter([])],
    }).compileComponents();
  });

  it('requires a valid email and an eight-character password', () => {
    const fixture = TestBed.createComponent(Login);
    const form = fixture.componentInstance.form;

    form.setValue({ email: 'invalid', password: 'short' });
    expect(form.invalid).toBeTrue();

    form.setValue({ email: 'douglas@example.com', password: 'password1' });
    expect(form.valid).toBeTrue();
  });
});
