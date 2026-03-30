import { ChangeDetectionStrategy, Component, computed, inject } from '@angular/core';
import { RouterLink, RouterLinkActive, RouterOutlet } from '@angular/router';
import { AuthStateService } from './core/auth/auth-state.service';
import { apiConfig } from './core/config/api.config';

@Component({
  selector: 'app-root',
  standalone: true,
  imports: [RouterLink, RouterLinkActive, RouterOutlet],
  templateUrl: './app.html',
  styleUrl: './app.css',
  changeDetection: ChangeDetectionStrategy.OnPush
})
export class AppComponent {
  private readonly authState = inject(AuthStateService);

  protected readonly isAuthenticated = this.authState.isAuthenticated;
  protected readonly apiBaseUrl = computed(() => apiConfig.baseUrl);
}
