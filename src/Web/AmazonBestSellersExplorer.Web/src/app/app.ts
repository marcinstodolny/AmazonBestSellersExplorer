import { ChangeDetectionStrategy, Component, inject } from '@angular/core';
import { Router, RouterLink, RouterLinkActive, RouterOutlet } from '@angular/router';
import { AuthStateService } from './core/auth/auth-state.service';

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
  private readonly router = inject(Router);

  protected readonly isAuthenticated = this.authState.isAuthenticated;
  protected readonly username = this.authState.username;

  protected logout(): void {
    this.authState.clearToken();
    void this.router.navigateByUrl('/bestsellers');
  }
}
