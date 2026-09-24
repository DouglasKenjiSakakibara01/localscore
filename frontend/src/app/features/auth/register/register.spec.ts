import { provideHttpClient } from '@angular/common/http';
import { TestBed } from '@angular/core/testing';
import { provideRouter } from '@angular/router';
import { Register } from './register';

describe('Register', () => {
  beforeEach(async () => {
    await TestBed.configureTestingModule({
      imports: [Register],
      providers: [provideHttpClient(), provideRouter([])],
    }).compileComponents();
  });

  it('requires matching passwords that follow the policy', () => {
    const fixture = TestBed.createComponent(Register);
    const component = fixture.componentInstance;

    component.form.setValue({
      name: 'Douglas Silva',
      email: 'douglas@example.com',
      password: 'password1',
      passwordConfirmation: 'different1',
    });

    expect(component.form.hasError('passwordsMismatch')).toBeTrue();

    component.form.controls.passwordConfirmation.setValue('password1');

    expect(component.form.valid).toBeTrue();
  });

  it('accepts names with accents, spaces, hyphens, and apostrophes', () => {
    const fixture = TestBed.createComponent(Register);
    const name = fixture.componentInstance.form.controls.name;

    for (const value of ["João da Silva", "Ana-Maria", "D'Ávila"]) {
      name.setValue(value);
      expect(name.valid).withContext(value).toBeTrue();
    }
  });
});
