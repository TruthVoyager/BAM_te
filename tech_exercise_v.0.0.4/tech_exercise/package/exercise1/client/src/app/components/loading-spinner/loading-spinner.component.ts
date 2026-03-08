import { Component, input } from '@angular/core';

@Component({
  selector: 'app-loading-spinner',
  standalone: true,
  templateUrl: './loading-spinner.component.html',
  styleUrl: './loading-spinner.component.css',
})
export class LoadingSpinnerComponent {
  /** Optional message shown below the spinner (e.g. "Loading…", "Loading ranks…"). */
  message = input<string>('');
}
