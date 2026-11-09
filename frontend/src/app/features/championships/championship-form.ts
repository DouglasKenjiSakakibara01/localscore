import { Component, inject, OnInit, signal } from '@angular/core';
import { FormControl, FormGroup, ReactiveFormsModule, Validators } from '@angular/forms';
import { ActivatedRoute, Router, RouterLink } from '@angular/router';
import { apiErrorMessage } from '../../core/api/api-error';
import { ChampionshipService } from './championship.service';

@Component({
  selector: 'app-championship-form',
  imports: [ReactiveFormsModule, RouterLink],
  templateUrl: './championship-form.html',
})
export class ChampionshipForm implements OnInit {
  private readonly service = inject(ChampionshipService);
  private readonly route = inject(ActivatedRoute);
  private readonly router = inject(Router);

  readonly id = this.route.snapshot.paramMap.get('id');
  readonly loading = signal(this.id !== null);
  readonly saving = signal(false);
  readonly error = signal<string | null>(null);
  readonly canChangeSlug = signal(true);

  readonly form = new FormGroup({
    name: new FormControl('', {
      nonNullable: true,
      validators: [Validators.required, Validators.minLength(3), Validators.maxLength(120)],
    }),
    slug: new FormControl('', {
      nonNullable: true,
      validators: [Validators.maxLength(100)],
    }),
    description: new FormControl('', {
      nonNullable: true,
      validators: [Validators.maxLength(1000)],
    }),
    startDate: new FormControl('', { nonNullable: true }),
    endDate: new FormControl('', { nonNullable: true }),
  });

  ngOnInit(): void {
    if (!this.id) {
      return;
    }

    this.service.getChampionship(this.id).subscribe({
      next: (item) => {
        if (item.status.code === 3) {
          void this.router.navigate(['/app/championships', item.id]);
          return;
        }

        this.canChangeSlug.set(item.canChangeSlug);
        this.form.setValue({
          name: item.name,
          slug: item.slug,
          description: item.description ?? '',
          startDate: item.startDate ?? '',
          endDate: item.endDate ?? '',
        });

        if (!item.canChangeSlug) {
          this.form.controls.slug.disable();
        }

        this.loading.set(false);
      },
      error: (error) => {
        this.error.set(apiErrorMessage(error));
        this.loading.set(false);
      },
    });
  }

  save(): void {
    if (this.form.invalid || this.saving()) {
      this.form.markAllAsTouched();
      return;
    }

    const raw = this.form.getRawValue();

    if (raw.startDate && raw.endDate && raw.endDate < raw.startDate) {
      this.error.set('A data final não pode ser anterior à data inicial.');
      return;
    }

    const input = {
      name: raw.name.trim(),
      slug: raw.slug.trim() || null,
      description: raw.description.trim() || null,
      startDate: raw.startDate || null,
      endDate: raw.endDate || null,
    };

    this.saving.set(true);
    this.error.set(null);

    const request = this.id
      ? this.service.updateChampionship(this.id, { ...input, slug: raw.slug })
      : this.service.createChampionship(input);

    request.subscribe({
      next: (item) => void this.router.navigate(['/app/championships', item.id]),
      error: (error) => {
        this.error.set(apiErrorMessage(error));
        this.saving.set(false);
      },
    });
  }
}
