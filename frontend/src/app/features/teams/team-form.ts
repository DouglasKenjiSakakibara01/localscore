import { Component, inject, OnInit, signal } from '@angular/core';
import { FormControl, FormGroup, ReactiveFormsModule, Validators } from '@angular/forms';
import { ActivatedRoute, Router, RouterLink } from '@angular/router';
import { apiErrorMessage } from '../../core/api/api-error';
import { ChampionshipService } from '../championships/championship.service';

@Component({
  selector: 'app-team-form',
  imports: [ReactiveFormsModule, RouterLink],
  templateUrl: './team-form.html',
})
export class TeamForm implements OnInit {
  private readonly service = inject(ChampionshipService);
  private readonly route = inject(ActivatedRoute);
  private readonly router = inject(Router);

  readonly id = this.route.snapshot.paramMap.get('id');
  readonly returnUrl =
    this.route.snapshot.queryParamMap.get('returnUrl') || '/app/teams';
  readonly loading = signal(this.id !== null);
  readonly saving = signal(false);
  readonly error = signal<string | null>(null);

  readonly form = new FormGroup({
    name: new FormControl('', {
      nonNullable: true,
      validators: [Validators.required, Validators.minLength(2), Validators.maxLength(100)],
    }),
    shortName: new FormControl('', {
      nonNullable: true,
      validators: [Validators.maxLength(30)],
    }),
    acronym: new FormControl('', {
      nonNullable: true,
      validators: [Validators.pattern(/^[A-Za-z0-9]{0,10}$/)],
    }),
    city: new FormControl('', {
      nonNullable: true,
      validators: [Validators.maxLength(100)],
    }),
    primaryColor: new FormControl('', { nonNullable: true }),
    secondaryColor: new FormControl('', { nonNullable: true }),
  });

  ngOnInit(): void {
    if (!this.id) {
      return;
    }

    this.service.getTeam(this.id).subscribe({
      next: (team) => {
        this.form.setValue({
          name: team.name,
          shortName: team.shortName ?? '',
          acronym: team.acronym ?? '',
          city: team.city ?? '',
          primaryColor: team.primaryColor ?? '',
          secondaryColor: team.secondaryColor ?? '',
        });
        this.loading.set(false);
      },
      error: (error) => {
        this.error.set(apiErrorMessage(error));
        this.loading.set(false);
      },
    });
  }

  addColor(field: 'primaryColor' | 'secondaryColor'): void {
    const color = field === 'primaryColor' ? '#18794E' : '#14261E';
    this.form.controls[field].setValue(color);
  }

  clearColor(field: 'primaryColor' | 'secondaryColor'): void {
    this.form.controls[field].setValue('');

    if (field === 'primaryColor') {
      this.form.controls.secondaryColor.setValue('');
    }
  }

  save(): void {
    if (this.form.invalid || this.saving()) {
      this.form.markAllAsTouched();
      return;
    }

    const value = this.form.getRawValue();

    if (value.secondaryColor && !value.primaryColor) {
      this.error.set('Informe a cor principal antes da secundária.');
      return;
    }

    const optional = (input: string) => input.trim() || null;
    const input = {
      name: value.name.trim(),
      shortName: optional(value.shortName),
      acronym: optional(value.acronym),
      city: optional(value.city),
      primaryColor: optional(value.primaryColor),
      secondaryColor: optional(value.secondaryColor),
    };

    this.saving.set(true);
    this.error.set(null);

    const request = this.id
      ? this.service.updateTeam(this.id, input)
      : this.service.createTeam(input);

    request.subscribe({
      next: () => void this.router.navigateByUrl(this.returnUrl),
      error: (error) => {
        this.error.set(apiErrorMessage(error));
        this.saving.set(false);
      },
    });
  }
}
