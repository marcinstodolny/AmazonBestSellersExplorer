import { ChangeDetectionStrategy, Component } from '@angular/core';

@Component({
  selector: 'app-bestsellers-page',
  standalone: true,
  template: `
    <section class="feature-placeholder">
      <span class="feature-badge">Public</span>
      <h1>Bestsellers</h1>
      <p>Placeholder pod listę bestsellerów Amazon Software z backendu .NET.</p>
    </section>
  `,
  styles: `
    .feature-placeholder {
      display: grid;
      gap: 0.75rem;
    }

    .feature-badge {
      width: fit-content;
      padding: 0.35rem 0.7rem;
      border-radius: 999px;
      background: rgba(188, 108, 37, 0.12);
      color: #8f4a11;
      font-size: 0.85rem;
      font-weight: 600;
    }

    h1 {
      margin: 0;
      font-size: clamp(2rem, 3vw, 2.75rem);
      line-height: 1;
    }

    p {
      margin: 0;
      max-width: 42rem;
      color: #4c5b67;
      font-size: 1rem;
      line-height: 1.6;
    }
  `,
  changeDetection: ChangeDetectionStrategy.OnPush
})
export class BestsellersPageComponent {}
